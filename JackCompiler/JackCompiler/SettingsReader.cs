using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    public static class SettingsReader
    {
        public static readonly string _appSettingsPath;


        static SettingsReader()
        {
            _appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), @"App_Data\appsettings.json");
        }

        public static AppSettings AppSettings
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_appSettingsPath))
                {
                    using (StreamReader reader = new StreamReader(_appSettingsPath))
                    {
                        string jsonContent = reader.ReadToEnd();
                        var appSettings = JsonConvert.DeserializeObject<AppSettings>(jsonContent);
                        return appSettings;
                    }
                }
                return null;
            }
        }


        public static List<string> Keywords
        {
            get
            {
                if (AppSettings != null)
                {
                    return AppSettings.Keywords;
                }
                
                return null;
            }
        }

        public static List<char> Symbols
        {
            get
            {
                if(AppSettings != null)
                {
                    return AppSettings.Symbols;
                }
                return null;
            }
        }

    }
}
