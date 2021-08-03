using System;
using System.Collections.Generic;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Represents a binary operator.
    /// <para/>
    /// Examples:<br/> 
    /// - not ...<br/>
    /// - - ...<br/>
    /// </summary>
    public class QueryexUnaryOperator : QueryexBase
    {
        public QueryexUnaryOperator(string op, QueryexBase operand)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        /// <summary>
        /// The unary operator.
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// The expression that is the operand onto which the operator is applied.
        /// </summary>
        public QueryexBase Operand { get; }

        public override string ToString()
        {
            return $"({Operator} {Operand})";
        }

        public override IEnumerable<QueryexBase> Children
        {
            get
            {
                yield return Operand;
            }
        }

        public override (string sql, QxType type, QxNullity nullity) CompileNative(QxCompilationContext ctx)
        {
            // Note: The way the logic is structured assumes that for ALL operators
            // if either operand is NULL, then the result is NULL

            // Convenience variables
            string operandSql;
            QxNullity operandNullity;

            // The result
            string resultSql;
            QxType resultType;
            QxNullity resultNullity;

            string opLower = Operator?.ToLower();
            switch (opLower)
            {
                case "+":
                case "-":
                    // + maye be either addition or string concatenation
                    // The output type is uniquely determined by the input types (context doesn't matter)
                    // since there is no implicit cast from numeric to string or vice versa
                    {
                        if (!Operand.TryCompile(QxType.Numeric, ctx, out operandSql, out operandNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Operand {Operand} could not be interpreted as {QxType.Numeric}.");
                        }

                        // Addition
                        resultType = QxType.Numeric;
                        resultNullity = operandNullity;

                        if (opLower == "+")
                        {
                            resultSql = operandSql; // +ve sign (does not do anything)
                        }
                        else if (opLower == "-")
                        {
                            resultSql = $"(-{operandSql})"; // -ve sign
                        }
                        else
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Unknown unary arithmetic operator {opLower}.");
                        }

                        break;
                    }

                case "!":
                case "not":
                    // These only accept booleans and return a boolean
                    {
                        resultType = QxType.Boolean;

                        if (!Operand.TryCompile(QxType.Boolean, ctx, out operandSql, out operandNullity))
                        {
                            throw new QueryException($"Operator '{Operator}': Operand {Operand} could not be interpreted as {QxType.Boolean}.");
                        }

                        resultNullity = operandNullity;
                        if (resultNullity != QxNullity.NotNull)
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"[Bug] A nullable boolean expression: {this}");
                        }

                        if (operandSql == FALSE)
                        {
                            resultSql = TRUE;
                        }
                        else if (operandSql == TRUE)
                        {
                            resultSql = FALSE;
                        }
                        else
                        {
                            resultSql = $"(NOT {operandSql})";
                        }

                        break;
                    }

                default:
                    // Developer mistake
                    throw new InvalidOperationException($"Unknown unary operator {Operator}"); // Future proofing
            }

            // Return the result (or NULL if that's the only possible value)
            if (resultNullity == QxNullity.Null)
            {
                resultSql = "NULL";
            }

            return (resultSql, resultType, resultNullity);

        }

        public override bool Equals(object exp)
        {
            return exp is QueryexUnaryOperator uo
                && StringComparer.OrdinalIgnoreCase.Equals(uo.Operator, Operator)
                && uo.Operand.Equals(Operand);
        }

        public override int GetHashCode()
        {
            int opCode = StringComparer.OrdinalIgnoreCase.GetHashCode(Operator);
            return opCode ^ Operand.GetHashCode();
        }

        public override QueryexBase Clone(string[] prefix = null) => new QueryexUnaryOperator(Operator, Operand.Clone(prefix));
    }
}
