
namespace Domain.Entities;

public class Nomad : BourceBase
{
    public virtual ICollection<NomadDate> NomadDates { get; set; } = null;
}

