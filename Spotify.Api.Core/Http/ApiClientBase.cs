using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Spotify.Api.Core.Configuration;
using Spotify.Api.Core.Extensions;
using Spotify.Api.Core.Interfaces;

namespace Spotify.Api.Core.Http
{
    public class ApiClientBase
    {
        protected readonly string BaseUrl;
        protected readonly SpotifyClientConfiguration ClientConfig;
        private static IAuthenticationClient _authClient;

        public static readonly IFlurlClientFactory ClientFactory = new PerBaseUrlFlurlClientFactory();

        public ApiClientBase(SpotifyClientConfiguration clientConfig, IAuthenticationClient authClient)
        {
            BaseUrl = clientConfig.BaseUrl;
            ClientConfig = clientConfig;
            _authClient = authClient;
        }

        protected Task<IFlurlRequest> CreateRequest(IEnumerable<KeyValuePair<string, object>> parameters = null, CancellationToken cancellation = default, params object[] urlSegments)
            => _authClient.AuthenticateAsync(cancellation)
                .ThenAsync(accessToken => ClientFactory.Get(ClientConfig.BaseUrl)
                    .Request(urlSegments)
                    .SetQueryParams(parameters)
                    .WithTimeout(TimeSpan.FromSeconds(60))
                    .WithOAuthBearerToken(accessToken));

        protected Task<IFlurlRequest> CreateJsonRequest(CancellationToken cancellation = default, params object[] urlSegments)
            => _authClient.AuthenticateAsync(cancellation)
                .ThenAsync(accessToken => ClientFactory.Get(ClientConfig.BaseUrl)
                    .Request(urlSegments)
                    .WithHeader("Content-Type", "application/json")
                    .WithTimeout(TimeSpan.FromSeconds(60))
                    .WithOAuthBearerToken(accessToken));
    }
}
