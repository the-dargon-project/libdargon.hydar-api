namespace Dargon.Distributed
{
   public interface ICacheProjector<TKey, TValue, TProjection>
   {
      TProjection Project(IEntry<TKey, TValue> entry);
   }
}