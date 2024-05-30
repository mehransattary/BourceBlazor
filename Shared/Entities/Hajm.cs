

using System.ComponentModel.DataAnnotations;

namespace AppShared.Entities;

public class Hajm : BourceBase
{
    [MaxLength(350)]
    public string HajmName { get; set; }

    public int Counter { get; set; }
}
