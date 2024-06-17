using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class BourceBase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; }

    public string Name { get; set; }
}


