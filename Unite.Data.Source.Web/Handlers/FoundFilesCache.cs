namespace Unite.Data.Source.Web.Handlers;

public class FoundFilesCache
{
    private readonly string _path;    
    private readonly HashSet<string> _entries = [];


    public FoundFilesCache(string path)
    {
        if (!File.Exists(path))
            File.Create(path).Close();

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
