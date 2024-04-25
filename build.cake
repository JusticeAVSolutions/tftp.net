#addin nuget:?package=JAVS.Cake&version=4.0.0.3&loaddependencies=true

var target = Argument("target", "Default");
var config = Argument("config", "Release");
var buildNumber = Argument("buildNumber", "0");
var fullVersion = $"1.3.1.{buildNumber}";

Task("Update-Version")
    .Does(() =>
    {
        foreach (var csproj in GetFiles("**/*.csproj"))
        {
            Information($"Updating {csproj} to {fullVersion}...");
            RegexReplaceInFile(csproj.ToString(), "(<Version>)([0-9\\.]+)(</Version>)", $"${{1}}{fullVersion}$3");
        }

        foreach (var file in GetFiles("**/AssemblyInfo.cs"))
        {
            Information($"Updating {file} to {fullVersion}...");
            RegexReplaceInFile(file.ToString(), "(\\[assembly: AssemblyVersion\\(\")([0-9\\.]+)(\"\\)\\])", $"${{1}}{fullVersion}$3");
            RegexReplaceInFile(file.ToString(), "(\\[assembly: AssemblyFileVersion\\(\")([0-9\\.]+)(\"\\)\\])", $"${{1}}{fullVersion}$3");
        }
    });

Task("Build-Project")
    .Does(() => DotNetBuild("JAVS.TFTP", new DotNetBuildSettings
    {
        Configuration = config,
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error),
    }));

Task("Default")
    .IsDependentOn("Update-Version")
    .IsDependentOn("Build-Project");

RunTarget(target);
