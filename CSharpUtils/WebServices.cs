using System;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Web.Configuration;

namespace SoftLife.CSharp
{
    /// <summary>
    /// Classe de utilitários relacionados com WebServices e WebMethods.
    /// </summary>
    public static class WebUtils
    {
        /// <summary>
        /// Faz ping a um endereço. FROM: http://stackoverflow.com/a/11804416/675577
        /// </summary>
        /// <param name="nameOrAddress">Endereço a pingar.</param>
        /// <returns>BadRoute se endereço desconhecido; Success se conseguir resposta; Timeout caso contrário.</returns>
        public static IPStatus Ping(string nameOrAddress)
        {
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(nameOrAddress);
                return reply.Status;
            }
            catch (PingException) { return IPStatus.BadRoute; }
        }

        /// <summary>
        /// Retorna uma reposta PONG.
        /// </summary>
        /// <param name="dbConnectionString">Ligação à base de dados.</param>
        /// <param name="assemblyVersion">Versão do webservice: typeof(ClasseDoWebservice).Assembly.GetName().Version</param>
        /// <param name="returnAsXml">True se o resultado deve ser retornado com a estrutura de XML.</param>
        /// <returns></returns>
        public static string Pong(string dbConnectionString, string assemblyVersion, bool returnAsXml = true)
        {
            DatabaseLogger logger = DatabaseLogger.GetInstance("Ping");
            string pong;
            try
            {
                SqlConnectionStringBuilder sqlConnectionStr = new SqlConnectionStringBuilder(dbConnectionString);
                string sqlConnection = string.Format("{0}.{1}", sqlConnectionStr.DataSource, sqlConnectionStr.InitialCatalog);

                pong = "PONG"
                    + " | " + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss")
                    + " | " + assemblyVersion
                    + " | " + sqlConnection;
            }
            catch (Exception e)
            {
                pong = "ERROR " + e.Message;
            }

            if (returnAsXml) pong = string.Format("<scalar>{0}</scalar>", pong);
            return pong;
        }

        /// <summary>
        /// Dada uma connection devolve o nome do servidor onde está a base de dados.
        /// </summary>
        /// <param name="connectionString">Ligação à base de dados.</param>
        /// <returns>Nome do servidor onde está a base de dados</returns>
        public static string GetConnectionServerName(string connectionString)
        {
            string[] connectionParams = connectionString.Split(';');
            foreach (string param in connectionParams)
            {
                if (param.Contains("server="))
                    return param.Replace("server=", "").Trim();
            }
            return "Couldn't read SQL connection string";
        }

        /// <summary>
        /// Encodes a given text to a URL safe representation.
        /// </summary>
        /// <param name="text">Url with unsafe parameters.</param>
        /// <returns>Url with encoded parameters.</returns>
        public static string EncodeToUrl(string text)
        {
            return Uri.EscapeDataString(text);
        }
    }

    /// <summary>
    /// Classe de utilitários relacionados com leitura e escrita de WebConfigs.
    /// </summary>
    public static class WebConfigs
    {
        /// <summary>
        /// Lê configuração do ficheiro Web.Config.
        /// </summary>
        /// <param name="configName">Nome (KEY) da configuração</param>
        /// <returns>Valor (VALUE) da configuração</returns>
        public static string ReadValue(string configName)
        {
            return WebConfigurationManager.AppSettings[configName];
        }

        /// <summary>
        /// Lê ConnectionStrings do ficheiro Web.Config.
        /// </summary>
        /// <param name="connectionName">Nome da connection string</param>
        /// <returns>String da ligação à BD (connection string)</returns>
        public static string ReadConnectionString(string connectionName)
        {
            return WebConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }
    }    
}
