using System.IO;

namespace QuoteService.Model;

public class ScriptConfiguration
{
    public string? PythonEnv { get; set; }
    public required string CliProgram { get; set; }
    public required string ScriptName { get; set; }
    public required string ScriptLocation { get; set; }
    public required string PointsFileName { get; set; }
    public required string ChartFileName { get; set; }
    public required string ExportDateFormat { get; set; }

    public string ScriptFullName => Path.Combine(ScriptLocation, ScriptName);
}