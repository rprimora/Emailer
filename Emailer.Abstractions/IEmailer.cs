using System.Threading.Tasks;

namespace Emailer.Abstractions
{
    /// <summary>
    /// Describes the interface for sending emails.
    /// </summary>
    public interface IEmailer
    {
        /// <summary>
        /// Asynchronously sends an email.
        /// </summary>
        /// <param name="model">Email model.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendEmailAsync<TModel>(TModel model) where TModel : EmailModel;

        /// <summary>
        /// Asynchronously sends an email as given sender.
        /// </summary>
        /// <param name="model">Email model.</param>
        /// <param name="sender">Sender.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendEmailAsync<TModel>(TModel model, string sender) where TModel : EmailModel;
    }
}
