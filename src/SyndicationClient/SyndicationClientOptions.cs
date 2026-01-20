using SyndicationClient.Authentication;
using SyndicationClient.Models;

namespace SyndicationClient;

/// <summary>
/// Configuration options for the syndication client.
/// </summary>
public class SyndicationClientOptions
{
    /// <summary>
    /// Gets or sets the default request timeout. Default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum number of redirects to follow. Default is 5.
    /// </summary>
    public int MaxRedirects { get; set; } = 5;

    /// <summary>
    /// Gets or sets the User-Agent header value. Default is "SyndicationClient/1.0".
    /// </summary>
    public string UserAgent { get; set; } = "SyndicationClient/1.0";

    /// <summary>
    /// Gets or sets whether HTTP caching is enabled. Default is true.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the validation mode for feed parsing. Default is Lenient.
    /// </summary>
    public ValidationMode ValidationMode { get; set; } = ValidationMode.Lenient;

    /// <summary>
    /// Gets or sets the retry policy options.
    /// </summary>
    public RetryPolicyOptions RetryPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets the default credentials for authenticated feeds.
    /// </summary>
    public FeedCredentials? DefaultCredentials { get; set; }
}

/// <summary>
/// Configuration options for retry policies.
/// </summary>
public class RetryPolicyOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retries. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retries. Default is 1 second.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the backoff type. Default is Exponential.
    /// </summary>
    public BackoffType BackoffType { get; set; } = BackoffType.Exponential;

    /// <summary>
    /// Gets or sets whether the circuit breaker is enabled. Default is true.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of failures before the circuit opens. Default is 5.
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets how long the circuit stays open. Default is 30 seconds.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}
