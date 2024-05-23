using AppShared.Helper;
using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages;

public partial class NomadAction
{
    #region <----------> Fields

    [Parameter]
    public string InsCode { get; set; }

    [Parameter]
    public string NomadName { get; set; }

    [Parameter]
    public int NomadDate { get; set; }

    public string Title { get; set; }

    Grid<TradeHistory> grid = default!;

    public bool IsLoad { get; set; } = true;

    private IEnumerable<TradeHistory> TradeHistories = default!;

    private HashSet<TradeHistory> selectedEmployees = new();

    #endregion

    #region <----------> Methods
    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(InsCode))
        {
            Title = $"{NomadName}  {NomadDate.ToPersianDate()} ";          
        }
    }

    /// <summary>
    /// لود اولیه برای گرید
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<GridDataProviderResult<TradeHistory>> GetDataProvider(GridDataProviderRequest<TradeHistory> request)
    {
        if (TradeHistories is null)
        {
            TradeHistories = await GetData();
            IsLoad = !IsLoad;
        }
        StateHasChanged();

        return request.ApplyTo(TradeHistories);
    }

    /// <summary>
    /// دیتا را از وب سرویس بگیر
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    private async Task<IEnumerable<TradeHistory>> GetData()
    {
        var urlAction = configuration["Urls:UrlAction"];

        try
        {
            var response = await httpClient.GetFromJsonAsync<Root>(urlAction + InsCode + "/" + NomadDate + "/false");

            if (response != null && response.tradeHistory.Any())
            {

                TradeHistories = response.tradeHistory.Select((item, index) => new TradeHistory
                {
                    nTran = item.nTran,
                    hEven = item.hEven,
                    qTitTran = item.qTitTran,
                    pTran = item.pTran,
                    canceled = item.canceled
                }).OrderBy(_ => _.nTran);

                return TradeHistories;
            }

            return new List<TradeHistory>();
        }
        catch (Exception)
        {
            return new List<TradeHistory>();
        }

    }

    /// <summary>
    /// انتخاب شونده ها توسط چک باکس ها
    /// </summary>
    /// <param name="employees"></param>
    /// <returns></returns>
    private Task OnSelectedItemsChanged(HashSet<TradeHistory> employees)
    {
        selectedEmployees = employees is not null && employees.Any() ? employees : new();
        return Task.CompletedTask;
    }
    
    #endregion
}

public partial class NomadAction
{
    private IEnumerable<InstrumentSearch> instrumentSearches = default!;

    private string? searchNomadName;
    private async Task<AutoCompleteDataProviderResult<InstrumentSearch>> GetNomadDataProvider(AutoCompleteDataProviderRequest<InstrumentSearch> request)
    {
        if (instrumentSearches is null)
        {
            instrumentSearches = await GetData(searchNomadName);
        }
        return request.ApplyTo(instrumentSearches);
    }

    /// <summary>
    /// دیتا را از وب سرویس بگیر
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    private async Task<IEnumerable<InstrumentSearch>> GetData(string search)
    {
        search = (string.IsNullOrEmpty(search)) ? "خودرو" : search;
        if (string.IsNullOrEmpty(search))
        {
            return new List<InstrumentSearch>();
        }
        var urlSearch = configuration["Urls:UrlSearch"];
        try
        {
            var response = await httpClient.GetFromJsonAsync<RootInstrument>(urlSearch + search);

            if (response != null && response.instrumentSearch.Any())
            {

                instrumentSearches = response.instrumentSearch.Select((item, index) => new InstrumentSearch
                {
                    Counter = ++index,
                    lVal30 = item.lVal30,
                    lVal18AFC = item.lVal18AFC,
                    insCode = item.insCode,
                    flowTitle = item.flowTitle
                });

                return instrumentSearches;
            }

            return new List<InstrumentSearch>();
        }
        catch (Exception)
        {
            return new List<InstrumentSearch>();
        }
    }

    private void OnAutoCompleteChanged(InstrumentSearch instrumentSearch)
    {
        Console.WriteLine($"'{instrumentSearch?.insCode}' selected.");
    }
}

