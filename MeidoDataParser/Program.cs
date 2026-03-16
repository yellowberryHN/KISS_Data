using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Force.Crc32;

namespace MeidoDataParser
{
    public class DataInfo
    {
        public string version { get; set; }
        
        public List<GameInfo> games { get; set; }
    }
    
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
        
        // if the file is compressed
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool compressed { get; set; }
        
        // if the file is from a DLC, not an update
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool dlc { get; set; }
        
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
        public bool checkDLCs { get; set; }
        
        public bool sortUpdates { get; set; }
        
        public bool verbose { get; set; }
    }

    public class Program
    {
        private static List<GameInfo> _games = new();

        private static ProgramSettings _settings;

        private static void CheckMissing()
        {
            List<string> missingLst = new();

            foreach (var gamePath in Directory.GetDirectories(_settings.searchDir))
            {
                foreach (var dir in Directory.EnumerateDirectories(gamePath, "*", SearchOption.AllDirectories))
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (dirInfo.GetDirectories().Length == 0 && dirInfo.GetFiles("*.lst").Length == 0)
                    {
                        missingLst.Add(Path.GetRelativePath(_settings.searchDir, dir));
                    }
                }
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
        
        private static char[] IgnoredDLCChars = ['*','/',';'];

        private static Dictionary<string, string> FetchDLCList(string url)
        {
            return new WebClient().DownloadString(url).Trim().Split('\n')
                .Skip(1).Select(x => x.Split(',', 2))
                .Where(x => x.Length == 2 && !IgnoredDLCChars.Contains(x[0][0]))
                .ToDictionary(x => x[0], x => x[1].Trim('"'));
        }

        private static void CheckDLC(string name, string url)
        {
            Console.WriteLine($"\n=== {name.ToUpper()} ===");
            var files = _games.FindAll(x => x.game == name).SelectMany(x => x.files).ToList();
            var dlcs = FetchDLCList(url);
            
            var missing = new Dictionary<string, string>();

            foreach (var dlc in dlcs.Where(dlc => files.All(x => !x.filepath.EndsWith(dlc.Key))))
            {
                if(_settings.verbose) Console.WriteLine("DLC file missing: " + dlc.Key + " - " + dlc.Value);
                missing.Add(dlc.Key, dlc.Value);
            }
            
            Console.WriteLine($"- Missing DLC: {missing.Count()}");
        }
        
        private static void CheckDLCs()
        {
            var cm3d2_DLC = "https://raw.githubusercontent.com/MeidosFriend/CM3D2_DLC_Checker/master/CM_NewListDLC.lst";
            var com3d2_DLC = "https://raw.githubusercontent.com/krypto5863/COM3D2_DLC_Checker/master/COM_NewListDLC.lst";
            var com3d2_en_DLC = "https://raw.githubusercontent.com/MeidosFriend/COM3D2_EN_DLC_Checker/master/COM_EN_NewListDLC.lst";
            var com3d2_en_inm_DLC = "https://raw.githubusercontent.com/MeidosFriend/COM3D2_INM_DLC_Checker/master/COM_INM_NewListDLC.lst";
            var cres_kces_DLC = "https://raw.githubusercontent.com/MeidosFriend/CRE_DLC_Checker/refs/heads/main/CRE_NewListDLC.lst";

            Console.WriteLine("Checking DLCs...");
            try
            {
                // CM3D2
                CheckDLC("cm3d2", cm3d2_DLC);

                // COM3D2
                CheckDLC("com3d2", com3d2_DLC);
                
                // COM3D2 EN (R18)
                CheckDLC("com3d2-en", com3d2_en_DLC);
                
                // COM3D2 INM
                CheckDLC("com3d2-en-inm", com3d2_en_inm_DLC);
                
                // CRES/KCES
                CheckDLC("cres-kces", cres_kces_DLC);
            }
            catch (WebException e)
            {
                Console.WriteLine($"Error while checking DLCs: {e}\nMoving on...");
            }
        }

        private static string JsonName(bool min)
        {
            var filter = string.IsNullOrEmpty(_settings.filter) ? "" : $"-{_settings.filter}";
            var minStr = min ? ".min" : "";
            return $"kiss{filter}{minStr}.json";
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
            
            if(_settings.checkDLCs) CheckDLCs();
            
            //CheckData(@"C:\KISS\CM3D2\");
            //CheckData(@"F:\Games\KISS\CM3D2 Editor Trial\");
            //CheckData(@"F:\Games\KISS\CM3D2\");

            var jsonSettings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            var jsonSettingsIndent = new JsonSerializerOptions(jsonSettings) { WriteIndented = true };
            
            var allData = new DataInfo {games = _games, version = DateTime.UtcNow.ToString("yyyy-MM-dd")};
            
            string json = JsonSerializer.Serialize(allData, jsonSettingsIndent);
            string jsonMin = JsonSerializer.Serialize(allData, jsonSettings);
            
            File.WriteAllText(Path.Combine(_settings.searchDir, JsonName(false)), json, Encoding.UTF8);
            File.WriteAllText(Path.Combine(_settings.searchDir, JsonName(true)), jsonMin, Encoding.UTF8);

            Console.WriteLine($"Successfully parsed info for {_games.Count} games!");
        }

        private static void ReadData(string filter)
        {
            foreach (string gamePath in Directory.EnumerateDirectories(_settings.searchDir, "*",
                         SearchOption.TopDirectoryOnly))
            {
                var game = new GameInfo {game = Path.GetFileName(gamePath)};
                
                game.files = new List<FileInfo>();
                
                if (_settings.verbose)
                    Console.WriteLine($"Game '{Path.GetRelativePath(_settings.searchDir, gamePath)}'");
                
                foreach (string file in Directory.EnumerateFiles(gamePath, "*.lst", SearchOption.AllDirectories))
                {
                    if (!string.IsNullOrEmpty(filter) && !file.Contains(filter)) continue;

                    var isDLC = file.Contains(Path.Combine(gamePath, "_DLC"));
                    if (_settings.noDLC && isDLC) continue;
                    
                    if(_settings.verbose)
                        Console.Write($"  - {Path.GetRelativePath(gamePath, Path.GetDirectoryName(file))}");

                    var source = Path.GetFileName(Path.GetDirectoryName(file));
                
                    ParseLst(game.files, file, source, isDLC);
                }
                
                _games.Add(game);
            }
        }

        private static void ParseLst(List<FileInfo> fileinfos, string filename, string source, bool isDLC)
        {
            var files = File.ReadAllLines(filename);

            var dups = 0;

            foreach (var fileDef in files)
            {
                if(string.IsNullOrEmpty(fileDef)) continue;
                var file = new UpdateFile();
                string[] args = fileDef.Split(',');
                
                // TODO: verify in updater/installer decomp
                // cm3d2 seems to use .bz2, and cm3d seems to use .nei ???
                file.compressed = args[0] == "1" && args[1] != "0";

                string filepath;
                
                // setup lst
                if (Path.GetFileName(filename).StartsWith("setup"))
                {
                    filepath = args[1];
                    
                    file.size = long.Parse(args[2]);
                    file.hash = args[3];
                    file.version = 100;
                }
                else if (args.Length == 4)
                {
                    filepath = args[1];
                    
                    file.hash = args[2];
                    file.version = int.Parse(args[3]);
                }
                else if (args.Length == 5)
                {
                    filepath = args[2];
                    
                    file.hash = args[3];
                    file.version = int.Parse(args[4]);
                }
                else
                {
                    filepath = args[2];
                    
                    file.size = long.Parse(args[3]);
                    file.hash = args[4];
                    file.version = int.Parse(args[5]);
                }
                
                file.source = source;
                file.dlc = isDLC;

                // trim compression extension from filename for equality search
                if (file.compressed)
                {
                    filepath = filepath.Substring(0, filepath.Length-4);
                }
                
                var fileInfo = fileinfos.Find(x => x.filepath == filepath) ??
                               new FileInfo {filepath = filepath, updates = [] };
                
                // likelihood of hash being the same with different size on the same file is extremely low, check anyway
                if (fileInfo.updates.All(x => x.hash != file.hash || x.size != file.size))
                    fileInfo.updates.Add(file);
                else dups++;
                
                if (!fileinfos.Contains(fileInfo))
                    fileinfos.Add(fileInfo);
                
                if(_settings.sortUpdates) fileInfo.updates.Sort((x, y) => x.version.CompareTo(y.version));
            }

            if (!_settings.verbose) return;
            
            if (dups > 0) Console.WriteLine($" [{dups} duplicates]");
            else Console.Write("\n");
        }

        // this is a quick and dirty dataset validation, there's more proper ways to do this
        private static void CheckData(string gamePath)
        {
            var knownPaths = new HashSet<string>();

            foreach (var game in _games)
            {
                knownPaths.UnionWith(game.files.Select(x => x.filepath));
            }
            
            foreach (var file in Directory.EnumerateFiles(gamePath, "*", SearchOption.AllDirectories))
            {
                var relPath = Path.GetRelativePath(gamePath, file);
                
                if (!knownPaths.Contains(relPath)) continue;
                
                string hash;
                
                // File.ReadAllBytes will fail on files larger than 2GB, i'm lazy
                try
                {
                    hash = Crc32Algorithm.Compute(File.ReadAllBytes(file)).ToString("X8");
                }
                catch (IOException)
                {
                    hash = "!FAILED!";
                }
                
                foreach (var game in _games)
                {
                    var fileInfo = game.files.Find(x => x.filepath == relPath);
                    if (fileInfo == null) continue;

                    var known = fileInfo.updates.Find(x => x.hash == hash);
                    
                    var version = known != null ? $"{known.version} ({known.source})" : $"??? ({hash})";

                    if (known == null) continue;
                    
                    Console.WriteLine($"[{game.game}] {relPath}: {version}");
                    break;
                }
            }
        }
    }
}
