using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Migrations
{
    /// <summary>
    /// An extended version of the default <see cref="SqlServerMigrationsSqlGenerator"/> 
    /// which adds functionality for creating and dropping User-Defined Table Types of SQL 
    /// server inside migration files using the same syntax as creating and dropping tables, 
    /// to use this generator, register it using <see cref="DbContextOptionsBuilder.ReplaceService{ISqlMigr, TImplementation}"/>
    /// in order to replace the default implementation of <see cref="IMigrationsSqlGenerator"/>
    /// </summary>
    public class CustomSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
        public CustomSqlServerMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            IMigrationsAnnotationProvider migrationsAnnotations) : base(dependencies, migrationsAnnotations)
        {
        }

        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            if (operation is CreateUserDefinedTableTypeOperation createUdtOperation)
            {
                GenerateCreateUdt(createUdtOperation, model, builder);
            }
            else if(operation is DropUserDefinedTableTypeOperation dropUdtOperation)
            {
                GenerateDropUdt(dropUdtOperation, builder);
            }
            else
            {
                base.Generate(operation, model, builder);
            }
        }

        private void GenerateCreateUdt(
            CreateUserDefinedTableTypeOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            builder
                .Append("CREATE TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" AS TABLE (");

            using (builder.Indent())
            {
                for (var i = 0; i < operation.Columns.Count; i++)
                {
                    var column = operation.Columns[i];
                    ColumnDefinition(column, model, builder);

                    if (i != operation.Columns.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }

                builder.AppendLine();
            }

            builder.Append(")");
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator).EndCommand();
        }

        private void GenerateDropUdt(
            DropUserDefinedTableTypeOperation operation,
            MigrationCommandListBuilder builder)
        {
            builder
                .Append("DROP TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }
    }
}

// Note: This code was inspired by: https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/operations
// Following the same patterns in the official EF Core repository on GitHub:
// https://github.com/aspnet/EntityFrameworkCore/blob/release/2.1/src/EFCore.SqlServer/Migrations/SqlServerMigrationsSqlGenerator.cs
