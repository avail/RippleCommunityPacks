using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;
using osu_api;

namespace RippleCommunityPacks
{
    class Program
    {
        public class Beatmap
        {
            public int ID { get; set; }
            public int SetID { get; set; }
            public string Artist { get; set; }
            public string Title { get; set; }
            public string Difficulty { get; set; }
        }

        static List<Beatmap> nm = new List<Beatmap>();
        static List<Beatmap> hr = new List<Beatmap>();
        static List<Beatmap> hd = new List<Beatmap>();
        static List<Beatmap> dt = new List<Beatmap>();
        static List<Beatmap> fm = new List<Beatmap>();
        static List<Beatmap> tb = new List<Beatmap>();

        static string downloadUrlPrefix = "//dl.avail.pw/rct/packs/";

        static bool downloadMaps = true;
        static bool generateHtml = false;
        static bool createPacks = true;

        static string currentIniFile;

        static osuAPI api = new osuAPI(File.ReadAllText("_key.txt"));

        static void GenerateHtml(List<Beatmap> maps, string packType)
        {
            Console.WriteLine(String.Format("Generating html for {0}/{1}.", currentIniFile, packType));

            Directory.CreateDirectory("html");

            StreamWriter writer = new StreamWriter(String.Format("html/{0}_{1}.html", currentIniFile, packType));

            writer.WriteLine(String.Format("<h2>{0}</h2>", packType));
            writer.WriteLine(String.Format("<a href=\"{0}rct-{1}-{2}.zip\">[ Download Pack ]</a>", downloadUrlPrefix, currentIniFile, packType));
            writer.WriteLine("<ul>");

            foreach (Beatmap map in maps)
            {
                writer.WriteLine(String.Format("<li>{0} - {1} [{2}]</li>", map.Artist, map.Title, map.Difficulty));
            }

            writer.WriteLine("</ul>");

            writer.Close();
        }

        static void DeletePreviousHtmlFiles()
        {
            Console.WriteLine("Deleting previous html files, if present");

            DirectoryInfo di = new DirectoryInfo("html");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        static void DeletePreviousPackFiles()
        {
            Console.WriteLine("Deleting previous packs, if present");

            DirectoryInfo di = new DirectoryInfo("maps");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        static void DeleteLeftoverMapFiles(string packType)
        {
            Console.WriteLine("Deleting leftover .osz files");

            DirectoryInfo di = new DirectoryInfo(Path.Combine("maps", packType, currentIniFile));

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        static void CreatePackSingle(List<Beatmap> packMaps, string packType)
        {
            string packFileName = String.Format("rct-{0}-{1}.zip", currentIniFile, packType);

            Console.WriteLine(String.Format("Creating pack {0}.", packFileName));

            Directory.CreateDirectory("packs");

            ZipFile.CreateFromDirectory(Path.Combine("maps", currentIniFile, packType), Path.Combine("packs", packFileName));

            Console.WriteLine(String.Format("Finished creating pack {0}.", packFileName));
        }

        static void CreatePack()
        {
            if (generateHtml)
            {
                GenerateHtml(nm, "nomod");
                GenerateHtml(hr, "hardrock");
                GenerateHtml(hd, "hidden");
                GenerateHtml(dt, "doubletime");
                GenerateHtml(fm, "freemod");
                GenerateHtml(tb, "tiebreaker");
            }

            if (createPacks)
            {
                CreatePackSingle(nm, "nomod");
                CreatePackSingle(hr, "hardrock");
                CreatePackSingle(hd, "hidden");
                CreatePackSingle(dt, "doubletime");
                CreatePackSingle(fm, "freemod");
                CreatePackSingle(tb, "tiebreaker");
            }

        }

        static void GetBeatmapInfo(string section, int id)
        {
            Beatmap beatmap = new Beatmap();
            beatmap.ID = id;

            List<osu_api.Beatmap> bm = api.GetBeatmaps(id);

            beatmap.SetID = bm[0].beatmapset_id;
            beatmap.Artist = bm[0].artist;
            beatmap.Title = bm[0].title;
            beatmap.Difficulty = bm[0].version;

            //Console.WriteLine(String.Format("{0} - {1} [{2}]", beatmap.Artist, beatmap.Title, beatmap.Difficulty));
            
            switch(section)
            {
                case "NoMod":
                    nm.Add(beatmap);
                    break;
                case "HardRock":
                    hr.Add(beatmap);
                    break;
                case "Hidden":
                    hd.Add(beatmap);
                    break;
                case "DoubleTime":
                    dt.Add(beatmap);
                    break;
                case "FreeMod":
                    fm.Add(beatmap);
                    break;
                case "Tiebreaker":
                    tb.Add(beatmap);
                    break;
            }

            if (downloadMaps)
            {
                Downloader.DownloadBeatmap(beatmap, section.ToLower(), currentIniFile);
            }
        }


        static void ParseIniFile(string file)
        {
            string currentSection = "";

            using (StreamReader reader = new StreamReader(file))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();

                    if (line == String.Empty) // if it's an empty line
                    {
                        continue; // skip the loop
                    }

                    if (line.StartsWith("[")) // we're in a section
                    {
                        currentSection = line.Substring(1, line.Length - 2);
                        //Console.WriteLine(String.Format("Section: {0}", currentSection));
                        continue;
                    }

                    string[] keyValue = line.Split('=');
                    
                    GetBeatmapInfo(currentSection, Convert.ToInt32(keyValue[1]));
                }
            }
        }

        static void Main(string[] args)
        {
            // grr we have to initialize it
            string[] files = { "" };

            try {
                files = Directory.GetFiles("inis/", "*.ini");
            }
            catch
            {
                Console.WriteLine("Directory 'inis' not found or does not contain any .ini files");
                Environment.Exit(0);
            }

            DeletePreviousPackFiles();
            DeletePreviousHtmlFiles();

            foreach (var file in files)
            {
                currentIniFile = file.Substring(5, file.Length - 9);
                
                try
                {
                    nm.Clear();
                    hr.Clear();
                    hd.Clear();
                    dt.Clear();
                    fm.Clear();
                    tb.Clear();
                }
                catch
                {

                }

                ParseIniFile(file);
                CreatePack();
            }

            Console.WriteLine("Done, packs *should be* present in the 'packs' directory if everything went according to plan");
            Console.ReadLine();
        }
    }
}
