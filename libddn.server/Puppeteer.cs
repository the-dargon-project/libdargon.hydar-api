using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dargon.PortableObjects;
using ItzWarty;

namespace Dargon.Distributed.Server
{
   public interface ICachePuppeteer<K, V>
   {
      void RegisterPuppet(ICachePuppet<K, V> puppet);
      
      V Get(K key);
      void Put(K key, V value);
      R Invoke<R, TProcessor>(K key, TProcessor entryProcessor) where TProcessor : IEntryProcessor<K, V, R>;
      IDictionary<K, R> InvokeAll<R>(ISet<K> keys, IEntryProcessor<K, V, R> entryProcessor);
   }

   public class UniformlyDistributedCachePuppeteer<K, V> : ICachePuppeteer<K, V>
   {
      private readonly UniformlyDistributedConfiguration configuration;
      private readonly List<ICachePuppet<K, V>> puppets = new List<ICachePuppet<K, V>>();
      private readonly List<Bucket> buckets = new List<Bucket>();
      private readonly LocalPuppeteerServer<K, V> server;

      public UniformlyDistributedCachePuppeteer(UniformlyDistributedConfiguration configuration) {
         this.configuration = configuration;
         InitializeKeyspaceBuckets();
         this.server = new LocalPuppeteerServer<K, V>(this, configuration);
      }

      private void InitializeKeyspaceBuckets()
      {
         const ulong hashspaceSize = 0x100000000UL; // (UL)2^32, size of uint range
         uint bucketCount = (uint)(hashspaceSize / (ulong)configuration.HashesPerBucket);
         for (uint i = 0; i < bucketCount; i++) {
            uint hashStart = configuration.HashesPerBucket * i;
            uint hashEnd = configuration.HashesPerBucket * (i + 1) - 1;
            if (i == bucketCount - 1) {
               hashEnd = uint.MaxValue;
            }
            var bucket = new Bucket(hashStart, hashEnd);
            buckets.Add(bucket);
         }
      }

      public void RegisterPuppet(ICachePuppet<K, V> puppet) { throw new NotImplementedException(); }

      public V Get(K key) { throw new NotImplementedException(); }
      public void Put(K key, V value) { throw new NotImplementedException(); }
      public R Invoke<R, TProcessor>(K key, TProcessor entryProcessor) where TProcessor : IEntryProcessor<K, V, R> { throw new NotImplementedException(); }
      public IDictionary<K, R> InvokeAll<R>(ISet<K> keys, IEntryProcessor<K, V, R> entryProcessor) { throw new NotImplementedException(); }

      private class Bucket
      {
         private readonly uint hashStart;
         private readonly uint hashEnd;

         public Bucket(uint hashStart, uint hashEnd)
         {
            this.hashStart = hashStart;
            this.hashEnd = hashEnd;
         }
      }
   }

   public class DistributedConfiguration
   {
      private readonly string name;
      private readonly string masterAddress;
      private readonly int port;
      private readonly PofContext pofContext; 

      public DistributedConfiguration(string name, string masterAddress, int port, PofContext pofContext)
      {
         this.name = name;
         this.masterAddress = masterAddress;
         this.port = port;
         this.pofContext = pofContext;
      }

      public string Name { get { return name; } }
      public string MasterAddress { get { return masterAddress; } }
      public int Port { get { return port; } }
      public PofContext PofContext { get { return pofContext; } }
   }

   public class UniformlyDistributedConfiguration : DistributedConfiguration
   {
      private readonly uint hashesPerBucket;
      private readonly int redundancyCount;

      public UniformlyDistributedConfiguration(int port, PofContext pofContext, int redundancyCount, uint hashesPerBucket = 0x00002000FU)
         : base(port, pofContext)
      {
         this.redundancyCount = redundancyCount;
         this.hashesPerBucket = hashesPerBucket;
      }

      public uint HashesPerBucket { get { return hashesPerBucket; } }
      public int RedundancyCount { get { return redundancyCount; } }
   }
}
