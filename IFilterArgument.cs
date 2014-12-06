namespace Dargon.Hydar {
   public interface IFilterArgument<out V, out TProjection> {
      V Value { get; }
      TProjection Projection { get; }
   }
}
