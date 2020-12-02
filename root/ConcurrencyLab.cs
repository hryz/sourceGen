using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace net5
{
    public class ConcurrencyLab
    {
        record Model(int Id, bool Enabled);
        
        public static async Task Run()
        {
            var partitions = Environment.ProcessorCount;
            var capacity = 5;
            int hasher(Model m) => m.Id;

            int total = 0;

            var partitioner = new Partitioner<Model>(partitions, capacity, hasher);
            for (int i = 0; i < partitions; i++)
            {
                var partition = i;
                Task.Run(async () =>
                {
                    await foreach (var msg in partitioner.Outputs[partition].ReadAllAsync())
                    {
                        Interlocked.Increment(ref total);
                        Console.WriteLine($"partition: {partition}, msg: {msg}");
                        await Task.Delay(1000);
                    }
                    Console.WriteLine($"finished partition {partition}, total read {total}");
                });
            }

            for (int i = 0; i < 100; i++)
            {
                await partitioner.Push(new Model(i, i % 2 == 0), CancellationToken.None);
            }
            partitioner.Complete();

            Console.WriteLine("done");
            Console.Read();
        }
    }

    public interface IPartitioner<T>
    {
        ChannelReader<T>[] Outputs { get; }
        ValueTask Push(T item, CancellationToken token);
        void Complete();
    }

    public class Partitioner<T> : IPartitioner<T>
    {
        private readonly int _partitions;
        private readonly Func<T, int> _hasher;
        private readonly Channel<T>[] _channels;

        public Partitioner(int partitions, int capacity, Func<T,int> hasher)
        {
            _partitions = partitions;
            _hasher = hasher;
            var opt = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true,
                SingleReader = true,
            };
            
            _channels = new Channel<T>[partitions];
            Outputs = new ChannelReader<T>[partitions];
            
            for (int i = 0; i < partitions; i++)
            {
                _channels[i] = Channel.CreateBounded<T>(opt);
                Outputs[i] = _channels[i].Reader;
            }
        }

        public ChannelReader<T>[] Outputs { get; }

        public ValueTask Push(T item, CancellationToken token)
        {
            var partition = _hasher(item) % _partitions;
            return _channels[partition].Writer.WriteAsync(item, token);
        }

        public void Complete()
        {
            for (int i = 0; i < _partitions; i++)
            {
                _channels[i].Writer.Complete();
            }
        }
    }
}