using System;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Timing
{
    /// <summary>
    ///     Watches the specified interval duration during each call of <see cref="TryTick"/>.
    /// </summary>
    /// <remarks>
    ///     Timer is accumulating specified delta time during each call of <see cref="TryTick"/>,
    ///     once the accumulated time reaches the specified interval duration, the method returns true.
    /// </remarks>
    public class IntervalTimer
    {
        /// <summary>
        ///     Creates a new timer configured by the number of seconds.
        /// </summary>
        /// <param name="interval">Length of a single interval in the number of seconds.</param>
        /// <param name="startImmediately">Indicates whether the timer should at first <see cref="TryTick"/> return true.</param>
        public IntervalTimer(float interval, bool startImmediately = false)
        {
            Guard.Against.LessThanZero(interval);
            
            ElapsedTime = startImmediately ? interval : 0;
            Interval = interval;
        }

        /// <summary>
        ///     Creates a new timer configured by instance of <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="interval">Instance of <see cref="TimeSpan"/> configuring the length of a single interval.</param>
        /// <param name="startImmediately">Indicates whether the timer should at first <see cref="TryTick"/> return true.</param>
        public IntervalTimer(TimeSpan interval, bool startImmediately = false) : this((float)interval.TotalSeconds, startImmediately)
        {
        }
        
        /// <summary>
        ///     Interval of the timer in seconds.
        /// </summary>
        public float Interval { get; }

        /// <summary>
        ///     Remaining time until the next successful interval tick.
        /// </summary>
        public float Remaining => Math.Max(0, Interval - ElapsedTime);

        /// <summary>
        ///     Tracks the amount of time elapsed since the last interval tick.
        /// </summary>
        private float ElapsedTime { get; set; }

        /// <summary>
        ///     Increments the accumulated time by the specified delta.
        /// </summary>
        /// <remarks>
        ///     Once the accumulated time reaches the <see cref="Interval"/>, it is being reset.
        /// </remarks>
        /// <param name="deltaTime">Delta by which the timer is updated.</param>
        /// <returns>True when accumulated time reaches the <see cref="Interval"/>. Otherwise, false.</returns>
        public bool TryTick(float deltaTime)
        {
            ElapsedTime += deltaTime;
            if (ElapsedTime < Interval)
            {
                return false;
            }

            ElapsedTime -= Interval;
            return true;
        }
    }
}