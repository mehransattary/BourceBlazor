using System.Text.Json.Serialization;

namespace AppShared.Entities;

public class Nomad : BourceBase
{
    public virtual ICollection<NomadDate> NomadDates { get; set; } = null;
}

