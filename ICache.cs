using System.Collections;
using System.Collections.Generic;
using Dargon.Hydar.Caching;

namespace Dargon.Hydar {
   public interface ICache {
      string Name { get; }
   }

   public interface ICache<K, V> : IDictionary<K, V>, ICache {
      R Invoke<R>(K key, EntryProcessor<K, V, R> entryProcessor);
      IDictionary<K, R> InvokeAll<R>(ISet<K> keys, EntryProcessor<K, V, R> entryProcessor);

      ICacheIndex<K, V, TProjection> GetIndex<TProjection>(string name);

      ISet<K> Filter<TProjection>(ICacheIndex<K, V, TProjection> index, TProjection value);
      ISet<K> Filter<TProjection>(ICacheIndex<K, V, TProjection> index, EntryProcessor<K, IFilterArgument<V, TProjection>, bool> filter);

      ISet<ReadableEntry<K, V>> FilterEntries<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, TProjection value);
      ISet<ReadableEntry<K, V>> FilterEntries<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, EntryProcessor<K, IFilterArgument<V, TProjection>, bool> filter);

      IDictionary<K, V> GetMany(IEnumerable<K> keys);
      IDictionary<K, V> this[IEnumerable<K> keys] { get; }

      IDictionary<K, V> GetAll();
   }
}
