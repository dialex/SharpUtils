using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SoftLife.CSharp
{
    #region Tipos

    /// <summary>
    /// Classe que permite as enumerações terem valores do tipo string associdados.
    /// FROM: http://www.codeassociate.com/caapi/html/T_CA_Common_Attributes_StringEnum.htm
    /// </summary>
    public static class StringEnum
    {
        #region Auxiliary classes

        /// <summary>
        /// Simple attribute class for storing string values that can be that can be accessed at run time.
        /// From http://www.codeassociate.com/caapi/html/T_CA_Common_Attributes_StringValueAttribute.htm
        /// </summary>
        public class StringValueAttribute : Attribute
        {
            public StringValueAttribute(string value)
            {
                Value = value;
            }

            public string Value { get; private set; }
        }

        /// <summary>
        /// A struct used for holding the values of an individual string enum. This struct is used for the bindable lists and used to construct the StringEnumDataTable.
        /// From: http://www.codeassociate.com/caapi/html/T_CA_Common_Attributes_StringEnumContainer.htm
        /// </summary>
        public struct StringEnumContainer
        {
            public Enum EnumCodevalue;
            public int EnumIntvalue;
            public string EnumStringvalue;
        }

        /// <summary>
        /// This class represents a specialization of data table used to hold the data from the enums.
        /// From: http://www.codeassociate.com/caapi/html/T_CA_Common_Attributes_StringEnumDataTable.htm
        /// </summary>
        public class StringEnumDataTable : DataTable
        {

            public StringEnumDataTable()
            {
                Columns.Add("ID", typeof(int));
                Columns.Add("EnumCodeValue", typeof(string));
                Columns.Add("EnumStringValue", typeof(string));
                AcceptChanges();
            }

            public void AddRowFromContainer(StringEnumContainer container)
            {
                object[] datarow = new object[3];
                datarow[0] = container.EnumIntvalue;
                datarow[1] = container.EnumCodevalue.ToString();
                datarow[2] = container.EnumStringvalue;
                LoadDataRow(datarow, true);
            }

            public new void WriteXmlSchema(System.IO.Stream s)
            {
                // This simply provides a documentation hook point for the WriteXmlSchema method
                base.WriteXmlSchema(s);
            }


            public new void ReadXmlSchema(System.IO.Stream s)
            {
                // This simply provides a documentation hook point for the ReadXmlSchema method
                base.ReadXmlSchema(s);
            }
        }

        #endregion

        private static Hashtable _stringValues = new Hashtable();

        private static void ValidateAsEnumClass(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("Supplied type must be an Enum.  Type was {0}", enumType));
        }

        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            if (_stringValues != null && _stringValues.ContainsKey(value))
            {
                StringValueAttribute sv = (StringValueAttribute)_stringValues[value];
                if (sv != null)
                    output = sv.Value;
            }
            else
            {
                //Look for our 'StringValueAttribute' in the field's custom attributes
                FieldInfo fi = type.GetField(value.ToString());
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null && attrs.Length > 0)
                {
                    // lock the Hash table while inserting new data. 
                    lock (_stringValues)
                    {
                        _stringValues.Add(value, attrs[0]);
                    }
                    output = attrs[0].Value;
                }

            }
            return output;
        }

        public static object Parse(Type type, string stringValue)
        {
            return Parse(type, stringValue, false);
        }

        public static object Parse(Type type, string stringValue, bool ignoreCase)
        {
            object output = null;
            string enumStringValue = null;

            ValidateAsEnumClass(type);

            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in type.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs != null && attrs.Length > 0)
                    enumStringValue = attrs[0].Value;

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, stringValue, ignoreCase) == 0)
                {
                    output = Enum.Parse(type, fi.Name);
                    break;
                }
            }

            return output;
        }

        public static bool IsStringDefined(Type enumType, string stringValue)
        {
            return Parse(enumType, stringValue) != null;
        }

        public static bool IsStringDefined(Type enumType, string stringValue, bool ignoreCase)
        {
            return Parse(enumType, stringValue, ignoreCase) != null;
        }

        public static IList<StringEnumContainer> GetListValues(Type enumType)
        {
            ValidateAsEnumClass(enumType);
            Type underlyingType = Enum.GetUnderlyingType(enumType);

            List<StringEnumContainer> result = new List<StringEnumContainer>();

            foreach (FieldInfo fi in enumType.GetFields())
            {
                if (fi.FieldType.FullName != underlyingType.FullName)
                {
                    StringEnumContainer ListEntry = new StringEnumContainer();
                    //Check for the StringValueAttribute custom attribute
                    StringValueAttribute[] attrs =
                        fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                    ListEntry.EnumIntvalue = (int)Convert.ChangeType(Enum.Parse(enumType, fi.Name), underlyingType);
                    ListEntry.EnumCodevalue = (Enum)Enum.Parse(enumType, fi.Name);

                    if (attrs != null && attrs.Length > 0)
                        ListEntry.EnumStringvalue = attrs[0].Value;
                    else
                        ListEntry.EnumStringvalue = null; // use the default;

                    result.Add(ListEntry);
                }
            }
            return result;
        }

        public static StringEnumDataTable GetEnumValuesAsDataTable(Type enumType)
        {
            // get the IList first to avoid duplicate logic
            IList<StringEnumContainer> list = StringEnum.GetListValues(enumType);
            // now create a data table to hold the data
            StringEnumDataTable result = new StringEnumDataTable();
            // now loop though all elements in the list inserting them into the table. 
            foreach (StringEnumContainer obj in list)
            {
                result.AddRowFromContainer(obj);
            }
            return result;
        }
    }

    #endregion

    #region Enumerações

    /// <summary>
    /// Separadores de linha, em alternativa à newline.
    /// </summary>
    public enum OutputSeparator
    {
        /// <summary>Linha de caracteres "=".</summary>
        [StringEnum.StringValue("=================================")]
        StrongDashLine,

        /// <summary>Linha de caracteres "-".</summary>
        [StringEnum.StringValue("---------------------------------")]
        LightDashLine,

        /// <summary>Linha em branco.</summary>
        [StringEnum.StringValue(" ")]
        EmptyLine
    }

    #endregion
}
