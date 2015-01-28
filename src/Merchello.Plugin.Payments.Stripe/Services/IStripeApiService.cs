namespace Merchello.Plugin.Payments.Stripe.Services
{
    /// <summary>
    /// Defines the <see cref="StripeApiService"/>
    /// </summary>
    public interface IStripeApiService
    {
        IStripeCustomerApiService Customer { get; }
    }
}
