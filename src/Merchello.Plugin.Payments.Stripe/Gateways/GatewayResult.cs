using System;
using System.Collections.Generic;
using System.Linq;

namespace Merchello.Plugin.Payments.Stripe.Gateways
{
    public class GatewayResult<T> where T : class
    {
        public IEnumerable<Exception> Errors { get; set; }

        public bool IsSuccess()
        {
            return Errors == null || !Errors.Any();
        }

        public T Target { get; set; }
    }
}
