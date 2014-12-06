namespace Dargon.Hydar {
   public interface ICacheProjector {}

   public interface ICacheProjector<TKey, TValue, TProjection> : ICacheProjector {
      TProjection Project(IEntry<TKey, TValue> entry);
   }
}
