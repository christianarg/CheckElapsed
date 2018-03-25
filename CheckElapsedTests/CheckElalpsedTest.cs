using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CheckElapsed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CheckElapsedTests
{
    [TestClass]
    public class CheckElalpsedTest
    {
        [TestCleanup]
        public void CleanUp()
        {
            ElapsedChecker.ElapsedDictionary = new ConcurrentDictionary<string, ElapsedEntry>();
        }

        [TestMethod]
        public void SendMailOnceAminuteSimple()
        {
            // ARRANGE
            var someService = new SomeService();
            someService.SendMailEvery = TimeSpan.FromMinutes(1);
            // ACT
            someService.SendEmailIfSomethingGoneWrong();
            // ASSERT
            Assert.AreEqual(1, someService.EmailsSent);
        }

        [TestMethod]
        public void SendMailOnceAminuteMultiple()
        {
            // ARRANGE
            var someService = new SomeService();
            someService.SendMailEvery = TimeSpan.FromMinutes(1);
            // ACT
            ExecuteSendmailNTimes(someService, times: 100);
            // ASSERT
            Assert.AreEqual(1, someService.EmailsSent);
        }

        [TestMethod]
        public void SendMailOnce10SecondsAndThenWaitForElapsed()
        {
            // ARRANGE
            var someService = new SomeService();
            someService.SendMailEvery = TimeSpan.FromSeconds(10);
            // ACT
            ExecuteSendmailNTimes(someService, times: 10);

            Thread.Sleep(TimeSpan.FromSeconds(10));
            someService.SendEmailIfSomethingGoneWrong();

            // ASSERT
            Assert.AreEqual(2, someService.EmailsSent);
        }

        [TestMethod]
        public void SendMailOnce10SecondsAndThenWaitForElapsedMultiple()
        {
            // ARRANGE
            var someService = new SomeService();
            someService.SendMailEvery = TimeSpan.FromSeconds(10);
            // ACT
            ExecuteSendmailNTimes(someService, times: 10);
            Thread.Sleep(TimeSpan.FromSeconds(10));

            someService.SendEmailIfSomethingGoneWrong();
            someService.SendEmailIfSomethingGoneWrong();

            // ASSERT
            Assert.AreEqual(2, someService.EmailsSent);
        }

        [TestMethod]
        public void SendMailOnceAminuteMultipleParallel()
        {
            // ARRANGE
            var someService = new SomeService();
            someService.SendMailEvery = TimeSpan.FromMinutes(1);
            // ACT
            ExecuteSendMailNTimesParallel(someService, times:10);

            // ASSERT
            Assert.AreEqual(1, someService.EmailsSent);
        }

        [TestMethod]
        public void SendMailOnce10SecondsAndThenWaitForElapsedMultipleParallel()
        {
            // ARRANGE
            var someService = new SomeService();
            someService.SendMailEvery = TimeSpan.FromSeconds(10);
            // ACT
            ExecuteSendMailNTimesParallel(someService, times: 10);
            Thread.Sleep(TimeSpan.FromSeconds(10));

            ExecuteSendMailNTimesParallel(someService, times: 4);

            // ASSERT
            Assert.AreEqual(2, someService.EmailsSent);
        }


        private static void ExecuteSendMailNTimesParallel(SomeService someService, int times)
        {
            Parallel.For(0, times, new ParallelOptions { MaxDegreeOfParallelism = 10 }, (i) =>
            {
                someService.SendEmailIfSomethingGoneWrong();
            });
        }

        private static void ExecuteSendmailNTimes(SomeService someService, int times)
        {
            for (int i = 0; i < times; i++)
            {
                someService.SendEmailIfSomethingGoneWrong();
            }
        }
    }

    public class SomeService
    {
        public bool SomethingGoneWrong { get; set; } = true;

        public int EmailsSent { get; set; }

        public TimeSpan SendMailEvery { get; set; }

        public void SendEmailIfSomethingGoneWrong()
        {
            if (SomethingGoneWrong)
            {
                SendEmailOnlyOnceEvery(SendMailEvery);
            }
        }

        public void SendEmailOnlyOnceEvery(TimeSpan timespan)
        {
            if (ElapsedChecker.CheckElapsed(nameof(SomeService), timespan))
            {
                SendEmail();
            }
        }

        private void SendEmail()
        {
            EmailsSent++;
        }
    }
}
