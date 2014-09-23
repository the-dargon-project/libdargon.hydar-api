using Dargon.PortableObjects;

namespace Dargon.Distributed.Server
{
   public class PrepareCommitResponse : IPortableObject
   {
      private int transactionId;
      private bool okay;

      public PrepareCommitResponse() { }

      public PrepareCommitResponse(int transactionId, bool okay)
      {
         this.transactionId = transactionId;
         this.okay = okay;
      }

      public int TransactionId { get { return transactionId; } }
      public bool Okay { get { return okay; } }

      public void Serialize(IPofWriter writer)
      {
         writer.WriteS32(0, transactionId);
         writer.WriteBoolean(1, okay);
      }

      public void Deserialize(IPofReader reader)
      {
         transactionId = reader.ReadS32(0);
         okay = reader.ReadBoolean(1);
      }
   }
   public class PrepareCommitRequest<K> : IPortableObject
   {
      private int transactionId;
      private K key;

      public PrepareCommitRequest() { } 
      public PrepareCommitRequest(int transactionId, K key)
      {
         this.transactionId = transactionId;
         this.key = key;
      }

      public int TransactionId { get { return transactionId; } }
      public K Key { get { return key; } }

      public void Serialize(IPofWriter writer)
      {
         writer.WriteS32(0, transactionId);
         writer.WriteObject(1, key);
      }

      public void Deserialize(IPofReader reader)
      {
         transactionId = reader.ReadS32(0);
         key = reader.ReadObject<K>(1);
      }
   }
}