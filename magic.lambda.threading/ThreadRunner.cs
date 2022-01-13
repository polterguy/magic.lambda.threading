/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using magic.node;
using magic.signals.contracts;

namespace magic.lambda.threading
{
    /// <summary>
    /// Helper class to run threads.
    /// </summary>
    public class ThreadRunner
    {
        readonly ISignaler _signaler;

        /// <summary>
        /// Constructor creating new instance of type.
        /// </summary>
        /// <param name="signaler">Dependency injected signals implementation.</param>
        public ThreadRunner(ISignaler signaler)
        {
            _signaler = signaler;
        }

        /// <summary>
        /// Executes the specified lambda node on a new thread, fire and forget style.
        /// </summary>
        /// <param name="input">Node being lambda object to execute.</param>
        public Task Run(Node input)
        {
            if (input.Parent?.Name == "join")
            {
                /*
                 * Notice, fire && forget invocation of async method.
                 *
                 * In this version we'll need to Clone the node, for then as the thread is
                 * done executing, make sure we copy its value and children into the original node.
                 * This is done internally within [eval] though, as as long as apply the correct
                 * lambda execution object to be executed after SignalAsync is done doing its job.
                 */
                var clone = input.Clone();
                return _signaler.SignalAsync("eval", clone, () => 
                {
                    input.Value = clone.Value;
                    input.Clear();
                    input.AddRange(clone.Children.ToList());
                });
            }
            else
            {
                /*
                 * Notice, fire && forget invocation of async method.
                 *
                 * However, unless parent node is [join], we clone lambda object, to avoid messing with
                 * parent nodes, resulting in potential race conditions.
                 */
                return _signaler.SignalAsync("eval", input.Clone());
            }
        }
    }
}
