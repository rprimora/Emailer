using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Emailer.Abstractions;
using ViewRenderer.Abstractions;

namespace Emailer.SendGrid
{
    /// <summary>
    /// Email service.
    /// </summary>
    public class SendGridEmailer : IEmailer
    {
        #region Members

        private readonly SendGridOptions m_options;
        private readonly IServiceProvider m_serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Emailer"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="options">Options.</param>
        public SendGridEmailer(IServiceProvider serviceProvider, IOptions<SendGridOptions> options)
        {
            m_serviceProvider = serviceProvider;
            m_options = options.Value;
        }

        #endregion

        #region IEmail implementation

        /// <summary>
        /// Asynchronously sends an email to the default sender defined in the configuration.
        /// </summary>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task SendEmailAsync<TModel>(TModel model) where TModel : EmailModel
        {
            var body = await m_serviceProvider.GetService<IViewToStringRenderer>().RenderViewToStringAsync(model.EmailView, model);

            var senderInfo = m_options.Senders["default"];

            var message = new SendGridMessage()
            {
                From = new EmailAddress(senderInfo.Email, senderInfo.Name),
                HtmlContent = body,
                Subject = model.Subject
            };

            message.AddTo(new EmailAddress(model.To));

            await GetClient().SendEmailAsync(message);
        }

        /// <summary>
        /// Asynchronously sends an email to the given sender.
        /// </summary>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <param name="sender">Sender. Must be present in the configuration.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task SendEmailAsync<TModel>(TModel model, string sender) where TModel : EmailModel
        {
            var body = await m_serviceProvider.GetService<IViewToStringRenderer>().RenderViewToStringAsync(model.EmailView, model);

            var senderInfo = m_options.Senders[sender];

            var message = new SendGridMessage()
            {
                From = new EmailAddress(senderInfo.Email, senderInfo.Name),
                HtmlContent = body,
                Subject = model.Subject
            };

            message.AddTo(new EmailAddress(model.To));

            await GetClient().SendEmailAsync(message);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns a <see cref="SendGridClient"/>.
        /// </summary>
        /// <returns><see cref="SendGridClient"/> object.</returns>
        private SendGridClient GetClient()
        {
            return new SendGridClient(m_options.APIKey);
        }

        #endregion
    }

    /// <summary>
    /// Contains extension methods for <see cref="Emailer"/>.
    /// </summary>
    public static class EmailExtension
    {
        /// <summary>
        /// Adds <see cref="IEmailer"/> service to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for <see cref="IEmailer"/> service.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddSendGridEmailer(this IServiceCollection services, Action<SendGridOptions> options)
        {
            services.Configure(options);
            services.AddTransient<IEmailer, SendGridEmailer>();
            return services;
        }

        /// <summary>
        /// Adds <see cref="IEmailer"/> service to the service collection. This method assumes you have added configuration in the appsettings.json.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddSendGridEmailer(this IServiceCollection services, IConfiguration configuration)
        {
            SendGridOptions options = new SendGridOptions();
            void configureOptions(SendGridOptions o) => configuration.GetSection("SendGridSettings").Bind(o);
            services.Configure((Action<SendGridOptions>)configureOptions);
            services.AddTransient<IEmailer, SendGridEmailer>();
            return services;
        }
    }
}
