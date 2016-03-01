using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftLife.CSharp
{
    #region UDL

    /// <summary>
    /// Class to handle UDL files.
    /// </summary>
    public static class UdlReader
    {
        private static string EMPTY_UDL = "[oledb]" + Environment.NewLine + "; Everything after this line is an OLE DB initstring" + Environment.NewLine + "Provider=SQLOLEDB.1;Persist Security Info=True" + Environment.NewLine;

        /// <summary>
        /// Tenta ler e devolver a ConnectionString guardada no ficheiro "master.udl" na pasta do executável.
        /// </summary>
        /// <returns>[0] server, [1] volume, [2] username, [3] password</returns>
        public static string[] GetConnectionDetails()
        {
            string server, volume, user, pass;
            SqlConnectionStringBuilder connectionParser = new SqlConnectionStringBuilder(GetConnectionString());

            server = connectionParser.DataSource;
            volume = connectionParser.InitialCatalog;
            user = connectionParser.UserID;
            pass = connectionParser.Password;

            return new string[] { server, volume, user, pass };
        }

        /// <summary>
        /// Tenta ler e devolver a ConnectionString guardada no ficheiro "master.udl" na pasta do executável.
        /// </summary>
        /// <returns>ConnectionString pronta a usar no construtor do SqlConnection; ou string vazia se ficheiro não tiver formato correto</returns>
        public static string GetConnectionString()
        {
            // Default: procura ficheiro "master.udl" na pasta atual
            string filePath = Directory.GetCurrentDirectory().ToString() + "\\";
            string fileName = "master.udl";

            return GetConnectionString(filePath, fileName);
        }

        /// <summary>
        /// Tenta ler e devolver a ConnectionString guardada num ficheiro.
        /// </summary>
        /// <param name="filePath">Caminho completo até à pasta que contém o ficheiro</param>
        /// <param name="fileName">Nome do ficheiro, incluindo a extensão .udl, que contém a ConnectionString</param>
        /// <returns>ConnectionString pronta a usar no construtor do SqlConnection; ou string vazia se ficheiro não tiver formato correto</returns>
        public static string GetConnectionString(string filePath, string fileName)
        {
            string fullPath = filePath + fileName;

            // Tenta encontrar ficheiro
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, EMPTY_UDL, Encoding.Unicode);   // cria ficheiro.udl default
                throw new Exception("File 'master.udl' couldn't be found. A default file was created. Please configure it and try again.");
            }
            else
            {
                // Lê ficheiro
                var udl = File.ReadAllText(fullPath, Encoding.Unicode);
                var rex = new Regex("(Provider[^;]*);(.*)", RegexOptions.Multiline);
                var match = rex.Match(udl);

                // Saca connection string
                if (match.Success) return match.Groups[2].ToString();
                else return String.Empty;
            }
        }
    }

    #endregion

    #region Query construction

    /// <summary>
    /// Base para construir comandos SQL, semelhante ao StringBuilder.
    /// </summary>
    public abstract class SqlCommandBuilder
    {
        /// <summary>
        /// Tabela onde vai ser aplicado o comando.
        /// </summary>
        public string TableName;

        internal Dictionary<string, string> _mapColumnValue;

        /// <summary>
        /// Adiciona uma coluna e valor ao comando.
        /// </summary>
        /// <param name="columnName">Nome da coluna.</param>
        /// <param name="columnValue">Valor a associar à coluna.</param>
        public void AddColumn(string columnName, string columnValue)
        {
            _mapColumnValue.Add(columnName, columnValue);
        }

        /// <summary>
        /// Constrói o comando SQL.
        /// </summary>
        /// <returns>O comando SQL</returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// Usado para construir comandos SQL de inserção, semelhante ao StringBuilder.
    /// </summary>
    public class SqlInsertBuilder : SqlCommandBuilder
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="tableName">Tabela onde vai ser feito o insert.</param>
        public SqlInsertBuilder(string tableName)
        {
            TableName = tableName;
            _mapColumnValue = new Dictionary<string, string>();
        }

        /// <summary>
        /// Constrói o comando SQL.
        /// </summary>
        /// <returns>O comando SQL de inserção.</returns>
        public override string ToString()
        {
            if (_mapColumnValue.Count == 0) return string.Empty; // fast fail

            string columns = "", values = "";
            foreach (KeyValuePair<string, string> param in _mapColumnValue)
            {
                columns += param.Key + ",";
                values += param.Value + ",";
            }
            return string.Format("INSERT INTO {0}({1}) VALUES ({2})", TableName, columns.TrimEnd(','), values.TrimEnd(','));
        }

        /// <summary>
        /// Builds the INSERT command which will return the ids specified.
        /// </summary>
        /// <param name="returnFieldName">The name of the column id.</param>
        /// <returns>The complete INSERT query.</returns>
        public string ToStringWithReturn(string returnFieldName)
        {
            if (_mapColumnValue.Count == 0) return string.Empty; // fast fail

            string columns = "", values = "";
            foreach (KeyValuePair<string, string> param in _mapColumnValue)
            {
                columns += param.Key + ",";
                values += param.Value + ",";
            }
            return string.Format("INSERT INTO {0}({1}) OUTPUT INSERTED.{2} VALUES ({3})", TableName, columns.TrimEnd(','), returnFieldName, values.TrimEnd(','));
        }
    }

    /// <summary>
    /// Usado para construir comandos SQL de atualização, semelhante ao StringBuilder.
    /// </summary>
    public class SqlUpdateBuilder : SqlCommandBuilder
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="tableName">Tabela onde vai ser feita a atualização.</param>
        public SqlUpdateBuilder(string tableName)
        {
            TableName = tableName;
            _mapColumnValue = new Dictionary<string, string>();
        }

        /// <summary>
        /// Constrói o comando SQL.
        /// </summary>
        /// <returns>O comando SQL de atualização.</returns>
        public override string ToString()
        {
            if (_mapColumnValue.Count == 0) return string.Empty; // fast fail

            string updates = "";
            foreach (KeyValuePair<string, string> param in _mapColumnValue)
            {
                updates += string.Format("{0}={1},", param.Key, param.Value);
            }
            return string.Format("UPDATE {0} SET {1}", TableName, updates.TrimEnd(','));
        }
    }

    #endregion

    #region Query execution

    /// <summary>
    /// Classe usadas para fazer (non-)queries a uma base de dados.
    /// </summary>
    public class DatabaseAdapter
    {
        #region Constants

        private const int DEFAULT_COMMAND_TIMEOUT = 30;

        #endregion

        #region Properties

        /// <summary>
        /// Ligação à base de dados onde executar as queries.
        /// </summary>
        public string SqlConnectionString;

        /// <summary>
        /// Tempo (segundos) para um comando SQL ser terminado por timeout.
        /// </summary>
        public int SqlCommandTimeout = DEFAULT_COMMAND_TIMEOUT;

        #endregion

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="sqlConnectionString">Ligação à base de dados onde executar as queries.</param>
        /// <param name="sqlCommandTimeout">Tempo (segundos) para um comando ser terminado por timeout.</param>
        public DatabaseAdapter(string sqlConnectionString, int sqlCommandTimeout = DEFAULT_COMMAND_TIMEOUT)
        {
            this.SqlConnectionString = sqlConnectionString;
            this.SqlCommandTimeout = sqlCommandTimeout;
        }

        #region Methods

        /// <summary>
        /// Executa um comando SQL que não retorna um resultado (ex. INSERT, UPDATE, SP).
        /// </summary>
        /// <param name="command">Comando SQL a executar.</param>
        public void ExecuteCommand(string command)
        {
            ExecuteCommand(command, SqlConnectionString, SqlCommandTimeout);
        }

        /// <summary>
        /// Executa um comando SQL que não retorna um resultado (ex. INSERT, UPDATE, SP).
        /// </summary>
        /// <param name="command">Comando SQL a executar.</param>
        public void ExecuteCommand(SqlCommand command)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("ExecuteCommand");

            using (SqlConnection dbConnection = new SqlConnection(SqlConnectionString))
            {
                try
                {
                    dbConnection.Open();

                    command.Connection = dbConnection;
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message + " ao executar SqlCommand: " + command.CommandText);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executa um query que devolve um resultado escalar (uma coluna, uma linha).
        /// </summary>
        /// <param name="query">Query a executar.</param>
        /// <returns>Resultado escalar (primeira coluna e primeira linha) de executar a query.</returns>
        public string GetScalar(string query)
        {
            return GetScalar(query, SqlConnectionString, SqlCommandTimeout);
        }

        /// <summary>
        /// Executa um query que devolve uma concatenação de valores separados por vírgulas (uma coluna, várias linhas).
        /// </summary>
        /// <param name="query">Query a executar.</param>
        /// <param name="quoteOutput">True se cada valor retornado pela query deve ser quoted.</param>
        /// <returns>Resultado escalar (primeira coluna) de executar a query, com os valores de todas as linhas separados por vírgulas.</returns>
        public string GetScalarList(string query, bool quoteOutput = false)
        {
            return GetScalarList(query, SqlConnectionString, quoteOutput, SqlCommandTimeout);
        }

        /// <summary>
        /// Executa um query que devolve uma tabela (várias colunas, várias linhas).
        /// </summary>
        /// <param name="query">Query a executar.</param>
        /// <returns>Resultado de executar a query em forma de tabela.</returns>
        public DataTable GetTable(string query)
        {
            return GetTable(query, SqlConnectionString, SqlCommandTimeout);
        }

        /// <summary>
        /// Executa um query que devolve uma tabela (várias colunas, várias linhas).
        /// </summary>
        /// <param name="command">Comando SQL a executar.</param>
        /// <returns>Resultado de executar a query em forma de tabela.</returns>
        public DataTable GetTable(SqlCommand command)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("GetTable");

            DataTable resultTable = new DataTable();
            using (SqlConnection dbConnection = new SqlConnection(SqlConnectionString))
            {
                try
                {
                    dbConnection.Open();
                    command.Connection = dbConnection;

                    SqlDataAdapter tableAdapter = new SqlDataAdapter();
                    tableAdapter.SelectCommand = command;
                    tableAdapter.Fill(resultTable);
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message + " ao executar Query: " + command.CommandText);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
            return resultTable;
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Executa um comando SQL que não retorna um resultado (ex. INSERT, UPDATE).
        /// </summary>
        /// <param name="query">Comando SQL a executar.</param>
        /// <param name="connectionString">Ligação à base de dados onde a query deve ser executada.</param>
        /// <param name="commandTimeout">Tempo (segundos) para um comando SQL ser terminado por timeout.</param>
        public static void ExecuteCommand(string query, string connectionString, int commandTimeout = DEFAULT_COMMAND_TIMEOUT)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("ExecuteCommand");
            //if (commandTimeout == 0) commandTimeout = DEFAULT_COMMAND_TIMEOUT;

            using (SqlConnection dbConnection = new SqlConnection(connectionString))
            {
                try
                {
                    dbConnection.Open();
                    SqlCommand command = new SqlCommand(query, dbConnection);
                    command.CommandTimeout = commandTimeout;

                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message + " ao executar NonQuery: " + query);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executa um query que devolve um resultado escalar (uma coluna, uma linha).
        /// </summary>
        /// <param name="query">Query a executar.</param>
        /// <param name="connectionString">Ligação à base de dados onde a query deve ser executada.</param>
        /// <param name="commandTimeout">Tempo (segundos) para um comando SQL ser terminado por timeout.</param>
        /// <returns>Resultado escalar (primeira coluna e primeira linha) de executar a query.</returns>
        public static string GetScalar(string query, string connectionString, int commandTimeout = DEFAULT_COMMAND_TIMEOUT)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("GetScalar");
            connectionString = (connectionString == "") ? WebConfigs.ReadConnectionString("DBConnectionString") : connectionString;  //default

            string result;
            using (SqlConnection dbConnection = new SqlConnection(connectionString))
            {
                try
                {
                    dbConnection.Open();
                    SqlCommand command = new SqlCommand(query, dbConnection);
                    command.CommandTimeout = commandTimeout;
                    object commandResult = command.ExecuteScalar();

                    if (commandResult != null)
                        result = commandResult.ToString();
                    else
                    {
                        logger.LogOnDB("WARNING Query não retornou resultados: " + query, DatabaseLogger.LogLevel.Debug);
                        result = string.Empty;
                    }
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message + " ao executar Query: " + query);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// Executa um query que devolve uma concatenação de valores separados por vírgulas (uma coluna, várias linhas).
        /// </summary>
        /// <param name="query">Query a executar.</param>
        /// <param name="connectionString">Ligação à base de dados onde a query deve ser executada.</param>
        /// <param name="quoteOutput">True se cada valor retornado pela query deve ser quoted.</param>
        /// <param name="commandTimeout">Tempo (segundos) para um comando SQL ser terminado por timeout.</param>
        /// <returns>Resultado escalar (primeira coluna) de executar a query, com os valores de todas as linhas separados por vírgulas.</returns>
        public static string GetScalarList(string query, string connectionString, bool quoteOutput = false, int commandTimeout = DEFAULT_COMMAND_TIMEOUT)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("GetScalarList");
            StringBuilder result = new StringBuilder();

            try
            {
                DataTable records = GetTable(query, connectionString, commandTimeout);
                foreach (DataRow record in records.Rows)
                {
                    string scalar = (quoteOutput) ? Utils.Quote(record[0].ToString()) : record[0].ToString();
                    result.Append(scalar + ",");
                }
            }
            catch (Exception e)
            {
                logger.LogErrorOnDB(e.Message + " ao executar Query: " + query);
                logger.LogErrorOnDB("STACK " + e.StackTrace);
                result.Clear();
            }

            return result.ToString().TrimEnd(','); // remove last comma
        }

        /// <summary>
        /// Executa um query que devolve uma tabela (várias colunas, várias linhas).
        /// </summary>
        /// <param name="query">Query a executar.</param>
        /// <param name="connectionString">Ligação à base de dados onde a query deve ser executada.</param>
        /// <param name="commandTimeout">Tempo (segundos) para um comando SQL ser terminado por timeout.</param>
        /// <returns>Resultado de executar a query em forma de tabela.</returns>
        public static DataTable GetTable(string query, string connectionString, int commandTimeout = DEFAULT_COMMAND_TIMEOUT)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("GetTable");
            DataTable resultTable = new DataTable();

            using (SqlConnection dbConnection = new SqlConnection(connectionString))
            {
                try
                {
                    dbConnection.Open();
                    SqlCommand command = new SqlCommand(query, dbConnection);
                    command.CommandTimeout = commandTimeout;

                    SqlDataAdapter tableAdapter = new SqlDataAdapter();
                    tableAdapter.SelectCommand = command;
                    tableAdapter.Fill(resultTable);
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message + " ao executar Query: " + query);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
            return resultTable;
        }

        /// <summary>
        /// Executes the specified stored procedure, which receives the parameters as input, one of which is the return parameter.
        /// Should be used to call USPs which return nothing, but have one of the parameters of the input as output.
        /// </summary>
        /// <param name="uspName"></param>
        /// <param name="connectionString"></param>
        /// <param name="parameters"></param>
        /// <param name="returnParameter"></param>
        /// <returns>Value of the output parameter.</returns>
        public static object ExecuteStoredProcedureOutput(string uspName, string connectionString, List<SqlParameter> parameters, SqlParameter returnParameter)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("ExecuteStoredProcedureOutput");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = uspName;
                        command.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter param in parameters)
                            command.Parameters.Add(param);

                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = command;
                        DataTable dt = new DataTable();

                        da.Fill(dt);
                        return returnParameter.Value;
                    }
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes the specified stored procedure, which receives the parameters as input, some of which are return parameters.
        /// Should be used to call USPs which return nothing, but have one or more of the parameters of the input as output.
        /// </summary>
        /// <param name="uspName"></param>
        /// <param name="connectionString"></param>
        /// <param name="parameters"></param>
        /// <param name="returnParameters"></param>
        /// <returns>List of the output sql parameters.</returns>
        public static List<SqlParameter> ExecuteStoredProcedureOutputList(string uspName, string connectionString, List<SqlParameter> parameters, List<SqlParameter> returnParameters)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("ExecuteStoredProcedureOutputList");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = uspName;
                        command.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter param in parameters)
                            command.Parameters.Add(param);

                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = command;
                        DataTable dt = new DataTable();

                        da.Fill(dt);
                        return returnParameters;
                    }
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
        }

        /// <summary>
        /// Executes the specified stored procedure.
        /// Should be used to call USPs which return a table (as a result of a SELECT).
        /// </summary>
        /// <param name="uspName"></param>
        /// <param name="connectionString"></param>
        /// <param name="parameters"></param>
        /// <returns>DataTable with the result of the stored procedure.</returns>
        public static DataTable ExecuteStoredProcedure(string uspName, string connectionString, List<SqlParameter> parameters)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("ExecuteStoredProcedure");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = uspName;
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null && parameters.Count > 0)
                            foreach (SqlParameter param in parameters)
                                command.Parameters.Add(param);

                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = command;
                        DataTable dt = new DataTable();

                        da.Fill(dt);
                        return dt;
                    }
                }
                catch (Exception e)
                {
                    logger.LogErrorOnDB(e.Message);
                    logger.LogErrorOnDB("STACK " + e.StackTrace);
                    throw;
                }
            }
        }

        /// <summary>
        /// This method should be used in conjuntion with the stored procedure methods above.
        /// ATTENTION: When adding a table type, size should be zero.
        /// </summary>
        /// <param name="list">The list that will receive the new parameter.</param>
        /// <param name="parameterName"></param>
        /// <param name="direction">Input/Output</param>
        /// <param name="value"></param>
        /// <param name="sqlDbType"></param>
        /// <param name="size">Typically used by nvarchars.</param>
        /// <param name="typeName">Used when the parameter is a custom table types</param>
        /// <returns>The SqlParameter inserted, specially useful when the parameter is the output parameter.</returns>
        public static SqlParameter AddSqlParameterToList(List<SqlParameter> list, string parameterName, ParameterDirection direction, object value, SqlDbType sqlDbType, int size = 0, string typeName = "")
        {
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = parameterName;
            parameter.Direction = direction;
            parameter.Value = value;
            parameter.SqlDbType = sqlDbType;
            if (size != 0)
                parameter.Size = size;
            if (!string.IsNullOrEmpty(typeName))
                parameter.TypeName = typeName;

            list.Add(parameter);

            return parameter;
        }

        #endregion
    }

    #endregion
}