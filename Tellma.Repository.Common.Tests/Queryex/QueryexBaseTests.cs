using System;
using Tellma.Repository.Common.Queryex;
using Xunit;

namespace Tellma.Repository.Common.Tests
{
    public class QueryexBaseTests
    {
        [Theory]
        [InlineData("1 + 2", typeof(QueryexBinaryOperator))]
        [InlineData("A and B", typeof(QueryexBinaryOperator))]
        [InlineData("Name contains 'e'", typeof(QueryexBinaryOperator))]
        [InlineData("null", typeof(QueryexNull))]
        [InlineData("33.4", typeof(QueryexNumber))]
        [InlineData("'Hello, World!'", typeof(QueryexQuote))]
        [InlineData("'It''s 7am in the morning.'", typeof(QueryexQuote))]
        [InlineData("sum(Value * Direction)", typeof(QueryexFunction))]
        [InlineData("today()", typeof(QueryexFunction))]
        [InlineData("Account.Name", typeof(QueryexColumnAccess))]
        [InlineData("false", typeof(QueryexBit))]
        [InlineData("true", typeof(QueryexBit))]
        [InlineData("!IsActive", typeof(QueryexUnaryOperator))]
        [InlineData("-10", typeof(QueryexUnaryOperator))]
        public void Parse_ParsesToCorrectType(string input, Type correctType)
        {
            // Act
            var asts = QueryexBase.Parse(input);

            // Assert
            var ast = Assert.Single(asts);
            Assert.IsType(correctType, ast);
        }
    }
}
