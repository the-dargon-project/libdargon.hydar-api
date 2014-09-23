using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Distributed.Server
{
   public class RemoteCachePuppet<K, V> : ICachePuppet<K, V>
   {
      private readonly DistributedConfiguration configuration;
      private readonly NodeSession session;
      private readonly PofSerializer serializer;
      private readonly ConcurrentDictionary<int, TaskCompletionSource<object>> responseSourceByTransactionId = new ConcurrentDictionary<int, TaskCompletionSource<object>>(); 
      private int commitIdCounter = 0;

      public RemoteCachePuppet(DistributedConfiguration configuration, NodeSession session)
      {
         this.configuration = configuration;
         this.session = session;
         this.serializer = new PofSerializer(configuration.PofContext);
      }

      public async Task<bool> TryBeginCommitAsync(K key, int transactionId)
      {
         var responseSource = AllocateResponseSource(transactionId);
         Send(Opcode.PrepareCommit, new PrepareCommitRequest<K>(transactionId, key));
         var response = (PrepareCommitResponse)await GetResponse(responseSource);
         return response.Okay;
      }

      public async Task<bool> CancelCommitAsync(int transactionId)
      {
         Send(Opcode.CancelCommit, new CancelCommitRequest(transactionId));
         return true;
      }

      public async Task<R> FinishCommitAsync<R, TProcessor>(int transactionId, TProcessor processor) 
         where TProcessor : IEntryProcessor<K, V, R> 
      {
         var responseSource = AllocateResponseSource(transactionId);
         Send(Opcode.FinishCommit, new FinishCommitRequest<TProcessor>(transactionId, processor));
         var response = (FinishCommitResponse<R>)await GetResponse(responseSource);
         return response.Result;
      }

      private object AllocateResponseSource(int transactionId)
      {
         var taskCompletionSource = new TaskCompletionSource<object>();
         responseSourceByTransactionId.AddOrUpdate(transactionId, taskCompletionSource, (a, b) => taskCompletionSource);
         return taskCompletionSource;
      }

      private async Task<object> GetResponse(object o)
      {
         var taskCompletionSource = (TaskCompletionSource<object>)o;
         return await taskCompletionSource.Task;
      }

      private int GetNextTransactionId() { return Interlocked.Increment(ref commitIdCounter); }

      private void Send<TPayload>(Opcode opcode, TPayload payload)
         where TPayload : IPortableObject
      {
         using (var ms = new MemoryStream()) {
            ms.Position = 4;
            using (var writer = new BinaryWriter(ms)) {
               writer.Write((uint)opcode);
               serializer.Serialize(writer, payload);
            }
            ms.Position = 0;
            using (var writer = new BinaryWriter(ms)) {
               writer.Write((uint)ms.Length);
            }
            session.FrameWriter.WriteRaw(ms);
         }
      }
   }
}