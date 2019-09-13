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

        public static Fallible<TResult> Select<TSource, TResult>(this Fallible<TSource> source, Func<TSource, Fallible<TResult>> projection)
        {
            return source.IsSuccess ? projection(source.Value) : Fallible<TResult>.Fail(source.Exception);
        }

        public static async Task<Fallible<TResult>> SelectAsync<TSource, TResult>(this Fallible<TSource> source, Func<TSource, Task<TResult>> projection)
        {
            return source.IsSuccess ? await OperationAsync(() => projection(source.Value)).ConfigureAwait(false) : Fallible<TResult>.Fail(source.Exception);
        }

        public static async Task<Fallible<TResult>> SelectAsync<TSource, TResult>(this Fallible<TSource> source, Func<TSource, Task<Fallible<TResult>>> projection)
        {
            return source.IsSuccess ? await projection(source.Value).ConfigureAwait(false) : Fallible<TResult>.Fail(source.Exception);
        }

        public static void Do<T>(this Fallible<T> source, Action<T> onSuccess, Action<Exception> onFail)
        {
            if (source.IsSuccess)
            {
                onSuccess(source.Value);
            }
            else
            {
                onFail(source.Exception);
            }
        }

        public static Fallible<T> OnSuccess<T>(this Fallible<T> source, Action<T> action)
        {
            if (source.IsSuccess)
            {
                action(source.Value);
            }

            return source;
        }

        public static async Task<Fallible<T>> OnSuccessAsync<T>(this Fallible<T> source, Func<T, Task> action)
        {
            if (source.IsSuccess)
            {
                await action(source.Value).ConfigureAwait(false);
            }

            return source;
        }

        public static Fallible<T> OnFailure<T>(this Fallible<T> source, Action<Exception> action)
        {
            if (source.IsFailure)
            {
                action(source.Exception);
            }

            return source;
        }

        public static async Task<Fallible<T>> OnFailureAsync<T>(this Fallible<T> source, Func<Exception, Task> action)
        {
            if (source.IsFailure)
            {
                await action(source.Exception).ConfigureAwait(false);
            }

            return source;
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

        public bool IsFailure => !IsSuccess;

        public T Value { get; }

        public Exception Exception { get; }
    }
}
