using AppShared.Entities;
using AppShared.Helper;
using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel.Nomad.ClosingPriceDaily;
using AppShared.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using BlazorInputTags;
using Microsoft.JSInterop;
using AppShared.ViewModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
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

    private List<TradeHistory> TradeHistoriesList = new();


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
                          .OrderBy(_ => _.nTran).ToList();

        TradeHistoriesList = TradeHistories.ToList();
    }

    private List<FormolSwitchViewModel> SeletedFormolSwitches { get; set; } = new();

    private void GetEventCallbackSelectedFormolSwitches(List<FormolSwitchViewModel> formolSwitches)
    {
        SeletedFormolSwitches = formolSwitches;
    }

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

    //مقادیر  ردیف پایه
    private BaseTradeHistoriesViewModel BaseTradeHistoriesViewModel { get; set; } = new();

    //فرمول
    private FormolSwitchViewModel formol = new();

    //مرحله جاری
    private int CurrentMultiStage = 1;

    //اتمام مرحله
    private bool Restart = true;

    private async Task GetFilterByFormolAll1()
    {
        await EnableLoadGrid();

        var _formol = SeletedFormolSwitches.FirstOrDefault().Formol;

        var model = new FormolSendAction
        {
              CalculationPrice = _formol.CalculationPrice,
              NomadDate= NomadDate,
              InsCode = InsCode ,
              HajmFormol = _formol.HajmFormol,
              MultiStage = _formol.MultiStage,
              TimeFormol = _formol.TimeFormol
        };

        var response = await httpClient.PostAsJsonAsync($"/GetCalculateFormols", model);

        if (response.IsSuccessStatusCode)
        {
            var responseContent =await response.Content.ReadFromJsonAsync<List<TradeHistory>>();

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
    //محاسبه فرمول اصلی
    private async Task GetFilterByFormolAll()
    {
        Restart = true;

        await EnableLoadGrid();

        var _formol = SeletedFormolSwitches.FirstOrDefault();

        if (_formol != null)
            formol = _formol;

        while (Restart)
        {
            BaseTradeHistories.Clear();

            if (TradeHistoriesList.Count <= CurrentMultiStage)
            {
                CurrentMultiStage = 1;
                TradeHistoriesList.Clear();
                TradeHistories = MainRealBaseTradeHistories.AsEnumerable();
                await grid.RefreshDataAsync();
                SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
                SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");
                Restart = false;
                await DisableLoadGrid();
                return;
            }

            var result = TradeHistoriesList
                   .Skip(0)
                   .Take(CurrentMultiStage)
                   .ToList();

            BaseTradeHistories.AddRange(result);

            if (CurrentMultiStage == 1)
            {
                var firstTradeHistories = BaseTradeHistories.FirstOrDefault();

                if (firstTradeHistories == null)
                {
                    await Console.Out.WriteLineAsync("firstTradeHistories is null");
                    return;
                }

                BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
                BaseTradeHistoriesViewModel.BaseHajm = firstTradeHistories.qTitTran;
                BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
                BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.Formol.TimeFormol;
            }
            else if (CurrentMultiStage > 1)
            {
                var firstTradeHistories = BaseTradeHistories.Skip(0).Take(1).FirstOrDefault();

                if (firstTradeHistories == null)
                {
                    await Console.Out.WriteLineAsync("firstTradeHistories is null");
                    return;
                }

                BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
                BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
                BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.Formol.TimeFormol;
                BaseTradeHistoriesViewModel.BaseHajm = BaseTradeHistories.Sum(b => b.qTitTran);
            }

            //محاسبه خود ردیف پایه با فرمول
            if (BaseTradeHistoriesViewModel.BaseHajm == formol.Formol.HajmFormol)
            {
                BaseTradeHistories.ForEach(item => TradeHistoriesList.Remove(item));

                //ریست کن مراحل        
                //تنظیم ردیف پایه جدید
                CurrentMultiStage = 1;

                continue;
            }

            var calculateTradeHistories = TradeHistoriesList
                                         .Skip(CurrentMultiStage)
                                         .Where(x => x.hEven <= BaseTradeHistoriesViewModel.BaseEndTime)
                                         .ToList();

            if (calculateTradeHistories.Count < BaseTradeHistories.Count)
            {
                //اولین ردیف از پایه هارا حذف کن
                var first = BaseTradeHistories.FirstOrDefault();

                if (first != null)
                {
                    TradeHistoriesList.Remove(first);
                    MainRealBaseTradeHistories.Add(first);
                    CurrentMultiStage = 1;
                }

                continue;
            }

            bool isFinshFor = false;
            foreach (var item in calculateTradeHistories)
            {
                if (!isFinshFor)
                {
                    var sum = BaseTradeHistoriesViewModel.BaseHajm + item.qTitTran;

                    if (sum == formol.Formol.HajmFormol)
                    {
                        BaseTradeHistories.ForEach(b => TradeHistoriesList.Remove(b));
                        TradeHistoriesList.Remove(item);
                        CurrentMultiStage = 1;
                        isFinshFor = true;
                    }
                }
            }


            if (isFinshFor)
            {
                continue;
            }

            //مرحله جاری را افزایش بده
            CurrentMultiStage = CurrentMultiStage + 1;

            var multiStage = formol.Formol.MultiStage;

            // در صورتی که به مرحله آخر رسیده باشید
            if (multiStage < CurrentMultiStage)
            {
                var first = BaseTradeHistories.FirstOrDefault();

                if (first != null)
                {
                    TradeHistoriesList.Remove(first);
                    MainRealBaseTradeHistories.Add(first);
                    CurrentMultiStage = 1;
                    continue;
                }
            }

            var counTradeHistoriesListt = TradeHistoriesList.Count;

            if (counTradeHistoriesListt == 0 || TradeHistoriesList.Count <= CurrentMultiStage)
            {
                CurrentMultiStage = 1;
                TradeHistoriesList.Clear();
                TradeHistories = MainRealBaseTradeHistories.AsEnumerable();
                await grid.RefreshDataAsync();
                SumHajm = TradeHistories.Select(x => x.qTitTran).Sum().ToString("#,0");
                SumCount = TradeHistories.Select(x => x.nTran).Count().ToString("#,0");
                Restart = false;
                await DisableLoadGrid();
                return;
            }
        }
    }


    //محاسبه فرمول اصلی
    private void GetFilterByFormol()
    {
        SetFormol();

        if (formol == null)
            return;

        //مرحله اول
        ResetBase();

        CalculatesWithFormol();

        FinshedWhenEmptyCounTradeHistoriesList();
    }

    private void SetFormol()
    {
        var _formol = SeletedFormolSwitches.FirstOrDefault();

        if (_formol != null)
            formol = _formol;
    }

    //============>>

    //مرحله اول
    public void ResetBase()
    {
        //تنظیم ردیف های  پایه
        SetBaseTradeHistories();

        //تنظیم فرمول های اصلی با ردیف های پایه
        SetBaseTradeHistoriesViewModel();

        //محاسبه ردیف پایه با فرمول
        CalculateBaseTradeHistoriesWithFormol();
    }

    private void SetBaseTradeHistories()
    {
        BaseTradeHistories.Clear();

        var result = TradeHistoriesList
                        .Skip(0)
                        .Take(CurrentMultiStage)
                        .ToList();

        BaseTradeHistories.AddRange(result);
    }

    private void SetBaseTradeHistoriesViewModel()
    {
        if (CurrentMultiStage == 1)
        {
            var firstTradeHistories = BaseTradeHistories.FirstOrDefault();

            if (firstTradeHistories == null)
                return;

            BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
            BaseTradeHistoriesViewModel.BaseHajm = firstTradeHistories.qTitTran;
            BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
            BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.Formol.TimeFormol;
        }
        else if (CurrentMultiStage > 1)
        {
            var firstTradeHistories = BaseTradeHistories.Skip(0).Take(1).FirstOrDefault();

            if (firstTradeHistories == null)
                return;

            BaseTradeHistoriesViewModel.BasePrice = firstTradeHistories.pTran;
            BaseTradeHistoriesViewModel.BaseTime = firstTradeHistories.hEven;
            BaseTradeHistoriesViewModel.BaseEndTime = firstTradeHistories.hEven + formol.Formol.TimeFormol;
            BaseTradeHistoriesViewModel.BaseHajm = BaseTradeHistories.Sum(b => b.qTitTran);
        }
    }

    private void CalculateBaseTradeHistoriesWithFormol()
    {
        //محاسبه خود ردیف پایه با فرمول
        if (BaseTradeHistoriesViewModel.BaseHajm == formol.Formol.HajmFormol)
        {
            BaseTradeHistories.ForEach(item => TradeHistoriesList.Remove(item));

            //ریست کن مراحل        
            //تنظیم ردیف پایه جدید
            CurrentMultiStage = 1;
            //............ 
            GetFilterByFormol();
        }
    }

    //============>>

    public void CalculatesWithFormol()
    {
        //محاسبه 
        var calculateTradeHistories = CalculateBaseTradeHistoriesWhenBiggercalculateTradeHistories();

        if (!calculateTradeHistories.Any())
        {
            GetFilterByFormol();
            return;
        }

        //محاسبه اصلی ردیف های پایه با ردیف های دیگر طبق شرط زمانبندی
        MainCalculateTradeHistories(calculateTradeHistories);

        PlusCurrentMultiStage();

        CheckMultiStageWithCurrentMultiStage();
    }


    /// <summary>
    /// معالات رابر اساس مرحله جاری و پایان زمان بیار
    /// </summary>
    /// <returns></returns>
    private List<TradeHistory> CalculateBaseTradeHistoriesWhenBiggercalculateTradeHistories()
    {
        var calculateTradeHistories = TradeHistoriesList
                                      .Skip(CurrentMultiStage)
                                      .Where(x => x.hEven <= BaseTradeHistoriesViewModel.BaseEndTime)
                                      .ToList();

        if (calculateTradeHistories.Count < BaseTradeHistories.Count)
        {
            //اولین ردیف از پایه هارا حذف کن
            var first = BaseTradeHistories.FirstOrDefault();

            if (first != null)
            {
                TradeHistoriesList.Remove(first);
                MainRealBaseTradeHistories.Add(first);
                CurrentMultiStage = 1;
            }

            return new List<TradeHistory>();
        }
        else
        {
            return calculateTradeHistories;
        }
    }

    private void MainCalculateTradeHistories(List<TradeHistory> calculateTradeHistories)
    {
        foreach (var item in calculateTradeHistories)
        {
            var sum = BaseTradeHistoriesViewModel.BaseHajm + item.qTitTran;

            if (sum == formol.Formol.HajmFormol)
            {
                BaseTradeHistories.ForEach(b => TradeHistoriesList.Remove(b));
                TradeHistoriesList.Remove(item);
                CurrentMultiStage = 1;
                GetFilterByFormol();

                break;
            }
        }
    }

    private void PlusCurrentMultiStage()
    {
        //مرحله جاری را افزایش بده
        CurrentMultiStage = CurrentMultiStage + 1;
    }

    private void CheckMultiStageWithCurrentMultiStage()
    {
        var multiStage = formol.Formol.MultiStage;

        //برسی کن ببین به مرحله آخر رسیدی یا نه اگر رسیدی از اول ریست کن
        if (multiStage < CurrentMultiStage)
        {
            //اولین ردیف از پایه هارا حذف کن
            var first = BaseTradeHistories.FirstOrDefault();

            if (first != null)
            {
                TradeHistoriesList.Remove(first);
                MainRealBaseTradeHistories.Add(first);
            }

            CurrentMultiStage = 1;

            //............
            GetFilterByFormol();

        }
    }

    //============>>

    public void FinshedWhenEmptyCounTradeHistoriesList()
    {
        var counTradeHistoriesListt = TradeHistoriesList.Count;

        if (counTradeHistoriesListt == 0)
        {
            CurrentMultiStage = 1;
            Restart = true;
            TradeHistoriesList.Clear();
            TradeHistories = MainRealBaseTradeHistories.AsEnumerable();
            grid.RefreshDataAsync();
            SetSumHajmAndCount();
        }
        else
        {
            GetFilterByFormol();
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
        if (closingPriceDaily == null)
        {
            await DisableLoadGrid();

            SetEmptySumHajmAndCount();
            SetEmptBaseFormol();
            TradeHistories = new List<TradeHistory>();
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
