﻿using AppShared.Helper;
using AppShared.ViewModel.Nomad.Actions;
using AppShared.ViewModel.Nomad.ClosingPriceDaily;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages.Components;

public partial class SearchDate
{

    //==========Parameter========================//

    [Parameter]
    public string InsCode { get; set; }

    [Parameter]
    public EventCallback<ClosingPriceDaily> EventCallbackOnChangeDate { get; set; }

    //==========Fileds========================//

    private IEnumerable<ClosingPriceDaily> closingPriceDailies = default!;

    private AutoComplete<ClosingPriceDaily> RefAutoComplete = default!;

    private string? searchNomadDate;

    //==========Methods========================//
    protected override void OnInitialized()
    {
        searchNomadDate = "140";
    }
    protected override void OnAfterRender(bool firstRender)
    {
        if ((string.IsNullOrEmpty(InsCode)))
        {
            searchNomadDate = "140";
        }
    }
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
            if (!string.IsNullOrEmpty(InsCode) && InsCode != "0")
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
        await EventCallbackOnChangeDate.InvokeAsync(closingPriceDaily);
    }
}
