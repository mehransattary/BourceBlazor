using AppShared.Entities;
using AppShared.ViewModel;
using AppShared.ViewModel.Nomad.Actions;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages.Components;

public partial class FormolsByCode
{
    [Parameter]
    public string InsCode { get; set; } = string.Empty;

    [Parameter]
    public bool ISChangeFormols { get; set; } = false;

    [Parameter]
    public bool IsCleanNomad { get; set; } = false;

    [Parameter]
    public IEnumerable<TradeHistory>? TradeHistories { get; set; }

    [Parameter]
    public EventCallback<List<FormolSwitchViewModel>> EventCallbackSelectedFormolSwitches { get; set; }


    private IEnumerable<Formol> Formols = new List<Formol>();

    private List<FormolSwitchViewModel> FormolSwitches { get; set; } = new();

    private int SelectedFormolCounter = 0;

    private int MainHajm = 0;

    private int MainTime = 0;


    protected override void OnInitialized()
    {
        Formols = new List<Formol>();
        FormolSwitches.Clear();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!string.IsNullOrEmpty(InsCode) && !Formols.Any() && !FormolSwitches.Any() && ISChangeFormols)
            await GetFormols();

        if (IsCleanNomad)
        {
            Formols = new List<Formol>();
            FormolSwitches.Clear();
            SelectedFormolCounter = 0;
        }
    }

    private async Task GetFormols()
    {
        Formols = await httpClient.GetFromJsonAsync<IEnumerable<Formol>>($"/GetFormolsByInCode/{InsCode}");

        if (Formols != null)
        {
            foreach (var formol in Formols)
            {
                var model = new FormolSwitchViewModel()
                {
                    Formol = formol,
                };

                FormolSwitches.Add(model);
            }

            FormolSwitches = FormolSwitches
                            .OrderBy(x => x.Counter)
                            .ThenByDescending(X => X.Checked)
                            .ToList();
        }
    }

    private void AddToFormolSwitchViewModel(FormolSwitchViewModel formolSwitch)
    {
        if (!formolSwitch.Checked)
        {
            SetActiveFormol(formolSwitch);
            SetFormolOnData(formolSwitch);
        }
        else
        {
            SetDisableFormole(formolSwitch);
        }

        SetOrderFormolSwitches();

        SetEventCallbackSelectedFormolSwitches();

        StateHasChanged();
    }

    private void SetOrderFormolSwitches()
    {
        FormolSwitches = FormolSwitches
                         .OrderBy(x => x.Counter == 0 ? 1 : 0)
                         .ThenBy(x => x.Counter)
                         .ToList();

    }

    private void SetEventCallbackSelectedFormolSwitches()
    {
        var selectedFormolSwitch = FormolSwitches.Where(x => x.Checked && x.Counter > 0).ToList();

        EventCallbackSelectedFormolSwitches.InvokeAsync(selectedFormolSwitch);
    }

    private void SetDisableFormole(FormolSwitchViewModel formolSwitch)
    {
        UpdateCounterShift();

        formolSwitch.Checked = false;
        formolSwitch.Counter = 0;
        formolSwitch.BeforeTradeHistories = new();
        formolSwitch.AfterTradeHistories = new();

        SelectedFormolCounter =0;

        void UpdateCounterShift()
        {
            var valuesCurrentCounterLesser = FormolSwitches
                                            .Where(x => x.Counter > formolSwitch.Counter)
                                            .ToList();

            foreach (var item in FormolSwitches)
            {
                if (valuesCurrentCounterLesser.Contains(item))
                {
                    item.Counter = item.Counter - 1;
                }
            }
        }
    }

    private void SetActiveFormol(FormolSwitchViewModel formolSwitch)
    {
        formolSwitch.Checked = true;
        formolSwitch.Counter = SelectedFormolCounter + 1;
        formolSwitch.BeforeTradeHistories.AddRange(TradeHistories);
        formolSwitch.AfterTradeHistories.AddRange(TradeHistories);

        SelectedFormolCounter += 1;
    }

    private void SetFormolOnData(FormolSwitchViewModel formolSwitch)
    {
        var TradeHistoriesList = TradeHistories.ToList();

        var firstItem = TradeHistoriesList.OrderBy(_ => _.nTran).FirstOrDefault();

        if (firstItem == null)
            return;

        MainHajm = firstItem.qTitTran;
        MainTime = firstItem.hEven + formolSwitch.Formol.TimeFormol;

        var listSelected = new List<TradeHistory>();

        listSelected.Add(firstItem);

        var TradeHistoriesListFormoly = TradeHistoriesList.Where(t => t.hEven <= MainTime).ToList();

        foreach (var tradeHistorie in TradeHistoriesListFormoly)
        {
            if(tradeHistorie.Counter!= firstItem.Counter)
            {
                listSelected.Add(tradeHistorie);

                var validation = (tradeHistorie.qTitTran + MainHajm) > formolSwitch.Formol.HajmFormol;

                MainHajm += tradeHistorie.qTitTran;

                if (validation)
                {
                    listSelected.ForEach(x =>
                    {
                        TradeHistoriesList.Remove(x);
                    });
                }
            }
        }

        TradeHistories = TradeHistoriesList;
    }
}
