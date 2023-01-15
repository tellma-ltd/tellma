using System.Collections.Generic;

namespace Tellma.Client
{
    /// <summary>
    /// Indicates that the request record(s) could not be found.
    /// </summary>
    public class NotFoundException : TellmaException
    {
        public NotFoundException(IEnumerable<object> ids) : base("Could not find the supplied Id(s).")
        {
            Ids = ids;
        }

        public IEnumerable<object> Ids { get; }

        public override string ToString()
        {
            var stringifiedIds = string.Join(", ", Ids);

            return @$"{base.ToString()}

--- Ids ---
{stringifiedIds}";
        }
    }
}
