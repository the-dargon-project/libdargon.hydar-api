using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.Collections;

namespace Dargon.Distributed.Server
{
   public class LocalCachePuppet<K, V> : ICachePuppet<K, V>
   {
      private readonly DistributedConfiguration configuration;
      private readonly InMemoryCache<K, V> localCache;
      private readonly ConcurrentSet<K> lockedKeys = new ConcurrentSet<K>();
      private readonly ConcurrentDictionary<int, ITransactionDescription> transactionsById = new ConcurrentDictionary<int, ITransactionDescription>();

      public LocalCachePuppet(DistributedConfiguration configuration)
      {
         this.configuration = configuration;
         this.localCache = new InMemoryCache<K, V>(configuration.Name, null);
         this.puppetClient = new PuppetClient<K, V>(this, configuration);
      }

      public async Task<bool> TryBeginCommitAsync<R, TProcessor>(K[] keys, int transactionId, TProcessor processor) where TProcessor : IEntryProcessor<K, V, R>
      {
         if (LockKeys(keys)) {
            var desc = new TransactionDescription<R>(keys, processor);
            transactionsById.TryAdd(transactionId, desc);
            return true;
         } else {
            return false;
         }
      }

      private bool LockKeys(K[] keys)
      {
         for (var i = 0; i < keys.Length; i++) {
            if (!lockedKeys.TryAdd(keys[i])) {
               for (var j = 0; j < i; j++) {
                  lockedKeys.TryRemove(keys[i]);
               }
               return false;
            }
         }
         return true;
      }

      private void UnlockKeys(K[] keys)
      {
         foreach (var key in keys) {
            lockedKeys.TryRemove(key);
         }
      }

      public async Task<bool> CancelCommitAsync(int transactionId)
      {
         ITransactionDescription transaction;
         if (transactionsById.TryRemove(transactionId, out transaction)) {
            UnlockKeys(transaction.Keys);
            return true;
         } else {
            throw new InvalidOperationException();
         }
      }

      public async Task<IDictionary<K, R>> FinishCommitAsync<R>(int transactionId)
      {
         ITransactionDescription t;
         if (transactionsById.TryRemove(transactionId, out t)) {
            var transaction = (ITransactionDescription<R>)t;
            var result = localCache.InvokeAll(new HashSet<K>(transaction.Keys), transaction.EntryProcessor);
            UnlockKeys(transaction.Keys);
            return result;
         } else {
            throw new InvalidOperationException();
         }
      }

      public interface ITransactionDescription
      {
         K[] Keys { get; }  
      }

      public interface ITransactionDescription<R> : ITransactionDescription
      {
         IEntryProcessor<K, V, R> EntryProcessor { get; }
      }
      public class TransactionDescription<R> : ITransactionDescription<R>
      {
         private readonly K[] keys;
         private readonly IEntryProcessor<K, V, R> entryProcessor;

         public TransactionDescription(K[] keys, IEntryProcessor<K, V, R> entryProcessor)
         {
            this.keys = keys;
            this.entryProcessor = entryProcessor;
         }

         public K[] Keys { get { return keys; } }
         public IEntryProcessor<K, V, R> EntryProcessor { get { return entryProcessor; } }
      }
   }
}
