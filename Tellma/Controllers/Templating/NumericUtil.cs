using System;
using System.Collections.Generic;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Contains various numeric functions that can handle numeric values passed as objects
    /// </summary>
    public static class NumericUtil
    {
        /// <summary>
        /// Divides left by right after converting both to their common numeric type. Throws a meaningful exception if right is equal to 0
        /// </summary>
        public static object Divide(object left, object right, Type commonType, ExpressionBase rightExp)
        {
            try
            {
                return commonType.Name switch
                {
                    _sbyte => Convert.ToSByte(left) / Convert.ToSByte(right),
                    _byte => Convert.ToByte(left) / Convert.ToByte(right),
                    _short => Convert.ToInt16(left) / Convert.ToInt16(right),
                    _ushort => Convert.ToUInt16(left) / Convert.ToUInt16(right),
                    _int => Convert.ToInt32(left) / Convert.ToInt32(right),
                    _uint => Convert.ToUInt32(left) / Convert.ToUInt32(right),
                    _long => Convert.ToInt64(left) / Convert.ToInt64(right),
                    _ulong => Convert.ToUInt64(left) / Convert.ToUInt64(right),
                    _float => Convert.ToSingle(left) / Convert.ToSingle(right),
                    _double => Convert.ToDouble(left) / Convert.ToDouble(right),
                    _decimal => Convert.ToDecimal(left) / Convert.ToDecimal(right),
                    _ => throw new Exception($"Unknown numeric type {commonType.Name}"),// Developer mistake
                };
            }
            catch (DivideByZeroException)
            {
                throw new TemplateException($"Operator '/' could not be applied. Operand ({rightExp.ToString()}) evaluates to 0");
            }
        }

        /// <summary>
        /// Finds left modulo right after converting both to their common numeric type. Throws a meaningful exception if right is equal to 0
        /// </summary>
        public static object Modulo(object left, object right, Type commonType, ExpressionBase rightExp)
        {
            try
            {
                return commonType.Name switch
                {
                    _sbyte => Convert.ToSByte(left) % Convert.ToSByte(right),
                    _byte => Convert.ToByte(left) % Convert.ToByte(right),
                    _short => Convert.ToInt16(left) % Convert.ToInt16(right),
                    _ushort => Convert.ToUInt16(left) % Convert.ToUInt16(right),
                    _int => Convert.ToInt32(left) % Convert.ToInt32(right),
                    _uint => Convert.ToUInt32(left) % Convert.ToUInt32(right),
                    _long => Convert.ToInt64(left) % Convert.ToInt64(right),
                    _ulong => Convert.ToUInt64(left) % Convert.ToUInt64(right),
                    _float => Convert.ToSingle(left) % Convert.ToSingle(right),
                    _double => Convert.ToDouble(left) % Convert.ToDouble(right),
                    _decimal => Convert.ToDecimal(left) % Convert.ToDecimal(right),
                    _ => throw new Exception($"Unknown numeric type {commonType.Name}"),// Developer mistake
                };
            }
            catch (DivideByZeroException)
            {
                throw new TemplateException($"Operator '%' could not be applied. Operand ({rightExp.ToString()}) evaluates to 0");
            }
        }

        /// <summary>
        /// Multiplies two numeric values after converting them to their common numeric type
        /// </summary>
        public static object Multiply(object left, object right, Type commonType)
        {
            return commonType.Name switch
            {
                _sbyte => Convert.ToSByte(left) * Convert.ToSByte(right),
                _byte => Convert.ToByte(left) * Convert.ToByte(right),
                _short => Convert.ToInt16(left) * Convert.ToInt16(right),
                _ushort => Convert.ToUInt16(left) * Convert.ToUInt16(right),
                _int => Convert.ToInt32(left) * Convert.ToInt32(right),
                _uint => Convert.ToUInt32(left) * Convert.ToUInt32(right),
                _long => Convert.ToInt64(left) * Convert.ToInt64(right),
                _ulong => Convert.ToUInt64(left) * Convert.ToUInt64(right),
                _float => Convert.ToSingle(left) * Convert.ToSingle(right),
                _double => Convert.ToDouble(left) * Convert.ToDouble(right),
                _decimal => Convert.ToDecimal(left) * Convert.ToDecimal(right),
                _ => throw new Exception($"Unknown numeric type {commonType.Name}"),// Developer mistake
            };
        }

        /// <summary>
        /// Adds two numeric values after converting them to their common numeric type
        /// </summary>
        public static object Add(object left, object right, Type commonType)
        {
            return commonType.Name switch
            {
                _sbyte => Convert.ToSByte(left) + Convert.ToSByte(right),
                _byte => Convert.ToByte(left) + Convert.ToByte(right),
                _short => Convert.ToInt16(left) + Convert.ToInt16(right),
                _ushort => Convert.ToUInt16(left) + Convert.ToUInt16(right),
                _int => Convert.ToInt32(left) + Convert.ToInt32(right),
                _uint => Convert.ToUInt32(left) + Convert.ToUInt32(right),
                _long => Convert.ToInt64(left) + Convert.ToInt64(right),
                _ulong => Convert.ToUInt64(left) + Convert.ToUInt64(right),
                _float => Convert.ToSingle(left) + Convert.ToSingle(right),
                _double => Convert.ToDouble(left) + Convert.ToDouble(right),
                _decimal => Convert.ToDecimal(left) + Convert.ToDecimal(right),
                _ => throw new Exception($"Unknown numeric type {commonType.Name}"),// Developer mistake
            };
        }

        /// <summary>
        /// Subtracts right from left after converting them to their common numeric type
        /// </summary>
        public static object Subtract(object left, object right, Type commonType)
        {
            return commonType.Name switch
            {
                _sbyte => Convert.ToSByte(left) - Convert.ToSByte(right),
                _byte => Convert.ToByte(left) - Convert.ToByte(right),
                _short => Convert.ToInt16(left) - Convert.ToInt16(right),
                _ushort => Convert.ToUInt16(left) - Convert.ToUInt16(right),
                _int => Convert.ToInt32(left) - Convert.ToInt32(right),
                _uint => Convert.ToUInt32(left) - Convert.ToUInt32(right),
                _long => Convert.ToInt64(left) - Convert.ToInt64(right),
                _ulong => Convert.ToUInt64(left) - Convert.ToUInt64(right),
                _float => Convert.ToSingle(left) - Convert.ToSingle(right),
                _double => Convert.ToDouble(left) - Convert.ToDouble(right),
                _decimal => Convert.ToDecimal(left) - Convert.ToDecimal(right),
                _ => throw new Exception($"Unknown numeric type {commonType.Name}"),// Developer mistake
            };
        }

        /// <summary>
        /// Determines the common numeric type that two numerical values must be converted to
        /// before arithmetic operators or comparisons can be applied onto these numerical values.
        /// If either value is not numeric, the returned value will be null.
        /// </summary>
        public static Type CommonNumericType(object n1, object n2)
        {
            return n1?.GetType()?.Name switch
            {
                null => n2?.GetType()?.Name switch
                {
                    null => typeof(int), // When both are null
                    _ => IsNumericType(n2) ? n2.GetType() : null,
                },
                _sbyte => n2?.GetType()?.Name switch
                {
                    null => typeof(sbyte),

                    _sbyte => typeof(int),
                    _byte => typeof(int),
                    _short => typeof(int),
                    _ushort => typeof(int),

                    _int => typeof(int),
                    _uint => typeof(long),
                    _long => typeof(long),
                    _ulong => typeof(double),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _byte => n2?.GetType()?.Name switch
                {
                    null => typeof(byte),

                    _sbyte => typeof(int),
                    _byte => typeof(int),
                    _short => typeof(int),
                    _ushort => typeof(int),

                    _int => typeof(int),
                    _uint => typeof(uint),
                    _long => typeof(long),
                    _ulong => typeof(ulong),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _short => n2?.GetType()?.Name switch
                {
                    null => typeof(short),

                    _sbyte => typeof(int),
                    _byte => typeof(int),
                    _short => typeof(int),
                    _ushort => typeof(int),

                    _int => typeof(int),
                    _uint => typeof(long),
                    _long => typeof(long),
                    _ulong => typeof(double),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _ushort => n2?.GetType()?.Name switch
                {
                    null => typeof(ushort),

                    _sbyte => typeof(int),
                    _byte => typeof(int),
                    _short => typeof(int),
                    _ushort => typeof(int),

                    _int => typeof(int),
                    _uint => typeof(uint),
                    _long => typeof(long),
                    _ulong => typeof(ulong),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _int => n2?.GetType()?.Name switch
                {
                    null => typeof(int),

                    _sbyte => typeof(int),
                    _byte => typeof(int),
                    _short => typeof(int),
                    _ushort => typeof(int),

                    _int => typeof(int),
                    _uint => typeof(long),
                    _long => typeof(long),
                    _ulong => typeof(double),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _uint => n2?.GetType()?.Name switch
                {
                    null => typeof(uint),

                    _sbyte => typeof(long),
                    _byte => typeof(uint),
                    _short => typeof(long),
                    _ushort => typeof(uint),

                    _int => typeof(long),
                    _uint => typeof(uint),
                    _long => typeof(long),
                    _ulong => typeof(ulong),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _long => n2?.GetType()?.Name switch
                {
                    null => typeof(long),

                    _sbyte => typeof(long),
                    _byte => typeof(long),
                    _short => typeof(long),
                    _ushort => typeof(long),

                    _int => typeof(long),
                    _uint => typeof(long),
                    _long => typeof(long),
                    _ulong => typeof(double),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _ulong => n2?.GetType()?.Name switch
                {
                    null => typeof(ulong),

                    _sbyte => typeof(double),
                    _byte => typeof(ulong),
                    _short => typeof(double),
                    _ushort => typeof(ulong),

                    _int => typeof(double),
                    _uint => typeof(ulong),
                    _long => typeof(double),
                    _ulong => typeof(ulong),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _float => n2?.GetType()?.Name switch
                {
                    null => typeof(float),

                    _sbyte => typeof(float),
                    _byte => typeof(float),
                    _short => typeof(float),
                    _ushort => typeof(float),

                    _int => typeof(float),
                    _uint => typeof(float),
                    _long => typeof(float),
                    _ulong => typeof(float),

                    _float => typeof(float),
                    _double => typeof(double),
                    _decimal => typeof(double),
                    _ => null,
                },
                _double => n2?.GetType()?.Name switch
                {
                    null => typeof(double),

                    _sbyte => typeof(double),
                    _byte => typeof(double),
                    _short => typeof(double),
                    _ushort => typeof(double),

                    _int => typeof(double),
                    _uint => typeof(double),
                    _long => typeof(double),
                    _ulong => typeof(double),

                    _float => typeof(double),
                    _double => typeof(double),
                    _decimal => typeof(double),
                    _ => null,
                },
                _decimal => n2?.GetType()?.Name switch
                {
                    null => typeof(decimal),

                    _sbyte => typeof(decimal),
                    _byte => typeof(decimal),
                    _short => typeof(decimal),
                    _ushort => typeof(decimal),

                    _int => typeof(decimal),
                    _uint => typeof(decimal),
                    _long => typeof(decimal),
                    _ulong => typeof(decimal),

                    _float => typeof(double),
                    _double => typeof(double),
                    _decimal => typeof(decimal),
                    _ => null,
                },
                _ => null,
            };
        }

        /// <summary>
        /// Casts the numeric value to the target type and returns it as an <see cref="IComparable"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static IComparable CastToNumeric(object value, Type targetType)
        {
            return targetType.Name switch
            {
                _sbyte => Convert.ToSByte(value),
                _byte => Convert.ToByte(value),
                _short => Convert.ToInt16(value),
                _ushort => Convert.ToUInt16(value),
                _int => Convert.ToInt32(value),
                _uint => Convert.ToUInt32(value),
                _long => Convert.ToInt64(value),
                _ulong => Convert.ToUInt64(value),
                _float => Convert.ToSingle(value),
                _double => Convert.ToDouble(value),
                _decimal => Convert.ToDecimal(value),
                _ => throw new InvalidOperationException($"Unkown numeric type {targetType.Name}") // Developer mistake
            };
        }

        /// <summary>
        /// Returns true if the given value is one of the 11 numeric types of C# (either integral or floating point).
        /// The full list is in <see cref="_numeric"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNumericType(object obj)
        {
            return obj == null || _numeric.Contains(obj.GetType().Name);
        }

        public const string _sbyte = "SByte";
        public const string _byte = "Byte";
        public const string _short = "Int16";
        public const string _ushort = "UInt16";
        public const string _int = "Int32";
        public const string _uint = "UInt32";
        public const string _long = "Int64";
        public const string _ulong = "UInt64";
        public const string _float = "Single";
        public const string _double = "Double";
        public const string _decimal = "Decimal";
        public const string _string = "String";
        public static readonly HashSet<string> _numeric = new HashSet<string> { _sbyte, _byte, _short, _ushort, _int, _uint, _long, _ulong, _float, _double, _decimal };
    }
}
