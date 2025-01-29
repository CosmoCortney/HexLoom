using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using System.IO;

namespace HexLoom
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string docsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HexLoom";

            if (!System.IO.Directory.Exists(docsDir))
                System.IO.Directory.CreateDirectory(docsDir);

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
