using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NCMDump
{
    public class Artist
    {
        public string ArtistName { get; set; }

        public int ArtistId { get; set; }
    }


    public class NeteaseCryptoMusicMetaData
    {
        [JsonPropertyName("musicId")]
        public long MusicId { get; set; }

        [JsonPropertyName("musicName")]
        public string MusicName { get; set; }

        [JsonPropertyName("artist")]
        public List<List<object>> artist { get; set; }

        [JsonPropertyName("albumId")]
        public long AlbumId { get; set; }

        [JsonPropertyName("album")]
        public string Album { get; set; }

        [JsonPropertyName("albumPicDocId")]
        public string AlbumPicDocId { get; set; }

        [JsonPropertyName("albumPic")]
        public string AlbumPic { get; set; }

        [JsonPropertyName("bitrate")]
        public int Bitrate { get; set; }

        [JsonPropertyName("mp3DocId")]
        public string Mp3DocId { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("mvId")]
        public long mvId { get; set; }

        [JsonPropertyName("alias")]
        public List<string> Alias { get; set; }

        [JsonPropertyName("transNames")]
        public List<string> TransNames { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("flag")]
        public int Flag { get; set; }

        public List<Artist> Artists
        {
            get
            {
                List<Artist> artists = new List<Artist>();
                foreach (var item in artist)
                {

                    artists.Add(new Artist { ArtistName = ((JsonElement)item[0]).GetString(), ArtistId = ((JsonElement)item[1]).GetInt32() });
                }
                return artists;
            }
        }

    }
}
