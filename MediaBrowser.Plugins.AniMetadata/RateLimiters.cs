using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.AniMetadata
{
    internal class RateLimiters : IRateLimiters
    {
        public static readonly IRateLimiters Instance = new RateLimiters();

        private RateLimiters()
        {
            AniDb = new RateLimiter(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(5), new SemaphoreSlim(1, 1));
        }

        public IRateLimiter AniDb { get; }

        /// <summary>
        ///     The RateLimiter class attempts to regulate the rate at which an event occurs, by delaying
        ///     new occurances of the event.
        /// </summary>
        /// <remarks>
        ///     The <see cref="RateLimiter" /> will allow bursts of activity (down to a minimum occurance interval),
        ///     but attempts to maintain a minimum interval between occurances over a given time window.
        /// </remarks>
        private class RateLimiter : IRateLimiter
        {
            private readonly AsyncLock _lock;
            private readonly int _maxAllowedInWindow;
            private readonly TimeSpan _minimumInterval;
            private readonly TimeSpan _targetInterval;
            private readonly TimeSpan _timeWindowDuration;

            private readonly List<DateTime> _window;

            private DateTime _lastTake;

            /// <summary>
            ///     Creates a new instance of the <see cref="RateLimiter" /> class.
            /// </summary>
            /// <param name="minimumInterval">The minimum time between events.</param>
            /// <param name="targetInterval">The target average time between events.</param>
            /// <param name="timeWindow">The time span over which the average rate is calculated.</param>
            public RateLimiter(TimeSpan minimumInterval, TimeSpan targetInterval, TimeSpan timeWindow,
                SemaphoreSlim semaphore)
            {
                _window = new List<DateTime>();
                _lock = new AsyncLock();
                _minimumInterval = minimumInterval;
                _targetInterval = targetInterval;
                _timeWindowDuration = timeWindow;
                Semaphore = semaphore;

                _maxAllowedInWindow = (int)(timeWindow.Ticks / targetInterval.Ticks);

                _lastTake = DateTime.Now - minimumInterval;
            }

            /// <summary>
            ///     Attempts to trigger an event, waiting if needed.
            /// </summary>
            /// <returns>A task which completes when it is safe to proceed.</returns>
            public async Task TickAsync()
            {
                using (await _lock.LockAsync())
                {
                    var wait = CalculateWaitDuration();
                    if (wait.Ticks > 0)
                    {
                        await Task.Delay(wait);
                    }

                    var now = DateTime.Now;
                    _window.Add(now);
                    _lastTake = now;
                }
            }

            public SemaphoreSlim Semaphore { get; }

            private TimeSpan CalculateWaitDuration()
            {
                FlushExpiredRecords();
                if (_window.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                var now = DateTime.Now;
                var minWait = _lastTake + _minimumInterval - now;

                var load = (float)_window.Count / _maxAllowedInWindow;

                var waitTicks = minWait.Ticks + (_targetInterval.Ticks - minWait.Ticks) * load;
                return new TimeSpan((long)waitTicks);
            }

            private void FlushExpiredRecords()
            {
                var now = DateTime.Now;
                while (_window.Count > 0 && now - _window[0] > _timeWindowDuration)
                    _window.RemoveAt(0);
            }
        }
    }
}