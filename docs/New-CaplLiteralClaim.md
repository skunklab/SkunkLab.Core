


New-CaplLiteralClaim Cmdlet
===
[Back](MgmtApi.md)

Returns: CAPL LiteralClaim object.

| **Parameter** | **Optional** | **Description**                    |
|---------------|--------------|------------------------------------|
| ClaimType     | N            | Claim type of the literal claim, e.g., http://skunklab.io/claim |
| ClaimValue    | N            |Claim value of the literal claim, e.g., myvalue       |
|               |              |                                    |

**Example**
```
$claimType = “http://skunklab.io/claim/role”
$claimValue = “device”

$literalClaim = New-CaplLiteralClaim `
                                   -ClaimType $claimType `
                                   -ClaimValue $claimValue  
```
[Management API](MgmtApi.md)
