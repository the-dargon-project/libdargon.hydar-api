using System.Collections.Generic;

namespace Dargon.Distributed
{
    public interface IEntryProcessor<K, V, R>
    {
       R Process(IEntry<K, V> entry);
    }
}
