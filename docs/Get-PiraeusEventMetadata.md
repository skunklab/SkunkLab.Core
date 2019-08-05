
Get-PiraeusEventMetadata cmdlet
=====
[Back](MgmtApi.md)

Returns the metadata for a Piraeus π-system.

**Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| ResourceUriString        |N| A unique identifier of the pi-system as a URI                                                                                                                                               |
**Example**

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$pisystemId = "http://example.org/pisystem-A"

Get-PiraeusEventMetadata `
              -ResourceUriString $pisystemId `
              -ServiceUrl $url  `
              -SecurityToken $token

```
[Management API](MgmtApi.md)

