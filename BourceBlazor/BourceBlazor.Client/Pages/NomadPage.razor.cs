using AppShared.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages;

public partial class NomadPage
{
    #region <----------> Fields
    [Parameter]
    public string NomadName { get; set; }

    Grid<InstrumentSearch> grid = default!;

    public string Search { get; set; } = string.Empty;

    public bool IsLoad { get; set; } = true;

    private IEnumerable<InstrumentSearch> instrumentSearches = default!;

    private HashSet<InstrumentSearch> selectedEmployees = new();

    #endregion

    #region <----------> Methods
    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(NomadName))
        {
            Search = NomadName;
        }
    }

    /// <summary>
    /// لود اولیه برای گرید
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<GridDataProviderResult<InstrumentSearch>> GetDataProvider(GridDataProviderRequest<InstrumentSearch> request)
    { 

        if (instrumentSearches is null)
        {
            instrumentSearches = await GetData(Search);
            IsLoad = !IsLoad;
        }

        StateHasChanged();

        return request.ApplyTo(instrumentSearches);
    }

    /// <summary>
    /// جستجو کن وبعد گرید لود کن
    /// </summary>
    /// <returns></returns>
    private async Task GetSearch()
    {
        instrumentSearches = await GetData(Search);

        if (instrumentSearches != null && instrumentSearches.Any())
        {
            grid.Data = instrumentSearches!;
            await grid.RefreshDataAsync();
            StateHasChanged();
        }
    }

    /// <summary>
    /// دیتا را از وب سرویس بگیر
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    private async Task<IEnumerable<InstrumentSearch>> GetData(string search)
    {
        if (NomadName != null)
        {
            search = NomadName;
        }

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

    /// <summary>
    /// انتخاب شونده ها توسط چک باکس ها
    /// </summary>
    /// <param name="employees"></param>
    /// <returns></returns>
    private Task OnSelectedItemsChanged(HashSet<InstrumentSearch> employees)
    {
        selectedEmployees = employees is not null && employees.Any() ? employees : new();
        return Task.CompletedTask;
    }

    /// <summary>
    /// رفتن به صفحه تاریخ ها
    /// </summary>
    /// <param name="url"></param>
    private void GoNomadDate(string url)
    {
        NavigationManager.NavigateTo(url);
    }

    #endregion
}
