using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NCMDump
{
    public class NeteaseCryptoMusic
    {
        private static readonly byte[] core_key = { 0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57 };
        private static readonly byte[] meta_key = { 0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28 };


        private string path;

        public bool IsEnabedImages { get; set; }


        private NeteaseCryptoMusic(string _path, NeteaseCryptoMusicMetaData metaData, ImageSource coverimage, byte[] coverImageBitmap, bool isEnabedImages)
        {
            path = _path;
            MetaData = metaData;
            CoverImage = coverimage;
            CoverImageBitmap = coverImageBitmap;
            IsEnabedImages = isEnabedImages;
        }

        public NeteaseCryptoMusicMetaData MetaData
        {
            get;
            set;
        }

        public ImageSource CoverImage
        {
            get;
            set;
        }

        public byte[] CoverImageBitmap
        {
            get;
            set;
        }

        public static NeteaseCryptoMusic FromFile(string path)
        {
            using (var fs = System.IO.File.Open(path, FileMode.Open))
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
                var metadata = System.Text.Encoding.UTF8.GetString(decrypt_metadata).Substring(6).Trim();
                //var metadata_object = JsonSerializer.Deserialize<NeteaseCryptoMusicMetaData>(metadata);
                var metadata_object = JsonConvert.DeserializeObject<NeteaseCryptoMusicMetaData>(metadata);

                //skip crc32&gap2
                fs.Seek(9, SeekOrigin.Current);

                ImageSource coverimage_source;

                //cover_data_size
                var cover_data_size = ReadUInt32(fs);
                byte[] coverimage_bitmap;
                bool isEnabedImages = false;
                if (cover_data_size != 0)
                {
                    byte[] cover_data = new byte[cover_data_size];
                    fs.Read(cover_data);
                    var coverimage = new MemoryStream(cover_data);
                    coverimage_bitmap = coverimage.ToArray();
                    coverimage_source = ImageSource.FromStream(() => { return coverimage; });
                    isEnabedImages = true;
                }
                else
                {
                    var stream = FileSystem.OpenAppPackageFileAsync("music.png").Result;
                    MemoryStream memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    coverimage_bitmap = memoryStream.ToArray();
                    coverimage_source = ImageSource.FromStream(() => { return memoryStream; });
                    stream.Close();
                    memoryStream.Close();
                }
                return new NeteaseCryptoMusic(path, metadata_object, coverimage_source, coverimage_bitmap, isEnabedImages);
            }
        }

        public bool WriteDecryptMusic(string outdir)
        {
            var NewFilePath = outdir + "/" + System.IO.Path.GetFileNameWithoutExtension(path) + "." + MetaData.Format;
            if (System.IO.Path.Exists(NewFilePath))
            {
                Debug.WriteLine($"File Exists -> {NewFilePath}");
                return true;
            }
            try
            {
                using (var fs = System.IO.File.Open(path, FileMode.Open))
                {
                    //MAGIC_HEADER
                    if (ReadUInt32(fs) != 0x4e455443)
                    {
                        return false;
                    }
                    if (ReadUInt32(fs) != 0x4d414446)
                    {
                        return false;
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

                    using (var OutputFileStream = System.IO.File.Create(NewFilePath))
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

                            OutputFileStream.Write(tb);
                        }
                        OutputFileStream.Flush();
                    }

                    var tagfile = TagLib.File.Create(NewFilePath);
                    if (IsEnabedImages)
                    {
                        var tag_pic = new TagLib.Picture(new TagLib.ByteVector(CoverImageBitmap));
                        tagfile.Tag.Pictures = new TagLib.Picture[] { tag_pic };
                    }
                    else if (GlobalVars.Configs.DownloadCoverImage)
                    {
                        var image_bytes = NeteaseMusicDataDownload.GetCoverImage(MetaData.AlbumPic).Result;
                        if (image_bytes != null)
                        {
                            var tag_pic = new TagLib.Picture(new TagLib.ByteVector(image_bytes));
                            tagfile.Tag.Pictures = new TagLib.Picture[] { tag_pic };
                        }
                    }
                    if (GlobalVars.Configs.DownloadLyric)
                    {
                        var music_lyric = NeteaseMusicDataDownload.GetLyric(MetaData.MusicId).Result;
                        if (music_lyric != null)
                        {
                            if (music_lyric.code == 200)
                            {
                                tagfile.Tag.Lyrics = music_lyric.lrc.lyric;
                            }
                        }
                    }

                    tagfile.Tag.Comment = MetaData.Album;
                    tagfile.Tag.Title = MetaData.MusicName;
                    tagfile.Tag.Performers = MetaData.Artist.Select(x => x.ArtistName).ToArray();
                    tagfile.Tag.Album = MetaData.Album;
                    tagfile.Save();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception -> {ex.Message}");
                Debug.WriteLine($"StackTrace:\n {ex.StackTrace}");
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

        private static UInt32 ReadUInt32(FileStream fs)
        {
            var bytes = new byte[4];
            fs.Read(bytes);
            return BitConverter.ToUInt32(bytes);
        }

    }
}
