using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Distributed.Server
{
   public enum NodeRole : int
   {
      Puppeteer   = 1,
      Puppet      = 2,
      Client      = 4
   }
}
