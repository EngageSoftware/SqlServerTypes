namespace Engage.Dnn.SqlServerTypes.Build.Tasks
{
    using System.Linq;

    using Cake.Common.Build;
    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Solution.Project.Properties;
    using Cake.Common.Tools.MSBuild;
    using Cake.Common.Xml;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;
    using Cake.Frosting;

    public sealed class Package : FrostingTask<Context>
    {
        public override void Run(Context context)
        {
            var dist = context.Directory("dist");
            var pkg = dist.Path.Combine("pkg");
            var pkgBin = pkg.Combine("bin");
            var packagesConfig = context.File("src/packages.config");
            var assemblyInfoPath = context.File("src/Properties/AssemblyInfo.cs");

            var version = context.XmlPeek(packagesConfig, "//package[@id=\"Microsoft.SqlServer.Types\"]/@version");
            context.Information($"Microsoft.SqlServer.Types NuGet package version {version}");

            var assemblyInfo = context.ParseAssemblyInfo(assemblyInfoPath);
            context.CreateAssemblyInfo(assemblyInfoPath, new AssemblyInfoSettings
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

            var settings =
                new MSBuildSettings { Configuration = "Release", MaxCpuCount = 0, Verbosity = Verbosity.Minimal, }
                    .WithRestore()
                    .WithTarget("clean")
                    .WithTarget("build");
            context.MSBuild("Engage.Dnn.SqlServerTypes.sln", settings);

            context.CreateDirectory(dist);
            context.CleanDirectory(dist);
            context.CreateDirectory(pkgBin);

            context.CopyFiles("src/bin/Release/**/*.dll", pkgBin, preserveFolderStructure: true);
            var manifest = pkg.CombineWithFilePath("Engage.Dnn.SqlServerTypes.dnn");
            context.CopyFile("src/bin/Release/Engage.Dnn.SqlServerTypes.dnn", manifest);

            context.XmlPoke(manifest, "//package/@version", version);
            context.XmlPoke(manifest, "//assembly[name/text()=\"Engage.Dnn.SqlServerTypes.dll\"]/version", version);
            context.XmlPoke(manifest, "//assembly[name/text()=\"Microsoft.SqlServer.Types.dll\"]/version", version);
            context.XmlPoke(manifest, "//assembly[name/text()=\"SqlServerSpatial140.dll\"]/version", version);

            context.Zip(pkg, dist.Path.CombineWithFilePath($"Engage.Dnn.SqlServerTypes_{version}_Install.zip"));

            context.DeleteDirectory(pkg, new DeleteDirectorySettings { Force = true, Recursive = true, });
        }
    }
}