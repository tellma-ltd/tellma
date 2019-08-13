using BSharp.Controllers.Misc;
using BSharp.EntityModel;

namespace BSharp.Controllers.Dto
{
    public class ExportArguments : GetArguments
    {
        // Same parameter exists in TemplateArguments
        [ChoiceList(new object[] { FileFormats.Xlsx, FileFormats.Csv })]
        public string Format { get; set; } = FileFormats.Xlsx;
    }
}
