using System;

namespace osu_api
{
    public class Beatmap
    {
        public string approved { get; set; }
        public string approved_date { get; set; }
        public string last_update { get; set; }
        public string artist { get; set; }
        public int beatmap_id { get; set; }
        public int beatmapset_id { get; set; }
        public string bpm { get; set; }
        public string creator { get; set; }
        public string difficultyrating { get; set; }
        public string diff_size { get; set; }
        public string diff_overall { get; set; }
        public string diff_approach { get; set; }
        public string diff_drain { get; set; }
        public string hit_length { get; set; }
        public string source { get; set; }
        public string title { get; set; }
        public string total_length { get; set; }
        public string version { get; set; }
        public string mode { get; set; }
    }
}