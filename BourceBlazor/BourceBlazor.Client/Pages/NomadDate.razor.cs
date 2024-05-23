using AppShared.ViewModel.Nomad.ClosingPriceDaily;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages;

public partial class NomadDate
{
    #region <----------> Fields

    [Parameter]
    public string InsCode { get; set; }

    [Parameter]
    public string NomadName { get; set; } 

    Grid<ClosingPriceDaily> grid = default!;

    public bool IsLoad { get; set; } = true;

    private IEnumerable<ClosingPriceDaily> closingPriceDailies = default!;

    private HashSet<ClosingPriceDaily> selectedEmployees = new();

    #endregion

    #region <----------> Methods

    /// <summary>
    /// لود اولیه برای گرید
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<GridDataProviderResult<ClosingPriceDaily>> GetDataProvider(GridDataProviderRequest<ClosingPriceDaily> request)
    {
        if (closingPriceDailies is null)
        {
            closingPriceDailies = await GetData();
            IsLoad = !IsLoad;
        }
        StateHasChanged();
        return request.ApplyTo(closingPriceDailies);
        
    }

    /// <summary>
    /// دیتا را از وب سرویس بگیر
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    private async Task<IEnumerable<ClosingPriceDaily>> GetData()
    { 
        var urlDate = configuration["Urls:UrlDate"];
        try
        {
            var response = await httpClient.GetFromJsonAsync<RootClosingPriceDaily>(urlDate + InsCode + "/0");

            if (response != null && response.closingPriceDaily.Any())
            {

                closingPriceDailies = response.closingPriceDaily.Select((item, index) => new ClosingPriceDaily
                {
                    Counter = ++index,
                    insCode = item.insCode,
                    dEven = item.dEven,
                    zTotTran = item.zTotTran,
                    qTotTran5J = item.qTotTran5J,
                    qTotCap = item.qTotCap
                });

                return closingPriceDailies;
            }

            return new List<ClosingPriceDaily>();
        }
        catch (Exception)
        {
            return new List<ClosingPriceDaily>();
        }

    }

    /// <summary>
    /// انتخاب شونده ها توسط چک باکس ها
    /// </summary>
    /// <param name="employees"></param>
    /// <returns></returns>
    private Task OnSelectedItemsChanged(HashSet<ClosingPriceDaily> employees)
    {
        selectedEmployees = employees is not null && employees.Any() ? employees : new();
        return Task.CompletedTask;
    }

    /// <summary>
    /// رفتن به صفحه تاریخ ها
    /// </summary>
    /// <param name="url"></param>
    private void GoNomadAction(string url)
    {
        NavigationManager.NavigateTo(url);
    }

    #endregion
}
