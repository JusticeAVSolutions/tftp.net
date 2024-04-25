using System.Text;
using System.Text.RegularExpressions;

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
            RegexReplaceInFile(csproj.ToString(), "(<PackageVersion>)([0-9\\.]+)(</PackageVersion>)", $"${{1}}{fullVersion}$3");
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

void RegexReplaceInFile(string filePath, string searchText, string replaceText)
{
    var encoding = GetEncoding(filePath);

    var oldContent = System.IO.File.ReadAllText(filePath, encoding);
    var newContent = Regex.Replace(oldContent, searchText, replaceText);

    System.IO.File.WriteAllText(filePath, newContent, encoding);
}

Encoding GetEncoding(string filePath)
{
    var bom = new byte[4];
    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
    {
        stream.Read(bom, 0, bom.Length);
    }

#pragma warning disable CS0618
#pragma warning disable SYSLIB0001
    if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
#pragma warning restore SYSLIB0001
#pragma warning restore CS0618
    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
    if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
    if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
    if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;

    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    return Encoding.GetEncoding(1252);
}