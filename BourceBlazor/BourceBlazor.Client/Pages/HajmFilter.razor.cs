using AppShared.Entities;
using AppShared.ViewModel.Nomad.Instrument;
using BlazorBootstrap;
using BlazorInputTags;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages;

/// <summary>
/// Grid
/// </summary>
public partial class HajmFilter
{
    //=========Fields==========================//
    Grid<Hajm> gridHajm = default!;

    private bool IsLoad { get; set; } = true;

    private IEnumerable<Hajm> hajms = default!;

    //==========Methods=========================//
    protected override void OnInitialized()
    {
        InputTagOptions = new InputTagOptions()
        {
            DisplayLabel = false,
            InputPlaceholder = "حجم را وارد نمائید و اینتر بزنید...",
        };
    }

    private async Task<GridDataProviderResult<Hajm>> GetDataProvider(GridDataProviderRequest<Hajm> request)
    {
        if (hajms is null)
        {
            hajms = await GetData();
            IsLoad = false;
        }

        StateHasChanged();
        return request.ApplyTo(hajms);
    }

    private async Task<IEnumerable<Hajm>> GetData()
    {
        try
        {
           var query =await  httpClient.GetFromJsonAsync<IEnumerable<Hajm>>("/api/Hajms");

            var result = query.Select( (item,index) => new Hajm()
            {
                Counter = ++index ,
                Name = item.Name,
                HajmValue =item.HajmValue,
                Id = item.Id
            });

            gridHajm.Data = result;

            StateHasChanged();

            return result;
        }
        catch (Exception)
        {
            return new List<Hajm>();
        }
    }

    private async Task DeleteHajm(Guid id)
    {
        await httpClient.DeleteAsync($"api/Hajms/{id}");
        await ReloadHajm();
    }
  
}

/// <summary>
/// AutoComplete
/// </summary>
public partial class HajmFilter
{ 
    //=========Fields==========================//
    private List<string> Tags { get; set; } = new();

    private InputTagOptions InputTagOptions { get; set; } = new();

    private IEnumerable<InstrumentSearch> instrumentSearches = default!;

    private AutoComplete<InstrumentSearch> InstrumentSearchAuto = default!;

    private string searchNomadName { get; set; }

    private string searchNomadInsCode { get; set; }


    //==========Methods=========================//
    private async Task<AutoCompleteDataProviderResult<InstrumentSearch>> GetNomadProvider(AutoCompleteDataProviderRequest<InstrumentSearch> request)
    {
        var value = InstrumentSearchAuto.Value;
        instrumentSearches = await GetNomadData(value);
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

    private async Task SaveHajm()
    {
        var hajmModels = new List<Hajm>();

        searchNomadName = InstrumentSearchAuto.Value;

        if (!string.IsNullOrEmpty(searchNomadName) && Tags.Any())
        {
            foreach (var tag in Tags)
            {
                var hajmModel = new Hajm()
                {                
                    Name = searchNomadName,
                    Code = searchNomadInsCode,
                    HajmValue =Convert.ToInt32(tag)
                };

                hajmModels.Add(hajmModel);
            }

            foreach (var _hajm in hajmModels)
            {
                await httpClient.PostAsJsonAsync<Hajm>("/api/Hajms", _hajm);
            }

           await ReloadHajm();
        }

    }

    private async Task ReloadHajm()
    {
        hajms = await GetData();
        await gridHajm.RefreshDataAsync();
        searchNomadName = string.Empty;
        Tags.Clear();
        StateHasChanged();
    }

    private void OnAutoCompleteChanged(InstrumentSearch instrumentSearch)
    {
        searchNomadInsCode = instrumentSearch.insCode;
    }

}
