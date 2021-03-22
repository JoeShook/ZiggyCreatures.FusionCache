#if NETSTANDARD2_1
using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace ZiggyCreatures.Caching.Fusion
{
    /// <summary>
    /// Implementation of <see cref="EventSource"/> for collecting cache hit and miss metrics.
    ///
    /// Reference https://docs.microsoft.com/en-us/dotnet/core/diagnostics/event-counter-perf#implement-an-eventsource
    /// </summary>
    [EventSource(Name = "Fusion_Cache")]
    public sealed partial class FusionCacheEventSource : EventSource
    {
        public const string DefaultCacheName = "Fusion";
        
        public static readonly FusionCacheEventSource Log = new FusionCacheEventSource();

        /// <summary>
        /// Possible idea to inject a name so when monitoring metrics we can set the cache name as a <see cref="Tag"/> 
        /// </summary>
        /// <param name="cacheName"></param>
        /// <returns></returns>
        public static FusionCacheEventSource LogByName(string cacheName)
        {
            Log._cacheName = cacheName;
            
            return Log;
        }

        private string _cacheName;
        private long _cacheHitAccumulator;
        private long _cacheMissAccumulator;
        private IncrementingPollingCounter _cacheHitPollingCounter;
        private IncrementingPollingCounter _cacheMissPollingCounter;

        /// <summary>
        /// Semantically meaningful in the context of a time series database such as InfluxDb 
        /// </summary>
        public static class Field
        {
            public const string CacheHits = "cache-hit-count";
            public const string CacheMisses = "cache-miss-count";
        }

        /// <summary>
        /// Semantically meaningful in the context of a time series database such as InfluxDb 
        /// </summary>
        public static class Tag
        {
            public const string CacheName = "cacheName";
        }

        private FusionCacheEventSource()
        {
            _cacheName = DefaultCacheName;
        }
        
        
        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            _cacheMissPollingCounter = CacheMissCounter(_cacheName);
            _cacheHitPollingCounter = CacheHitPollingCounter(_cacheName);
        }

        /// <summary>
        /// Accessor to accumulate a cache hit
        /// </summary>
        [NonEvent]
        public void CacheHit()
        {
            if (!IsEnabled())
            {
                return;
            }
            
            AccumulateCacheHit();
        }

        [NonEvent]
        private IncrementingPollingCounter CacheHitPollingCounter(string cacheName)
        {
            var counter = new IncrementingPollingCounter(
                Field.CacheHits,
                this,
                () => _cacheHitAccumulator)
            {
                DisplayName = $"{cacheName}-Cache Hits",
                DisplayRateTimeScale = TimeSpan.FromSeconds(5)
            };

            counter.AddMetadata(Tag.CacheName, cacheName);

            return counter;
        }

        /// <summary>
        /// Accessor to accumulate a cache miss
        /// </summary>
        [NonEvent]
        public void CacheMiss()
        {
            if (!IsEnabled())
            {
                return;
            }
            
            AccumulateCacheMiss();
        }

        [NonEvent]
        private IncrementingPollingCounter CacheMissCounter(string cacheName)
        {
            var counter = new IncrementingPollingCounter(
                Field.CacheMisses,
                this,
                () => _cacheMissAccumulator)
            {
                DisplayName = $"{cacheName}-Cache Misses",
                DisplayRateTimeScale = TimeSpan.FromSeconds(5)
            };

            counter.AddMetadata(Tag.CacheName, cacheName);

            return counter;
        }


        [Event(1, Level = EventLevel.Informational, Message = "Cache Hit")]
        private void AccumulateCacheHit()
        {
            Interlocked.Increment(ref _cacheHitAccumulator);
        }

        [Event(2, Level = EventLevel.Informational, Message = "Cache Miss")]
        private void AccumulateCacheMiss()
        {
            Interlocked.Increment(ref _cacheMissAccumulator);
        }
    }
}
#endif
