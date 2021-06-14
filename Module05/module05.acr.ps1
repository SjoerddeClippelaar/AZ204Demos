
$prefix = Read-Host "Please enter your preferred resource prefix (for generating a unique name in Azure)"

$location = "westeurope"
$resourceGroupName = "demos-az204-module05-acr-rg"

$acrName = "$($prefix)az204module05acr"
$containerName = "container1"

# login
Login-AzAccount

# create resource group
$rg = New-AzResourceGroup -Location $location -Name $resourceGroupName

# create container registry
$acr = New-AzContainerRegistry `
    -ResourceGroupName $resourceGroupName `
    -Location $location `
    -Name $acrName `
    -EnableAdminUser `
    -Sku Basic

# login to container registry
$creds = Get-AzContainerRegistryCredential -Registry $acr

$creds.Password | docker login $acr.LoginServer -u $creds.Username --password-stdin

# build image and push it to our new registry
cd "Module05.DockerApp"

# Docker Desktop needs to be installed for the following part to work
# the binaries also need to be added to the Path Environment variable

# first build the image locally
docker build -t "counter-image:latest" .

# tag the image with the location we want to push it to (our azure container registry)
$remoteImageName = "$($acrName).azurecr.io/counter-image:latest"
docker tag counter-image:latest $remoteImageName

#then push it there
docker push $remoteImageName

# test running a container locally from the pushed image with the following line
# docker run "$($acrName).azurecr.io/counter-image:latest"


# create a container group from the image
$credpwsecure = ConvertTo-SecureString $creds.Password -AsPlainText -Force
$pscred = new-object System.Management.Automation.PSCredential ($acrName, $credpwsecure)
$containerGroup = New-AzContainerGroup `
    -ResourceGroupName $resourceGroupName `
    -Name $containerName `
    -Image $remoteImageName `
    -IpAddressType Public `
    -RegistryCredential $pscred