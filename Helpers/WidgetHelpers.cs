using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexLoom
{
    internal class WidgetHelpers
    {
        public static void PanUpdated(ContentView instance, object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    instance.Opacity = 0.5;
                    break;
                case GestureStatus.Running:
                    instance.TranslationY = e.TotalY;
                    break;
                case GestureStatus.Completed:
                {
                    instance.Opacity = 1;
                    double dragPosY = instance.Frame.Center.Y + instance.TranslationY;
                    instance.TranslationY = 0;
                    var parentStack = instance.Parent as VerticalStackLayout;

                    if (parentStack != null)
                    {
                        var items = parentStack.Children;
                        double itemHeight = parentStack.Height / parentStack.Count;
                        int currentIndex = items.IndexOf(instance);
                        int newIndex = (int)(dragPosY / itemHeight);

                        if (newIndex == currentIndex)
                            return;

                        if (newIndex >= items.Count)
                            newIndex = items.Count - 1;
                        else if (newIndex < 0)
                            newIndex = 0;

                        items.RemoveAt(currentIndex);
                        items.Insert(newIndex, instance);
                    }
                } break;
            }
        }

        public static void SetWidgetPadding<Widget>(Int32 topLeft, Int32 topRight, Int32 bottomLeft, Int32 bottomRight)
        {
            if (typeof(Widget) == typeof(Entry))
            {
                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is Entry)
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                });
            }
            else if (typeof(Widget) == typeof(Button))
            {
                Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is Microsoft.Maui.Controls.Button)
                    {
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                        handler.PlatformView.Margin = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                        handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
                    }
                });
            }
            else if (typeof(Widget) == typeof(Picker))
            {
                Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is Picker)
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(topLeft, topRight, bottomLeft, bottomRight);
                });
            }
            else if (typeof(Widget) == typeof(CheckBox))
            {
                Microsoft.Maui.Handlers.CheckBoxHandler.Mapper.AppendToMapping("", (handler, view) =>
                {
                    if (view is CheckBox)
                    {
                        handler.PlatformView.Content = null;
                        handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
                        handler.PlatformView.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                        handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 0);
                        handler.PlatformView.Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 0);
                    }
                });
            }
        }
    }
}
