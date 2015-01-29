namespace Merchello.Plugin.Payments.Stripe.Services
{
    using System;
    using Core;
    using Models;

    public class StripeApiService : IStripeApiService
    {
        /// <summary>
        /// The <see cref="StripeProcessorSettings"/>.
        /// </summary>
        private readonly StripeProcessorSettings _settings;

        /// <summary>
        /// The <see cref="IStripeCustomerApiService"/>.
        /// </summary>
        private Lazy<IStripeCustomerApiService> _customer;

        public StripeApiService(StripeProcessorSettings settings)
            : this(MerchelloContext.Current, settings)
        {
            
        }

        internal StripeApiService(IMerchelloContext merchelloContext, StripeProcessorSettings settings)
        {
            //Mandate.ParameterNotNull(merchelloContext, "merchelloContext");
            Mandate.ParameterNotNull(settings, "settings");

            this._settings = settings;

            this.Initialize(merchelloContext);
        }

        /// <summary>
        /// Gets the customer API provider
        /// </summary>
        public IStripeCustomerApiService Customer
        {
            get { return _customer.Value; }
        }

        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="merchelloContext">
        /// The merchello context.
        /// </param>
        private void Initialize(IMerchelloContext merchelloContext)
        {
            if (_customer == null)
                _customer = new Lazy<IStripeCustomerApiService>(() => new StripeCustomerApiService(merchelloContext, _settings));

            //if (_paymentMethod == null)
            //    _paymentMethod = new Lazy<IBraintreePaymentMethodApiService>(() => new BraintreePaymentMethodApiService(merchelloContext, _settings, _customer.Value));

            //if (_subscription == null)
            //    _subscription = new Lazy<IBraintreeSubscriptionApiService>(() => new BraintreeSubscriptionApiService(merchelloContext, _settings));

            //if (_transaction == null)
            //    _transaction = new Lazy<IBraintreeTransactionApiService>(() => new BraintreeTransactionApiService(merchelloContext, _settings));

            //if (_webhooks == null)
            //    _webhooks = new Lazy<IBraintreeWebhooksApiService>(() => new BraintreeWebhooksApiService(merchelloContext, _settings));
        }
    }
}
