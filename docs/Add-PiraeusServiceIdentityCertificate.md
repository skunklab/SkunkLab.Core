

Add-PiraeusServiceIdentityCertificate Cmdlet
=====
[Back](MgmtApi.md)

Sets an X.509 certificate to be used by Piraeus as an identity for subscriptions requiring a certificate identity.  The cmdlet is not required to be used and a certificate identity can be set on Piraeus deployment.  Using the cmdlet requires that the certificate is accessible by a container or server used by Piraeus. 
**Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| Name|N| Unique name of the service identity.                                                                                                                                              |
| Path|Y| Path to certificate. Use either this OR store, location, and thumbprint parameters.                                                                                                                                            |
| Store|Y| Store name where certificate is located. Used with location and thumbprint, but must omit Path parameter.                                                                                                                                           |
| Location|Y| Location where certificate is located. Used with store and thumbprint, but must omit Path parameter.                                                                                                                                            |
| Thumbprint|Y| Thumbprint of certificate. Used with store and location, but must omit Path parameter.                                                                                                                                         |
| Password|N| Certificate password.                                                                                                                                       |
**Example**

```diff
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$path = "./certs"
$password = "mysecert"

Add-PiraeusServiceIdentityCertificate `
              -Path $path `
              -Password $passwrod `
              -ServiceUrl $url  `
              -SecurityToken $token

```
[Management API](MgmtApi.md)

