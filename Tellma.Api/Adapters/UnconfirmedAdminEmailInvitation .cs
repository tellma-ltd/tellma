namespace Tellma.Api
{
    public class UnconfirmedAdminEmailInvitation : ConfirmedAdminEmailInvitation
    {
        public string EmailConfirmationLink { get; set; }
    }
}
