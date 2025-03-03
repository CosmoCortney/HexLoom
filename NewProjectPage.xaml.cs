using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using CommunityToolkit.Maui.Storage;

namespace HexLoom
{
    public class ProjectSettings
    {
        public string ProjectName { get; set; }
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public UInt64 BaseAddress { get; set; }
        public string ProjectJsonPath { get; set; }
        public bool IsBigEndian { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ProjectName) && !string.IsNullOrEmpty(InputFilePath) && !string.IsNullOrEmpty(OutputFilePath) && !string.IsNullOrEmpty(ProjectJsonPath);
        }
    }

    public partial class NewProjectPage : ContentPage
    {
        public NewProjectPage()
        {
            InitializeComponent();
            _ProjectSettings = new ProjectSettings();
        }

        public NewProjectPage(ProjectSettings projectSettings)
        {
            InitializeComponent();
            _ProjectSettings = projectSettings;
            ProjectName.Text = _ProjectSettings.ProjectName;
            InputBinary.Text = _ProjectSettings.InputFilePath;
            OutputBinary.Text = _ProjectSettings.OutputFilePath;
            BaseAddess.Text = _ProjectSettings.BaseAddress.ToString("X");
        }

        public ProjectSettings _ProjectSettings { get; private set; }

        public event EventHandler<string> DisappearingWithText;

        private async void onOkButtonClicked(object sender, EventArgs e)
        {
            if (! await isValidProjectName())
                return;

            if (! await areFilePathsValid())
                return;

            if(! await parseBaseAddress())
                return;

            await Navigation.PopModalAsync();
        }

        private async Task<bool> parseBaseAddress()
        {
            try
            {
                _ProjectSettings.BaseAddress = Convert.ToUInt64(BaseAddess.Text, 16);
                return true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Base address is ill-formed!\nException thrown: " + ex.Message, "OK");
                _ProjectSettings.BaseAddress = 0;
                return false;
            }
        }

        private async Task<bool> isValidProjectName()
        {
            if (string.IsNullOrEmpty(ProjectName.Text))
            {
                await DisplayAlert("Error", "Project Name mustn't be empty!", "OK");
                return false;
            }

            _ProjectSettings.ProjectName = ProjectName.Text;
            char[] invalidChars = Path.GetInvalidFileNameChars();

            if (_ProjectSettings.ProjectName.Any(c => invalidChars.Contains(c)))
            {
                await DisplayAlert("Error", "Project Name mustn't contain < > : \" / \\ | ? or *", "OK");
                return false;
            }

            _ProjectSettings.ProjectJsonPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HexLoom\\" + _ProjectSettings.ProjectName + ".json";

            if(File.Exists(_ProjectSettings.ProjectJsonPath))
            {
                await DisplayAlert("Error", "Project Name already exists!", "OK");
                _ProjectSettings.ProjectJsonPath = "";
                return false;
            }

            return true;
        }

        private async Task<bool> areFilePathsValid()
        {
            if (string.IsNullOrEmpty(_ProjectSettings.InputFilePath))
            {
                await DisplayAlert("Error", "Input file path mustn't be empty!", "OK");
                return false;
            }

            if (string.IsNullOrEmpty(_ProjectSettings.OutputFilePath))
            {
                await DisplayAlert("Error", "Output file path mustn't be empty!", "OK");
                return false;
            }

            if (_ProjectSettings.InputFilePath.Equals(_ProjectSettings.OutputFilePath))
            {
                await DisplayAlert("Error", "Input and Output file paths must be different!", "OK");
                return false;
            }

            return true;
        }

        private void onOInputPathTextChanged(object sender, EventArgs e)
        {
            _ProjectSettings.InputFilePath = InputBinary.Text;
        }

        private void onOutputPathTextChanged(object sender, EventArgs e)
        {
            _ProjectSettings.OutputFilePath = OutputBinary.Text;
        }

        private async void onCancelButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void onEndiannessPickerIndexChanged(object sender, EventArgs e)
        {
            _ProjectSettings.IsBigEndian = EndiannessPicker.SelectedIndex > 0;
        }

        private async void getInputFilePath(object sender, EventArgs e)
        {
            _ProjectSettings.InputFilePath = "";
            InputBinary.Text = "";

            try
            {
                var result = await FilePicker.Default.PickAsync(PickOptions.Default);

                if (result == null)
                    return;

                _ProjectSettings.InputFilePath = result.FullPath.ToString();
                InputBinary.Text = _ProjectSettings.InputFilePath;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void getOutputFilePath(object sender, EventArgs e)
        {
            _ProjectSettings.OutputFilePath = "";
            OutputBinary.Text = "";

            try
            {
                var resultl = await CommunityToolkit.Maui.Storage.FolderPicker.PickAsync(default);

                if (resultl == null)
                    return;

                string path = resultl.Folder.Path;
                string fileName = await Application.Current.MainPage.DisplayPromptAsync("New File", "Enter desired file name:", "OK", "Cancel", "newfile.bin");

                if (fileName == null)
                    return;

                _ProjectSettings.OutputFilePath = path + '\\' + fileName;
                OutputBinary.Text = _ProjectSettings.OutputFilePath;
                return;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
