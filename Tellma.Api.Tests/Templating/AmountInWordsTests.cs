using Tellma.Api.Templating;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.Api.Tests.Templating
{
    public class AmountInWordsTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Fact]
        public void Test_GenerateInlineData()
        {
            // Just a utility to help print out the InlineData attributes of the other unit test
            foreach (var n in new[] { 101201 })
            {
                var x = ToWords(n);

                _output.WriteLine($"[InlineData({n}, \"{x}\")]");
            }
        }

        [Theory]
        [InlineData(-1, "سالب واحد")]
        [InlineData(0, "صفر")]
        [InlineData(1, "واحد")]
        [InlineData(2, "اثنان")]
        [InlineData(3, "ثلاثة")]
        [InlineData(4, "أربعة")]
        [InlineData(10, "عشرة")]
        [InlineData(11, "أحد عشر")]
        [InlineData(12, "اثنا عشر")]
        [InlineData(19, "تسعة عشر")]
        [InlineData(20, "عشرون")]
        [InlineData(21, "واحد وعشرون")]
        [InlineData(29, "تسعة وعشرون")]
        [InlineData(30, "ثلاثون")]
        [InlineData(31, "واحد وثلاثون")]
        [InlineData(39, "تسعة وثلاثون")]
        [InlineData(40, "أربعون")]
        [InlineData(41, "واحد وأربعون")]
        [InlineData(99, "تسعة وتسعون")]
        [InlineData(100, "مائة")]
        [InlineData(101, "مائة وواحد")]
        [InlineData(109, "مائة وتسعة")]
        [InlineData(110, "مائة وعشرة")]
        [InlineData(111, "مائة وأحد عشر")]
        [InlineData(112, "مائة واثنا عشر")]
        [InlineData(113, "مائة وثلاثة عشر")]
        [InlineData(114, "مائة وأربعة عشر")]
        [InlineData(119, "مائة وتسعة عشر")]
        [InlineData(120, "مائة وعشرون")]
        [InlineData(130, "مائة وثلاثون")]
        [InlineData(199, "مائة وتسعة وتسعون")]
        [InlineData(200, "مئتان")]
        [InlineData(999, "تسعمائة وتسعة وتسعون")]
        [InlineData(1000, "ألف")]
        [InlineData(1001, "ألف وواحد")]
        [InlineData(1002, "ألف واثنان")]
        [InlineData(1003, "ألف وثلاثة")]
        [InlineData(1004, "ألف وأربعة")]
        [InlineData(1100, "ألف ومائة")]
        [InlineData(1101, "ألف ومائة وواحد")]
        [InlineData(2000, "ألفان")]
        [InlineData(3000, "ثلاثة آلاف")]
        [InlineData(9999, "تسعة آلاف وتسعمائة وتسعة وتسعون")]
        [InlineData(10000, "عشرة آلاف")]
        [InlineData(10001, "عشرة آلاف وواحد")]
        [InlineData(11000, "أحد عشر ألف")]
        [InlineData(12000, "اثنا عشر ألف")]
        [InlineData(13000, "ثلاثة عشر ألف")]
        [InlineData(100000, "مائة ألف")]
        [InlineData(101000, "مائة ألف وألف")]
        [InlineData(101001, "مائة ألف وألف وواحد")]
        [InlineData(101200, "مائة ألف وألف ومئتان")]
        [InlineData(101201, "مائة ألف وألف ومئتان وواحد")]
        [InlineData(102000, "مائة واثنان ألف")]
        [InlineData(103000, "مائة وثلاثة ألف")]
        [InlineData(301000, "ثلاثمائة ألف وألف")]
        [InlineData(1000000, "مليون")]
        [InlineData(301000000, "ثلاثمائة مليون ومليون")]
        [InlineData(302000000, "ثلاثمائة واثنان مليون")]
        public void Test_Cases(int n, string expected)
        {
            var actual = ToWords(n);
            
            Assert.Equal(expected, actual);
        }

        private static string ToWords(int n)
        {
            return new AmountInWordsArabic(n,
                new CurrencyInfo("USD"),
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty).ConvertToArabic();
        }
    }
}
