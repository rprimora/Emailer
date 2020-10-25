namespace Emailer.Abstractions
{
    /// <summary>
    /// Defines an email model.
    /// </summary>
    public abstract class EmailModel
    {
        /// <summary>
        /// Gets or sets the name of the view that represents the email body.
        /// </summary>
        public string EmailView { get; set; }

        /// <summary>
        /// Gets or sets the email subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the email address of the recipient.
        /// </summary>
        public string To { get; set; }
    }
}
