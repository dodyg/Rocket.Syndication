namespace SyndicationClient.Models;

/// <summary>
/// Represents the type of authentication to use for feed requests.
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    /// No authentication.
    /// </summary>
    None,

    /// <summary>
    /// HTTP Basic authentication.
    /// </summary>
    Basic,

    /// <summary>
    /// Bearer token authentication.
    /// </summary>
    Bearer,

    /// <summary>
    /// Custom headers (e.g., API keys).
    /// </summary>
    CustomHeaders,

    /// <summary>
    /// Cookie-based authentication.
    /// </summary>
    Cookie
}
