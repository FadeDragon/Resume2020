using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Dapper;
using EmailService.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Npgsql;

namespace EmailService.Core.Data
{
    [ExcludeFromCodeCoverage]
    public class PostgreSQLDataClient : IPgDataClient
    {
        private readonly ConnectionStringsConfig config;

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        class JsonbCollectionHandler<T> : SqlMapper.TypeHandler<IEnumerable<T>>
        {
            private JsonbCollectionHandler()
            {
            }

            public static JsonbCollectionHandler<T> Instance => new JsonbCollectionHandler<T>();

            public override IEnumerable<T> Parse(object value)
            {
                var json = (string)value;

                return string.IsNullOrWhiteSpace(json) ? Enumerable.Empty<T>() : JsonConvert.DeserializeObject<IEnumerable<T>>(json, Settings);
            }

            public override void SetValue(IDbDataParameter parameter, IEnumerable<T> value)
            {
                parameter.Value = JsonConvert.SerializeObject(value, Settings);
            }
        }

        class JsonbObjectHandler<T> : SqlMapper.TypeHandler<T>
        {
            private JsonbObjectHandler()
            {
            }

            public static JsonbObjectHandler<T> Instance => new JsonbObjectHandler<T>();

            public override T Parse(object value)
            {
                var json = (string)value;

                return string.IsNullOrWhiteSpace(json) ? default(T) : JsonConvert.DeserializeObject<T>(json, Settings);
            }

            public override void SetValue(IDbDataParameter parameter, T value)
            {
                parameter.Value = JsonConvert.SerializeObject(value, Settings);
            }
        }

        static PostgreSQLDataClient()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(JsonbCollectionHandler<Recipient>.Instance);
            SqlMapper.AddTypeHandler(JsonbObjectHandler<IDictionary<string, object>>.Instance);
        }

        public PostgreSQLDataClient(ConnectionStringsConfig connConfig)
        {
            /*config = new ConnectionStringsConfig
            {
                PostgreSQL = Config.PostgresConnectionString,
                ConnectionTimeout = 15
            };*/
            config = connConfig;
            LambdaLogger.Log("connection string is " + this.config.PostgreSQL);
        }

        public async ValueTask<IEnumerable<T>> Query<T>(IDataQuery query, CancellationToken token = default)
        {
            using (var conn = new NpgsqlConnection(config.PostgreSQL))
            {
                var commandDef = new CommandDefinition(query.CmdText,
                                                       query.Parameters,
                                                       commandTimeout: config.ConnectionTimeout,
                                                       cancellationToken: token);

                return await conn.QueryAsync<T>(commandDef);
            }
        }

        public async ValueTask<T> FirstOrDefault<T>(IDataQuery query, CancellationToken token = default)
        {
            using (var conn = new NpgsqlConnection(config.PostgreSQL))
            {
                var commandDef = new CommandDefinition(query.CmdText,
                                                       query.Parameters,
                                                       commandTimeout: config.ConnectionTimeout,
                                                       cancellationToken: token);

                return await conn.QueryFirstOrDefaultAsync<T>(commandDef);
            }
        }

        public async ValueTask<int> Execute(IDataQuery query, CancellationToken token = default)
        {
            using (var conn = new NpgsqlConnection(config.PostgreSQL))
            {
                var commandDef = new CommandDefinition(query.CmdText,
                                                       query.Parameters,
                                                       commandTimeout: config.ConnectionTimeout,
                                                       cancellationToken: token);

                return await conn.ExecuteAsync(commandDef);
            }
        }

        public async ValueTask<int> Insert(IDataQuery query, object obj, CancellationToken token = default)
        {
            using (var conn = new NpgsqlConnection(config.PostgreSQL))
            {
                var commandDef = new CommandDefinition(query.CmdText,
                                                       obj,
                                                       commandTimeout: config.ConnectionTimeout,
                                                       cancellationToken: token);

                return await conn.ExecuteAsync(commandDef);
            }
        }

        public async ValueTask<T> ExecuteScalar<T>(IDataQuery query, CancellationToken token = default)
        {
            using (var conn = new NpgsqlConnection(config.PostgreSQL))
            {
                var commandDef = new CommandDefinition(query.CmdText,
                                                       query.Parameters,
                                                       commandTimeout: config.ConnectionTimeout,
                                                       cancellationToken: token);

                return await conn.ExecuteScalarAsync<T>(commandDef);
            }
        }
    }
}
