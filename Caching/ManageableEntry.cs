namespace Dargon.Hydar.Caching {
   public interface ManageableEntry<K, V> : ReadableEntry<K, V> {
      new V Value { get; set; }
      bool IsDirty { get; set; }
      void Remove();
   }
}
