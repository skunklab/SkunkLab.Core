Piraeus Management API

| Cmdlet | Get-PiraeusManagementToken |
|--------|----------------------------|


Returns: JWT security token for use in all other cmdlets

| **Parameter** | **Optional** | **Description**                                                      |
|---------------|--------------|----------------------------------------------------------------------|
| ServiceUrl    | N            | URL of Piraeus Management API                                        |
| Key           | N            | Authentication code used to authenticate with Piraeus Management API |
                                                                                                    

**Example**
```
$url = “https://<dns>.<location>.cloudapp.azure.com”  
$code = “12345678”

$token = Get-PiraeusManagementToken -ServiceUrl $url -Key $code
```

