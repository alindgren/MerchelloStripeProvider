using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Net;
using Merchello.Plugin.Payments.Stripe.Exceptions;
using Merchello.Plugin.Payments.Stripe.Models;
using Newtonsoft.Json.Linq;

namespace Merchello.Plugin.Payments.Stripe.Gateways
{
    public class CustomerGateway
    {
        private readonly StripeProcessorSettings _settings;
        private const string Api_Url = "https://api.stripe.com/v1/customers";

        public CustomerGateway(StripeProcessorSettings settings)
        {
            _settings = settings;
        }

        public GatewayResult<StripeCustomer> Create(CustomerRequest request)
        {
            var result = new GatewayResult<StripeCustomer>();
            var response = StripeHelper.MakeStripeApiRequest(Api_Url, "POST", request.ToNameValueCollection(), _settings);

            if (response.StatusCode == HttpStatusCode.OK && response.ContentLength > 0)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    try
                    {
                        JObject responseJson = JObject.Parse(reader.ReadToEnd());
                        result.Target = new StripeCustomer
                        {
                            Id = (string) responseJson["id"]
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