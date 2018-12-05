using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Migrations
{
    /// <summary>
    /// A <see cref="MigrationOperation"/> for creating a new user-defined table type
    /// </summary>
    public class CreateUserDefinedTableTypeOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the user defined table type.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     The schema that contains the user-defined table type, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; set; }

        /// <summary>
        ///     An ordered list of <see cref="AddColumnOperation" /> for adding columns to the user-defined table type.
        /// </summary>
        public virtual List<AddColumnOperation> Columns { get; } = new List<AddColumnOperation>();
    }
}
