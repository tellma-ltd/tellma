﻿using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Asynchronously returns the <see cref="QueryArguments"/> needed by the <see cref="EntityQuery{T}"/>
    /// and <see cref="AggregateQuery{T}"/> to execute and load data, this allows the instantiation
    /// of the queries to be synchronous since this factory is the only constructor argument they require
    /// </summary>
    public delegate Task<QueryArguments> QueryArgumentsFactory(CancellationToken cancellation);
}
