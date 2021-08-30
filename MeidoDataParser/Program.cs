using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MeidoDataParser
{
    public class UpdateFile
    {
        public string fileName;
        public string updateFileName;
        public long size;
        public string hash;
        public int version;
    }

    public class UpdatePack
    {
        public string name;
        public string company;
        public string appName;
        public string appExe;
        public string category;
        public string registry;
        public List<UpdateFile> files;
    }

    public class ProgramSettings
    {
        public string filter;
        public bool outputMini;
        public bool outputFull;
        public string searchDir;
        public bool noDLC;
    }

    class Program
    {
        static List<UpdatePack> packs = new List<UpdatePack>();

        static ProgramSettings settings = null;

        static void Main(string[] args)
        {
            try { settings = JsonConvert.DeserializeObject<ProgramSettings>(File.ReadAllText("config.json")); }
            catch(FileNotFoundException)
            {
                Console.WriteLine("HALT: No config.");
                Environment.Exit(1);
            }
            
            ReadData(settings.filter);

            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string json = JsonConvert.SerializeObject(packs, Formatting.Indented, jsonSettings);
            string jsonMin = JsonConvert.SerializeObject(packs, Formatting.None, jsonSettings);

            //Console.WriteLine(jsonIgnoreNullValues);
            File.WriteAllText(Path.Combine(settings.searchDir, @$"kiss{(string.IsNullOrEmpty(settings.filter)?"":$"-{settings.filter}")}.json"), json);
            File.WriteAllText(Path.Combine(settings.searchDir, @$"kiss{(string.IsNullOrEmpty(settings.filter)?"":$"-{settings.filter}")}.min.json"), jsonMin);
        }

        static void ReadData(string filter)
        {
            foreach (string file in Directory.EnumerateFiles(settings.searchDir, "*.lst", SearchOption.AllDirectories))
            {
                if (!string.IsNullOrEmpty(filter) && !file.Contains(filter)) continue;
                if (settings.noDLC && file.Contains(Path.Combine(settings.searchDir, "DLC"))) continue;

                var pack = new UpdatePack { name = Path.GetFileName(Path.GetDirectoryName(file)) };

                ParseIni(pack, Directory.GetFiles(Path.GetDirectoryName(file), "*.ini")[0]);
                ParseLst(pack, file);

                packs.Add(pack);
            }
        }

        static void ParseIni(UpdatePack pack, string filename)
        {
            var ini = new Ini(filename);
            pack.company = ini.GetValue("Company", "UPDATER", null);
            pack.appName = ini.GetValue("AppName", "UPDATER", null);
            pack.appExe = ini.GetValue("AppExe", "UPDATER", null);
            pack.registry = ini.GetValue("Registry1", "UPDATER", null);
            pack.category = ini.GetValue("Category", "UPDATER", null);
        }

        static void ParseLst(UpdatePack pack, string filename)
        {
            var files = File.ReadAllLines(filename);

            var fileList = new List<UpdateFile>();

            foreach (var fileDef in files)
            {
                if(string.IsNullOrEmpty(fileDef)) continue;
                var file = new UpdateFile();
                string[] args = fileDef.Split(',');
                file.fileName = args[2];
                file.updateFileName = (args[0] == "share" && args[1] != "0") ? args[1] : null;
                file.size = long.Parse(args[3]);
                file.hash = args[4];
                file.version = int.Parse(args[5]);

                fileList.Add(file);
            }

            pack.files = fileList;
        }
    }
}
