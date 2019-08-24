using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASS.Server.Services
{
    class GitCredentialService
    {
        IConfiguration config;
        ILogger logger;

        public GitCredentialService(IConfiguration globalConfig, ILogger<GitCredentialService> log)
        {
            config = globalConfig;
            logger = log;
        }

        public Credentials GetCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var uri = new Uri(url);
            var cleaned = uri.Authority + uri.PathAndQuery + uri.Fragment;
            var creds = config.GetSection("Repository:Credentials").GetSection(cleaned);
            if (!string.IsNullOrEmpty(creds["Username"]) && !string.IsNullOrEmpty(creds["Password"]))
            {
                return new UsernamePasswordCredentials() { Username = creds["Username"], Password = creds["Password"] };
            }
            logger.LogWarning("Failed to get credentials for '{cleaned}'.", cleaned);
            return null;
        }
    }
}
