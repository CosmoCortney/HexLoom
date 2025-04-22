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
                                    EditorHelpers.SetArrayValues(HexEditorEdited, entity, _projectSettings.IsBigEndian);
                                break;
                                case (Int32)PrimaryTypes.COLOR:
                                    EditorHelpers.SetColorValue(HexEditorEdited, entity, _projectSettings.IsBigEndian);
                                break;
                                case (Int32)PrimaryTypes.STRING:
                                    EditorHelpers.SetStringValue(HexEditorEdited, entity);
                                break;
                                default: //PRIMITIVE
                                    EditorHelpers.SetPrimitiveValues(HexEditorEdited, entity, _projectSettings.IsBigEndian);
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
                                    EditorHelpers.UnsetSingleArrayValue(HexEditorOriginal, HexEditorEdited, entity);
                                break;
                                case (Int32)PrimaryTypes.COLOR:
                                    EditorHelpers.UnsetSingleColorValue(HexEditorOriginal, HexEditorEdited, entity);
                                break;
                                case (Int32)PrimaryTypes.STRING:
                                    EditorHelpers.UnsetSingleStringValue(HexEditorOriginal, HexEditorEdited, entity);
                                break;
                                default: //PRIMITIVE
                                    EditorHelpers.UnsetSinglePrimitiveValue(HexEditorOriginal, HexEditorEdited, entity);
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

        private void onAddGroupClicked(object sender, EventArgs e)
        {
            EntityGroupStack.Children.Add(new EntityGroup());
        }

        private void onGenerateOutputFileClicked(object sender, EventArgs e)
        {
            applyChanges();
            System.IO.File.WriteAllBytes(_projectSettings.OutputFilePath, _binaryDataEdited);
        }
    }
}
