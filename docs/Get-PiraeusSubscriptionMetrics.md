


Get-PiraeusSubscriptionMetrics cmdlet
=====
[Back](MgmtApi.md)

Returns metrics for a subscription.

| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| SubscriptionUriString| N            |  Unique URI identifier of subscription.                   |                                                    
|                                                                                                         
**Example**

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 
	
$subscriptionId = "http://example.org/pisystem-A/c8b59c7c-484e-4dbe-9c3d-f25ee6fefa46"

Get-PiraeusSubscriptionMetrics `
              -ServiceUrl $url  `
              -SecurityToken $token `
              -SubscriptionUriString $subscriptionId 
```
[Management API](MgmtApi.md)


