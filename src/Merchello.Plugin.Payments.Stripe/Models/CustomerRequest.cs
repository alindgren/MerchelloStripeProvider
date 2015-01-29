using System.Collections.Specialized;

namespace Merchello.Plugin.Payments.Stripe.Models
{
    public class CustomerRequest
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }

        public NameValueCollection ToNameValueCollection()
        {
            var requestParams = new NameValueCollection();
            if (Id != null)
                requestParams.Add("id", Id);
            if (Description != null)
                requestParams.Add("description", Description);
            if (Email != null)
                requestParams.Add("email", Email);

            return requestParams;
        }
    }
}
