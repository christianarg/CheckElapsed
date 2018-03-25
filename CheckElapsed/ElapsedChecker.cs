using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckElapsed
{
    public class ElapsedChecker
    {
        internal static ConcurrentDictionary<string, ElapsedData> ElapsedDictionary { get; set; } = new ConcurrentDictionary<string, ElapsedData>();

        public static bool CheckElapsed(string key, TimeSpan timespan)
        {
            var now = DateTime.UtcNow;
            // Caso todavía no se ejecutó nunca
            if (!ElapsedDictionary.TryGetValue(key,out ElapsedData currentData))
            {
                Console.WriteLine("new");
                var elapsedData = new ElapsedData
                {
                    StartTime = now,
                    EndTime = now.Add(timespan)
                };
                ElapsedDictionary.TryAdd(key, elapsedData);
                return true;
            }
            else
            {
                // Caso ya se ejecutó, estamos esperando a que transcurra el tiempo
                if (currentData.EndTime < now)
                {
                    Console.WriteLine("elapsed");

                    var elapsedData = new ElapsedData
                    {
                        StartTime = now,
                        EndTime = now.Add(timespan)
                    };
                    ElapsedDictionary[key] = elapsedData;
                    return true;
                }
                else
                {
                    Console.WriteLine("not elapsed");

                    return false;
                }
            }
        }
    }

    public class ElapsedData
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
