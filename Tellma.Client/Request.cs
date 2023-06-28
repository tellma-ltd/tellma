using Tellma.Model.Application;

namespace Tellma.Client
{
    /// <summary>
    /// Base class of all requests to the Tellma server.
    /// </summary>
    public class Request
    {
        private static readonly Request _default = new Request();

        internal static Request Default => _default;

        /// <summary>
        /// If set to True, the <see cref="User.LastAccess"/> property of the user is not touched.
        /// </summary>
        /// <remarks> 
        /// By default when a request is made by a user, Tellma updates that user's 
        /// <see cref="User.LastAccess"/> property in the database to the time of the request.
        /// </remarks>
        public bool IsSilent { get; set; }

        /// <summary>
        /// The request calendar. Defaults to <see cref="Calendar.GC"/>.
        /// </summary>
        public Calendar Calendar { get; set; } = Calendar.GC;
    }

    /// <summary>
    /// Base class of all requests to the Tellma server that carry arguments.
    /// </summary>
    public class Request<T> : Request
    {
        /// <summary>
        /// The arguments to send with the request.
        /// </summary>
        public T Arguments { get; set; }

        /// <summary>
        /// Implicit conversion.
        /// </summary>
        public static implicit operator Request<T>(T args) => new Request<T> { Arguments = args };
    }

    public enum Calendar
    {
        /// <summary>
        /// Gregorian.
        /// </summary>
        GC = 0,

        /// <summary>
        /// Umm Al-Qura.
        /// </summary>
        UQ = 1,

        /// <summary>
        /// Ethiopian
        /// </summary>
        ET = 2
    }
}
