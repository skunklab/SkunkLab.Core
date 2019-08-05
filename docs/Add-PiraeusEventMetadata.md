
Add-PiraeusEventMetadata Cmdlet
=====
[Back](MgmtApi.md)

Adds a $\pi$-system to Piraeus.


| **Parameter** | **Optional** | **Description**                                                                       |
|---------------|--------------|---------------------------------------------------------------------------------------|
| ServiceUrl    | N            | The management API service URL, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com |
| SecurityToken | N            | The security token previously acquired to access the management API.                  |
| ResourceUriString        | N|A unique identifier of the pi-system as a URI                                                                                                                                               |
| Description              |Y | An optional text description of the pi-system                                                                                                                                               |
| DiscoveryUrl             |Y| An optional URL where more information can be found about this pi-system                                                                                                                    |
| Enabled                  |N| A boolean flag that determines if the pi-system is operational.  If false the pi-system is cannot send or receive events.                                                                                                                |
| Expires                  |Y| An optional DateTime for expiration.  If omitted never expires.                                                                                                                             |
| MaxSubscriptionDuration  |Y| An optional maximum duration (TimeSpan) that a subscription can associated with the pi-system.                                                                                              |
| Audit                    |N| A boolean flag that determines whether information following through the pi-system is audited.                                                                                              |
| RequireEncryptedChannel  |N| A boolean flag that determines if information received or sent must be through an encrypted channel.  Note:  If an ingress controller is used for SSL offloading, the flag should be false. |
| PublishPolicyUriString   |N| The CAPL PolicyID to apply access control for subsystems sending to the pi-system                                                                                                           |
| SubscribePolicyUriString |N| The CAPL PolicyID to apply access control for subsystems receiving from the pi-system                                                                                                       |

**Example**

The PowerShell sample below shows how the $\pi$-system is added to Piraeus
```diff

$description = "My Sample"
$publishCaplPolicy = "http://example.org/publish/pisystem-A"
$subscribeCaplPolicy = "http://example.org/subscribe/pisystem-A"
$pisystemId = "http://example.org/pisystem-A"

Add-PiraeusEventMetadata `
	-ResourceUriString $pisystemId `
	-Enabled $true `
	-RequireEncryptedChannel $true `
	-Audit $false `
	-Description $description `
	-PublishPolicyUriString $publicCaplPolciy `
	-SubscribePolicyUriString $subscribeCaplPolicy `
	-ServiceUrl $url `
	-SecurityToken $token 
```
[Management API](MgmtApi.md)

