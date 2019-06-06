Piraeus Management API

| Cmdlet | New-CaplTransform |
|--------|-------------------|


Returns: CAPL transform object.

| **Parameter** | **Optional** | **Description**                                                                                                                                                     |
|---------------|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| TransformType | N            | One of ( Add \| Remove \| Replace)                                                                                                                                  |
| Match         | N            | CAPL Match Expression object                                                                                                                                        |
| LiteralClaim  | Y            | A Claim LiteralClaim object. Required for 'add' and 'replace' transforms. Not used for 'remove' transform.                                                          |
| Term          | Y            | Optional Evaluation Expression, i.e., Rule, LogicalAnd, or LogicalOr. The Term is used only whether a determination is needed as to whether to apply the transform. |
|               |              |                                                                                                                                                                                                                                                                                                                                    |

Example
```
$replacevalue = “device”

$transform = New-CaplTransform  `
                              -TransformType $replacevalue  `
                              -Match $match  `
                              -LiteralClaim $literalClaim
```
