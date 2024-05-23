using System.Text.Json.Serialization;

namespace Domain.Entittes.Bource;

public class NomadDate : BourceBase
{
    /// <summary>
    /// تاریخ
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// نماد
    /// </summary>
    public int NomadId { get; set; }

    public Nomad? Nomad { get; set; }

    [JsonIgnore]
    public virtual ICollection<NomadAction> NomadActions { get; set; } = null;
}

