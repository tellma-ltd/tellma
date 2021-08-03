namespace Tellma.Api
{
    public class UserForInvitation
    {
        /// <summary>
        /// The email of the invited user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The name of the user in their preferred language.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The preferred language of the invited user.
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// The name of the inviter in the user's preferred language.
        /// </summary>
        public string InviterName { get; set; }

        /// <summary>
        /// The name of the company in the user's preferred language.
        /// </summary>
        public string CompanyName { get; set; }
    }
}
