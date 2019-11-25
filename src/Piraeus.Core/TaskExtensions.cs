using Piraeus.Core.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Piraeus.Core
{
    public static class TaskExtensions
    {
        public static void LogExceptions(this Task task)
        {
            task.ContinueWith(t =>
            {
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions)
                {
                    Trace.TraceError(exception.Message);
                    Console.WriteLine(exception.Message);
                }

            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        public static Task LogExceptions(this Task task, ILog log = null)
        {
            return task.ContinueWith(t =>
            {
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions)
                {
                    log?.LogErrorAsync(exception, exception.Message);
                }

            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        //public static void Ignore(this Task task)
        //{
        //    if (task.IsCompleted)
        //    {
        //        var ignored = task.Exception;
        //    }
        //    else
        //    {
        //        IgnoreAsync(task);
        //    }

        //    async void IgnoreAsync(Task asyncTask)
        //    {
        //        try
        //        {
        //            await asyncTask.ConfigureAwait(false);
        //        }
        //        catch
        //        {
        //            // Ignored.
        //        }
        //    }
        //}
    }
}
