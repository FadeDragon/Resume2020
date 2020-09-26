using System.Collections.Generic;

namespace EmailService.Core.Data
{
    public interface IDataQuery
    {
        IDictionary<string, object> Parameters { get; }

        string CmdText { get; set; }
    }
}
