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
using BourceBlazor.Client.Pages.Components;

namespace BourceBlazor.Client.Pages;

/// <summary>
/// گرید
/// </summary>
public partial class NomadAction
{
    //==========Parameter========================//

    [Parameter]
    public string InsCode { get; set; } = string.Empty;

    [Parameter]
    public string NomadName { get; set; } = string.Empty;

    [Parameter]
    public int NomadDate { get; set; }


    //==========Fileds========================//
    private string Title { get; set; } = string.Empty;

    private Grid<TradeHistory> grid = default!;

    public bool IsLoad { get; set; } = true;

    private IEnumerable<TradeHistory> TradeHistories = default!;


    //==========Methods========================//

    protected override void OnInitialized()
    {
        InputTagOptions = new InputTagOptions()
        {
            DisplayLabel = false,
            InputPlaceholder = "حجم را وارد نمائید و اینتر بزنید...",
        };

        if (!string.IsNullOrEmpty(InsCode))
        {
            Title = $"{NomadName}  {NomadDate.ToPersianDate()} ";
        }
    }

    private async Task<GridDataProviderResult<TradeHistory>> GetDataProvider(GridDataProviderRequest<TradeHistory> request)
    {
        if (TradeHistories is null)
        {
            TradeHistories = await GetDataGrid();
            IsLoad = false;
        }

        StateHasChanged();
        return request.ApplyTo(TradeHistories);
    }

    private async Task<IEnumerable<TradeHistory>> GetDataGrid()
    {
        var urlAction = configuration["Urls:UrlAction"];

        try
        {
            var response = await httpClient.GetFromJsonAsync<Root>(urlAction + InsCode + "/" + NomadDate + "/false");

            if (response != null && response.tradeHistory.Any())
            {
                await GetTradeHistoriesAndSumHajmCount(response.tradeHistory);
                return TradeHistories;
            }

            return new List<TradeHistory>();
        }
        catch (Exception)
        {
            return new List<TradeHistory>();
        }

    }

    private async Task GetTradeHistoriesAndSumHajmCount(List<TradeHistory> tradeHistory)
    {
        await GetFillHajms();

        GetTradeHistories(tradeHistory);

        SetSumHajmAndCount();

        StateHasChanged();
    }

    private void GetTradeHistories(List<TradeHistory> tradeHistory)
    {
        TradeHistories = tradeHistory
                   .Where(x => x.canceled == 0)
                   .DistinctBy(x => new { x.nTran, x.qTitTran, x.hEven })
                   .Select((item, index) => new TradeHistory
                   {
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
    } 
  
}

/// <summary>
/// جستجوی نماد
/// </summary>
public partial class NomadAction
{
    private async Task GetEventCallbackInstrumentSearch(InstrumentSearch instrumentSearch)
    {

        if (instrumentSearch != null)
        {
            InsCode = instrumentSearch!.insCode!;
            NomadName = instrumentSearch!.lVal30;
            await GetFillHajms();
            
            StateHasChanged();
        }
        else
        {
            HajmsTags.Clear();
            NomadName = string.Empty;
            InsCode = string.Empty;
            TradeHistories = null;
            await grid.RefreshDataAsync();
            SetEmptySumHajmAndCount();
        }
    }
}

/// <summary>
///  جستجوی تاریخ
/// </summary>
public partial class NomadAction
{
    private async Task GetEventCallbackOnChangeDate(ClosingPriceDaily closingPriceDaily)
    {
        if (closingPriceDaily == null)
        {
            SetEmptySumHajmAndCount();
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

/// <summary>
/// حجم
/// </summary>
public partial class NomadAction
{

    //==========Fileds========================//

    private InputTagOptions InputTagOptions { get; set; } = new();

    private List<string> HajmsTags { get; set; } = new();

    //==========Methods========================//

    private async Task SaveHajm(string tag)
    {
        var hajmModels = new List<Hajm>();

        if (!string.IsNullOrEmpty(NomadName) && HajmsTags.Any())
        {
            var hajmModel = new Hajm()
            {
                Name = NomadName,
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

    private async Task DoFilterOnGrid()
    {
        await EnableLoadGrid();

        await GetDataGrid();

        FilterTradeHistoriesByHajms();

        SetSumHajmAndCount();

        await DisableLoadGrid();

        await grid.RefreshDataAsync();

        StateHasChanged();
    }
}

/// <summary>
/// متفرقه
/// </summary>
public partial class NomadAction
{
    //==========Fileds========================//

    private Collapse collapse1 = default!;

    private List<Hajm> Hajms = new List<Hajm>();
    private string SumHajm { get; set; } = string.Empty;
    private string SumCount { get; set; } = string.Empty;


    //==========Methods========================//

    private async Task ToggleContentAsync() => await collapse1.ToggleAsync();

    private async Task GetFillHajms()
    {
        if (!string.IsNullOrEmpty(InsCode))
        {
            Hajms = await httpClient.GetFromJsonAsync<List<Hajm>>($"/GetHajmByCode/{InsCode}");

            if (Hajms!.Any())
                HajmsTags = Hajms!.Select(x => x.HajmValue.ToString()).ToList();
        }
    }

    private void SetEmptySumHajmAndCount()
    {
        SumHajm = string.Empty;
        SumCount = string.Empty;
    }

    private void SetSumHajmAndCount()
    {
        SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
        SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");
    }

    private void GoBackNomadDate()
    {
        NavigationManager.NavigateTo($"/NomadDates/{InsCode}/{NomadName}");
    }

    private async Task ReoladGrid()
    {
        IsLoad = true;
        TradeHistories = await GetDataGrid();
        await grid.RefreshDataAsync();
        IsLoad = false;
        StateHasChanged();
    }

    private async Task EnableLoadGrid()
    {
        IsLoad = true;
        await jsRunTime.InvokeVoidAsync("SetOpacity_3");
    }

    private async Task DisableLoadGrid()
    {
        IsLoad = false;
        await jsRunTime.InvokeVoidAsync("SetOpacityFull");
    }

    private void FilterTradeHistoriesByHajms()
    {
        var hajmsCode = Hajms.Select(x => x.HajmValue);
        TradeHistories = TradeHistories.Where(x => !hajmsCode.Contains(x.qTitTran));
    }
}
