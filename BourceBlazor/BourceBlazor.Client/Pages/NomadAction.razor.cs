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
using Microsoft.JSInterop;

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

    private Collapse collapse1 = default!;

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

    private async Task ToggleContentAsync() => await collapse1.ToggleAsync();

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

        searchNomadName = InstrumentNomadSearchAuto.Value;

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
                        .DistinctBy(x => new {x.nTran, x.qTitTran, x.hEven})
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
        IsLoad = true;
        TradeHistories = await GetData();    
        await grid.RefreshDataAsync();
        IsLoad = false;
        StateHasChanged();
    }

    private async Task DoFilterOnGrid()
    {
        IsLoad = true;
        await jsRunTime.InvokeVoidAsync("SetOpacity_3");
        await GetData();
        var hajmsCode = Hajms.Select(x => x.HajmValue);
        TradeHistories = TradeHistories.Where(x => !hajmsCode.Contains(x.qTitTran));
        SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
        SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");  
        await jsRunTime.InvokeVoidAsync("SetOpacityFull");
        IsLoad = false;
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
    private List<InstrumentSearch> instrumentNomadSearches = new();

    private AutoComplete<InstrumentSearch> InstrumentNomadSearchAuto = default!;

    private string searchNomadName { get; set; }

    private async Task<AutoCompleteDataProviderResult<InstrumentSearch>> GetNomadProvider(AutoCompleteDataProviderRequest<InstrumentSearch> request)
    {
        Hajms.Clear();
        var value = request.Filter.Value.FixPersianChars();
         InstrumentNomadSearchAuto.Value = value;
        await GetNomadData(value);
        return request.ApplyTo(instrumentNomadSearches);
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
                instrumentNomadSearches.Clear();

                instrumentNomadSearches = response.instrumentSearch.Select((item, index) => new InstrumentSearch
                {
                    Counter = ++index,
                    //نام کامل
                    lVal30 = item.lVal18AFC +" - "+ item.lVal30,
                    //نام اختصار
                    lVal18AFC = item.lVal18AFC,
                    insCode = item.insCode,
                }).ToList();

                return instrumentNomadSearches;
            }

            return new List<InstrumentSearch>();
        }
        catch (Exception ex)
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
            if (!string.IsNullOrEmpty(InsCode) && InsCode!="0")
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
            searchNomadDate = "140";

            TradeHistories = new List<TradeHistory>();
            grid.Data = TradeHistories;
            await grid.RefreshDataAsync();

        }
        else
        {
            NomadDate = closingPriceDaily.dEven;
            await ReoladGrid();
        }
    }
}
