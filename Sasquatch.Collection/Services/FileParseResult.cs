namespace Sasquatch.Collection.Services;

/// <summary>
/// Result from parsing a file upload - contains data, warnings, and errors
/// </summary>
/// <typeparam name="T">The type of data records parsed</typeparam>
public class FileParseResult<T>
{
    public List<T> Data { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();

    public bool Success => !Errors.Any();
    public int RecordsProcessed => Data.Count;
    public int WarningCount => Warnings.Count;
    public int ErrorCount => Errors.Count;

    /// <summary>
    /// Add a warning message
    /// </summary>
    public void AddWarning(string message)
    {
        Warnings.Add(message);
    }

    /// <summary>
    /// Add an error message
    /// </summary>
    public void AddError(string message)
    {
        Errors.Add(message);
    }
}
