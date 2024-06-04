

using AppShared.Entities;
using AppShared.ViewModel.Nomad.Actions;

namespace AppShared.ViewModel;

public class FormolSwitchViewModel
{
    public Formol Formol { get; set; }
    public List<TradeHistory> BeforeTradeHistories { get; set; } = new();
    public List<TradeHistory> AfterTradeHistories { get; set; } = new();
    public bool Checked { get; set; }
    public int Counter { get; set; }

}
