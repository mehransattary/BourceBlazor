
using System.ComponentModel.DataAnnotations;

namespace Domain.Entittes.Bource;

public class BourceBase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; }

    public string Name { get; set; }
}


