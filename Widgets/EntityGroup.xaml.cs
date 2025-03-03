using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexLoom
{
    public partial class EntityGroup : ContentView
    {
        public EntityGroup()
        {
            InitializeComponent();
            Helpers.SetWidgetPadding<Entry>(0, 0, 0, 0);
            Helpers.SetWidgetPadding<Microsoft.Maui.Controls.Button>(10, 2, 10, 2);
        }

        public VerticalStackLayout _EntityStack => this.EntityStack;
        public string _Name => this.EntryGroupName.Text;
        private void onAddItemClicked(object sender, EventArgs e)
        {
            this.EntityStack.Children.Add(new Entity());
        }

        private void onPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            Helpers.PanUpdated(this, sender, e);
        }

        private void onDeleteClicked(object sender, EventArgs e)
        {
            var parentStack = this.Parent as VerticalStackLayout;

            if (parentStack != null)
                parentStack.Children.Remove(this);
        }
    }
}
