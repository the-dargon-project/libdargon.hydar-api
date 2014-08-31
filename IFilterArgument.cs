
namespace Dargon.Distributed
{
   public interface IFilterArgument<out V, out TProjection>
   {
      V Value { get; }
      TProjection Projection { get; }
   }
}
