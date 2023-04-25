// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

public class GroupedDictionary<TGroup, TKey, TValue> : IGroupedDictionary<TGroup, TKey, TValue> where TGroup : notnull where TKey : notnull
{
    private readonly IEqualityComparer<TGroup> _groupComparer;
    private readonly IEqualityComparer<TKey>? _keyComparer;

    /// <summary>Represents a collection of keys and values separated into groups.</summary>
    /// <typeparam name="TGroup">The type of the groups in the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="categorizeOnKey">The function that should be used to categorize the elements</param>
    public GroupedDictionary(IGroupedDictionary<TGroup, TKey, TValue>.Categorize categorizeOnKey) : 
        this(categorizeOnKey, 0, 0, null, null) {}

    /// <summary>Represents a collection of keys and values separated into groups.</summary>
    /// <typeparam name="TGroup">The type of the groups in the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="categorizeOnKey">The function that should be used to categorize the elements</param>
    /// <param name="groupCapacity">How many groups is expected</param>
    public GroupedDictionary(IGroupedDictionary<TGroup, TKey, TValue>.Categorize categorizeOnKey, int groupCapacity) : 
        this(categorizeOnKey, groupCapacity, 0, null, null) {}

    /// <summary>Represents a collection of keys and values separated into groups.</summary>
    /// <typeparam name="TGroup">The type of the groups in the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="categorizeOnKey">The function that should be used to categorize the elements</param>
    /// <param name="groupCapacity">How many groups is expected</param>
    /// <param name="defaultGroupSizeCapacity">How many elements is expected in each group. This can also me set using <see cref="DefaultGroupSizeCapacity"/></param>
    public GroupedDictionary(IGroupedDictionary<TGroup, TKey, TValue>.Categorize categorizeOnKey,
        int groupCapacity,
        int defaultGroupSizeCapacity) : 
        this(categorizeOnKey, groupCapacity, defaultGroupSizeCapacity, null, null) {}

    /// <summary>Represents a collection of keys and values separated into groups.</summary>
    /// <typeparam name="TGroup">The type of the groups in the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="categorizeOnKey">The function that should be used to categorize the elements</param>
    /// <param name="groupEqualityComparer">The <see cref="IEqualityComparer{TGroup}"/> implementation to use when comparing group keys, or <b>null</b> to use the default <see cref="IEqualityComparer{TGroup}"/> for the type of key.</param>
    /// <param name="keyEqualityComparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing element keys, or <b>null</b> to use the default <see cref="IEqualityComparer{TKey}"/> for the type of key.</param>
    public GroupedDictionary(IGroupedDictionary<TGroup, TKey, TValue>.Categorize categorizeOnKey,
        IEqualityComparer<TGroup>? groupEqualityComparer,
        IEqualityComparer<TKey>? keyEqualityComparer) : 
        this(categorizeOnKey, 0, 0, groupEqualityComparer, keyEqualityComparer) {}

    /// <summary>Represents a collection of keys and values separated into groups.</summary>
    /// <typeparam name="TGroup">The type of the groups in the dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="categorizeOnKey">The function that should be used to categorize the elements</param>
    /// <param name="groupCapacity">How many groups is expected</param>
    /// <param name="defaultGroupSizeCapacity">How many elements is expected in each group. This can also me set using <see cref="DefaultGroupSizeCapacity"/></param>
    /// <param name="groupEqualityComparer">The <see cref="IEqualityComparer{TGroup}"/> implementation to use when comparing group keys, or <b>null</b> to use the default <see cref="IEqualityComparer{TGroup}"/> for the type of key.</param>
    /// <param name="keyEqualityComparer">The <see cref="IEqualityComparer{TKey}"/> implementation to use when comparing element keys, or <b>null</b> to use the default <see cref="IEqualityComparer{TKey}"/> for the type of key.</param>
    public GroupedDictionary(IGroupedDictionary<TGroup, TKey, TValue>.Categorize categorizeOnKey,
        int groupCapacity,
        int defaultGroupSizeCapacity,
        IEqualityComparer<TGroup>? groupEqualityComparer,
        IEqualityComparer<TKey>? keyEqualityComparer)
    {
        ArgumentNullException.ThrowIfNull(categorizeOnKey);

        CategorizeOnKey = categorizeOnKey;
        DefaultGroupSizeCapacity = defaultGroupSizeCapacity;

            _groupComparer = groupEqualityComparer ?? EqualityComparer<TGroup>.Default;
        _keyComparer = keyEqualityComparer;

        Groups = new Dictionary<TGroup, IDictionary<TKey, TValue>>(groupCapacity, _groupComparer);
    }

