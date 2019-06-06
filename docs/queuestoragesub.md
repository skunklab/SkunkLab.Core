

Azure Queue Storage Subscription
===============================

Adds a subscription for Azure Queue storage as a static route to a π-system.

| **Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | The Azure storage account name.                                                                                                |
| Key               | N            | The Azure storage key.                                                                                                         |
| Queue         | N            | Azure storage queue that receives the event. Blob Storage.                                                                 |
| TTL          | Y            | Time-To-Live (TTL) for the message in the Azure Storage Queue. If omitted the TTL is infinite.                                                             |
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |


**Example**

```

$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key $code  
  
$piSystem = “http://skunklab.io/test/resource-a”  
$account = “mystorageacct”  
$container = “myfiles”  
$filename = “myappendfile.txt”  
$key = <storage_account_key_from_portal>  
$description = “Test Azure Blob storage subscription”

Add-PiraeusBlobStorageSubscription -ServiceUrl $url -SecurityToken $token `  
-ResourceUriString $piSystem `  
-Account $account `  
-Container $container `  
-BlobType Append `  
-Filename $filename ```
