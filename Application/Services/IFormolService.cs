using Application.ViewModel;
using Application.ViewModel.Nomad.Actions;

namespace Application.Services;

public interface IFormolService
{
    Task<ResultCalculateFormol> GetFilterByFormolAll(List<FormolSendAction> formols,
                                                     List<TradeHistory> TradeHistoriesList);

}