    public IGroupedDictionary<TGroup, TKey, TValue>.Categorize CategorizeOnKey { get; }

    /// <inheritdoc />
    public int DefaultGroupSizeCapacity { get; set; }

    #region IDictionary
    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var group in Groups.Values)
        {
            foreach (var item in group)
            {
                yield return item;
            }
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    /// <inheritdoc />
    public void Add(KeyValuePair<TKey, TValue> item) =>
        Add(item.Key, item.Value);

    /// <inheritdoc />
    public void Clear() =>
        Groups.Clear();

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        var grpKey = CategorizeOnKey(item.Key);
        return grpKey != null && Groups[grpKey].Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex <= 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");

        foreach (var item in this)
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(item.Key, item.Value);
    }

    /// <inheritdoc />
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        try
        {
            var grpKey = CategorizeOnKey(item.Key);
            return grpKey != null && Groups[grpKey].Remove(item);
        }
        finally
        {
            RemoveEmptyGroups();
        }
    }

    /// <inheritdoc />
    public int Count => Groups.Values.Select(grp => grp.Count).Sum();
    /// <inheritdoc />
    public bool IsReadOnly => Groups.Values.Select(grp => grp.IsReadOnly).Aggregate((result, value) => result && value);

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        var grpKey = CategorizeOnKey(key) ?? throw new KeyNotFoundException($"The given key '{key}' could not be categorized.");

        if (!Groups.TryGetValue(grpKey, out var grp))
        {
            grp = new Dictionary<TKey, TValue>(DefaultGroupSizeCapacity, _keyComparer);
            Groups.Add(grpKey, grp);
        }

        grp.Add(key, value);
    }

    /// <inheritdoc />
    public bool ContainsKey(TKey key)
    {
        var grpKey = CategorizeOnKey(key);
        return grpKey != null && Groups[grpKey].ContainsKey(key);
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        try
        {
            var grpKey = CategorizeOnKey(key);
            return grpKey != null && Groups[grpKey].Remove(key);
        }
        finally
        {
            RemoveEmptyGroups();
        }
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
        var grpKey = CategorizeOnKey(key);
        if (grpKey != null)
        {
            return Groups[grpKey].TryGetValue(key, out value!);
        }

        value = default!;
        return false;
    }

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get
        {
            var grpKey = CategorizeOnKey(key);
            if (grpKey != null) return Groups[grpKey][key];
            throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
        }
        set => Add(key, value);
    }

    /// <inheritdoc />
    public ICollection<TKey> Keys =>
        Groups.Values.Select(grp => grp.Keys).Aggregate(new List<TKey>(), (result, list) =>
        {
            result.AddRange(list);
            return result;
        });

    /// <inheritdoc />
    public ICollection<TValue> Values =>
        Groups.Values.Select(grp => grp.Values).Aggregate(new List<TValue>(), (result, list) =>
        {
            result.AddRange(list);
            return result;
        });

    /// <inheritdoc />
    public IDictionary<TGroup, IDictionary<TKey, TValue>> Groups { get; }
    #endregion

    /// <inheritdoc />
    public void Group(bool continueOnDuplicates = true)
    {
        var  misplaced = new Dictionary<TKey, TValue>(_keyComparer);
        foreach (var grpKey in Groups.Keys)
        {
            var removeList = new List<TKey>();
            foreach (var key in Groups[grpKey].Keys)
            {
                if (_groupComparer.Equals(grpKey, CategorizeOnKey(key))) continue;
                removeList.Add(key);
                misplaced.Add(key, Groups[grpKey][key]);
            }

            foreach (var key in removeList) Groups[grpKey].Remove(key);
        }

        foreach (var item in misplaced)
            try
            {
                Add(item);
            }
            catch
            {
                if (!continueOnDuplicates)
                    throw;
            }

        RemoveEmptyGroups();
    }

    /// <inheritdoc />
    public void RemoveEmptyGroups()
    {
        var removeList = (from @group in Groups where @group.Value.Count == 0 select @group.Key).ToList();

        foreach (var group in removeList) Groups.Remove(group);
    }

    /// <inheritdoc />
    public bool ValidateGroups(out IList<TKey> misplaced, bool failFast = false)
    {
        misplaced = new List<TKey>();
        foreach (var grpKey in Groups.Keys)
        {
            foreach (var key in Groups[grpKey].Keys)
            {
                if (grpKey.Equals(CategorizeOnKey(key))) continue;

                misplaced.Add(key);
                if (failFast) return false;
            }
        }

        return misplaced.Count > 0;
    }
}