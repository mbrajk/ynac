namespace ynac.OSFeatures;

internal class FileService : IFileService
{
    public bool Exists(string path) => File.Exists(path);
    
    public string[] ReadAllLines(string path) => File.ReadAllLines(path);
    
    public void WriteAllLines(string path, string[] contents) => File.WriteAllLines(path, contents);
    
    public void WriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path, contents);
}
