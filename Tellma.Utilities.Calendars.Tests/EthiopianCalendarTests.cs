using System;
using System.Collections.Generic;
using Xunit;

namespace Tellma.Utilities.Calendars.Tests
{
    public class EthiopianCalendarTests
    {
        [Theory(DisplayName = "Converts Gregorian to Ethiopian dates correctly ")]
        [MemberData(nameof(Data))]
        public void ToDateTime_ResultIsCorrect(int gYear, int gMonth, int gDay, int eYear, int eMonth, int eDay)
        {
            // Arrange
            var etCalendar = new EthiopianCalendar();

            // Act
            var gDate = new DateTime(gYear, gMonth, gDay);
            var eDate = etCalendar.ToDateTime(eYear, eMonth, eDay, 0, 0, 0, 0);

            // Assert
            Assert.Equal(gDate, eDate);
        }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            // Maps GC dates to expected ET dates
            new object[] { 2021,05,28, 2013,09,20 },
            new object[] { 2020,01,01, 2012,04,22 },
            new object[] { 1990,09,21, 1983,01,11 },
            new object[] { 2035,07,01, 2027,10,24 },
        };
    }
}
