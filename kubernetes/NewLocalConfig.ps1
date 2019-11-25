function New-LocalConfig()
{    
    
    $url = "http://localhost:29763"
    Write-Host "Using $url for management api" -ForegroundColor Yellow

    Import-Module "../src/Piraeus.Module.Core/bin/Debug/netcoreapp3.0/Piraeus.Module.Core.dll"
    Write-Host "Module imported" -ForegroundColor Yellow

    #get a security token for the management API
    Write-Host "--- Get security token for Piraeus configuration ---" -Foreground Yellow
    $token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"
    if($token -eq $null)
    {
		Write-Host ("Security token not acquired...terminating script.") -ForegroundColor Yellow
		return
    }  
    
    Write-Host "--- Got security token, ready to configure Piraeus ---" -ForegroundColor Green
         

    #define the claim type to match to determines the client's role
    $authority = "localhost"
    $matchClaimType = "http://$authority/role"

    #create a match expression of 'Literal' to match the role claim type
    $match = New-CaplMatch -Type Literal -ClaimType $matchClaimType -Required $true  

    #create an operation to check the match claim value is 'Equal' to "A"
    $operation_A = New-CaplOperation -Type Equal -Value "A"

    #create a rule to bind the match expression and operation
    $rule_A = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_A
    $rule_A2 = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_A
    $rules = @($rule_A, $rule_A2)
    $logicalAnd = New-CaplLogicalAnd -Evaluates $true -Terms $rules

    #define a unique identifier (as URI) for the policy
    $policyId_A = "http://$authority/policy/resource-a" 

    #create the policy for clients in role "A"
    $policy_A = New-CaplPolicy -PolicyID $policyId_A -EvaluationExpression $logicalAnd
    #-------------------End Policy for "B"------------------------------------
        
        
    #--------------- CAPL policy for users in role "B" ------------------------

    #create an operation to check the match claim value is 'Equal' to "B"
    $operation_B = New-CaplOperation -Type Equal -Value "B"

    #create a rule to bind the match expression and operation
    $rule_B = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_B

    #define a unique identifier (as URI) for the policy
    $policyId_B = "http://$authority/policy/resource-b" 

    #create the policy for users in role "A"
    $policy_B = New-CaplPolicy -PolicyID $policyId_B -EvaluationExpression $rule_B

    #-------------------End Policy for "B"------------------------------------

    # The policies are completed.  We need to add them to Piraeus

    Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_A 
    Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_B

    Write-Host "CAPL policies added to Piraeus" -ForegroundColor Yellow


    #Uniquely identify Piraeus resources by URI
    $resource_A = "http://$authority/resource-a"
    $resource_B = "http://$authority/resource-b"

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
}
