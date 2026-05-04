using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Auth.Shared.Extensions.Cors;

[OptionsValidator]
public sealed partial class CorsSetting : IValidateOptions<CorsSetting>
{
    public const string ConfigurationSection = "Cors";

    [Required] public IList<string> Origins { get; } = [];

    [Required] public IList<string> Headers { get; } = [];

    [Required] public IList<string> Methods { get; } = [];

    [Range(0, int.MaxValue)] public int? MaxAge { get; set; }

    public bool AllowCredentials { get; set; }
}