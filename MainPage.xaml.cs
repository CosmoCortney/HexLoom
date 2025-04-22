using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using HexEditor;
using System.Runtime.InteropServices;

namespace HexLoom
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            MenuItemNewProject.IsEnabled = true;
            MenuItemChangeSettings.IsEnabled = false;
            HexEditorOriginal.SyncTarget = HexEditorEdited;
        }

        private void onAddGroupClicked(object sender, EventArgs e)
        {
            EntityGroupStack.Children.Add(new EntityGroup());
        }

        private void onGenerateOutputFileClicked(object sender, EventArgs e)
        {
            applyChanges();
            System.IO.File.WriteAllBytes(_projectSettings.OutputFilePath, _binaryDataEdited);
        }

        private ProjectSettings _projectSettings;
        private bool _projectOpen = false;
        private bool _projectChanged = false;
        public bool _ProjectChanged
        { 
            get => _projectChanged;
            set
            {
                _projectChanged = value;
                MenuItemSaveChanges.IsEnabled = value;
            }
        }
        private Byte[] _binaryDataOriginal;
        private Byte[] _binaryDataEdited;

        private async void onNewProjectMenuItemClicked(object sender, EventArgs e)
        {
            await openSettingsPage(true);
        }

        private async void onChangeSettingsMenuItemClicked(object sender, EventArgs e)
        {
            await openSettingsPage(false);
        }

        private async Task openSettingsPage(bool newProject)
        {
            var proj = newProject ? new NewProjectPage() : new NewProjectPage(_projectSettings);
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
                    setMenuItemStates(false, true, true, false, true, true);
                    enableListButtons();
                }
            }

            if (sender is Page page)
                page.Disappearing -= OnNewProjectPageDisappearing;

            if ((_projectOpen && _ProjectChanged))
                if (!loadBinary())
                    return;

            setHexEditors();
            Helpers.SetWindowTitle(this.Content.Window, _projectSettings.ProjectName);
            enableListButtons();
        }

        private void onSaveProjectClicked(object sender, EventArgs e)
        {
            if (!_projectOpen || !_ProjectChanged)
                return;

            JObject project = JsonHelpers.SerializeProjectSettings(_projectSettings);
            project["Groups"] = JsonHelpers.SerializeEntityGroups(EntityGroupStack.Children);
            System.IO.File.WriteAllText(_projectSettings.ProjectJsonPath, project.ToString());
            _ProjectChanged = false;
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

            _binaryDataEdited = (Byte[])_binaryDataOriginal.Clone();
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

            Helpers.SetHexEditor(HexEditorOriginal, _binaryDataOriginal, _projectSettings);
            Helpers.SetHexEditor(HexEditorEdited, _binaryDataEdited, _projectSettings);
        }

        private void onApplyClicked(object sender, EventArgs e)
        {
            applyChanges();
        }

        private async void onProjectCloseClicked(object sender, EventArgs e)
        {
            if(_ProjectChanged)
            {
                var result = await DisplayAlert("Warning", "You have unsaved changes. Do you want to save them before closing?", "Yes", "No");

                if (result)
                    onSaveProjectClicked(sender, e);

                _ProjectChanged = false;
            }

            resetEditors();
            resetList();
            Helpers.SetWindowTitle(this.Content.Window, "");
            setMenuItemStates(true, false, false, true, false, false);
        }

        private void resetEditors()
        {
            _binaryDataOriginal = new Byte[0];
            _binaryDataEdited = new Byte[0];
            HexEditorOriginal.Reset();
            HexEditorEdited.Reset();
        }

        private void resetList()
        {
            EntityGroupStack.Children.Clear();
            ButtonAddGroup.IsEnabled = false;
            ButtonApply.IsEnabled = false;
        }

        private void setMenuItemStates(bool newProject, bool changeSettings, bool saveChanges, bool openProject, bool generateFile, bool closeProject)
        {
            MenuItemNewProject.IsEnabled = newProject;
            MenuItemChangeSettings.IsEnabled = changeSettings;
            _ProjectChanged = saveChanges; //also sets MenuItemSaveChanges.IsEnabled
            MenuItemOpenProject.IsEnabled = openProject;
            MenuItemGenerate.IsEnabled = generateFile;
            MenuItemCloseProject.IsEnabled = closeProject;
            _projectOpen = closeProject;
        }

        private async void onOpenProjectClicked(object sender, EventArgs e)
        {
            _projectSettings = new ProjectSettings();
            _binaryDataOriginal = new Byte[0];
            _binaryDataEdited = new Byte[0];

            try
            {
                PickOptions options = new PickOptions
                {
                    PickerTitle = "Select a project file.",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".json" } }
                    })
                };

                var result = await FilePicker.Default.PickAsync(options);

                if (result == null)
                    return;

                Newtonsoft.Json.Linq.JObject project = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(result.FullPath));

                if(project == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid project file.", "OK");
                    return;
                }

                _projectSettings = JsonHelpers.DeSerializeProjectSettings(project);
                _projectSettings.ProjectJsonPath = result.FullPath;
                JsonHelpers.DeSerializeEntityGroups(EntityGroupStack.Children, project);
                setMenuItemStates(false, true, false, false, true, true);

                if (!loadBinary())
                    return;

                setHexEditors();
                Helpers.SetWindowTitle(this.Content.Window, _projectSettings.ProjectName);
                enableListButtons();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void enableListButtons()
        {
            ButtonAddGroup.IsEnabled = true;
            ButtonApply.IsEnabled = true;
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

                    if (entity._Apply)
                    {
                        try
                        {
                            switch (entity._PrimaryType)
                            {
                                case (Int32)PrimaryTypes.ARRAY:
                                    setArrayValues(entity);
                                break;
                                case (Int32)PrimaryTypes.COLOR:
                                    setColorValue(entity);
                                break;
                                case (Int32)PrimaryTypes.STRING:
                                    setStringValue(entity);
                                break;
                                default: //PRIMITIVE
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
                    else
                    {
                        try
                        {
                            switch (entity._PrimaryType)
                            {
                                case (Int32)PrimaryTypes.ARRAY:
                                    unsetSingleArrayValue(entity);
                                break;
                                case (Int32)PrimaryTypes.COLOR:
                                    unsetSingleColorValue(entity);
                                break;
                                case (Int32)PrimaryTypes.STRING:
                                    unsetSingleStringValue(entity);
                                break;
                                default: //PRIMITIVE
                                    unsetSinglePrimitiveValue(entity);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("Error", "Cannot not unset value " + entity._EntityName + ".\nException thrown: " + ex.Message, "OK");
                            return;
                        }
                    }
                }
            }
        }

        private void setPrimitiveValues(Entity entity)
        {
            if(entity._SecondaryType == (Int32)PrimitiveTypes.BOOL)
            {
                byte[] value = new byte[1];
                value[0] = entity._EntityValueBool ? (byte)1 : (byte)0;
                HexEditorEdited.SetBytes(value, entity._EntityOffset);
            }
            else
                setSinglePrimitiveValue(entity._EntityValue, entity._SecondaryType, entity._EntityOffset);
        }

        private void unsetSingleStringValue(Entity entity)
        {
            byte[] value = MorphText.ConvertString(entity);
            value = HexEditorOriginal.GetBytes(entity._EntityOffset, value.Length);
            HexEditorEdited.SetBytes(value, entity._EntityOffset);
        }

        private void unsetSinglePrimitiveValue(Entity entity)
        {
            byte[] value;
            UInt64 offset = entity._EntityOffset;

            switch (entity._SecondaryType)
            {
                case (Int32)PrimitiveTypes.SINT16:
                case (Int32)PrimitiveTypes.UINT16:
                {
                    value = HexEditorOriginal.GetBytes(offset, 2);
                } break;
                case (Int32)PrimitiveTypes.SINT32:
                case (Int32)PrimitiveTypes.UINT32:
                case (Int32)PrimitiveTypes.FLOAT:
                {
                    value = HexEditorOriginal.GetBytes(offset, 4);
                } break;
                case (Int32)PrimitiveTypes.SINT64:
                case (Int32)PrimitiveTypes.UINT64:
                case (Int32)PrimitiveTypes.DOUBLE:
                {
                    value = HexEditorOriginal.GetBytes(offset, 8);
                } break;
                default: //SINT8, UINT8, BOOL
                {
                    value = HexEditorOriginal.GetBytes(offset, 1);
                } break;
            }

            HexEditorEdited.SetBytes(value, offset);
        }

        private void unsetSingleColorValue(Entity entity)
        {
            byte[] value;
            UInt64 offset = entity._EntityOffset;

            switch (entity._SecondaryType)
            {
                case (Int32)ColorTypes.RGBA:
                    {
                        value = HexEditorOriginal.GetBytes(offset, 4);
                    }
                    break;
                case (Int32)ColorTypes.RGBF:
                    {
                        value = HexEditorOriginal.GetBytes(offset, 12);
                    }
                    break;
                case (Int32)ColorTypes.RGBAF:
                    {
                        value = HexEditorOriginal.GetBytes(offset, 16);
                    }
                    break;
                default: //RGB
                    {
                        value = HexEditorOriginal.GetBytes(offset, 3);
                    }
                    break;
            }

            HexEditorEdited.SetBytes(value, offset);
        }

        private void unsetSingleArrayValue(Entity entity)
        {
            byte[] value;
            UInt64 offset = entity._EntityOffset;
            Int32 count = entity._EntityValue.Count(c => c == ',') + 1;

            switch (entity._SecondaryType)
            {
                case (Int32)ArrayTypes.SINT16:
                case (Int32)ArrayTypes.UINT16:
                    {
                        value = HexEditorOriginal.GetBytes(offset, count * 2);
                    }
                    break;
                case (Int32)ArrayTypes.SINT32:
                case (Int32)ArrayTypes.UINT32:
                case (Int32)ArrayTypes.FLOAT:
                    {
                        value = HexEditorOriginal.GetBytes(offset, count * 4);
                    }
                    break;
                case (Int32)ArrayTypes.SINT64:
                case (Int32)ArrayTypes.UINT64:
                case (Int32)ArrayTypes.DOUBLE:
                    {
                        value = HexEditorOriginal.GetBytes(offset, count * 8);
                    }
                    break;
                default: //SINT8, UINT8, BOOL
                    {
                        value = HexEditorOriginal.GetBytes(offset, count);
                    }
                    break;
            }

            HexEditorEdited.SetBytes(value, offset);
        }

        private void setSinglePrimitiveValue(string valueStr, Int32 type, UInt64 offset)
        {
            byte[] value;

            switch (type)
            {
                case (Int32)PrimitiveTypes.SINT8:
                {
                    value = new byte[1];
                    value[0] = (byte)Helpers.ConvertStringToIntegralType<sbyte>(valueStr);
                } break;
                case (Int32)PrimitiveTypes.UINT8:
                {
                    value = new byte[1];
                value[0] = (byte)Helpers.ConvertStringToIntegralType<byte>(valueStr);
                } break;
                case (Int32)PrimitiveTypes.SINT16:
                {
                    value = new byte[2];
                    Int16 temp = 0;
                    temp = Helpers.ConvertStringToIntegralType<Int16>(valueStr);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.UINT16:
                {
                    value = new byte[2];
                    UInt16 temp = 0;
                    temp = Helpers.ConvertStringToIntegralType<UInt16>(valueStr);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.SINT32:
                {
                    value = new byte[4];
                    Int32 temp = 0;
                    temp = Helpers.ConvertStringToIntegralType<Int32>(valueStr);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                    value[2] = (byte)((temp >> 16) & 0xFF);
                    value[3] = (byte)((temp >> 24) & 0xFF);
                } break;
                case (Int32)PrimitiveTypes.UINT32:
                {
                    value = new byte[4];
                    UInt32 temp = 0;
                    temp = Helpers.ConvertStringToIntegralType<UInt32>(valueStr);
                    value[0] = (byte)(temp & 0xFF);
                    value[1] = (byte)((temp >> 8) & 0xFF);
                    value[2] = (byte)((temp >> 16) & 0xFF);
                    value[3] = (byte)((temp >> 24) & 0xFF);
                } break;
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
                } break;
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
                } break;
                case (Int32)PrimitiveTypes.FLOAT:
                {
                    float tempf = 0;
                    tempf = Helpers.ConvertStringToFloatType<float>(valueStr);
                    value = BitConverter.GetBytes(tempf);
                } break;
                case (Int32)PrimitiveTypes.DOUBLE:
                {
                    double tempf = 0;
                    tempf = Helpers.ConvertStringToFloatType<double>(valueStr);
                    value = BitConverter.GetBytes(tempf);
                } break;
                default: //bool
                {
                    value = new byte[1];
                    value[0] = string.Equals(valueStr, "True", StringComparison.CurrentCultureIgnoreCase) ? (byte)1 : (byte)0;
                } break;
            }

            if (_projectSettings.IsBigEndian)
                value = Helpers.ByteSwap(value);

            HexEditorEdited.SetBytes(value, offset);
        }

        private void setArrayValues(Entity entity)
        {
            string valueStr = entity._EntityValue.Replace(" ", "");
            var arrays = Helpers.ParseArray(valueStr);
            UInt64 offset = entity._EntityOffset;
            setArrayValuesRecursive(arrays, entity._SecondaryType, ref offset);
        }

        private void setArrayValuesRecursive(object array, Int32 type, ref UInt64 offset)
        {
            if (array is object[] arr)
            {
                foreach (var element in arr)
                {
                    setArrayValuesRecursive(element, type, ref offset);
                }
            }
            else if (array is string valueStr)
            {
                setSinglePrimitiveValue(valueStr, type, offset);
                offset += (UInt64)Helpers.GetPrimitiveTypeSize(type);
            }
        }

        private void setColorValue(Entity entity)
        {
            Int32 type = entity._SecondaryType;
            string valueStr = Helpers.SanitizeColorString(entity._EntityValue, type);

            switch (entity._SecondaryType)
            {
                case (Int32)ColorTypes.RGBA:
                {
                    setSinglePrimitiveValue("0x" + valueStr, (Int32)PrimitiveTypes.UINT32, entity._EntityOffset);
                } break;
                case (Int32)ColorTypes.RGBF:
                case (Int32)ColorTypes.RGBAF:
                {
                    var arrays = Helpers.ParseArray(valueStr);
                    UInt64 offset = entity._EntityOffset;
                    setArrayValuesRecursive(arrays, (Int32)ArrayTypes.FLOAT, ref offset);
                } break;
                default: //RGB
                {
                    byte[] value = new byte[3];
                    value[0] = Convert.ToByte(valueStr.Substring(0, 2), 16);
                    value[1] = Convert.ToByte(valueStr.Substring(2, 2), 16);
                    value[2] = Convert.ToByte(valueStr.Substring(4, 2), 16);
                    HexEditorEdited.SetBytes(value, entity._EntityOffset);
                } break;
            }
        }

        private void setStringValue(Entity entity)
        {
            byte[] value = MorphText.ConvertString(entity);
            HexEditorEdited.SetBytes(value, entity._EntityOffset);
        }
    }
}
