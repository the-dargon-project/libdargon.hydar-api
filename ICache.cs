using System;
using System.Collections.Generic;

namespace Dargon.Distributed
{
   public interface ICache<K, V> : IDictionary<K, V>, ICache
   {
      R Invoke<R>(K key, IEntryProcessor<K, V, R> entryProcessor);
      IDictionary<K, R> InvokeAll<R>(ISet<K> keys, IEntryProcessor<K, V, R> entryProcessor);

      ICacheIndex<K, V, TProjection> GetIndex<TProjection>(string name);

      ISet<K> Filter<TProjection>(ICacheIndex<K, V, TProjection> index, TProjection value);
      ISet<K> Filter<TProjection>(ICacheIndex<K, V, TProjection> index, IEntryProcessor<K, IFilterArgument<V, TProjection>, bool> filter);

      ISet<IEntry<K, V>> FilterEntries<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, TProjection value);
      ISet<IEntry<K, V>> FilterEntries<TProjection>(ICacheIndex<K, V, TProjection> cacheIndex, IEntryProcessor<K, IFilterArgument<V, TProjection>, bool> filter);
   }

   public interface ICache
   {
      string Name { get; }
   }
}