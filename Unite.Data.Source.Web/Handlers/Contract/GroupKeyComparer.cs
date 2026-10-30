using System.Reflection;

namespace Unite.Data.Source.Web.Handlers.Contract;

public class GroupKeyComparer<T> : IEqualityComparer<T> where T : class
{
    private static readonly PropertyInfo[] _properties = typeof(T)
        .GetProperties()
        .Where(property => Attribute.IsDefined(property, typeof(GroupKeyAttribute)))
        .ToArray();


    public bool Equals(T first, T second)
    {
        if (ReferenceEquals(first, second))
            return true;

        if (first == null || second == null)
            return false;

        return _properties.All(property => object.Equals(property.GetValue(first), property.GetValue(second)));
    }

    public int GetHashCode(T value)
    {
        var hash = new HashCode();

        foreach (var property in _properties)
            hash.Add(property.GetValue(value));

        return hash.ToHashCode();
    }
}
