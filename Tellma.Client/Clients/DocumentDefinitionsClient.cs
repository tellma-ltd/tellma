using Tellma.Model.Application;

namespace Tellma.Client
{
    public class DocumentDefinitionsClient : CrudClientBase<DocumentDefinitionForSave, DocumentDefinition, int>
    {
        internal DocumentDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "document-definitions";
    }
}
