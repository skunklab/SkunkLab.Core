



Add-PiraeusIotHubDeviceSubscription Cmdlet
=====
[Back](MgmtApi.md)

Adds a subscription for IoT Hub device as a static route from a π-system, i.e., the subscription acts as a IoT Hub device.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | Account name of IoT Hub, e.g, <account>.azure-devices.net.                                                                                                |
| DeviceId| N            | Device ID that will receive messages.                                                                                                        |                                                                                      |
| PropertyName| Y            | (Optional) property name to use when sending to device, i.e., used with property value.                                                                 |
| PropertyValue| Y            | (Optional) property value to use when sending to device, i.e., used with property name.                                                              |
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
$key= "...some_sas_token..."
$propertyName= "CommandName"
$propertyValue = “MyCommand” 
$description = “Test IoT Hub Device subscription”

Add-PiraeusIotHubDeviceSubscription `
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystemId `  
	-Account $account `
	-DeviceId $deviceId `
	-Key $key `  
	-PropertyName $propertyName `
	-PropertyVale $propertyValue `
	-Description $description
```

[Management API](MgmtApi.md)
