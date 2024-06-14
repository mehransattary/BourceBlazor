using AppShared.ViewModel.Nomad.Actions;

namespace BourceBlazor.Services;

public interface IHttpService
{
    Task<List<TradeHistory>> GetTradeHistoriesByApi(string insCode, int nomadDate);
}
