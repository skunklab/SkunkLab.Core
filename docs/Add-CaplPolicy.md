Add-CaplPolicy Cmdlet
===
[Back](MgmtApi.md)

Adds a CAPL authorization policy to Piraeus.



| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| Policy      | N            | CAPL policy to add to Piraeus.  If the policy ID exists, the policy will be updated.                             |
|                                                                                                         

**Example**
```
$policyId = “http://skunklab.io/policy/test”

Add-CaplPolicy `
              -Policy $policy  `
              -ServiceUrl $url  `
              -SecurityToken $token `
                                          
```

[Management API](MgmtApi.md)

