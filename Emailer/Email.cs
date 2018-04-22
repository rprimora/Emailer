using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Emailer
{
    /// <summary>
    /// Describes an email service used to send emails.
    /// </summary>
    public interface IEmail
    {
        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendEmailAsync<TModel>(TModel model) where TModel: EmailModel;

        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <param name="client">Function that returns <see cref="SmtpClient"/>.</param>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendEmailAsync<TModel>(Func<SmtpClient> client, TModel model) where TModel : EmailModel;
    }

    /// <summary>
    /// Email service.
    /// </summary>
    public class Email : IEmail
    {
        #region Members

        private EmailOptions m_options;
        private IServiceProvider m_serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Email"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="options">Options.</param>
        public Email(IServiceProvider serviceProvider, IOptions<EmailOptions> options)
        {
            m_serviceProvider = serviceProvider;
            m_options = options.Value;
        }

        #endregion

        #region IEmail implementation

        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task SendEmailAsync<TModel>(TModel model) where TModel : EmailModel
        {
            var body = await m_serviceProvider.GetService<IViewToStringRenderer>().RenderViewToString(model.EmailView, model);
            
            MailMessage mailMessage = new MailMessage(string.IsNullOrEmpty(m_options.Sender) ? m_options.Username : m_options.Sender, model.To)
            {
                Subject = model.Subject,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Body = body
            };

            await GetClient().SendMailAsync(mailMessage);
        }

        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <param name="client">Function that returns <see cref="SmtpClient"/>.</param>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task SendEmailAsync<TModel>(Func<SmtpClient> client, TModel model) where TModel : EmailModel
        {
            var body = await m_serviceProvider.GetService<IViewToStringRenderer>().RenderViewToString(model.EmailView, model);

            MailMessage mailMessage = new MailMessage(string.IsNullOrEmpty(m_options.Sender) ? m_options.Username : m_options.Sender, model.To)
            {
                Subject = model.Subject,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Body = body
            };

            await client().SendMailAsync(mailMessage);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns a <see cref="SmtpClient"/>.
        /// </summary>
        /// <returns><see cref="SmtpClient"/> object.</returns>
        private SmtpClient GetClient()
        {
            return new SmtpClient()
            {
                Port = m_options.Port,
                Host = m_options.Host,
                EnableSsl = m_options.EnableSsl,
                Timeout = m_options.Timeout,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(m_options.Username, m_options.Password)
            };
        }

        #endregion
    }

    /// <summary>
    /// Contains extension methods for <see cref="Email"/>.
    /// </summary>
    public static class EmailExtension
    {
        /// <summary>
        /// Adds <see cref="IEmail"/> service to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for <see cref="IEmail"/> service.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddEmail(this IServiceCollection services, Action<EmailOptions> options)
        {
            services.Configure(options);
            services.AddTransient<IEmail, Email>();
            return services;
        }

        /// <summary>
        /// Adds <see cref="IEmail"/> service to the service collection. This method assumes you have added configuration in the appsettings.json.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            EmailOptions options = new EmailOptions();
            Action<EmailOptions> configureOptions = (o) => configuration.GetSection("SmtpSettings").Bind(o);
            services.Configure(configureOptions);
            services.AddTransient<IEmail, Email>();
            return services;
        }
    }

    /// <summary>
    /// Options used to instantiate <see cref="SmtpClient"/>.
    /// </summary>
    public class EmailOptions
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
