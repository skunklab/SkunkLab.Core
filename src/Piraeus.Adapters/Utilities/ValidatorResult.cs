namespace Piraeus.Adapters.Utilities
{
    public class ValidatorResult
    {
        public ValidatorResult(bool validated, string error = null)
        {
            this.Validated = validated;
            this.ErrorMessage = error;
        }   
        public bool Validated { get; internal set; }

        public string ErrorMessage { get; set; }
    }
}
