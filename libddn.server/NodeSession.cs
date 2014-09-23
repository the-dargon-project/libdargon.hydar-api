using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Dargon.Distributed.Server
{
   public class NodeSession
   {
      private readonly Socket socket;
      private readonly NodeRole localRole;
      private readonly NetworkStream ns;
      private readonly BinaryReader reader;
      private readonly BinaryWriter writer;
      private readonly Thread thread;
      private NodeRole remoteRole;
      
      private readonly FrameWriter frameWriter;

      public NodeSession(Socket socket, NodeRole localRole)
      {
         this.socket = socket;
         this.localRole = localRole;
         this.ns = new NetworkStream(socket, FileAccess.ReadWrite, true);
         this.reader = new BinaryReader(ns, Encoding.UTF8, true);
         this.writer = new BinaryWriter(ns, Encoding.UTF8, true);

         ProcessHandshake();

         this.frameWriter = new FrameWriter(writer);
//         this.frameReader = new FrameReader(reader);
      }

      private void ProcessHandshake()
      {
         writer.Write((Int32)this.localRole);
         this.remoteRole = (NodeRole)reader.ReadInt32();
      }

      public NodeRole LocalRole { get { return localRole; } }
      public NodeRole RemoteRole { get { return remoteRole; } }
      public FrameWriter FrameWriter { get { return frameWriter; } }
   }

   public class FrameWriter : IDisposable
   {
      private readonly BinaryWriter writer;

      public FrameWriter(BinaryWriter writer) {
         this.writer = writer;
      }

      public void WriteFrame(byte[] data)
      {
         writer.Write((uint)data.Length);
         writer.Write(data);
      }

      public void WriteFrame(MemoryStream ms)
      {
         var length = (int)ms.Length;
         writer.Write(unchecked((uint)length));
         writer.Write(ms.GetBuffer(), 0, length);
      }

      public void WriteRaw(byte[] data) { writer.Write(data); }

      public void WriteRaw(MemoryStream ms) { writer.Write(ms.GetBuffer(), 0, (int)ms.Length); }

      public void Dispose() { writer.Dispose();  }
   }
}