using Tellma.Model.Application;

namespace Tellma.Client
{
    public class DocumentsClient : CrudClientBase<DocumentForSave, Document, int>
    {
        private readonly int _definitionId;

        internal DocumentsClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"documents/{_definitionId}";
    }
}
