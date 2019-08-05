



Add-PiraeusQueueStorageSubscription cmdlet
=====
[Back](MgmtApi.md)
Adds a subscription for an Azure storage queue to receive events as a static route from a π-system.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | Account name of Azure Queue Storage, e.g, <account>.queue.core.windows.net                                                                                                |
| Queue| N            | Name of queue to write messages.                                                                                                        |
| TTL| Y| (Optional) TTL as TimeSpan for messages to remain in queue.|                                                                                              |
| Key| N            | Either storage key or SAS token for account or queue.               |
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |


**Example**

```

$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$piSystemId = “http://skunklab.io/test/resource-a”  
$account = “mystorageacct”  
$queue= “myqueue”  
$ttl = New-TimeSpan -Hours  1
$key= "...storage_acct_key..."
$description = “Test Queue Storage subscription”

Add-PiraeusQueueStorageSubscription `
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystemId `  
	-Account $account `
	-Queue $queue `
	-Key $key ` 
	-TTL $ttl `
	-Description $description
```

[Management API](MgmtApi.md)

