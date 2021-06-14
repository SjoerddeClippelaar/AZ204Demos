echo "Please enter a resource name prefix."
read myPrefix

myLocation="westeurope"
myResourceGroup="demos-serverlessarchitecture-rg"

storageName="${myPrefix}tollboothstorage"
storageImagesContainerName="images"
storageExportContainerName="export"

appInsightsName="TollboothMonitor"

netFunctionAppName="${myPrefix}-tollboothfa"
nodeFunctionAppName="${myPrefix}-tollboothevents"

eventTopicName="${myPrefix}-TollboothEventGrid"



# create resource group
az group create -n $myResourceGroup -l $myLocation

# provision storage account, store the connection string for later and crreate two containers

storage=$(az storage account create -g $myResourceGroup -n $storageName)
storageConnString=$(az storage account show-connection-string -n $storageName -o tsv)
az storage container create --connection-string $storageConnString -n $storageImagesContainerName
az storage container create --connection-string $storageConnString -n $storageExportContainerName


#provision an app insights object
# create app insights
# app insights extension isn't necessarily installed yet, may get a prompt here
appInsights=$(az monitor app-insights component create -a $appInsightsName -l $myLocation -g $myResourceGroup)
ai_key=$(echo $appInsights | jq -r '.instrumentationKey')
ai_connstring=$(echo $appInsights | jq -r '.connectionString')



# provision a function app for .net code
function1=$(az functionapp create \
	-g $myResourceGroup -n $netFunctionAppName \
	--storage-account $storageName \
	--consumption-plan-location $myLocation \
	--functions-version 3 \
	--runtime dotnet \
	--app-insights $appInsightsName)
	
# provision a function app for events using node.js
function2=$(az functionapp create \
	-g $myResourceGroup -n $nodeFunctionAppName \
	--storage-account $storageName \
	--consumption-plan-location $myLocation \
	--functions-version 3 \
	--runtime node --runtime-version 12 \
	--app-insights $appInsightsName)
	
	
#provision an event grid, store endpoint and access key for later
eventTopic=$(az eventgrid topic create \
	-l $myLocation \
	-g $myResourceGroup \
	-n $eventTopicName)
eventTopicEndpoint=$(echo $eventTopic | jq -r '.endpoint')


#==========================================================================
# Important: At this point, first run the powershell script to
# deploy the cosmosdb ARM template.
# don't forget to make sure the $cosmosdbName variable matches the db name you use in the ps script.
#
# then in the portal also set the flag "Allow access from azure services"
#==========================================================================

cosmosdbName="${myPrefix}-tollboothdb"
databaseName="LicensePlates"

processedContainerName="Processed"
processedPartitionKey="/licensePlateText"
manualContainerName="NeedsManualReview"
manualPartitionKey="/fileName"

tollboothVisionName="${myPrefix}-tollboothvision"

keyvaultName="${myPrefix}toolboothkv-1" #cycle the postfix if needed


# create db and allow azure services to access it
az cosmosdb sql database create -g $myResourceGroup -a $cosmosdbName -n $databaseName
az cosmosdb update --network-acl-bypass AzureServices -g $myResourceGroup -n $cosmosdbName

# create containers
az cosmosdb sql container create -g $myResourceGroup -a $cosmosdbName -d $databaseName -n $processedContainerName -p $processedPartitionKey
az cosmosdb sql container create -g $myResourceGroup -a $cosmosdbName -d $databaseName -n $manualContainerName -p $manualPartitionKey


#provision a computer vision account
computervision=$(az cognitiveservices account create \
	-g $myResourceGroup \
	-l $myLocation \
	--kind "ComputerVision" \
	-n $tollboothVisionName \
	--sku F0 --yes)



#provision keyvault
keyvault=$(az keyvault create -g $myResourceGroup -l $myLocation -n $keyvaultName)
#az keyvault secret set --vault-name $keyvaultName -n "maindbconnstring" --value "[some database connection string]"
#az keyvault secret set --vault-name $keyvaultName -n "AppConfig" --value $appconfigconnstring

#add secrets to keyvault with keys and connection strings and store the urls
computervisionKey1=$(az cognitiveservices account keys list -g $myResourceGroup -n $tollboothVisionName --query "key1" -o tsv)
computervisionSecretId=$(az keyvault secret set \
	--vault-name $keyvaultName \
	-n "computerVisionApiKey" --value $computervisionKey1 \
	-o tsv --query "id")
	
	
eventTopicKey1=$(az eventgrid topic key list -n $eventTopicName -g $myResourceGroup --query 'key1' -o tsv)
eventTopicSecretId=$(az keyvault secret set \
	--vault-name $keyvaultName \
	-n "eventGridTopicKey" --value $eventTopicKey1 \
	-o tsv --query "id")
	
cosmosdbPrimaryKey=$(az cosmosdb keys list -g $myResourceGroup -n $cosmosdbName --query "primaryMasterKey" -o tsv)
cosmosdbSecretId=$(az keyvault secret set \
	--vault-name $keyvaultName \
	-n "cosmosDBAuthorizationKey" --value $cosmosdbPrimaryKey \
	-o tsv --query "id")
	
storageConnString=$(az storage account show-connection-string -n $storageName -o tsv)
storageSecretId=$(az keyvault secret set \
	--vault-name $keyvaultName \
	-n "blobStorageConnection" --value $storageConnString \
	-o tsv --query "id")


# create a managed identity for the function app we made earlier and give it access to the key vault
appidentity=$(az webapp identity assign -n $netFunctionAppName -g $myResourceGroup --query "principalId" -o tsv)
az keyvault set-policy --object-id $appidentity --secret-permissions get list -n $keyvaultName




# add app settings to the .NET function app
cosmosdbEndpoint=$(az cosmosdb show -g $myResourceGroup -n $cosmosdbName --query "documentEndpoint" -o tsv)
computervisionEndpoint=$(az cognitiveservices account show -g $myResourceGroup -n $tollboothVisionName -o tsv --query "properties.endpoint")
eventTopicEndpoint=$(az eventgrid topic show -g $myResourceGroup -n $eventTopicName --query "endpoint" -o tsv)
az webapp config appsettings set -n $netFunctionAppName -g $myResourceGroup \
	--settings \
		computerVisionApiUrl="${computervisionEndpoint}vision/v2.0/ocr" \
		computerVisionApiKey="@Microsoft.KeyVault(SecretUri=${computervisionSecretId})" \
		eventGridTopicEndpoint=$eventTopicEndpoint \
		eventGridTopicKey="@Microsoft.KeyVault(SecretUri=${eventTopicSecretId})" \
		cosmosDBEndPointUrl=$cosmosdbEndpoint \
		cosmosDBAuthorizationKey="@Microsoft.KeyVault(SecretUri=${cosmosdbSecretId})" \
		cosmosDBDatabaseId=$databaseName \
		cosmosDBCollectionId=$processedContainerName \
		exportCsvContainerName=$storageExportContainerName \
		blobStorageConnection="@Microsoft.KeyVault(SecretUri=${storageSecretId})"



#==========================================================================
# Publish the Azure Functions project (in the folder Tollbooth)
#==========================================================================

eventSubscriptionName="${myPrefix}processimagesub"

# create event subscription for the storage account
storageId=$(az storage account show -g $myResourceGroup -n $storageName --query "id" -o tsv)





