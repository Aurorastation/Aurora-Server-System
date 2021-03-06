﻿using ASS.API;
using ASS.Server.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using System.IO;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace ASS.Server.Services
{
    public class ByondService : Byond.ByondBase
    {
        const string BYOND_LATEST_URL = "https://secure.byond.com/download/build/LATEST/";
        const string BYOND_DOWNLOAD_URL = "https://secure.byond.com/download/build/";

        HttpClient httpclient;
        IConfiguration config;
        IConfiguration globalConfig;
        ILogger logger;

        public ByondService(HttpClient _httpClient, IConfiguration configuration, ILogger<ByondService> log)
        {
            httpclient = _httpClient;
            globalConfig = configuration;
            config = configuration.GetSection("BYOND");
            logger = log;
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


        private static string getByondVersionDirectoryName(ByondVersion version) => $"{version.Major}.{version.Minor}";
        public string GetByondDirectoryPath(ByondVersion version, params string[] extraPaths) => FileSystemHelper.GetPath(globalConfig, extraPaths, config["Dir"], getByondVersionDirectoryName(version));
        public string GetByondDirectoryPath(params string[] extraPaths) => FileSystemHelper.GetPath(globalConfig, extraPaths, config["Dir"], "live");

        public async Task<IEnumerable<ByondVersion>> GetVersions()
        {
            var response = await httpclient.SendAsync(new HttpRequestMessage(HttpMethod.Get, BYOND_LATEST_URL));
            var content = await response.Content.ReadAsStringAsync();
            response.Dispose();
            var regex = new Regex("\\\"([\\d]+)\\.([\\d]+)_byond.zip\\\"");
            var matches = regex.Matches(content);
            return matches.Select(m => new ByondVersion() { Major = int.Parse(m.Captures[0].Value), Minor = int.Parse(m.Captures[0].Value) });
        }

        public async Task DownloadByond(ByondVersion version)
        {
            using (var response = await httpclient.SendAsync(new HttpRequestMessage(HttpMethod.Get, GetDownloadUrl(version))))
                using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                        archive.ExtractToDirectory(GetByondDirectoryPath(version), true);
            using (var versionData = File.Create(GetByondDirectoryPath(version, "version.dat")))
                version.WriteTo(versionData);
        }

        private ByondVersion getVersion() => getVersion(GetByondDirectoryPath());
        private ByondVersion getVersion(string path)
        {
            var filePath = Path.Combine(path, "version.dat");
            if (!File.Exists(filePath))
                return null;
            ByondVersion version;
            using (var versionData = File.OpenRead(filePath))
                version = ByondVersion.Parser.ParseFrom(versionData);
            return version;
        }

        public async Task SwitchToVersion(ByondVersion version)
        {
            if (version.Equals(getVersion()))
            {
                logger.LogInformation($"Server is on {version.Major}.{version.Minor} version. No switch is needed.");
                return;
            }
            if (!Directory.Exists(GetByondDirectoryPath(version)))
            {
                logger.LogInformation($"Version {version.Major}.{version.Minor} is not installed. Downloading and installing...");
                await DownloadByond(version);
            }
            if (!getVersion(GetByondDirectoryPath(version)).Equals(version))
                throw new Exception($"Byond version '{version.Major}.{version.Minor}' data mismatches folder name or ByondVersion data. Please manually remove this version.");
            if (Directory.Exists(GetByondDirectoryPath()))
                Directory.Delete(GetByondDirectoryPath());
            FileSystemHelper.CreateRelativeSymbolicLink(GetByondDirectoryPath(version), GetByondDirectoryPath(), Emet.FileSystems.FileType.Directory);
            if (!version.Equals(getVersion()))
                throw new Exception($"Byond version switch failed.");
        }
    }
}
