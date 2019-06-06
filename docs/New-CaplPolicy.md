Piraeus Management API

| Cmdlet | New-CaplPolicy |
|--------|----------------|


Returns: CAPL AuthorizationPolicy object.

| **Parameter**        | **Optional** | **Description**                                                                                                                                                                                       |
|----------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| PolicyID             | N            | The unique identifier of the policy as a URI.                                                                                                                                                         |
| EvaluationExpression | N            | One of Rule, LogicalOr, or LogicalAnd objects.                                                                                                                                                        |
| Transforms           | Y            | An optional array of 1 or more Transform objects                                                                                                                                                      |
| Delegation           | Y            | An optional Boolean that indicates whether policy will evaluation claims by constrained delegation, i.e., the secondary set of claims associated with the security token. The default value is false. |
                                                                                                                                                                                                

Example
```
$policyId = “http://skunklab.io/policy/test”

$policy = New-CaplPolicy -PolicyID $policyId -EvaluationExpression $rule  
```


