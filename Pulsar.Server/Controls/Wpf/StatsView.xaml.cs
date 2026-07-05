using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using LiveChartsCore.SkiaSharpView.WPF;
using Pulsar.Server.Statistics;

#nullable enable

namespace Pulsar.Server.Controls.Wpf
{
    public partial class StatsView : UserControl
    {
        private readonly StatsViewModel _viewModel;
        private readonly CartesianChart _newClientsChart;
        private readonly PieChart _countryChart;
        private readonly PieChart _operatingSystemChart;

        public StatsView()
        {
            InitializeComponent();
            _viewModel = new StatsViewModel();
            DataContext = _viewModel;

            Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            _newClientsChart = CreateCartesianChart();
            _countryChart = CreatePieChart();
            _operatingSystemChart = CreatePieChart();

            NewClientsChartHost.Content = _newClientsChart;
            CountryChartHost.Content = _countryChart;
            OperatingSystemChartHost.Content = _operatingSystemChart;

            Bind(_newClientsChart, CartesianChart.SeriesProperty, nameof(StatsViewModel.NewClientsSeries));
            Bind(_newClientsChart, CartesianChart.XAxesProperty, nameof(StatsViewModel.NewClientsXAxes));
            Bind(_newClientsChart, CartesianChart.YAxesProperty, nameof(StatsViewModel.NewClientsYAxes));

            Bind(_countryChart, PieChart.SeriesProperty, nameof(StatsViewModel.ClientsByCountrySeries));
            Bind(_operatingSystemChart, PieChart.SeriesProperty, nameof(StatsViewModel.ClientsByOperatingSystemSeries));
        }

        private void OnDispatcherUnhandledException(object? sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is NullReferenceException &&
                e.Exception.StackTrace?.Contains("LiveChartsCore.SkiaSharpView.WPF.Rendering.CompositionTargetTicker.DisposeTicker", StringComparison.Ordinal) == true)
            {
                e.Handled = true;
            }
        }

        public void ShowLoading()
        {
            Dispatcher.Invoke(() => _viewModel.SetLoading());
        }

        public void ShowError(string message)
        {
            Dispatcher.Invoke(() => _viewModel.SetError(message));
        }

        public void UpdateSnapshot(ClientStatisticsSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            Dispatcher.Invoke(() => _viewModel.UpdateSnapshot(snapshot));
        }

        public void ApplyTheme(bool isDarkMode)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateBrush("StatsBackgroundBrush", isDarkMode ? "#FF1A1A1A" : "#FFFFFFFF");
                UpdateBrush("CardBackgroundBrush", isDarkMode ? "#FF222327" : "#FFF5F5F5");
                UpdateBrush("CardBorderBrush", isDarkMode ? "#FF2E3136" : "#FFE0E0E0");
                UpdateBrush("CardForegroundBrush", isDarkMode ? "#FFE8EAED" : "#FF1F1F1F");
                UpdateBrush("MutedTextBrush", isDarkMode ? "#FF9AA0A6" : "#FF5F6368");
                UpdateBrush("AccentBrush", isDarkMode ? "#FF64B5F6" : "#FF1976D2");
                UpdateBrush("PositiveAccentBrush", isDarkMode ? "#FF81C784" : "#FF2E7D32");
                UpdateBrush("NegativeAccentBrush", isDarkMode ? "#FFEF5350" : "#FFC62828");
                UpdateBrush("SectionHeaderBrush", isDarkMode ? "#FF64B5F6" : "#FF1976D2");
                UpdateBrush("ChartBackgroundBrush", isDarkMode ? "#FF1E1F23" : "#FFFFFFFF");
                UpdateBrush("ChartBorderBrush", isDarkMode ? "#FF2F3338" : "#FFE0E0E0");
                UpdateBrush("ScrollBarTrackBrush", isDarkMode ? "#FF1E1E1E" : "#FFE5E5E5");
                UpdateBrush("ScrollBarThumbBrush", isDarkMode ? "#FF444444" : "#FFB5B5B5");
                UpdateBrush("ScrollBarThumbHoverBrush", isDarkMode ? "#FF5A5A5A" : "#FF9E9E9E");
                UpdateBrush("ScrollBarThumbPressedBrush", isDarkMode ? "#FF737373" : "#FF7C7C7C");

                LayoutRoot.Background = (Brush)Resources["StatsBackgroundBrush"];
                ApplyChartTheme();
                _viewModel.UpdateTheme(isDarkMode);
            });
        }

        private void UpdateBrush(string resourceKey, string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex)!;
            if (Resources[resourceKey] is SolidColorBrush brush)
            {
                if (!brush.IsFrozen)
                {
                    brush.Color = color;
                }
                else
                {
                    var mutable = brush.Clone();
                    mutable.Color = color;
                    Resources[resourceKey] = mutable;
                }
            }
            else
            {
                Resources[resourceKey] = new SolidColorBrush(color);
            }
        }

        private void ApplyChartTheme()
        {
            if (Resources["ChartBackgroundBrush"] is not SolidColorBrush chartBackgroundBrush)
            {
                return;
            }

            if (Resources["ChartBorderBrush"] is not SolidColorBrush chartBorderBrush)
            {
                return;
            }

            _newClientsChart.Background = chartBackgroundBrush;
            _countryChart.Background = chartBackgroundBrush;
            _operatingSystemChart.Background = chartBackgroundBrush;

            _newClientsChart.BorderBrush = chartBorderBrush;
            _countryChart.BorderBrush = chartBorderBrush;
            _operatingSystemChart.BorderBrush = chartBorderBrush;

            var borderThickness = new Thickness(1);
            _newClientsChart.BorderThickness = borderThickness;
            _countryChart.BorderThickness = borderThickness;
            _operatingSystemChart.BorderThickness = borderThickness;
        }

        private static CartesianChart CreateCartesianChart()
        {
            return new CartesianChart
            {
                Height = 240,
                Padding = new Thickness(8)
            };
        }

        private static PieChart CreatePieChart()
        {
            return new PieChart
            {
                Height = 160,
                Padding = new Thickness(8)
            };
        }

        private static Binding CreateOneWayBinding(string path)
        {
            return new Binding(path)
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
        }

        private static void Bind(FrameworkElement element, DependencyProperty property, string path)
        {
            element.SetBinding(property, CreateOneWayBinding(path));
        }
    }
}
