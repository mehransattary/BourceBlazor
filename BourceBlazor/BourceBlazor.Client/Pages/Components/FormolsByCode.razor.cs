using Application.ViewModel;
using Application.ViewModel.Nomad.Actions;
using Domain.Entities;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages.Components;

/// <summary>
/// Parameters and Fields
/// </summary>
public partial class FormolsByCode
{
    [Parameter]
    public string InsCode { get; set; } = string.Empty;

    [Parameter]
    public bool IsChangeFormols { get; set; } = false;

    [Parameter]
    public bool IsCleanNomad { get; set; } = false;

    [Parameter]
    public IEnumerable<TradeHistory>? TradeHistories { get; set; }

    [Parameter]
    public EventCallback<List<FormolSwitchViewModel>> EventCallbackSelectedFormolSwitches { get; set; }


    private List<FormolSwitchViewModel> FormolSwitches { get; set; } = new();

    private IEnumerable<Formol> Formols = new List<Formol>();

    private int SelectedFormolCounter = 0;

}

/// <summary>
/// Methods
/// </summary>
public partial class FormolsByCode
{   
    protected override void OnInitialized()
    {
        Formols = new List<Formol>();
        FormolSwitches.Clear();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var validation = !string.IsNullOrEmpty(InsCode) &&
                         !Formols.Any() &&
                         !FormolSwitches.Any() &&
                         IsChangeFormols;

        if (validation)
        {
            await SetFormolSwitches();
        }

        if (IsCleanNomad)
        {
            Formols = new List<Formol>();
            FormolSwitches.Clear();
            SelectedFormolCounter = 0;
        }
    }

    private async Task SetFormolSwitches()
    {
        FormolSwitches.Clear();

        var formols =await GetFormols();

        Formols = formols;

        if (!Formols.Any())
        {
            return;
        }

        foreach (var formol in Formols)
        {
            var model = new FormolSwitchViewModel()
            {
                Formol = formol
            };

            FormolSwitches.Add(model);
        }

        FormolSwitches = FormolSwitches
                        .OrderBy(x => x.Counter)
                        .ThenByDescending(X => X.Checked)
                        .ToList();
    }

    private async Task<IEnumerable<Formol>> GetFormols()
    {
        IEnumerable<Formol>? Formolsenumerable =
           await httpClient.GetFromJsonAsync<IEnumerable<Formol>>($"/GetFormolsByInCode/{InsCode}");

        if (Formolsenumerable == null)
        {
            return new List<Formol>();
        }

        return Formolsenumerable;
    }

    private void AddToFormolSwitchViewModel(FormolSwitchViewModel formolSwitch)
    {
        if (!formolSwitch.Checked)
        {
            SetActiveFormol(formolSwitch);
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
        SelectedFormolCounter = 0;

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
        SelectedFormolCounter += 1;
    }


}