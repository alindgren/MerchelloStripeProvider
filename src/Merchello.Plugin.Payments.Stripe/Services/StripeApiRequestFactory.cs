namespace Merchello.Plugin.Payments.Stripe.Services
{
    using System;
    using Core.Models;
    using Models;

    internal class StripeApiRequestFactory
    {
        /// <summary>
        /// The <see cref="StripeProcessorSettings"/>.
        /// </summary>
        private readonly StripeProcessorSettings _settings;

        public StripeApiRequestFactory(StripeProcessorSettings settings)
        {
            Mandate.ParameterNotNull(settings, "settings");

            _settings = settings;
        }

        #region Customer Request

        /// <summary>
        /// Creates a simple <see cref="CustomerRequest"/>.
        /// </summary>
        /// <param name="customer">
        /// The customer.
        /// </param>
        /// <returns>
        /// The <see cref="CustomerRequest"/>.
        /// </returns>
        public CustomerRequest CreateCustomerRequest(ICustomer customer)
        {
            Mandate.ParameterNotNull(customer, "customer");

            return new CustomerRequest()
            {
                Id = customer.Key.ToString(),
                Email = customer.Email
            };
        }

        #endregion
    }
}
