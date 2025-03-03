using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace HexLoom
{
    internal class Helpers
    {
        [DllImport("MorphText.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)] // wchar_t* to char*
        public static extern IntPtr ConvertWcharStringToCharStringUnsafe(char[] input, int inputEncoding, int outputEncoding);
        [DllImport("MorphText.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)] // wchar_t* to wchar_t*
        public static extern IntPtr ConvertWcharStringToWcharStringUnsafe(char[] input, int inputEncoding, int outputEncoding);

        [DllImport("MorphText.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)] // wchar_t* to char32_t*
        public static extern IntPtr ConvertWcharStringToU32charStringUnsafe(char[] input, int inputEncoding, int outputEncoding);
        [DllImport("MorphText.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemoryCharPtr(IntPtr ptr);

        [DllImport("MorphText.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemoryWcharPtr(IntPtr ptr);

        [DllImport("MorphText.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemoryU32charPtr(IntPtr ptr);

        public static void PanUpdated(ContentView instance, object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    instance.Opacity = 0.5;
                    break;
                case GestureStatus.Running:
                    instance.TranslationY = e.TotalY;
                    break;
                case GestureStatus.Completed:
                    {
                        instance.Opacity = 1;
                        double dragPosY = instance.Frame.Center.Y + instance.TranslationY;
                        instance.TranslationY = 0;
                        var parentStack = instance.Parent as VerticalStackLayout;

                        if (parentStack != null)
                        {
                            var items = parentStack.Children;
                            double itemHeight = parentStack.Height / parentStack.Count;
                            int currentIndex = items.IndexOf(instance);
                            int newIndex = (int)(dragPosY / itemHeight);

                            if (newIndex == currentIndex)
                                return;

                            if (newIndex >= items.Count)
                                newIndex = items.Count - 1;
                            else if (newIndex < 0)
                                newIndex = 0;

                            items.RemoveAt(currentIndex);
                            items.Insert(newIndex, instance);
                        }
                    }
                    break;
            }
        }

        public static int GetByteArrayLength(IntPtr ptr)
        {
            int length = 0;

            while (Marshal.ReadByte(ptr, length) != 0)
                ++length;

            return length;
        }

        public static int GetCharArrayLength(IntPtr ptr)
        {
            int length = 0;

            while (Marshal.ReadInt16(ptr, length * 2) != 0)
                ++length;

            return length;
        }

        public static int GetUIntArrayLength(IntPtr ptr)
        {
            int length = 0;

            while (Marshal.ReadInt32(ptr, length * 4) != 0)
                ++length;

            return length;
        }

        public static void SetWidgetPadding<Widget>(Int32 topLeft, Int32 topRight, Int32 bottomLeft, Int32 bottomRight)
        {
            if (typeof(Widget) == typeof(Entry))
            {
                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is Entry)
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                });
            }
            else if (typeof(Widget) == typeof(Button))
            {
                Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is Microsoft.Maui.Controls.Button)
                    {
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                        handler.PlatformView.Margin = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                        handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                    }
                });
            }
            else if (typeof(Widget) == typeof(Picker))
            {
                Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is Picker)
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                });
            }
            else if(typeof(Widget) == typeof(CheckBox))
            {
                Microsoft.Maui.Handlers.CheckBoxHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is CheckBox)
                    {
                        handler.PlatformView.Content = null;
                        handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
                        handler.PlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 0);
                        handler.PlatformView.Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 0);
                    }
                });
            }
        }

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

            switch(type)
            {
                case (Int32)ColorTypes.RGBF:
                case (Int32)ColorTypes.RGBAF:
                {
                    if(type == (Int32)ColorTypes.RGBF)
                        if (res.Count(f => f == ',') != 2)
                            throw new InvalidDataException($"Ill-formed RGBF color string. Did you ensure to include 3 elements separated by ','?");

                    if(type == (Int32)ColorTypes.RGBAF)
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
                        if(res.Length != 7)
                            throw new InvalidDataException($"Ill-formed RGB color string. Did you ensure to include 3 elements of 2 characters each?");

                    if (type == (Int32)ColorTypes.RGBA)
                        if (res.Length != 9)
                            throw new InvalidDataException($"Ill-formed RGBA color string. Did you ensure to include 4 elements of 2 characters each?");

                    return res.Substring(1);
                }
            }
        }
    }
}
