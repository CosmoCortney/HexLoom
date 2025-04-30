using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
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

            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("--cmd"))
            {
                RunCommandLineMode(args);
                Environment.Exit(0);
            }

            return builder.Build();
        }

        private static void RunCommandLineMode(string[] args)
        {
            Console.WriteLine("Running in CMD mode...");
            List<string> argList = new List<string>(args);
            String jsonPath = new String("");
            bool hasJsonPath = false;
            argList.ForEach(arg => { 
                
                if (arg.Contains(".json"))
                    jsonPath = arg;
            });

            if (jsonPath == null || jsonPath.Length == 0)
            {
                Console.WriteLine("Improper arguments. Exiting...");
                Environment.Exit(0);
            }

            HexEditor.HexEditor hexEditor = new HexEditor.HexEditor();
            Newtonsoft.Json.Linq.JObject project = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(jsonPath));
            ProjectSettings projectSettings = JsonHelpers.DeSerializeProjectSettings(project);
            Byte[] binaryData = new Byte[0];

            binaryData = System.IO.File.ReadAllBytes(projectSettings.InputFilePath);

            if (binaryData == null)
                return;

            if (binaryData.Length == 0)
                return;

            hexEditor.SetBinaryData(binaryData);
            hexEditor.SetBaseAddress(projectSettings.BaseAddress);
            hexEditor._IsBigEndian = projectSettings.IsBigEndian;
            EditorHelpers.ApplyFromJson(hexEditor, project);
            System.IO.File.WriteAllBytes(projectSettings.OutputFilePath, binaryData);
        }
    }
}
