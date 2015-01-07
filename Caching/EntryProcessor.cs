namespace Dargon.Hydar.Caching {
   public interface EntryProcessor<K, V, R> {
      R Process(ManageableEntry<K, V> entry);
   }
}
