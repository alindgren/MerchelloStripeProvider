using System;
using Merchello.Plugin.Payments.Stripe.Models;

namespace Merchello.Plugin.Payments.Stripe.Gateways
{
    public class StripeGateway
    {
        private readonly StripeProcessorSettings _settings;

        /// <summary>
        /// The <see cref="CustomerGateway"/>.
        /// </summary>
        private Lazy<CustomerGateway> _customer;

        public StripeGateway(StripeProcessorSettings settings)
        {
            _settings = settings;
            Initialize();
        }

        /// <summary>
        /// Gets the customer gateway
        /// </summary>
        public CustomerGateway Customer
        {
            get { return _customer.Value; }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        private void Initialize()
        {
            if (_customer == null)
                _customer = new Lazy<CustomerGateway>(() => new CustomerGateway(_settings));

        }
    }
}
