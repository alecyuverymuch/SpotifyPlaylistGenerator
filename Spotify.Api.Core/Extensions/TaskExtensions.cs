using System;
using System.Threading.Tasks;

namespace Spotify.Api.Core.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<TResult> ThenAsync<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> predicate)
            => predicate(await source);

        public static async Task<TResult> WhenAsync<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> predicate)
        {
            var func = predicate;
            var source1 = await source;
            var result = await func(source1);
            return result;
        }
    }
}
