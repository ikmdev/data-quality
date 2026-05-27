using System.Collections.Concurrent;

namespace PIQI_Engine.Server.Services
{
    /// <summary>
    /// A thread-safe, in-memory cache service for storing and retrieving key-value pairs.
    /// </summary>
    public sealed class FileCacheService
    {
        /// <summary>
        /// Represents a cached item with its value and the last modified timestamp.
        /// </summary>
        /// <typeparam name="T">Type of the cached value.</typeparam>
        public class CacheItem<T>
        {
            /// <summary>
            /// Gets or sets the cached value.
            /// </summary>
            public T Value { get; set; } = default!;

            /// <summary>
            /// Gets or sets the last modified timestamp of the cache item.
            /// </summary>
            public DateTime LastModified { get; set; }
        }

        /// <summary>
        /// Internal concurrent dictionary used for storing cache items.
        /// </summary>
        private readonly ConcurrentDictionary<string, object> _items = new();

        /// <summary>
        /// Gets all items currently stored in the cache.
        /// </summary>
        public ConcurrentDictionary<string, object> SavedItems => _items;

        /// <summary>
        /// Adds a new item to the cache.
        /// </summary>
        /// <typeparam name="T">Type of the value to store.</typeparam>
        /// <param name="key">Unique key for the cache item.</param>
        /// <param name="value">Value to store in the cache.</param>
        /// <returns>True if the item was added; false if the key already exists.</returns>
        public bool Add<T>(string key, T value)
        {
            var item = new CacheItem<T>
            {
                Value = value,
                LastModified = DateTime.UtcNow
            };
            return _items.TryAdd(key, item);
        }

        /// <summary>
        /// Adds or updates an item in the cache. If the key exists, it updates the value; otherwise, it adds a new entry.
        /// </summary>
        /// <typeparam name="T">Type of the value to store.</typeparam>
        /// <param name="key">Unique key for the cache item.</param>
        /// <param name="value">Value to store in the cache.</param>
        /// <returns>Always returns true since AddOrUpdate always succeeds.</returns>
        public bool AddOrUpdate<T>(string key, T value)
        {
            var item = new CacheItem<T>
            {
                Value = value,
                LastModified = DateTime.UtcNow
            };

            _items.AddOrUpdate(
                key,
                item,                // If key does not exist
                (k, existing) => item // If key already exists
            );

            return true;
        }

        /// <summary>
        /// Retrieves a cached item by its key.
        /// </summary>
        /// <typeparam name="T">Type of the cached value.</typeparam>
        /// <param name="key">Key of the item to retrieve.</param>
        /// <param name="item">Outputs the cached item if found; otherwise, null.</param>
        /// <returns>True if the item exists in the cache; otherwise, false.</returns>
        public bool Get<T>(string key, out CacheItem<T>? item)
        {
            if (_items.TryGetValue(key, out var value) && value is CacheItem<T> itemOut)
            {
                item = itemOut;
                return true;
            }

            item = null;
            return false;
        }

        /// <summary>
        /// Removes a cached item by its key.
        /// </summary>
        /// <param name="key">Key of the item to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
        public bool Remove(string key)
        {
            return _items.TryRemove(key, out _);
        }

        /// <summary>
        /// Clears all items from the cache.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }
    }
}
