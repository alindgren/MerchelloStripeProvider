using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Merchello.Plugin.Payments.Stripe.Gateways
{
    using Exceptions;
    using Models;
    using Newtonsoft.Json.Linq;

    public class CardGateway
    {
        private readonly StripeProcessorSettings _settings;
        private const string Api_Url = "https://api.stripe.com/v1/customers/{0}/cards";

        public CardGateway(StripeProcessorSettings settings)
        {
            _settings = settings;
        }

        public GatewayResult<StripeCard> Create(StripeCard request)
        {
            var result = new GatewayResult<StripeCard>();
            string url = string.Format(Api_Url, request.CustomerId);
            var response = StripeHelper.MakeStripeApiRequest(url, "POST", request.ToNameValueCollection(), _settings);

            if (response.StatusCode == HttpStatusCode.OK && response.ContentLength > 0)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    try
                    {
                        JObject responseJson = JObject.Parse(reader.ReadToEnd());
                        result.Target = new StripeCard
                        {
                            Id = (string)responseJson["id"]
                        };
                    }
                    catch (Exception ex)
                    {
                        result.Errors = new List<Exception>
                        {
                            ex
                        };
                    }
                }
            }
            else
            {
                result.Errors = new List<Exception>
                {
                    new StripeApiException("")
                };
            }

            return result;
        }

    }
}
