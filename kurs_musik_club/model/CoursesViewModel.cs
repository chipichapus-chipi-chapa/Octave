using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

public class Service
{
    public string Title { get; set; }
    public string Description { get; set; }
    // Другие свойства услуги
}

public class RecordingViewModel : INotifyPropertyChanged
{
    private ObservableCollection<Service> _services;
    public ObservableCollection<Service> Services
    {
        get => _services;
        set
        {
            _services = value;
            OnPropertyChanged(nameof(Services));
        }
    }

    public ICommand SelectServiceCommand { get; }

    public RecordingViewModel()
    {
        SelectServiceCommand = new RelayCommand(OnSelectService);
        LoadServices();
    }

    private void OnSelectService(object parameter)
    {
        if (parameter is Service selectedService)
        {
            // Обработка выбора услуги
            // Например, открытие окна заказа
        }
    }

    private async void LoadServices()
    {
        // Загрузка данных из БД
     /*   using (var context = new LoadServices())
        {
            var services = await context.Services.ToListAsync();
            Services = new ObservableCollection<Service>(services.Select(s => new Service
            {
                Title = s.Name,
                Description = s.Description
                // Другие свойства
            }));
        }*/
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// RelayCommand для обработки команд
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

    public void Execute(object parameter) => _execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}