using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BSharp.Services.Migrations
{
    public static class MigrationBuilderExtensions
    {
        /// <summary>
        ///     Builds an <see cref="CreateUserDefinedTableTypeOperation" /> to create a new user-defined table type.
        /// </summary>
        /// <typeparam name="TColumns"> Type of a typically anonymous type for building columns. </typeparam>
        /// <param name="name"> The name of the user-defined table type. </param>
        /// <param name="columns">
        ///     A delegate using a <see cref="ColumnsBuilder" /> to create an anonymous type configuring the columns of the user-defined table type.
        /// </param>
        /// <param name="schema"> The schema that contains the user-defined table type, or <c>null</c> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public static MigrationBuilder CreateUserDefinedTableType<TColumns>(
            this MigrationBuilder builder,
            string name,
            Func<UserDefinedTableTypeColumnsBuilder, TColumns> columns,
            string schema = null)
        {
            var createUdtOperation = new CreateUserDefinedTableTypeOperation
            {
                Name = name,
                Schema = schema
            };

            var columnBuilder = new UserDefinedTableTypeColumnsBuilder(createUdtOperation);
            var columnsObject = columns(columnBuilder);
            var columnMap = new Dictionary<PropertyInfo, AddColumnOperation>();

            foreach (var property in typeof(TColumns).GetTypeInfo().DeclaredProperties)
            {
                var addColumnOperation = ((IInfrastructure<AddColumnOperation>)property.GetMethod.Invoke(columnsObject, null)).Instance;
                if (addColumnOperation.Name == null)
                {
                    addColumnOperation.Name = property.Name;
                }

                columnMap.Add(property, addColumnOperation);
            }

            builder.Operations.Add(createUdtOperation);

            return builder;
        }

        /// <summary>
        ///     Builds an <see cref="DropUserDefinedTableTypeOperation" /> to drop an existing user-defined table type.
        /// </summary>
        /// <param name="name"> The name of the user-defined table type to drop. </param>
        /// <param name="schema"> The schema that contains the user-defined table type, or <c>null</c> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public static MigrationBuilder DropUserDefinedTableType(
            this MigrationBuilder builder,
            string name,
            string schema = null)
        {
            builder.Operations.Add(new DropUserDefinedTableTypeOperation
            {
                Name = name,
                Schema = schema
            });

            return builder;
        }
    }
}
