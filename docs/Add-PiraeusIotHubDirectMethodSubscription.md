



Add-PiraeusIotHubDirectMethodSubscription cmdlet
=====
[Back](MgmtApi.md)
Adds a subscription for IoT Hub direct method to send to a device as a static route from a π-system.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | Account name of IoT Hub, e.g, <account>.azure-devices.net.                                                                                                |
| DeviceId| N            | Device ID that will receive messages.                                                                                                        |
| Method | N| Name of method to be called on device.|
| KeyName| N            | Name key used for authentication.                                                                                                |
| Key| N            | SAS token used for authentication.                |
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |


**Example**

```

$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$piSystemId = “http://skunklab.io/test/resource-a”  
$account = “myiothubacct”  
$deviceId= “device1”  
$keyname= "mykeyname"
$key= "...some_sas_token..."
$method= "mymethod"
$description = “Test IoT Hub Direct Method subscription”

Add-PiraeusIotHubDirectMethodSubscription`
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystemId `  
	-Account $account `
	-DeviceId $deviceId `
	-KeyName $keyname `
	-Key $key `  
	-Method $method `
	-Description $description
```

[Management API](MgmtApi.md)
