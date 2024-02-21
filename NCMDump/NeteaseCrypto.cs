using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Java.Nio.FileNio.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TagLib;
using static Android.Provider.MediaStore.Audio;
using static Android.Resource;

namespace NCMDump
{
    class NeteaseCrypto
    {
        private static readonly byte[] core_key = { 0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57 };
        private static readonly byte[] meta_key = { 0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28 };

        private static UInt32 ReadUInt32(FileStream fs)
        {
            var bytes = new byte[4];
            fs.Read(bytes);
            return BitConverter.ToUInt32(bytes);
        }

        public static byte[]? GetImageBytes(string filePath)
        {
            using (var fs = System.IO.File.Open(filePath, FileMode.Open))
            {
                //MAGIC_HEADER
                if (ReadUInt32(fs) != 0x4e455443)
                {
                    return null;
                }
                if (ReadUInt32(fs) != 0x4d414446)
                {
                    return null;
                }

                //gap1
                fs.Seek(2, SeekOrigin.Current);

                //rc4_key_enc_size
                var skip_len = ReadUInt32(fs);
                fs.Seek(skip_len, SeekOrigin.Current);

                //metadata_enc_size
                skip_len = ReadUInt32(fs);
                fs.Seek(skip_len, SeekOrigin.Current);

                //skip crc32&gap2
                fs.Seek(9, SeekOrigin.Current);

                //cover_data_size
                var cover_data_size = ReadUInt32(fs);
                byte[] cover_data = new byte[cover_data_size];
                fs.Read(cover_data);
                return cover_data;
            }
        }

        public static string? GetMetaData(string filePath)
        {
            using (var fs = System.IO.File.Open(filePath, FileMode.Open))
            {
                //MAGIC_HEADER
                if (ReadUInt32(fs) != 0x4e455443)
                {
                    return null;
                }
                if (ReadUInt32(fs) != 0x4d414446)
                {
                    return null;
                }

                //gap1
                fs.Seek(2, SeekOrigin.Current);

                //rc4_key_enc_size
                var rc4_key_enc_size = ReadUInt32(fs);
                //解密Metedata用不到
                fs.Seek(rc4_key_enc_size, SeekOrigin.Current);

                //metadata_enc_size
                var metadata_enc_size = ReadUInt32(fs);
                byte[] metadata_enc = new byte[metadata_enc_size];
                fs.Read(metadata_enc);

                for (int i = 0; i < metadata_enc.Length; i++)
                {
                    metadata_enc[i] ^= 0x63;
                }

                // 从 Base64 字符串进行解码
                var decrypt_base64 = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(GetBytesByOffset(metadata_enc, 22)));
                var decrypt_metadata = DecryptAex128Ecb(meta_key, decrypt_base64);
                return System.Text.Encoding.UTF8.GetString(decrypt_metadata);
            }
        }

        public static byte[]? GetMusicData(string filePath)
        {
            using (var fs = System.IO.File.Open(filePath, FileMode.Open))
            {
                //MAGIC_HEADER
                if (ReadUInt32(fs) != 0x4e455443)
                {
                    return null;
                }
                if (ReadUInt32(fs) != 0x4d414446)
                {
                    return null;
                }

                //gap1
                fs.Seek(2, SeekOrigin.Current);

                //rc4_key_enc_size
                var rc4_key_enc_size = ReadUInt32(fs);
                byte[] rc4_key_enc = new byte[rc4_key_enc_size];
                fs.Read(rc4_key_enc);

                for (int i = 0; i < rc4_key_enc.Length; i++)
                {
                    rc4_key_enc[i] ^= 0x64;
                }

                // 此处解析出来的值应该为减去字符串 "neteasecloudmusic" 长度之后的信息
                var keydata = GetBytesByOffset(DecryptAex128Ecb(core_key, rc4_key_enc), 17);
                var box = BuildKeyBox(keydata);

                //metadata_enc_size
                var metadata_enc_size = ReadUInt32(fs);
                fs.Seek(metadata_enc_size, SeekOrigin.Current);

                //skip crc32&gap2
                fs.Seek(9, SeekOrigin.Current);

                //cover_data_size
                var cover_data_size = ReadUInt32(fs);
                fs.Seek(cover_data_size, SeekOrigin.Current);

                var n = 0x8000;

                // 输出歌曲文件
                using (var output = new MemoryStream())
                {
                    while (true)
                    {
                        var tb = new byte[n];
                        var result = fs.Read(tb);
                        if (result <= 0) break;

                        for (int i = 0; i < n; i++)
                        {
                            var j = (byte)((i + 1) & 0xff);
                            tb[i] ^= box[box[j] + box[(box[j] + j) & 0xff] & 0xff];
                        }

                        output.Write(tb);
                    }
                    output.Flush();
                    return output.ToArray();
                }
            }
        }

        public static string GetMusicNameFromMetaData(string metadata)
        {
            try
            {
                var fixed_metadata = metadata.Substring(6).Trim();
                var metadata_json = JObject.Parse(fixed_metadata);
                var MusicName = metadata_json["musicName"].Value<string>();
                return MusicName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return "No Name";
            }
        }

        public static int GetBitrateFromMetaData(string metadata)
        {
            try
            {
                var fixed_metadata = metadata.Substring(6).Trim();
                var metadata_json = JObject.Parse(fixed_metadata);
                var Bitrate = metadata_json["bitrate"].Value<int>();
                return Bitrate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 0;
            }
        }

