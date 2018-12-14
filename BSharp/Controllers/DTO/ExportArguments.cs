using BSharp.Controllers.Misc;

namespace BSharp.Controllers.DTO
{
    public class ExportArguments : GetArguments
    {
        // Same parameter exists in TemplateArguments
        [ChoiceList(FileFormats.Xlsx, FileFormats.Csv)]
        public string Format { get; set; } = FileFormats.Xlsx;
    }
}
