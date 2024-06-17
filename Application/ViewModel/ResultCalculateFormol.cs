using Application.ViewModel.Nomad.Actions;

namespace Application.ViewModel
{
    public class ResultCalculateFormol
    {
        public List<TradeHistory> MainRealBaseTradeHistories = new();

        public List<TradeHistory> DeletedTradeHistories = new();

        public List<FormolSwitchViewModel> SeletedFormolSwitches { get; set; }

        public string SumHajm { get; set; } = string.Empty;

        public string SumCount { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }
    }
}
