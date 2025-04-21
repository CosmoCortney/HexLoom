using System.Runtime.InteropServices;

namespace HexLoom
{
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, Int32 cmdShow);

        public App()
        {
            InitializeComponent();

            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();
                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                ShowWindow(windowHandle, 3);
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());
            Helpers.SetWindowTitle(window, "");
            return window;
        }
    }
}