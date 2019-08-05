





Add-PiraeusRedisCacheSubscription Cmdlet
=====
[Back](MgmtApi.md)

Adds a subscription for Redis Cache to receive events as a static route from a π-system.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account| N            | Azure Redis account, e.g., <account>.redis.cache.windows.net.                                                                                           |
| SecurityKey| N            | Redis security key.                                                                                                        |
| DatabaseNum| Y| (Optional) Redis database number to use for the cache.  If omitted, will use the default database.|                                                                                              |
| Expiry| Y            |(Optional) expiry of a cached item.               |
|ClaimType|Y|(Optional) claim type for the identity used as the cache key.  If omitted, the resource URI query string must contain cachekey parameter and value to set the key.  If query string parameter is used it will override the claim type.|
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |


**Example**

```

$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$piSystemId = “http://skunklab.io/test/resource-a”  
$account= “myredisacct”  
$securityKey = “...redis_key...”  
$databaseNo = 1
$expiry = New-TimeSpan -Hours  1
$claimType= "http://skunklab.io/name"
$description = “Test Web Service subscription”

Add-PiraeusRedisCacheSubscription `
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystemId `  
	-Account $account `
	-SecurityKey $securityKey `
	-DatabaseNum $databaseNo `
	-Expiry $expiry `
	-ClaimType claimType `
	-Description $description
```

[Management API](MgmtApi.md)


