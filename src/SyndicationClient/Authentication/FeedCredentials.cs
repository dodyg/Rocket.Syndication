using SyndicationClient.Models;

namespace SyndicationClient.Authentication;

/// <summary>
/// Represents credentials for authenticated feed requests.
/// </summary>
public sealed record FeedCredentials
{
    /// <summary>
    /// Gets the authentication type.
    /// </summary>
    public AuthenticationType Type { get; init; }

    /// <summary>
    /// Gets the username for Basic authentication.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets the password for Basic authentication.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Gets the bearer token for Bearer authentication.
    /// </summary>
    public string? BearerToken { get; init; }

    /// <summary>
    /// Gets custom headers to include in requests.
    /// </summary>
    public IReadOnlyDictionary<string, string>? CustomHeaders { get; init; }

    /// <summary>
    /// Gets cookies to include in requests.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Cookies { get; init; }

    /// <summary>
    /// Creates credentials for Basic authentication.
    /// </summary>
    public static FeedCredentials Basic(string username, string password) =>
        new() { Type = AuthenticationType.Basic, Username = username, Password = password };

    /// <summary>
    /// Creates credentials for Bearer token authentication.
    /// </summary>
    public static FeedCredentials Bearer(string token) =>
        new() { Type = AuthenticationType.Bearer, BearerToken = token };

    /// <summary>
    /// Creates credentials with custom headers.
    /// </summary>
    public static FeedCredentials WithHeaders(IReadOnlyDictionary<string, string> headers) =>
        new() { Type = AuthenticationType.CustomHeaders, CustomHeaders = headers };

    /// <summary>
    /// Creates credentials with cookies.
    /// </summary>
    public static FeedCredentials WithCookies(IReadOnlyDictionary<string, string> cookies) =>
        new() { Type = AuthenticationType.Cookie, Cookies = cookies };
}
