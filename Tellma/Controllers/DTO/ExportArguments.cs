using Tellma.Controllers.Utilities;
using Tellma.Entities;

namespace Tellma.Controllers.Dto
{
    public class ExportArguments : GetArguments
    {
        // Same parameter exists in TemplateArguments
        [ChoiceList(new object[] { FileFormats.Xlsx, FileFormats.Csv })]
        public string Format { get; set; } = FileFormats.Xlsx;
    }
}
