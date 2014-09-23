using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using ItzWarty;

namespace Dargon.Distributed.Server
{
   public class LocalPuppeteerServer<K, V>
   {
      private const int kListenerBacklogSize = 32;
      
      private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      private readonly CancellationToken cancellationToken;
      private readonly ICachePuppeteer<K, V> puppeteer;
      private readonly DistributedConfiguration configuration;
      private readonly Thread listenerThread;
      private readonly Thread processorThread;
      private readonly PofContext pofContext;

      public LocalPuppeteerServer(ICachePuppeteer<K, V> puppeteer, DistributedConfiguration configuration)
      {
         this.cancellationToken = cancellationTokenSource.Token;
         this.puppeteer = puppeteer;
         this.configuration = configuration;
         this.pofContext = configuration.PofContext;
         this.listenerThread = new Thread(ListenerThreadStart).With(t => { t.IsBackground = true; }).With(t => t.Start());
         this.processorThread = new Thread(ProcessorThreadStart).With(t => { t.IsBackground = false; }).With(t => t.Start());
      }

      private void ListenerThreadStart()
      {
         var taskFactory = new TaskFactory<Socket>();
         while (!cancellationToken.IsCancellationRequested) {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, configuration.Port));
            listener.Listen(kListenerBacklogSize);
            var task = taskFactory.FromAsync(listener.BeginAccept, listener.EndAccept, new object());
            task.Wait(cancellationToken);
            if (!task.IsCanceled) {
               var node = task.Result;
               HandleNodeConnected(node);
            }
         }
      }

      private void HandleNodeConnected(Socket nodeSocket)
      {
         var session = new NodeSession(nodeSocket, NodeRole.Puppeteer);
         if (session.RemoteRole == NodeRole.Puppet) {
            this.puppeteer.RegisterPuppet(new RemoteCachePuppet<K, V>(configuration, session));
         }
      }

      private void ProcessorThreadStart()
      {
         while (!cancellationToken.IsCancellationRequested) {

         }
      }
   }
}