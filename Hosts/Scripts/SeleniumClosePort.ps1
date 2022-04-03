[CmdletBinding()]
param (
    $RunName
)

function removeRuleFromNsg {
    param ([string]$nsgname, [string]$rulename, [string]$resourceGroup)

    try {
        $nsg = Get-AzNetworkSecurityGroup -Name $nsgname -ResourceGroupName $resourceGroup

        Remove-AzNetworkSecurityRuleConfig -Name $rulename -NetworkSecurityGroup $nsg

        $nsg | Set-AzNetworkSecurityGroup
        return 1
    }
    catch {
        Write-Host "An error occurred:"
        Write-Host $_.Exception.Message
        return 0
    }
}

function removeRuleFromAppService{
    Param( 
        [Parameter(Mandatory = $true)] 
        [string] $ResourceGroupName, 
        [Parameter(Mandatory = $true)] 
        [string] $AppServiceName, 
        [Parameter(Mandatory = $true)]
        [string] $rulename 
    )

    try {
        $ErrorActionPreference = "Stop"
        $APIVersion = "2018-11-01"

        $WebAppConfig = Get-AzResource -ResourceName $AppServiceName -ResourceType Microsoft.Web/sites/config -ResourceGroupName $ResourceGroupName -ApiVersion $APIVersion 

        $IpSecurityRestrictions = $WebAppConfig.properties.ipSecurityRestrictions
        [System.Collections.ArrayList]$list = $IpSecurityRestrictions

        $foundRecord = $list | where { $_.name -eq $rulename }
        if($foundRecord) {
           $list.Remove($foundRecord)
           $WebAppConfig.Properties.ipSecurityRestrictions = $list
       }

       Set-AzResource -ResourceId $WebAppConfig.ResourceId -Properties $WebAppConfig.Properties -Force -ApiVersion $APIVersion 
       return 1
    }
    catch {
        Write-Host "An error occurred:"
        Write-Host $_.Exception.Message
        return 0
    }
}


# Selenium NSG variabelen
$seleniumResourceGroup = "selenium"
$seleniumNsgName="SeleniumHubNode-nsg"
$seleniumPort=4444

#Shared variables
$rulename=$RunName



#Selenium NSG sluiten
$try = 0
do{
    $result = removeRuleFromNsg -nsgname $seleniumNsgName -rulename $rulename -resourceGroup $seleniumResourceGroup
}until ($try++ -ge 2 -or $result)


if($result -eq 0){
    throw "Sluiten van poort is niet geslaagd na " + $try + " pogingen."
}
