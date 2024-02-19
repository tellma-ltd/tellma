namespace Tellma.Integration.Zatca
{
    public class Credentials
    {
        public Credentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"'{nameof(username)}' cannot be null or whitespace.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"'{nameof(password)}' cannot be null or whitespace.", nameof(password));
            }

            Username = username;
            Password = password;
        }

        public string Username { get; }
        public string Password { get; }
    }
}