using Application.ViewModel.Nomad.Actions;

namespace Application.Services;

public interface IHttpService
{
    Task<List<TradeHistory>> GetTradeHistoriesByApi(string insCode, int nomadDate, int skip, int take, bool reload);
}
