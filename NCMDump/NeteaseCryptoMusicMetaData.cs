using Microsoft.Maui.Graphics.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace NCMDump
{
    public class ArtistDataConverter : JsonConverter<List<Artist>>
    {
        public override List<Artist> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray token");
            }

            List<Artist> artists = new List<Artist>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return artists;
                }

                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException("Expected StartArray token");
                }

                string name = string.Empty;
                int value = 0;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    if (reader.TokenType != JsonTokenType.String && reader.TokenType != JsonTokenType.Number)
                    {
                        throw new JsonException("Expected String or Number token");
                    }

                    if (reader.TokenType == JsonTokenType.String)
                    {
                        name = reader.GetString();
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        value = reader.GetInt32();
                    }
                }

                artists.Add(new Artist { ArtistName = name, ArtistId = value });
            }

            throw new JsonException("Unexpected end when reading JSON.");
        }

        public override void Write(Utf8JsonWriter writer, List<Artist> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var artist in value)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(artist.ArtistName);
                writer.WriteNumberValue(artist.ArtistId);
                writer.WriteEndArray();
            }

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
        [JsonPropertyName("musicId")]
        public long MusicId { get; set; }

        [JsonPropertyName("musicName")]
        public string MusicName { get; set; }

        [JsonPropertyName("artist")]
        [JsonConverter(typeof(ArtistDataConverter))]
        public List<Artist> Artist { get; set; }

        [JsonPropertyName("albumId")]
        public long AlbumId { get; set; }

        [JsonPropertyName("album")]
        public string Album { get; set; }

        [JsonPropertyName("albumPic")]
        public string AlbumPic { get; set; }

        [JsonPropertyName("bitrate")]
        public int Bitrate { get; set; }

        [JsonPropertyName("mp3DocId")]
        public string Mp3DocId { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("mvId")]
        public UInt64 MvId { get; set; }

        //[JsonPropertyName("alias")]
        //public List<string> Alias { get; set; }

        [JsonPropertyName("transNames")]
        public List<string> TransNames { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("flag")]
        public int Flag { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(NeteaseCryptoMusicMetaData))]
    internal partial class NeteaseCryptoMusicMetaDataSourceGenerationContext : JsonSerializerContext
    {
    }
}
