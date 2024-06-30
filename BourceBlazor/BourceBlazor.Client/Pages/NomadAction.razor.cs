using Domain.Entities;
using AppShared.Helper;
using Application.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using BlazorInputTags;
using Microsoft.JSInterop;
using Application.ViewModel;
using Application.ViewModel.Nomad.ClosingPriceDaily;
using Application.ViewModel.Nomad.Actions;
using System.Runtime.CompilerServices;
namespace BourceBlazor.Client.Pages;

/// <summary>
/// گرید
/// </summary>
public partial class NomadAction
{
    #region Parameter

    //==========Parameter========================//

    [Parameter]
    public string InsCode { get; set; } = string.Empty;

    [Parameter]
    public string NomadName { get; set; } = string.Empty;

    [Parameter]
    public int NomadDate { get; set; }

    #endregion

    #region Fileds

    //==========Fileds========================//


    private string Title { get; set; } = string.Empty;


    private Grid<TradeHistory> grid = default!;

    public bool IsLoad { get; set; } = true;

    private IEnumerable<TradeHistory> TradeHistories = default!;

    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 0;

    public bool Reload { get; set; } 


    #endregion

    #region Methods   

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
        try
        {         

            var response = await httpClient
                .GetFromJsonAsync<List<TradeHistory>>($"/api/TradeHistory/{InsCode}/{NomadDate}/{Skip}/{Take}/{Reload}");

            if (response != null && response.Any())
            {
                TradeHistories = response;
                await GetTradeHistoriesAndSumHajmCount();
                return TradeHistories;
            }

            return new List<TradeHistory>();
        }
        catch (Exception)
        {
            return new List<TradeHistory>();
        }
    }

    private string GetRowClass(TradeHistory item)
    {      
       return "table-success";       
    }
    private async Task GetTradeHistoriesAndSumHajmCount()
    {
        await GetFillHajms();
        SetSumHajmAndCount();
        StateHasChanged();
    }

    private List<FormolSwitchViewModel> SeletedFormolSwitches { get; set; } = new();

    private void GetEventCallbackSelectedFormolSwitches(List<FormolSwitchViewModel> formolSwitches)
    {
        SeletedFormolSwitches = formolSwitches;
    }

    #endregion
}

/// <summary>
///فرمول نویسی
/// </summary>
public partial class NomadAction
{
    //ردیف های واقعی
    private List<TradeHistory> MainRealBaseTradeHistories = new();

    //ردیف های پایه
    private List<TradeHistory> BaseTradeHistories = new();  

    private async Task GetFilterByFormol(bool isDataRemoved)
    {
        Reload = false;
        await EnableLoadGrid();

        var formolSendActions = new List<FormolSendAction>();
        
        SeletedFormolSwitches.ForEach(item =>
        {
            var model = new FormolSendAction
            {
                CalculationPrice = item.Formol.CalculationPrice,
                NomadDate = NomadDate,
                InsCode = InsCode,
                HajmFormol = item.Formol.HajmFormol,
                MultiStage = item.Formol.MultiStage,
                TimeFormol = item.Formol.TimeFormol,
                IsDataRemoved = isDataRemoved,
            };

            formolSendActions.Add(model);
        });       

        var response = await httpClient.PostAsJsonAsync($"/GetCalculateFormols/{Skip}/{Take}/{Reload}", formolSendActions);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadFromJsonAsync<List<TradeHistory>>();

            TradeHistories = responseContent;
            SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
            SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");
            await grid.RefreshDataAsync();
            await DisableLoadGrid();

        }
        else
        {
            Console.WriteLine("Error: " + response.StatusCode);
        }

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
            IsCleanNomad = true;
            HajmsTags.Clear();
            NomadName = string.Empty;    
            InsCode = string.Empty;
            NomadDate = 0;
            TradeHistories = null;
            await grid.RefreshDataAsync();
            SetEmptySumHajmAndCount();
            IsCleanNomad = false;
        }
    }
}

/// <summary>
///  جستجوی تاریخ
/// </summary>
public partial class NomadAction
{
    public bool ISChangeFormols { get; set; }
    public bool IsCleanNomad { get; set; }
    private async Task GetEventCallbackOnChangeDate(ClosingPriceDaily closingPriceDaily)
    {
        Reload = false;

        if (closingPriceDaily == null)
        {
            await DisableLoadGrid();
            SetEmptySumHajmAndCount();
            SetEmptBaseFormol();
            TradeHistories = new List<TradeHistory>();
            NomadDate = 0;
            grid.Data = TradeHistories;
            await grid.RefreshDataAsync();
        }
        else
        {
            ISChangeFormols = true;
            NomadDate = closingPriceDaily.dEven;
            await ReoladGrid();
            ISChangeFormols = false;
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

    private async Task ReloadGrid()
    {
        Reload = true;
        await EnableLoadGrid();
        await GetDataGrid();
        FilterTradeHistoriesByHajms();
        SetSumHajmAndCount();
        await DisableLoadGrid();
        await grid.RefreshDataAsync();
        StateHasChanged();
    }

    private async Task DoFilterOnGrid()
    {
        Reload = false;

        await EnableLoadGrid();

        await GetDataGrid();

        await GetTradeHistoriesAndSumHajmCount();

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

    private void SetEmptBaseFormol()
    {
        MainRealBaseTradeHistories.Clear();
        BaseTradeHistories.Clear();
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
