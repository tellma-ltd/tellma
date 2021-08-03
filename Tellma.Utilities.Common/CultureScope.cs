using System;
using System.Globalization;

namespace Tellma.Utilities.Common
{
    /// <summary>
    /// Used to temporarily change <see cref="CultureInfo.CurrentUICulture"/> in a safe and clean way.
    /// </summary>
    public class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        /// <summary>
        /// Changes the UI Culture until <see cref="Dispose"/> is invoked, Intended for use in a "using" block. 
        /// Useful when we want to change the <see cref="CultureInfo.CurrentUICulture"/> temporarily, e.g. when
        /// creating a notification email body in a different culture.
        /// </summary>
        public CultureScope(CultureInfo culture)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            // Capture the original language
            _originalCulture = CultureInfo.CurrentUICulture;

            // Change the current UI culture
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            // Change the current UI culture back the way it was
            CultureInfo.CurrentUICulture = _originalCulture;
            GC.SuppressFinalize(this); // https://bit.ly/3fwzcp6
        }
    }
}
