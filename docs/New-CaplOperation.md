Piraeus Management API

| Cmdlet | New-CaplOperation |
|--------|-------------------|


Returns: CAPL operation object.

| **Parameter** | **Optional** | **Description**                                                                                                                                                                                                                                                                                                                                   |
|---------------|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Value         | Y            | Value as RHS bound input into a binary operation. Can be omitted for unary operations.                                                                                                                                                                                                                                                            |
| OperationType | N            | The type of operation to execute one of (i) BetweenDateTime (ii) Contains (iii) EqualDateTime (iv) EqualNumeric (v) Equal (vi) Exists (vii) GreaterThanDateTime (viii) GreaterThan (ix) GreaterThanOrEqualDateTime (x) GreaterThanOrEqual (xi) LessThanDateTime (xii) LessThan (xiii) LessThanOrEqualDateTime (xiv) LessThanOrEqual (xv) NotEqual |
|               |              |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |

Example
```
$value = “device”

$operation = New-CaplOperation  `
                              -Value $value  `
                              -OperationType Equal
```

