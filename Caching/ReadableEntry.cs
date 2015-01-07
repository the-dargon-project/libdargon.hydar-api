namespace Dargon.Hydar.Caching {
   public interface ReadableEntry<K, V> {
      K Key { get; }
      V Value { get; }
      bool IsPresent { get; }
   }
}