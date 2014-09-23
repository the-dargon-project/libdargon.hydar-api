using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dargon.Distributed.Server
{
   public interface ICachePuppet<K, V>
   {
      Task<bool> TryBeginCommitAsync<R, TProcessor>(K[] keys, int transactionId, TProcessor processor) where TProcessor : IEntryProcessor<K, V, R>;
      Task<bool> CancelCommitAsync(int transactionId);
      Task<IDictionary<K, R>> FinishCommitAsync<R>(int transactionId);
   }
}