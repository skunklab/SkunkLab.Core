
New-CaplRule Cmdlet
===
[Back](MgmtApi.md)

Returns a CAPL rule object.

| **Parameter**   | **Optional** | **Description**                                                           |
|-----------------|--------------|---------------------------------------------------------------------------|
| Evaluates       | N            | The truthful evaluation of the Rule (true \| false)                       |
| MatchExpression | N            | CAPL match expression object.                                             |
| Operation       | N            | CAPL operation object.                                                    |
| Issuer          | Y            | Optional Issuer used to scope the Rule to the issuer of a security token. |
|                 |              |                                                                           
**Example**
```
$rule = New-CaplRule  `
                   -Evaluates $true  `
                   -MatchExpression $match  `
                   -Operation $operation
```
[Management API](MgmtApi.md)
