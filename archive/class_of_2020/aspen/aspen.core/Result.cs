using System;
using System.Threading.Tasks;
using Aspen.Core.Models;

namespace Aspen.Core
{
    //maybe rename to internalresult
    public struct Result<T>
    {
        public Result(T state)
        {
            IsSucccess = true;
            State = state;
            Error = null;
        }

        public bool IsFailure => !IsSucccess;
        public bool IsSucccess { get; }
        public T State { get; }
        public string Error { get; }

        private Result(bool IsSucccess, T state, string error)
        {
            this.IsSucccess = IsSucccess;
            State = state;
            Error = error;
        }

        public static Result<T> Success(T state) => new Result<T>(true, state, null);
        public static Result<T> Failure(string error) => new Result<T>(false, default(T), error);

    }

    public static class ResultHelper
    {
        public static Result<U> ValidateFunction<U, T>(this T state, Func<T, Result<U>> func)
        {
            return func(state);
        }
        public async static Task<Result<U>> ValidateFunction<U, T>(this T state, Func<T, Task<Result<U>>> func)
        {
            return await func(state);
        }

        public static Result<U> Apply<U, T>(this Result<T> result, Func<T, Result<U>> func) =>
            result.IsSucccess switch
            {
                true => func(result.State),
                false => Result<U>.Failure(result.Error)
            };

        public static async Task<Result<U>> ApplyAsync<U, T>(this Result<T> result, Func<T, Task<Result<U>>> func) =>
            result.IsSucccess switch
            {
                true => await func(result.State),
                false => Result<U>.Failure(result.Error)
            };

        public static async Task<Result<U>> ApplyAsync<U, T>(this Task<Result<T>> task, Func<T, Task<Result<U>>> func)
        {
            await task;
            if (task.Result.IsSucccess)
            {
                return await func(task.Result.State);
            }
            else
            {
                return Result<U>.Failure(task.Result.Error);
            }
        }

        public static async Task<ApiResult> ReturnApiResult<T>(this Task<Result<T>> task)
        {
            await task;
            if (task.Result.IsSucccess)
            {
                return ApiResult.Success(task.Result.State);
            }
            else
            {
                return ApiResult.Failed(task.Result.Error);
            }
        }

        public static async Task<ApiResult> ReturnEmptyApiResult<T>(this Task<Result<T>> task)
        {
            await task;
            if (task.Result.IsSucccess)
            {
                return ApiResult.Success(null);
            }
            else
            {
                return ApiResult.Failed(task.Result.Error);
            }
        }
    }
}