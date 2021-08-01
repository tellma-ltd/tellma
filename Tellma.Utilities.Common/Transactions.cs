using System.Transactions;

namespace Tellma
{
    public class Transactions
    {
        public static TransactionScope ReadCommitted(TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            var opt = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            return new TransactionScope(scopeOption, opt, TransactionScopeAsyncFlowOption.Enabled);
        }

        public static TransactionScope Serializable(TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            var opt = new TransactionOptions { IsolationLevel = IsolationLevel.Serializable };
            return new TransactionScope(scopeOption, opt, TransactionScopeAsyncFlowOption.Enabled);
        }

        public static TransactionScope Suppress()
        {
            return new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
