using System.Collections.Generic;

namespace Dargon.Distributed
{
   public interface ICache<K, V> : IDictionary<K, V>, ICache
   {
      R Invoke<R>(K key, IEntryProcessor<K, V, R> entryProcessor);
      IDictionary<K, R> InvokeAll<R>(ISet<K> keys, IEntryProcessor<K, V, R> entryProcessor);
   }

   public interface ICache
   {
      string Name { get; }
   }
}