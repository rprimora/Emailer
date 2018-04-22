using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Emailer
{
    /// <summary>
    /// Describes an email service used to send emails.
    /// </summary>
    public interface IEmailer
    {
        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendEmailAsync<TModel>(TModel model) where TModel : EmailModel;

        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <param name="client">Function that returns <see cref="SmtpClient"/>.</param>
        /// <typeparam name="TModel">Type of model.</typeparam>
        /// <param name="model">Model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendEmailAsync<TModel>(Func<SmtpClient> client, TModel model) where TModel : EmailModel;
    }
}