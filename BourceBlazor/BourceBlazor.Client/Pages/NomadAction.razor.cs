using AppShared.Entities;
using AppShared.Helper;
using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel.Nomad.ClosingPriceDaily;
using AppShared.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using BlazorInputTags;
using System.Xml.Serialization;

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

    private List<Hajm> Hajms = new List<Hajm>();

    private HashSet<TradeHistory> selectedEmployees = new();

    private InputTagOptions InputTagOptions { get; set; } = new();

    private List<string> HajmsTags { get; set; } = new();

    public string SumHajm { get; set; }

    public string SumCount { get; set; }

    #endregion

    #region <----------> Methods
    protected override async Task OnInitializedAsync()
    {
        InputTagOptions = new InputTagOptions()
        {
            DisplayLabel = false,
            InputPlaceholder = "حجم را وارد نمائید و اینتر بزنید...",
        };

        searchNomadDate = "140";

        if (!string.IsNullOrEmpty(InsCode))
        {
            Title = $"{NomadName}  {NomadDate.ToPersianDate()} ";
            searchNomadName = NomadName;
            searchNomadDate = NomadDate.ToPersianDate();
        }
    }

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

    private async Task<IEnumerable<TradeHistory>> GetData()
    {
        var urlAction = configuration["Urls:UrlAction"];

        try
        {
            var response = await httpClient.GetFromJsonAsync<Root>(urlAction + InsCode + "/" + NomadDate + "/false");

            if (response != null && response.tradeHistory.Any())
            {
                await  GetTradeHistoriesAndSumHajmCount(response.tradeHistory);
                return TradeHistories;
            }

            return new List<TradeHistory>();
        }
        catch (Exception)
        {
            return new List<TradeHistory>();
        }

    }

    private Task OnSelectedItemsChanged(HashSet<TradeHistory> employees)
    {
        selectedEmployees = employees is not null && employees.Any() ? employees : new();
        return Task.CompletedTask;
    }
 
    private void GoBackNomadDate()
    {
        NavigationManager.NavigateTo($"/NomadDates/{InsCode}/{NomadName}");
    }

    private async Task SaveHajm(string tag)
    {
        var hajmModels = new List<Hajm>();

        searchNomadName = InstrumentSearchAuto.Value;

        if (!string.IsNullOrEmpty(searchNomadName) && HajmsTags.Any())
        {
            var hajmModel = new Hajm()
            {
                Name = searchNomadName,
                Code = InsCode,
                HajmValue = Convert.ToInt32(tag)
            };

            hajmModels.Add(hajmModel);

            foreach (var _hajm in hajmModels)
            {
                await httpClient.PostAsJsonAsync<Hajm>("/api/Hajms", _hajm);
            }
           
        }

    }
   
    private async Task DeleteHajm(string tag)
    {
        int _tag = Convert.ToInt32(tag);

        await httpClient.DeleteAsync($"/DeleteHajmsByTagsAndCode/{_tag}/{InsCode}");
        
    }

    private async Task GetTradeHistoriesAndSumHajmCount(List<TradeHistory> tradeHistory)
    {
        await GetFillHajms();     

        TradeHistories = tradeHistory
                        .Where(x => x.canceled == 0)
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

        SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
        SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");
        StateHasChanged();
    }

    private async Task ReoladGrid()
    {       
        TradeHistories = await GetData();    
        await grid.RefreshDataAsync();  
        StateHasChanged();
    }
    private async Task DoFilterOnGrid()
    {
        IsLoad = true;
        grid.Class = "opacity_3";
        await grid.RefreshDataAsync();

        await GetData();
        var hajmsCode = Hajms.Select(x => x.HajmValue);
        TradeHistories = TradeHistories.Where(x => !hajmsCode.Contains(x.qTitTran));
        SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
        SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");  
        
        IsLoad = false;
        grid.Class = "opacity1";
        await grid.RefreshDataAsync();

        StateHasChanged();
    }
    #endregion
}

/// <summary>
/// جستجوی نماد
/// </summary>
public partial class NomadAction
{
    private List<InstrumentSearch> instrumentSearches = new();

    private AutoComplete<InstrumentSearch> InstrumentSearchAuto = default!;

    private string searchNomadName { get; set; }

    private async Task<AutoCompleteDataProviderResult<InstrumentSearch>> GetNomadProvider(AutoCompleteDataProviderRequest<InstrumentSearch> request)
    {
        Hajms.Clear();
        var value = InstrumentSearchAuto.Value;
        value = value.FixPersianChars();
        InstrumentSearchAuto.Value = value;
        await GetNomadData(value);
        return request.ApplyTo(instrumentSearches);
    } 

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
            RootInstrument response = await httpClient.GetFromJsonAsync<RootInstrument>(urlSearch + search);

            if (response != null && response.instrumentSearch.Any())
            {
                instrumentSearches.Clear();

                instrumentSearches = response.instrumentSearch.Select((item, index) => new InstrumentSearch
                {
                    Counter = ++index,
                    //نام کامل
                    lVal30 = item.lVal18AFC +" - "+ item.lVal30,
                    //نام اختصار
                    lVal18AFC = item.lVal18AFC,
                    insCode = item.insCode,
                }).ToList();

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

        if ((!string.IsNullOrEmpty(InsCode)))
        {
            IsLoadSearchNomadDate = true;

            await GetFillHajms();

            HajmsTags = Hajms.Select(x => x.HajmValue.ToString()).ToList();

            closingPriceDailies = await GetNomadDateData();

            IsLoadSearchNomadDate = false;

            StateHasChanged();
        }
        else
        {
            HajmsTags.Clear();
            TradeHistories = null;
            searchNomadDate = "140";
            //await DateAuto.RefreshDataAsync();
            await grid.RefreshDataAsync();
            SumHajm = string.Empty;
            SumCount = string.Empty;
        }
    }

    private async Task GetFillHajms()
    {
        if (!string.IsNullOrEmpty(InsCode))
        {
            Hajms = await httpClient.GetFromJsonAsync<List<Hajm>>($"/GetHajmByCode/{InsCode}");
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

    private async Task<IEnumerable<ClosingPriceDaily>> GetNomadDateData()
    {
        var urlDate = configuration["Urls:UrlDate"];

        try
        {
            if (!string.IsNullOrEmpty(InsCode))
            {
                RootClosingPriceDaily response = await httpClient.GetFromJsonAsync<RootClosingPriceDaily>(urlDate + InsCode + "/0");

                if (response != null && response.closingPriceDaily.Any())
                {
                    closingPriceDailies = response.closingPriceDaily
                                            .Select((item, index) => new ClosingPriceDaily
                                            {
                                                Counter = ++index,
                                                insCode = item.insCode,
                                                dEvenPersian = item.dEven.ToPersianDate(),
                                                dEven = item.dEven
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
        if (closingPriceDaily == null)
        {
            SumHajm = string.Empty;
            SumCount = string.Empty; 
        }
        else
        {
            NomadDate = closingPriceDaily.dEven;
            await ReoladGrid();
        }
    }
}
