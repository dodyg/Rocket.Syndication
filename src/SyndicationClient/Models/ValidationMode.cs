namespace SyndicationClient.Models;

/// <summary>
/// Represents the validation mode for feed parsing.
/// </summary>
public enum ValidationMode
{
    /// <summary>
    /// Lenient mode - parse what's available, use nulls/defaults for missing fields.
    /// </summary>
    Lenient,

    /// <summary>
    /// Strict mode - reject items missing required fields per specification.
    /// </summary>
    Strict
}
