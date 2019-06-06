Piraeus Management API

| Cmdlet | Get-CaplPolicy |
|--------|----------------|


Returns: CAPL AuthorizationPolicy object from Piraeus.

| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| PolicyID      | N            | The unique policy ID URI that identifies the CAPL policy.                             |
|                                                                                                         

Example
```
$policyId = “http://skunklab.io/policy/test”

$policy = Get-CaplPolicy
                         -PolicyID $policyId  
                         -ServiceUrl $url  
                         – SecurityToken $token
                                          
```



