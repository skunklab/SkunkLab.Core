
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
$pisystemId = "http://example.org/pisystem-A"

Get-PiraeusEventMetadata `
              -ResourceUriString $pisystemId `
              -ServiceUrl $url  `
              -SecurityToken $token

```
[Management API](MgmtApi.md)

