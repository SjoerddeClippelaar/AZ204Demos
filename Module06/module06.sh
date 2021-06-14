
echo "Please enter a resource name prefix."
read myPrefix

myLocation="westeurope"
myResourceGroup="demos-az204-module06-rg"
storageName="${myPrefix}az204mod6safestor"

#create group
az group create -n $myResourceGroup -l $myLocation

#create a storage account
storage=$(az storage account create -g $myResourceGroup -n $storageName)


# additional steps:
# create an app registration (single-tenant is fine)
# assign a role in the storage account for the app registration with enough rights that it can create containers

# update the C# console app with the following:
# - tenant id
# - app id
# - storage account name

# then give the console app a way to authenticate itself to AD:
#   - create a client secret in the app registration 
#     add its value as a user secret to the console project
# OR
#   - Create a client certificate in the app registration
#$    a client certificate
