﻿using Tellma.Model.Application;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// When the number of user assigned <see cref="Document"/>s changes, or the known number thereof.
    /// </summary>
    public class InboxStatusToSend : TenantStatusToSend
    {
        public int Count { get; set; }
        public int UnknownCount { get; set; }
        public bool UpdateInboxList { get; set; }
    }
}
