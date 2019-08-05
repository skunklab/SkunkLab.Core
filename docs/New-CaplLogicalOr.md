
New-CaplLogicalOr Cmdlet
===
[Back](MgmtApi.md)

Returns a CAPL LogicalOr object.

| **Parameter** | **Optional** | **Description**                                                                                  |
|---------------|--------------|--------------------------------------------------------------------------------------------------|
| Evaluates     | N            | Truthful evaluation of the Logical Or connective.                                               |
| Terms         | N            | An array of Evaluation Expressions, i.e., Rules and/or Logical Connectives (Logical OR and ANDs) |
|               |              |                                                                                                                                                                                           

Example
```
$terms = @($rule1, $rule2)

$logicalOr = New-CapLogicalOr -Evaluates $true -Terms $terms  
```
[Management API](MgmtApi.md)
