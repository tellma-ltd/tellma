namespace Tellma.Integration.Zatca
{
    public enum VatExemptionReason
    {
        // Exempt Reasons
        VATEX_SA_29 = 1,
        VATEX_SA_29_7,
        VATEX_SA_30,

        // Zero-Rated Reasons
        VATEX_SA_32,
        VATEX_SA_33,
        VATEX_SA_34_1,
        VATEX_SA_34_2,
        VATEX_SA_34_3,
        VATEX_SA_34_4,
        VATEX_SA_34_5,
        VATEX_SA_35,
        VATEX_SA_36,
        VATEX_SA_EDU,
        VATEX_SA_HEA,
        VATEX_SA_MLTRY,

        // Not subject to VAT Reasons
        VATEX_SA_OOS,
    }
}
