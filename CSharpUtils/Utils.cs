using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SoftLife.CSharp
{
    /// <summary>
    /// Class containing constants used across several applications.
    /// </summary>
    public static class Constants
    {
        #region Datas

        public const int NUM_DAYS_OF_MONTH = 30;
        public static DateTime MIN_DATE = new DateTime(1900, 01, 01, 00, 00, 00);
        public static DateTime MAX_DATE = new DateTime(9999, 12, 31, 23, 59, 59);

        #endregion
    }

    /// <summary>
    /// Class containing methods and features used across several applications.
    /// </summary>
    public static class Utils
    {
        //TODO Move this to Constants class
        #region Display de dados - Constantes

        /// <summary>
        /// String used when a DateTime is not suitable for display.
        /// </summary>
        public static string EMPTY_DATE = "---";
        /// <summary>
        /// String used when a string is not suitable for display.
        /// </summary>
        public static string EMPTY_DESC = "---";
        /// <summary>
        /// String used when a number is not suitable for display.
        /// </summary>
        public static string EMPTY_TOTAL = "--";

        /// <summary>
        /// Integer used instead of NULL.
        /// </summary>
        public static int DEFAULT_INTEGER = 0;
        /// <summary>
        /// Decimal used instead of NULL.
        /// </summary>
        public static decimal DEFAULT_DECIMAL = 0.0m;
        /// <summary>
        /// DateTime used instead of NULL.
        /// </summary>
        public static DateTime DEFAULT_DATETIME = new DateTime(1900, 01, 01);

        public enum DisplayTypes { Datetime, Date, HumanDate, Money, Decimal, Integer, String, ShortName }

        /// <summary>
        /// DateTime.ToString format according to ISO 8601.
        /// </summary>
        public const string UTC_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        /// <summary>
        /// Date culture invariant format for machines.
        /// </summary>
        public const string DATE_INVARIANTFORMAT = "yyyy-MM-dd";
        /// <summary>
        /// DateTime.ToString format used to display date only.
        /// </summary>
        public const string DATE_FORMAT = "d";
        /// <summary>
        /// DateTime culture invariant format for machines.
        /// </summary>
        public const string DATETIME_INVARIANTFORMAT = "yyyy-MM-dd HH:mm:ss";
        /// <summary>
        /// DateTime.ToString format used to display both date and time.
        /// </summary>
        public const string DATETIME_FORMAT = "g";
        /// <summary>
        /// DateTime.ToString format used for humans to read.
        /// </summary>
        public const string HUMANDATE_FORMAT = "d MMM yyyy";

        #endregion
        #region Display de dados

        /// <summary>
        /// Formats a string according to display type and culture (uses a default value if value is empty).
        /// </summary>
        /// <param name="value">String value to format.</param>
        /// <param name="type">Format the value according to a data type.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatValue(string value, DisplayTypes type)
        {
            string formattedValue = String.Empty;
            bool isDefault = false;
            switch (type)
            {
                case DisplayTypes.Datetime:
                    formattedValue = FormatDatetime(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DATE;
                    break;
                case DisplayTypes.Date:
                    formattedValue = FormatDate(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DATE;
                    break;
                case DisplayTypes.HumanDate:
                    formattedValue = FormatHumanDate(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DATE;
                    break;
                case DisplayTypes.Money:
                    formattedValue = FormatMoney(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DESC;
                    break;
                case DisplayTypes.Decimal:
                    formattedValue = FormatDecimal(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DESC;
                    break;
                case DisplayTypes.Integer:
                    formattedValue = FormatInteger(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DESC;
                    break;
                case DisplayTypes.String:
                    formattedValue = FormatString(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DESC;
                    break;
                case DisplayTypes.ShortName:
                    formattedValue = FormatShortName(value, out isDefault);
                    if (isDefault) formattedValue = EMPTY_DESC;
                    break;
            }
            return formattedValue;
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="money">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayMoney(decimal money)
        {
            return DisplayDecimal(money) + " " + CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
        }

        /// <summary>
        /// Formats string according to Money format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatMoney(string str, out bool isDefault)
        {
            decimal money = ConvertToDecimal(str, out isDefault, CultureInfo.CurrentCulture.Name);
            return DisplayMoney(money);
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="percentage">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayPercentage(decimal percentage)
        {
            return DisplayDecimal(percentage) + " %";
        }

        /// <summary>
        /// Formats string according to Percentage format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatPercentage(string str, out bool isDefault)
        {
            decimal percentage = ConvertToDecimal(str, out isDefault, CultureInfo.CurrentCulture.Name);
            return DisplayPercentage(percentage);
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="number">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayDecimal(decimal number)
        {
            decimal truncated = Math.Truncate(number * 100) / 100;          // truncate decimal digits
            return truncated.ToString("0.##", CultureInfo.CurrentCulture);  // 0.## decimal digits are display if != 0
        }

        /// <summary>
        /// Formats string according to Decimal number format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatDecimal(string str, out bool isDefault)
        {
            decimal money = ConvertToDecimal(str, out isDefault, CultureInfo.CurrentCulture.Name);
            return DisplayDecimal(money);
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="number">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayInteger(int number)
        {
            return number.ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats string according to Integer number format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatInteger(string str, out bool isDefault)
        {
            int number = ConvertToInteger(str, out isDefault, CultureInfo.CurrentCulture.Name);
            return DisplayInteger(number);
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="str">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayString(string str)
        {
            return str;
        }

        /// <summary>
        /// Formats string according to String format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatString(string str, out bool isDefault)
        {
            isDefault = (str == string.Empty) ? true : false;
            return str;
        }

        /// <summary>
        /// Formats name into short format (First and Last name).
        /// </summary>
        /// <param name="personName">Name to format.</param>
        /// <param name="isDefault">True, if personName was empty and a default value was returned.</param>
        /// <returns>Formatted name.</returns>
        private static string FormatShortName(string personName, out bool isDefault)
        {
            if (personName != string.Empty)
            {
                string[] names = personName.Split(' ');
                string shortName = string.Format("{0} {1}", names[0], names[names.Length-1]);

                isDefault = false;
                return shortName;
            }
            else
            {
                isDefault = true;
                return EMPTY_DESC;
            }
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="datetime">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayDatetime(DateTime datetime)
        {
            if (datetime != DEFAULT_DATETIME)
                return datetime.ToString(DATETIME_FORMAT, CultureInfo.CurrentCulture);
            else
                return EMPTY_DATE;
        }

        /// <summary>
        /// Formats string according to DateTime format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatDatetime(string str, out bool isDefault)
        {
            DateTime datetime = ConvertToDatetime(str, out isDefault, CultureInfo.CurrentCulture.Name);
            return (datetime == DEFAULT_DATETIME) ? EMPTY_DATE : DisplayDatetime(datetime);
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="datetime">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayDate(DateTime datetime)
        {
            if (datetime != DEFAULT_DATETIME)
                return datetime.ToString(DATE_FORMAT, CultureInfo.CurrentCulture);
            else
                return EMPTY_DATE;
        }

        /// <summary>
        /// Formats string according to Date only format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatDate(string str, out bool isDefault)
        {
            DateTime datetime = ConvertToDatetime(str, out isDefault, CultureInfo.CurrentCulture.Name);
            return DisplayDate(datetime);
        }

        /// <summary>
        /// Converts the data type to its string representation (culture dependant).
        /// </summary>
        /// <param name="datetime">Object to format.</param>
        /// <returns>String representation (culture dependant).</returns>
        public static string DisplayHumanDate(DateTime datetime)
        {
            if (datetime != DEFAULT_DATETIME)
                return datetime.ToString(HUMANDATE_FORMAT, CultureInfo.CurrentCulture);
            else
                return EMPTY_DATE;
        }

        /// <summary>
        /// Formats string according to Human-like, Date only format.
        /// </summary>
        /// <param name="str">String to format.</param>
        /// <param name="isDefault">True, if str was empty and a default value was returned.</param>
        /// <returns>Formatted string.</returns>
        private static string FormatHumanDate(string str, out bool isDefault)
        {
            DateTime datetime = ConvertToDatetime(str, out isDefault, CultureInfo.CurrentCulture.Name);
            return DisplayHumanDate(datetime);
        }

        public static string UnformatPercentage(string formattedPercentage)
        {
            formattedPercentage = formattedPercentage.Replace("%", "");
            formattedPercentage = formattedPercentage.Replace(",", ".");
            return formattedPercentage.Trim();
        }

        public static DateTimePicker LoadDateTimePicker(DateTimePicker datetimePicker, DateTime datetime)
        {
            try
            {
                if (datetime.Year > DEFAULT_DATETIME.Year)
                    datetimePicker.Value = datetime;
                else
                    throw new Exception("The date received represents an empty date");
            }
            catch (Exception)
            {
                datetimePicker.Format = DateTimePickerFormat.Custom;
                datetimePicker.CustomFormat = " ";
            }
            return datetimePicker;
        }

        #endregion
        #region Conversores de dados

        // Xml

        /// <summary>
        /// Converts a string to an XDocument.
        /// </summary>
        /// <param name="xmlContent"></param>
        /// <param name="removeNamespaces">True, ignores xml namespaces during the conversion.</param>
        /// <returns>Input parsed as XDocument, otherwise empty XDocument.</returns>
        public static XDocument ConvertToXDoc(string xmlContent, bool removeNamespaces = false)
        {
            // Invalid scenario
            if (string.IsNullOrEmpty(xmlContent)) return new XDocument();

            // Valid scenario
            if (removeNamespaces)
                return RemoveNamespaces(xmlContent);
            else
                return XDocument.Parse(xmlContent);
        }

        /// <summary>
        /// Converts a XmlDocument to an XDocument.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="removeNamespaces">True, ignores xml namespaces during the conversion.</param>
        /// <returns>Input parsed as XDocument, otherwise empty XDocument.</returns>
        public static XDocument ConvertToXDoc(XmlDocument xml, bool removeNamespaces = false)
        {
            if (xml != null)
                return ConvertToXDoc(xml.OuterXml, removeNamespaces);
            else
                return new XDocument();
        }

        // Date and Time

        /// <summary>
        /// Converts the string into a datetime.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="cultureName">Culture name (ToString) to use during the convertion.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Input não é válido para a cultura do sistema.</exception>
        /// <exception cref="OverflowException"></exception>
        public static DateTime ConvertToDatetime(string str, string cultureName = "CurrentCulture")
        {
            CultureInfo culture = (cultureName == "CurrentCulture") ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);

            return Convert.ToDateTime(str, culture);
        }

        /// <summary>
        /// Converts the string into a datetime. If the string is an invalid datetime, the default datetime (1900-01-01) is returned.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="isDefault">True, if convertion failed and a default value was returned.</param>
        /// <param name="cultureName">Culture name (ToString) to use during the convertion.</param>
        /// <returns></returns>
        public static DateTime ConvertToDatetime(string str, out bool isDefault, string cultureName = "CurrentCulture")
        {
            CultureInfo culture = (cultureName == "CurrentCulture") ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);
            DateTime result;
            try
            {
                result = ConvertToDatetime(str, culture.Name);
                isDefault = false;
            }
            catch (Exception)
            {
                result = DEFAULT_DATETIME;
                isDefault = true;
            }
            return result;
        }

        /// <summary>
        /// Converts the string into a datetime. If the string is an invalid datetime, the default datetime (1900-01-01) is returned.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <returns></returns>
        public static DateTime ConvertToDatetimeFromInvariant(string str)
        {
            bool isDefault;
            return ConvertToDatetime(str, out isDefault, CultureInfo.InvariantCulture.Name);
        }

        /// <summary>
        /// Converts a DateTime to an invariant string.
        /// </summary>
        /// <param name="datetime">DateTime to convert.</param>
        /// <returns></returns>
        public static string ConvertDatetimeToInvariant(DateTime datetime)
        {
            return datetime.ToString(UTC_FORMAT, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a Date to an invariant string.
        /// </summary>
        /// <param name="datetime">Date to convert.</param>
        /// <returns></returns>
        public static string ConvertDateToInvariant(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        // Decimals and Integers

        /// <summary>
        /// Converts a string to a decimal.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="cultureName">Culture name (ToString) to use during the convertion.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Input não é válido para a cultura do sistema.</exception>
        /// <exception cref="OverflowException"></exception>
        public static decimal ConvertToDecimal(string str, string cultureName = "CurrentCulture")
        {
            CultureInfo culture = (cultureName == "CurrentCulture") ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);

            return Convert.ToDecimal(str, culture);
        }

        /// <summary>
        /// Converts a string to a decimal. If the string is an invalid decimal, the default decimal (0.0) is returned.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="isDefault">True, if convertion failed and a default value was returned.</param>
        /// <param name="cultureName">Culture name (ToString) to use during the convertion.</param>
        /// <returns></returns>
        public static decimal ConvertToDecimal(string str, out bool isDefault, string cultureName = "CurrentCulture")
        {
            CultureInfo culture = (cultureName == "CurrentCulture") ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);
            decimal result;
            try
            {
                result = ConvertToDecimal(str, culture.Name);
                isDefault = false;
            }
            catch (Exception)
            {
                result = DEFAULT_DECIMAL;
                isDefault = true;
            }
            return result;
        }

        /// <summary>
        /// Converts a string to a decimal. If the string is an invalid decimal, the default decimal (0.0) is returned.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <returns></returns>
        public static decimal ConvertToDecimalFromInvariant(string str)
        {
            bool isDefault;
            return ConvertToDecimal(str, out isDefault, CultureInfo.InvariantCulture.Name);
        }

        /// <summary>
        /// Converts a decimal to an invariant string.
        /// </summary>
        /// <param name="number">Decimal to convert.</param>
        /// <returns></returns>
        public static string ConvertDecimalToInvariant(decimal number)
        {
            return number.ToString("0.##", CultureInfo.InvariantCulture);
        }

        // Numbers

        /// <summary>
        /// Converts a string to an integer.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="cultureName">Culture name (ToString) to use during the convertion.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Input não é válido para a cultura do sistema.</exception>
        /// <exception cref="OverflowException"></exception>
        public static int ConvertToInteger(string str, string cultureName = "CurrentCulture")
        {
            CultureInfo culture = (cultureName == "CurrentCulture") ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);

            return Convert.ToInt32(str, culture);
        }

        /// <summary>
        /// Converts a string to an integer. If the string is an invalid integer, the default integer (0) is returned.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="isDefault">True, if convertion failed and a default value was returned.</param>
        /// <param name="cultureName">Culture name (ToString) to use during the convertion.</param>
        /// <returns></returns>
        public static int ConvertToInteger(string str, out bool isDefault, string cultureName = "CurrentCulture")
        {
            CultureInfo culture = (cultureName == "CurrentCulture") ? CultureInfo.CurrentCulture : new CultureInfo(cultureName);
            int result;
            try
            {
                result = ConvertToInteger(str, culture.Name);
                isDefault = false;
            }
            catch (Exception)
            {
                result = DEFAULT_INTEGER;
                isDefault = true;
            }
            return result;
        }

        /// <summary>
        /// Converts a string to an integer. If the string is an invalid integer, the default integer (0) is returned.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <returns></returns>
        public static int ConvertToIntegerFromInvariant(string str)
        {
            bool isDefault;
            return ConvertToInteger(str, out isDefault, CultureInfo.InvariantCulture.Name);
        }

        // Boolean

        /// <summary>
        /// Converts a string to boolean type. Empty strings are converted to False.
        /// </summary>
        /// <param name="str">Valid strings (CI): true, false, 1, 0</param>
        /// <returns>String converted to boolean.</returns>
        /// <exception cref="FormatException">The string is not recognized as a valid boolean value.</exception>
        public static bool ConvertToBoolean(string str)
        {
            if ((str == "") || (str.ToLower() == "false") || (str == "0"))
            {
                return false;
            }
            else if ((str.ToLower() == "true") || (str == "1"))
            {
                return true;
            }
            else
            {
                throw new FormatException("The string is not recognized as a valid boolean value.");
            }
        }
        
        #endregion
        #region XML

        public static XDocument RemoveNamespaces(XDocument oldXml)
        {
            // FROM: http://social.msdn.microsoft.com/Forums/en-US/bed57335-827a-4731-b6da-a7636ac29f21/xdocument-remove-namespace?forum=linqprojectgeneral
            // Remove all xmlns:* instances from the passed XmlDocument to simplify our xpath expressions
            try
            {
                XDocument newXml = XDocument.Parse(Regex.Replace(
                    oldXml.ToString(),
                    @"(xmlns:?[^=]*=[""][^""]*[""])",
                    "",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline)
                );
                return newXml;
            }
            catch (XmlException error)
            {
                throw new XmlException(error.Message + " at Utils.RemoveNamespaces");
            }
        }

        public static XDocument RemoveNamespaces(string xml)
        {
            XDocument xmlDoc = XDocument.Parse(xml);
            return RemoveNamespaces(xmlDoc);
        }

        /// <summary>
        /// Returns true if XML is valid, false otherwise.
        /// </summary>
        public static bool IsValidXml(string sXml, string xsdFilePath, out string reasonWhyInvalid)
        {
            try
            {
                ValidateXml(sXml, xsdFilePath);
            }
            catch (XmlSchemaValidationException e)
            {
                reasonWhyInvalid = e.Message;
                return false;
            }
            reasonWhyInvalid = "";
            return true;
        }

        /// <summary>
        /// Throws an exception if XML is not valid.
        /// </summary>
        public static void ValidateXml(string sXml, string xsdFilePath)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(sXml);
            xdoc.Schemas.Add(null, xsdFilePath);
            xdoc.Validate(ValidationCallBack);  //throws an exception if not valid
        }

        public static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                throw new XmlSchemaValidationException("Schema not found, no validation occurred. " + args.Message);
            else
                throw new XmlSchemaValidationException(args.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <param name="attributeName"></param>
        /// <returns>Valor do atributo ou string vazia.</returns>
        public static string GetAttributeOrEmpty(XElement xmlElement, string attributeName)
        {
            return GetAttributeOrDefault(xmlElement, attributeName, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue">Valor por omissão, quando atributo não existe.</param>
        /// <returns>Valor do atributo ou valor por omissão.</returns>
        public static string GetAttributeOrDefault(XElement xmlElement, string attributeName, string defaultValue)
        {
            if (xmlElement.Attribute(attributeName) != null)
                return xmlElement.Attribute(attributeName).Value;
            else
                return defaultValue;
        }

        #endregion
        #region Tables/Rows

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <returns>Valor da coluna ou string vazia.</returns>
        public static string GetColumnOrEmpty(DataRow row, string columnName)
        {
            return GetColumnOrDefault(row, columnName, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <param name="defaultValue">Valor por omissão, quando coluna não existe.</param>
        /// <returns>Valor da coluna ou valor por omissão.</returns>
        public static string GetColumnOrDefault(DataRow row, string columnName, string defaultValue)
        {
            if (row.Table.Columns.Contains(columnName))
                return row[columnName].ToString();
            else
                return defaultValue;
        }

        #endregion
        #region Strings

        /// <summary>
        /// Converts string to byte[]. From http://stackoverflow.com/a/10380166/675577
        /// </summary>
        public static byte[] GetBytes(string str)
        {
            //byte[] bytes = new byte[str.Length * sizeof(char)];
            //Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            //return bytes;

            return Encoding.UTF8.GetBytes(str);
        }

        public static Stream StringToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string StreamToString(Stream stream)
        {
            // convert stream to string
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Converts a string that has been HTML-enconded for HTTP transmission into a decoded string.
        /// </summary>
        /// <param name="escapedString">String to decode.</param>
        /// <returns>Decoded (unescaped) string.</returns>
        public static string UnescapeString(string escapedString)
        {
            return HttpUtility.HtmlDecode(escapedString);
        }

        /// <summary>
        /// Converts a plain string to HTTP transmission, making it HTML safe.
        /// </summary>
        /// <param name="escapedString">String to encode.</param>
        /// <returns>Encoded (escaped) string.</returns>
        public static string EscapeString(string escapedString)
        {
            return HttpUtility.HtmlEncode(escapedString);
        }

        /// <summary>
        /// Devolve a string entre caracteres, por omissão plicas ('), tal como o QuotedStr do Delphi.
        /// </summary>
        /// <param name="str">String.</param>
        /// <param name="quoteChar">Caracter que vai rodear a string. Por omissão, é usado plica (')</param>
        /// <returns>String rodeada por caracter</returns>
        public static string Quote(string str, char quoteChar = '\'')
        {
            return quoteChar + str + quoteChar;
        }

        #endregion
        #region DateTimes

        public static string DisplayTimeInterval(DateTime startTime, DateTime dateTime)
        {
            double minutes, seconds;
            seconds = (DateTime.Now - startTime).TotalSeconds;

            if (seconds < 60)
            {
                return String.Format("({0}s)", Math.Round(seconds));
            }
            else if (seconds < 60 * 60)
            {
                minutes = Math.Floor(seconds / 60);
                seconds = Math.Round(((seconds / 60) - minutes) * 60);
                return String.Format("({0}m{1}s)", minutes, seconds);
            }
            else
            {
                return String.Format("({0}h)", Math.Round(((seconds / 60) / 60), 2));
            }
        }


        #endregion
        #region Id Generator

        private static int IdGenerator = 1;

        public static int GenerateId()
        {
            return IdGenerator++;
        }

        #endregion
        #region Cursores

        public static void DisplayDefaultCursor()
        {
            Cursor.Current = Cursors.Default;
        }

        public static void DisplayWaitCursor()
        {
            Cursor.Current = Cursors.WaitCursor;
        }

        public static void DisplayCustomCursor(Cursor customCursor)
        {
            Cursor.Current = customCursor;
        }

        #endregion
        #region Mensagens de I/O

        public static DialogResult InputBox(string title, string promptText, ref string value, bool isPassword = false)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            if (isPassword) textBox.PasswordChar = '●';

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        public static void ShowDebugMsg(string message, string caption = "DEBUG")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static void ShowInfoMsg(string message, string caption = "Informação")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarningMsg(string message, string caption = "Aviso")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static void ShowErrorMsg(string message, string caption = "Erro")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ShowDecisionMsg(string question, string caption = "Confirmação")
        {
            return MessageBox.Show(question, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        #endregion
        #region Imagens

        /// <summary>
        /// Converts a Base64 string into an Image.
        /// </summary>
        /// <param name="encodedImage">Image encoded in Base64.</param>
        /// <returns></returns>
        public static Image ConvertBase64ToBitmap(string encodedImage)
        {
            byte[] rawImage = Convert.FromBase64String(encodedImage);
            using(MemoryStream stream = new MemoryStream(rawImage, 0, rawImage.Length))
            {
                Image image = Image.FromStream(stream);
                return image;
            }
        }

        #endregion

        #region Execução de DLLs

        /// <summary>
        /// Dynamically calls a DLL method, passing it a single argument.
        /// </summary>
        /// <param name="methodName">The method's name.</param>
        /// <param name="methodParam">The method's input. For multiple inputs, consider passing an XML or JSON.</param>
        /// <param name="dllPath">Location of the DLL.</param>
        /// <param name="dllName">Name of the DLL (without file extension).</param>
        /// <returns>Return of calling DLL's method with the given parameter.</returns>
        public static object CallDllMethod(string methodName, string methodParam, string dllPath, string dllName)
        {
            // This code was adapted from:
            // http://stackoverflow.com/questions/11886845/dynamically-calling-a-dll-and-method-with-arguments
            try
            {
                // Make sure the name doesn't contain the extension
                if (dllName.ToLowerInvariant().EndsWith(".dll"))
                {
                    int extensionIndex = dllName.ToLowerInvariant().LastIndexOf(".dll");
                    dllName = dllName.Substring(0, extensionIndex);
                }
                string dllNamespace = dllName + ".DllClass";

                // Locate DLL, read methods and its arguments
                Assembly assembly = Assembly.LoadFrom(dllPath + dllName + ".dll");
                Type type = assembly.GetType(dllNamespace);
                Object o = Activator.CreateInstance(type);
                MethodInfo method = type.GetMethod(methodName);

                // Prepare arguments for method
                ParameterInfo[] parameters = method.GetParameters();
                object[] methodParameters = new object[parameters.GetLength(0)];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var converter = TypeDescriptor.GetConverter(parameters[i].ParameterType);
                    methodParameters[i] = converter.ConvertFrom(methodParam);
                }

                // Call method
                return method.Invoke(o, methodParameters);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error while trying to call method {0} from DLL {1}.", methodName, dllName), e);
            }
        }

        #endregion

        #region Execução de processos

        /// <summary>
        /// Cria um processo para correr um executável.
        /// FROM: http://stackoverflow.com/a/181857/675577
        /// </summary>
        /// <param name="filename">Nome do executável. Exemplo: para a linha de comandos, "cmd.exe".</param>
        /// <param name="arguments">Argumentos a passar executável.</param>
        /// <param name="windowVisibility">Visibilidade da janela do executável.</param>
        /// <param name="waitForIt">True, se execução de quem chama fica em espera.</param>
        public static void RunProcess(string filename, string arguments, ProcessWindowStyle windowVisibility, bool waitForIt)
        {
            Process process = new Process();

            // configures the process using the StartInfo properties
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WindowStyle = windowVisibility;
            process.Start();

            if (waitForIt)
                process.WaitForExit();  // waits for the process to exit
        }

        /// <summary>
        /// Cria um processo para correr um executável e aguarda a sua terminação.
        /// </summary>
        /// <param name="filename">Nome do executável. Exemplo: para a linha de comandos, "cmd.exe".</param>
        /// <param name="arguments">Argumentos a passar executável.</param>
        /// <param name="windowVisibility">Visibilidade da janela do executável.</param>
        public static void RunProcessAndWait(string filename, string arguments, ProcessWindowStyle windowVisibility = ProcessWindowStyle.Hidden)
        {
            RunProcess(filename, arguments, windowVisibility, true);
        }

        #endregion

        #region ConsoleApplication

        // Lógica para minimizar "janela" de uma ConsoleApplication

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Codes here: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633548(v=vs.85).aspx
        /// </summary>
        public enum WindowState { HIDE = 0, SHOW = 5, RESTORE = 5, MINIMIZE = 6, MAXIMIZE = 3 }

        /// <summary>
        /// Altera a visibilidade da janela atual do tipo ConsoleApplication.
        /// </summary>
        /// <param name="state"></param>
        public static void SetWindowState(WindowState state)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, (int)state);
        }

        #endregion
    }
}
