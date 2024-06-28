using Application.ViewModel.Nomad.Actions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http.Json;

namespace Application.Services;

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration configuration;
    private readonly IMemoryCache _memoryCache;

    public HttpService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        this.configuration = configuration;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// دریافت داده ها 
    /// </summary>
    /// <param name="insCode"></param>
    /// <param name="nomadDate"></param>
    /// <returns></returns>
    public async Task<List<TradeHistory>> GetTradeHistoriesByApi(string insCode, int nomadDate, int skip, int take, bool reload)
    {
        var urlAction = configuration["Urls:UrlAction"];

        string cacheKey = $"cache_tradeHistories_{insCode}_{nomadDate}";

        var cachedData = _memoryCache.Get<Root>(cacheKey);

        if (cachedData == null)
        {
            cachedData = await _httpClient.GetFromJsonAsync<Root>(urlAction + insCode + "/" + nomadDate + "/false");

            _memoryCache.Set(cacheKey, cachedData);
        }

        if(reload)
        {
            cachedData = await _httpClient.GetFromJsonAsync<Root>(urlAction + insCode + "/" + nomadDate + "/false");
            skip = 0;
            take = 0;
            _memoryCache.Set(cacheKey, cachedData);
        }

        if (cachedData != null && cachedData.tradeHistory.Any())
        {

            var result = cachedData.tradeHistory.Where(x => x.canceled == 0)
                                           .DistinctBy(x => new { x.nTran, x.qTitTran, x.hEven })
                                           .OrderBy(_ => _.nTran)
                                           .Select((item, index) => new TradeHistory
                                           {
                                               Counter = index + 1,
                                               //ردیف
                                               nTran = item.nTran,
                                               //زمان
                                               hEven = item.hEven,
                                               //حجم
                                               qTitTran = item.qTitTran,
                                               //قیمت
                                               pTran = item.pTran,
                                               //لغو شده
                                               canceled = item.canceled

                                           }).ToList();

            if (skip!=0 && take!=0)
            {
                result = result.Skip(skip-1).Take(take).ToList();
            }
            else
            {
                result = result.ToList();
            }                                 

            return result;
        }

        return new List<TradeHistory>();
    }

}
