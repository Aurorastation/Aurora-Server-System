using ASS.Server.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASS.Server.Helpers
{
    public static class FileSystemHelper
    {
        public static string GetPath(IConfiguration globalConfig, params string[] paths)
        {
            if(Path.IsPathRooted(paths[0]))
                return Path.GetFullPath(Path.Combine(paths));
            return Path.GetFullPath(Path.Combine(paths.PreAppend(globalConfig["WorkDir"])));
        }

        public static string GetPath(IConfiguration globalConfig, string[] extraPaths, params string[] paths)
        {
            if (Path.IsPathRooted(paths[0]))
                return Path.GetFullPath(Path.Combine(extraPaths.PreAppend(paths)));
            return Path.GetFullPath(Path.Combine(extraPaths.PreAppend(paths).PreAppend(globalConfig["WorkDir"])));
        }
    }
}
