using Chorder.Clients.Searcher;
using Chorder.Models;
using Chorder.Models.Entities;

namespace Chorder.Services
{
    public class SearchService
    {
        private readonly BiliBiliSearcher _BiliBiliSearcher;

        public SearchService(BiliBiliSearcher searcher)
        {
            _BiliBiliSearcher = searcher;
        }

        public async Task<List<BiliBiliSearchItem>> SearchAsync(string keyword)
        {
            var result=await _BiliBiliSearcher.SearchAsync(keyword);
            return result;
        }
        public async Task<List<BiliBiliPageItem>> GetPageInfo(string bvid)
        {
            var result=await _BiliBiliSearcher.GetPageInfo(bvid);
            return result;
        }
    }
}
