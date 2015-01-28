namespace Merchello.Plugin.Payments.Stripe.Caching
{
    using System;

    /// <summary>
    /// The cache keys used in this Braintree plugin
    /// </summary>
    internal static class CacheKeys
    {
        /// <summary>
        /// Cache key to cache a Braintree customer.
        /// </summary>
        /// <param name="customerKey">
        /// The customer key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> cache key.
        /// </returns>
        public static string StripeCustomer(Guid customerKey)
        {
            return StripeCustomer(customerKey.ToString());
        }

        /// <summary>
        /// Cache key to cache a Braintree customer.
        /// </summary>
        /// <param name="customerId">
        /// The Stripe customer Id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> cache key.
        /// </returns>
        public static string StripeCustomer(string customerId)
        {
            return string.Format("stripe.customerId.{0}", customerId);
        }

        /// <summary>
        /// Cache key to cache a payment method.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> cache key.
        /// </returns>
        public static string StripePaymentMethod(string token)
        {
            return string.Format("stripe.paymentmethod.{0}", token);
        }

        /// <summary>
        /// Cache key used to cache a Braintree subscription.
        /// </summary>
        /// <param name="subscriptionId">
        /// The subscription id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> cache key.
        /// </returns>
        public static string StripeSubscription(string subscriptionId)
        {
            return string.Format("stripe.subscription.{0}", subscriptionId);
        }
    }
}