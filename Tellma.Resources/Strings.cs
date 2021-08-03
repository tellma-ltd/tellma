namespace Tellma
{
    /// <summary>
    /// A dummy class to allow us to have a single shared resource file,
    /// as per the official docs https://bit.ly/2Z1fH0k.
    /// </summary>
    public class Strings
    {
        /// <summary>
        /// The UI cultures currently supported by the system.
        /// </summary>
        public static readonly string[] SUPPORTED_CULTURES = new string[] 
        {
            "en", // English
            "ar", // Arabic
            "zh", // Chinese
            "am", // Amharic
            "om", // Oromo
        };
    }
}
