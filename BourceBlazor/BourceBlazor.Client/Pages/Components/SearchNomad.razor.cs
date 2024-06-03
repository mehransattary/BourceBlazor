using AppShared.Helper;
using AppShared.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages.Components;

public partial class SearchNomad
{
    //==========Parameter========================//

    [Parameter]
    public EventCallback<InstrumentSearch> EventCallbackInstrumentSearch { get; set; }

    //==========Fileds========================//

    private AutoComplete<InstrumentSearch> RefAutoComplete = default!;


    private string NomadName { get; set; } = string.Empty;

    //==========Methods========================//


    private async Task<AutoCompleteDataProviderResult<InstrumentSearch>> GetNomadProvider(AutoCompleteDataProviderRequest<InstrumentSearch> request)
    {
        var value = request.Filter.Value.FixPersianChars();
        RefAutoComplete.Value = value;
        var result = await GetNomadData(value);

        return request.ApplyTo(result);
    }

    private async Task<IEnumerable<InstrumentSearch>> GetNomadData(string search)
    {
        search = string.IsNullOrEmpty(search) ? "خودرو" : search;

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
                return response.instrumentSearch.Select((item, index) => new InstrumentSearch
                {
                    Counter = index + 1,
                    lVal30 = item.lVal18AFC + " - " + item.lVal30,
                    lVal18AFC = item.lVal18AFC,
                    insCode = item.insCode,

                }).ToList();
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
        await EventCallbackInstrumentSearch.InvokeAsync(instrumentSearch);
    }
}
