using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace HexLoom
{
    internal class MorphText
    {
        [DllImport("dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)] // wchar_t* to char*
        public static extern IntPtr ConvertWcharStringToCharStringUnsafe(char[] input, int inputEncoding, int outputEncoding);
        [DllImport("dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)] // wchar_t* to wchar_t*
        public static extern IntPtr ConvertWcharStringToWcharStringUnsafe(char[] input, int inputEncoding, int outputEncoding);

        [DllImport("dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)] // wchar_t* to char32_t*
        public static extern IntPtr ConvertWcharStringToU32charStringUnsafe(char[] input, int inputEncoding, int outputEncoding);
        [DllImport("dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemoryCharPtr(IntPtr ptr);

        [DllImport("dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemoryWcharPtr(IntPtr ptr);

        [DllImport("dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeMemoryU32charPtr(IntPtr ptr);

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

        public static byte[] ConvertString(Entity entity)
        {
            Int32 type = entity._SecondaryType;
            byte[] value;

            switch (type)
            {
                case (Int32)StringTypes.UTF16LE:
                case (Int32)StringTypes.UTF16BE:
                {
                    IntPtr resultPtr = ConvertWcharStringToWcharStringUnsafe(entity._EntityValue.ToCharArray(), (Int32)StringTypes.UTF16LE, type);
                    int length = GetCharArrayLength(resultPtr);
                    value = new byte[length * 2];
                    Marshal.Copy(resultPtr, value, 0, length * 2);
                    FreeMemoryWcharPtr(resultPtr);
                } break;
                case (Int32)StringTypes.UTF32LE:
                case (Int32)StringTypes.UTF32BE:
                {
                    IntPtr resultPtr = ConvertWcharStringToU32charStringUnsafe(entity._EntityValue.ToCharArray(), (Int32)StringTypes.UTF16LE, type);
                    int length = GetUIntArrayLength(resultPtr);
                    value = new byte[length * 4];
                    Marshal.Copy(resultPtr, value, 0, length * 4);
                    FreeMemoryU32charPtr(resultPtr);
                } break;
                default: //single and variable byte char strings
                {
                    IntPtr resultPtr = ConvertWcharStringToCharStringUnsafe(entity._EntityValue.ToCharArray(), (Int32)StringTypes.UTF16LE, type);
                    int length = GetByteArrayLength(resultPtr);
                    value = new byte[length];
                    Marshal.Copy(resultPtr, value, 0, length);
                    FreeMemoryCharPtr(resultPtr);
                } break;
            }

            return value;
        }
    }
}
