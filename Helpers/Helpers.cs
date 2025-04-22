using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace HexLoom
{
    internal class Helpers
    {
        public static byte[] ByteSwap(byte[] input)
        {
            byte[] output = new byte[input.Length];

            for (int i = 0; i < input.Length; ++i)
                output[i] = input[input.Length - i - 1];

            return output;
        }

        public static int GetPrimitiveTypeSize(Int32 type)
        {
            return type switch
            {
                (Int32)PrimitiveTypes.SINT8 => sizeof(sbyte),
                (Int32)PrimitiveTypes.UINT8 => sizeof(byte),
                (Int32)PrimitiveTypes.SINT16 => sizeof(Int16),
                (Int32)PrimitiveTypes.UINT16 => sizeof(UInt16),
                (Int32)PrimitiveTypes.SINT32 => sizeof(Int32),
                (Int32)PrimitiveTypes.UINT32 => sizeof(UInt32),
                (Int32)PrimitiveTypes.SINT64 => sizeof(Int64),
                (Int32)PrimitiveTypes.UINT64 => sizeof(UInt64),
                (Int32)PrimitiveTypes.FLOAT => sizeof(float),
                (Int32)PrimitiveTypes.DOUBLE => sizeof(double),
                _ => throw new InvalidOperationException($"Unsupported type: {type}"),
            };
        }
        public static object ParseArray(string arrayStr)
        {
            if (arrayStr.StartsWith("[") && arrayStr.EndsWith("]"))
            {
                arrayStr = arrayStr.Substring(1, arrayStr.Length - 2);
                var elements = new List<object>();
                int bracketCount = 0;
                int startIndex = 0;

                for (int i = 0; i < arrayStr.Length; i++)
                {
                    if (arrayStr[i] == '[') bracketCount++;
                    if (arrayStr[i] == ']') bracketCount--;
                    if (arrayStr[i] == ',' && bracketCount == 0)
                    {
                        elements.Add(ParseArray(arrayStr.Substring(startIndex, i - startIndex)));
                        startIndex = i + 1;
                    }
                }

                if (startIndex < arrayStr.Length)
                    elements.Add(ParseArray(arrayStr.Substring(startIndex)));

                return elements.ToArray();
            }
            else
                return arrayStr.Trim();
        }

        public static T ConvertStringToIntegralType<T>(string input) where T : IConvertible
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string cannot be null or empty.");

            Type targetType = typeof(T);
            Int64 value;

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                value = Convert.ToInt64(input.Substring(2), 16);
            else
                value = Convert.ToInt64(input, 10);

            if (targetType == typeof(sbyte))
                return (T)(object)(sbyte)value;
            else if (targetType == typeof(byte))
                return (T)(object)(byte)value;
            else if (targetType == typeof(Int16))
                return (T)(object)(short)value;
            else if (targetType == typeof(UInt16))
                return (T)(object)(UInt16)value;
            else if (targetType == typeof(Int32))
                return (T)(object)(Int32)value;
            else if (targetType == typeof(UInt32))
                return (T)(object)(UInt32)value;
            else if (targetType == typeof(Int64))
                return (T)(object)value;
            else if (targetType == typeof(UInt64))
                return (T)(object)(UInt64)value;
            else
                throw new InvalidOperationException($"Unsupported target type: {targetType}");
        }

        public static T ConvertStringToFloatType<T>(string input) where T : IConvertible
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string cannot be null or empty.");

            Type targetType = typeof(T);

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (targetType == typeof(float))
                {
                    UInt32 intValue = Convert.ToUInt32(input.Substring(2), 16);
                    float floatValue = BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0);
                    return (T)(object)floatValue;
                }
                else if (targetType == typeof(double))
                {
                    UInt64 longValue = Convert.ToUInt64(input.Substring(2), 16);
                    double doubleValue = BitConverter.ToDouble(BitConverter.GetBytes(longValue), 0);
                    return (T)(object)doubleValue;
                }
                else
                    throw new InvalidOperationException($"Unsupported target type: {targetType}");
            }
            else
            {
                input = input.Replace(',', '.');
                var cultureInfo = System.Globalization.CultureInfo.InvariantCulture;
                var numberStyles = System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands;

                if (targetType == typeof(float))
                    return (T)(object)float.Parse(input, numberStyles, cultureInfo);
                else if (targetType == typeof(double))
                    return (T)(object)double.Parse(input, numberStyles, cultureInfo);
                else
                    throw new InvalidOperationException($"Unsupported target type: {targetType}");
            }
        }

        public static string SanitizeColorString(string value, Int32 type)
        {
            string res = value.Replace(" ", "");

            switch (type)
            {
                case (Int32)ColorTypes.RGBF:
                case (Int32)ColorTypes.RGBAF:
                    {
                        if (type == (Int32)ColorTypes.RGBF)
                            if (res.Count(f => f == ',') != 2)
                                throw new InvalidDataException($"Ill-formed RGBF color string. Did you ensure to include 3 elements separated by ','?");

                        if (type == (Int32)ColorTypes.RGBAF)
                            if (res.Count(f => f == ',') != 3)
                                throw new InvalidDataException($"Ill-formed RGBFA color string. Did you ensure to include 4 elements separated by ','?");

                        if (!res.StartsWith("[") || !res.EndsWith("]"))
                            throw new InvalidDataException($"Ill-formed RGBF(A) color string. Did you ensure to prepand '[' and append ']'?");

                        return res;
                    }
                default: //RGB, RGBA
                    {
                        if (!res.StartsWith("#"))
                            throw new InvalidDataException($"Ill-formed RGB(A) color string. Did you forget to prepend '#'?");

                        if (type == (Int32)ColorTypes.RGB)
                            if (res.Length != 7)
                                throw new InvalidDataException($"Ill-formed RGB color string. Did you ensure to include 3 elements of 2 characters each?");

                        if (type == (Int32)ColorTypes.RGBA)
                            if (res.Length != 9)
                                throw new InvalidDataException($"Ill-formed RGBA color string. Did you ensure to include 4 elements of 2 characters each?");

                        return res.Substring(1);
                    }
            }
        }

        public static void SetPickerIndexByIs(Picker picker, Int32 id)
        {
            for (Int32 i = 0; i < picker.ItemsSource.Count; ++i)
            {
                if (((PickerItem)picker.ItemsSource[i]).Id == id)
                {
                    picker.SelectedIndex = i;
                    break;
                }
            }
        }

        public static void SetWindowTitle(Window window, string title)
        {
            if (window == null)
                return;

            string versionString = AppInfo.VersionString;
            window.Title = AppInfo.Name + " v" + (versionString.Count(c => c == '.') == 3 ? versionString.Substring(0, versionString.LastIndexOfAny(new char[] { '.' })) : versionString);

            if (title == null)
                return;

            if (title.Length == 0)
                return;

            window.Title += " - " + title;
        }

        public static JObject SerializeProjectSettings(ProjectSettings settings)
        {
            JObject project = new JObject();
            project["ProjectName"] = settings.ProjectName;
            project["InputFilePath"] = settings.InputFilePath;
            project["OutputFilePath"] = settings.OutputFilePath;
            project["BaseAddress"] = settings.BaseAddress;
            project["IsBigEndian"] = settings.IsBigEndian;
            return project;
        }

        public static JArray SerializeEntities(EntityGroup entityGroup)
        {
            var entityArr = new JArray();

            foreach (var entityS in entityGroup._EntityStack.Children)
            {
                if (entityS is not Entity)
                    return entityArr;

                var entity = entityS as Entity;
                var entityObj = new JObject();
                entityObj["EntityName"] = entity._EntityName;
                entityObj["PrimaryType"] = entity._PrimaryType;
                entityObj["SecondaryType"] = entity._SecondaryType;
                entityObj["Offset"] = entity._EntityOffset;
                entityObj["Apply"] = entity._Apply;

                if (entity._PrimaryType == (Int32)PrimaryTypes.PRIMITIVE && entity._SecondaryType == (Int32)PrimitiveTypes.BOOL)
                    entityObj["Value"] = entity._EntityValueBool;
                else
                    entityObj["Value"] = entity._EntityValue;

                entityArr.Add(entityObj);
            }

            return entityArr;
        }

        public static JArray SerializeEntityGroups(System.Collections.Generic.IList<IView> children)
        {
            var groupArr = new JArray();

            foreach (var groupS in children)
            {
                if (groupS is not EntityGroup)
                    return groupArr;

                var entityGroup = groupS as EntityGroup;
                var groupObj = new JObject();
                groupObj["GroupName"] = entityGroup._Name;
                groupObj["Collapse"] = entityGroup._Collapse;
                var entityArr = Helpers.SerializeEntities(entityGroup);
                groupObj["Entities"] = entityArr;
                groupArr.Add(groupObj);
            }

            return groupArr;
        }

        public static void SetHexEditor(HexEditor.HexEditor hexEditor, Byte[] data, ProjectSettings settings)
        {
            if(hexEditor == null)
                return;

            hexEditor.SetBinaryData(data);
            hexEditor.SetBaseAddress(settings.BaseAddress);
            hexEditor._IsBigEndian = settings.IsBigEndian;
        }
    }
}
