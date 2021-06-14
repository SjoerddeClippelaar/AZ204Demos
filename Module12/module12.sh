read myPrefix

myLocation="westeurope"
myResourceGroup="demos-az204-module12-rg"

appInsightsName="${myPrefix}-module12-ai"

appServicePlanName="module12-appsp1"
webappName1="${myPrefix}-module12-webapi1"
webappName2="${myPrefix}-module12-webapp1"

# create resource group
az group create -n $myResourceGroup -l $myLocation

# create app insights
# app insights extension for cli isn't necessarily installed yet, may get a prompt here
appInsights=$(az monitor app-insights component create -a $appInsightsName -l $myLocation -g $myResourceGroup)
ai_key=$(echo $appInsights | jq -r '.instrumentationKey')
ai_connstring=$(echo $appInsights | jq -r '.connectionString')

# create web app
az appservice plan create -n $appServicePlanName -g $myResourceGroup --sku S1
az webapp create -n $webappName1 -p $appServicePlanName -g $myResourceGroup
az webapp create -n $webappName2 -p $appServicePlanName -g $myResourceGroup

# set up app insights for webapp
az webapp config appsettings set \
	-g $myResourceGroup -n $webappName1 \
	--settings \
		APPINSIGHTS_INSTRUMENTATIONKEY=$ai_key \
		APPLICATIONINSIGHTS_CONNECTION_STRING=$ai_connstring \
		ApplicationInsightsAgent_EXTENSION_VERSION="~2"

az webapp config appsettings set \
	-g $myResourceGroup -n $webappName2 \
	--settings \
		APPINSIGHTS_INSTRUMENTATIONKEY=$ai_key \
		APPLICATIONINSIGHTS_CONNECTION_STRING=$ai_connstring \
		ApplicationInsightsAgent_EXTENSION_VERSION="~2"
		
#===============================================================================================
# at this point, update the web api with the instrumentationkey to test it locally
# or publish it to the web app and let it use the application settings instrumentationkey
#
# Also go to diagnostic settings in the web apps and set up a permanent storage solution for the logs
# so you can view the logs in log analytics
#===============================================================================================


