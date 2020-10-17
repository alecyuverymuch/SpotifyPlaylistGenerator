using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Spotify.Api.Core.Configuration;
using Spotify.Api.Core.Extensions;
using Spotify.Api.Core.Interfaces;
using Spotify.Api.Core.Models.Responses;
using static Spotify.Api.Core.Extensions.FlurlExtensions;

namespace Spotify.Api.Core.Clients
{
    public class SpotifyCredentialClient : IAuthenticationClient
    {
        private static SpotifyAuthenticationConfiguration _configuration;
        private static IMemoryCache _cache;
        private static ILogger<SpotifyCredentialClient> _logger;
        private static readonly IFlurlClientFactory ClientFactory = new PerBaseUrlFlurlClientFactory();

        private const string AccessTokenCacheKey = "AccessTokenKey";

        public SpotifyCredentialClient(SpotifyAuthenticationConfiguration configuration, IMemoryCache cache, ILoggerFactory logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger.CreateLogger<SpotifyCredentialClient>();
        }

        public async Task<string> AuthenticateAsync(CancellationToken cancellation = default)
        {
            if (_cache.TryGetValue(AccessTokenCacheKey, out string accessToken))
                return accessToken;

            var tokenData = await GetAuthenticationToken(cancellation);
            _cache.Set(AccessTokenCacheKey, tokenData.AccessToken, TimeSpan.FromSeconds(tokenData.ExpiresIn * 0.8));
            return tokenData.AccessToken;
        }

        private async Task<SpotifyAuthenticationResponse> GetAuthenticationToken(CancellationToken cancellation = default)
        {
            var data = new
            {
                grant_type = "client_credentials",
                scope = "user-read-private user-read-email user-top-read playlist-modify-public playlist-modify-private"
            };
            _logger.LazyLog(LogLevel.Information, () => $"Requesting token from {_configuration.AuthBaseUrl}");
            return await ExecuteWithRetryPolicyAsync(() =>
                CreateAuthenticationRequest(null, "api", "token").WithBasicAuth(_configuration.ClientId, _configuration.ClientSecret).PostUrlEncodedAsync<SpotifyAuthenticationResponse>(data, cancellation));
        }

        private static IFlurlRequest CreateAuthenticationRequest(IEnumerable<KeyValuePair<string, string>> parameters = null, params object[] urlSegments)
            => ClientFactory.Get(_configuration.AuthBaseUrl)
                .Request(urlSegments)
                .SetQueryParams(parameters)
                .WithTimeout(TimeSpan.FromSeconds(60));
    }
}