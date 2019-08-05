



Get-PiraeusEventMetrics cmdlet
=====
[Back](MgmtApi.md)

Returns metrics for a π-system.

| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| ResourceUriString| N            |  Unique URI identifier of π-system.                   |                                                    
|                                                                                                         
**Example**

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 
	
$piSystemId= "http://example.org/pisystem-A"

Get-PiraeusSubscriptionMetrics `
              -ServiceUrl $url  `
              -SecurityToken $token `
              -ResourceUri $piSystemId
```
[Management API](MgmtApi.md)


