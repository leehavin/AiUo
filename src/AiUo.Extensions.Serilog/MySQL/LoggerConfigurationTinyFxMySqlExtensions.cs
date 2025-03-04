/*
using System;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.AiUoMySQL;
using AiUo.Configuration;
using AiUo.Data;
using AiUo.Extensions.Serilog.MySQL;

namespace Serilog
{
    /// <summary>
    ///     Adds the WriteTo.MySQL() extension method to <see cref="LoggerConfiguration" />.
    /// </summary>
    public static class LoggerConfigurationAiUoMySQLExtensions
    {
        /// <summary>
        ///     Adds a sink that writes log events to a MySQL database.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to MySQL database.</param>
        /// <param name="enabled">The connection string to MySQL database.</param>
        /// <param name="tableName">The name of the MySQL table to store log.</param>
        /// <param name="pkType"></param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="storeTimestampInUtc">Store timestamp in UTC format</param>
        /// <param name="batchSize">Number of log messages to be sent as batch. Supported range is between 1 and 1000</param>
        /// <param name="levelSwitch">
        /// A switch allowing the pass-through minimum level to be changed at runtime.
        /// </param>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration AiUoMySQL(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString,
            bool enabled = true,
            string tableName = "s_log",
            LogTablePKType pkType = LogTablePKType.Identity,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            bool storeTimestampInUtc = false,
            uint batchSize = 100,
            LoggingLevelSwitch levelSwitch = null)
        {
            if (loggerConfiguration == null)
                throw new ArgumentNullException(nameof(loggerConfiguration));
            // 长度少于
            if (string.IsNullOrEmpty(connectionString) || connectionString.Length < 30)
            {
                var section = ConfigUtil.GetSection<DataSection>();
                if (section == null)
                    throw new Exception("配置文件中Serilog:WriteTo:Name[AiUoMySQL]:connectionString需要Data配置节但没有配置");

                connectionString = DbConfigManager.GetConnectionStringConfig(connectionString).ConnectionString;
            }
            
            if (batchSize < 1 || batchSize > 1000)
                throw new ArgumentOutOfRangeException("[batchSize] argument must be between 1 and 1000 inclusive");

            try {
                return loggerConfiguration.Sink(
                    new AiUoMySqlSink(connectionString, enabled, tableName, pkType, storeTimestampInUtc, batchSize),
                    restrictedToMinimumLevel,
                    levelSwitch);
            }
            catch (Exception ex) {
                SelfLog.WriteLine(ex.Message);

                throw;
            }
        }
    }
}
*/