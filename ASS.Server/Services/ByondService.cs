using ASS.API;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ASS.Server.Services
{
    class ByondService : Byond.ByondBase
    {
        const string BYOND_LATEST_URL = "https://secure.byond.com/download/build/LATEST/";
        const string BYOND_DOWNLOAD_URL = "https://secure.byond.com/download/build/";

        HttpClient httpclient;
        IConfiguration config;

        public ByondService(HttpClient _httpClient, IConfiguration configuration)
        {
            httpclient = _httpClient;
            config = configuration.GetSection("BYOND");
        }

        public static string GetDownloadUrl(string version)
        {
            var split = version.Split(".");
            return GetDownloadUrl(split[0], split[1]);
        }
        public static string GetDownloadUrl(int major, int minor) => GetDownloadUrl(major.ToString(), minor.ToString());
        public static string GetDownloadUrl(ByondVersion version) => GetDownloadUrl(version.Major, version.Minor);
        public static string GetDownloadUrl(string major, string minor)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return $"{BYOND_DOWNLOAD_URL}/{major}/{major}.{minor}_byond_linux.zip";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $"{BYOND_DOWNLOAD_URL}/{major}/{major}.{minor}_byond.zip";
            throw new Exception("Unsupported OS");
        }

        public async Task<IEnumerable<ByondVersion>> GetVersions()
        {
            var response = await httpclient.SendAsync(new HttpRequestMessage(HttpMethod.Get, BYOND_LATEST_URL));
            var content = await response.Content.ReadAsStringAsync();
            var regex = new Regex("\\\"([\\d]+)\\.([\\d]+)_byond.zip\\\"");
            var matches = regex.Matches(content);
            return matches.Select(m => new ByondVersion() { Major = int.Parse(m.Captures[0].Value), Minor = int.Parse(m.Captures[0].Value) });
        }
    }
}
