
function New-SampleConfig()
{
    param([string]$DnsName, [string]$Location, [string]$Key="12345678")
    
    $url = "https://$DnsName.$Location.cloudapp.azure.com"
    Write-Host "Using $url for management api" -ForegroundColor Yellow

    $importer = Read-Host "Do you need to import the Piraeus Management Powershell Module (Y/N) ? "
    if($importer.ToLowerInvariant() -eq "y")
    {
        Import-Module "../src/Piraeus.Module.Core/bin/Release/netcoreapp2.2/Piraeus.Module.Core.dll"
        Write-Host "Module imported" -ForegroundColor Yellow
    }

    #get a security token for the management API
    $token = Get-PiraeusManagementToken -ServiceUrl $url -Key $Key
        
    Write-Host "---  INFORMATION ABOUT Sample Config ----" -ForegroundColor White
    Write-Host "The client demos create security tokens based on the selection of a 'Role', i.e., 'A' or 'B'" -ForegroundColor White
    Write-Host "The script will create 2 CAPL policies" -ForegroundColor White
    Write-Host "  (1) a client in role 'A' may transmit to 'resource-a' and subscribe to 'resource-b'" -ForegroundColor White
    Write-Host "  (2) a client in role 'B' may transmit to 'resource-b' and subscribe to 'resource-a'" -ForegroundColor White
    Write-Host "-----------------------------------------" -ForegroundColor White
    Write-Host ""
    Start-Sleep -Seconds 3
        
    #--------------- CAPL policy for users in role "A" ------------------------
    Write-Host "-- Building CAPL Authorization Policies ---" -ForegroundColor White
    Write-Host "  (1) Match Expression : Find a claim type in the security token" -ForegroundColor White
    Write-Host "  (2) Operation -- Binds a claim value from the matched claim type to perform an operation, e.g., Equals" -ForegroundColor White
    Write-Host "  (3) Rule -- Create a rule that binds a match expression and an operation" -ForegroundColor White
    Write-Host "  (4) Policy -- create a policy that is uniquely identifiable, that incorporates a Rule (or Logical Connective)" -ForegroundColor White
    Write-Host ""
    Start-Sleep -Seconds 5              

    #define the claim type to match to determines the client's role
    $matchClaimType = "http://www.skunklab.io/role"

    #create a match expression of 'Literal' to match the role claim type
    $match = New-CaplMatch -Type Literal -ClaimType $matchClaimType -Required $true  

    #create an operation to check the match claim value is 'Equal' to "A"
    $operation_A = New-CaplOperation -Type Equal -Value "A"

    #create a rule to bind the match expression and operation
    $rule_A = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_A

    #define a unique identifier (as URI) for the policy
    $policyId_A = "http://www.skunklab.io/resource-a" 

    #create the policy for clients in role "A"
    $policy_A = New-CaplPolicy -PolicyID $policyId_A -EvaluationExpression $rule_A
    #-------------------End Policy for "B"------------------------------------
        
        
    #--------------- CAPL policy for users in role "B" ------------------------

    #create an operation to check the match claim value is 'Equal' to "B"
    $operation_B = New-CaplOperation -Type Equal -Value "B"

    #create a rule to bind the match expression and operation
    $rule_B = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_B

    #define a unique identifier (as URI) for the policy
    $policyId_B = "http://www.skunklab.io/resource-b" 

    #create the policy for users in role "A"
    $policy_B = New-CaplPolicy -PolicyID $policyId_B -EvaluationExpression $rule_B

    #-------------------End Policy for "B"------------------------------------

    # The policies are completed.  We need to add them to Piraeus

    Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_A 
    Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_B

    Write-Host "CAPL policies added to Piraeus" -ForegroundColor Yellow


    #Uniquely identify Piraeus resources by URI
    $resource_A = "http://www.skunklab.io/resource-a"
    $resource_B = "http://www.skunklab.io/resource-b"

    #Add the resources to Piraeus

    #Resource "A" lets users with role "A" send and users with role "B" subscribe to receive transmissions
    Add-PiraeusEventMetadata -ResourceUriString $resource_A -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $policyId_A -SubscribePolicyUriString $policyId_B -ServiceUrl $url -SecurityToken $token -Audit $false

    #Resource "B" lets users with role "B" send and users with role "A" subscribe to receive transmissions
    Add-PiraeusEventMetadata -ResourceUriString $resource_B -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $policyId_B -SubscribePolicyUriString $policyId_A -ServiceUrl $url -SecurityToken $token -Audit $false

    Write-Host "PI-System metadata added to Piraeus" -ForegroundColor Yellow
    Write-Host""


        
    #Quick check get the resource data and verify what was set
    Write-Host "----- PI-System $resource_A Metadata ----" -ForegroundColor Green
    Get-PiraeusEventMetadata -ResourceUriString $resource_A -ServiceUrl $url -SecurityToken $token
    

    Write-Host""


    Write-Host "----- PI-System $resource_B Metadata ----" -ForegroundColor Green
    Get-PiraeusEventMetadata -ResourceUriString $resource_B -ServiceUrl $url -SecurityToken $token

    Write-Host""
    Write-Host "Done :-)  Dare Mighty Things" -ForegroundColor Cyan

    
}