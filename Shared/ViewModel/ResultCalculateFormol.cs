using AppShared.ViewModel.Nomad.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppShared.ViewModel
{
    public class ResultCalculateFormol
    {
        public List<TradeHistory> MainRealBaseTradeHistories = new();


        public List<TradeHistory> TradeHistories = new();

        public List<FormolSwitchViewModel> SeletedFormolSwitches { get; set; } 

        public string SumHajm { get; set; } = string.Empty;

        public string SumCount { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }
    }
}
