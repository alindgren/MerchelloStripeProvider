namespace Merchello.Plugin.Payments.Stripe.Models
{
    using System;

    public class StripeCustomer
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
    }
}
