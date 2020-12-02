namespace Engage.Dnn.SqlServerTypes.Build.Tasks
{
    using System.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.IO.Paths;
    using Cake.Common.Solution.Project.Properties;
    using Cake.Common.Tools.MSBuild;
    using Cake.Common.Tools.NuGet;
    using Cake.Common.Tools.NuGet.Restore;
    using Cake.Common.Xml;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;
    using Cake.Frosting;

    public sealed class Package : FrostingTask<Context>
    {
        public override void Run(Context context)
        {
            var version = ReadVersion(context);
            SetAssemblyInfoVersion(context, version);

            var sln = context.File("Engage.Dnn.SqlServerTypes.sln");
            context.NuGetRestore(sln, new NuGetRestoreSettings { Verbosity = NuGetVerbosity.Quiet });
            CleanAndBuild(context, sln);

            var dist = context.Directory("dist");
            var (pkg, pkgBin) = CreateAndCleanDist(context, dist);

            var manifest = pkg.CombineWithFilePath("Engage.Dnn.SqlServerTypes.dnn");
            CopyPackageFiles(context, pkgBin, manifest);
            SetManifestVersions(context, manifest, version);

            context.Zip(pkg, dist.Path.CombineWithFilePath($"Engage.Dnn.SqlServerTypes_{version}_Install.zip"));

            context.DeleteDirectory(pkg, new DeleteDirectorySettings { Force = true, Recursive = true, });
        }

        private static (DirectoryPath pkg, DirectoryPath pkgBin) CreateAndCleanDist(Context context, ConvertableDirectoryPath dist)
        {
            context.CreateDirectory(dist);
            context.CleanDirectory(dist);
            var pkg = dist.Path.Combine("pkg");
            var pkgBin = pkg.Combine("bin");
            context.CreateDirectory(pkgBin);

            return (pkg, pkgBin);
        }

        private static string ReadVersion(Context context)
        {
            var packagesConfig = context.File("src/packages.config");
            var version = context.XmlPeek(packagesConfig, "//package[@id=\"Microsoft.SqlServer.Types\"]/@version");
            context.Information($"Microsoft.SqlServer.Types NuGet package version {version}");
            return version;
        }

        private static void CopyPackageFiles(Context context, DirectoryPath pkgBin, FilePath manifest)
        {
            context.CopyFiles("src/bin/Release/**/*.dll", pkgBin, preserveFolderStructure: true);
            context.CopyFile("src/bin/Release/Engage.Dnn.SqlServerTypes.dnn", manifest);
        }

        private static void SetManifestVersions(Context context, FilePath manifest, string version)
        {
            context.XmlPoke(manifest, "//package/@version", version);
            context.XmlPoke(manifest, "//assembly[name/text()=\"Engage.Dnn.SqlServerTypes.dll\"]/version", version);
            context.XmlPoke(manifest, "//assembly[name/text()=\"Microsoft.SqlServer.Types.dll\"]/version", version);
            context.XmlPoke(manifest, "//assembly[name/text()=\"SqlServerSpatial140.dll\"]/version", version);
        }

        private static void CleanAndBuild(Context context, FilePath sln)
        {
            var settings = new MSBuildSettings { Configuration = "Release", MaxCpuCount = 0, Verbosity = Verbosity.Minimal, }
                .WithRestore()
                .WithTarget("clean")
                .WithTarget("build");
            context.MSBuild(sln, settings);
        }

        private static void SetAssemblyInfoVersion(Context context, string version)
        {
            var assemblyInfoPath = context.File("src/Properties/AssemblyInfo.cs");
            var assemblyInfo = context.ParseAssemblyInfo(assemblyInfoPath);
            context.CreateAssemblyInfo(
                assemblyInfoPath,
                new AssemblyInfoSettings
                    {
                        Version = version,
                        FileVersion = version,
                        InformationalVersion = version,
                        Company = assemblyInfo.Company,
                        Configuration = assemblyInfo.Configuration,
                        CLSCompliant = assemblyInfo.ClsCompliant,
                        Copyright = assemblyInfo.Copyright,
                        Description = assemblyInfo.Description,
                        Guid = string.IsNullOrWhiteSpace(assemblyInfo.Guid) ? null : assemblyInfo.Guid,
                        Product = assemblyInfo.Product,
                        Title = assemblyInfo.Title,
                        Trademark = assemblyInfo.Trademark,
                        ComVisible = assemblyInfo.ComVisible,
                        InternalsVisibleTo = assemblyInfo.InternalsVisibleTo,
                    });
        }
    }
}