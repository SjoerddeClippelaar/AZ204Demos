


echo "Please enter a resource name prefix."
read myPrefix

myLocation="westeurope"
myResourceGroup="demos-az204-module03-rg"
storageName="${myPrefix}az204mod3stor"

#create group
az group create -n $myResourceGroup -l $myLocation

#create a storage account
storage=$(az storage account create -g $myResourceGroup -n $storageName)
storageConnString=$(az storage account show-connection-string -n $storageName -o tsv)