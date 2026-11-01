using System.Text.Json;

namespace Portal.Web.Data;

/// <summary>
/// A tiny, thread-safe, file-backed collection of records of type T, keyed by a string Id.
/// This is the Portal's whole persistence layer for this scaffold (see README: "Why no EF
/// Core"). Each collection is one JSON array file under App_Data/. Reads are served from
/// an in-memory copy; every write re-serializes the whole collection to disk, which is
/// completely fine at pilot scale (one class, a few hundred rows per collection) and keeps
/// the code free of any database/ORM dependency. Swap this for an
/// IRepository&lt;T&gt;-over-EF-Core implementation later without touching the controllers,
/// which only depend on JsonCollection&lt;T&gt;'s public surface.
/// </summary>
public class JsonCollection<T>
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private List<T> _items = new();
    private readonly Func<T, string> _getId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public JsonCollection(string dataDirectory, string fileName, Func<T, string> getId)
    {
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, fileName);
        _getId = getId;
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            _items = new List<T>();
            return;
        }

        var json = File.ReadAllText(_filePath);
        _items = string.IsNullOrWhiteSpace(json)
            ? new List<T>()
            : JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_items, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public List<T> All()
    {
        lock (_lock)
        {
            return new List<T>(_items);
        }
    }

    public T? Find(string id)
    {
        lock (_lock)
        {
            return _items.FirstOrDefault(i => _getId(i) == id);
        }
    }

    public List<T> Where(Func<T, bool> predicate)
    {
        lock (_lock)
        {
            return _items.Where(predicate).ToList();
        }
    }

    public void Upsert(T item)
    {
        lock (_lock)
        {
            var id = _getId(item);
            var index = _items.FindIndex(i => _getId(i) == id);
            if (index >= 0)
                _items[index] = item;
            else
                _items.Add(item);
            Save();
        }
    }

    public void Delete(string id)
    {
        lock (_lock)
        {
            _items.RemoveAll(i => _getId(i) == id);
            Save();
        }
    }

    public bool SeedIfEmpty(IEnumerable<T> seed)
    {
        lock (_lock)
        {
            if (_items.Count > 0) return false;
            _items = seed.ToList();
            Save();
            return true;
        }
    }
}
