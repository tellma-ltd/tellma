namespace Tellma.Controllers.Dto
{
    public class ImportResult
    {
        public int Inserted { get; set; }

        public int Updated { get; set; }

        /// <summary>
        /// Instrumentation
        /// </summary>
        public long Milliseconds { get; set; }

        ///// <summary>
        ///// Instrumentation
        ///// </summary>
        //public decimal AttributeValidationInCSharp { get; set; }

        ///// <summary>
        ///// Instrumentation
        ///// </summary>
        //public decimal ParsingToDtosForSave { get; set; }

        ///// <summary>
        ///// Instrumentation
        ///// </summary>
        //public decimal ValidatingAndSaving { get; set; }
    }
}
