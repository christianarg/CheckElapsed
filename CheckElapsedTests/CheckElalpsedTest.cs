using System;
using System.Collections.Concurrent;
using System.Threading;
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
            for (int i = 0; i < 100; i++)
            {
                someService.SendEmailIfSomethingGoneWrong();
            }
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
            for (int i = 0; i < 10; i++)
            {
                someService.SendEmailIfSomethingGoneWrong();
            }
            
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
            Console.WriteLine("StartFor");
            for (int i = 0; i < 10; i++)
            {
                someService.SendEmailIfSomethingGoneWrong();
            }
            Console.WriteLine("EndFor");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            
            someService.SendEmailIfSomethingGoneWrong();
            someService.SendEmailIfSomethingGoneWrong();
            //for (int i = 0; i < 10; i++)
            //{
            //    someService.SendEmailIfSomethingGoneWrong();
            //}
            // ASSERT
            Assert.AreEqual(2, someService.EmailsSent);
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
