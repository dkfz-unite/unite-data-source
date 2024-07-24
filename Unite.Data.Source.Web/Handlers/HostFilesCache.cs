namespace Unite.Data.Source.Web.Handlers;

public class HostFilesCache
{
    private readonly string _path;
    private readonly Dictionary<string, string> _entries = [];


    public HostFilesCache(string path)
    {
        if (!File.Exists(path))
            File.Create(path).Close();

        _path = path;
        _entries = File
            .ReadAllLines(path)
            .Select(line => line.Split('\t'))
            .ToDictionary(parts => parts[0], parts => parts[1]);
    }


    public void Add(string key, string value)
    {
        var created = _entries.TryAdd(key, value);

        if (created)
        {
            var line = $"{key}\t{value}";
            File.AppendAllLines(_path, [line]);
        }
    }

    public string Get(string key)
    {
        return _entries.TryGetValue(key, out var value) ? value : null;
    }

    public bool Contains(string key)
    {
        return _entries.ContainsKey(key);
    }
}
