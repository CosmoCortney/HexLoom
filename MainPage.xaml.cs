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
        }
    }
}
