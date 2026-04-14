using Chorder.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
namespace Chorder.Clients.Searcher
{
    public class BiliBiliSearcher
    {
        private readonly HttpClient _http = new();

        public async Task<List<BiliBiliSearchItem>> SearchAsync(string keyword)
        {
            try
            {
                string url = $"https://api.bilibili.com/x/web-interface/search/type?search_type=video&keyword={keyword}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Referer", "https://search.bilibili.com");
                request.Headers.Add("User-Agent", "Mozilla/5.0");
                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.GetProperty("data").TryGetProperty("result", out var results))
                {
                    return new List<BiliBiliSearchItem>();
                }
                var items = new List<BiliBiliSearchItem>();
                foreach (var v in results.EnumerateArray())
                {
                    var item = new BiliBiliSearchItem
                    {
                        Title = CleanKeywordHtml(v.GetProperty("title").GetString()),
                        Bvid = v.GetProperty("bvid").GetString(),
                        Author = v.GetProperty("author").GetString(),
                        Duration = v.GetProperty("duration").GetString()
                    };
                    items.Add(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                return new List<BiliBiliSearchItem>();
            }
        }
    
        public async Task<List<BiliBiliPageItem>> GetPageInfo(string bvid)
        {
            try
            {
                string url = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Referer", "https://search.bilibili.com");
                request.Headers.Add("User-Agent", "Mozilla/5.0");
                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.GetProperty("data").TryGetProperty("pages", out var results))
                {
                    return new List<BiliBiliPageItem>();
                }
                var items = new List<BiliBiliPageItem>();
                foreach (var v in results.EnumerateArray())
                {
                    var item = new BiliBiliPageItem
                    {
                        Page = v.GetProperty("page").GetInt32(),
                        Title = v.GetProperty("part").GetString(),
                        Duration = v.GetProperty("duration").GetInt32()
                    };
                    items.Add(item);
                }
                return items;
            }
            catch (Exception ex)
            {
                return new List<BiliBiliPageItem>();
            }
        }
    
        public static string CleanKeywordHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 去掉 <em class="keyword">xxx</em>
            input = Regex.Replace(input, @"<em[^>]*>", "");
            input = Regex.Replace(input, @"</em>", "");

            return input;
        }
    }


}
