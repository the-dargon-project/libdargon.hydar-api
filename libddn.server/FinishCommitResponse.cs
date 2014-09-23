using Dargon.PortableObjects;

namespace Dargon.Distributed.Server
{
   public class FinishCommitResponse<R> : IPortableObject
   {
      private R result;

      public FinishCommitResponse(R result) { this.result = result; }

      public R Result { get { return result; } }

      public void Serialize(IPofWriter writer) { writer.WriteObject(0, result); }
      public void Deserialize(IPofReader reader) { result = reader.ReadObject<R>(0); }
   }
}