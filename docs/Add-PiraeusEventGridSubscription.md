


Add-PiraeusEventGridSubscription Cmdlet
=====
[Back](MgmtApi.md)

Adds a subscription for Event Grid as a static route from a π-system.

| **Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Host| N            | Full host name of the Event Grid, e.g., piraeussampletopic.eastus-1.eventgrid.azure.net.                                                                                                |
| TopicKey               | N            | Event Grid topic key.                                                                                                         |
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
$topicKey = "...some_topic_key..."
$host = "piraeussampletopic.eastus-1.eventgrid.azure.net"
$numClients = 1
$description = "Test Event Grid Subscription"

Add-PiraeusEventGridSubscription -ServiceUrl $url -SecurityToken $token `
                                 -ResourceUriString $piSystemId`
                                 -TopicKey $topicKey `
                                 -Host $host `
                                 -NumClients $numClients `
                                 -Description $description
  ```
  
  [Management API](MgmtApi.md)                
                  
