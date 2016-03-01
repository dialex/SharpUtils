using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace SoftLife.CSharp
{
    /// <summary>
    /// Classe utilitária que faz log num ficheiro.
    /// </summary>
    public class FileLogger
    {
        private static string _filename = "log";
        private static string _extension = "txt";
        private static string Filepath { get { return string.Format("{0}\\{1}.{2}", Directory.GetCurrentDirectory(), _filename, _extension); } }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        public FileLogger(string filename, string extension)
        {
            _filename = filename;
            _extension = extension.Replace(".","");
        }

        /// <summary>
        /// Escreve uma mensagem num ficheiro. Acrescenta uma mudança de linha no final.
        /// </summary>
        /// <param name="message"></param>
        public void LogLine(string message)
        {
            LogLine(message, _filename, _extension);
        }

        /// <summary>
        /// Escreve uma mensagem num ficheiro. Acrescenta uma mudança de linha no final.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        public static void LogLine(string message, string filename = "log", string extension = "txt")
        {
            Log(message + Environment.NewLine, filename, extension);
        }

        /// <summary>
        /// Escreve uma mensagem num ficheiro.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            Log(message, _filename, _extension);
        }

        /// <summary>
        /// Escreve uma mensagem num ficheiro.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        public static void Log(string message, string filename = "log", string extension = "txt")
        {
            string prefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | ";
            File.AppendAllText(Filepath, prefix + message);
        }
    }

    /// <summary>
    /// Classe utilitária que faz log na consola do Windows.
    /// </summary>
    public static class ConsoleLogger
    {
        /// <summary>
        /// Imprime uma mensagem na consola. Acrescenta uma mudança de linha no final.
        /// </summary>
        /// <param name="message"></param>
        public static void LogLine(string message)
        {
            string prefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ";
            Console.WriteLine(prefix + message);
        }

        /// <summary>
        /// Imprime uma mensagem na consola.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            string prefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ";
            Console.Write(prefix + message);
        }
    }

    /// <summary>
    /// Classe que permite fazer Log para uma base de dados. Deve existir um objeto global à aplicação (GetMasterInstance), configurado devidamente.
    /// Para cada método que pertende fazer Log na base de dados deve pedir à MasterInstance uma nova instância (GetInstance).
    /// </summary>
    public class DatabaseLogger
    {
        /// <summary>
        /// Níveis de debug. Controla a quantidade de mensagens que são escritas.
        /// </summary>
        public enum LogLevel
        {
            /// <summary>Nível de uma msg que deve ser sempre mostrada</summary>
            None = 0,
            /// <summary>Nível de uma msg útil na manutenção do sistema</summary>
            Maintenance = 1,
            /// <summary>Nível de uma msg útil no debug do sistema</summary>
            Debug = 2,
            /// <summary>Nível de uma msg útil no debug desesperado do sistema</summary>
            Maximum = 3
        }
        /// <summary>
        /// Tamanho máximo (em caracteres) das entradas no log
        /// </summary>
        public const int MAXIMUM_ENTRY_LENGTH = 2500;

        // Campos globais a todas as instâncias
        public static LogLevel MaximumLevel { get; set; }
        public static string DBConnectionString { get; private set; }
        private static string QueryThatLogsOnDB;
        private static bool LogMethodCall;
        private static bool LogThreadId;

        // Campos específicos de cada instância
        public string Prefix { get; set; }

        #region Construtores

        /// <summary>
        /// Construtor base vazio.
        /// </summary>
        private DatabaseLogger() { }

        /// <summary>
        /// ATENÇÃO: deve ser usado apenas como um Setter aos atributos static (globais a todos os Loggers).
        /// </summary>
        /// <param name="maxLevel">Faz log de todas as mensagens com este nível ou inferior.</param>
        /// <param name="connectionString">Ligação à BD onde vai ser escrito o log</param>
        /// <param name="queryToLogToDatabase">Query de escrita na BD a ser usado pelo String.Format, usar {0} no local da mensagem. Ex: "INSERT INTO NomeTabela (Data, Output) VALUES (GETDATE(), {0})"</param>
        /// <param name="logMethodCall">Se True escreve no log o 'methodName' sempre que é invocado o GetInstance(methodName)</param>
        /// <param name="logThreadId">Se True escreve no log o identificador da thread atual</param>
        private DatabaseLogger(LogLevel maxLevel, string connectionString, string queryToLogToDatabase, bool logMethodCall, bool logThreadId)
        {
            MaximumLevel = maxLevel;
            DBConnectionString = connectionString;
            QueryThatLogsOnDB = queryToLogToDatabase;
            LogMethodCall = logMethodCall;
            LogThreadId = logThreadId;
        }

        /// <summary>
        /// Este é o construtor do Logger da aplicação (global), que define os atributos globais a todas as instâncias.
        /// </summary>
        /// <param name="configDebugLevel">Nome (string) de um nível da enumeração LogLevel. Faz log de todas as mensagens com este nível ou inferior.</param>
        /// <param name="connectionString">Ligação à BD onde vai ser escrito o log</param>
        /// <param name="queryToInsertLogOnDatabase">Query de escrita na BD a ser usado pelo String.Format, usar {0} no local da mensagem. Ex: "INSERT INTO NomeTabela (Data, Output) VALUES (GETDATE(), {0})</param>
        /// <param name="logMethodCall">Se True escreve no log o 'methodName' sempre que é invocado o GetInstance(methodName)</param>
        /// <param name="logThreadId">Se True escreve no log o identificador da thread atual</param>
        /// <returns></returns>
        public static DatabaseLogger GetMasterInstance(string configDebugLevel, string connectionString, string queryToInsertLogOnDatabase, string logMethodCall = "False", string logThreadId = "False")
        {
            LogLevel debugLevel;
            switch (configDebugLevel)
            {
                case "None": debugLevel = LogLevel.None; break;
                case "Maintenance": debugLevel = LogLevel.Maintenance; break;
                case "Debug": debugLevel = LogLevel.Debug; break;
                case "Maximum": debugLevel = LogLevel.Maximum; break;
                default: debugLevel = LogLevel.None; break;
            }
            return new DatabaseLogger(debugLevel, connectionString, queryToInsertLogOnDatabase, bool.Parse(logMethodCall), bool.Parse(logThreadId));
        }

        /// <summary>
        /// Este é o construtor do Logger de método (particular), que partilha os valores dos campos globais, mas que pode ser personalizado.
        /// </summary>
        /// <param name="methodName">Nome do método será usado como prefixo das suas mensagens</param>
        public static DatabaseLogger GetInstance(string methodName = "")
        {
            DatabaseLogger freshInstance = new DatabaseLogger();

            if (LogThreadId)
                freshInstance.Prefix = methodName + string.Format(" ({0}) | ", Thread.CurrentThread.ManagedThreadId);
            else
                freshInstance.Prefix = methodName + " | ";

            // Porque GetInstance é chamado no início de cada método, pode servir para fazer log de que o método foi chamado
            if (LogMethodCall) freshInstance.LogOnDB(methodName, LogLevel.Maximum);

            return freshInstance;
        }

        #endregion

        /// <summary>
        /// Faz log de mensagem com prefixo de erro. Não tem nível, é sempre feito log.
        /// </summary>
        /// <param name="msg">Mensagem de erro</param>
        public void LogErrorOnDB(string msg)
        {
            LogOnDB("ERROR " + msg);
        }

        /// <summary>
        /// Faz log da exceção. Se em nível máximo, faz log da call stack. Não tem nível, é sempre feito log.
        /// </summary>
        /// <param name="error">Exceção que causou o erro</param>
        public void LogErrorOnDB(Exception error)
        {
            LogErrorOnDB(error.Message);
            if (MaximumLevel == LogLevel.Maximum)
            {
                LogErrorOnDB("STACK " + error.StackTrace);
            }
        }

        /// <summary>
        /// Faz log de mensagem com prefixo de output da função.
        /// </summary>
        /// <param name="msg">Valor do retorno da função</param>
        /// <param name="level">Nível de detalhe da mensagem. Se não for passado, escreve sempre no log.</param>
        public void LogOutputOnDB(string msg, LogLevel level = LogLevel.Maximum)
        {
            LogOnDB("OUTPUT: " + msg, level);
        }

        /// <summary>
        /// Faz log de mensagem com prefixo de input da função.
        /// </summary>
        /// <param name="msg">Nome da variável (ESPAÇO) valor da variável (VIRGULA)</param>
        /// <param name="level">Nível de detalhe da mensagem. Se não for passado, escreve sempre no log.</param>
        public void LogInputOnDB(string msg, LogLevel level = LogLevel.Debug)
        {
            LogOnDB("INPUT: " + msg, level);
        }

        /// <summary>
        /// Faz log da mensagem na BD, se o nível atual do Logger for igual ou superior ao da mensagem.
        /// </summary>
        /// <param name="msg">Mensagem para fazer log</param>
        /// <param name="level">Nível de detalhe da mensagem. Se não for passado, escreve sempre no log.</param>
        public void LogOnDB(string msg, LogLevel level = LogLevel.None)
        {
            if (IsAllowedToLog(level))
            {
                try
                {
                    // Limpa a mensagem de caracteres especiais
                    msg = Regex.Replace(msg, "(\r)|(\n)|'", "");

                    // Limita o tamanho da mensagem, se estiver definido tal limite
                    msg = Prefix + msg;
                    if (MAXIMUM_ENTRY_LENGTH > 0)
                        msg = (msg.Length <= MAXIMUM_ENTRY_LENGTH) ? msg : msg.Substring(0, MAXIMUM_ENTRY_LENGTH - 5) + "...";

                    // Constrói a mensagem a escrever, usando prefixo, e escreve-a na DB
                    using (SqlConnection connection = new SqlConnection(DBConnectionString))
                    {
                        string query = String.Format(QueryThatLogsOnDB, msg);

                        connection.Open();
                        new SqlCommand(query, connection).ExecuteNonQuery();
                    }
                }
                catch (Exception) { }   // Se deu erro a fazer log, não pode fazer log do erro, duh!
            }
        }

        #region Auxiliares

        private bool IsAllowedToLog(LogLevel desiredLevel)
        {
            return (int)MaximumLevel >= (int)desiredLevel;
        }

        #endregion
    }
}
