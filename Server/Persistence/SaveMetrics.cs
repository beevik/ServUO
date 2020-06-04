using System;
using System.Diagnostics;
using System.Threading;

namespace Server
{
    public sealed class SaveMetrics : IDisposable
    {
        private const string PerformanceCategoryName = "ServUO";
        private const string PerformanceCategoryDesc = "Performance counters for ServUO";

        private long numberOfWorldSaves;

        private long itemsPerSecond;
        private long mobilesPerSecond;
        private long dataPerSecond;

        private long serializedBytesPerSecond;
        private long writtenBytesPerSecond;

        public SaveMetrics()
        {
            if (!PerformanceCounterCategory.Exists(PerformanceCategoryName))
            {
                CounterCreationDataCollection counters = new CounterCreationDataCollection();

                counters.Add(new CounterCreationData(
                    "Save - Count",
                    "Number of world saves.",
                    PerformanceCounterType.NumberOfItems32));

                counters.Add(new CounterCreationData(
                    "Save - Items/sec",
                    "Number of items saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Mobiles/sec",
                    "Number of mobiles saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Customs/sec",
                    "Number of cores saved per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Serialized bytes/sec",
                    "Amount of world-save bytes serialized per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                counters.Add(new CounterCreationData(
                    "Save - Written bytes/sec",
                    "Amount of world-save bytes written to disk per second.",
                    PerformanceCounterType.RateOfCountsPerSecond32));

                if (!Core.Unix)
                {
                    try
                    {
                        PerformanceCounterCategory.Create(PerformanceCategoryName, PerformanceCategoryDesc, PerformanceCounterCategoryType.SingleInstance, counters);
                    }
                    catch(Exception ex)
                    {
                        if (Core.Debug)
                            Console.WriteLine("Metrics: Metrics enabled. Performance counters creation requires ServUO to be run as Administrator once!");

                        Server.Diagnostics.ExceptionLogging.LogException(ex);
                    }
                }
                else
                {
                    Utility.PushColor(ConsoleColor.Yellow);
                    Console.WriteLine("WARNING: You've enabled SaveMetrics. This is currently not supported on Unix based operating systems. Please disable this option to hide this message.");
                    Utility.PopColor();
                }
            }

            // increment number of world saves
            Interlocked.Increment(ref this.numberOfWorldSaves);
        }

        public void OnItemSaved(int numberOfBytes)
        {
            Interlocked.Increment(ref this.itemsPerSecond);

            Interlocked.Add(ref this.serializedBytesPerSecond, (long)numberOfBytes);
        }

        public void OnMobileSaved(int numberOfBytes)
        {
            Interlocked.Increment(ref this.mobilesPerSecond);

            Interlocked.Add(ref this.serializedBytesPerSecond, (long)numberOfBytes);
        }

        public void OnGuildSaved(int numberOfBytes)
        {
            Interlocked.Add(ref this.serializedBytesPerSecond, (long)numberOfBytes);
        }

        public void OnDataSaved(int numberOfBytes)
        {
            Interlocked.Increment(ref this.dataPerSecond);

            Interlocked.Add(ref this.serializedBytesPerSecond, (long)numberOfBytes);
        }

        public void OnFileWritten(int numberOfBytes)
        {
            Interlocked.Add(ref this.writtenBytesPerSecond, (long)numberOfBytes);
        }

        private bool isDisposed;

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
            }
        }
    }
}
