using AppShared.Helper;
using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel.Nomad.ClosingPriceDaily;
using AppShared.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

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

    /// <summary>
    ///حجم معاملات
    /// </summary>
    public string SumHajm { get; set; }

    /// <summary>
    ///تعداد معاملات 
    /// </summary>
    public string SumCount { get; set; }

    #endregion

    #region <----------> Methods
    protected override async Task OnInitializedAsync()
    {
        searchNomadDate = "140";

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
            IsLoad = false;
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
                TradeHistories = response.tradeHistory
                                .Where(x => x.canceled==0 )
                                .Select((item, index) => new TradeHistory
                                {
                                    //Counter = ++index,
                                    //ردیف
                                    nTran = item.nTran,
                                    //زمان
                                    hEven = item.hEven,
                                    //حجم
                                    qTitTran = item.qTitTran,
                                    //قیمت
                                    pTran = item.pTran,
                                    canceled = item.canceled
                                })
                                .OrderBy(_ => _.nTran);

                SumHajm = TradeHistories.Select(x=>x.qTitTran).Sum().ToString("#,0");

                SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");

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

    /// <summary>
    /// برگشتن به صفحه تاریخ نماد ها
    /// </summary>
    private void GoBackNomadDate()
    {
        NavigationManager.NavigateTo($"/NomadDates/{InsCode}/{NomadName}");
    }
    #endregion
}

/// <summary>
/// جستجوی نماد
/// </summary>
public partial class NomadAction
{
    private IEnumerable<InstrumentSearch> instrumentSearches = default!;

    AutoComplete<InstrumentSearch> InstrumentSearchAuto = default!;
    public string searchNomadName { get; set; }
    private async Task<AutoCompleteDataProviderResult<InstrumentSearch>> GetNomadProvider(AutoCompleteDataProviderRequest<InstrumentSearch> request)
    {
        var value = InstrumentSearchAuto.Value;
        instrumentSearches = await GetNomadData(value);
        return request.ApplyTo(instrumentSearches);
    }

    /// <summary>
    /// دیتا را از وب سرویس بگیر
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    private async Task<IEnumerable<InstrumentSearch>> GetNomadData(string search)
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

    private async Task OnNomadAutoCompleteChanged(InstrumentSearch instrumentSearch)
    {    
        InsCode = instrumentSearch?.insCode;

        if (closingPriceDailies is null && (!string.IsNullOrEmpty(InsCode)))
        {
            IsLoadSearchNomadDate = true;
            StateHasChanged();

            closingPriceDailies = await GetNomadDateData();

            IsLoadSearchNomadDate = false;
            StateHasChanged();
        }
    }
}

/// <summary>
///  جستجوی تاریخ
/// </summary>
public partial class NomadAction
{
    private IEnumerable<ClosingPriceDaily> closingPriceDailies = default!;

    AutoComplete<ClosingPriceDaily> DateAuto = default!;

    private string? searchNomadDate;

    public bool IsLoadSearchNomadDate { get; set; } = false;

    private async Task<AutoCompleteDataProviderResult<ClosingPriceDaily>> GetNomadDataProvider(AutoCompleteDataProviderRequest<ClosingPriceDaily> request)
    {   
        if (closingPriceDailies is null && (!string.IsNullOrEmpty(InsCode)))
        {
            closingPriceDailies = await GetNomadDateData();
        }
        return await Task.FromResult(request.ApplyTo(closingPriceDailies.OrderBy(customer => customer.Counter)));
    }

    /// <summary>
    /// دیتا را از وب سرویس بگیر
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    private async Task<IEnumerable<ClosingPriceDaily>> GetNomadDateData()
    {
        var urlDate = configuration["Urls:UrlDate"];

        try
        {
            if(!string.IsNullOrEmpty(InsCode))
            {            
                var response = await httpClient.GetFromJsonAsync<RootClosingPriceDaily>(urlDate + InsCode + "/0");

                if (response != null && response.closingPriceDaily.Any())
                {
                    closingPriceDailies = response.closingPriceDaily.Select((item, index) => new ClosingPriceDaily
                    {
                        Counter = ++index,
                        insCode = item.insCode,
                        dEvenPersian = item.dEven.ToPersianDate(),    
                        dEven =item.dEven
                    });

                    return closingPriceDailies;
                }
            }
            return new List<ClosingPriceDaily>();
        }
        catch (Exception)
        {
            return new List<ClosingPriceDaily>();
        }
    }

    private async Task OnNomadDateAutoCompleteChanged(ClosingPriceDaily closingPriceDaily)
    {
        NomadDate = closingPriceDaily.dEven;
        TradeHistories = await GetData();
        await grid.RefreshDataAsync();
        StateHasChanged();
    }
}
