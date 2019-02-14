using System;
using System.Management.Automation;
using Capl.Authorization;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.New, "CaplPolicy")]
    public class CaplPolicyCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Uniquely identifies the policy as a URI", Mandatory = true)]
        public string PolicyID;

        [Parameter(HelpMessage = "(Optional) Determines if the policy should use delegation.", Mandatory = false)]
        public bool Delegation;

        [Parameter(HelpMessage = "Evaluation expression (Rule, LogicalAnd, LogicalOr)", Mandatory = true)]
        public Term EvaluationExpression;

        [Parameter(HelpMessage = "(Optional) transforms", Mandatory = false)]
        public Transform[] Transforms;
        protected override void ProcessRecord()
        {
            AuthorizationPolicy policy = new AuthorizationPolicy(this.EvaluationExpression, new Uri(this.PolicyID), this.Delegation);

            if(this.Transforms != null && this.Transforms.Length > 0)
            {
                foreach(Transform transform in this.Transforms)
                {
                    policy.Transforms.Add(transform);
                }
            }

            WriteObject(policy);
        }
    }
}
