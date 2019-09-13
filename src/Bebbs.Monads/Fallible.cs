using System;
using System.Threading.Tasks;

namespace Bebbs.Monads
{
    public static class Fallible
    {
        public static Fallible<T> Operation<T>(Func<T> operation)
        {
            try
            {
                return Fallible<T>.Success(operation());
            }
            catch (Exception exception)
            {
                return Fallible<T>.Fail(exception);
            }
        }

        public static async Task<Fallible<T>> OperationAsync<T>(Func<Task<T>> operation)
        {
            try
            {
                return Fallible<T>.Success(await operation().ConfigureAwait(false));
            }
            catch (Exception exception)
            {
                return Fallible<T>.Fail(exception);
            }
        }

        public static Fallible<TResult> Select<TSource, TResult>(this Fallible<TSource> source, Func<TSource, TResult> projection)
        {
            return source.IsSuccess ? Operation(() => projection(source.Value)) : Fallible<TResult>.Fail(source.Exception);
        }

        public static async Task<Fallible<TResult>> SelectAsync<TSource, TResult>(this Fallible<TSource> source, Func<TSource, Task<TResult>> projection)
        {
            return source.IsSuccess ? await OperationAsync(() => projection(source.Value)).ConfigureAwait(false) : Fallible<TResult>.Fail(source.Exception);
        }

        public static T ValueOrThrow<T>(this Fallible<T> source)
        {
            if (source.IsSuccess)
            {
                return source.Value;
            }
            else
            {
                throw source.Exception;
            }
        }
    }

    public struct Fallible<T>
    {
        public static Fallible<T> Success(T result)
        {
            return new Fallible<T>(result);
        }

        public static Fallible<T> Fail(Exception exception)
        {
            return new Fallible<T>(exception);
        }

        public Fallible(T value)
        {
            IsSuccess = true;
            Value = value;
            Exception = null;
        }

        public Fallible(Exception exception)
        {
            IsSuccess = false;
            Value = default;
            Exception = exception;
        }

        public bool IsSuccess { get; }

        public bool IsFail => !IsSuccess;

        public T Value { get; }

        public Exception Exception { get; }
    }
}
