using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace net5
{
    record Odds(decimal Decimal, int American);
    
    record Line(decimal Probability, int ExtractionMethod);
    
    //already declared in DataLayer
    interface ICache
    {
        Line? this[int key] { get; set; }
    }

    //to declare
    interface ICacheWrapper : ICache
    {
        bool EnableTracing { get; set; }
        Dictionary<int, Line?> TouchedCacheEntries { get; }
    }

    //our top level pipeline incl. providers
    interface IPipeline
    {
        Odds? Calculate(int id);
    }

    //already implemented in DataLayer
    sealed class Cache : ICache
    {
        public Cache()
        {
            Console.WriteLine("cache created " + GetHashCode());
            _store = new Dictionary<int, Line?>();
            
            //imagine the cache is already full of data
            for (int i = 0; i < 1000; i++)
            {
                _store[i] = new Line(Probability: 0.001M * i, ExtractionMethod: i);
            }
        }

        private readonly Dictionary<int, Line?> _store;
        public Line? this[int key]
        {
            get => _store.ContainsKey(key) ? _store[key] : null;
            set => _store[key] = value;
        }
    }

    //to be implemented
    sealed class CacheWrapper : ICacheWrapper
    {
        private readonly ICache _inner;
        public CacheWrapper(ICache inner)
        {
            _inner = inner;
            TouchedCacheEntries = new Dictionary<int, Line?>();
            Console.WriteLine("cache wrapper created " + GetHashCode());
        }

        public Line? this[int key]
        {
            get
            {
                var result = _inner[key];
                if (!EnableTracing)
                    return result;

                TouchedCacheEntries[key] = result;
                return result;
            }
            set => _inner[key] = value;
        }

        public bool EnableTracing { get; set; }
        public Dictionary<int, Line?> TouchedCacheEntries { get; }
    }

    //already implemented (top layer pipeline)
    sealed class Pipeline : IPipeline
    {
        private readonly ICacheWrapper _cache;
        public Pipeline(ICacheWrapper cache)
        {
            _cache = cache;
            Console.WriteLine("pipeline created " + GetHashCode());
        }

        //calculation with cache access
        public Odds? Calculate(int id)
        {
            Console.WriteLine("triggering calculation of " + id);
            var line = _cache[id];
            if (line == null)
                return null;
            
            //logic to access 1 cache item
            if (id % 2 == 0)
            {
                return new Odds(1M / line.Probability, 0);
            }

            //logic to access 2 cache items
            var line2 = _cache[id + 1];
            if (line2 == null)
                return null;

            var probability = (line.Probability + line2.Probability) / 2;
            return new Odds(1M / probability, 0);

        }
    }

    //background process (change providers -> "cache loader")
    class Consumer
    {
        private readonly IPipeline _pipeline;
        public Consumer(IPipeline pipeline)
        {
            _pipeline = pipeline;
            Task.Run(ProcessIncomingMessages);
        }

        async Task ProcessIncomingMessages()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Consumer triggering calculation for " + i);
                var result = _pipeline.Calculate(i);
                await Task.Delay(1000);
            }
        }
    }
    
    //diagnostic controller with dump action
    sealed class Controller
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public Controller(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

        public (Odds? result, Dictionary<int, Line?> touchedItems) Calculate(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var wrapper = scope.ServiceProvider.GetRequiredService<ICacheWrapper>();
            var pipeline = scope.ServiceProvider.GetRequiredService<IPipeline>();
            Console.WriteLine("controller: got pipeline " + pipeline.GetHashCode());

            wrapper.EnableTracing = true;
            var result = pipeline.Calculate(id);
            return (result, wrapper.TouchedCacheEntries);
        }
    }

    public class DumpCacheDemo
    {
        public static async Task Run()
        {
            var srv = new ServiceCollection();
            srv.AddSingleton<ICache, Cache>();
            srv.AddScoped<ICacheWrapper, CacheWrapper>(); //wrapper - scoped
            srv.AddScoped<IPipeline, Pipeline>(); //pipeline and providers - scoped
            srv.AddSingleton<Consumer>();
            srv.AddTransient<Controller>();

            var prv = srv.BuildServiceProvider();

            var consumer = prv.GetRequiredService<Consumer>();
            //consumer starts working

            //cache dump request
            for (int i = 0; i < 3; i++)
            {
                var cont = prv.GetRequiredService<Controller>();
                var (res, touched) = cont.Calculate(100 + i);
                Console.WriteLine($"got a result {res}, dump count: {touched.Count}");
                await Task.Delay(2000);
            }

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}