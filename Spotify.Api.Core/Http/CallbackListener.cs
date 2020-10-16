using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Win32;
using Spotify.Api.Core.Extensions;
using Spotify.Api.Core.Models.Responses;

namespace Spotify.Api.Core.Http
{
    public class CallbackListener : IDisposable
    {
        private readonly HttpListener _listener;

        public CallbackListener()
        {
            _listener = new HttpListener();
        }

        public async Task<AuthenticationCallbackResponse> GetCallbackAsync(string authenticationUrl, string callbackUrl)
        {
            var startInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new ProcessStartInfo("xdg-open", authenticationUrl)
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? new ProcessStartInfo("open", authenticationUrl)
                    : new ProcessStartInfo(GetWindowsBrowser(), authenticationUrl);
            var process = new Process { StartInfo = startInfo };
            process.Start();

            _listener.Prefixes.Add(callbackUrl.ResetToRoot() + "/");
            _listener.Start();
            var response = await _listener.GetContextAsync().ThenAsync(c => c.Request.QueryString);
            process.Kill(true);

            return new AuthenticationCallbackResponse
            {
                Code = response["code"],
                Error = response["error"],
                State = response["state"]
            };
        }

        private static string GetWindowsBrowser()
        {
            var browser = string.Empty;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command");
                if (key != null)
                    browser = key.GetValue(null).ToString().ToLower().Trim(new[] { '"' });
                if (!browser.EndsWith("exe"))
                    browser = browser.Substring(0, browser.LastIndexOf(".exe", StringComparison.InvariantCultureIgnoreCase) + 4);
            }
            finally
            {
                key?.Close();
            }
            return browser;
        }

        public void Dispose()
        {
            _listener.Close();
        }
    }
}
