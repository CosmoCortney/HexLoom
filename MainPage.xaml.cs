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
    }
}
