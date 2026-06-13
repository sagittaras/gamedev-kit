using System;
using Sagittaras.GuardClauses;
using Sagittaras.GuardClauses.Extensions;

namespace Sagittaras.Timing
{
    /// <summary>
    ///     Timer tracking and managing a cooldown period, allowing controlled execution of time-dependent actions.
    /// </summary>
    public class Cooldown
    {
        /// <summary>
        ///     Creates a new instance of <see cref="Cooldown"/> with the specified duration.
        /// </summary>
        /// <param name="duration">The duration of the cooldown in seconds.</param>
        /// <param name="startsOnCooldown">Prevents the cooldown to be immediately ready.</param>
        public Cooldown(float duration, bool startsOnCooldown = false)
        {
            Guard.Against.LessThanZero(duration);
            
            Duration = duration;
            ElapsedTime = startsOnCooldown ? 0 : duration;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Cooldown"/> configured by <see cref="TimeSpan"/> instance.
        /// </summary>
        /// <param name="duration">Instance of <see cref="TimeSpan"/> configuring the duration of the cooldown.</param>
        /// <param name="startsOnCooldown">Prevents the cooldown to be immediately ready.</param>
        public Cooldown(TimeSpan duration, bool startsOnCooldown = false) : this((float)duration.TotalSeconds, startsOnCooldown)
        {
        }
        
        /// <summary>
        ///     Duration of the cooldown in seconds.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        ///     Currently remaining time of the cooldown in seconds.
        /// </summary>
        public float Remaining => Math.Max(0, Duration - ElapsedTime);

        /// <summary>
        ///     Indicates whether the cooldown is ready to be used.
        /// </summary>
        public bool IsReady => Remaining <= 0;
        
        /// <summary>
        ///     Watches the accumulation of time between <see cref="Update"/> calls.
        /// </summary>
        private float ElapsedTime { get; set; }

        /// <summary>
        ///     Updates the accumulation towards the cooldown duration.
        /// </summary>
        /// <param name="deltaTime">Delta by which the accumulation of time is increased.</param>
        public void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;
        }
        
        /// <summary>
        ///     Resets the accumulated time marking the <see cref="Cooldown"/> as not ready.
        /// </summary>
        public void Reset()
        {
            ElapsedTime = 0;
        }
    }
}