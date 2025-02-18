using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using HexEditor;

namespace HexLoom
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            MenuItemNewProject.IsEnabled = true;
            MenuItemChangeSettings.IsEnabled = false;
            setTestData();
        }

        private void onAddGroupClicked(object sender, EventArgs e)
        {
            EntityGroupStack.Children.Add(new EntityGroup());
        }

        private ProjectSettings _projectSettings;
        private bool _projectOpen = false;
        private bool _projectChanged = false;
        private Byte[] _binaryDataOriginal;
        private Byte[] _binaryDataEdited;

        private async void onNewProjectMenuItemClicked(object sender, EventArgs e)
        {
            var proj = new NewProjectPage();
            proj.Disappearing += OnNewProjectPageDisappearing;
            await Navigation.PushModalAsync(proj);
        }
        private async void onChangeSettingsMenuItemClicked(object sender, EventArgs e)
        {
            var proj = new NewProjectPage(_projectSettings);
            proj.Disappearing += OnNewProjectPageDisappearing;
            await Navigation.PushModalAsync(proj);
        }

        private void OnNewProjectPageDisappearing(object sender, EventArgs e)
        {
            if (sender is NewProjectPage newProjectPage)
            {
                _projectSettings = newProjectPage._ProjectSettings;

                if (_projectSettings.IsValid())
                {
                    _projectOpen = true;
                    _projectChanged = true;
                    MenuItemNewProject.IsEnabled = false;
                    MenuItemChangeSettings.IsEnabled = true;
                    MenuItemSaveChanges.IsEnabled = true;
                    MenuItemCloseProject.IsEnabled = true;
                    this.Content.Window.Title = "HexLoom - " + _projectSettings.ProjectName;
                }
            }

            if (sender is Page page)
                page.Disappearing -= OnNewProjectPageDisappearing;

            if ((_projectOpen && _projectChanged))
                loadBinary();
        }

        private void onSaveProjectClocked(object sender, EventArgs e)
        {
            if (!_projectOpen || !_projectChanged)
                return;

            JObject project = new JObject();
            project["ProjectName"] = _projectSettings.ProjectName;
            project["InputFilePath"] = _projectSettings.InputFilePath;
            project["OutputFilePath"] = _projectSettings.OutputFilePath;
            project["BaseAddress"] = _projectSettings.BaseAddress;
            var groupArr = new JArray();

            foreach (var groupS in EntityGroupStack.Children)
            {
                if (groupS is not EntityGroup)
                    return;

                var entityGroup = groupS as EntityGroup;

                var groupObj = new JObject();
                groupObj["GroupName"] = entityGroup._Name;
                var entityArr = new JArray();

                foreach (var entityS in entityGroup._EntityStack.Children)
                {
                    if (entityS is not Entity)
                        return;

                    var entity = entityS as Entity;
                    var entityObj = new JObject();
                    entityObj["EntityName"] = entity._EntityName;
                    entityObj["PrimaryType"] = entity._PrimaryType;
                    entityObj["SecondaryType"] = entity._SecondaryType;
                    entityObj["Offset"] = entity._EntityOffset;

                    if(entity._PrimaryType == (Int32)PrimaryTypes.PRIMITIVE && entity._SecondaryType == (Int32)PrimitiveTypes.BOOL)
                        entityObj["Value"] = entity._EntityValueBool;
                    else
                        entityObj["Value"] = entity._EntityValue;

                    entityArr.Add(entityObj);
                }

                groupObj["Entities"] = entityArr;
                groupArr.Add(groupObj);
            }

            project["Groups"] = groupArr;
            System.IO.File.WriteAllText(_projectSettings.ProjectJsonPath, project.ToString());
            _projectChanged = false;
        }

        private bool loadBinary()
        {
            _binaryDataOriginal = new Byte[0];
            _binaryDataEdited = new Byte[0];

            if (!_projectOpen)
                return false;

            _binaryDataOriginal = System.IO.File.ReadAllBytes(_projectSettings.InputFilePath);

            if (_binaryDataOriginal == null)
                return false;

            if (_binaryDataOriginal.Length == 0)
                return false;

            _binaryDataEdited = _binaryDataOriginal;
            setHexEditors();
            return true;
        }

        private void setHexEditors()
        {
            if (!_projectOpen)
                return;

            if (_binaryDataOriginal == null || _binaryDataEdited == null)
                return;

            if (_binaryDataOriginal.Length == 0 || _binaryDataEdited.Length == 0)
                return;

            HexEditorOriginal.SetBinaryData(_binaryDataOriginal);
            HexEditorOriginal.SetBaseAddress(_projectSettings.BaseAddress);
            HexEditorOriginal._IsBigEndian = _projectSettings.IsBigEndian;
            HexEditorEdited.SetBinaryData(_binaryDataEdited);
            HexEditorEdited.SetBaseAddress(_projectSettings.BaseAddress);
            HexEditorEdited._IsBigEndian = _projectSettings.IsBigEndian;
        }

        private void onApplyClicked(object sender, EventArgs e)
        {
            applyChanges();
        }

        private async void applyChanges()
        {
            foreach (var groupS in EntityGroupStack.Children)
            {
                if (groupS is not EntityGroup)
                    return;

                var entityGroup = groupS as EntityGroup;

                foreach (var entityS in entityGroup._EntityStack.Children)
                {
                    if (entityS is not Entity)
                        return;

                    var entity = entityS as Entity;

                    if (!entity._Apply)
                        continue;

                    try
                    {
                        switch (entity._PrimaryType)
                        {
                            default:
                                setPrimitiveValues(entity);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", "Ill-formed value in " + entity._EntityName + ".\nException thrown: " + ex.Message, "OK");
                        return;
                    }
                }
            }
        }

        private void setPrimitiveValues(Entity entity)
        {
            byte[] value = new byte[0];

            switch((Int32)entity._SecondaryType)
            {
                case (Int32)PrimitiveTypes.SINT8:
                    value = new byte[1];
                    value[0] = (byte)convertStringToIntegralType<sbyte>(entity._EntityValue);
                break;
                case (Int32)PrimitiveTypes.UINT8:
                    value = new byte[1];
                    value[0] = (byte)convertStringToIntegralType<byte>(entity._EntityValue);
                break;
                case (Int32)PrimitiveTypes.SINT16:
                {
                    value = new byte[2];
                    Int16 temp = 0;
                    temp = convertStringToIntegralType<Int16>(entity._EntityValue);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.UINT16:
                {
                    value = new byte[2];
                    UInt16 temp = 0;
                    temp = convertStringToIntegralType<UInt16>(entity._EntityValue);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.SINT32:
                {
                    value = new byte[4];
                    Int32 temp = 0;
                    temp = convertStringToIntegralType<Int32>(entity._EntityValue);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                    value[2] = (byte)((temp >> 16) & 0xFF);
                    value[3] = (byte)((temp >> 24) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.UINT32:
                {
                    value = new byte[4];
                    UInt32 temp = 0;
                    temp = convertStringToIntegralType<UInt32>(entity._EntityValue);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                    value[2] = (byte)((temp >> 16) & 0xFF);
                    value[3] = (byte)((temp >> 24) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.SINT64:
                {
                    value = new byte[8];
                    Int64 temp = 0;
                    temp = convertStringToIntegralType<Int64>(entity._EntityValue);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                    value[2] = (byte)((temp >> 16) & 0xFF);
                    value[3] = (byte)((temp >> 24) & 0xFF);
                    value[4] = (byte)((temp >> 32) & 0xFF);
                    value[5] = (byte)((temp >> 40) & 0xFF);
                    value[6] = (byte)((temp >> 48) & 0xFF);
                    value[7] = (byte)((temp >> 56) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.UINT64:
                {
                    value = new byte[8];
                    UInt64 temp = 0;
                    temp = convertStringToIntegralType<UInt64>(entity._EntityValue);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                    value[2] = (byte)((temp >> 16) & 0xFF);
                    value[3] = (byte)((temp >> 24) & 0xFF);
                    value[4] = (byte)((temp >> 32) & 0xFF);
                    value[5] = (byte)((temp >> 40) & 0xFF);
                    value[6] = (byte)((temp >> 48) & 0xFF);
                    value[7] = (byte)((temp >> 56) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.FLOAT:
                {
                    float tempf = 0;
                    tempf = convertStringToFloatType<float>(entity._EntityValue);
                    value = BitConverter.GetBytes(tempf);
                } break;
                case (Int32)PrimitiveTypes.DOUBLE:
                {
                    double tempf = 0;
                    tempf = convertStringToFloatType<double>(entity._EntityValue);
                    value = BitConverter.GetBytes(tempf);
                } break;
                default: //bool
                    value = new byte[1];
                    value[0] = entity._EntityValueBool ? (byte)1 : (byte)0;
                break;
            }
           
            if(_projectSettings.IsBigEndian)
                value = byteSwap(value);

            HexEditorEdited.SetBytes(value, entity._EntityOffset);
        }

        private byte[] byteSwap(byte[] input)
        {
            byte[] output = new byte[input.Length];

            for (int i = 0; i < input.Length; ++i)
                output[i] = input[input.Length - i - 1];

            return output;
        }

        private static T convertStringToIntegralType<T>(string input) where T : IConvertible
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string cannot be null or empty.");

            Type targetType = typeof(T);
            Int64 value;

            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = Convert.ToInt64(input.Substring(2), 16);
            }
            else
            {
                value = Convert.ToInt64(input, 10);
            }

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

        private static T convertStringToFloatType<T>(string input) where T : IConvertible
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
                {
                    return (T)(object)float.Parse(input, numberStyles, cultureInfo);
                }
                else if (targetType == typeof(double))
                {
                    return (T)(object)double.Parse(input, numberStyles, cultureInfo);
                }
                else
                    throw new InvalidOperationException($"Unsupported target type: {targetType}");
            }
        }

        private void setTestData()
        {
            _projectSettings = new ProjectSettings();
            _projectSettings.ProjectName = "Test Project";
            _projectSettings.InputFilePath = "C:\\Users\\s_sch\\Documents\\line(jpn)__,lz.rel";
            _projectSettings.OutputFilePath = "C:\\Users\\s_sch\\Documents\\testEdited.bin";
            _projectSettings.ProjectJsonPath = "C:\\Users\\s_sch\\Documents\\HexLoom\\Test Project.json";
            _projectSettings.IsBigEndian = true;
            _projectSettings.BaseAddress = 0;
            _projectOpen = true;
            loadBinary();
            setHexEditors();
        }
    }
}
