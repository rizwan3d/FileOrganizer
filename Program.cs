using System;
using System.Collections.Generic;
using System.IO;

namespace FileOrganizer
{
    class Program
    {
        static string WatchPath, OrgPath;

        static Dictionary<string, string> orgSubPaths;

        static void Main(string[] args)
        {
            string[] Config = File.ReadAllLines("Config.csv");

            WatchPath = Config[0].Split(',')[1];
            OrgPath = Config[1].Split(',')[1];

            orgSubPaths = new Dictionary<string, string>();
            for(int i = 2;i < Config.Length;i++){
                string[] ExtensionOrgSubPath = Config[i].Split(',');
                orgSubPaths.Add(ExtensionOrgSubPath[0], ExtensionOrgSubPath[1]);
            }


            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = WatchPath;
                watcher.Filter = "*.*";
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;

                watcher.Created += OnCreated;
                watcher.EnableRaisingEvents = true;

                Console.WriteLine("Press 'q' to quit.");
                while (Console.Read() != 'q') ;
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string watchFilePath = e.FullPath;

            string Extension = Path.GetExtension(watchFilePath).Substring(1);
            if (orgSubPaths.ContainsKey(Extension.ToLower()))
            {
                MoveFile(watchFilePath, orgSubPaths[Extension.ToLower()],Extension);
            }
            else
            {
                MoveFile(watchFilePath, "UnKnown",Extension);
            }
        }

        private static void MoveFile(string watchFilePath, string orgSubPath,string Extension)
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = now.ToString("MMMM");
            string day = now.ToString("dd");

            string FileName = Path.GetFileName(watchFilePath);
            string path2ndPart = $@"{orgSubPath}\{year}\{month}\{day}\";
            string FullDirectoryPath = Path.Combine(OrgPath, path2ndPart);

            Directory.CreateDirectory(FullDirectoryPath);

            string OrgEdPath = Path.Combine(FullDirectoryPath, FileName);
            if(File.Exists(OrgEdPath)){
                FileName  = $"{Path.GetFileNameWithoutExtension(OrgEdPath)}-{now.Minute}{now.Second}.{Extension}";
                OrgEdPath = Path.Combine(FullDirectoryPath, FileName);
            }

            while(IsFileLocked(watchFilePath) || IsFileLocked(OrgEdPath,true));
            File.Move(watchFilePath, OrgEdPath);
            File.AppendAllText("Log.csv",$"{now},{watchFilePath},{OrgEdPath}{Environment.NewLine}");
        }

        private static bool IsFileLocked(string FilePath, bool del = false)
        {
            bool locked = false;
            try{
                FileStream fs = File.Open(FilePath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
                fs.Close();
                if(del)
                    File.Delete(FilePath);
            }
            catch(Exception){
                locked = true;
            }
            return locked;
        }
    }
}
