
namespace Dargon.Distributed
{
   public interface IEntry<TKey, TValue>
   {
      TKey Key { get; }
      TValue Value { get; set; }
      bool IsPresent { get; }
      bool IsDirty { get; }
      void Remove();
      void FlagAsDirty();
   }
}