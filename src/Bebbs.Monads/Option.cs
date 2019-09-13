﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bebbs.Monads
{
    public static class Option
    {
        public static Option<TResult> Select<TSource,TResult>(this Option<TSource> source, Func<TSource, TResult> projection)
        {
            return source.IsSome ? Option<TResult>.Some(projection(source.Value)) : Option<TResult>.None;
        }

        public static async Task<Option<TResult>> SelectAsync<TSource, TResult>(this Option<TSource> source, Func<TSource, Task<TResult>> projection)
        {
            return source.IsSome ? Option<TResult>.Some(await projection(source.Value).ConfigureAwait(false)) : Option<TResult>.None;
        }

        public static IEnumerable<T> Collect<T>(this IEnumerable<Option<T>> source)
        {
            return source.Where(option => option.IsSome).Select(option => option.Value);
        }

        public static T Coalesce<T>(this Option<T> source, Func<T> value)
        {
            return source.IsSome ? source.Value : value();
        }

        public static Option<TValue> TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key)
        {
            if (source.TryGetValue(key, out TValue value))
            {
                return Option<TValue>.Some(value);
            }
            else
            {
                return Option<TValue>.None;
            }
        }

        public static T ValueOrThrow<T>(this Option<T> source, Func<Exception> exceptionFactory)
        {
            if (source.IsSome)
            {
                return source.Value;
            }
            else
            {
                throw exceptionFactory();
            }
        }
    }

    public struct Option<T>
    {
        public static readonly Option<T> None = new Option<T>();

        public static Option<T> Some(T value)
        {
            return new Option<T>(true, value);
        }

        public Option(bool isSome, T value)
        {
            IsSome = isSome;
            Value = value;
        }

        public bool IsSome { get; }

        public bool IsNone => !IsSome;

        public T Value { get; }
    }
}
