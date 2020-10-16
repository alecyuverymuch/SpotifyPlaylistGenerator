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
using Spotify.Api.Core.Http;
using Spotify.Api.Core.Interfaces;
using Spotify.Api.Core.Models.Responses;
using static Spotify.Api.Core.Extensions.FlurlExtensions;

namespace Spotify.Api.Core.Clients
{
    public class SpotifyAuthenticationClient : IAuthenticationClient
    {
        private static SpotifyAuthenticationConfiguration _configuration;
        private static IMemoryCache _cache;
        private static ILogger<SpotifyAuthenticationClient> _logger;
        private static readonly IFlurlClientFactory ClientFactory = new PerBaseUrlFlurlClientFactory();
        private static readonly string State = Guid.NewGuid().ToString();

        private const string AccessTokenCacheKey = "AccessTokenKey";
        private const string RefreshTokenCacheKey = "RefreshTokenKey";

        public SpotifyAuthenticationClient(SpotifyAuthenticationConfiguration configuration, IMemoryCache cache, ILoggerFactory logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger.CreateLogger<SpotifyAuthenticationClient>();
        }

        public async Task<string> AuthenticateAsync(CancellationToken cancellation = default)
        {
            if (_cache.TryGetValue(AccessTokenCacheKey, out string accessToken))
                return accessToken;

            if (_cache.TryGetValue(RefreshTokenCacheKey, out string refreshToken))
            {
                var tokenData = await RefreshTokenAsync(refreshToken, cancellation);
                _cache.Set(AccessTokenCacheKey, tokenData.AccessToken, TimeSpan.FromSeconds(tokenData.ExpiresIn * 0.8));
                return tokenData.AccessToken;
            }

            return await GetAuthentication(cancellation);
        }

        private static IFlurlRequest CreateAuthenticationRequest(IEnumerable<KeyValuePair<string, string>> parameters = null, params object[] urlSegments)
            => ClientFactory.Get(_configuration.AuthBaseUrl)
                .Request(urlSegments)
                .SetQueryParams(parameters)
                .WithTimeout(TimeSpan.FromSeconds(60));

        private async Task<string> GetAuthentication(CancellationToken cancellation = default)
        {
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _configuration.ClientId },
                { "redirect_uri", _configuration.RedirectUri },
                { "scope", "user-read-private user-read-email user-top-read playlist-modify-public playlist-modify-private" },
                { "state", State },
                { "response_type", "code" },
                { "show_dialog", "false" }
            };
            using var listener = new CallbackListener();
            var response = await listener.GetCallbackAsync(CreateAuthenticationRequest(parameters, "authorize").Url, _configuration.RedirectUri);

            if (!string.IsNullOrEmpty(response.Error))
            {
                _logger.LazyLog(LogLevel.Warning, () => $"Authentication Error: ${response.Error}");
                return null;
            }
            if (response.State != State)
            {
                _logger.LazyLog(LogLevel.Warning, () => $"Authentication Error: State received {response.State} does not match");
                return null;
            }
            if (string.IsNullOrEmpty(response.Code))
            {
                _logger.LazyLog(LogLevel.Warning, () => "Authentication Error: No authentication code received");
                return null;
            }

            var tokenData = await GetCallbackAuthenticationToken(response.Code, cancellation);
            _cache.Set(AccessTokenCacheKey, tokenData.AccessToken, TimeSpan.FromSeconds(tokenData.ExpiresIn * 0.8));
            _cache.Set(RefreshTokenCacheKey, tokenData.RefreshToken, TimeSpan.FromHours(1));
            return tokenData.AccessToken;
        }

        private async Task<SpotifyAuthenticationResponse> GetCallbackAuthenticationToken(string responseCode, CancellationToken cancellation = default)
        {
            var data = new
            {
                code = responseCode,
                redirect_uri = _configuration.RedirectUri,
                client_id = _configuration.ClientId,
                client_secret = _configuration.ClientSecret,
                grant_type = "authorization_code"
            };
            _logger.LazyLog(LogLevel.Information, () => $"Requesting callback authentication from {_configuration.AuthBaseUrl}");
            return await ExecuteWithRetryPolicyAsync(() => CreateAuthenticationRequest(null, "api", "token").PostUrlEncodedAsync<SpotifyAuthenticationResponse>(data, cancellation));
        }

        private async Task<SpotifyAuthenticationResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellation = default)
        {
            var data = new
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken
            };
            _logger.LazyLog(LogLevel.Information, () => $"Requesting refresh token from {_configuration.AuthBaseUrl}");
            return await ExecuteWithRetryPolicyAsync(() =>
                CreateAuthenticationRequest(null, "api", "token").WithBasicAuth(_configuration.ClientId, _configuration.ClientSecret).PostUrlEncodedAsync<SpotifyAuthenticationResponse>(data, cancellation));
        }
    }
}
