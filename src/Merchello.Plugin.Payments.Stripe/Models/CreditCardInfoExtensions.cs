using Merchello.Core.Gateways.Payment;

namespace Merchello.Plugin.Payments.Stripe.Models
{
    public static class CreditCardInfoExtensions
    {
        public static ProcessorArgumentCollection AsProcessorArgumentCollection(this StripeCard creditCard)
        {
            return new ProcessorArgumentCollection()
            {
                { "creditCardType", creditCard.Type },
                { "cardholderName", creditCard.CardholderName },
                { "cardNumber", creditCard.Number },
                { "expireMonth", creditCard.ExpireMonth },
                { "expireYear", creditCard.ExpireYear },
                { "cardCode", creditCard.CardCode }
            };
        }

        public static StripeCard AsCreditCardFormData(this ProcessorArgumentCollection args)
        {
            return new StripeCard()
            {
                Type = args.ArgValue("creditCardType"),
                CardholderName = args.ArgValue("cardholderName"),
                Number = args.ArgValue("cardNumber"),
                ExpireMonth = args.ArgValue("expireMonth"),
                ExpireYear = args.ArgValue("expireYear"),
                CardCode = args.ArgValue("cardCode")
            };
        }

        private static string ArgValue(this ProcessorArgumentCollection args, string key)
        {
            return args.ContainsKey(key) ? args[key] : string.Empty;
        }

    }
}