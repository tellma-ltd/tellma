using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Migrations
{
    /// <summary>
    ///     A builder for <see cref="CreateUserDefinedTableTypeOperation" /> operations.
    /// </summary>
    /// <typeparam name="TColumns"> Type of a typically anonymous type for building columns. </typeparam>
    public class UserDefinedTableTypeColumnsBuilder
    {
        private readonly CreateUserDefinedTableTypeOperation _createTableOperation;

        /// <summary>
        ///     Constructs a builder for the given <see cref="CreateUserDefinedTableTypeOperation" />.
        /// </summary>
        /// <param name="createUserDefinedTableTypeOperation"> The operation. </param>
        public UserDefinedTableTypeColumnsBuilder(CreateUserDefinedTableTypeOperation createUserDefinedTableTypeOperation)
        {
            _createTableOperation = createUserDefinedTableTypeOperation ??
                throw new ArgumentNullException(nameof(createUserDefinedTableTypeOperation));
        }

        public virtual OperationBuilder<AddColumnOperation> Column<T>(
            string type = null,
            bool? unicode = null,
            int? maxLength = null,
            bool rowVersion = false,
            string name = null,
            bool nullable = false,
            object defaultValue = null,
            string defaultValueSql = null,
            string computedColumnSql = null,
            bool? fixedLength = null)
        {
            var operation = new AddColumnOperation
            {
                Schema = _createTableOperation.Schema,
                Table = _createTableOperation.Name,
                Name = name,
                ClrType = typeof(T),
                ColumnType = type,
                IsUnicode = unicode,
                MaxLength = maxLength,
                IsRowVersion = rowVersion,
                IsNullable = nullable,
                DefaultValue = defaultValue,
                DefaultValueSql = defaultValueSql,
                ComputedColumnSql = computedColumnSql,
                IsFixedLength = fixedLength
            };
            _createTableOperation.Columns.Add(operation);

            return new OperationBuilder<AddColumnOperation>(operation);
        }
    }
}
