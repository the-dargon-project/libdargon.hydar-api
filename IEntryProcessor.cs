namespace Dargon.Hydar {
   public interface IEntryProcessor<K, V, R> {
      R Process(IEntry<K, V> entry);
   }
}
