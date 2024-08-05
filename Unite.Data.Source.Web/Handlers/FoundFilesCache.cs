namespace Unite.Data.Source.Web.Handlers;

public class FoundFilesCache
{
    private readonly string _path;    
    private readonly HashSet<string> _entries = [];


    public FoundFilesCache(string path)
    {
        Console.WriteLine($"Creating cache at {path}");

        var folderPath = Path.GetDirectoryName(path);
        var filePath = path;

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        
        if (!File.Exists(filePath))
            File.Create(filePath).Close();

        _path = path;
        _entries = File.ReadAllLines(path).ToHashSet();
    }


    public void Add(string entry)
    {
        var created = _entries.Add(entry);

        if (created)
        {
            File.AppendAllLines(_path, [entry]);
        }
    }

    public bool Contains(string entry)
    {
        return _entries.Contains(entry);
    }
}
