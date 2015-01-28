using System;
using Merchello.Core;
using Merchello.Core.Models;
using Merchello.Plugin.Payments.Stripe.Gateways;
using Merchello.Plugin.Payments.Stripe.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Merchello.Plugin.Payments.Stripe.Services
{
    
    internal abstract class StripeApiServiceBase
    {
        /// <summary>
        /// The <see cref="StripeApiRequestFactory"/>.
        /// </summary>
        private Lazy<StripeApiRequestFactory> _requestFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BraintreeApiServiceBase"/> class.
        /// </summary>
        /// <param name="merchelloContext">
        /// The <see cref="IMerchelloContext"/>.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        protected StripeApiServiceBase(IMerchelloContext merchelloContext, StripeProcessorSettings settings)
        {
            Mandate.ParameterNotNull(merchelloContext, "merchelloContext");
            Mandate.ParameterNotNull(settings, "settings");

            MerchelloContext = merchelloContext;

            Initialize(settings);
        }

        /// <summary>
        /// Gets the merchello context.
        /// </summary>
        protected IMerchelloContext MerchelloContext { get; private set; }

        /// <summary>
        /// Gets the stripe gateway.
        /// </summary>
        protected StripeGateway StripeGateway { get; private set; }

        /// <summary>
        /// Gets the runtime cache.
        /// </summary>
        protected IRuntimeCacheProvider RuntimeCache
        {
            get { return this.MerchelloContext.Cache.RuntimeCache; }
        }

        /// <summary>
        /// Gets the request factory.
        /// </summary>
        protected StripeApiRequestFactory RequestFactory
        {
            get
            {
                return _requestFactory.Value;
            }
        }

        /// <summary>
        /// Attempts to execute an API request
        /// </summary>
        /// <param name="apiMethod">
        /// The api method.
        /// </param>
        /// <typeparam name="T">
        /// The type of Result to return
        /// </typeparam>
        /// <returns>
        /// The result <see cref="Attempt{T}"/> of the API request.
        /// </returns>
        protected Attempt<T> TryGetApiResult<T>(Func<T> apiMethod)
        {
            try
            {
                var result = apiMethod.Invoke();

                return Attempt<T>.Succeed(result);
            }
            catch (Exception ex)
            {
                LogHelper.Error<StripeApiServiceBase>("Stripe API request failed.", ex);
                return Attempt<T>.Fail(default(T), ex);
            }
        }

        /// <summary>
        /// Makes a customer cache key.
        /// </summary>
        /// <param name="customer">
        /// The customer.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> cache key.
        /// </returns>
        protected string MakeCustomerCacheKey(ICustomer customer)
        {
            return Caching.CacheKeys.StripeCustomer(customer.Key);
        }

        /// <summary>
        /// Performs class initialization logic.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        private void Initialize(StripeProcessorSettings settings)
        {
            StripeGateway = new StripeGateway(settings);

            _requestFactory = new Lazy<StripeApiRequestFactory>(() => new StripeApiRequestFactory(settings));
        }
    }
}
