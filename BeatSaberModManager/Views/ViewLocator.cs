using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BeatSaberModManager.ViewModels;

using ReactiveUI;


namespace BeatSaberModManager.Views
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
            Type viewType = typeof(IViewFor<>).MakeGenericType(viewModelType);
            return _services.GetService(viewType) as IControl ?? throw new InvalidOperationException();
        }

        public bool Match(object data) => data is ViewModelBase;
    }
}