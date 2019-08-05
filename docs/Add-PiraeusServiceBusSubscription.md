




Add-PiraeusServiceBusSubscription cmdlet
=====
[Back](MgmtApi.md)
Adds a subscription for an Service Bus topic to receive events as a static route from a π-system.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | Account name of Azure Queue Storage, e.g, <account>.queue.core.windows.net                                                                                                |
| Topic| N            | Service Bus topic send messages.                                                                                                        |
| KeyName| N| Name of key used for authentication.|                                                                                              |
| Key| N            | SAS token used for authentication.               |
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |


**Example**

```

$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$piSystemId = “http://skunklab.io/test/resource-a”  
$account = “mysbacct”  
$topic= “myqueue”  
$keyname= "mykeyname'
$key= "..sb_sas_token.."
$description = “Test Service Bus subscription”

Add-PiraeusServiceBusSubscription `
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystemId `  
	-Account $account `
	-Topic $topic `
	-KeyName $keyname
	-Key $key ` 
	-Description $description
```

[Management API](MgmtApi.md)

