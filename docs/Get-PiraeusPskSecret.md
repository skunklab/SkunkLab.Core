



Get-PiraeusPskSecret cmdlet
=====
[Back](MgmtApi.md)

Returns a PSK secret for a PSK identity.

| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| Identity| N            | Psk Identity.                |                                                    
|                                                                                                         
**Example**

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 
$identity = "myPskIdentity"
Get-PiraeusPskSecret `
              -ServiceUrl $url  `
              -SecurityToken $token `
              -Identity $identity
```
[Management API](MgmtApi.md)
