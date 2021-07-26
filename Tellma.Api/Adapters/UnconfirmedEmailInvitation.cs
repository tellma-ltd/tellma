namespace Tellma.Api
{
    public class UnconfirmedEmailInvitation : ConfirmedEmailInvitation
    {
        public string EmailConfirmationLink { get; set; }
    }
}
