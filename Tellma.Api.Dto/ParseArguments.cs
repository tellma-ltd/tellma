namespace Tellma.Api.Dto
{
    public class ParseArguments
    {
        /// <summary>
        /// Determines whether the imported file is used to create new items, update existing items or both
        /// </summary>
        public string Mode { get; set; } // Default

        /// <summary>
        /// The property used as lookup key when performing an update or a merge
        /// </summary>
        public string Key { get; set; }
    }

    public static class ImportModes
    {
        /// <summary>
        /// All imported items are to be newly created
        /// </summary>
        public const string Insert = nameof(Insert);

        /// <summary>
        /// All imported items are updating existing items
        /// </summary>
        public const string Update = nameof(Update);

        /// <summary>
        /// Imported items are either creating or updating existing items
        /// </summary>
        public const string Merge = nameof(Merge);

        /// <summary>
        /// All import modes
        /// </summary>
        public static readonly string[] All = { Insert, Update, Merge };
    }
}
