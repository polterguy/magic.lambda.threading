/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Threading.Tasks;
using sys = System.Threading;
using System.Collections.Concurrent;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.threading
{
    /// <summary>
    /// [semaphore] slot, allowing you to create a semaphore,
    /// only allowing one caller entry into some lambda object at the same time.
    /// </summary>
    [Slot(Name = "semaphore")]
    public class Semaphore : ISlot, ISlotAsync
    {
        static readonly ConcurrentDictionary<string, sys.SemaphoreSlim> _semaphores =
            new ConcurrentDictionary<string, sys.SemaphoreSlim>();

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var key = GetKey(input);

            var semaphore = _semaphores.GetOrAdd(key, (name) =>
            {
                return new sys.SemaphoreSlim(1);
            });
            semaphore.Wait();
            try
            {
                signaler.Signal("eval", input);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaiatble task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var key = GetKey(input);

            var semaphore = _semaphores.GetOrAdd(key, (name) =>
            {
                return new sys.SemaphoreSlim(1);
            });
            await semaphore.WaitAsync();
            try
            {
                await signaler.SignalAsync("eval", input);
            }
            finally
            {
                semaphore.Release();
            }
        }

        #region [ -- Private helper methods -- ]

        string GetKey(Node input)
        {
            return input.GetEx<string>() ??
                throw new HyperlambdaException("A semaphore must have a value, used to uniquely identity your sempahore");
        }

        #endregion
    }
}
