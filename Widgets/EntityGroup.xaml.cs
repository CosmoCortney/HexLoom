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
            WidgetHelpers.SetWidgetPadding<Entry>(0, 0, 0, 0);
            WidgetHelpers.SetWidgetPadding<Microsoft.Maui.Controls.Button>(10, 2, 10, 2);
        }

        public EntityGroup(Newtonsoft.Json.Linq.JObject json) : this()
        {
            _Name = json["GroupName"].ToString();

            foreach (Newtonsoft.Json.Linq.JObject entity in json["Entities"])
            {
                if (entity == null)
                    return;

                _EntityStack.Children.Add(new Entity(entity));
            }
        }
        public VerticalStackLayout _EntityStack => this.EntityStack;
        public string _Name
        {
            get => this.EntryGroupName.Text;
            set => this.EntryGroupName.Text = value;
        }
        private void onAddItemClicked(object sender, EventArgs e)
        {
            this.EntityStack.Children.Add(new Entity());
            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            WidgetHelpers.PanUpdated(this, sender, e);
            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onDeleteClicked(object sender, EventArgs e)
        {
            var parentStack = this.Parent as VerticalStackLayout;

            if (parentStack != null)
                parentStack.Children.Remove(this);

            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onGroupNameTextChanged(object sender, EventArgs e)
        {
            WidgetHelpers.GetMainPage()._ProjectChanged = true;
        }

        private void onCollapseChanged(object sender, EventArgs e)
        {
            this.EntityScrollView.MaximumHeightRequest = this.CheckBoxCollapse.IsChecked ? 25.0 : 200.0;
        }
    }
}
