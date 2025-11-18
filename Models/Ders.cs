using System.ComponentModel.DataAnnotations.Schema;

public class Ders
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Ad { get; set; } = null!;

    // Removed Kodu (or alternatively: [NotMapped] public string? Kodu { get; set; })

    // ... rest unchanged
}   