/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;

using Serilog.Core;
using Serilog.Debugging;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Sinks.Batch;
using Serilog.Sinks.Extensions;
using AiUo;
using AiUo.Data.MySql;
using AiUo.Extensions.IDGenerator;
using AiUo.Extensions.Serilog;
using AiUo.Extensions.Serilog.MySQL;
using AiUo.Text;
using MySqlDatabase = AiUo.Data.MySql.MySqlDatabase;

namespace Serilog.Sinks.AiUoMySQL
{
    internal class AiUoMySqlSink : AiUoBatchProvider, ILogEventSink
    {
        private readonly string _connectionString;
        private readonly bool _storeTimestampInUtc;
        private readonly string _tableName;
        private readonly bool _enabled;
        private LogTablePKType _pkType;

        public AiUoMySqlSink(
            string connectionString,
            bool enabled,
            string tableName,
            LogTablePKType pkType,
            bool storeTimestampInUtc = false,
            uint batchSize = 100) : base((int) batchSize)
        {
            _connectionString    = connectionString;
            _enabled = enabled;
            _tableName           = tableName;
            _pkType = pkType;
            _storeTimestampInUtc = storeTimestampInUtc;
        }

        public void Emit(LogEvent logEvent)
        {
            PushEvent(logEvent);
        }

        private MySqlConnection GetSqlConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new Exception("_connectionString不能为空");
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private MySqlCommand GetInsertCommand(MySqlConnection sqlConnection)
        {
            var tableCommandBuilder = new StringBuilder();
            tableCommandBuilder.Append($"INSERT INTO  {_tableName} (");
            if (_pkType == LogTablePKType.Identity)
            {
                tableCommandBuilder.Append("Timestamp, Level, LevelNum, Template, Message, Exception, Properties,ProjectId,Environment,MachineIP,TemplateHash) ");
                tableCommandBuilder.Append("VALUES (@Timestamp,@Level,@LevelNum, @Template, @Message, @Exception,@Properties, @ProjectId, @Environment, @MachineIP, @TemplateHash)");
            }
            else
            {
                tableCommandBuilder.Append("LogID, Timestamp, Level, LevelNum, Template, Message, Exception, Properties,ProjectId,Environment,MachineIP,TemplateHash) ");
                tableCommandBuilder.Append("VALUES (@LogID, @Timestamp,@Level,@LevelNum, @Template, @Message, @Exception,@Properties, @ProjectId, @Environment, @MachineIP, @TemplateHash)");
            }

            var cmd = sqlConnection.CreateCommand();
            cmd.CommandText = tableCommandBuilder.ToString();

            switch (_pkType)
            {
                case LogTablePKType.Guid:
                    cmd.Parameters.Add(new MySqlParameter("@LogID", MySqlDbType.VarChar));
                    break;
                case LogTablePKType.Snowflake:
                    cmd.Parameters.Add(new MySqlParameter("@LogID", MySqlDbType.Int64));
                    break;
            }
            cmd.Parameters.Add(new MySqlParameter("@Timestamp", MySqlDbType.DateTime));
            cmd.Parameters.Add(new MySqlParameter("@Level", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@LevelNum", MySqlDbType.Byte));
            cmd.Parameters.Add(new MySqlParameter("@Template", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@Message", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@Exception", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@Properties", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@ProjectId", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@Environment", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@MachineIP", MySqlDbType.VarChar));
            cmd.Parameters.Add(new MySqlParameter("@TemplateHash", MySqlDbType.Int64));

            return cmd;
        }

        protected override async Task<bool> WriteLogEventAsync(ICollection<LogEvent> logEventsBatch)
        {
            if (!_enabled) return true;
            try {
                using var sqlCon = GetSqlConnection();
                using var tr = await sqlCon.BeginTransactionAsync().ConfigureAwait(false);
                var insertCommand = GetInsertCommand(sqlCon);
                insertCommand.Transaction = tr;

                foreach (var logEvent in logEventsBatch)
                {
                    var logMessageString = new StringWriter(new StringBuilder());
                    logEvent.RenderMessage(logMessageString);

                    switch (_pkType)
                    {
                        case LogTablePKType.Guid:
                            insertCommand.Parameters["@LogID"].Value = ObjectId.NewId();
                            break;
                        case LogTablePKType.Snowflake:
                            insertCommand.Parameters["@LogID"].Value = IDGeneratorUtil.NextId();
                            break;
                    }
                    insertCommand.Parameters["@Timestamp"].Value = _storeTimestampInUtc
                        ? logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
                        : logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                    insertCommand.Parameters["@Level"].Value = logEvent.Level.ToString();
                    insertCommand.Parameters["@LevelNum"].Value = (byte)logEvent.Level;
                    insertCommand.Parameters["@Template"].Value = logEvent.MessageTemplate.ToString();
                    insertCommand.Parameters["@Message"].Value = logMessageString;
                    if (logEvent.Exception != null)
                    {
                        insertCommand.Parameters["@Exception"].Value = SerializerUtil.SerializeJsonNet(logEvent.Exception);
                    }
                    insertCommand.Parameters["@Properties"].Value = logEvent.Properties.Count > 0
                        ? logEvent.Properties.Json()
                        : null;
                    var properties = logEvent.Properties;
                    if (properties.TryGetValue(SerilogUtil.ProjectIdPropertyName, out LogEventPropertyValue value1))
                        insertCommand.Parameters["@ProjectId"].Value = value1?.ToString().Trim('"');
                    if (properties.TryGetValue(SerilogUtil.EnvironmentNamePropertyName, out LogEventPropertyValue value2))
                        insertCommand.Parameters["@Environment"].Value = value2?.ToString().Trim('"');
                    if (properties.TryGetValue(SerilogUtil.MachineIPPropertyName, out LogEventPropertyValue value3))
                        insertCommand.Parameters["@MachineIP"].Value = value3?.ToString().Trim('"');
                    //if (properties.TryGetValue(TemplateHashEnricher.PropertyName, out LogEventPropertyValue value4))
                    //    insertCommand.Parameters["@TemplateHash"].Value = value4?.ToString();
                    await insertCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                tr.Commit();

                return true;
            }
            catch (Exception ex) {
                try
                {
                    SerilogUtil.ExtLogger.Error(ex, SerilogUtil.CommonMessageTemplate);
                }
                catch{ }
                return false;
            }
        }
    }
}
*/