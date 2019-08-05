





Add-PiraeusWebServiceSubscription Cmdlet
=====
[Back](MgmtApi.md)

Adds a subscription for an Web Service, which includes Azure Functions with HTTP-POST trigger to receive events as a static route from a π-system.

**Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| WebServiceUrl| N            | URL of Web service to send messages which can include a query string.                                                                                                |
| Issuer| Y            | (Optional) Issuer to include in security token sent to Web service for symmetric key tokens.                                                                                                        |
| Audience| Y| (Optional) Audience to include in security token sent to Web service for symmetric key tokens.|                                                                                              |
| TokenType| N            | Type of security token to be used when sending to Web service, e.g., None, JWT, X509.               |
|Key|Y|(Optional) Symmetric key used to build security token for authentication with Web service when TokenType is JWT.|
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |


**Example**

```

$url = "http://piraeus.eastus.cloudapp.azure.com"  
$code = "12345678"  
$token = Get-PiraeusManagementToken '
	-ServiceUrl $url `
	-Key $code 

$piSystemId = “http://skunklab.io/test/resource-a”  
$wsurl= “https://foo.bar.com/api/doit?code=xyz”  
$key= “...symmetric_key_for_jwt”  
$issuer= "http://foo.bar.com/'
$audience= "http://foo.bar.com/"
$description = “Test Web Service subscription”

Add-PiraeusWebServiceSubscription `
	-ServiceUrl $url `
	-SecurityToken $token `	
	-ResourceUriString $piSystemId `  
	-WebServiceUrl $wsUrl `
	-Issuer $isser `
	-Audience $audience `
	-TokenType JWT
	-Key $key ` 
	-Description $description
```

[Management API](MgmtApi.md)

