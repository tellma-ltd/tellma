using BSharp.Controllers.Misc;

namespace BSharp.Controllers.DTO
{
    public class ImportArguments : ParseArguments
    {
        [ChoiceList("Insert", "Update", "Merge", "Delete")]
        public string Mode { get; set; } = "Insert"; // Default
    }
}
