using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.IO;

namespace RippleCommunityPacks
{
    class Downloader
    {
        private static HttpClient _client;

        public static HttpClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        // method for logging in
        public static HttpClient Login(HttpClient client)
        {
            client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", File.ReadAllText("_username.txt")),
                new KeyValuePair<string, string>("password", File.ReadAllText("_password.txt")),
                new KeyValuePair<string, string>("autologin", "on"),
                new KeyValuePair<string, string>("sid", ""),
                new KeyValuePair<string, string>("redirect", "index.php"),
                new KeyValuePair<string, string>("viewonline", "off"),
                new KeyValuePair<string, string>("login", "Login")
            });

            var response = client.PostAsync("https://osu.ppy.sh/forum/ucp.php?mode=login", content).Result;

            return client;
        }


        public static async void DownloadBeatmap(Program.Beatmap beatmap, string section, string packType)
        {
            var filename = beatmap.SetID + " " + (beatmap.Artist != "" ? beatmap.Artist + " - " : "") + beatmap.Title;

            Directory.CreateDirectory("maps");
            Directory.CreateDirectory(Path.Combine("maps", packType));
            Directory.CreateDirectory(Path.Combine("maps", packType, section));

            if (Client == null)// that means we need to login to page
            {
                Console.WriteLine("Logging in...");
                Client = Login(Client);
            }

            Console.WriteLine("Downloading {0}", filename);

            var request = new HttpRequestMessage(HttpMethod.Get, String.Format("https://osu.ppy.sh/d/{0}", beatmap.SetID));
            var sendTask = Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var response = sendTask.Result.EnsureSuccessStatusCode();
            var httpStream =  await response.Content.ReadAsStreamAsync();

            using (var fileStream = File.Create(Path.Combine("maps", packType, section) + "/" + filename + ".osz"))
            using (var reader = new StreamReader(httpStream))
            {
                httpStream.CopyTo(fileStream);
                fileStream.Flush();
            }

            Console.WriteLine("Finished downloading {0}", filename);
        }
    }
}
