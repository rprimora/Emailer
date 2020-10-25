using Emailer.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ViewRenderer.Abstractions;

namespace Emailer.SmtpClient
{
    /// <summary>
    /// Email service.
    /// </summary>
    public class SmtpEmailer : IEmailer
    {
        #region Members

        private readonly SmtpOptions m_options;
        private readonly IServiceProvider m_serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Emailer"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="options">Options.</param>
        public SmtpEmailer(IServiceProvider serviceProvider, IOptions<SmtpOptions> options)
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
            var body = await m_serviceProvider.GetService<IViewToStringRenderer>().RenderViewToStringAsync(model.EmailView, model);

            var senderInfo = m_options.Senders["default"];

            MailMessage mailMessage = new MailMessage(new MailAddress(senderInfo.Email, senderInfo.Name), new MailAddress(model.To))
            {
                Subject = model.Subject,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Body = body
            };

            await GetClient(senderInfo).SendMailAsync(mailMessage);
        }

        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <param name="client">Function that returns <see cref="SmtpClient"/>.</param>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task SendEmailAsync<TModel>(TModel model, string sender) where TModel : EmailModel
        {
            var body = await m_serviceProvider.GetService<IViewToStringRenderer>().RenderViewToStringAsync(model.EmailView, model);

            var senderInfo = m_options.Senders[sender];

            MailMessage mailMessage = new MailMessage(new MailAddress(senderInfo.Email, senderInfo.Name), new MailAddress(model.To))
            {
                Subject = model.Subject,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Body = body
            };

            await GetClient(senderInfo).SendMailAsync(mailMessage);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns a <see cref="SmtpClient"/>.
        /// </summary>
        /// <returns><see cref="SmtpClient"/> object.</returns>
        private System.Net.Mail.SmtpClient GetClient(SenderInformation sender)
        {
            return new System.Net.Mail.SmtpClient()
            {
                Port = m_options.Port,
                Host = m_options.Host,
                EnableSsl = m_options.EnableSsl,
                Timeout = m_options.Timeout,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(sender.Email, sender.Password)
            };
        }

        #endregion
    }

    /// <summary>
    /// Contains extension methods for <see cref="SmtpEmailer"/>.
    /// </summary>
    public static class EmailExtension
    {
        /// <summary>
        /// Adds <see cref="IEmailer"/> service to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for <see cref="IEmailer"/> service.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddSmtpEmailer(this IServiceCollection services, Action<SmtpOptions> options)
        {
            services.Configure(options);
            services.AddTransient<IEmailer, SmtpEmailer>();
            return services;
        }

        /// <summary>
        /// Adds <see cref="IEmailer"/> service to the service collection. This method assumes you have added configuration in the appsettings.json.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddSmtpEmailer(this IServiceCollection services, IConfiguration configuration)
        {            
            SmtpOptions options = new SmtpOptions();
            void configureOptions(SmtpOptions o) => configuration.GetSection("SmtpSettings").Bind(o);
            services.Configure((Action<SmtpOptions>)configureOptions);
            services.AddTransient<IEmailer, SmtpEmailer>();
            return services;
        }
    }
}
