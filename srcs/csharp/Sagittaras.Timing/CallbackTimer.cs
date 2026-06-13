using System;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Timing
{
    /// <summary>
    ///     Timer invoking a callback action at the rate of specified duration.
    /// </summary>
    public class CallbackTimer
    {
        /// <summary>
        ///     Callback to be invoked when the <see cref="_timer"/> reaches the duration.
        /// </summary>
        private readonly Action _callback;

        /// <summary>
        ///     Internal timer controlling the interval between each <see cref="_callback"/> call.
        /// </summary>
        private readonly IntervalTimer _timer;

        /// <summary>
        ///     Creates a new callback timer with a specified duration.
        /// </summary>
        /// <param name="duration">Duration in a number of seconds between each call of the callback action.</param>
        /// <param name="callback">Callback action to be invoked once the internal timer reaches the accumulation.</param>
        /// <param name="startImmediately">Indicates whether the first call of <see cref="TryUse"/> should be successful, or the accumulation of time is required from the beginning.</param>
        public CallbackTimer(float duration, Action callback, bool startImmediately = false)
        {
            Guard.Against.LessThanZero(duration);
            
            _callback = callback;
            _timer = new IntervalTimer(duration, startImmediately);
        }

        /// <summary>
        ///     Creates a new callback timer configured through <see cref="TimeSpan"/> instance.
        /// </summary>
        /// <param name="duration">Instance of <see cref="TimeSpan"/> configuring the rate of callback calls in time.</param>
        /// <param name="callback">Callback action to be invoked once the internal timer reaches the accumulation.</param>
        /// <param name="startImmediately">Indicates whether the first call of <see cref="TryUse"/> should be successful, or the accumulation of time is required from the beginning.</param>
        public CallbackTimer(TimeSpan duration, Action callback, bool startImmediately = false) : this((float)duration.TotalSeconds, callback, startImmediately)
        {
        }

        /// <summary>
        ///     Duration between each call of the configured callback.
        /// </summary>
        public float Duration => _timer.Interval;

        /// <summary>
        ///     Remaining time until the next callback invocation.
        /// </summary>
        public float Remaining => _timer.Remaining;

        /// <summary>
        ///     Updates the internal timer's accumulation and try to use the callback.
        /// </summary>
        /// <remarks>
        ///     Once the configured duration elapses, the callback is invoked.
        /// </remarks>
        /// <param name="deltaTime">Delta time to update internal timer.</param>
        public void TryUse(float deltaTime)
        {
            if (!_timer.TryTick(deltaTime))
            {
                return;
            }
            
            _callback.Invoke();
        }
    }
}