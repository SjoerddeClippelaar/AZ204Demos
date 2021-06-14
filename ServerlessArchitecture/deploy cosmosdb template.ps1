

param(
    [Parameter(Mandatory, ParameterSet="PrefixSet", Position=0)]
    [string]$Prefix, 

    [Parameter(Mandatory, ParameterSet="DatabaseNameSet")]
    [string]$DatabaseName)

if ($PSCmdlet.ParameterSetName -eq "PrefixSet")
{
    if ($Prefix.Length -eq 0)
    {
        Write-Error -Message "Prefix must contain at least 1 character"
        exit
    }    

    $DatabaseName = "$($Prefix)-tolldboothdb"
}

if ($DatabaseName.Length -eq 0)
{
    Write-Error -Message "Database name must contain at least 1 character"
    exit
}    

Login-AzAccount

$myResourceGroup="demos-serverlessarchitecture-rg"

Write-Host "Deploying template, this may take a few minutes"
$result = New-AzResourceGroupDeployment `
    -ResourceGroupName $myResourceGroup `
    -TemplateFile "cosmosdb-template.json" `
    -TemplateParameterObject @{"databaseAccounts_cosmosdb_name"="ewv-tollboothdb"} `
    -Name "dbrollout1"

if ($result.ProvisioningState -eq "Succeeded")
{
    Write-Host "Operation complete! You can now proceed to the third part."
}