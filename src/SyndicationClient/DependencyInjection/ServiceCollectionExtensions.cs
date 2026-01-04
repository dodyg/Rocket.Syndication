using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using SyndicationClient.Caching;
using SyndicationClient.Discovery;
using SyndicationClient.Http;
using SyndicationClient.Models;
using SyndicationClient.Parsing;

namespace SyndicationClient.DependencyInjection;

/// <summary>
/// Extension methods for registering syndication client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds syndication client services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSyndicationClient(
        this IServiceCollection services,
        Action<SyndicationClientOptions>? configure = null)
    {
        // Configure options
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.TryAddSingleton(Options.Create(new SyndicationClientOptions()));
        }

        // Register cache
        services.TryAddSingleton<IFeedCache, InMemoryFeedCache>();

        // Register parsers
        services.TryAddSingleton<IFeedParser, RssFeedParser>();
        services.TryAddSingleton<IFeedParser, AtomFeedParser>();
        services.TryAddSingleton<FeedParserPipeline>();

        // Register HTTP client with Polly policies
        services.AddHttpClient<FeedHttpClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SyndicationClientOptions>>().Value;
            client.Timeout = options.Timeout;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression = System.Net.DecompressionMethods.All
        })
        .AddPolicyHandler((sp, _) =>
        {
            var options = sp.GetRequiredService<IOptions<SyndicationClientOptions>>().Value;
            return CreateRetryPolicy(options.RetryPolicy);
        })
        .AddPolicyHandler((sp, _) =>
        {
            var options = sp.GetRequiredService<IOptions<SyndicationClientOptions>>().Value;
            return CreateCircuitBreakerPolicy(options.RetryPolicy);
        });

        // Register discovery service HTTP client
        services.AddHttpClient<FeedDiscoveryService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SyndicationClientOptions>>().Value;
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        });

        // Register main client
        services.TryAddScoped<ISyndicationClient, SyndicationFeedClient>();

        return services;
    }

    /// <summary>
    /// Adds syndication client services with a custom cache implementation.
    /// </summary>
    /// <typeparam name="TCache">The cache implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSyndicationClient<TCache>(
        this IServiceCollection services,
        Action<SyndicationClientOptions>? configure = null)
        where TCache : class, IFeedCache
    {
        services.AddSingleton<IFeedCache, TCache>();
        return services.AddSyndicationClient(configure);
    }

    /// <summary>
    /// Adds a custom feed parser to the pipeline.
    /// </summary>
    /// <typeparam name="TParser">The parser type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFeedParser<TParser>(this IServiceCollection services)
        where TParser : class, IFeedParser
    {
        services.AddSingleton<IFeedParser, TParser>();
        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(RetryPolicyOptions options)
    {
        if (options.MaxRetries <= 0)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }

        var delays = options.BackoffType switch
        {
            BackoffType.Constant => GenerateConstantBackoff(options.InitialDelay, options.MaxRetries),
            BackoffType.Linear => GenerateLinearBackoff(options.InitialDelay, options.MaxRetries),
            BackoffType.Exponential => GenerateExponentialBackoff(options.InitialDelay, options.MaxRetries),
            _ => GenerateExponentialBackoff(options.InitialDelay, options.MaxRetries)
        };

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delays);
    }

    private static IEnumerable<TimeSpan> GenerateConstantBackoff(TimeSpan delay, int retryCount)
    {
        for (int i = 0; i < retryCount; i++)
        {
            yield return delay;
        }
    }

    private static IEnumerable<TimeSpan> GenerateLinearBackoff(TimeSpan initialDelay, int retryCount)
    {
        for (int i = 0; i < retryCount; i++)
        {
            yield return TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * (i + 1));
        }
    }

    private static IEnumerable<TimeSpan> GenerateExponentialBackoff(TimeSpan initialDelay, int retryCount)
    {
        for (int i = 0; i < retryCount; i++)
        {
            yield return TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, i));
        }
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(RetryPolicyOptions options)
    {
        if (!options.EnableCircuitBreaker)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                options.CircuitBreakerThreshold,
                options.CircuitBreakerDuration);
    }
}
