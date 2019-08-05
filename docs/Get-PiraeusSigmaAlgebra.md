

Get-PiraeusSigmaAlgebra cmdlet
=====
Returns a list of  Piraeus π-systems as an array of strings.

| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |                          
|                                                                                                         
## Putting It Together

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
+#variables

$pisystemId = "http://example.org/pisystem-A"

+#add the pi-system to piraeus

Get-PiraeusSigmaAlgebra `
              -ServiceUrl $url  `
              -SecurityToken $token


```

