
read prefix

# create resource group
myLocation="westeurope"
myResourceGroup="demos-az204-module11-svcbus-rg"
queueName="sbqueue"
topicName="sbtopic"

az group create -n $myResourceGroup -l $myLocation


# create service bus namespace
namespaceName="${prefix}az204svcbus"
az servicebus namespace create \
--resource-group $myResourceGroup \
--name $namespaceName \
--location $myLocation

# create service bus queue
az servicebus queue create --resource-group $myResourceGroup \
--namespace-name $namespaceName \
--name az204-queue

# get servicebus namespace connection string
connectionString=$(az servicebus namespace authorization-rule keys list \
--resource-group $myResourceGroup \
--namespace-name $namespaceName \
--name RootManageSharedAccessKey \
--query primaryConnectionString --output tsv)
echo $connectionString





# create servicebus topic
az servicebus topic create --resource-group $myResourceGroup --namespace-name $namespaceName --name az-204sbtopic

