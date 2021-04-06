<div align="center">

![FusionCache logo](logo-128x128.png)

</div>

# :unicorn: A Gentle Introduction

FusionCache is an easy to use, high performance and robust **multi-level cache** with some advanced features.

It uses a memory cache (any impl of the standard `IMemoryCache` interface) as the **primary** backing store and optionally a distributed, 2nd level cache (any impl of the standard `IDistributedCache` interface) as a **secondary** backing store for better resilience and higher performance, for example in a multi-node scenario or to avoid the typical effects of a cold start (initial empty cache, maybe after a restart).

FusionCache includes some advanced features like **fail-safe**, concurrent **factory calls optimization** for the same cache key, fine grained **soft/hard timeouts**, **background factory completion**, customizable **extensive logging** and more (see below).

<div style="text-align:center;">

![FusionCache diagram](images/diagram.png)

</div>

## :twisted_rightwards_arrows: Cache Levels ([more](CacheLevels.md))

There are 2 caching levels, transparently handled by FusionCache for you.

These are:
- **Primary**: it's a memory cache, is always there and is used to have a very fast access to data in memory, with high data locality. You can give FusionCache any implementation of `IMemoryCache` or let FusionCache create one for you
- **Secondary**: is an *optional* distributed cache (any implementation of `IDistributedCache` will work) and, since it's not strictly necessary and it serves the purpose of **easing a cold start** or **coordinating with other nodes**, it is treated differently than the primary one. This means that any potential error happening on this level (remember the [fallacies of distributed computing](https://en.wikipedia.org/wiki/Fallacies_of_distributed_computing) ?) can be automatically handled by FusionCache to not impact the overall application, all while (optionally) logging any detail of it for further investigation

Everything is handled transparently for you.

You can read more [**here**](CacheLevels.md), or enjoy the complete [**step by step**](StepByStep.md) guide.
## :house_with_garden: Feels Like Home

FusionCache tries to feel like a native part of .NET by adhering to the naming conventions of the standard **memory** and **distributed** cache components:

|                               | Memory Cache                | Distributed Cache              | Fusion Cache              |
| ---:                          | :---:                       | :---:                          | :---:                     |
| **Cache Interface**           | `IMemoryCache`              | `IDistributedCache`            | `IFusionCache`            |
| **Cache Implementation**      | `MemoryCache`               | `[Various]Cache`               | `FusionCache`             |
| **Cache Options**             | `MemoryCacheOptions`        | `[Various]CacheOptions`        | `FusionCacheOptions`      |
| **Cache Entry Options**       | `MemoryCacheEntryOptions`   | `DistributedCacheEntryOptions` | `FusionCacheEntryOptions` |

If you've ever used one of those you'll feel at home with FusionCache.

## :rocket: Factory ([more](FactoryOptimization.md))

A factory is just a function that you specify when using the main `GetOrSet[Async]` method: basically it's the way you specify **how to get a value** when it is not in the cache or is expired.

Here's an example:

```csharp
var id = 42;

var product = cache.GetOrSet<Product>(
    $"product:{id}",
    _ => GetProductFromDb(id), // THIS IS THE FACTORY
    options => options.SetDuration(TimeSpan.FromMinutes(1))
);
```

FusionCache will search for the value in the cache (*memory* and *distributed*, if available) and, if nothing is there, will call the factory to obtain the value: it then saves it into the cache with the specified options, and returns it to the caller, all transparently.

Special care has been put into ensuring that only 1 factory per-key will be execute concurrently: you can read more [**here**](FactoryOptimization.md), or enjoy the complete [**step by step**](StepByStep.md) guide.

## :bomb: Fail-Safe ([more](FailSafe.md))

Sometimes things can go wrong, and calling a factory for an expired cache entry can lead to exceptions because the database or the network is temporarily down: normally in this case the exception will cause an error page in your website, a failure status code in your api or something like that.

By enabling the fail-safe mechanism you can simply tell FusionCache to ignore those errors and **temporarily use the expired cache entry**: your website or service will remain online, and your users would not notice anything.

You can read more [**here**](FailSafe.md), or enjoy the complete [**step by step**](StepByStep.md) guide.

## :stopwatch: Timeouts ([more](Timeouts.md))

Sometimes your data source (database, webservice, etc) is overloaded, the network is congested or something else is happening, and the end result is a **long wait** for a fresh piece of data.

Wouldn't it be nice if there could be a way to simply let FusionCache temporarily reuse an expired cache entry if the factory is taking too long?

Enter **soft/hard timeouts**.

You can specify a **soft timeout** to be used if there's an expired cache entry to use as a fallback, and a **hard timeout** to be used in any case, no matter what: in this last case an exception will be thrown and you will have to handle it yourself, but in some cases that would be more preferable than a very slow response.

In both cases it is possible (and enabled *by default*, so you don't have to do anything) to let the timed-out factory keep running in the background, and update the cached value as soon as it finishes, so you get the best of both worlds: a **fast response** and **fresh data** as soon as possible.

You can read more [**here**](Timeouts.md), or enjoy the complete [**step by step**](StepByStep.md) guide.

## :level_slider: Options ([more](Options.md))

There are 2 kinds of options:
 
 - `FusionCacheOptions`: cache-wide options, related to the entire FusionCache instance
 - `FusionCacheEntryOptions`: per-entry options, related to each call/entry

You can read more [**here**](Options.md), or enjoy the complete [**step by step**](StepByStep.md) guide.

## :joystick: Core Methods ([more](CoreMethods.md))

At a high level there are 5 core methods:

- `Set[Async]`
- `Remove[Async]`
- `TryGet[Async]`
- `GetOrDefault[Async]`
- `GetOrSet[Async]`

All of them work **on both the memory cache and the distributed cache** (if any) in a transparent way: you don't have to do anything extra for it to coordinate the 2 layers.

All of them are available in both a **sync** and an **async** version.

Finally, most of them have a set of ♻ overloads for a better ease of use.

You can read more [**here**](CoreMethods.md).

## :dizzy: Natively Sync and Async

Everything is natively available for both the **sync** and **async** programming models.

Any operation works seamlessly with any other, even if one is **sync** and the other is **async**: an example is multiple concurrent factory calls for the same cache key, some of them **sync** while others **async**, all coordinated togheter at the same time with no problems and a guarantee that only one will be executed at the same time.

## :page_with_curl: Logging
FusionCache can log extensively to help you pinpoint any possible problem in your production environment.

It uses the standard `ILogger<T>` interface and a structured logging approach so it fits well in the .NET ecosystem, allowing you to use any implementation you want that is compatible with it (Serilog, NLog, etc).

Since logging can have an impact on performance, a great care has been put into not having to pay that price if not necessary, and in paying as little as you want when you actually need it.

FusionCache lets you customize which `LogLevel` to use for any of the main events (see the `FusionCacheOptions` details [here](Options.md#fusioncacheoptions)) and this, combined with the standard ability to set a minimum `LogLevel` per category (see [here](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging#configure-logging)), can greatly reduce the volume of logged events to obtain less background noise while investigating a problem you may have.