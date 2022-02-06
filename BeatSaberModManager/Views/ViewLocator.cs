using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BeatSaberModManager.ViewModels;

using ReactiveUI;


namespace BeatSaberModManager.Views
{
    /// <summary>
    /// Provides a mechanism to map ViewModels to their respective Views.
    /// </summary>
    public class ViewLocator : IDataTemplate
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewLocator"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceProvider"/> used to resolve the Views.</param>
        public ViewLocator(IServiceProvider services)
        {
            _services = services;
        }

        /// <inheritdoc />
        public IControl Build(object param)
        {
            Type viewModelType = param.GetType();
            Type viewType = typeof(IViewFor<>).MakeGenericType(viewModelType);
            return _services.GetService(viewType) as IControl ?? throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public bool Match(object data) => data is ViewModelBase;
    }
}