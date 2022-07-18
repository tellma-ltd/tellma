using System.Collections.Generic;
using Tellma.Model.Application;
using Tellma.Model.Common;

namespace Tellma.Api.Dto
{
    public abstract class Result
    {
    }

    public class IdResult : Result
    {
        public int Id { get; set; }
    }

    public class EntitiesResult<TEntity> : Result where TEntity : Entity
    {
        public EntitiesResult(IReadOnlyList<TEntity> data, int? count)
        {
            Data = data;
            Count = count;
        }

        public EntitiesResult(IReadOnlyList<TEntity> data) : this(data, null)
        {
        }

        public IReadOnlyList<TEntity> Data { get; }
        public int? Count { get; }

        public static EntitiesResult<TEntity> Empty() => new EntitiesResult<TEntity>(null);
    }

    public class EntityResult<TEntity> : Result where TEntity : Entity
    {
        public EntityResult(TEntity entity)
        {
            Entity = entity;
        }

        public TEntity Entity { get; }

        public static EntityResult<TEntity> Empty() => new EntityResult<TEntity>(null);
    }

    public class FactResult : Result
    {
        public FactResult(IReadOnlyList<DynamicRow> data, int? count)
        {
            Data = data;
            Count = count;
        }

        public IReadOnlyList<DynamicRow> Data { get; }
        public int? Count { get; }
    }

    public class AggregateResult : Result
    {
        public AggregateResult(IReadOnlyList<DynamicRow> data, IEnumerable<DimensionAncestorsResult> ancestors)
        {
            Data = data;
            Ancestors = ancestors;
        }

        public IReadOnlyList<DynamicRow> Data { get; }
        public IEnumerable<DimensionAncestorsResult> Ancestors { get; }
    }

    public class DimensionAncestorsResult : Result
    {
        public DimensionAncestorsResult(IReadOnlyList<DynamicRow> data, int idIndex, int minIndex)
        {
            Data = data;
            IdIndex = idIndex;
            MinIndex = minIndex;
        }

        /// <summary>
        /// The id of the index, clients use this value to identify which tree dimension
        /// this represents if there were multiple of them in the same query.
        /// </summary>
        public int IdIndex { get; }

        /// <summary>
        /// Column index i from <see cref="Data"/> maps to column index i + <see cref="MinIndex"/> 
        /// in the principal result.
        /// </summary>
        public int MinIndex { get; }

        /// <summary>
        /// The dynamic rows of the dimension ancestors from the DB.
        /// </summary>
        public IReadOnlyList<DynamicRow> Data { get; }
    }

    public class FileResult : Result
    {
        public FileResult(byte[] fileBytes, string fileName)
        {
            FileBytes = fileBytes;
            FileName = fileName;
        }

        public byte[] FileBytes { get; }
        public string FileName { get; }
    }

    public class ImageResult : Result
    {
        public ImageResult(string imageId, byte[] imageBytes)
        {
            ImageId = imageId;
            ImageBytes = imageBytes;
        }

        public string ImageId { get; }
        public byte[] ImageBytes { get; }
    }

    public class StatementResult : Result
    {
        public StatementResult(
            IReadOnlyList<DetailsEntry> data,
            decimal opening,
            decimal openingQuantity,
            decimal openingMonetaryValue,
            decimal closing,
            decimal closingQuantity,
            decimal closingMonetaryValue,
            int count)
        {
            Data = data;
            Opening = opening;
            OpeningQuantity = openingQuantity;
            OpeningMonetaryValue = openingMonetaryValue;
            Closing = closing;
            ClosingQuantity = closingQuantity;
            ClosingMonetaryValue = closingMonetaryValue;
            Count = count;
        }

        public IReadOnlyList<DetailsEntry> Data { get; }
        public decimal Opening { get; }
        public decimal OpeningQuantity { get; }
        public decimal OpeningMonetaryValue { get; }
        public decimal Closing { get; }
        public decimal ClosingQuantity { get; }
        public decimal ClosingMonetaryValue { get; }
        public int Count { get; }
    }

    public class DocumentsResult : EntitiesResult<Document>
    {
        public DocumentsResult(IReadOnlyList<Document> data, IReadOnlyList<RequiredSignature> requiredSignatures, int? count) : base(data, count)
        {
            RequiredSignatures = requiredSignatures;
        }

