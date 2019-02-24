using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Admin
{
    public partial class CultureEnglishName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "Cultures",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "",
                column: "EnglishName",
                value: "Invariant Language (Invariant Country)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "aa",
                column: "EnglishName",
                value: "Afar");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "af",
                column: "EnglishName",
                value: "Afrikaans");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "agq",
                column: "EnglishName",
                value: "Aghem");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ak",
                column: "EnglishName",
                value: "Akan");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "am",
                column: "EnglishName",
                value: "Amharic");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ar",
                column: "EnglishName",
                value: "Arabic");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "arn",
                column: "EnglishName",
                value: "Mapudungun");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "as",
                column: "EnglishName",
                value: "Assamese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "asa",
                column: "EnglishName",
                value: "Asu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ast",
                column: "EnglishName",
                value: "Asturian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "az",
                column: "EnglishName",
                value: "Azerbaijani");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "az-Cyrl",
                column: "EnglishName",
                value: "Azerbaijani (Cyrillic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "az-Latn",
                column: "EnglishName",
                value: "Azerbaijani (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ba",
                column: "EnglishName",
                value: "Bashkir");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bas",
                column: "EnglishName",
                value: "Basaa");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "be",
                column: "EnglishName",
                value: "Belarusian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bem",
                column: "EnglishName",
                value: "Bemba");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bez",
                column: "EnglishName",
                value: "Bena");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bg",
                column: "EnglishName",
                value: "Bulgarian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bin",
                column: "EnglishName",
                value: "Edo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bm",
                column: "EnglishName",
                value: "Bamanankan");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bm-Latn",
                column: "EnglishName",
                value: "Bamanankan (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bn",
                column: "EnglishName",
                value: "Bangla");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bo",
                column: "EnglishName",
                value: "Tibetan");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "br",
                column: "EnglishName",
                value: "Breton");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "brx",
                column: "EnglishName",
                value: "Bodo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bs",
                column: "EnglishName",
                value: "Bosnian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bs-Cyrl",
                column: "EnglishName",
                value: "Bosnian (Cyrillic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bs-Latn",
                column: "EnglishName",
                value: "Bosnian (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "byn",
                column: "EnglishName",
                value: "Blin");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ca",
                column: "EnglishName",
                value: "Catalan");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ce",
                column: "EnglishName",
                value: "Chechen");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cgg",
                column: "EnglishName",
                value: "Chiga");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "chr",
                column: "EnglishName",
                value: "Cherokee");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "chr-Cher",
                column: "EnglishName",
                value: "Cherokee");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "co",
                column: "EnglishName",
                value: "Corsican");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cs",
                column: "EnglishName",
                value: "Czech");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cu",
                column: "EnglishName",
                value: "Church Slavic");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cy",
                column: "EnglishName",
                value: "Welsh");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "da",
                column: "EnglishName",
                value: "Danish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dav",
                column: "EnglishName",
                value: "Taita");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "de",
                column: "EnglishName",
                value: "German");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dje",
                column: "EnglishName",
                value: "Zarma");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dsb",
                column: "EnglishName",
                value: "Lower Sorbian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dua",
                column: "EnglishName",
                value: "Duala");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dv",
                column: "EnglishName",
                value: "Divehi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dyo",
                column: "EnglishName",
                value: "Jola-Fonyi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dz",
                column: "EnglishName",
                value: "Dzongkha");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ebu",
                column: "EnglishName",
                value: "Embu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ee",
                column: "EnglishName",
                value: "Ewe");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "el",
                column: "EnglishName",
                value: "Greek");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "en",
                column: "EnglishName",
                value: "English");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "eo",
                column: "EnglishName",
                value: "Esperanto");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "es",
                column: "EnglishName",
                value: "Spanish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "et",
                column: "EnglishName",
                value: "Estonian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "eu",
                column: "EnglishName",
                value: "Basque");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ewo",
                column: "EnglishName",
                value: "Ewondo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fa",
                column: "EnglishName",
                value: "Persian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ff",
                column: "EnglishName",
                value: "Fulah");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ff-Latn",
                column: "EnglishName",
                value: "Fulah");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fi",
                column: "EnglishName",
                value: "Finnish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fil",
                column: "EnglishName",
                value: "Filipino");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fo",
                column: "EnglishName",
                value: "Faroese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fr",
                column: "EnglishName",
                value: "French");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fur",
                column: "EnglishName",
                value: "Friulian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fy",
                column: "EnglishName",
                value: "Western Frisian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ga",
                column: "EnglishName",
                value: "Irish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gd",
                column: "EnglishName",
                value: "Scottish Gaelic");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gl",
                column: "EnglishName",
                value: "Galician");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gn",
                column: "EnglishName",
                value: "Guarani");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gsw",
                column: "EnglishName",
                value: "Swiss German");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gu",
                column: "EnglishName",
                value: "Gujarati");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "guz",
                column: "EnglishName",
                value: "Gusii");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gv",
                column: "EnglishName",
                value: "Manx");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ha",
                column: "EnglishName",
                value: "Hausa");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ha-Latn",
                column: "EnglishName",
                value: "Hausa (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "haw",
                column: "EnglishName",
                value: "Hawaiian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "he",
                column: "EnglishName",
                value: "Hebrew");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hi",
                column: "EnglishName",
                value: "Hindi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hr",
                column: "EnglishName",
                value: "Croatian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hsb",
                column: "EnglishName",
                value: "Upper Sorbian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hu",
                column: "EnglishName",
                value: "Hungarian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hy",
                column: "EnglishName",
                value: "Armenian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ia",
                column: "EnglishName",
                value: "Interlingua");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ibb",
                column: "EnglishName",
                value: "Ibibio");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "id",
                column: "EnglishName",
                value: "Indonesian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ig",
                column: "EnglishName",
                value: "Igbo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ii",
                column: "EnglishName",
                value: "Yi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "is",
                column: "EnglishName",
                value: "Icelandic");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "it",
                column: "EnglishName",
                value: "Italian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "iu",
                column: "EnglishName",
                value: "Inuktitut");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "iu-Cans",
                column: "EnglishName",
                value: "Inuktitut (Syllabics)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "iu-Latn",
                column: "EnglishName",
                value: "Inuktitut (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ja",
                column: "EnglishName",
                value: "Japanese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jgo",
                column: "EnglishName",
                value: "Ngomba");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jmc",
                column: "EnglishName",
                value: "Machame");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jv",
                column: "EnglishName",
                value: "Javanese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jv-Java",
                column: "EnglishName",
                value: "Javanese (Javanese)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jv-Latn",
                column: "EnglishName",
                value: "Javanese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ka",
                column: "EnglishName",
                value: "Georgian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kab",
                column: "EnglishName",
                value: "Kabyle");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kam",
                column: "EnglishName",
                value: "Kamba");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kde",
                column: "EnglishName",
                value: "Makonde");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kea",
                column: "EnglishName",
                value: "Kabuverdianu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "khq",
                column: "EnglishName",
                value: "Koyra Chiini");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ki",
                column: "EnglishName",
                value: "Kikuyu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kk",
                column: "EnglishName",
                value: "Kazakh");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kkj",
                column: "EnglishName",
                value: "Kako");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kl",
                column: "EnglishName",
                value: "Greenlandic");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kln",
                column: "EnglishName",
                value: "Kalenjin");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "km",
                column: "EnglishName",
                value: "Khmer");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kn",
                column: "EnglishName",
                value: "Kannada");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ko",
                column: "EnglishName",
                value: "Korean");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kok",
                column: "EnglishName",
                value: "Konkani");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kr",
                column: "EnglishName",
                value: "Kanuri");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ks",
                column: "EnglishName",
                value: "Kashmiri");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ks-Arab",
                column: "EnglishName",
                value: "Kashmiri (Perso-Arabic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ksb",
                column: "EnglishName",
                value: "Shambala");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ks-Deva",
                column: "EnglishName",
                value: "Kashmiri (Devanagari)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ksf",
                column: "EnglishName",
                value: "Bafia");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ksh",
                column: "EnglishName",
                value: "Colognian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ku",
                column: "EnglishName",
                value: "Central Kurdish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ku-Arab",
                column: "EnglishName",
                value: "Central Kurdish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kw",
                column: "EnglishName",
                value: "Cornish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ky",
                column: "EnglishName",
                value: "Kyrgyz");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "la",
                column: "EnglishName",
                value: "Latin");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lag",
                column: "EnglishName",
                value: "Langi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lb",
                column: "EnglishName",
                value: "Luxembourgish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lg",
                column: "EnglishName",
                value: "Ganda");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lkt",
                column: "EnglishName",
                value: "Lakota");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ln",
                column: "EnglishName",
                value: "Lingala");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lo",
                column: "EnglishName",
                value: "Lao");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lrc",
                column: "EnglishName",
                value: "Northern Luri");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lt",
                column: "EnglishName",
                value: "Lithuanian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lu",
                column: "EnglishName",
                value: "Luba-Katanga");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "luo",
                column: "EnglishName",
                value: "Luo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "luy",
                column: "EnglishName",
                value: "Luyia");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lv",
                column: "EnglishName",
                value: "Latvian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mas",
                column: "EnglishName",
                value: "Masai");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mer",
                column: "EnglishName",
                value: "Meru");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mfe",
                column: "EnglishName",
                value: "Morisyen");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mg",
                column: "EnglishName",
                value: "Malagasy");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mgh",
                column: "EnglishName",
                value: "Makhuwa-Meetto");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mgo",
                column: "EnglishName",
                value: "Metaʼ");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mi",
                column: "EnglishName",
                value: "Maori");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mk",
                column: "EnglishName",
                value: "Macedonian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ml",
                column: "EnglishName",
                value: "Malayalam");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mn",
                column: "EnglishName",
                value: "Mongolian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mn-Cyrl",
                column: "EnglishName",
                value: "Mongolian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mni",
                column: "EnglishName",
                value: "Manipuri");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mn-Mong",
                column: "EnglishName",
                value: "Mongolian (Traditional Mongolian)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "moh",
                column: "EnglishName",
                value: "Mohawk");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mr",
                column: "EnglishName",
                value: "Marathi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ms",
                column: "EnglishName",
                value: "Malay");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mt",
                column: "EnglishName",
                value: "Maltese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mua",
                column: "EnglishName",
                value: "Mundang");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "my",
                column: "EnglishName",
                value: "Burmese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mzn",
                column: "EnglishName",
                value: "Mazanderani");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "naq",
                column: "EnglishName",
                value: "Nama");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nb",
                column: "EnglishName",
                value: "Norwegian Bokmål");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nd",
                column: "EnglishName",
                value: "North Ndebele");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nds",
                column: "EnglishName",
                value: "Low German");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ne",
                column: "EnglishName",
                value: "Nepali");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nl",
                column: "EnglishName",
                value: "Dutch");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nmg",
                column: "EnglishName",
                value: "Kwasio");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nn",
                column: "EnglishName",
                value: "Norwegian Nynorsk");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nnh",
                column: "EnglishName",
                value: "Ngiemboon");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "no",
                column: "EnglishName",
                value: "Norwegian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nqo",
                column: "EnglishName",
                value: "N'ko");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nr",
                column: "EnglishName",
                value: "South Ndebele");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nso",
                column: "EnglishName",
                value: "Sesotho sa Leboa");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nus",
                column: "EnglishName",
                value: "Nuer");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nyn",
                column: "EnglishName",
                value: "Nyankole");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "oc",
                column: "EnglishName",
                value: "Occitan");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "om",
                column: "EnglishName",
                value: "Oromo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "or",
                column: "EnglishName",
                value: "Odia");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "os",
                column: "EnglishName",
                value: "Ossetic");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pa",
                column: "EnglishName",
                value: "Punjabi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pa-Arab",
                column: "EnglishName",
                value: "Punjabi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pa-Guru",
                column: "EnglishName",
                value: "Punjabi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pap",
                column: "EnglishName",
                value: "Papiamento");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pl",
                column: "EnglishName",
                value: "Polish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "prg",
                column: "EnglishName",
                value: "Prussian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "prs",
                column: "EnglishName",
                value: "Dari");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ps",
                column: "EnglishName",
                value: "Pashto");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pt",
                column: "EnglishName",
                value: "Portuguese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "quc",
                column: "EnglishName",
                value: "K'iche'");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "quc-Latn",
                column: "EnglishName",
                value: "K'iche'");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "quz",
                column: "EnglishName",
                value: "Quechua");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rm",
                column: "EnglishName",
                value: "Romansh");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rn",
                column: "EnglishName",
                value: "Rundi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ro",
                column: "EnglishName",
                value: "Romanian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rof",
                column: "EnglishName",
                value: "Rombo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ru",
                column: "EnglishName",
                value: "Russian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rw",
                column: "EnglishName",
                value: "Kinyarwanda");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rwk",
                column: "EnglishName",
                value: "Rwa");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sa",
                column: "EnglishName",
                value: "Sanskrit");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sah",
                column: "EnglishName",
                value: "Sakha");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "saq",
                column: "EnglishName",
                value: "Samburu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sbp",
                column: "EnglishName",
                value: "Sangu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sd",
                column: "EnglishName",
                value: "Sindhi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sd-Arab",
                column: "EnglishName",
                value: "Sindhi");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sd-Deva",
                column: "EnglishName",
                value: "Sindhi (Devanagari)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "se",
                column: "EnglishName",
                value: "Northern Sami");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "seh",
                column: "EnglishName",
                value: "Sena");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ses",
                column: "EnglishName",
                value: "Koyraboro Senni");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sg",
                column: "EnglishName",
                value: "Sango");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "shi",
                column: "EnglishName",
                value: "Tachelhit");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "shi-Latn",
                column: "EnglishName",
                value: "Tachelhit (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "shi-Tfng",
                column: "EnglishName",
                value: "Tachelhit (Tifinagh)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "si",
                column: "EnglishName",
                value: "Sinhala");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sk",
                column: "EnglishName",
                value: "Slovak");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sl",
                column: "EnglishName",
                value: "Slovenian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sma",
                column: "EnglishName",
                value: "Sami (Southern)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "smj",
                column: "EnglishName",
                value: "Sami (Lule)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "smn",
                column: "EnglishName",
                value: "Sami (Inari)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sms",
                column: "EnglishName",
                value: "Sami (Skolt)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sn",
                column: "EnglishName",
                value: "Shona");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sn-Latn",
                column: "EnglishName",
                value: "Shona (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "so",
                column: "EnglishName",
                value: "Somali");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sq",
                column: "EnglishName",
                value: "Albanian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sr",
                column: "EnglishName",
                value: "Serbian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sr-Cyrl",
                column: "EnglishName",
                value: "Serbian (Cyrillic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sr-Latn",
                column: "EnglishName",
                value: "Serbian (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ss",
                column: "EnglishName",
                value: "siSwati");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ssy",
                column: "EnglishName",
                value: "Saho");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "st",
                column: "EnglishName",
                value: "Sesotho");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sv",
                column: "EnglishName",
                value: "Swedish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sw",
                column: "EnglishName",
                value: "Kiswahili");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "syr",
                column: "EnglishName",
                value: "Syriac");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ta",
                column: "EnglishName",
                value: "Tamil");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "te",
                column: "EnglishName",
                value: "Telugu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "teo",
                column: "EnglishName",
                value: "Teso");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tg",
                column: "EnglishName",
                value: "Tajik");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tg-Cyrl",
                column: "EnglishName",
                value: "Tajik (Cyrillic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "th",
                column: "EnglishName",
                value: "Thai");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ti",
                column: "EnglishName",
                value: "Tigrinya");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tig",
                column: "EnglishName",
                value: "Tigre");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tk",
                column: "EnglishName",
                value: "Turkmen");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tn",
                column: "EnglishName",
                value: "Setswana");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "to",
                column: "EnglishName",
                value: "Tongan");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tr",
                column: "EnglishName",
                value: "Turkish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ts",
                column: "EnglishName",
                value: "Tsonga");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tt",
                column: "EnglishName",
                value: "Tatar");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "twq",
                column: "EnglishName",
                value: "Tasawaq");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm",
                column: "EnglishName",
                value: "Central Atlas Tamazight");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm-Arab",
                column: "EnglishName",
                value: "Central Atlas Tamazight (Arabic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm-Latn",
                column: "EnglishName",
                value: "Central Atlas Tamazight (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm-Tfng",
                column: "EnglishName",
                value: "Central Atlas Tamazight (Tifinagh)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ug",
                column: "EnglishName",
                value: "Uyghur");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uk",
                column: "EnglishName",
                value: "Ukrainian");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ur",
                column: "EnglishName",
                value: "Urdu");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz",
                column: "EnglishName",
                value: "Uzbek");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz-Arab",
                column: "EnglishName",
                value: "Uzbek (Perso-Arabic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz-Cyrl",
                column: "EnglishName",
                value: "Uzbek (Cyrillic)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz-Latn",
                column: "EnglishName",
                value: "Uzbek (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vai",
                column: "EnglishName",
                value: "Vai");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vai-Latn",
                column: "EnglishName",
                value: "Vai (Latin)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vai-Vaii",
                column: "EnglishName",
                value: "Vai (Vai)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ve",
                column: "EnglishName",
                value: "Venda");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vi",
                column: "EnglishName",
                value: "Vietnamese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vo",
                column: "EnglishName",
                value: "Volapük");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vun",
                column: "EnglishName",
                value: "Vunjo");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "wae",
                column: "EnglishName",
                value: "Walser");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "wal",
                column: "EnglishName",
                value: "Wolaytta");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "wo",
                column: "EnglishName",
                value: "Wolof");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "xh",
                column: "EnglishName",
                value: "isiXhosa");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "xog",
                column: "EnglishName",
                value: "Soga");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "yav",
                column: "EnglishName",
                value: "Yangben");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "yi",
                column: "EnglishName",
                value: "Yiddish");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "yo",
                column: "EnglishName",
                value: "Yoruba");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zgh",
                column: "EnglishName",
                value: "Standard Moroccan Tamazight");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zgh-Tfng",
                column: "EnglishName",
                value: "Standard Moroccan Tamazight (Tifinagh)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zh",
                column: "EnglishName",
                value: "Chinese");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zh-Hans",
                column: "EnglishName",
                value: "Chinese (Simplified)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zh-Hant",
                column: "EnglishName",
                value: "Chinese (Traditional)");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zu",
                column: "EnglishName",
                value: "isiZulu");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "Cultures");
        }
    }
}
