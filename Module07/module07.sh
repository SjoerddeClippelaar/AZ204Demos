
# prepare name variables
echo "Please enter a resource name prefix."
read myPrefix

myLocation="westeurope"
myResourceGroup="demos-az204-module07-rg"

keyvaultName="${myPrefix}module07kv-2" #keyvaults only do a soft delete so you may need to change the suffix if you try to recreate one too soon

appconfigName="${myPrefix}module07appconfig"

appServicePlanName="module07-appsp1"
appName="${myPrefix}-module07-app1"

# create resource group
az group create -n $myResourceGroup -l $myLocation

# create application configuration
az appconfig create -l $myLocation -g $myResourceGroup -n $appconfigName --sku Free
#set features and key value pairs
az appconfig feature set --feature testFeature1 -n $appconfigName -y
az appconfig feature set --feature testFeature2 -n $appconfigName -y
az appconfig kv set -y -n $appconfigName --key "testAppBackgroundColor" --value "blue"
az appconfig kv set -y -n $appconfigName --key "testAppBoxSize" --value "64"
# store the app config conn string for later
appconfigconnstring=$(az appconfig credential list -n $appconfigName --query "[0].connectionString" -o tsv)


# create keyvault 
# note: if you get an MSI error you can try to run this from within the azure cloud shell instead
az keyvault create -g $myResourceGroup -l $myLocation -n $keyvaultName
az keyvault secret set --vault-name $keyvaultName -n "maindbconnstring" --value "[some database connection string]"
az keyvault secret set --vault-name $keyvaultName -n "AppConfig" --value $appconfigconnstring

#create service plan and webapp
az appservice plan create -n $appServicePlanName -g $myResourceGroup --sku F1
az webapp create -n $appName -p $appServicePlanName -g $myResourceGroup

# add keyvaultname as an app setting to the webapp
az webapp config appsettings set -n $appName -g $myResourceGroup --settings keyvaultName=$keyvaultName

# create a managed identity for the app and store the principal id
# then assign rights to it on the keyvault
appidentity=$(az webapp identity assign -n $appName -g $myResourceGroup --query "principalId" -o tsv)
az keyvault set-policy --object-id $appidentity --secret-permissions get list -n $keyvaultName

# find out what the uri is for the keyvault secret we made earlier (which contains the application configuration connection string)
# then store it in an app connection string as a keyvualt reference
secretvalue=$(az keyvault secret show -n "AppConfig" --vault-name $keyvaultName --query "id" -o tsv)
connstringvalue="@Microsoft.KeyVault(SecretUri=${secretvalue})"
az webapp config connection-string set -g $myResourceGroup -n $appName -t "Custom" --settings AppConfig=$connstringvalue


