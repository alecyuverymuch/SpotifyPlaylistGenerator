using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Polly;
using Polly.Retry;

namespace Spotify.Api.Core.Extensions
{
    public static class FlurlExtensions
    {
        public static Task<T> GetAsync<T>(this IFlurlRequest request, CancellationToken cancellation)
            => request.GetJsonAsync<T>(cancellation);

        public static Task<T> PostUrlEncodedAsync<T>(this IFlurlRequest request, object data, CancellationToken cancellation)
            => request.PostUrlEncodedAsync(data, cancellation).ReceiveJson<T>();

        public static Task<T> PostJsonAsync<T>(this IFlurlRequest request, object data, CancellationToken cancellation)
            => request.PostJsonAsync(data, cancellation).ReceiveJson<T>();

        public static Task<T> ExecuteWithRetryPolicyAsync<T>(Func<Task<T>> predicate)
            => BuildRetryPolicy()
                .ExecuteAsync(predicate);

        private static AsyncRetryPolicy BuildRetryPolicy()
            => Policy.Handle<FlurlHttpTimeoutException>()
                .Or<FlurlHttpException>(httpException =>
                    httpException.Call.Response?.StatusCode == HttpStatusCode.InternalServerError || httpException.Call.Response?.StatusCode == HttpStatusCode.ServiceUnavailable || httpException.Call.Response?.StatusCode == HttpStatusCode.GatewayTimeout || httpException.Call.Response?.StatusCode == HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) - 1));
    }
}
