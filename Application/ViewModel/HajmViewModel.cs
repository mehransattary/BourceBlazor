
using Domain.Entities;

namespace Application.ViewModel;

public class HajmViewModel
{
    public int Counter { get; set; }
    public string HajmName { get; set; }

    public List<Hajm> Hajms { get; set; } = new();
}
