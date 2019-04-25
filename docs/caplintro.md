Claims Authorization Policy Language – CAPL
===========================================

CAPL is a distributed and security token access control language based on
claims. It is logic-based and allows an author to create simple or complex
expressions to allow or deny access to resources.  When a $\pi$-system is added to Piraeus, i.e., an event for sending and receiving information, the metadata of the $\pi$-system encodes a publish and subscription policy, when is reference by the PolicyID of the CAPL policy add to Piraeus.  

Policy 
-------

A policy encapsulates an *evaluation expression* used to make an authorization
decision, i.e., true or false. A policy is uniquely identifiable by a PolicyID,
i.e, URI.

Rule
----

A *rule* is a simple expression, i.e., a binary or unary expression, that
returns a truthful evaluation of either true or false. The *evaluates* field of
the *rule* determines the truthful evaluation of the rule. The truthful
evaluation of a *rule* is true, when the *rule* evaluates to true and the
*evaluates* field is set to true. The truthful evaluation of also true, then the
*rule* evaluates to false and the *evaluates* field is set to false. Otherwise,
the truthful evaluation of the *rule* is false. A *rule* is composed of a *match
expression* and an *operation*.

Match Expression
----------------

A *match expression* locates a claim type in an identity and binds its value to
an expression for evaluation. A match expression is composed of a *claim type*
to match in the identity and optionally a *required* field that specifies
whether the claim type is required to be matched. If the required field is
false, then if the identity does not have the claim type specified in the *match
expression*, the *rule* always returns a truthful evaluation of true. If the
*required* field is true and the identity does not have a claim type specified
in the match expression, the rules always returns a truthful evaluation of
false. If the claim type specifies matches a claim in the identity, then it is
always evaluated by the rule.

Operation
---------

An *operation* is used by a *rule* to define the operator used in an expression
and optionally a constant used in a binary expression. For example, x = y is a
simple binary expression. The *rule* would use the match expression to bind the
variable “x” and the *operation* would supply the operator “=” and the constant
“y” to the *rule*.

Logical Connectives
-------------------

A *logical connective* is used to construct a complex expression, i.e., Logical
AND or Logical OR. A *logical connective* is composed of any combination of
*rules* and/or other *logical connectives*. The truthful evaluation of a
*logical connective* is the same as a *rule*, i.e., using the *evaluates* field.

Putting It Together
-------------------
The following PowerShell sample demonstrates how to create a simple expression and add it to Piraeus.  The CAPL policy can then be referenced by its PolicyID in the metadata for a $\pi$-system for either sending or receiving an event.

```diff
+#a claim type to match in the identity 
$matchClaimType = "http://www.skunklab.io/role"

+#create a match expression of type 'Literal' to match the role claim type  
$match = New-CaplMatch -Type Literal -ClaimType $matchClaimType -Required $true

+#create an operation to check the match claim value is 'Equal' to "A"  
$operation_A = New-CaplOperation -Type Equal -Value "A"

+#create a rule to bind the match expression and operation  
$rule_A = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_A

+#define a unique identifier (as URI) for the policy  
$policyId_A = "http://www.skunklab.io/resource-a"

+#create the policy for clients in role "A"  
$policy_A = New-CaplPolicy -PolicyID $policyId_A -EvaluationExpression $rule_A

+#add the capl policy to Piraeus  
Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_A
```


