using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using Dargon.Distributed;
using ItzWarty;

namespace Dargon.Distributed
{
   public unsafe class InMemoryCache<K, V> : ICache<K, V>
   {
      private const uint SECTOR_COUNT = 128;

      private readonly string name;
      private readonly IReadOnlyDictionary<string, ICacheIndex> indicesByName = null;

      private readonly ConcurrentDictionary<K, V> dict = new ConcurrentDictionary<K, V>();
      private readonly object[] locksBySector = Util.Generate((int)SECTOR_COUNT, i => new object());

      public InMemoryCache(string name, IReadOnlyList<ICacheIndex> indicesByName)
      {
         this.name = name;
         this.indicesByName = (indicesByName ?? new List<ICacheIndex>()).Aggregate(new Dictionary<string, ICacheIndex>(), (dict, value) => dict.With(d => d.Add(value.Name, value)));
      }

      public R Invoke<R>(K key, IEntryProcessor<K, V, R> entryProcessor)
      {
         var hash = key.GetHashCode();
         var sectorId = (*(uint*)&hash) % SECTOR_COUNT;
         var sector = locksBySector[sectorId];
         lock (sector) {
            return InvokeHelper(key, entryProcessor);
         }
      }

      public IDictionary<K, R> InvokeAll<R>(ISet<K> keys, IEntryProcessor<K, V, R> entryProcessor)
      {
         var resultDict = new Dictionary<K, R>(keys.Count);
         foreach (var keyGroup in keys.GroupBy((k) => unchecked((uint)k.GetHashCode()) % SECTOR_COUNT)) {
            var sectorId = keyGroup.Key;
            var sector = locksBySector[sectorId];
            lock (sector) {
               foreach (var key in keyGroup) {
                  var result = InvokeHelper(key, entryProcessor);
                  resultDict.Add(key, result);
               }
            }
         }
         return resultDict;
      }

      public ICacheIndex<K, V, TProjection> GetIndex<TProjection>(string name)
      {
         return (ICacheIndex<K, V, TProjection>)indicesByName[name];
      }

      public ISet<K> Filter<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, TProjection value)
      {
         var index = (InMemoryCacheIndex<K, V, TProjection>)cacheIndex;
         var entries = this.dict.Select(kvp => new InMemoryEntry<K, V>(kvp.Key, kvp.Value, true));
         var results = entries.Select(entry => new { Key = entry.Key, Projection = index.Projector.Project(entry) }).Where((pair) => pair.Projection.Equals(value));
         return new HashSet<K>(results.Select(result => result.Key));
      }

      public ISet<K> Filter<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, IEntryProcessor<K, IFilterArgument<V, TProjection>, bool> filter)
      {
         var index = (InMemoryCacheIndex<K, V, TProjection>)cacheIndex;
         var entries = this.dict.Select(kvp => new InMemoryEntry<K, IFilterArgument<V, TProjection>>(kvp.Key, new InMemoryFilterArgument<V, TProjection>(kvp.Value, index.Projector.Project(new InMemoryEntry<K, V>(kvp.Key, kvp.Value, true))), true));
         return new HashSet<K>(entries.Select(entry => entry.Key));
      }

      public ISet<IEntry<K, V>> FilterEntries<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, TProjection value)
      {
         var index = (InMemoryCacheIndex<K, V, TProjection>)cacheIndex;
         var entries = this.dict.Select(kvp => new InMemoryEntry<K, V>(kvp.Key, kvp.Value, true));
         var results = entries.Select(entry => new { Entry = entry, Projection = index.Projector.Project(entry) }).Where((pair) => pair.Projection.Equals(value));
         return new HashSet<IEntry<K, V>>(results.Select(result => result.Entry));
      }

      public ISet<IEntry<K, V>> FilterEntries<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, IEntryProcessor<K, IFilterArgument<V, TProjection>, bool> filter)
      {
         var index = (InMemoryCacheIndex<K, V, TProjection>)cacheIndex;
         var entries = this.dict.Select(kvp => new InMemoryEntry<K, IFilterArgument<V, TProjection>>(kvp.Key, new InMemoryFilterArgument<V, TProjection>(kvp.Value, index.Projector.Project(new InMemoryEntry<K, V>(kvp.Key, kvp.Value, true))), true));
         return new HashSet<IEntry<K, V>>(entries.Select(e => new InMemoryEntry<K, V>(e.Key, e.Value.Value, e.IsPresent)));
      }

      private R InvokeHelper<R>(K key, IEntryProcessor<K, V, R> entryProcessor)
      {
         V value;
         bool isPresent = dict.TryGetValue(key, out value);
         var entry = new InMemoryEntry<K, V>(key, value, isPresent);
         var result = entryProcessor.Process(entry);
         if (entry.IsDirty || entry.IsRemoved)
         {
            if (entry.IsRemoved)
            {
               V removedValue;
               dict.TryRemove(key, out removedValue);
            }
            else
            {
               dict[key] = entry.Value;
            }
         }
         return result;
      }
      
      public bool TryGetValue(K key, out V value) { return dict.TryGetValue(key, out value); }
      public V this[K key] { get { return dict.GetValueOrDefault(key); } set { dict[key] = value; } }

      public bool Contains(KeyValuePair<K, V> item)
      {
         V value;
         return dict.ContainsKey(item.Key) && dict.TryGetValue(item.Key, out value) && value.Equals(item.Value);
      }

      public bool ContainsKey(K key) { return dict.ContainsKey(key); }

      public void Add(K key, V value) { dict.TryAdd(key, value); }

      public bool Remove(K key)
      {
         V value;
         return dict.TryRemove(key, out value);
      }

      public int Count { get { return dict.Count; } }
      public void Clear() { dict.Clear(); }

      public ICollection<K> Keys { get { return dict.Keys; } }
      public ICollection<V> Values { get { return dict.Values; } }
      public IEnumerator<KeyValuePair<K, V>> GetEnumerator() { return dict.GetEnumerator(); }
      public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) { throw new NotImplementedException(); }

      IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
      public void Add(KeyValuePair<K, V> item) { Add(item.Key, item.Value); }
      public bool Remove(KeyValuePair<K, V> item) { throw new NotImplementedException(); }

      public bool IsReadOnly { get { return false; }}
      public string Name { get { return name; } }

      public class InMemoryCacheIndex<TKey, TValue, TProjection> : ICacheIndex<TKey, TValue, TProjection>
      {
         private readonly string name;
         private readonly ICacheProjector<TKey, TValue, TProjection> projector;

         public InMemoryCacheIndex(string name, ICacheProjector<TKey, TValue, TProjection> projector)
         {
            this.name = name;
            this.projector = projector;
         }

         public string Name { get { return name; } }
         public ICacheProjector<TKey, TValue, TProjection> Projector { get { return projector; } }
      }

      public class InMemoryFilterArgument<V, TProjection> : IFilterArgument<V, TProjection>
      {
         private readonly V value;
         private readonly TProjection projection;

         public InMemoryFilterArgument(V value, TProjection projection)
         {
            this.value = value;
            this.projection = projection;
         }

         public V Value { get { return value; } }
         public TProjection Projection { get { return projection; } }
      }
   }
}
