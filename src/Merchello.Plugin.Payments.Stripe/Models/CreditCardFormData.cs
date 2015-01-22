using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchello.Plugin.Payments.Stripe.Models
{
    public class CreditCardFormData
    {
        /// <summary>
        /// The Stripe customer id
        /// </summary>
        public string StripeCustomerId { get; set; }

        /// <summary>
        /// The Stripe card id
        /// </summary>
        public string StripeCardId { get; set; }

        /// <summary>
        /// The Stripe card token
        /// </summary>
        public string StripeCardToken { get; set; }

        /// <summary>
        /// The type of the credit card.  
        /// </summary>
        public string CreditCardType { get; set; }

        /// <summary>
        /// The card holders name
        /// </summary>
        public string CardholderName { get; set; }

        /// <summary>
        /// The credit card number
        /// </summary>
        public string CardNumber { get; set; }

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
    }
}
