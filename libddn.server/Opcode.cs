using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Distributed.Server
{
   public enum Opcode : uint
   {
      PrepareCommit     = 1,
      CancelCommit      = 2,
      FinishCommit      = 4
   }
}
