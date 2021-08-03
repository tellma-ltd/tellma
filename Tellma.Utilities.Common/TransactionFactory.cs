using System.Transactions;

namespace Tellma
{
    /// <summary>
    /// Contains helper methods for creating transactions with default properties.
    /// </summary>
    public class TransactionFactory
    {
        /// <summary>
        /// Creates a <see cref="TransactionScope"/> with a ReadCommitted isolation level and the given <paramref name="scopeOption"/>.
        /// </summary>
        /// <param name="scopeOption">The <see cref="TransactionScopeOption"/> of the created <see cref="TransactionScope"/>.</param>
        public static TransactionScope ReadCommitted(TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            var opt = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            return new TransactionScope(scopeOption, opt, TransactionScopeAsyncFlowOption.Enabled);
        }

        /// <summary>
        /// Creates a <see cref="TransactionScope"/> with a Serializable isolation level and the given <paramref name="scopeOption"/>.
        /// </summary>
        /// <param name="scopeOption">The <see cref="TransactionScopeOption"/> of the created <see cref="TransactionScope"/>.</param>
        public static TransactionScope Serializable(TransactionScopeOption scopeOption = TransactionScopeOption.Required)
        {
            var opt = new TransactionOptions { IsolationLevel = IsolationLevel.Serializable };
            return new TransactionScope(scopeOption, opt, TransactionScopeAsyncFlowOption.Enabled);
        }

        /// <summary>
        /// Creates a <see cref="TransactionScope"/> that suppresses any ambient <see cref="Transaction"/>s.
        /// </summary>
        public static TransactionScope Suppress()
        {
            return new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
