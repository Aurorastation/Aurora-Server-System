using ASS.Server.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASS.Server.Services
{

    class UpdateService
    {

        IConfiguration config;
        

        public UpdateService(IConfiguration configuration)
        {
            config = configuration;
            //Emet.FileSystems.FileSystem.ReadLink(GetLiveDirectory());
        }

        public string GetRpositoryDirectory(params string[] extraPaths) => FileSystemHelper.GetPath(config, extraPaths, "Repo");
        public string GetOverrideDirectory(params string[] extraPaths) => FileSystemHelper.GetPath(config, extraPaths, "Override");
        public string GetLiveDirectory(params string[] extraPaths) => FileSystemHelper.GetPath(config, extraPaths, "Live");
        public string GetRealLiveDirectory(params string[] extraPaths) => throw new NotImplementedException();
        public string GetStagingDirectory(params string[] extraPaths) => throw new NotImplementedException();
    }
}
