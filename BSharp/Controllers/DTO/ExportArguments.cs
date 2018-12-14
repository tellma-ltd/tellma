using BSharp.Controllers.Misc;

namespace BSharp.Controllers.DTO
{
    public class ExportArguments : GetArguments
    {
        // Same parameter exists in TemplateArguments
        [ChoiceList(new object[] { FileFormats.Xlsx, FileFormats.Csv }, new string[] { FileFormats.Xlsx, FileFormats.Csv })]
        public string Format { get; set; } = FileFormats.Xlsx;
    }
}
