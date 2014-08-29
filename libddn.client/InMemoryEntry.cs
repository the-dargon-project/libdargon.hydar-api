using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Distributed
{
   public class InMemoryEntry<K, V> : IEntry<K, V>
   {
      private K key;
      private V value;
      private bool isPresent = false;
      private bool isDirty = false;
      private bool isRemoved = false;

      public InMemoryEntry(K key, V value, bool isPresent)
      {
         this.key = key;
         this.value = value;
         this.isPresent = isPresent;
      }

      public K Key { get { return key; } }

      public V Value
      {
         get { return value; }
         set
         {
            this.value = value;
            isDirty = true;
         }
      }

      public bool IsPresent { get { return isPresent; } }
      public bool IsRemoved { get { return isRemoved; } }
      public bool IsDirty { get { return isDirty; } }
      public void Remove() { isRemoved = true; }

      public void FlagAsDirty() { this.isDirty = true; }
   }
}
