namespace HexLoom
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void onAddGroupClicked(object sender, EventArgs e)
        {
            EntityGroupStack.Children.Add(new EntityGroup());
        }
    }
}
