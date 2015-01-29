namespace Merchello.Plugin.Payments.Stripe.Models
{
    public class StripeCard
    {
        /// <summary>
        /// The Stripe customer id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// The Stripe card id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Stripe card token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The type of the credit card.  
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The card holders name
        /// </summary>
        public string CardholderName { get; set; }

        /// <summary>
        /// The credit card number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The expiration month - format MM
        /// </summary>
        public string ExpireMonth { get; set; }

        /// <summary>
        /// The expiration year = format yy
        /// </summary>
        public string ExpireYear { get; set; }

        /// <summary>
        /// The credit card code or CVV
        /// </summary>
        public string CardCode { get; set; }

        // optional properties
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}