        public IReadOnlyList<RequiredSignature> RequiredSignatures { get; }

        public static new DocumentsResult Empty() => new DocumentsResult(null, null, null);
    }

    public class DocumentResult : EntityResult<Document>
    {
        public DocumentResult(Document entity, IReadOnlyList<RequiredSignature> requiredSignatures) : base(entity)
        {
            RequiredSignatures = requiredSignatures;
        }

        public IReadOnlyList<RequiredSignature> RequiredSignatures { get; }

        public static new DocumentResult Empty() => new DocumentResult(null, null);
    }

    public class LinesResult : EntitiesResult<LineForSave>
    {
        public LinesResult(IReadOnlyList<LineForSave> lines,
            IReadOnlyList<Account> accounts,
            IReadOnlyList<Resource> resources,
            IReadOnlyList<Agent> agents,
            IReadOnlyList<EntryType> entryTypes,
            IReadOnlyList<Center> centers,
            IReadOnlyList<Currency> currencies,
            IReadOnlyList<Unit> units) : base(lines)
        {
            Accounts = accounts;
            Resources = resources;
            Agents = agents;
            EntryTypes = entryTypes;
            Centers = centers;
            Currencies = currencies;
            Units = units;
        }

        public IReadOnlyList<Account> Accounts { get; }
        public IReadOnlyList<Resource> Resources { get; }
        public IReadOnlyList<Agent> Agents { get; }
        public IReadOnlyList<EntryType> EntryTypes { get; }
        public IReadOnlyList<Center> Centers { get; }
        public IReadOnlyList<Currency> Currencies { get; }
        public IReadOnlyList<Unit> Units { get; }
    }

    public class InboxResult : EntitiesResult<InboxRecord>
    {
        public InboxResult(IReadOnlyList<InboxRecord> data, int? count, int? statusCount, int? unknownCount) : base(data, count)
        {
            StatusCount = statusCount;
            UnknownCount = unknownCount;
        }

        public int? StatusCount { get; }
        public int? UnknownCount { get; }
    }

    public class EmailResult : EntityResult<EmailForQuery>
    {
        public EmailResult(EmailForQuery entity, string body) : base(entity)
        {
            Body = body;
        }

        public string Body { get; }
    }

    public class PreviewResult : Result
    {
        public PreviewResult(string body, string downloadName)
        {
            Body = body;
            DownloadName = downloadName;
        }

        public string Body { get; }
        public string DownloadName { get; }
    }

    public class UnreconciledResult : Result
    {
        public UnreconciledResult(
            IReadOnlyList<ExternalEntry> externalEntries,
            IReadOnlyList<EntryForReconciliation> entries,
            decimal entriesBalance,
            decimal unreconciledEntriesBalance,
            decimal unreconciledExternalEntriesBalance,
            int unreconciledEntriesCount,
            int unreconciledExternalEntriesCount)
        {
            ExternalEntries = externalEntries;
            Entries = entries;
            EntriesBalance = entriesBalance;
            UnreconciledEntriesBalance = unreconciledEntriesBalance;
            UnreconciledExternalEntriesBalance = unreconciledExternalEntriesBalance;
            UnreconciledEntriesCount = unreconciledEntriesCount;
            UnreconciledExternalEntriesCount = unreconciledExternalEntriesCount;
        }

        public IReadOnlyList<ExternalEntry> ExternalEntries { get; }
        public IReadOnlyList<EntryForReconciliation> Entries { get; }
        public decimal EntriesBalance { get; }
        public decimal UnreconciledEntriesBalance { get; }
        public decimal UnreconciledExternalEntriesBalance { get; }
        public int UnreconciledEntriesCount { get; }
        public int UnreconciledExternalEntriesCount { get; }
    }

    public class ReconciledResult : Result
    {
        public ReconciledResult(IReadOnlyList<Reconciliation> reconciliations, int reconciledCount)
        {
            Reconciliations = reconciliations;
            ReconciledCount = reconciledCount;
        }

        public IReadOnlyList<Reconciliation> Reconciliations { get; }
        public int ReconciledCount { get; }
    }
}
