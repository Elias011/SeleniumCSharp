[CmdletBinding()]
param (
    $RunName
)

function addRuleToNsg {
    param ([int]$port, [int]$priority, [string]$SourceIpAddress, [string]$rulename, [string]$resourceGroup, [string] $nsgname)

    try {
        # Get the NSG resource
        $nsg = Get-AzNetworkSecurityGroup -Name $nsgname -ResourceGroupName $resourceGroup

        # Add the inbound security rule.
        $nsg | Add-AzNetworkSecurityRuleConfig -Name $rulename -Description "Allow app port" -Access Allow `
            -Protocol * -Direction Inbound -Priority $priority -SourceAddressPrefix $SourceIpAddress -SourcePortRange * `
            -DestinationAddressPrefix * -DestinationPortRange $port

        # Update the NSG.
        $nsg | Set-AzNetworkSecurityGroup
        return 1
    }
    catch {
        Write-Host "An error occurred:"
        Write-Host $_.Exception.Message
        return 0
    }
}

function addRuleToAppService{
    Param( 
        [Parameter(Mandatory = $true)] 
        [string] $ResourceGroupName, 
        [Parameter(Mandatory = $true)] 
        [string] $AppServiceName, 
        [Parameter(Mandatory = $true)]
        [Hashtable[]] $NewIpRules #PSObject has parsing bug in az-module, so we ahve to use hashtable instead
    )

    try {
        $ErrorActionPreference = "Stop"
        $APIVersion = "2018-11-01"

        $WebAppConfig = Get-AzResource -ResourceName $AppServiceName -ResourceType Microsoft.Web/sites/config -ResourceGroupName $ResourceGroupName -ApiVersion $APIVersion 

        $IpSecurityRestrictions = $WebAppConfig.properties.ipSecurityRestrictions
        [System.Collections.ArrayList]$list = $IpSecurityRestrictions

        foreach($NewIpRule in $NewIpRules) {

            $newRecord = [PSCustomObject]@{
                ipAddress = $NewIpRule.ipAddress + '/32'; 
                action = $NewIpRule.action;
                priority = $NewIpRule.priority;
                name = $NewIpRule.name;
                description = $NewIpRule.description;
                tag = $NewIpRule.tag;
            }

            $list.Add($newRecord)
        }
       $WebAppConfig.Properties.ipSecurityRestrictions = $list

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
$priority = 2000 + (Get-Random -Maximum 500)
$rulename=$RunName
$SourceIpAddress = Invoke-RestMethod http://ipinfo.io/json | Select -exp ip



# Selenium NSG openen
$try = 0
do{
    $result = addRuleToNsg -port $seleniumPort -priority $priority -SourceIpAddress $SourceIpAddress -rulename $rulename -resourceGroup $seleniumResourceGroup -nsgname $seleniumNsgName
}until ($try++ -ge 2 -or $result)


if($result -eq 0){
    throw "open port in '"+ $nsgName +"' is not succsed after " + $try + " try."
}