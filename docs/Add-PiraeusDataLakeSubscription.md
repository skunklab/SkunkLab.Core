

Add-PiraeusDataLakeSubscription cmdlet
=====
[Back](MgmtApi.md)
Adds a subscription for Azure Data Lake as a static route from a π-system.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | The Azure Data Lake store account name.                                                                                                |
| Domain| N            | The AAD domain,e.g, microsoft.onmicrosoft.com.                                                                                                         |
| AppId| N            | Application ID for access from AAD.                                                                                                |
| ClientSecret| N            | Secret for access from AAD.                                                                 |
| Folder| N            | Name of folder to write data.                                                              |
| Filename          | N            | Name of filename to write data, but exclusive of an extension.                |
| NumClients        | Y            | The number Azure Data Lake clients associated with deployment. For high frequency writes multiple clients can be used. The default is 1. |
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
$domain= “microsoft.onmicrosoft.com”  
$appId = "...AAD App ID..."
$clientSecret = "...AAD client secret..."
$folder = "myfolder"
$filename = “myappendfile.txt” 
$description = “Test Azure Data Lake subscription”

Add-PiraeusDataLakeSubscription `
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystemId `  
	-Account $account `
	-Domain $container `
	-AppId $appId `
	-ClientSecret $clientSecret `  
	-Folder $folder `
	-Filename $filename `
	-Description $description
```

[Management API](MgmtApi.md)
