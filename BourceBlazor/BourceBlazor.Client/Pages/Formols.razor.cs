using AppShared.Entities;
using AppShared.ViewModel.Nomad.Instrument;
using AppShared.ViewModel;
using BlazorBootstrap;
using BlazorInputTags;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace BourceBlazor.Client.Pages;


/// <summary>
/// Grid
/// </summary>
public partial class Formols
{
    //=========Fields==========================//

    private Grid<FormolViewModel> gridFormol = default!;

    private bool IsLoad { get; set; } = true;

    private IEnumerable<FormolViewModel> formols = default!;

    [SupplyParameterFromForm]
    public Formol? Model { get; set; }



    //==========Methods=========================//
    protected override void OnInitialized()
    {   
        Model ??= new();
    }

    private async Task<GridDataProviderResult<FormolViewModel>> GetDataProvider(GridDataProviderRequest<FormolViewModel> request)
    {
        if (formols is null)
        {
            formols = await GetData();
            IsLoad = false;
        }

        StateHasChanged();
        return request.ApplyTo(formols);
    }

    private async Task<IEnumerable<FormolViewModel>> GetData()
    {
        try
        {
            var resultApi = await httpClient.GetFromJsonAsync<IEnumerable<Formol>>("/api/formols");

            var result = resultApi.GroupBy(x => x.Code).Select(x => new FormolViewModel()
            {

                NomadName = x.FirstOrDefault().Name,
                Formols = x.Select(a => new Formol()
                {
                    HajmFormol = a.HajmFormol,
                    TimeFormol = a.TimeFormol,
                    Id = a.Id,
                    MultiStage =a.MultiStage,
                    CalculationPrice =a.CalculationPrice

                }).ToList()
            });

            gridFormol.Data = result;

            StateHasChanged();

            return result;
        }
        catch (Exception)
        {
            return new List<FormolViewModel>();
        }
    }

    private async Task DeleteHajm(Guid id)
    {
        await httpClient.DeleteAsync($"api/formols/{id}");
        await ReloadFormol();
    }

 

}

/// <summary>
/// AutoComplete
/// </summary>
public partial class Formols
{
    //=========Fields==========================//

    private string searchNomadName { get; set; } = string.Empty;
    private string searchNomadInsCode { get; set; } = string.Empty;

    //==========Methods=========================//
    private async Task SaveFormol()
    {
        var validation = Model != null &&
            !string.IsNullOrEmpty(searchNomadName) && 
            Model.TimeFormol != 0 &&
            Model.HajmFormol != 0 &&
            Model.MultiStage!=0;

        if (validation)
        {
            var model = new Formol()
            {
                Name = searchNomadName,
                Code = searchNomadInsCode,
                HajmFormol =Model!.HajmFormol,
                TimeFormol =Model!.TimeFormol,
                MultiStage =Model!.MultiStage,
                CalculationPrice = Model!.CalculationPrice
            };

            await httpClient.PostAsJsonAsync<Formol>("/api/formols", model);
            await ReloadFormol();
        }

    }

    private async Task ReloadFormol()
    {
        formols = await GetData();
        await gridFormol.RefreshDataAsync();
        Model.TimeFormol = (int)decimal.Zero;
        Model.HajmFormol = (int)decimal.Zero;
        Model.MultiStage = (int)decimal.Zero;
        Model.CalculationPrice = false;
        StateHasChanged();
    }

    private void GetEventCallbackInstrumentSearch(InstrumentSearch instrumentSearch)
    {
        searchNomadInsCode = instrumentSearch.insCode;
        searchNomadName = instrumentSearch.lVal30 + " ( " + instrumentSearch.lVal18AFC +" )";
    }

}