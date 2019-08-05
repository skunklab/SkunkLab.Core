

Add-PiraeusServiceIdentityClaims Cmdlet
=====
[Back](MgmtApi.md)

Sets Piraeus service identity claims for symmetric key tokens require by subscriptions, e.g., a Web service subscription using a JWT token.  The cmdlet is purely optional as these parameters are set on a Piraeus deployment.

**Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| Name|N| Unique name of the service identity.                                                                                                                                              |
| ClaimTypes|N| "Semi-colon delimited list of claim types.  Must match number of claim values.                                                                                                                                            |
| ClaimValues|N| Semi-colon delimited list of claim values.  Must match number of claim types.                                                                                                                                          |

**Example**

```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$claimTypes= "http://piraeus.skunklab.io/name;http://piraeus.skunklab.io/role"
$claimValues = "piraeus;service"

Add-PiraeusServiceIdentityClaims `
              -ClaimTypes $claimTypes `
              -ClaimValues $claimValues `
              -ServiceUrl $url  `
              -SecurityToken $token

```
[Management API](MgmtApi.md)


