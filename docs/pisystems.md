Piraeus π-systems
=====
Piraeus is a communications system which allows subsystems to interconnect.  That is possible because the subsystems share variables between each other, i.e., the output of one subsystem is the input to another.  This variable sharing implies  interconnected subsystems have an intersection in their respective $\sigma$-algebras, an event shared between the subsystems.  The importance of $\pi$-system is that if the subsystems agree on the $\pi$-system, then they agree of the $\sigma$-algebra that generated the $\pi$-system.

A $\pi$-system is Piraeus is where the subsystems "agree" either by sending or receiving from the $\pi$-system and thus the intersected event common in the subsystems respective $\sigma$-algebras, which establishes interconnection.

This means we need a mechanism to create $\pi$-systems in Piraeus.  The following are all the properties that can used to describe the $\pi$-system with its metadata.
| Property                 | Description                                                                                                                                                                                 |
|--------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ResourceUriString        | A unique identifier of the pi-system as a URI                                                                                                                                               |
| Description              | An optional text description of the pi-system                                                                                                                                               |
| DiscoveryUrl             | An optional URL where more information can be found about this pi-system                                                                                                                    |
| Enabled                  | A boolean flag that determines if the pi-system is operational.  If false the pi-system is cannot send or receive events.                                                                                                                |
| Expires                  | An optional DateTime for expiration.  If omitted never expires.                                                                                                                             |
| MaxSubscriptionDuration  | An optional maximum duration (TimeSpan) that a subscription can associated with the pi-system.                                                                                              |
| Audit                    | A boolean flag that determines whether information following through the pi-system is audited.                                                                                              |
| RequireEncryptedChannel  | A boolean flag that determines if information received or sent must be through an encrypted channel.  Note:  If an ingress controller is used for SSL offloading, the flag should be false. |
| PublishPolicyUriString   | The CAPL PolicyID to apply access control for subsystems sending to the pi-system                                                                                                           |
| SubscribePolicyUriString | The CAPL PolicyID to apply access control for subsystems receiving from the pi-system                                                                                                       |

## Putting It Together

The PowerShell sample below shows how the $\pi$-system is added to Piaeus
```diff
+#variables
$publishCaplPolicy = "http://example.org/publish/pisystem-A"
$subscribeCaplPolicy = "http://example.org/subscribe/pisystem-A"
$pisystemId = "http://example.org/pisystem-A"

+#add the pi-system to piraeus

Add-PiraeusEventMetadata -ResourceUriString $pisystemId -Enabled $true -RequireEncryptedChannel $false -Audit $false -PublishPolicyUriString $publicCaplPolciy -SubscribePolicyUriString $subscribeCaplPolicy -ServiceUrl $url -SecurityToken $token 


```

