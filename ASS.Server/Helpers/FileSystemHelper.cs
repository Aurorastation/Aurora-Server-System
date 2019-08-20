using ASS.Server.Extensions;
using Emet.FileSystems;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

        public static void CreateRelativeSymbolicLink(string target, string link, FileType targetHint = FileType.LinkTargetHintNotAvailable)
        {
            if (targetHint == FileType.File || targetHint == FileType.Directory)
                target = Path.GetRelativePath(Path.Combine(link, ".."), target);
            FileSystem.CreateSymbolicLink(target, link, targetHint);
        }
    }
}
