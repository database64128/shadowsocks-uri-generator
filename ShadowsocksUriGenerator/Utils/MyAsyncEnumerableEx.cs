using System.Collections.Generic;
using System.Linq;

namespace ShadowsocksUriGenerator.Utils
{
    /// <summary>
    /// Workaround for https://github.com/dotnet/reactive/issues/1284.
    /// See also https://github.com/dotnet/reactive/blob/main/Ix.NET/Source/System.Interactive.Async/System/Linq/Operators/Merge.cs.
    /// </summary>
    public static class MyAsyncEnumerableEx
    {
        /// <inheritdoc cref="AsyncEnumerableEx.Merge{TSource}(IAsyncEnumerable{TSource}[])"/>
        public static IAsyncEnumerable<TSource> ConcurrentMerge<TSource>(params IAsyncEnumerable<TSource>[] sources)
            => AsyncEnumerableEx.Merge(sources);

        /// <inheritdoc cref="AsyncEnumerableEx.Merge{TSource}(IEnumerable{IAsyncEnumerable{TSource}})"/>
        public static IAsyncEnumerable<TSource> ConcurrentMerge<TSource>(this IEnumerable<IAsyncEnumerable<TSource>> sources)
            => ConcurrentMerge(sources.ToArray());

        /// <inheritdoc cref="AsyncEnumerableEx.Merge{TSource}(IAsyncEnumerable{IAsyncEnumerable{TSource}})"/>
        public static IAsyncEnumerable<TSource> ConcurrentMerge<TSource>(this IAsyncEnumerable<IAsyncEnumerable<TSource>> sources)
            => ConcurrentMerge(sources.ToEnumerable().ToArray());
    }
}
