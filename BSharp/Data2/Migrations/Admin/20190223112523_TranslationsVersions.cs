using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BSharp.Data.Migrations.Admin
{
    public partial class TranslationsVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "Cultures",
                newName: "NeutralName");

            migrationBuilder.AddColumn<Guid>(
                name: "TranslationsVersion",
                table: "Cultures",
                nullable: false,
                defaultValue: new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd"));

            migrationBuilder.CreateTable(
                name: "GlobalSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SettingsVersion = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalSettings", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ar",
                columns: new[] { "IsActive", "NeutralName", "TranslationsVersion" },
                values: new object[] { false, "العربية", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") });

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "en",
                columns: new[] { "NeutralName", "TranslationsVersion" },
                values: new object[] { "English", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") });

            migrationBuilder.InsertData(
                table: "Cultures",
                columns: new[] { "Id", "IsActive", "Name", "NeutralName", "TranslationsVersion" },
                values: new object[,]
                {
                    { "quz", false, "Runasimi", "Runasimi", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "quc-Latn", false, "K'iche'", "K'iche'", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "quc", false, "K'iche'", "K'iche'", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "pt", false, "português", "português", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ps", false, "پښتو", "پښتو", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "prs", false, "درى", "درى", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "prg", false, "prūsiskan", "prūsiskan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "pl", false, "polski", "polski", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "pap", false, "Papiamentu", "Papiamentu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "pa-Guru", false, "ਪੰਜਾਬੀ", "ਪੰਜਾਬੀ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "pa-Arab", false, "پنجابی", "پنجابی", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "pa", false, "ਪੰਜਾਬੀ", "ਪੰਜਾਬੀ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "os", false, "ирон", "ирон", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "rm", false, "rumantsch", "rumantsch", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "rn", false, "Ikirundi", "Ikirundi", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "", false, "Invariant Language (Invariant Country)", "Invariant Language (Invariant Country)", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ro", false, "română", "română", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ses", false, "Koyraboro senni", "Koyraboro senni", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "seh", false, "sena", "sena", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "se", false, "davvisámegiella", "davvisámegiella", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sd-Deva", false, "सिन्धी", "सिन्धी", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sd-Arab", false, "سنڌي", "سنڌي", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sd", false, "سنڌي", "سنڌي", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "or", false, "ଓଡ଼ିଆ", "ଓଡ଼ିଆ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sbp", false, "Ishisangu", "Ishisangu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sah", false, "Саха", "Саха", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sa", false, "संस्कृत", "संस्कृत", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "rwk", false, "Kiruwa", "Kiruwa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "rw", false, "Kinyarwanda", "Kinyarwanda", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ru", false, "русский", "русский", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "rof", false, "Kihorombo", "Kihorombo", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "saq", false, "Kisampur", "Kisampur", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "om", false, "Oromoo", "Oromoo", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "oc", false, "Occitan", "Occitan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nyn", false, "Runyankore", "Runyankore", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mg", false, "Malagasy", "Malagasy", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mgh", false, "Makua", "Makua", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mgo", false, "metaʼ", "metaʼ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mi", false, "Reo Māori", "Reo Māori", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mk", false, "македонски", "македонски", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ml", false, "മലയാളം", "മലയാളം", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mn", false, "монгол", "монгол", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mn-Cyrl", false, "монгол", "монгол", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mn-Mong", false, "ᠮᠣᠩᠭᠣᠤᠯ ᠬᠡᠯᠡ", "ᠮᠣᠩᠭᠣᠤᠯ ᠬᠡᠯᠡ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mni", false, "মৈতৈলোন্", "মৈতৈলোন্", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "moh", false, "Kanien’kéha", "Kanien’kéha", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mr", false, "मराठी", "मराठी", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ms", false, "Melayu", "Melayu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mt", false, "Malti", "Malti", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mua", false, "MUNDAŊ", "MUNDAŊ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "my", false, "ဗမာ", "ဗမာ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mzn", false, "مازرونی", "مازرونی", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nus", false, "Thok Nath", "Thok Nath", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nso", false, "Sesotho sa Leboa", "Sesotho sa Leboa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nr", false, "isiNdebele", "isiNdebele", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nqo", false, "ߒߞߏ", "ߒߞߏ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "no", false, "norsk", "norsk", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nnh", false, "Shwóŋò ngiembɔɔn", "Shwóŋò ngiembɔɔn", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sg", false, "Sängö", "Sängö", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nn", false, "nynorsk", "nynorsk", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nl", false, "Nederlands", "Nederlands", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ne", false, "नेपाली", "नेपाली", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nds", false, "Neddersass’sch", "Neddersass’sch", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nd", false, "isiNdebele", "isiNdebele", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nb", false, "norsk bokmål", "norsk bokmål", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "naq", false, "Khoekhoegowab", "Khoekhoegowab", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "nmg", false, "Kwasio", "Kwasio", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "shi", false, "ⵜⴰⵛⵍⵃⵉⵜ", "ⵜⴰⵛⵍⵃⵉⵜ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "shi-Tfng", false, "ⵜⴰⵛⵍⵃⵉⵜ", "ⵜⴰⵛⵍⵃⵉⵜ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mfe", false, "kreol morisien", "kreol morisien", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tzm", false, "Tamaziɣt n laṭlaṣ", "Tamaziɣt n laṭlaṣ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tzm-Arab", false, "أطلس المركزية التامازيتية", "أطلس المركزية التامازيتية", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tzm-Latn", false, "Tamaziɣt n laṭlaṣ", "Tamaziɣt n laṭlaṣ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tzm-Tfng", false, "ⵜⴰⵎⴰⵣⵉⵖⵜ", "ⵜⴰⵎⴰⵣⵉⵖⵜ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ug", false, "ئۇيغۇرچە", "ئۇيغۇرچە", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "uk", false, "українська", "українська", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ur", false, "اُردو", "اُردو", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "uz", false, "o‘zbek", "o‘zbek", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "uz-Arab", false, "اوزبیک", "اوزبیک", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "uz-Cyrl", false, "ўзбекча", "ўзбекча", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "uz-Latn", false, "o‘zbek", "o‘zbek", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "vai", false, "ꕙꔤ", "ꕙꔤ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "vai-Latn", false, "Vai", "Vai", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "vai-Vaii", false, "ꕙꔤ", "ꕙꔤ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ve", false, "Tshivenḓa", "Tshivenḓa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "vi", false, "Tiếng Việt", "Tiếng Việt", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "vo", false, "Volapük", "Volapük", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "zh-Hant", false, "中文(繁體)", "中文(繁體)", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "zh-Hans", false, "中文(简体)", "中文(简体)", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "zh", false, "中文", "中文", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "zgh-Tfng", false, "ⵜⴰⵎⴰⵣⵉⵖⵜ", "ⵜⴰⵎⴰⵣⵉⵖⵜ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "zgh", false, "ⵜⴰⵎⴰⵣⵉⵖⵜ", "ⵜⴰⵎⴰⵣⵉⵖⵜ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "yo", false, "Èdè Yorùbá", "Èdè Yorùbá", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "twq", false, "Tasawaq senni", "Tasawaq senni", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "yi", false, "ייִדיש", "ייִדיש", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "xog", false, "Olusoga", "Olusoga", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "xh", false, "isiXhosa", "isiXhosa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "wo", false, "Wolof", "Wolof", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "wal", false, "ወላይታቱ", "ወላይታቱ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "wae", false, "Walser", "Walser", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "vun", false, "Kyivunjo", "Kyivunjo", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "yav", false, "nuasue", "nuasue", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "shi-Latn", false, "Tashelḥiyt", "Tashelḥiyt", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tt", false, "Татар", "Татар", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tr", false, "Türkçe", "Türkçe", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "si", false, "සිංහල", "සිංහල", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sk", false, "slovenčina", "slovenčina", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sl", false, "slovenščina", "slovenščina", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sma", false, "åarjelsaemiengïele", "åarjelsaemiengïele", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "smj", false, "julevusámegiella", "julevusámegiella", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "smn", false, "anarâškielâ", "anarâškielâ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sms", false, "sää´mǩiõll", "sää´mǩiõll", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sn", false, "chiShona", "chiShona", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sn-Latn", false, "chiShona (Latin)", "chiShona (Latin)", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "so", false, "Soomaali", "Soomaali", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sq", false, "shqip", "shqip", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sr", false, "srpski", "srpski", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sr-Cyrl", false, "српски", "српски", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sr-Latn", false, "srpski", "srpski", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ss", false, "Siswati", "Siswati", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ssy", false, "Saho", "Saho", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "st", false, "Sesotho", "Sesotho", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "to", false, "lea fakatonga", "lea fakatonga", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tn", false, "Setswana", "Setswana", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tk", false, "Türkmen dili", "Türkmen dili", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tig", false, "ትግረ", "ትግረ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ti", false, "ትግርኛ", "ትግርኛ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "th", false, "ไทย", "ไทย", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ts", false, "Xitsonga", "Xitsonga", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tg-Cyrl", false, "тоҷикӣ", "тоҷикӣ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "teo", false, "Kiteso", "Kiteso", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "te", false, "తెలుగు", "తెలుగు", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ta", false, "தமிழ்", "தமிழ்", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "syr", false, "ܣܘܪܝܝܐ", "ܣܘܪܝܝܐ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sw", false, "Kiswahili", "Kiswahili", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "sv", false, "svenska", "svenska", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "tg", false, "Тоҷикӣ", "Тоҷикӣ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mer", false, "Kĩmĩrũ", "Kĩmĩrũ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "mas", false, "Maa", "Maa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lv", false, "latviešu", "latviešu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "cs", false, "čeština", "čeština", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "cu", false, "церковнослове́нскїй", "церковнослове́нскїй", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "cy", false, "Cymraeg", "Cymraeg", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "da", false, "dansk", "dansk", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "dav", false, "Kitaita", "Kitaita", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "de", false, "Deutsch", "Deutsch", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "dje", false, "Zarmaciine", "Zarmaciine", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "dsb", false, "dolnoserbšćina", "dolnoserbšćina", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "dua", false, "duálá", "duálá", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "dv", false, "ދިވެހިބަސް", "ދިވެހިބަސް", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "dyo", false, "joola", "joola", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "dz", false, "རྫོང་ཁ", "རྫོང་ཁ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ebu", false, "Kĩembu", "Kĩembu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ee", false, "Eʋegbe", "Eʋegbe", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "co", false, "Corsu", "Corsu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "el", false, "Ελληνικά", "Ελληνικά", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "es", false, "español", "español", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "et", false, "eesti", "eesti", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "eu", false, "euskara", "euskara", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ewo", false, "ewondo", "ewondo", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "fa", false, "فارسی", "فارسی", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ff", false, "Fulah", "Fulah", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ff-Latn", false, "Fulah", "Fulah", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "fi", false, "suomi", "suomi", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "fil", false, "Filipino", "Filipino", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "fo", false, "føroyskt", "føroyskt", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "fr", false, "français", "français", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "fur", false, "furlan", "furlan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "fy", false, "Frysk", "Frysk", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ga", false, "Gaeilge", "Gaeilge", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "eo", false, "esperanto", "esperanto", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "chr-Cher", false, "ᏣᎳᎩ", "ᏣᎳᎩ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "chr", false, "ᏣᎳᎩ", "ᏣᎳᎩ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "cgg", false, "Rukiga", "Rukiga", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "aa", false, "Qafar", "Qafar", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "af", false, "Afrikaans", "Afrikaans", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "agq", false, "Aghem", "Aghem", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ak", false, "Akan", "Akan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "am", false, "አማርኛ", "አማርኛ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "arn", false, "Mapudungun", "Mapudungun", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "as", false, "অসমীয়া", "অসমীয়া", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "asa", false, "Kipare", "Kipare", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ast", false, "asturianu", "asturianu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "az", false, "azərbaycan", "azərbaycan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "az-Cyrl", false, "Азәрбајҹан дили", "Азәрбајҹан дили", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "az-Latn", false, "azərbaycan", "azərbaycan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ba", false, "Башҡорт", "Башҡорт", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bas", false, "Ɓàsàa", "Ɓàsàa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "be", false, "Беларуская", "Беларуская", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bem", false, "Ichibemba", "Ichibemba", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bez", false, "Hibena", "Hibena", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ce", false, "нохчийн", "нохчийн", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ca", false, "català", "català", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "byn", false, "ብሊን", "ብሊን", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bs-Latn", false, "bosanski", "bosanski", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bs-Cyrl", false, "босански", "босански", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bs", false, "bosanski", "bosanski", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "gd", false, "Gàidhlig", "Gàidhlig", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "brx", false, "बड़ो", "बड़ो", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bo", false, "བོད་ཡིག", "བོད་ཡིག", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bn", false, "বাংলা", "বাংলা", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bm-Latn", false, "bamanakan", "bamanakan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bm", false, "bamanakan", "bamanakan", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bin", false, "Ẹ̀dó", "Ẹ̀dó", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "bg", false, "български", "български", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "br", false, "brezhoneg", "brezhoneg", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "gl", false, "galego", "galego", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "gn", false, "Avañe’ẽ", "Avañe’ẽ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "gsw", false, "Schwiizertüütsch", "Schwiizertüütsch", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kk", false, "қазақ тілі", "қазақ тілі", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kkj", false, "kakɔ", "kakɔ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kl", false, "kalaallisut", "kalaallisut", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kln", false, "Kalenjin", "Kalenjin", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "km", false, "ភាសាខ្មែរ", "ភាសាខ្មែរ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kn", false, "ಕನ್ನಡ", "ಕನ್ನಡ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ko", false, "한국어", "한국어", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kok", false, "कोंकणी", "कोंकणी", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kr", false, "Kanuri", "Kanuri", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ks", false, "کٲشُر", "کٲشُر", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ks-Arab", false, "کٲشُر", "کٲشُر", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ks-Deva", false, "कॉशुर", "कॉशुर", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ksb", false, "Kishambaa", "Kishambaa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ksf", false, "rikpa", "rikpa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ksh", false, "Kölsch", "Kölsch", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ku", false, "کوردیی ناوەڕاست", "کوردیی ناوەڕاست", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ku-Arab", false, "کوردیی ناوەڕاست", "کوردیی ناوەڕاست", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "luy", false, "Luluhia", "Luluhia", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "luo", false, "Dholuo", "Dholuo", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lu", false, "Tshiluba", "Tshiluba", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lt", false, "lietuvių", "lietuvių", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lrc", false, "لۊری شومالی", "لۊری شومالی", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lo", false, "ລາວ", "ລາວ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ki", false, "Gikuyu", "Gikuyu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ln", false, "lingála", "lingála", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lg", false, "Luganda", "Luganda", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lb", false, "Lëtzebuergesch", "Lëtzebuergesch", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lag", false, "Kɨlaangi", "Kɨlaangi", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "la", false, "lingua latīna", "lingua latīna", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ky", false, "Кыргыз", "Кыргыз", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kw", false, "kernewek", "kernewek", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "lkt", false, "Lakȟólʼiyapi", "Lakȟólʼiyapi", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "zu", false, "isiZulu", "isiZulu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "khq", false, "Koyra ciini", "Koyra ciini", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kde", false, "Chimakonde", "Chimakonde", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "gu", false, "ગુજરાતી", "ગુજરાતી", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "guz", false, "Ekegusii", "Ekegusii", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "gv", false, "Gaelg", "Gaelg", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ha", false, "Hausa", "Hausa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ha-Latn", false, "Hausa", "Hausa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "haw", false, "ʻŌlelo Hawaiʻi", "ʻŌlelo Hawaiʻi", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "he", false, "עברית", "עברית", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "hi", false, "हिन्दी", "हिन्दी", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "hr", false, "hrvatski", "hrvatski", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "hsb", false, "hornjoserbšćina", "hornjoserbšćina", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "hu", false, "magyar", "magyar", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "hy", false, "Հայերեն", "Հայերեն", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ia", false, "interlingua", "interlingua", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ibb", false, "Ibibio-Efik", "Ibibio-Efik", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "id", false, "Indonesia", "Indonesia", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ig", false, "Igbo", "Igbo", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ii", false, "ꆈꌠꁱꂷ", "ꆈꌠꁱꂷ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kam", false, "Kikamba", "Kikamba", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kab", false, "Taqbaylit", "Taqbaylit", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ka", false, "ქართული", "ქართული", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "jv-Latn", false, "Basa Jawa", "Basa Jawa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "jv-Java", false, "ꦧꦱꦗꦮ", "ꦧꦱꦗꦮ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "jv", false, "Basa Jawa", "Basa Jawa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "kea", false, "kabuverdianu", "kabuverdianu", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "jmc", false, "Kimachame", "Kimachame", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "ja", false, "日本語", "日本語", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "iu-Latn", false, "Inuktitut", "Inuktitut", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "iu-Cans", false, "ᐃᓄᒃᑎᑐᑦ", "ᐃᓄᒃᑎᑐᑦ", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "iu", false, "Inuktitut", "Inuktitut", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "it", false, "italiano", "italiano", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "is", false, "íslenska", "íslenska", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") },
                    { "jgo", false, "Ndaꞌa", "Ndaꞌa", new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") }
                });

            migrationBuilder.InsertData(
                table: "GlobalSettings",
                columns: new[] { "Id", "SettingsVersion" },
                values: new object[] { 1, new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalSettings");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "aa");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "af");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "agq");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ak");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "am");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "arn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "as");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "asa");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ast");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "az");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "az-Cyrl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "az-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ba");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bas");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "be");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bem");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bez");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bin");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bm");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bm-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "br");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "brx");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bs");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bs-Cyrl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "bs-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "byn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ca");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ce");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cgg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "chr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "chr-Cher");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "co");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cs");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cu");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "cy");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "da");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dav");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "de");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dje");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dsb");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dua");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dv");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dyo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "dz");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ebu");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ee");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "el");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "eo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "es");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "et");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "eu");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ewo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fa");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ff");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ff-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fi");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fil");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fur");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "fy");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ga");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gd");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gsw");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gu");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "guz");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "gv");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ha");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ha-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "haw");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "he");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hi");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hsb");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hu");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "hy");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ia");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ibb");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "id");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ig");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ii");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "is");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "it");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "iu");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "iu-Cans");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "iu-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ja");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jgo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jmc");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jv");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jv-Java");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "jv-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ka");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kab");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kam");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kde");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kea");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "khq");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ki");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kk");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kkj");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kln");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "km");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ko");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kok");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ks");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ks-Arab");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ksb");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ks-Deva");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ksf");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ksh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ku");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ku-Arab");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "kw");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ky");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "la");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lag");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lb");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lkt");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ln");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lrc");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lt");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lu");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "luo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "luy");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "lv");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mas");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mer");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mfe");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mgh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mgo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mi");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mk");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ml");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mn-Cyrl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mni");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mn-Mong");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "moh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ms");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mt");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mua");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "my");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "mzn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "naq");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nb");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nd");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nds");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ne");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nmg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nnh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "no");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nqo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nso");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nus");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "nyn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "oc");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "om");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "or");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "os");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pa");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pa-Arab");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pa-Guru");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pap");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "prg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "prs");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ps");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "pt");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "quc");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "quc-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "quz");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rm");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ro");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rof");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ru");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rw");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "rwk");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sa");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sah");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "saq");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sbp");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sd");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sd-Arab");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sd-Deva");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "se");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "seh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ses");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "shi");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "shi-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "shi-Tfng");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "si");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sk");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sma");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "smj");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "smn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sms");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sn-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "so");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sq");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sr-Cyrl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sr-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ss");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ssy");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "st");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sv");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "sw");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "syr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ta");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "te");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "teo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tg");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tg-Cyrl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "th");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ti");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tig");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tk");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "to");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tr");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ts");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tt");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "twq");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm-Arab");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "tzm-Tfng");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ug");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uk");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ur");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz-Arab");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz-Cyrl");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "uz-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vai");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vai-Latn");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vai-Vaii");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ve");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vi");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "vun");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "wae");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "wal");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "wo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "xh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "xog");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "yav");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "yi");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "yo");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zgh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zgh-Tfng");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zh");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zh-Hans");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zh-Hant");

            migrationBuilder.DeleteData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "zu");

            migrationBuilder.DropColumn(
                name: "TranslationsVersion",
                table: "Cultures");

            migrationBuilder.RenameColumn(
                name: "NeutralName",
                table: "Cultures",
                newName: "Symbol");

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "ar",
                columns: new[] { "IsActive", "Symbol" },
                values: new object[] { true, "ع" });

            migrationBuilder.UpdateData(
                table: "Cultures",
                keyColumn: "Id",
                keyValue: "en",
                column: "Symbol",
                value: "En");
        }
    }
}
