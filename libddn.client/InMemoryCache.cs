using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using ItzWarty;

namespace Dargon.Distributed
{
   public unsafe class InMemoryCache<K, V> : ICache<K, V>
   {
      private const uint SECTOR_COUNT = 128;

      private string name;
      private ConcurrentDictionary<K, V> dict = new ConcurrentDictionary<K, V>();
      private object[] locksBySector = Util.Generate((int)SECTOR_COUNT, i => new object());

      public InMemoryCache(string name) { this.name = name; }

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
   }
}
