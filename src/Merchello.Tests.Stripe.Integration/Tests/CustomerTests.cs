using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Merchello.Plugin.Payments.Stripe.Services;
using Merchello.Plugin.Payments.Stripe.Models;
using NUnit.Framework;

namespace Merchello.Tests.Stripe.Integration.Tests
{
    [TestFixture]
    public class CustomerTests
    {
        protected StripeProcessorSettings Settings;
        [SetUp]
        public void Init()
        {
            Settings = new StripeProcessorSettings()
            {
                ApiKey = ConfigurationManager.AppSettings["stripeApiKey"]
            };
        }

        /// <summary>
        /// Test creating a customer
        /// </summary>
        [Test]
        public void Can_Create_Customer()
        {
            Assert.NotNull(Settings);
            var stripeApiService = new StripeApiService(Settings);
            Assert.NotNull(stripeApiService);
            Assert.NotNull(stripeApiService.Customer);
        }
    }
}
