using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SoftLife.CSharp
{
    /// <summary>
    /// Classe de utilitários relacionados com leitura e escrita de AppConfigs.
    /// </summary>
    public static class Configs
    {
        /// <summary>
        /// Lê ConnectionString do ficheiro App.config.
        /// </summary>
        /// <param name="connectionName">Nome da connection string</param>
        /// <returns>String da ligação à BD (connection string)</returns>
        public static string ReadConnectionString(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }

        /// <summary>
        /// Lê configuração do ficheiro App.config.
        /// </summary>
        /// <param name="configName">Nome (KEY) da configuração</param>
        /// <returns>Valor (VALUE) da configuração</returns>
        public static string ReadValue(string configName)
        {
            return ConfigurationManager.AppSettings[configName];
        }

        /// <summary>
        /// Lê configuração de um ficheiro *.config específico.
        /// </summary>
        /// <param name="configName">Nome (KEY) da configuração</param>
        /// <param name="configPath">Caminho completo para o ficheiro .config</param>
        /// <returns>Valor (VALUE) da configuração</returns>
        public static string ReadValue(string configName, string configPath)
        {
            // Validações ao input
            if (string.IsNullOrEmpty(configName) || string.IsNullOrEmpty(configPath))
                throw new ArgumentException("O nome e o caminho da configuração não podem ser vazios.");
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Não existe um ficheiro no caminho indicado.");

            // Lê ficheiro
            string fileContents = File.ReadAllText(configPath);
            if (fileContents == "") throw new Exception("Ficheiro da configuração está vazio.");

            // Saca valor da configuração
            XDocument file = XDocument.Parse(fileContents);
            string expression = "string(//add[@key='" + configName + "']/@value)";  // vai buscar o valor string do atributo value, do element add cujo atributo key é igual a configName
            return file.XPathEvaluate(expression).ToString();
        }

        /// <summary>
        /// Escreve configuração do ficheiro App.config.
        /// </summary>
        /// <param name="configName">Nome (KEY) da configuração</param>
        /// <param name="value">Novo valor (VALUE) da configuração</param>
        [Obsolete]// precisa de mais testes
        public static void WriteValue(string configName, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(configName);
            config.AppSettings.Settings.Add(configName, value);

            config.Save(ConfigurationSaveMode.Modified);        // Save the changes in App.config file
            ConfigurationManager.RefreshSection("appSettings"); // Force a reload of a changed section
        }

        /// <summary>
        /// Escreve configuração do ficheiro *.config específico.
        /// </summary>
        /// <param name="configName">Nome (KEY) da configuração</param>
        /// <param name="value">Novo valor (VALUE) da configuração</param>
        /// <param name="configPath">Caminho completo para o ficheiro .config</param>
        public static void WriteValue(string configName, string value, string configPath)
        {
            XDocument file = XDocument.Parse(File.ReadAllText(configPath));         // carrega ficheiro config
            string expression = "//add[@key='" + configName + "']";
            file.XPathSelectElement(expression).Attribute("value").Value = value;   // altera valor
            file.Save(configPath);                                                  // reescreve ficheiro
        }

        /// <summary>
        /// Garante que o ficheiro de configurações tem uma determinada estrutura.
        /// Ao adicionar elementos em falta usa os valores default recebidos.
        /// </summary>
        /// <param name="defaultStructure">Lista de pares (NomeConfig, ValorDefaultConfig)</param>
        /// <param name="configPath">Caminho completo para o ficheiro .config.</param>
        /// <returns>True se a estrutura do ficheiro .config foi alterada.</returns>
        public static bool HasMissingFields(Dictionary<string, string> defaultStructure, string configPath)
        {
            bool changesWereMade = false;
            XDocument file = XDocument.Parse(File.ReadAllText(configPath));         // carrega ficheiro config

            foreach (string configName in defaultStructure.Keys)
            {
                // Valida se estrutura está desatualizada
                string expression = "//add[@key='" + configName + "']";
                if (file.XPathSelectElement(expression) == null)
                {   
                    // Cria configuração em falta
                    string configDefault = defaultStructure[configName];
                    XElement missingElement = new XElement("add");
                    missingElement.Add(new XAttribute("key", configName));
                    missingElement.Add(new XAttribute("value", configDefault));

                    // Adiciona configuração à estrutura
                    file.XPathSelectElement("//appSettings").Add(missingElement);
                    changesWereMade = true;
                }
            }

            if (changesWereMade) file.Save(configPath);
            return changesWereMade;
        }

        /// <summary>
        /// Valida se o ficheiro de configurações tem um campos não preenchidos.
        /// </summary>
        /// <param name="defaultStructure">Lista de pares (NomeConfig, ValorDefaultConfig)</param>
        /// <param name="configPath">Caminho completo para o ficheiro .config.</param>
        /// <returns>True se há 1ou+ campos não preenchidos.</returns>
        public static bool HasEmptyFields(Dictionary<string, string> defaultStructure, string configPath)
        {
            bool foundEmptyField = false;
            XDocument file = XDocument.Parse(File.ReadAllText(configPath)); // carrega ficheiro config

            try
            {
                foreach (string configName in defaultStructure.Keys)
                {
                    string expression = "//add[@key='" + configName + "']";
                    string value = file.XPathSelectElement(expression).Attribute("value").Value;
                    if (value == "")
                    {
                        // Campo está vazio
                        foundEmptyField = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // Campo não existe
                foundEmptyField = true;
            }
            return foundEmptyField;
        }

        /// <summary>
        /// Diz se uma versão é maior que outra.
        /// </summary>
        /// <param name="higherVersion">Versão disponível</param>
        /// <param name="lowerVersion">Versão mínima requirida</param>
        /// <returns>1 se primeira versão é maior que segunda; -1 se for menor; 0 se forem iguais</returns>
        public static int CompareVersions(string higherVersion, string lowerVersion)
        {
            Version high, low;
            if (Version.TryParse(higherVersion, out high) == false) high = new Version("0.0.0.0");
            if (Version.TryParse(lowerVersion, out low) == false) low = new Version("0.0.0.0");
            return high.CompareTo(low);
        }

        /// <summary>
        /// Retorna versão da Dll. 
        /// </summary>
        /// <param name="name">Nome da DLL, incluindo a extensão .dll</param>
        /// <param name="path">Caminho até diretoria onde deve existir a DLL. Por omissão, a mesma diretoria do executável.</param>
        /// <returns></returns>
        public static Version GetDllVersion(string name, string path = "")
        {
            if (path == "") path = Directory.GetCurrentDirectory(); // default path

            return new Version(FileVersionInfo.GetVersionInfo(path + "\\" + name).FileVersion);
        }

        /// <summary>
        /// Valida se a versão atual de um componente é igual ou superior à versão mínima requirida. Caso contrário, lança Exception.
        /// </summary>
        /// <param name="available">Versão disponível</param>
        /// <param name="minimum">Versão mínima requirida</param>
        /// <param name="componentName">Nome do componente a ser validado</param>
        public static void AssertVersion(Version available, Version minimum, string componentName)
        {
            if (available.CompareTo(minimum) < 0)
                throw new Exception(string.Format("A aplicação necessita da versão {0} do componente \"{1}\", no entanto só está disponível uma versão anterior com a numeração {2}."
                    , minimum, componentName, available));
        }

        /// <summary>
        /// Valida se existe o ficheiro de configuração. Caso contrário, lança FileNotFoundException.
        /// </summary>
        /// <param name="configPath">Caminho completo para o ficheiro .config</param>
        public static void AssertExistsConfigFile(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Não foi encontrado o ficheiro de configuração na localização: " + configPath);
        }

        /// <summary>
        /// Valida se existe DLL auxiliar. Caso contrário, lança FileNotFoundException.
        /// </summary>
        /// <param name="name">Nome da DLL incluindo extensão .dll</param>
        /// <param name="path">Caminho até diretoria onde deve existir a DLL. Por omissão, a mesma diretoria do executável.</param>
        public static void AssertExistsDll(string name, string path = "")
        {
            if (path == "") path = Directory.GetCurrentDirectory(); // default path

            if (!File.Exists(path + "\\" + name))
                throw new FileNotFoundException(string.Format("Não foi encontrada a DLL \"{0}\" na localização \"{1}\".", name, path));
        }

        /// <summary>
        /// Valida se DLL existe e depois a sua versão. Caso contrário, lança FileNotFoundException ou Exception, respectivamente.
        /// </summary>
        /// <param name="name">Nome da DLL incluindo extensão .dll</param>
        /// <param name="minimum">Versão mínima requirida</param>
        /// <param name="path">Caminho até diretoria onde deve existir a DLL. Por omissão, a mesma diretoria do executável.</param>
        public static void AssertExistsDllWithVersion(string name, Version minimum, string path = "")
        {
            AssertExistsDll(name, path);
            AssertVersion(GetDllVersion(name, path), minimum, name);
        }
    }

    /// <summary>
    /// Classe de utilitários relcionados com a leitura e escrita da tabela Configuracao.
    /// </summary>
    public static class DatabaseConfigs
    {
        /// <summary>
        /// Faz uma leitura à tabela Configuracao e retorna a coluna Valor.
        /// </summary>
        /// <param name="dbConnection">Ligação à BD onde está a tabela com as configurações.</param>
        /// <param name="section"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ReadInteger(string dbConnection, string section, string name, int defaultValue)
        {
            string result = ReadString(dbConnection, section, name, defaultValue.ToString());
            return (string.IsNullOrEmpty(result)) ? defaultValue : Convert.ToInt32(result);
        }

        /// <summary>
        /// Faz uma leitura à tabela Configuracao e retorna a coluna Valor.
        /// </summary>
        /// <param name="dbConnection">Ligação à BD onde está a tabela com as configurações.</param>
        /// <param name="section"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ReadString(string dbConnection, string section, string name, string defaultValue)
        {
            try
            {
                string query =
                    " SELECT Valor " +
                    " FROM Configuracao " +
                    " WHERE Seccao = " + Utils.Quote(section) +
                    "   AND Nome = " + Utils.Quote(name);

                return DatabaseAdapter.GetScalar(query, dbConnection);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
