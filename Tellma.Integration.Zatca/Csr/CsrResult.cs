namespace Tellma.Integration.Zatca
{
    public class CsrResult
    {
        public CsrResult(string csrContent, string privateKey)
        {
            CsrContent = csrContent;
            PrivateKey = privateKey;
        }

        public string CsrContent { get; }

        /// <summary>
        /// The PEM content without the BEGIN and END parts.
        /// </summary>
        public string PrivateKey { get; }
    }
}
