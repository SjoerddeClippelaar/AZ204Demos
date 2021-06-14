read myPrefix

myLocation="westeurope"
myResourceGroup="demos-az204-module08-rg"

appServicePlanName="module08-appsp1"
apiName="${myPrefix}-module08-api1"

apimName="${myPrefix}-module08-apim1"

# create resource group
az group create -n $myResourceGroup -l $myLocation

#create service plan and webapp
az appservice plan create -n $appServicePlanName -g $myResourceGroup --sku F1
az webapp create -n $apiName -p $appServicePlanName -g $myResourceGroup

# create api management
az apim create \
	-g $myResourceGroup -n $apimName \
	--publisher-email "demo@contoso.com" \
	--publisher-name "Microsoft" \
	--sku-name Developer

# at this point you should first publish your api from visual studio to azure and register the openAPI definition
# not strictly required, but this helps apim automatically detect and register the api calls

# when you've added the api to the api manager, try the following:
# - test the api from the API blade
# - test the api from the browser, pay attention to the error you get if you don't include a key
# - create versions and revisions
# - publish the developer portal
#   - visit it and create a new user
#	- assign the user a subscription in portal
#   - find the individual key the user got for the subscription and test it
