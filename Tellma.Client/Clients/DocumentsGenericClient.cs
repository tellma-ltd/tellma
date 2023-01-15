using Tellma.Model.Application;

namespace Tellma.Client
{
    public class DocumentsGenericClient : FactWithIdClientBase<Document, int>
    {
        internal DocumentsGenericClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "documents";
    }
}
