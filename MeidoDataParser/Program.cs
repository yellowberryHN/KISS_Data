using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeidoDataParser
{
    public class GameInfo
    {
        public string game { get; set; }
        
        public List<FileInfo> files { get; set; }
    }
    
    public class FileInfo
    {
        public string filepath { get; set; }
        
        public List<UpdateFile> updates { get; set; }
    }
    
    public class UpdateFile
    {
        // file size of the update file
        public long size { get; set; }
        
        // file hash of update file
        public string hash { get; set; }
        
        // the internal version number
        public int version { get; set; }
        
        // the name of the archive where this file was found
        public string source { get; set; }
    }

    public class ProgramSettings
    {
        public string filter { get; set; }
        public bool outputMini { get; set; }
        public bool outputFull { get; set; }
        public string searchDir { get; set; }
        public bool noDLC { get; set; }
        public bool showUnknownKeys { get; set; }
    }

    public class Program
    {
        private static List<GameInfo> _games = new();

        private static ProgramSettings _settings;

        private static void CheckMissing()
        {
            List<string> missingLst = new();

            foreach (string iniPath in Directory.EnumerateFiles(_settings.searchDir, "*.ini",
                         SearchOption.AllDirectories))
            {
                if(Directory.GetFiles(Path.GetDirectoryName(iniPath), "*.lst").Length == 0)
                    missingLst.Add(Path.GetFileName(Path.GetDirectoryName(iniPath)));
            }

            if (missingLst.Count > 0)
            {
                Console.WriteLine("Some updates are missing file lists!\nLists Missing:\n  - " + string.Join("\n  - ", missingLst));
                Console.WriteLine("\nSaved to missing.txt. The program will now exit.");
                File.WriteAllLines(Path.Combine(_settings.searchDir, "missing.txt"), missingLst);
                Environment.Exit(1);
            }
            else
            {
                File.Delete(Path.Combine(_settings.searchDir, "missing.txt"));
            }
            
        }

        private static void Main(string[] args)
        {
            try { _settings = JsonSerializer.Deserialize<ProgramSettings>(File.ReadAllText("config.json")); }
            catch(FileNotFoundException)
            {
                Console.WriteLine("HALT: No config.");
                Environment.Exit(1);
            }

            CheckMissing();
            
            ReadData(_settings.filter);

            var jsonSettings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            var jsonSettingsIndent = new JsonSerializerOptions(jsonSettings) { WriteIndented = true };
            string json = JsonSerializer.Serialize(_games, jsonSettingsIndent);
            string jsonMin = JsonSerializer.Serialize(_games, jsonSettings);
            
            File.WriteAllText(Path.Combine(_settings.searchDir, @$"kiss{(string.IsNullOrEmpty(_settings.filter)?"":$"-{_settings.filter}")}.json"), json, Encoding.UTF8);
            File.WriteAllText(Path.Combine(_settings.searchDir, @$"kiss{(string.IsNullOrEmpty(_settings.filter)?"":$"-{_settings.filter}")}.min.json"), jsonMin, Encoding.UTF8);

            Console.WriteLine($"Successfully parsed info for {_games.Count} games!");
        }

        private static void ReadData(string filter)
        {
            foreach (string gamePath in Directory.EnumerateDirectories(_settings.searchDir, "*",
                         SearchOption.TopDirectoryOnly))
            {
                var game = new GameInfo {game = Path.GetFileName(gamePath)};
                
                game.files = new List<FileInfo>();
                
                foreach (string file in Directory.EnumerateFiles(gamePath, "*.lst", SearchOption.AllDirectories))
                {
                    if (!string.IsNullOrEmpty(filter) && !file.Contains(filter)) continue;
                    if (_settings.noDLC && file.Contains(Path.Combine(gamePath, "_DLC"))) continue;

                    var source = Path.GetFileName(Path.GetDirectoryName(file));
                
                    ParseLst(game.files, file, source);
                }
                
                _games.Add(game);
            }
        }

        private static void ParseLst(List<FileInfo> fileinfos, string filename, string source)
        {
            var files = File.ReadAllLines(filename);

            foreach (var fileDef in files)
            {
                if(string.IsNullOrEmpty(fileDef)) continue;
                var file = new UpdateFile();
                string[] args = fileDef.Split(',');
                
                var filepath = args[2];
                var newInfo = false;

                var fileInfo = fileinfos.Find(x => x.filepath == filepath) ??
                               new FileInfo {filepath = filepath, updates = [] };
                
                file.size = long.Parse(args[3]);
                file.hash = args[4];
                file.version = int.Parse(args[5]);
                file.source = source;
                
                if (fileInfo.updates.All(x => x.hash != file.hash || x.size != file.size)) fileInfo.updates.Add(file);
                if (!fileinfos.Contains(fileInfo)) fileinfos.Add(fileInfo);
            }
        }
    }
}
