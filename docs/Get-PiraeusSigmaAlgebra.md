

Get-PiraeusSigmaAlgebra cmdlet
=====
[Back](MgmtApi.md)

Returns a list of  Piraeus π-systems as an array of strings.

| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |                          
                                                                                                         
**Example**

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

Get-PiraeusSigmaAlgebra `
              -ServiceUrl $url  `
              -SecurityToken $token
```
[Management API](MgmtApi.md)
