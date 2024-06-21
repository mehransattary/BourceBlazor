using Application.ViewModel.Nomad.Actions;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http.Json;

namespace Application.Services;

public class HttpService : IHttpService
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
    public async Task<List<TradeHistory>> GetTradeHistoriesByApi(string insCode, int nomadDate, int skip, int take)
    {
        var urlAction = configuration["Urls:UrlAction"];

        var response = await _httpClient.GetFromJsonAsync<Root>(urlAction + insCode + "/" + nomadDate + "/false");

        if (response != null && response.tradeHistory.Any())
        {

            var result = response.tradeHistory.Where(x => x.canceled == 0)
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

                                           }).OrderBy(_ => _.nTran).ToList();

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
