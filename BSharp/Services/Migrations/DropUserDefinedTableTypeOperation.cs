using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Migrations
{
    /// <summary>
    /// A <see cref="MigrationOperation"/> for dropping an existing user-defined table type
    /// </summary>
    public class DropUserDefinedTableTypeOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the user defined table type.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     The schema that contains the user defined table type, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; set; }
    }
}
