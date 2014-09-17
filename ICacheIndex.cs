
namespace Dargon.Distributed
{
   public interface ICacheIndex
   {
      string Name { get; }  
   }
   
   public interface ICacheIndex<TKey, TValue, TProjection> : ICacheIndex
   {
   }
}
