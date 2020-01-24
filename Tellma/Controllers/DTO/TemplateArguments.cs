using Tellma.Controllers.Utilities;
using Tellma.Entities;

namespace Tellma.Controllers.Dto
{
    public class TemplateArguments
    {
        // Same parameter exists in ExportArguments
        [ChoiceList(new object[] { FileFormats.Xlsx, FileFormats.Csv })]
        public string Format { get; set; } = FileFormats.Xlsx;
    }
}
