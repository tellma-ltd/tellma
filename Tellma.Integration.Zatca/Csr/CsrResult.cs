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
        public string PrivateKey { get; }
    }
}
