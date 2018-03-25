using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckElapsed
{
    /// <summary>
    /// Helps to execute code that needs to be executed only every x time.
    /// 
    /// The main scenario is critical traces that send emails.
    /// 
    /// You might need to send an email if some http request to some critical service fails.
    /// 
    /// If the services fails, there are many chances that the same request would fail multiple times in a short timespan.
    /// 
    /// In that case, you don't want nor need to recibe more than 1 critical email to take action. Recieving 100 or 1k emails is
    /// anying.
    /// 
    /// This is what ElapsedChecker solves.
    /// 
    /// With the following code you will just recieve one mail every hour:
    /// 
    /// public void SendEmailOnlyOnceEveryHour()
    /// {
    ///     if (ElapsedChecker.CheckElapsed("someKey", TimeSpan.FromHours(1)))
    ///     {
    ///         SendEmail();
    ///     }
    /// }
    /// </summary>
    public class ElapsedChecker
    {
        internal static ConcurrentDictionary<string, ElapsedEntry> ElapsedDictionary { get; set; } = new ConcurrentDictionary<string, ElapsedEntry>();

        /// <summary>
        /// The first time this method is called returns TRUE and creates a new Entry for "key" to track the elapsed time
        /// needed for the ElapsedEntry time to expire.
        /// 
        /// The following calls this method will return FALSE if the key has now yet expired.
        /// 
        /// When the elapsed time for "key" has expired this method will return TRUE and re-create a new entry again
        /// for the expireTimeSpan.
        /// 
        /// The following calls this mehtod will again return FALSE, and the process will be repeated
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expireTimeSpan"></param>
        /// <returns></returns>
        public static bool CheckElapsed(string key, TimeSpan expireTimeSpan)
        {
            var now = DateTime.UtcNow;
            // Case: First execution
            if (!ElapsedDictionary.TryGetValue(key, out ElapsedEntry currentEntry))
            {
                return ElapsedDictionary.TryAdd(key, CreateElapsedEntry(expireTimeSpan, now));
            }
            else
            {
                // Case: Already executed, we're waiting for the entry to expire
                if (EntryHasExpired(now, currentEntry))
                {
                    ElapsedDictionary[key] = CreateElapsedEntry(expireTimeSpan, now);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static ElapsedEntry CreateElapsedEntry(TimeSpan timespan, DateTime now)
        {
            return new ElapsedEntry
            {
                StartTime = now,
                EndTime = now.Add(timespan)
            };
        }

        private static bool EntryHasExpired(DateTime now, ElapsedEntry currentEntry)
        {
            return currentEntry.EndTime < now;
        }
    }

    public class ElapsedEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
