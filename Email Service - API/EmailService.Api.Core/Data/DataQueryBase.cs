using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EmailService.Core.Data
{
    [ExcludeFromCodeCoverage]
    public class DataQueryBase : IDataQuery
    {
        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public string CmdText { get; set; }
    }
}
