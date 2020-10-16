using System;
using Microsoft.Extensions.Logging;

namespace Spotify.Api.Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void LazyLog<T>(this ILogger<T> logger, LogLevel logLevel, Func<string> getString)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.Log(logLevel, getString());
        }
    }
}
