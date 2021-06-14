
read prefix


# resource names
myLocation1="westeurope"
myResourceGroup1="demos-az204-module01-rg1"
appServicePlanName1="module01-appsp1"
appName1="${prefix}-module01-app1"

myLocation2="northeurope"
myResourceGroup2="demos-az204-module01-rg2"
appServicePlanName2="module01-appsp2"
appName2="${prefix}-module01-app2"


az group create -n $myResourceGroup1 -l $myLocation1
az appservice plan create -n $appServicePlanName1 -g $myResourceGroup1 --sku S1
az webapp create -n $appName1 -p $appServicePlanName1 -g $myResourceGroup1
az webapp config appsettings set -n $appName1 -g $myResourceGroup1 --settings ServerName=$myLocation1

# kudu website : ewv-module01-app1.scm.azurewebsites.net


# next step: publish webapp from visual studio
# if you publish as debug mode, you can attach debugger to it from cloud explorer

# Er zijn ook een aantal manieren om vanuit de portal logs te bekijken bijv bij App Service Logs schrijven naar blob storage


# Met Application Insights kan je inhaken op de app en grafieken maken van exceptions e.d. (zie module 12)



#================
# deployment slots
testSlotName="test"
az webapp deployment slot create -n $appName1 -g $myResourceGroup1 -s $testSlotName


#================
# second web app

az group create -n $myResourceGroup2 -l $myLocation2
az appservice plan create -n $appServicePlanName2 -g $myResourceGroup2 --sku S1
az webapp create -n $appName2 -p $appServicePlanName2 -g $myResourceGroup2
az webapp config appsettings set -n $appName2 -g $myResourceGroup2 --settings ServerName=$myLocation2


# traffic manager profiles
# note: doesn't work with the included blazor web app
az network traffic-manager profile create \
	-g $myResourceGroup1 \
	-n WeightedProfile \
	--unique-dns-name "${prefix}-az204-module1-weighted" \
	--routing-method Weighted

az network traffic-manager profile create \
	-g $myResourceGroup1 \
	-n PriorityProfile \
	--unique-dns-name "${prefix}-az204-module1-priority" \
	--routing-method Priority
# set up the weights and priorities in the portal
#