        public static string GetFormatFromMetaData(string metadata)
        {
            try
            {
                var fixed_metadata = metadata.Substring(6).Trim();
                var metadata_json = JObject.Parse(fixed_metadata);
                var Format = metadata_json["format"].Value<string>();
                return Format;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return "Unknown";
            }
        }

        public static string GetAlbumPicUrlFromMetaData(string metadata)
        {
            try
            {
                var fixed_metadata = metadata.Substring(6).Trim();
                var metadata_json = JObject.Parse(fixed_metadata);
                var albumPic = metadata_json["albumPic"].Value<string>();
                return albumPic;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return "Unknown";
            }
        }

        public static string GetAlbumFromMetaData(string metadata)
        {
            try
            {
                var fixed_metadata = metadata.Substring(6).Trim();
                var metadata_json = JObject.Parse(fixed_metadata);
                var album = metadata_json["album"].Value<string>();
                return album;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return "Unknown";
            }
        }

        public static string GetArtistNamesFromMetaData(string metadata)
        {
            try
            {
                List<string> Artists = new List<string>();
                var fixed_metadata = metadata.Substring(6).Trim();
                var metadata_json = JObject.Parse(fixed_metadata);
                var artistArray = (JArray)metadata_json["artist"];
                foreach (var artist in artistArray)
                {
                    // artist[0] 对应艺术家的名称
                    Console.WriteLine(artist[0].Value<string>());
                    Artists.Add(artist[0].Value<string>());
                }
                string artistNamesString = string.Join(", ", Artists);

                return artistNamesString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return "Unknown";
            }
        }

        public static string[] GetArtistNamesArrayFromMetaData(string metadata)
        {
            try
            {
                List<string> Artists = new List<string>();
                var fixed_metadata = metadata.Substring(6).Trim();
                var metadata_json = JObject.Parse(fixed_metadata);
                var artistArray = (JArray)metadata_json["artist"];
                foreach (var artist in artistArray)
                {
                    // artist[0] 对应艺术家的名称
                    Console.WriteLine(artist[0].Value<string>());
                    Artists.Add(artist[0].Value<string>());
                }
                return Artists.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return new string[]{""};
            }
        }


        public static bool WriteMusicFile(string filepath, string dstdir)
        {
            try
            {
                var metadata = GetMetaData(filepath);
                var extension = GetFormatFromMetaData(metadata);
                var music_name = GetMusicNameFromMetaData(metadata);
                var artis_name = GetArtistNamesArrayFromMetaData(metadata);
                var album_name = GetAlbumFromMetaData(metadata);
                var imagebytes = GetImageBytes(filepath);
                var outfile_name = dstdir + "/" + System.IO.Path.GetFileNameWithoutExtension(filepath) + "." + extension;
                Console.WriteLine("Dump:" + outfile_name);


                if (extension is null or "Unknown" or "")
                {
                    extension = "mp3";
                }

                using (var outfile = System.IO.File.Create(outfile_name))
                {
                    outfile.Write(GetMusicData(filepath));
                    outfile.Flush();
                }

                var tagfile = TagLib.File.Create(outfile_name);
                if (imagebytes is not null)
                {
                    var tag_pic = new TagLib.Picture(new TagLib.ByteVector(imagebytes));
                    tagfile.Tag.Pictures = new TagLib.Picture[] { tag_pic };
                }
                if(music_name is not null)
                {
                    tagfile.Tag.Title = music_name;
                }
                if(artis_name is not null)
                {
                    tagfile.Tag.Performers = artis_name;
                }
                if(album_name is not null)
                {
                    tagfile.Tag.Album = album_name;
                }
                tagfile.Save();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return false;
            }
        }

        private static byte[] DecryptAex128Ecb(byte[] keyBytes, byte[] data)
        {
            var aes = Aes.Create();
            if (aes != null)
            {
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.ECB;
                using (var decryptor = aes.CreateDecryptor(keyBytes, null))
                {
                    byte[] result = decryptor.TransformFinalBlock(data, 0, data.Length);
                    return result;
                }
            }

            return null;
        }

        private static byte[] BuildKeyBox(byte[] key)
        {
            byte[] box = new byte[256];
            for (int i = 0; i < 256; ++i)
            {
                box[i] = (byte)i;
            }

            byte keyLength = (byte)key.Length;
            byte c;
            byte lastByte = 0;
            byte keyOffset = 0;
            byte swap;

            for (int i = 0; i < 256; ++i)
            {
                swap = box[i];
                c = (byte)((swap + lastByte + key[keyOffset++]) & 0xff);

                if (keyOffset >= keyLength)
                {
                    keyOffset = 0;
                }

                box[i] = box[c];
                box[c] = swap;
                lastByte = c;
            }

            return box;
        }

        private static byte[] GetBytesByOffset(byte[] srcBytes, int offset = 0, long length = 0)
        {
            if (length == 0)
            {
                var resultBytes = new byte[srcBytes.Length - offset];
                System.Array.Copy(srcBytes, offset, resultBytes, 0, srcBytes.Length - offset);
                return resultBytes;
            }

            var resultBytes2 = new byte[length];
            System.Array.Copy(srcBytes, 0, resultBytes2, 0, length);
            return resultBytes2;
        }
    }
}
