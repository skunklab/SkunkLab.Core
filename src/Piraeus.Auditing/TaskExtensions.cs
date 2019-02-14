using System.Threading.Tasks;

namespace Piraeus.Auditing
{
    public static class TaskExtensions
    {
        public static void IgnoreException(this Task task)
        {
            if (task.IsCompleted)
            {
                var ignored = task.Exception;
            }
            else
            {
                IgnoreAsync(task);
            }

            async void IgnoreAsync(Task asyncTask)
            {
                try
                {
                    await asyncTask.ConfigureAwait(false);
                }
                catch
                {
                    // Ignored.
                }
            }
        }
    }
}
