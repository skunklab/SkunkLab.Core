
New-CaplMatch Cmdlet
===
[Back](MgmtApi.md)

Returns a CAPL match expression object.

| **Parameter** | **Optional** | **Description**                                                              |
|---------------|--------------|------------------------------------------------------------------------------|
| ClaimType     | N            |                                                                              |
| Required      | N            | Boolean value that determines if the Match is required.                      |
| MatchType     | N            | Type of match expression one of (Literal \| Pattern \| ComplexType \| Unary) |
| Value         | Y            | Value to match. List describes the value based on Match Type. (i) Literal - String (ii) Pattern - Reg Expression (iii) ComplexType - XPath (iv) Unary - Omitted                 |
|               |              |                                                                              

**Example**
```
$claimType = “http://skunklab.io/claim/role”  
$value = “device”

$matchExpression = New-CaplMatch  `
                       -ClaimType $claimType  `
                       -Required $true  `
                       -MatchType Literal `  
                       -Value $value `
```
[Management API](MgmtApi.md)

