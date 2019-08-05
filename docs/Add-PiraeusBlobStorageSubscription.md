
Add-PiraeusBlobStorageSubscription cmdlet
===============================
[Back](MgmtApi.md)
Adds a subscription for Azure Blob Storage as a static route from a π-system.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | The Azure Blob storage account name.                                                                                                |
| Key               | N            | The Azure Blob storage key.                                                                                                         |
| Container         | Y            | Optional container name to store the file(s) in Azure Blob Storage.                                                                 |
| BlobType          | N            | The type of Azure Blob to be used, i.e., one of (Block, Append, Page).                                                              |
| NumClients        | Y            | The number Azure Blob clients associated with deployment. For high frequency writes multiple clients can be used. The default is 1. |
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |
| Filename          | Y            | A filename which is an optional parameter, but used when the BlobType is “Append”, i.e., appending to a single file.                |

**Example**

```

$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$piSystem = “http://skunklab.io/test/resource-a”  
$account = “mystorageacct”  
$container = “myfiles”  
$filename = “myappendfile.txt”  
$key =  "...blob-storage-key..."
$description = “Test Azure Blob storage subscription”

Add-PiraeusBlobStorageSubscription `
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystem `  
	-Account $account `
	-Container $container`  
	-BlobType Append `  
	-Filename $filename
```

[Management API](MgmtApi.md)
