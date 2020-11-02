using System;
using Tellma.Entities;

namespace Tellma.Data
{
    /// <summary>
    /// Used to update notification states in bulk
    /// </summary>
    public class IdStateErrorTimestamp: Entity
    {
        public int Id { get; set; }
        public short State { get; set; }
        public string Error { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
