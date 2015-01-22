using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Merchello.Core.Models;
using Merchello.Plugin.Payments.Stripe;
using Merchello.Plugin.Payments.Stripe.Models;
using Newtonsoft.Json.Linq;

namespace Merchello.Plugin.Payments.Stripe
{
    public class StripeHelper
    {
        /// <summary>
        /// Gets a single use token that can be used in place of a credit card details. 
        /// The token can be used once for creating a new charge. 
        /// </summary>
        /// <param name="creditCard"></param>
        /// <param name="address"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string GetCardToken(CreditCardFormData creditCard, IAddress address, StripeProcessorSettings settings)
        {
            var requestParams = new NameValueCollection();
            requestParams.Add("card[number]", creditCard.CardNumber);
            requestParams.Add("card[exp_month]", creditCard.ExpireMonth);
            requestParams.Add("card[exp_year]", creditCard.ExpireYear);
            requestParams.Add("card[cvc]", creditCard.CardCode);
            requestParams.Add("card[name]", creditCard.CardholderName);

            if (address != null)
            {
                requestParams.Add("card[address_line1]", address.Address1);
                requestParams.Add("card[address_line2]", address.Address2);
                requestParams.Add("card[address_city]", address.Locality);
                if (!string.IsNullOrEmpty(address.Region)) 
                    requestParams.Add("card[address_state]", address.Region);
                requestParams.Add("card[address_zip]", address.PostalCode);
                if (!string.IsNullOrEmpty(address.CountryCode))
                    requestParams.Add("card[address_country]", address.CountryCode);
            }

            string postData =
                requestParams.AllKeys.Aggregate("",
                    (current, key) => current + (key + "=" + HttpUtility.UrlEncode(requestParams[key]) + "&"))
                    .TrimEnd('&');

            // https://stripe.com/docs/api#create_card_token
            var response = StripeHelper.MakeStripeApiRequest("https://api.stripe.com/v1/tokens", "POST", requestParams, settings);
            string apiResponse = null;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                apiResponse = reader.ReadToEnd();
            }
            JObject responseJson = JObject.Parse(apiResponse);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200
                    return (string) responseJson["id"];

                default:
                    
                    throw new Exception("Stripe error");
            }

        }

        public static NameValueCollection PreparePostDataForProcessPayment(IAddress billingAddress,
            TransactionMode transactionMode,
            string amount, string currency, CreditCardFormData creditCard, string invoiceNumber, string description)
        {
            var requestParams = new NameValueCollection();
            requestParams.Add("amount", amount);
            requestParams.Add("currency", currency);
            if (transactionMode == TransactionMode.Authorize)
                requestParams.Add("capture", "false");

            if (!String.IsNullOrEmpty(creditCard.StripeCustomerId))
            {
                requestParams.Add("customer", creditCard.StripeCustomerId);
                if (!String.IsNullOrEmpty(creditCard.StripeCardId))
                    requestParams.Add("card", creditCard.StripeCardId);
            }
            else
            {
                if (!String.IsNullOrEmpty(creditCard.StripeCardToken))
                {
                    requestParams.Add("card", creditCard.StripeCardToken);
                }
                else
                {
                    requestParams.Add("card[number]", creditCard.CardNumber);
                    requestParams.Add("card[exp_month]", creditCard.ExpireMonth);
                    requestParams.Add("card[exp_year]", creditCard.ExpireYear);
                    requestParams.Add("card[cvc]", creditCard.CardCode);
                    requestParams.Add("card[name]", creditCard.CardholderName);

                    //requestParams.Add("receipt_email", address.Email); // note: this will send receipt email - maybe there should be a setting controlling if this is passed or not. Email could also be added to metadata
                    requestParams.Add("card[address_line1]", billingAddress.Address1);
                    requestParams.Add("card[address_line2]", billingAddress.Address2);
                    requestParams.Add("card[address_city]", billingAddress.Locality);
                    if (!string.IsNullOrEmpty(billingAddress.Region))
                        requestParams.Add("card[address_state]", billingAddress.Region);
                    requestParams.Add("card[address_zip]", billingAddress.PostalCode);
                    if (!string.IsNullOrEmpty(billingAddress.CountryCode))
                        requestParams.Add("card[address_country]", billingAddress.CountryCode);
                }
            }


            requestParams.Add("metadata[invoice_number]", invoiceNumber);
            requestParams.Add("description", description);

            string postData =
                requestParams.AllKeys.Aggregate("",
                    (current, key) => current + (key + "=" + HttpUtility.UrlEncode(requestParams[key]) + "&"))
                    .TrimEnd('&');
            return requestParams;
        }


        /// <summary>
        /// Helper method to get the StripeProcessorSettings from the Merchello context.  
        /// </summary>
        /// <param name="merchelloContext"></param>
        /// <returns>StripeProcessorSettings</returns>
        public static StripeProcessorSettings GetStripeProcessorSettings(
            Merchello.Core.IMerchelloContext merchelloContext)
        {
            StripeProcessorSettings settings = null;
            var provider =
                merchelloContext.Gateways.Payment.GetProviderByKey(new Guid("15C87B6F-7987-49D9-8444-A2B4406941A8"));

            if (provider != null)
                settings = provider.GatewayProviderSettings.ExtendedData.GetProcessorSettings();

            return settings;
        }

        public static HttpWebResponse MakeStripeApiRequest(string apiUrl, string httpMethod,
            NameValueCollection requestParameters, StripeProcessorSettings settings)
        {
            string postData = requestParameters == null
                ? ""
                : requestParameters.AllKeys.Aggregate("",
                    (current, key) => current + (key + "=" + HttpUtility.UrlEncode(requestParameters[key]) + "&"))
                    .TrimEnd('&');

            var request = (HttpWebRequest) WebRequest.Create(apiUrl);
            request.Method = httpMethod;
            request.ContentLength = postData.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Authorization", BasicAuthToken(settings.ApiKey));
            request.Headers.Add("Stripe-Version", StripePaymentProcessor.ApiVersion);
            request.UserAgent = "Merchello (https://github.com/Merchello/Merchello)";
            if (requestParameters != null)
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postData);
                }
            }

            return (HttpWebResponse) request.GetResponse();
        }

        private static string BasicAuthToken(string apiKey)
        {
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:", apiKey)));
            return string.Format("Basic {0}", token);
        }
    }
}
