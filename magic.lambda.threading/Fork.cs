/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Threading.Tasks;
using System.Collections.Generic;
using magic.node;
using magic.signals.contracts;

namespace magic.lambda.threading
{
    /// <summary>
    /// [fork] slot, allowing you to create and start a new thread.
    /// </summary>
    [Slot(Name = "fork")]
    public class Fork : ISlot, ISlotAsync
    {
        readonly ThreadRunner _runner;

        /// <summary>
        /// CTOR for slot.
        /// </summary>
        /// <param name="runner">Dependency injected implementation</param>
        public Fork(ThreadRunner runner)
        {
            _runner = runner;
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var task = _runner.Run(input);

            // Pushing task unto stack, if needed.
            PushTaskToStack(signaler, input, task);
        }

        /// <summary>
        /// Implementation of signal
        /// </summary>
        /// <param name="signaler">Signaler used to signal</param>
        /// <param name="input">Parameters passed from signaler</param>
        /// <returns>An awaiatble task.</returns>
        public Task SignalAsync(ISignaler signaler, Node input)
        {
            var task = _runner.Run(input);

            // Pushing task unto stack, if needed.
            PushTaskToStack(signaler, input, task);

            // Notice, we do NOT wait for task to finish its job.
            return Task.CompletedTask;
        }

        #region [ -- Private helper methods -- ]

        /*
         * Adds task to awaitable list of tasks if we're inside of a [join] invocation.
         */
        void PushTaskToStack(ISignaler signaler, Node input, Task task)
        {
            // Checking if parent node is [join] at which point we push task unto stack object.
            if (input.Parent?.Name == "join")
                signaler.Peek<List<Task>>(".magic.lambda.threading.join").Add(task);
        }

        #endregion
    }
}
