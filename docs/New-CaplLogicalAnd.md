Piraeus Management API

| Cmdlet | New-CaplLogicalAnd |
|--------|--------------------|


Returns: CAPL LogicalAnd object.

| **Parameter** | **Optional** | **Description**                                                                                  |
|---------------|--------------|--------------------------------------------------------------------------------------------------|
| Evaluates     | N            | Truthful evaluation of the Logical And connective.                                               |
| Terms         | N            | An array of Evaluation Expressions, i.e., Rules and/or Logical Connectives (Logical OR and ANDs) |
|               |              |                                                                                                                                                                                           

Example
```
$terms = @($rule1, $rule2)

$logicalAnd = New-CapLogicalAnd -Evaluates $true -Terms $terms  
```

