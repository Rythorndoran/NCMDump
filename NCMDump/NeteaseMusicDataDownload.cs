using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMDump
{
    public class UserData
    {
        public int id { get; set; }
        public int status { get; set; }
        public int demand { get; set; }
        public int userid { get; set; }
        public string nickname { get; set; }
        public long uptime { get; set; }
    }
    public class LyricData
    {
        public int version { get; set; }
        public string lyric { get; set; }
    }

    public class NeteaseLyric
    {
        public LyricData lrc { get; set; }
        public LyricData klyric { get; set; }
        public LyricData tlyric { get; set; }
        public int code { get; set; }
    }

    internal class NeteaseMusicDataDownload
    {
        public static async Task<byte[]> GetCoverImage(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 发送GET请求并获取响应
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容并转换为字节数组
                    using (Stream imageStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            await imageStream.CopyToAsync(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"HTTP请求失败：{ex.Message}");
                    return null;
                }
            }
        }

        public static async Task<NeteaseLyric> GetLyric(long music_id)
        {
            try
            {
                string LyricAPI = $"https://music.163.com/api/song/lyric?id={music_id}&lv=1&kv=1&tv=-1";
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(LyricAPI);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<NeteaseLyric>(responseBody);
                    }
                    else
                    {
                        Debug.WriteLine("Failed to request the API. Status code: " + response.StatusCode);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception -> {ex.Message}");
                Debug.WriteLine($"StackTrace:\n {ex.StackTrace}");
                return null;
            }
        }
    }
}
