using Dargon.PortableObjects;

namespace Dargon.Distributed.Server
{
   public class FinishCommitRequest<TProcessor> : IPortableObject
   {
      private int transactionId;
      private TProcessor processor;

      public FinishCommitRequest(int transactionId, TProcessor processor)
      {
         this.transactionId = transactionId;
         this.processor = processor;
      }

      public int TransactionId { get { return transactionId; } }
      public TProcessor Processor { get { return processor; } }

      public void Serialize(IPofWriter writer)
      {
         writer.WriteS32(0, transactionId);
         writer.WriteObject(1, processor);
      }

      public void Deserialize(IPofReader reader)
      {
         transactionId = reader.ReadS32(0);
         processor = reader.ReadObject<TProcessor>(1);
      }
   }
}