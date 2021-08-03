using Tellma.Api.ImportExport;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Packages all the dependencies of crud service base classes, so that inheriting classes
    /// do not have to list them all as constructor arguments and to simplify adding more 
    /// dependencies in the future. <br/>
    /// This is registered in the DI as scoped.
    /// </summary>
    public class CrudServiceDependencies : FactServiceDependencies
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrudServiceDependencies"/> class.
        /// </summary>
        /// <param name="deps">A <see cref="DataParser"/> for importing CSV and Excel files.</param>
        /// <param name="parser">A <see cref="DataComposer"/> for exxporting CSV files.</param>
        public CrudServiceDependencies(FactServiceDependencies deps, DataParser parser, DataComposer composer) : 
            base(deps.Localizer, deps.Metadata, deps.TemplateService, deps.ContextAccessor)
        {
            Parser = parser;
            Composer = composer;
        }

        /// <summary>
        /// A <see cref="DataParser"/> for importing CSV and Excel files.
        /// </summary>
        public DataParser Parser { get; }

        /// <summary>
        /// A <see cref="DataComposer"/> for exxporting CSV files.
        /// </summary>
        public DataComposer Composer { get; }
    }
}
