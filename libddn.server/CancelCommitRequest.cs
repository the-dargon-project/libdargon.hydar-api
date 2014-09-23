using Dargon.PortableObjects;

namespace Dargon.Distributed.Server
{
   public class CancelCommitRequest : IPortableObject
   {
      private int transactionId;

      public CancelCommitRequest(int transactionId)
      {
         this.transactionId = transactionId;
      }

      public int TransactionId { get { return transactionId; }  }

      public void Serialize(IPofWriter writer) { writer.WriteS32(0, transactionId); }
      public void Deserialize(IPofReader reader) { transactionId = reader.ReadS32(0); }
   }
}