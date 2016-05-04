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
            var requestParams = new NameValueCollection
            {
                {"card[number]", creditCard.CardNumber},
                {"card[exp_month]", creditCard.ExpireMonth},
                {"card[exp_year]", creditCard.ExpireYear},
                {"card[cvc]", creditCard.CardCode},
                {"card[name]", creditCard.CardholderName}
            };

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

            // https://stripe.com/docs/api#create_card_token
            var response = MakeStripeApiRequest("https://api.stripe.com/v1/tokens", "POST", requestParams, settings);
            string apiResponse;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                apiResponse = reader.ReadToEnd();
            }
            var responseJson = JObject.Parse(apiResponse);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200
                    return (string)responseJson["id"];

                default:

                    throw new Exception("Stripe error");
            }

        }

        public static NameValueCollection PreparePostDataForProcessPayment(IAddress billingAddress,
            TransactionMode transactionMode,
            string amount, string currency, CreditCardFormData creditCard, string invoiceNumber, string description)
        {
            var requestParams = new NameValueCollection { { "amount", amount }, { "currency", currency } };
            if (transactionMode == TransactionMode.Authorize)
                requestParams.Add("capture", "false");

            if (!string.IsNullOrEmpty(creditCard.StripeCustomerId))
            {
                requestParams.Add("customer", creditCard.StripeCustomerId);
                if (!string.IsNullOrEmpty(creditCard.StripeCardId))
                    requestParams.Add("source", creditCard.StripeCardId);
            }
            else
            {
                if (!string.IsNullOrEmpty(creditCard.StripeCardToken))
                {
                    requestParams.Add("source", creditCard.StripeCardToken);
                }
                else
                {
                    requestParams.Add("source[number]", creditCard.CardNumber);
                    requestParams.Add("source[exp_month]", creditCard.ExpireMonth);
                    requestParams.Add("source[exp_year]", creditCard.ExpireYear);
                    requestParams.Add("source[cvc]", creditCard.CardCode);
                    requestParams.Add("source[name]", creditCard.CardholderName);

                    //requestParams.Add("receipt_email", address.Email); // note: this will send receipt email - maybe there should be a setting controlling if this is passed or not. Email could also be added to metadata
                    requestParams.Add("source[address_line1]", billingAddress.Address1);
                    requestParams.Add("source[address_line2]", billingAddress.Address2);
                    requestParams.Add("source[address_city]", billingAddress.Locality);
                    if (!string.IsNullOrEmpty(billingAddress.Region))
                        requestParams.Add("source[address_state]", billingAddress.Region);
                    requestParams.Add("source[address_zip]", billingAddress.PostalCode);
                    if (!string.IsNullOrEmpty(billingAddress.CountryCode))
                        requestParams.Add("source[address_country]", billingAddress.CountryCode);
                }
            }


            requestParams.Add("metadata[invoice_number]", invoiceNumber);
            requestParams.Add("description", description);

            return requestParams;
        }


        /// <summary>
        /// Helper method to get the StripeProcessorSettings from the Merchello context.  
        /// </summary>
        /// <param name="merchelloContext"></param>
        /// <returns>StripeProcessorSettings</returns>
        public static StripeProcessorSettings GetStripeProcessorSettings(
            Core.IMerchelloContext merchelloContext)
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
            Stream postData = null;

            if (requestParameters != null)
                postData = Encode(requestParameters);

            var request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = httpMethod;
            request.ContentLength = postData?.Length ?? 0;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");
            request.UserAgent = "Merchello (https://github.com/Merchello/Merchello)";

            if (postData != null)
                using (var requestStream = request.GetRequestStream())
                    postData.CopyTo(requestStream);

            return (HttpWebResponse)request.GetResponse();
        }

        public static Stream Encode(NameValueCollection formDataSet)
        {
            var result = new StringBuilder();
            var firstEntry = true;
            foreach (var key in formDataSet.Cast<string>())
            {
                Func<byte, byte[]> subsubsteps = b =>
                {
                    if (b == 0x20)
                        return new byte[] { 0x2B };
                    if (b == 0x2A || b == 0x2D || b == 0x2E || (b >= 0x30 && b <= 0x39) ||
                        (b >= 0x41 && b <= 0x5A) ||
                        b == 0x5F || (b >= 0x61 && b <= 0x7A))
                        return new[] { b };
                    return Encoding.ASCII.GetBytes("%" + b.ToString("X2"));
                };

                var name =
                    Encoding.ASCII.GetString(
                        Encoding.UTF8.GetBytes(key)
                            .Select(subsubsteps)
                            .SelectMany(bytes => bytes)
                            .ToArray());
                var value =
                    Encoding.ASCII.GetString(
                        Encoding.UTF8.GetBytes(formDataSet[key])
                            .Select(subsubsteps)
                            .SelectMany(bytes => bytes)
                            .ToArray());

                if (!firstEntry)
                    result.Append('&');

                result.Append(name);
                result.Append('=');
                result.Append(value);

                firstEntry = false;
            }

            return new MemoryStream(Encoding.ASCII.GetBytes(result.ToString()));
        }
    }
}
