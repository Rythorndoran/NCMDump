using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMDump
{
    public class ArtistDataConverter : Newtonsoft.Json.JsonConverter<Artist>
    {
        public override Artist ReadJson(JsonReader reader, Type objectType, Artist existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException("Expected StartArray token");
            }

            // 读取数组元素
            JArray array = JArray.Load(reader);
            string name = array[0].ToString();
            int value = array[1].Value<int>();

            return new Artist { ArtistName = name, ArtistId = value };
        }

        public override void WriteJson(JsonWriter writer, Artist value, Newtonsoft.Json.JsonSerializer serializer)
        {
            // 序列化 MyData 对象为无键的 JSON 数组
            writer.WriteStartArray();
            writer.WriteValue(value.ArtistName);
            writer.WriteValue(value.ArtistId);
            writer.WriteEndArray();
        }
    }

    public class Artist
    {
        public string ArtistName { get; set; }

        public int ArtistId { get; set; }
    }


    public class NeteaseCryptoMusicMetaData
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "musicId")]
        public long MusicId { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "musicName")]
        public string MusicName { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "artist" , ItemConverterType = typeof(ArtistDataConverter))]
        public List<Artist> Artist { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "albumId")]
        public long AlbumId { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "album")]
        public string Album { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "albumPicDocId")]
        public string AlbumPicDocId { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "albumPic")]
        public string AlbumPic { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "bitrate")]
        public int Bitrate { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "mp3DocId")]
        public string Mp3DocId { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "duration")]
        public int Duration { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "mvId")]
        public long MvId { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "alias")]
        public List<string> Alias { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "transNames")]
        public List<string> TransNames { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "format")]
        public string Format { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "flag")]
        public int Flag { get; set; }
    }
}
