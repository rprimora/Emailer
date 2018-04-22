namespace Emailer
{
    /// <summary>
    /// Options used to instantiate <see cref="SmtpClient"/>.
    /// </summary>
    public class EmailerOptions
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
        /// Gets or sets the username of email account.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password of email account.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a bool value indicating whether SSL is enabled.
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the timeout. Default is 10000(10s).
        /// </summary>
        public int Timeout { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the mail address of the sender.
        /// </summary>
        public string Sender { get; set; }
    }
}