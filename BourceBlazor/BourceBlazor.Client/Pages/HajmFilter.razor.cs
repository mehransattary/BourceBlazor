using Application.ViewModel;
using Application.ViewModel.Nomad.Instrument;
using Domain.Entities;
using BlazorBootstrap;
using BlazorInputTags;
using System.Net.Http.Json;

namespace BourceBlazor.Client.Pages;

/// <summary>
/// Grid
/// </summary>
public partial class HajmFilter
{
    //=========Fields==========================//

    private Grid<HajmViewModel> gridHajm = default!;

    private bool IsLoad { get; set; } = true;

    private IEnumerable<HajmViewModel> hajms = default!;

    //==========Methods=========================//
    protected override void OnInitialized()
    {
        InputTagOptions = new InputTagOptions()
        {
            DisplayLabel = false,
            InputPlaceholder = "حجم را وارد نمائید و اینتر بزنید...",
        };
    }

    private async Task<GridDataProviderResult<HajmViewModel>> GetDataProvider(GridDataProviderRequest<HajmViewModel> request)
    {
        if (hajms is null)
        {
            hajms = await GetData();
            IsLoad = false;
        }

        StateHasChanged();
        return request.ApplyTo(hajms);
    }

    private async Task<IEnumerable<HajmViewModel>> GetData()
    {
        try
        {
            var resultApi = await httpClient.GetFromJsonAsync<IEnumerable<Hajm>>("/api/Hajms");

            var result = resultApi.GroupBy(x => x.Code).Select(x => new HajmViewModel()
            {
              
                HajmName = x.FirstOrDefault().Name,
                Hajms = x.Select(a => new Hajm()
                {                  
                    HajmValue =a.HajmValue,
                    Id =a.Id                    

                }).ToList()
            }) ;

            gridHajm.Data = result;

            StateHasChanged();

            return result;
        }
        catch (Exception)
        {
            return new List<HajmViewModel>();
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

    private AutoComplete<InstrumentSearch> InstrumentSearchAuto = default!;
    private string searchNomadName { get; set; } = string.Empty;
    private string searchNomadInsCode { get; set; } = string.Empty;

    //==========Methods=========================//
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

    private void GetEventCallbackInstrumentSearch(InstrumentSearch instrumentSearch)
    {
        searchNomadInsCode = instrumentSearch.insCode;
    }

}


