﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Xunit.Assert;

namespace VotingSystem.Tests
{
    public class CounterManagerTests
    {
        public const string CounterName = "Counter Name";
        public Counter _counter = new Counter { Name = CounterName, Count = 5 };

        [Fact]
        public void GetCounterStatistics_InclueCounterName()
        {
            var statistics = new CounterManager().GetCounterStatistics(_counter, 5);
            Equal(CounterName, statistics.Name);
        }

        [Fact]
        public void GetStatistics_IncludeCounterCount()
        {
            var statistics = new CounterManager().GetCounterStatistics(_counter, 5);
            Equal(5, statistics.Count);
        }

        [Theory]
        [InlineData(5, 10, 50)]
        [InlineData(1, 3, 33.33)]
        [InlineData(2, 8, 25)]
        [InlineData(2, 3, 66.67)]
        public void GetStatistics_ShowsPercentageUpToTwoDecimalBaseOnTotalCount(int count, int total, double expected)
        {
            _counter.Count = count;
            var statistics = new CounterManager().GetCounterStatistics(_counter, total);
            Equal(expected, statistics.Percent);
        }

        [Fact]
        public void ResolveExcess_DoesntAddExcessWhenAllCountersAreEqual()
        {
            var counter1 = new Counter { Percent = 33.33 };
            var counter2 = new Counter { Percent = 33.33 };
            var counter3 = new Counter { Percent = 33.33 };

            var counters = new List<Counter> { counter1, counter2, counter3 };
            new CounterManager().ResolveExcess(counters);

            Equal(33.33, counter1.Percent);
            Equal(33.33, counter2.Percent);
            Equal(33.33, counter3.Percent);
        }

        [Theory]
        [InlineData(66.66, 66.67, 33.33)]
        [InlineData(66.65, 66.67, 33.33)]
        [InlineData(66.66, 66.68, 33.32)]
        public void ResolveExcess_AddExcessToHigestCounter(double initial, double expected, double lowest)
        {
            var counter1 = new Counter { Percent = initial };
            var counter2 = new Counter { Percent = lowest };

            var counters = new List<Counter> { counter1, counter2 };
            new CounterManager().ResolveExcess(counters);

            Equal(expected, counter1.Percent);
            Equal(lowest, counter2.Percent);

            var counter3 = new Counter { Percent = initial };
            var counter4 = new Counter { Percent = lowest };
            counters = new List<Counter> { counter4, counter3 };
            new CounterManager().ResolveExcess(counters);

            Equal(expected, counter3.Percent);
            Equal(lowest, counter4.Percent);
        }

        [Theory]
        [InlineData(11.11, 11.12, 44.44)]
        [InlineData(11.10, 11.12, 44.44)]
        public void ResolveExcess_AddsExcessToLowestCounterWhenMoreThanOneHighestCounters(double initial, double expected, double highest)
        {
            var counter1 = new Counter { Percent = highest };
            var counter2 = new Counter { Percent = highest };
            var counter3 = new Counter { Percent = initial };
            var counters = new List<Counter> { counter1, counter2, counter3 };

            new CounterManager().ResolveExcess(counters);

            Equal(highest, counter1.Percent);
            Equal(highest, counter2.Percent);
            Equal(expected, counter3.Percent);
        }

        [Fact]
        public void ResolveExcess_DoesntAddExcessIfTotalPercentIs100()
        {
            var counter1 = new Counter { Count = 4, Percent = 80 };
            var counter2 = new Counter { Count = 1, Percent = 20 };
            var counters = new List<Counter> { counter1, counter1 };

            new CounterManager().ResolveExcess(counters);

            Equal(80, counter1.Percent);
            Equal(20, counter2.Percent);
        }
    }

    public class Counter
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public double Percent { get; set; }
    }

    public class CounterManager
    {
        public Counter GetCounterStatistics(Counter counter, int totalCount)
        {
            counter.Percent = RoundUp(counter.Count * 100.0 / totalCount);
            return counter;
        }

        public void ResolveExcess(List<Counter> counters)
        {
            var totalPercnet = counters.Sum(x => x.Percent);
            if (totalPercnet == 100) return;

            var excess = 100 - totalPercnet;

            var higestPercent = counters.Max(x => x.Percent);
            var higestCounters = counters.Where(x => x.Percent == higestPercent).ToList();
            if (higestCounters.Count == 1)
            {
                counters.First(x => x.Percent == higestPercent).Percent += excess;
            }
            else if (higestCounters.Count < counters.Count)
            {
                var lowestPercent = counters.Min(x => x.Percent);
                var lowestCounter = counters.First(x => x.Percent == lowestPercent);

                lowestCounter.Percent = RoundUp(lowestCounter.Percent + excess);
            }
        }

        private static double RoundUp(double num) => Math.Round(num, 2);
    }
}
