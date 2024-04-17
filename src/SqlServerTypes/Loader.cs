namespace Engage.Dnn.SqlServerTypes
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>Utility methods related to CLR Types for SQL Server.</summary>
    internal class Loader
    {
        /// <summary>Loads the required native assemblies for the current architecture (x86 or x64).</summary>
        /// <param name="rootApplicationPath">Root path of the current application.</param>
        public static void LoadNativeAssemblies(string rootApplicationPath)
        {
            var nativeBinaryPath = IntPtr.Size > 4
                ? Path.Combine(rootApplicationPath, @"SqlServerTypes\x64\")
                : Path.Combine(rootApplicationPath, @"SqlServerTypes\x86\");

            LoadNativeAssembly(nativeBinaryPath, "SqlServerSpatial160.dll");
        }

        private static void LoadNativeAssembly(string nativeBinaryPath, string assemblyName)
        {
            var path = Path.Combine(nativeBinaryPath, assemblyName);
            var ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero)
            {
                var lastWin32Error = Marshal.GetLastWin32Error();
                var exception = new Win32Exception(lastWin32Error);
                exception.Data.Add("LastWin32Error", lastWin32Error);
                throw new ApplicationException($"Can't load DLL {path}", exception);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);
    }
}