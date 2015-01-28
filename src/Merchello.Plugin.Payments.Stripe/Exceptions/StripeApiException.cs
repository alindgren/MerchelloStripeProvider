namespace Merchello.Plugin.Payments.Stripe.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class StripeApiException : Exception
    {
        public StripeApiException(string message) : base(message)
        {
            
        }
        ///// <summary>
        ///// Initializes a new instance of the <see cref="StripeApiException"/> class.
        ///// </summary>
        ///// <param name="validationError">
        ///// The validation error.
        ///// </param>
        //public StripeApiException(ValidationError validationError)
        //    : this(new[] { validationError })
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="StripeApiException"/> class.
        ///// </summary>
        ///// <param name="validationErrors">
        ///// The validation errors.
        ///// </param>
        //public StripeApiException(ValidationErrors validationErrors)
        //    : this(validationErrors.All())
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="BraintreeApiException"/> class.
        ///// </summary>
        ///// <param name="validationErrors">
        ///// The validation errors.
        ///// </param>
        //public StripeApiException(IEnumerable<ValidationError> validationErrors)
        //    : base(string.Join(" ", validationErrors.Select(x => x.Message)))
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="BraintreeApiException"/> class.
        ///// </summary>
        ///// <param name="validationErrors">
        ///// The validation errors.
        ///// </param>
        ///// <param name="message">
        ///// The message.
        ///// </param>
        //public StripeApiException(ValidationErrors validationErrors, string message)
        //    : base(string.Format("{0} {1} {2}", message, System.Environment.NewLine, string.Join(" ", validationErrors.All().Select(x => x.Message))))
        //{

        //}
    }
}