echo "Please enter a resource name prefix."
read myPrefix

myLocation="westeurope"
myResourceGroup="demos-az204-module02-rg"

storageName="${myPrefix}module02storage"
azfunctionName="${myPrefix}-module02-azf1"
functionplanName="${myPrefix}-module02-functionplan"

blobinputs1="basicblobfunction-inputs"  # referred to in Azure Functions project

# create resource group
az group create -n $myResourceGroup -l $myLocation

storage=$(az storage account create -g $myResourceGroup -n $storageName)
storageConnString=$(az storage account show-connection-string -n $storageName -o tsv)
az storage container create --account-name $storageName -n $blobinputs1

# using a service plan instead of consumption for this demo, as consumption makes the app trigger very slowly
az appservice plan create -n $functionplanName -g $myResourceGroup --sku S1

az functionapp create \
	-g $myResourceGroup -n $azfunctionName \
	--storage-account $storageName \
	--plan $functionplanName \
	--functions-version 3 \
	--runtime dotnet
	
	
# at this stage publish functions app from visual studio
# be sure to untick "deploy from zip file" to be able to view the functions in azure portal

# things to try / look at:
# - Console under development tools to walk through published files
# - Individual Functions, code + test and integration