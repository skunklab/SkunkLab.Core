



Add-PiraeusEventHubSubscription Cmdlet
=====
[Back](MgmtApi.md)

Adds a subscription for Event Grid as a static route from a π-system.

| **Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account| N            | Account name of EventHub, e.g, <account>.servicebus.windows.net                                                                                                |
| Hub| N            | Name of Event Hub.                                                                                                      |
| PartitionId |Y|(Optional) ID of partition if you want to send message to a single partition.|
| KeyName|N| Name of key used for authentication.|
|Key|N|Token used for authentication.|
| NumClients          | Y            | Number of Event Grid clients. Default is 1.                                                             |
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |


**Example**

```
$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$pisSystemId= "http://skunklab.io/test/resource-a"
$account = "myeventhubacct"
$hub = "myhub"
$keyname= "mykeyname"
$key= "...some_event_hub_key..."
$numClients = 1
$description = "Test Event Hub Subscription"

Add-PiraeusEventHubSubscription -ServiceUrl $url -SecurityToken $token `
                                 -ResourceUriString $piSystemId`
                                 -Account $account `
                                 -Hub $hub `
                                 -KeyName $keyname `
                                 -Key $key `
                                 -NumClients $numClients `
                                 -Description $description
  ```
  
  [Management API](MgmtApi.md)                
                  
