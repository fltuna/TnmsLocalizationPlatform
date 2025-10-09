using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TnmsLocalizationPlatform.Models;

[Table("user_language")]
public class UserLanguage
{
    [Key]
    [Column("steam_id")]
    public long SteamId { get; set; }
    
    [Required]
    [Column("language_code")]
    [MaxLength(10)]
    public string LanguageCode { get; set; } = string.Empty;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
