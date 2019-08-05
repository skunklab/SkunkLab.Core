

Remove-PiraeusEvent cmdlet
=====
Removes a Piraeus π-system and all subscriptions.
roperty                 | Description                                                                                                                                                                                 |
|--------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ResourceUriString        | A unique identifier of the pi-system as a URI                                                                                                                                               |
                                                                                             |

## Putting It Together

The PowerShell sample below shows how the $\pi$-system is returned from Piraeus
```diff
+#variables

$pisystemId = "http://example.org/pisystem-A"

+#add the pi-system to piraeus

Get-PiraeusEventMetadata
              -ResourceUriString $pisystemId 
              -ServiceUrl $url  
              -SecurityToken $token


```

