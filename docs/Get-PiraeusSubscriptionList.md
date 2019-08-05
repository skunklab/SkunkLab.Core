




Get-PiraeusSubscriptionList cmdlet
=====
[Back](MgmtApi.md)

Returns a list of subscriptions for a π-system, which includes durable and ephemeral subscriptions.

| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| ResourceUriString| N            |  unique identifier of the pi-system as a URI.                   |                                                    
|                                                                                                         
**Example**

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 
	
$pisystemId = "http://example.org/pisystem-A"

Get-PiraeusSubscriptionList `
              -ServiceUrl $url  `
              -SecurityToken $token `
              -ResourceUriString $pisystemId 
```
[Management API](MgmtApi.md)

