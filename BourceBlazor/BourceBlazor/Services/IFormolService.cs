using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel;
using System.Net.Http;

namespace BourceBlazor.Services;

public interface IFormolService
{
    Task<ResultCalculateFormol> GetFilterByFormolAll(List<FormolSendAction> formols ,
                                                     List<TradeHistory> TradeHistoriesList);

}
