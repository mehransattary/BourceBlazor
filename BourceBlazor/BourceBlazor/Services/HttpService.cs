using AppShared.ViewModel.Nomad.Actions;

namespace BourceBlazor.Services;

public class HttpService: IHttpService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration configuration;

    public HttpService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        this.configuration = configuration;
    }

        /// <summary>
    /// دریافت داده ها 
    /// </summary>
    /// <param name="insCode"></param>
    /// <param name="nomadDate"></param>
    /// <returns></returns>
    public async Task<List<TradeHistory>> GetTradeHistoriesByApi(string insCode, int nomadDate)
    {
        var urlAction = configuration["Urls:UrlAction"];

        var response = await _httpClient.GetFromJsonAsync<Root>(urlAction + insCode + "/" + nomadDate + "/false");

        if (response != null && response.tradeHistory.Any())
        {
          var result =  response.tradeHistory.Where(x => x.canceled == 0)
                                             .DistinctBy(x => new { x.nTran, x.qTitTran, x.hEven })
                                             .Select(item => new TradeHistory
                                             {
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

                                             })
                                             .OrderBy(_ => _.nTran)
                                             .Skip(45).Take(50)
                                             .ToList();
                                            
            return result;
        }

        return new List<TradeHistory>();
    }

}
