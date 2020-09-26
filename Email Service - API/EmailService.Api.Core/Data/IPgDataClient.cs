using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Data
{
    public interface IPgDataClient
    {
        ValueTask<IEnumerable<T>> Query<T>(IDataQuery query, CancellationToken token = default);

        ValueTask<T> FirstOrDefault<T>(IDataQuery query, CancellationToken token = default);

        ValueTask<int> Execute(IDataQuery query, CancellationToken token = default);

        ValueTask<int> Insert(IDataQuery query, object obj, CancellationToken token = default);

        ValueTask<T> ExecuteScalar<T>(IDataQuery query, CancellationToken token = default);
    }
}
