namespace Merchello.Plugin.Payments.Stripe.Services
{
    using Core.Models;
    using Umbraco.Core;
    using Models;

    public interface IStripeCustomerApiService
    {
        /// <summary>
        /// Creates a Stripe <see cref="Customer"/> from a Merchello <see cref="ICustomer"/>
        /// </summary>
        /// <param name="customer">
        /// The customer.
        /// </param>
        /// <returns>
        /// The <see cref="Attempt{Customer}"/>.
        /// </returns>
        Attempt<StripeCustomer> Create(ICustomer customer);
    }
}
