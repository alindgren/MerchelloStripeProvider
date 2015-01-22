using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Merchello.Core.Gateways.Payment;
using Merchello.Core.Models;
using Merchello.Plugin.Payments.Stripe.Models;
using Newtonsoft.Json.Linq;
using Umbraco.Core;

namespace Merchello.Plugin.Payments.Stripe
{
    public class StripePaymentProcessor
    {
        private readonly StripeProcessorSettings _settings;

        public StripePaymentProcessor(StripeProcessorSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        ///     The Stripe API version
        /// </summary>
        public static string ApiVersion
        {
            get { return "2014-06-17"; }
        }

        /// <summary>
        ///     Processes the Authorize and AuthorizeAndCapture transactions
        /// </summary>
        /// <param name="invoice">The <see cref="IInvoice" /> to be paid</param>
        /// <param name="payment">The <see cref="IPayment" /> record</param>
        /// <param name="transactionMode">Authorize or AuthorizeAndCapture</param>
        /// <param name="amount">The money amount to be processed</param>
        /// <param name="creditCard">The <see cref="CreditCardFormData" /></param>
        /// <returns>The <see cref="IPaymentResult" /></returns>
        public IPaymentResult ProcessPayment(IInvoice invoice, IPayment payment, TransactionMode transactionMode,
            decimal amount, CreditCardFormData creditCard)
        {
            if (!IsValidCurrencyCode(invoice.CurrencyCode()))
                return new PaymentResult(Attempt<IPayment>.Fail(payment, new Exception("Invalid currency")), invoice,
                    false);

            // The minimum amount is $0.50 (or equivalent in charge currency). 
            // Test that the payment meets the minimum amount (for USD only).
            if (invoice.CurrencyCode() == "USD")
            {
                if (amount < 0.5m)
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment, new Exception("Invalid amount (less than 0.50 USD)")),
                            invoice, false);
            }
            else
            {
                if (amount < 1)
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment,
                                new Exception("Invalid amount (less than 1 " + invoice.CurrencyCode() + ")")),
                            invoice, false);
            }

            var requestParams = StripeHelper.PreparePostDataForProcessPayment(invoice.GetBillingAddress(), transactionMode,
                ConvertAmount(invoice, amount), invoice.CurrencyCode(), creditCard, invoice.PrefixedInvoiceNumber(), 
                string.Format("Full invoice #{0}", invoice.PrefixedInvoiceNumber()));

            // https://stripe.com/docs/api#create_charge
            try
            {
                var response = StripeHelper.MakeStripeApiRequest("https://api.stripe.com/v1/charges", "POST", requestParams, _settings);
                return GetProcessPaymentResult(invoice, payment, response);
            }
            catch (WebException ex)
            {
                return GetProcessPaymentResult(invoice, payment, (HttpWebResponse) ex.Response);
            }
        }

        private static string ConvertAmount(IInvoice invoice, decimal amount)
        {
            // need to convert non-zero-decimal currencies
            bool isZeroDecimalCurrency = IsZeroDecimalCurrency(invoice.CurrencyCode());
            decimal stripeAmountDecimal = isZeroDecimalCurrency ? amount : (amount*100);
            return Convert.ToInt32(stripeAmountDecimal).ToString(CultureInfo.InvariantCulture);
        }

        private static IPaymentResult GetProcessPaymentResult(IInvoice invoice, IPayment payment,
            HttpWebResponse response)
        {
            string apiResponse = null;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                apiResponse = reader.ReadToEnd();
            }
            JObject responseJson = JObject.Parse(apiResponse);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200
                    payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.StripeChargeId,
                        (string) responseJson["id"]);
                    payment.Authorized = true;
                    if ((bool) responseJson["captured"]) payment.Collected = true;
                    return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice,
                        invoice.ShippingLineItems().Any());
                case HttpStatusCode.PaymentRequired: // 402
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment,
                                new Exception(string.Format("{0}", responseJson["error"]["message"]))), invoice, false);

                default:
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment,
                                new Exception(string.Format("{0}", "Stripe unknown error"))), invoice, false);
            }
        }

        public IPaymentResult PriorAuthorizeCapturePayment(IInvoice invoice, IPayment payment)
        {
            string stripeChargeId = payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.StripeChargeId);
            string url = string.Format("https://api.stripe.com/v1/charges/{0}/capture", stripeChargeId);
            try
            {
                var response = StripeHelper.MakeStripeApiRequest(url, "POST", null, _settings);
                return GetCapturePaymentResult(invoice, payment, response);
            }
            catch (WebException ex)
            {
                return GetCapturePaymentResult(invoice, payment, (HttpWebResponse) ex.Response);
            }
        }

        // TODO: is this identical to GetProcessPaymentResult()? If so, consolidate...
        private static IPaymentResult GetCapturePaymentResult(IInvoice invoice, IPayment payment,
            HttpWebResponse response)
        {
            string apiResponse = null;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                apiResponse = reader.ReadToEnd();
            }
            JObject responseJson = JObject.Parse(apiResponse);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200
                    payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.StripeChargeId, (string) responseJson["id"]);
                    payment.Authorized = true;
                    if ((bool) responseJson["captured"]) payment.Collected = true;
                    return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice,
                        invoice.ShippingLineItems().Any());
                case HttpStatusCode.PaymentRequired: // 402
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment,
                                new Exception(string.Format("{0}", responseJson["error"]["message"]))), invoice, false);
                default:
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment,
                                new Exception(string.Format("{0}", "Stripe unknown error"))), invoice, false);
            }
        }

        public IPaymentResult RefundPayment(IInvoice invoice, IPayment payment, decimal amount)
        {
            string stripeChargeId = payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.StripeChargeId);
            if (!payment.Authorized || string.IsNullOrEmpty(stripeChargeId))
                return
                    new PaymentResult(
                        Attempt<IPayment>.Fail(payment,
                            new InvalidOperationException("Payment is not Authorized or Stripe charge id not present")),
                        invoice, false);
            string url = string.Format("https://api.stripe.com/v1/charges/{0}/refunds", stripeChargeId);
            var requestParams = new NameValueCollection();
            requestParams.Add("amount", ConvertAmount(invoice, amount));
            var response = StripeHelper.MakeStripeApiRequest(url, "POST", requestParams, _settings);
            return GetRefundPaymentResult(invoice, payment, response);
        }

        private IPaymentResult GetRefundPaymentResult(IInvoice invoice, IPayment payment, HttpWebResponse response)
        {
            string apiResponse = null;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                apiResponse = reader.ReadToEnd();
            }
            JObject responseJson = JObject.Parse(apiResponse);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200
                    return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice,
                        invoice.ShippingLineItems().Any());
                case HttpStatusCode.PaymentRequired: // 402
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment,
                                new Exception(string.Format("Error {0}", responseJson["message"]))), invoice, false);
                default:
                    return
                        new PaymentResult(
                            Attempt<IPayment>.Fail(payment,
                                new Exception(string.Format("Error {0}", "Stripe unknown error"))), invoice, false);
            }
        }

        public IPaymentResult VoidPayment(IInvoice invoice, IPayment payment)
        {
            // Stripe does not seem to have a Void method, so we do a full refund
            string stripeChargeId = payment.ExtendedData.GetValue(Constants.ExtendedDataKeys.StripeChargeId);
            if (!payment.Authorized || string.IsNullOrEmpty(stripeChargeId))
                return
                    new PaymentResult(
                        Attempt<IPayment>.Fail(payment,
                            new InvalidOperationException("Payment is not Authorized or Stripe charge id not present")),
                        invoice, false);
            string url = string.Format("https://api.stripe.com/v1/charges/{0}/refunds", stripeChargeId);
            var response = StripeHelper.MakeStripeApiRequest(url, "POST", null, _settings);
            return GetRefundPaymentResult(invoice, payment, response);
        }

        /// <summary>
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <returns></returns>
        private static bool IsValidCurrencyCode(string currencyCode)
        {
            switch (currencyCode)
            {
                case "USD": // TODO: add other valid codes
                    return true;
            }
            return IsZeroDecimalCurrency(currencyCode);
        }

        /// <summary>
        ///     See https://support.stripe.com/questions/which-zero-decimal-currencies-does-stripe-support
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <returns></returns>
        private static bool IsZeroDecimalCurrency(string currencyCode)
        {
            switch (currencyCode)
            {
                case "BIF":
                case "DJF":
                case "JPY":
                case "KRW":
                case "PYG":
                case "VND":
                case "XAF":
                case "XPF":
                case "CLP":
                case "GNF":
                case "KMF":
                case "MGA":
                case "RWF":
                case "VUV":
                case "XOF":
                    return true;
            }
            return false;
        }
    }
}