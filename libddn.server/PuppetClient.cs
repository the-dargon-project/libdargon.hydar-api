using System;
using System.Threading;
using ItzWarty;

namespace Dargon.Distributed.Server
{
   public class PuppetClient<K, V>
   {
      private readonly LocalCachePuppet<K, V> puppet;
      private readonly DistributedConfiguration configuration;
      private readonly Thread requestThread;

      public PuppetClient(LocalCachePuppet<K, V> puppet, DistributedConfiguration configuration)
      {
         this.puppet = puppet;
         this.configuration = configuration;
//         this.requestThread = new Thread(RequestThreadStart).With(t => { t.IsBackground = true; }).With(t => t.Start());
//         this.processorThread = new Thread(ProcessorThreadStart).With(t => { t.IsBackground = false; }).With(t => t.Start());
      }

      private void RequestThreadStart()
      {

      }
   }
}