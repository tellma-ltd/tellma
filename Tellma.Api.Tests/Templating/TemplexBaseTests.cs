using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Xunit;

namespace Tellma.Api.Tests
{
    public class TemplexServiceTests
    {
        [Theory(DisplayName = "Templex evaluates input to expected value ")]
        [InlineData("1", 1)]
        [InlineData("1 + 2", 3)]
        [InlineData("'Fizz' + 'Buzz'", "FizzBuzz")]
        [InlineData("true", true)]
        [InlineData("fruits#0", "Apple")]
        [InlineData("fruits#1", "Banana")]
        [InlineData("mark.Name", "Mark")]
        [InlineData("mark.Age", 25)]
        [InlineData("Fifty() / 10", 5)]
        public async Task Evaluate_EvaluatesCorrectly(string input, object expected)
        {
            // Arrange
            static int fifty() => 50;
            var fruits = new List<string> { "Apple", "Banana" };
            var mark = new TestEntity
            {
                Id = 1,
                Name = "Mark",
                Age = 25,
                EntityMetadata = {
                    [nameof(TestEntity.Name)] = FieldMetadata.Loaded,
                    [nameof(TestEntity.Age)] = FieldMetadata.Loaded 
                }
            };

            var funcs = new EvaluationContext.FunctionsDictionary
            {
                ["Fifty"] = new EvaluationFunction((_, ctx) => fifty())
            };
            var vars = new EvaluationContext.VariablesDictionary
            {
                ["fruits"] = new EvaluationVariable(fruits),
                ["mark"] = new EvaluationVariable(mark)
            };
            var ctx = EvaluationContext.Create(funcs, vars);

            // Act
            TemplexBase templex = TemplexBase.Parse(input);
            object actual = await templex.Evaluate(ctx);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Templex parses white-space input to null")]
        public void Evaluate_EmptyStringEvaluatesToNull()
        {
            // Act
            var templex = TemplexBase.Parse(" ");

            // Assert
            Assert.Null(templex);
        }
    }
}
