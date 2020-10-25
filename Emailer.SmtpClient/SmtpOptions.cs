using System.Collections.Generic;

namespace Emailer.SmtpClient
{
    /// <summary>
    /// Options used to instantiate smtp client.
    /// </summary>
    public class SmtpOptions
    {
        /// <summary>
        /// Gets or sets the port number.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets a bool value indicating whether SSL is enabled.
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the timeout. Default is 10000(10s).
        /// </summary>
        public int Timeout { get; set; } = 10000;

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
        /// Gets or sets the username of email account.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password of email account.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the sender name.
        /// </summary>
        public string Name { get; set; }
    }
}