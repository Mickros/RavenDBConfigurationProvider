// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

public interface IGroupedDictionary<TGroup, TKey, TValue>: IDictionary<TKey, TValue>
{
    /// <summary>
    /// Get the Groups the dictionary contains
    /// </summary>
    IDictionary<TGroup, IDictionary<TKey, TValue>> Groups { get; }

    /// <summary>
    /// Used to categorize <paramref name="key"/>
    /// </summary>
    /// <param name="key">The element to be categorized</param>
    /// <returns>The category of the <paramref name="key"/>. <see langword="Null"/> is used to indicate that a category could not be found</returns>
    public delegate TGroup? Categorize(TKey key);

    /// <summary>
/// The capacity for new groups
/// </summary>
    public int DefaultGroupSizeCapacity { get; set; }
    
    /// <summary>
    /// The function used to categorize based upon the <typeparamref name="TKey"/>
    /// </summary>
    public Categorize CategorizeOnKey { get; }

    /// <summary>
    /// Reorganize the groups
    /// </summary>
    /// <param name="continueOnDuplicates"><see langword="true" /> if we should discard duplicates as they are found; otherwise, <see langword="false" />.</param>
    void Group(bool continueOnDuplicates = true);

    /// <summary>
    /// Validate that each element is in the right group
    /// </summary>
    /// <param name="misplaced">A list of the misplaced elements</param>
    /// <param name="failFast">Fails upon the first misplacement</param>
    /// <returns><see langword="true" /> if elements is placed in the right group; otherwise, <see langword="false" />.</returns>
    bool ValidateGroups(out IList<TKey> misplaced, bool failFast = false);

    /// <summary>
    /// Cleanup empty groups
    /// </summary>
    void RemoveEmptyGroups();
}