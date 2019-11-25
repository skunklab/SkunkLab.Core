namespace Piraeus.Configuration
{
    public abstract class WebConfig
    {
        #region Encrypted Channel
        public virtual string HttpsCertficateFilename { get; set; }

        public virtual string HttpsCertificatePassword { get; set; }

        #endregion


    }
}
