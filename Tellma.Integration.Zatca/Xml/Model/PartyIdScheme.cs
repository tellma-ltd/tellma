namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// The scheme of <see cref="Party.Id"/>. The party can be either the seller or the buyer.
    /// </summary>
    public enum PartyIdScheme
    {
        TaxIdentificationNumber = 0,
        CommercialRegistration = 1,
        Momrah = 3,
        Mhrsd = 4,
        Number700 = 5,
        Misa = 6,
        NationalId = 7,
        GccId = 8,
        IqamaNumber = 9,
        PassportId = 10,
        OtherId = 11
    }
}
