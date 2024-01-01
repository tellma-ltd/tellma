namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// DTO for all the cryptographic values calculated when signing the invoice XML.
    /// </summary>
    public class SignatureInfo
    {
        public SignatureInfo(
            string signingTime,
            string certificateHash,
            string certificateIssuerName,
            string certificateSerialNumber,
            string digitalSignature,
            string signedPropertiesHash,
            string invoiceHash,
            string qrCode)
        {
            SigningTime = signingTime;
            CertificateHash = certificateHash;
            CertificateIssuerName = certificateIssuerName;
            CertificateSerialNumber = certificateSerialNumber;
            DigitalSignature = digitalSignature;
            SignedPropertiesHash = signedPropertiesHash;
            InvoiceHash = invoiceHash;
            QrCode = qrCode;
        }

        public string SigningTime { get; }
        public string CertificateHash { get; }
        public string CertificateIssuerName { get; }
        public string CertificateSerialNumber { get; }
        public string DigitalSignature { get; }
        public string SignedPropertiesHash { get; }
        public string InvoiceHash { get; }
        public string QrCode { get; }
    }
}
