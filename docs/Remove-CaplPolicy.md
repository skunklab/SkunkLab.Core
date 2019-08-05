

Remove-CaplPolicy Cmdlet
===
[Back](MgmtApi.md)

Deletes a a CAPL Authorization Policy from Piraeus.



| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| PolicyID      | N            | The unique policy ID URI that identifies the CAPL policy.                             |
|                                                                                                         

**Example**
```
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code

$policyId = “http://skunklab.io/policy/test”
$policy = Remove-CaplPolicy
                         -PolicyID $policyId  
                         -ServiceUrl $url  
                         – SecurityToken $token
                                          
```
[Management API](MgmtApi.md)



