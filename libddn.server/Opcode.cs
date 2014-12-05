namespace Dargon.Distributed.Server {
   public enum Opcode : uint {
      PrepareCommit = 1,
      CancelCommit = 2,
      FinishCommit = 4
   }
}
