using BSharp.Controllers.Misc;

namespace BSharp.Controllers.DTO
{
    public class TemplateArguments
    {
        // Same parameter exists in ExportArguments
        [ChoiceList(FileFormats.Xlsx, FileFormats.Csv)]
        public string Format { get; set; } = FileFormats.Xlsx;
    }
}
