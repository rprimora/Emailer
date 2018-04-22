using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Emailer
{
    /// <summary>
    /// Describes an interface to view render service.
    /// </summary>
    public interface IViewToStringRenderer
    {
        /// <summary>
        /// When implemented renders a given view to string.
        /// </summary>
        /// <typeparam name="TModel">View model type.</typeparam>
        /// <param name="name">View name.</param>
        /// <param name="model">Model.</param>
        /// <returns>String representation of the view.</returns>
        Task<string> RenderViewToString<TModel>(string name, TModel model);
    }

    /// <summary>
    /// Represents a view renderer that can output a view in to a string.
    /// </summary>
    public class ViewToStringRenderer : IViewToStringRenderer
    {
        #region Members

        private readonly IRazorViewEngine m_razorViewEngine;
        private readonly ITempDataProvider m_tempDataProvider;
        private readonly IServiceProvider m_serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ViewToStringRenderer"/> class.
        /// </summary>
        /// <param name="razorViewEngine">Razor view engine.</param>
        /// <param name="tempDataProvider">Temporary data provider.</param>
        /// <param name="serviceProvider">Service provider.</param>
        public ViewToStringRenderer(IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            m_razorViewEngine = razorViewEngine;
            m_tempDataProvider = tempDataProvider;
            m_serviceProvider = serviceProvider;
        }

        #endregion

        #region IViewToStringRenderer implementation

        /// <summary>
        /// Renders a view to string.
        /// </summary>
        /// <typeparam name="TModel">View model type.</typeparam>
        /// <param name="name">View name.</param>
        /// <param name="model">Model.</param>
        /// <returns>String representation of the view.</returns>
        public async Task<string> RenderViewToString<TModel>(string name, TModel model)
        {
            var actionContext = GetActionContext();

            var viewEngineResult = m_razorViewEngine.FindView(actionContext, name, false);

            if (!viewEngineResult.Success)
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));

            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(actionContext.HttpContext, m_tempDataProvider),
                    output,
                    new HtmlHelperOptions()
                );

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns the action context.
        /// </summary>
        /// <returns><see cref="ActionContext"/> object.</returns>
        private ActionContext GetActionContext()
        {
            return new ActionContext(new DefaultHttpContext() { RequestServices = m_serviceProvider }, new RouteData(), new ActionDescriptor());
        }

        #endregion
    }

    /// <summary>
    /// Contains extension methods for <see cref="ViewToStringRenderer"/>.
    /// </summary>
    public static class ViewToStringRendererExtensions
    {
        /// <summary>
        /// Adds <see cref="IViewToStringRenderer"/> service to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="rootPath">Root path of the application.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddRazorViewToStringRenderer(this IServiceCollection services, Action<ViewToStringRendererOptions> options)
        {
            ViewToStringRendererOptions viewToStringrendererOptions = new ViewToStringRendererOptions();
            options(viewToStringrendererOptions);
            services.Configure<RazorViewEngineOptions>(o => o.ViewLocationExpanders.Add(new ViewLocationExpander(viewToStringrendererOptions)));
            services.AddTransient<IViewToStringRenderer, ViewToStringRenderer>();
            return services;
        }
    }

    /// <summary>
    /// Options used to instantiate <see cref="ViewLocationExpander"/>.
    /// </summary>
    public class ViewToStringRendererOptions
    {
        /// <summary>
        /// Gets or sets the content root path.
        /// </summary>
        public string ContentRoot { get; set; }

        /// <summary>
        /// Gets or sets the name of the folder that contains email views. Default is 'Emails'.
        /// </summary>
        public string EmailsFolder { get; set; } = "Emails";
    }

    /// <summary>
    /// Represents a view location expander that the razor engine uses to determine the location of the view.
    /// </summary>
    public class ViewLocationExpander : IViewLocationExpander
    {
        #region Members

        private IEnumerable<string> m_directoryLocations;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ViewLocationExpander"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public ViewLocationExpander(ViewToStringRendererOptions options)
        {
            var root = Directory.GetCurrentDirectory();
            // Find all 'Emails' folders in the directory
            m_directoryLocations = Directory.GetDirectories(root, options.EmailsFolder, SearchOption.AllDirectories);
            // Only include the ones in the running directory, remove the root path (the view engine uses relative paths)
            // and append the file name
            m_directoryLocations = m_directoryLocations.Where(s => s.Contains(options.ContentRoot))
                                                    .Select(s => s.Replace(root, ""))
                                                    .Select(s => s.Insert(s.Length, "/{0}.cshtml"));

        }

        #endregion

        #region IViewLocationExpander implementation
        
        /// <summary>
        /// Invoked by a <see cref="Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine"/> to determine potential
        /// locations for a view.
        /// </summary>
        /// <param name="context">The <see cref="Microsoft.AspNetCore.Mvc.Razor.ViewLocationExpanderContext"/> for the current view location expansion operation.</param>
        /// <param name="viewLocations">The sequence of view locations to expand.</param>
        /// <returns>A list of expanded view locations.</returns>
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return m_directoryLocations.Union(viewLocations);
        }

        /// <summary>
        /// Invoked by a <see cref="Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine"/> to determine the
        /// values that would be consumed by this instance of <see cref="Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander"/>.
        /// The calculated values are used to determine if the view location has changed
        /// since the last time it was located.
        /// </summary>
        /// <param name="context">The <see cref="Microsoft.AspNetCore.Mvc.Razor.ViewLocationExpanderContext"/> for the current view location expansion operation.</param>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values["customviewlocation"] = nameof(ViewLocationExpander);
        }

        #endregion
    }
}