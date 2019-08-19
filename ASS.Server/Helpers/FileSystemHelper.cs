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
            CreateSymbolicLink(target, link, targetHint);
        }

        // TODO: Remove when https://github.com/joshudson/Emet/pull/3 is merged
        ///<summary>Creates a symbolic link to a file</summary>
		///<param name="targetpath">The path to write to the symbolic link</param>
		///<param name="linkpath">The path to create the new link at</param>
		///<param name="targethint">The type of node the link is referring to</param>
		///<exception cref="System.IO.IOException">An IO error occurred</exception>
		///<exception cref="System.PlatformNotSupportedException">linkpath doesn't exist and targethint was neither File nor Directory and this platform uses explicit link types</exception>
		///<remarks>If targetpath doesn't exist and targethint is not provided, this call will fail on Windows.</remarks>
		public static void CreateSymbolicLink(string targetpath, string linkpath, FileType targethint = FileType.LinkTargetHintNotAvailable)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var btargetpath = NameToByteArray(targetpath);
                var blinkpath = NameToByteArray(linkpath);
                if (EmetNativeMethods.symlink(btargetpath, blinkpath) != 0)
                {
                    var errno = Marshal.GetLastWin32Error();
                    var ci = new System.ComponentModel.Win32Exception();
                    throw new IOException(ci.Message, errno);
                }
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                uint flags = 0;
                if (targethint != FileType.File && targethint != FileType.Directory)
                {
                    if (!Path.IsPathRooted(targetpath))
                    {
                        var ldname = Path.GetDirectoryName(linkpath);
                        if (string.IsNullOrEmpty(ldname)) ldname = DirectoryEntry.CurrentDirectoryName;
                        targetpath = Path.Combine(ldname, targetpath);
                    }
                    var node = new DirectoryEntry(targetpath, FileSystem.FollowSymbolicLinks.Never);
                    var hint = (node.FileType == FileType.SymbolicLink) ? node.LinkTargetHint : node.FileType;
                    if (hint == FileType.Directory)
                        flags = EmetNativeMethods.SYMBOLIC_LINK_FLAG_DIRECTORY;
                    else if (hint != FileType.File)
                        throw new PlatformNotSupportedException("Windows can't handle symbolic links to file system nodes that don't exist.");
                }
                if (targethint == FileType.Directory)
                    flags = EmetNativeMethods.SYMBOLIC_LINK_FLAG_DIRECTORY;
                if (0 == EmetNativeMethods.CreateSymbolicLinkW(linkpath, targetpath, flags))
                {
                    var errno = (int)Marshal.GetLastWin32Error();
                    if (errno == 1314)
                    {
                        flags |= EmetNativeMethods.SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;
                        if (0 != EmetNativeMethods.CreateSymbolicLinkW(linkpath, targetpath, flags))
                            return;
                        var errno2 = (int)Marshal.GetLastWin32Error();
                        if (errno2 != 1314 && errno2 != 1 && errno2 != 0xA0)
                            errno = errno2; // Try to get a better error
                    }
                    var ci = new System.ComponentModel.Win32Exception();
                    throw new IOException(ci.Message, unchecked((int)0x80070000 | errno));
                }
            }
        }

        internal static byte[] NameToByteArray(string name)
        {
            var count = Encoding.UTF8.GetByteCount(name);
            var bytes = new byte[count + 1];
            Encoding.UTF8.GetBytes(name, 0, name.Length, bytes, 0);
            return bytes;
        }
    }
}
