using System.Windows.Controls;
using v2rayN.Manager;

namespace v2rayN.Views;

public partial class StatusBarView
{
    private static Config _config;

    public StatusBarView()
    {
        InitializeComponent();
        _config = AppManager.Instance.Config;

        menuExit.Click += menuExit_Click;
        menuRoutings.ItemContainerGenerator.StatusChanged += (_, _) => RefreshTraySelections();
        menuServers.ItemContainerGenerator.StatusChanged += (_, _) => RefreshTraySelections();
        txtRunningServerDisplay.PreviewMouseDown += txtRunningInfoDisplay_MouseDoubleClick;
        txtRunningInfoDisplay.PreviewMouseDown += txtRunningInfoDisplay_MouseDoubleClick;

        this.WhenActivated(disposables =>
        {
            //system proxy
            this.WhenAnyValue(v => v.ViewModel.BlSystemProxyClear, v => v.ViewModel.BlSystemProxySet,
                    v => v.ViewModel.BlSystemProxyNothing, v => v.ViewModel.BlSystemProxyPac)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(_ => RefreshProxyMenuChecks())
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SystemProxyClearCmd, v => v.menuSystemProxyClear).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SystemProxySetCmd, v => v.menuSystemProxySet).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SystemProxyNothingCmd, v => v.menuSystemProxyNothing).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SystemProxyPacCmd, v => v.menuSystemProxyPac).DisposeWith(disposables);

            //routings and servers
            this.OneWayBind(ViewModel, vm => vm.RoutingItems, v => v.menuRoutings.ItemsSource).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.BlRouting, v => v.menuRoutings.Visibility).DisposeWith(disposables);
            this.WhenAnyValue(v => v.ViewModel.BlRouting, v => v.ViewModel.BlServers, (routing, servers) => routing || servers)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(visible => sepRoutings.Visibility = visible ? Visibility.Visible : Visibility.Collapsed)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.Servers, v => v.menuServers.ItemsSource).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.BlServers, v => v.menuServers.Visibility).DisposeWith(disposables);
            this.WhenAnyValue(v => v.ViewModel.SelectedRouting, v => v.ViewModel.SelectedServer)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(_ => RefreshTraySelections())
                .DisposeWith(disposables);

            //tray menu
            this.BindCommand(ViewModel, vm => vm.ShowWindowCmd, v => v.menuShowWindow).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddServerViaClipboardCmd, v => v.menuAddServerViaClipboard2).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddServerViaScanCmd, v => v.menuAddServerViaScan2).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubUpdateCmd, v => v.menuSubUpdate2).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubUpdateViaProxyCmd, v => v.menuSubUpdateViaProxy2).DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.CopyProxyCmdToClipboardCmd, v => v.menuCopyProxyCmdToClipboard).DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.RunningServerToolTipText, v => v.tbNotify.ToolTipText).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.NotifyLeftClickCmd, v => v.tbNotify.LeftClickCommand).DisposeWith(disposables);

            //status bar
            this.OneWayBind(ViewModel, vm => vm.InboundDisplay, v => v.txtInboundDisplay.Text).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.InboundLanDisplay, v => v.txtInboundLanDisplay.Text).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.RunningServerDisplay, v => v.txtRunningServerDisplay.Text).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.RunningInfoDisplay, v => v.txtRunningInfoDisplay.Text).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SpeedProxyDisplay, v => v.txtSpeedProxyDisplay.Text).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.SpeedDirectDisplay, v => v.txtSpeedDirectDisplay.Text).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.EnableTun, v => v.togEnableTun.IsChecked).DisposeWith(disposables);

            this.Bind(ViewModel, vm => vm.SystemProxySelected, v => v.cmbSystemProxy.SelectedIndex).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.RoutingItems, v => v.cmbRoutings2.ItemsSource).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedRouting, v => v.cmbRoutings2.SelectedItem).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.BlRouting, v => v.cmbRoutings2.Visibility).DisposeWith(disposables);

            ViewModel.SetClipboardDataInteraction.RegisterHandler(interaction =>
            {
                var strData = interaction.Input;
                WindowsUtils.SetClipboardData(strData);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.DispatcherRefreshIconInteraction.RegisterHandler(interaction =>
            {
                Application.Current?.Dispatcher.Invoke(async () => await RefreshIcon(), DispatcherPriority.Normal);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);
        });

        _ = RefreshIcon();
    }

    private void RefreshProxyMenuChecks()
    {
        if (ViewModel == null) return;
        menuSystemProxyClear.IsChecked = ViewModel.BlSystemProxyClear;
        menuSystemProxySet.IsChecked = ViewModel.BlSystemProxySet;
        menuSystemProxyNothing.IsChecked = ViewModel.BlSystemProxyNothing;
        menuSystemProxyPac.IsChecked = ViewModel.BlSystemProxyPac;
    }

    private void ProxyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // Checked state comes from the applied mode, including when the current mode is clicked again.
        RefreshProxyMenuChecks();
    }

    private void RefreshTraySelections()
    {
        if (ViewModel == null) return;
        UpdateMenuSelection(menuRoutings, ViewModel.SelectedRouting);
        UpdateMenuSelection(menuServers, ViewModel.SelectedServer);
    }

    private static void UpdateMenuSelection(MenuItem parent, object? selected)
    {
        foreach (var item in parent.Items)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(item) is MenuItem container)
                container.IsChecked = ReferenceEquals(item, selected);
        }
    }

    private void SelectionMenu_Opened(object sender, RoutedEventArgs e) => RefreshTraySelections();

    private void RoutingMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && sender is MenuItem { DataContext: RoutingItem routing })
        {
            ViewModel.SelectedRouting = routing;
            RefreshTraySelections();
            e.Handled = true;
        }
    }

    private void ServerMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && sender is MenuItem { DataContext: ComboItem server })
        {
            ViewModel.SelectedServer = server;
            RefreshTraySelections();
            e.Handled = true;
        }
    }

    private async Task RefreshIcon()
    {
        tbNotify.Icon = await WindowsManager.Instance.GetNotifyIcon(_config);
        Application.Current.MainWindow?.Icon = WindowsManager.Instance.GetAppIcon(_config);
    }

    private async void menuExit_Click(object sender, RoutedEventArgs e)
    {
        tbNotify.Dispose();
        await AppManager.Instance.AppExitAsync(true);
    }

    private void txtRunningInfoDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ViewModel?.TestServerAvailability();
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.TestServerAvailability();
        }
    }
}
