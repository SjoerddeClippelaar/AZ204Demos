
echo "Please enter a resource prefix (to ensure unique FQDNs)"
read prefix

# this demo is based on the exercise from DP-200 course Module 6
# https://github.com/MicrosoftLearning/DP-200-Implementing-an-Azure-Data-Solution/blob/master/instructions/dp-200-06_instructions.md


myLocation=westeurope
myResourceGroup="demos-az204-module10-rg"
storageName="${prefix}az204mod10capturestor"
containerName="capture"

ehnsName="${prefix}-module10-ehns"
ehName="phoneanalysis"
ehSapName="phoneanalysis-sap"


#create resource group
az group create -n $myResourceGroup -l $myLocation

# create a storage to dump capture in later
storage=$(az storage account create -g $myResourceGroup -n $storageName)
storageConnString=$(az storage account show-connection-string \
	-g $myResourceGroup \
	-n $storageName \
	--query "connectionString" -o tsv)

# create event hubs namespace
#sku is standard by default, so we can test capture functionality
ehns=$(az eventhubs namespace create -n $ehnsName -g $myResourceGroup)


# create event hub
eh=$(az eventhubs eventhub create \
	-g $myResourceGroup \
	--namespace-name $ehnsName \
	-n $ehName)

# create sap
sap=$(az eventhubs eventhub authorization-rule create \
	-g $myResourceGroup \
	--namespace-name $ehnsName \
	--eventhub-name $ehName \
	-n $ehSapName \
	--rights Send Listen)

# connection string
ehConnString=$(az eventhubs eventhub authorization-rule keys list \
	-g $myResourceGroup \
	-n $ehSapName \
	--eventhub-name $ehName \
	--namespace-name $ehnsName \
	-o tsv --query 'primaryConnectionString')
	
	
# set the connection string value in the telcodatagen.config.exe in the datagenerator subfolder
# then do a test: run telcodatagen.exe with the following parameters: 
# 	1000 0.2 2 (1000 messages per hour, 20% simulated to be fraudulent calls, run for 2 hours)


# you can now use the WPF app to look at the events coming in, 
# or you can follow the rest of the DP-200 module 6 exercise to use a stream analytics job