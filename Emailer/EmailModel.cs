namespace Emailer
{
    /// <summary>
    /// Represents the base model for email views.
    /// </summary>
    public class EmailModel
    {
        /// <summary>
        /// Name of the view that represents the email body.
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