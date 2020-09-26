using System.Diagnostics.CodeAnalysis;

namespace EmailService.Core.Data
{
    [ExcludeFromCodeCoverage]
    public class ConnectionStringsConfig
    {
        public string PostgreSQL { get; set; }

        public int ConnectionTimeout { get; set; }
    }
}
