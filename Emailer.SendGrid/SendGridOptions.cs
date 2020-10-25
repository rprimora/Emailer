using System.Collections.Generic;

namespace Emailer.SendGrid
{
    /// <summary>
    /// SendGrid options.
    /// </summary>
    public class SendGridOptions
    {
        /// <summary>
        /// Gets or sets API key.
        /// </summary>
        public string APIKey { get; set; }

        /// <summary>
        /// Gets or sets the senders.
        /// </summary>
        public Dictionary<string, SenderInformation> Senders { get; set; } = new Dictionary<string, SenderInformation>();
    }

    /// <summary>
    /// Information about sender.
    /// </summary>
    public class SenderInformation
    {
        /// <summary>
        /// Gets or sets the sender email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the sender name.
        /// </summary>
        public string Name { get; set; }
    }
}