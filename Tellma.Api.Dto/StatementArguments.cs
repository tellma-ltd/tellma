using System;

namespace Tellma.Api.Dto
{
    public class StatementArguments
    {
        private const int DEFAULT_PAGE_SIZE = 60;

        public string Select { get; set; }

        public int Top { get; set; } = DEFAULT_PAGE_SIZE;

        public int Skip { get; set; } = 0;

        // Filter params

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? AccountId { get; set; }

        public int? CustodianId { get; set; }

        public int? RelationId { get; set; }

        public int? ResourceId { get; set; }

        public int? NotedRelationId { get; set; }

        public int? EntryTypeId { get; set; }

        public int? CenterId { get; set; }

        public string CurrencyId { get; set; }

        public bool? IncludeCompleted { get; set; }
    }
}
