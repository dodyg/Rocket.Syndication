namespace SyndicationClient.Models;

/// <summary>
/// Represents the type of backoff strategy for retries.
/// </summary>
public enum BackoffType
{
    /// <summary>
    /// Linear backoff - delay increases linearly.
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential backoff - delay doubles with each retry.
    /// </summary>
    Exponential,

    /// <summary>
    /// Constant backoff - delay remains the same.
    /// </summary>
    Constant
}
