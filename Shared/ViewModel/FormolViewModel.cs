using AppShared.Entities;

namespace AppShared.ViewModel;

public class FormolViewModel
{
    public int Counter { get; set; }

    public string NomadName { get; set; }

    public int MultiStage { get; set; }

    public bool CalculationPrice { get; set; }

    public List<Formol> Formols { get; set; } = new();
}
