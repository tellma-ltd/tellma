using Microsoft.AspNetCore.Identity.UI.Services;

namespace BSharp.Services.Email
{
    public interface IEmailSenderFactory
    {
        IEmailSender Create();
    }
}