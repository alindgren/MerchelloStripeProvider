namespace Merchello.Plugin.Payments.Stripe.Gateways
{
    using Merchello.Plugin.Payments.Stripe.Exceptions;
    using Merchello.Plugin.Payments.Stripe.Models;
    using Newtonsoft.Json.Linq;

    public class CardGateway
    {
        private readonly StripeProcessorSettings _settings;
        private const string Api_Url = "https://api.stripe.com/v1/customers/{CUSTOMER_ID}/cards";

        public CardGateway(StripeProcessorSettings settings)
        {
            _settings = settings;
        }
    }
}
