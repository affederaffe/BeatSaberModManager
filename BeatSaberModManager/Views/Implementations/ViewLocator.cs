using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BeatSaberModManager.ViewModels;

using ReactiveUI;


namespace BeatSaberModManager.Views.Implementations
{
    public class ViewLocator : IDataTemplate
    {
        private readonly IServiceProvider _services;

        public ViewLocator(IServiceProvider services)
        {
            _services = services;
        }

        public IControl Build(object param)
        {
            Type viewModelType = param.GetType();
            Type requestedType = typeof(IViewFor<>).MakeGenericType(viewModelType);
            return _services.GetService(requestedType) as IControl ?? new TextBlock { Text = $"View for {viewModelType.Name} not found." };
        }

        public bool Match(object data) => data is ViewModelBase;
    }
}