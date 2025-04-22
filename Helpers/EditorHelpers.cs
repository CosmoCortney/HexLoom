using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexLoom
{
    class EditorHelpers
    {
        private static void setSinglePrimitiveValue(HexEditor.HexEditor hexEditor, string valueStr, Int32 type, UInt64 offset, bool isBigEndian)
        {
            byte[] value;

            switch (type)
            {
                case (Int32)PrimitiveTypes.SINT8:
                    {
                        value = new byte[1];
                        value[0] = (byte)Helpers.ConvertStringToIntegralType<sbyte>(valueStr);
                    }
                    break;
                case (Int32)PrimitiveTypes.UINT8:
                    {
                        value = new byte[1];
                        value[0] = (byte)Helpers.ConvertStringToIntegralType<byte>(valueStr);
                    }
                    break;
                case (Int32)PrimitiveTypes.SINT16:
                    {
                        value = new byte[2];
                        Int16 temp = 0;
                        temp = Helpers.ConvertStringToIntegralType<Int16>(valueStr);
                        value[0] = (byte)(temp & 0xFF);
                        value[1] = (byte)((temp >> 8) & 0xFF);
                    }
                    break;
                case (Int32)PrimitiveTypes.UINT16:
                    {
                        value = new byte[2];
                        UInt16 temp = 0;
                        temp = Helpers.ConvertStringToIntegralType<UInt16>(valueStr);
                        value[0] = (byte)(temp & 0xFF);
                        value[1] = (byte)((temp >> 8) & 0xFF);
                    }
                    break;
                case (Int32)PrimitiveTypes.SINT32:
                    {
                        value = new byte[4];
                        Int32 temp = 0;
                        temp = Helpers.ConvertStringToIntegralType<Int32>(valueStr);
                        value[0] = (byte)(temp & 0xFF);
                        value[1] = (byte)((temp >> 8) & 0xFF);
                        value[2] = (byte)((temp >> 16) & 0xFF);
                        value[3] = (byte)((temp >> 24) & 0xFF);
                    }
                    break;
                case (Int32)PrimitiveTypes.UINT32:
                    {
                        value = new byte[4];
                        UInt32 temp = 0;
                        temp = Helpers.ConvertStringToIntegralType<UInt32>(valueStr);
                        value[0] = (byte)(temp & 0xFF);
                        value[1] = (byte)((temp >> 8) & 0xFF);
                        value[2] = (byte)((temp >> 16) & 0xFF);
                        value[3] = (byte)((temp >> 24) & 0xFF);
                    }
                    break;
                case (Int32)PrimitiveTypes.SINT64:
                    {
                        value = new byte[8];
                        Int64 temp = 0;
                        temp = Helpers.ConvertStringToIntegralType<Int64>(valueStr);
                        value[0] = (byte)(temp & 0xFF);
                        value[1] = (byte)((temp >> 8) & 0xFF);
                        value[2] = (byte)((temp >> 16) & 0xFF);
                        value[3] = (byte)((temp >> 24) & 0xFF);
                        value[4] = (byte)((temp >> 32) & 0xFF);
                        value[5] = (byte)((temp >> 40) & 0xFF);
                        value[6] = (byte)((temp >> 48) & 0xFF);
                        value[7] = (byte)((temp >> 56) & 0xFF);
                    }
                    break;
                case (Int32)PrimitiveTypes.UINT64:
                    {
                        value = new byte[8];
                        UInt64 temp = 0;
                        temp = Helpers.ConvertStringToIntegralType<UInt64>(valueStr);
                        value[0] = (byte)(temp & 0xFF);
                        value[1] = (byte)((temp >> 8) & 0xFF);
                        value[2] = (byte)((temp >> 16) & 0xFF);
                        value[3] = (byte)((temp >> 24) & 0xFF);
                        value[4] = (byte)((temp >> 32) & 0xFF);
                        value[5] = (byte)((temp >> 40) & 0xFF);
                        value[6] = (byte)((temp >> 48) & 0xFF);
                        value[7] = (byte)((temp >> 56) & 0xFF);
                    }
                    break;
                case (Int32)PrimitiveTypes.FLOAT:
                    {
                        float tempf = 0;
                        tempf = Helpers.ConvertStringToFloatType<float>(valueStr);
                        value = BitConverter.GetBytes(tempf);
                    }
                    break;
                case (Int32)PrimitiveTypes.DOUBLE:
                    {
                        double tempf = 0;
                        tempf = Helpers.ConvertStringToFloatType<double>(valueStr);
                        value = BitConverter.GetBytes(tempf);
                    }
                    break;
                default: //bool
                    {
                        value = new byte[1];
                        value[0] = string.Equals(valueStr, "True", StringComparison.CurrentCultureIgnoreCase) ? (byte)1 : (byte)0;
                    }
                    break;
            }

            if (isBigEndian)
                value = Helpers.ByteSwap(value);

            hexEditor.SetBytes(value, offset);
        }

        public static void SetPrimitiveValues(HexEditor.HexEditor hexEditor, Entity entity, bool isBigEndian)
        {
            if (entity._SecondaryType == (Int32)PrimitiveTypes.BOOL)
            {
                byte[] value = new byte[1];
                value[0] = entity._EntityValueBool ? (byte)1 : (byte)0;
                hexEditor.SetBytes(value, entity._EntityOffset);
            }
            else
                setSinglePrimitiveValue(hexEditor, entity._EntityValue, entity._SecondaryType, entity._EntityOffset, isBigEndian);
        }

        public static void SetArrayValues(HexEditor.HexEditor hexEditor, Entity entity, bool isBigEndian)
        {
            string valueStr = entity._EntityValue.Replace(" ", "");
            var arrays = Helpers.ParseArray(valueStr);
            UInt64 offset = entity._EntityOffset;
            setArrayValuesRecursive(hexEditor, arrays, entity._SecondaryType, ref offset, isBigEndian);
        }

        private static void setArrayValuesRecursive(HexEditor.HexEditor hexEditor, object array, Int32 type, ref UInt64 offset, bool isBigEndian)
        {
            if (array is object[] arr)
            {
                foreach (var element in arr)
                {
                    setArrayValuesRecursive(hexEditor, element, type, ref offset, isBigEndian);
                }
            }
            else if (array is string valueStr)
            {
                setSinglePrimitiveValue(hexEditor, valueStr, type, offset, isBigEndian);
                offset += (UInt64)Helpers.GetPrimitiveTypeSize(type);
            }
        }

        public static void SetColorValue(HexEditor.HexEditor hexEditor, Entity entity, bool isBigEndian)
        {
            Int32 type = entity._SecondaryType;
            string valueStr = Helpers.SanitizeColorString(entity._EntityValue, type);

            switch (entity._SecondaryType)
            {
                case (Int32)ColorTypes.RGBA:
                    {
                        setSinglePrimitiveValue(hexEditor, "0x" + valueStr, (Int32)PrimitiveTypes.UINT32, entity._EntityOffset, isBigEndian);
                    }
                    break;
                case (Int32)ColorTypes.RGBF:
                case (Int32)ColorTypes.RGBAF:
                    {
                        var arrays = Helpers.ParseArray(valueStr);
                        UInt64 offset = entity._EntityOffset;
                        setArrayValuesRecursive(hexEditor, arrays, (Int32)ArrayTypes.FLOAT, ref offset, isBigEndian);
                    }
                    break;
                default: //RGB
                    {
                        byte[] value = new byte[3];
                        value[0] = Convert.ToByte(valueStr.Substring(0, 2), 16);
                        value[1] = Convert.ToByte(valueStr.Substring(2, 2), 16);
                        value[2] = Convert.ToByte(valueStr.Substring(4, 2), 16);
                        hexEditor.SetBytes(value, entity._EntityOffset);
                    }
                    break;
            }
        }

        public static void SetStringValue(HexEditor.HexEditor hexEditor, Entity entity)
        {
            byte[] value = MorphText.ConvertString(entity);
            hexEditor.SetBytes(value, entity._EntityOffset);
        }

        public static void UnsetSingleArrayValue(HexEditor.HexEditor hexEditorOriginal, HexEditor.HexEditor hexEditorEdited, Entity entity)
        {
            byte[] value;
            UInt64 offset = entity._EntityOffset;
            Int32 count = entity._EntityValue.Count(c => c == ',') + 1;

            switch (entity._SecondaryType)
            {
                case (Int32)ArrayTypes.SINT16:
                case (Int32)ArrayTypes.UINT16:
                    {
                        value = hexEditorOriginal.GetBytes(offset, count * 2);
                    }
                    break;
                case (Int32)ArrayTypes.SINT32:
                case (Int32)ArrayTypes.UINT32:
                case (Int32)ArrayTypes.FLOAT:
                    {
                        value = hexEditorOriginal.GetBytes(offset, count * 4);
                    }
                    break;
                case (Int32)ArrayTypes.SINT64:
                case (Int32)ArrayTypes.UINT64:
                case (Int32)ArrayTypes.DOUBLE:
                    {
                        value = hexEditorOriginal.GetBytes(offset, count * 8);
                    }
                    break;
                default: //SINT8, UINT8, BOOL
                    {
                        value = hexEditorOriginal.GetBytes(offset, count);
                    }
                    break;
            }

            hexEditorEdited.SetBytes(value, offset);
        }

        public static void UnsetSingleStringValue(HexEditor.HexEditor hexEditorOriginal, HexEditor.HexEditor hexEditorEdited, Entity entity)
        {
            byte[] value = MorphText.ConvertString(entity);
            value = hexEditorOriginal.GetBytes(entity._EntityOffset, value.Length);
            hexEditorEdited.SetBytes(value, entity._EntityOffset);
        }

        public static void UnsetSinglePrimitiveValue(HexEditor.HexEditor hexEditorOriginal, HexEditor.HexEditor hexEditorEdited, Entity entity)
        {
            byte[] value;
            UInt64 offset = entity._EntityOffset;

            switch (entity._SecondaryType)
            {
                case (Int32)PrimitiveTypes.SINT16:
                case (Int32)PrimitiveTypes.UINT16:
                    {
                        value = hexEditorOriginal.GetBytes(offset, 2);
                    }
                    break;
                case (Int32)PrimitiveTypes.SINT32:
                case (Int32)PrimitiveTypes.UINT32:
                case (Int32)PrimitiveTypes.FLOAT:
                    {
                        value = hexEditorOriginal.GetBytes(offset, 4);
                    }
                    break;
                case (Int32)PrimitiveTypes.SINT64:
                case (Int32)PrimitiveTypes.UINT64:
                case (Int32)PrimitiveTypes.DOUBLE:
                    {
                        value = hexEditorOriginal.GetBytes(offset, 8);
                    }
                    break;
                default: //SINT8, UINT8, BOOL
                    {
                        value = hexEditorOriginal.GetBytes(offset, 1);
                    }
                    break;
            }

            hexEditorEdited.SetBytes(value, offset);
        }

        public static void UnsetSingleColorValue(HexEditor.HexEditor hexEditorOriginal, HexEditor.HexEditor hexEditorEdited, Entity entity)
        {
            byte[] value;
            UInt64 offset = entity._EntityOffset;

            switch (entity._SecondaryType)
            {
                case (Int32)ColorTypes.RGBA:
                    {
                        value = hexEditorOriginal.GetBytes(offset, 4);
                    }
                    break;
                case (Int32)ColorTypes.RGBF:
                    {
                        value = hexEditorOriginal.GetBytes(offset, 12);
                    }
                    break;
                case (Int32)ColorTypes.RGBAF:
                    {
                        value = hexEditorOriginal.GetBytes(offset, 16);
                    }
                    break;
                default: //RGB
                    {
                        value = hexEditorOriginal.GetBytes(offset, 3);
                    }
                    break;
            }

            hexEditorEdited.SetBytes(value, offset);
        }
    }
}
