using System;
using System.Linq;
using Merchello.Core;
using Umbraco.Core.Events;

namespace Merchello.Plugin.Payments.Stripe.Services
{
    using Models;
    using Umbraco.Core;
    using Core.Models;

    internal class StripeCustomerApiService : StripeApiServiceBase, IStripeCustomerApiService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StripeCustomerApiService"/> class.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public StripeCustomerApiService(StripeProcessorSettings settings)
            : this(Core.MerchelloContext.Current, settings)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StripeCustomerApiService"/> class.
        /// </summary>
        /// <param name="merchelloContext">
        /// The merchello context.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        internal StripeCustomerApiService(IMerchelloContext merchelloContext, StripeProcessorSettings settings)
            : base(merchelloContext, settings)
        {
        }

        #region Events

        /// <summary>
        /// Occurs before the Create
        /// </summary>
        public static event TypedEventHandler<StripeCustomerApiService, Core.Events.NewEventArgs<CustomerRequest>> Creating;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        public static event TypedEventHandler<StripeCustomerApiService, Core.Events.NewEventArgs<StripeCustomer>> Created;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<StripeCustomerApiService, SaveEventArgs<CustomerRequest>> Updating;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<StripeCustomerApiService, SaveEventArgs<StripeCustomer>> Updated;

        #endregion


        /// <summary>
        /// Creates a Stripe <see cref="Customer"/> from a Merchello <see cref="ICustomer"/>
        /// </summary>
        /// <param name="customer">
        /// The customer.
        /// </param>
        /// <param name="billingAddress">
        /// The billing address
        /// </param>
        /// <param name="shippingAddress">
        /// The shipping Address.
        /// </param>
        /// <returns>
        /// The <see cref="Attempt{Customer}"/>.
        /// </returns>
        public Attempt<StripeCustomer> Create(ICustomer customer)
        {
            if (Exists(customer)) return Attempt.Succeed(GetStripeCustomer(customer));

            var request = RequestFactory.CreateCustomerRequest(customer);

            Creating.RaiseEvent(new Core.Events.NewEventArgs<CustomerRequest>(request), this);

            var result = StripeGateway.Customer.Create(request);

            if (result.IsSuccess())
            {
                Created.RaiseEvent(new Core.Events.NewEventArgs<StripeCustomer>(result.Target), this);

                return
                    Attempt.Succeed(
                        (StripeCustomer) RuntimeCache.GetCacheItem(MakeCustomerCacheKey(customer), () => result.Target));
            }

            return Attempt<StripeCustomer>.Fail(result.Errors.First());
        }

        /// <summary>
        /// Returns true or false indicating whether the customer exists in Stripe
        /// </summary>
        /// <param name="customer">
        /// The customer.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Exists(ICustomer customer)
        {
            var cacheKey = MakeCustomerCacheKey(customer);

            var stripeCustomer = RuntimeCache.GetCacheItem(cacheKey);

            if (stripeCustomer == null)
            {
                //// TODO: 
                //var attempt = TryGetApiResult(() => StripeGateway.Customer.Find(customer.Key.ToString()));

                //if (!attempt.Success)
                //{
                //    return false;
                //}

                //stripeCustomer = attempt.Result;

                //RuntimeCache.GetCacheItem(cacheKey, () => stripeCustomer);
            }

            return stripeCustomer != null;
        }

        /// <summary>
        /// Gets the Stripe <see cref="Customer"/> corresponding to the Merchello <see cref="ICustomer"/>
        /// </summary>
        /// <param name="customerKey">
        /// The customer key.
        /// </param>
        /// <param name="createOnNotFound">True or false indicating whether or not the customer should be automatically created if not found</param>
        /// <returns>
        /// The <see cref="Customer"/>.
        /// </returns>
        public StripeCustomer GetStripeCustomer(Guid customerKey, bool createOnNotFound = true)
        {
            var customer = MerchelloContext.Services.CustomerService.GetByKey(customerKey);

            return GetStripeCustomer(customer, createOnNotFound);
        }

        /// <summary>
        /// Gets the Stripe <see cref="Customer"/> corresponding to the Merchello <see cref="ICustomer"/>
        /// </summary>
        /// <param name="customer">
        /// The customer.
        /// </param>
        /// <param name="createOnNotFound">
        /// True or false indicating whether or not the customer should be automatically created if not found
        /// </param>
        /// <returns>
        /// The <see cref="Customer"/>.
        /// </returns>
        public StripeCustomer GetStripeCustomer(ICustomer customer, bool createOnNotFound = true)
        {
            //Umbraco.Core.Mandate.ParameterNotNull(customer, "customer");

            //if (Exists(customer))
            //{
            //    var cacheKey = MakeCustomerCacheKey(customer);

            //    return (StripeCustomer)RuntimeCache.GetCacheItem(cacheKey, () => StripeGateway.Customer.Find(customer.Key.ToString()));
            //}

            if (!createOnNotFound) return null;

            var attempt = Create(customer);

            return attempt.Success ? attempt.Result : null;
        }

    }
}
