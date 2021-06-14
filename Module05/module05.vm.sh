


myLocation="westeurope"
myResourceGroup="demos-az204-module05-rg"

az group create -n $myResourceGroup -l $myLocation

# create a vm with debian os
az vm create -g $myResourceGroup -n quickvm --image Debian --admin-username practice --admin-password PracticePa55w.rd

# acquire its ip address
ipAddress=$(az vm list-ip-addresses --resource-group $myResourceGroup --name quickvm --query '[].{ip:virtualMachine.network.publicIpAddresses[0].ipAddress}' --output tsv)

#================================
# No associated demo (yet)
#
#================================