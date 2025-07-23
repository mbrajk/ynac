namespace ynac.OSFeatures;

public interface IFileService
{
    bool Exists(string path);
    string[] ReadAllLines(string path);
    void WriteAllLines(string path, string[] contents);
    void WriteAllLines(string path, IEnumerable<string> contents);
}
