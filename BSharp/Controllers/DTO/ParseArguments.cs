using BSharp.EntityModel;

namespace BSharp.Controllers.Dto
{
    public class ParseArguments
    {
        [ChoiceList(new object[] { "Insert", "Update", "Merge" })]
        public string Mode { get; set; } = "Insert"; // Default
    }
}
