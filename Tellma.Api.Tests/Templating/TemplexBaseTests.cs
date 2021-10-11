using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Xunit;

namespace Tellma.Api.Tests
{
    public class TemplexTests
    {
        [Theory(DisplayName = "Templex evaluates input to expected value ")]
        [InlineData("1", 1)]
        [InlineData("1 + 2", 3)]
        [InlineData("-5", -5)]
        [InlineData("+4", 4)]
        [InlineData("'Fizz' + 'Buzz'", "FizzBuzz")]
        [InlineData("true", true)]
        [InlineData("!true", false)]
        [InlineData("fruits#0", "Apple")]
        [InlineData("fruits#1", "Banana")]
        [InlineData("person.Name", "Mark")]
        [InlineData("person.Age", 25)]
        [InlineData("Fifty() / 10", 5)]
        [InlineData("FullName('John', 'Wick')", "John Wick")]
        [InlineData("person.Name = 'Mark'", true)]
        [InlineData("person.Name != 'Mark'", false)]
        [InlineData("person.Name = 'John'", false)]
        [InlineData("person.Name != 'John'", true)]
        [InlineData("person.Age >= 18", true)]
        [InlineData("person.Age <= 18", false)]
        [InlineData("!person.Age <= 18", true)]
        [InlineData("null", null)]
        public async Task Evaluate_EvaluatesCorrectly(string input, object expected)
        {
            // Arrange
            static int Fifty() => 50;
            static string FullName(object[] args)
            {
                const int expectedCount = 2;
                if (args.Length != expectedCount)
                {
                    throw new TemplateException($"Function {nameof(FullName)} expects exactly {expectedCount} arguments.");
                }

                return $"{args[0]} {args[1]}";
            }
            var fruits = new List<string> { "Apple", "Banana" };
            var person = new TestEntity
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
                ["Fifty"] = new EvaluationFunction((_, ctx) => Fifty()),
                ["FullName"] = new EvaluationFunction((args, ctx) => FullName(args))
            };
            var vars = new EvaluationContext.VariablesDictionary
            {
                ["fruits"] = new EvaluationVariable(fruits),
                ["person"] = new EvaluationVariable(person)
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
