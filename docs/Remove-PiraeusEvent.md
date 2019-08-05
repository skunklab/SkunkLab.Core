

Remove-PiraeusEvent cmdlet
=====
[Back](MgmtApi.md)

Removes a Piraeus π-system and all of its subscriptions.

**Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| ResourceUriString        |N| A unique identifier of the pi-system as a URI                                                                                                                                               |
**Example**

The PowerShell sample below shows how the $\pi$-system is removed from Piraeus
```diff
$piSystemId= "http://example.org/pisystem-A"

Remove-PiraeusEventMetadata `
              -ResourceUriString $piSystemId `
              -ServiceUrl $url  `
              -SecurityToken $token 


```
[Management API](MgmtApi.md)
