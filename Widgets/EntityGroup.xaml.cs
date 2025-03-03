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
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    this.Opacity = 0.5;
                    break;
                case GestureStatus.Running:
                    this.TranslationY = e.TotalY;
                    break;
                case GestureStatus.Completed:
                {
                    this.Opacity = 1;
                    this.TranslationY = 0;
                    var parentStack = this.Parent as VerticalStackLayout;

                    if (parentStack != null)
                    {
                        var items = parentStack.Children;
                        int currentIndex = items.IndexOf(this);
                        int newIndex = (int)(e.TotalY / this.Height);
                        newIndex = Math.Clamp(newIndex, 0, items.Count - 1);

                        if (newIndex != currentIndex)
                        {
                            items.RemoveAt(currentIndex);
                            items.Insert(newIndex, this);
                        }
                    }
                } break;
            }
        }

        private void onDeleteClicked(object sender, EventArgs e)
        {
            var parentStack = this.Parent as VerticalStackLayout;

            if (parentStack != null)
                parentStack.Children.Remove(this);
        }
    }
}
