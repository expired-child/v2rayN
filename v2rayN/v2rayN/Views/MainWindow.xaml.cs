using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using v2rayN.Base;
using v2rayN.Manager;

namespace v2rayN.Views;

public partial class MainWindow
{
    private static Config _config;
    private readonly SingleReplaceableDisposable _layoutBindingsDisposable = new();
    private readonly WorkspaceNavigation _workspaceNavigation = new();
    private bool _updatingNavigation;
    private bool _rootOverview = true;

    public MainWindow()
    {
        InitializeComponent();
        _workspaceNavigation.Changed += RenderWorkspacePage;
        navOverview.IsChecked = true;
        btnNavSubscriptions.Click += SubscriptionsNavigation_Click;
        btnDashboardSubscriptions.Click += SubscriptionsNavigation_Click;
        btnDashboardProfiles.Click += (_, _) => navProfiles.IsChecked = true;
        btnNavRouting.Click += RoutingNavigation_Click;
        btnQuickSettings.Click += SettingsNavigation_Click;


        _config = AppManager.Instance.Config;
        ThreadPool.RegisterWaitForSingleObject(App.ProgramStarted, OnProgramStarted, null, -1, false);

        App.Current.SessionEnding += Current_SessionEnding;
        Closing += MainWindow_Closing;
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        menuSettingsSetUWP.Click += MenuSettingsSetUWP_Click;
        menuCheckUpdate.Click += MenuCheckUpdate_Click;
        btnNewUpdate.Click += MenuCheckUpdate_Click;
        menuBackupAndRestore.Click += MenuBackupAndRestore_Click;

        pbTheme.Content ??= new ThemeSettingView();

        this.WhenActivated(disposables =>
        {
            this.BindCommand(ViewModel, vm => vm.AddServerViaClipboardCmd, v => v.btnQuickImport).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubUpdateCmd, v => v.btnQuickUpdate).DisposeWith(disposables);


            this.OneWayBind(ViewModel, vm => vm.StatusBarViewModel.InboundDisplay, v => v.txtShellStatus.Text).DisposeWith(disposables);

            //servers
            this.BindCommand(ViewModel, vm => vm.AddVmessServerCmd, v => v.menuAddVmessServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddVlessServerCmd, v => v.menuAddVlessServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddShadowsocksServerCmd, v => v.menuAddShadowsocksServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddSocksServerCmd, v => v.menuAddSocksServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddHttpServerCmd, v => v.menuAddHttpServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddTrojanServerCmd, v => v.menuAddTrojanServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddHysteria2ServerCmd, v => v.menuAddHysteria2Server).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddTuicServerCmd, v => v.menuAddTuicServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddWireguardServerCmd, v => v.menuAddWireguardServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddAnytlsServerCmd, v => v.menuAddAnytlsServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddNaiveServerCmd, v => v.menuAddNaiveServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddMasqueServerCmd, v => v.menuAddMasqueServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddCustomServerCmd, v => v.menuAddCustomServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddCustomOutboundServerCmd, v => v.menuAddCustomOutboundServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddPolicyGroupServerCmd, v => v.menuAddPolicyGroupServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddProxyChainServerCmd, v => v.menuAddProxyChainServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddServerViaScanCmd, v => v.menuAddServerViaScan).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddServerViaImageCmd, v => v.menuAddServerViaImage).DisposeWith(disposables);

            //sub
            this.BindCommand(ViewModel, vm => vm.SubUpdateCmd, v => v.menuSubUpdate).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubUpdateViaProxyCmd, v => v.menuSubUpdateViaProxy).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubGroupUpdateCmd, v => v.menuSubGroupUpdate).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubGroupUpdateViaProxyCmd, v => v.menuSubGroupUpdateViaProxy).DisposeWith(disposables);

            //setting
            this.BindCommand(ViewModel, vm => vm.DNSSettingCmd, v => v.menuDNSSetting).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.FullConfigTemplateCmd, v => v.menuFullConfigTemplate).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.GlobalHotkeySettingCmd, v => v.menuGlobalHotkeySetting).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RebootAsAdminCmd, v => v.menuRebootAsAdmin).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.ClearServerStatisticsCmd, v => v.menuClearServerStatistics).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.OpenTheFileLocationCmd, v => v.menuOpenTheFileLocation).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RegionalPresetDefaultCmd, v => v.menuRegionalPresetsDefault).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RegionalPresetRussiaCmd, v => v.menuRegionalPresetsRussia).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RegionalPresetIranCmd, v => v.menuRegionalPresetsIran).DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.ReloadCmd, v => v.menuReload).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.BlReloadEnabled, v => v.menuReload.IsEnabled).DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.BlNewUpdate, v => v.btnNewUpdate.Visibility).DisposeWith(disposables);

            _layoutBindingsDisposable.DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel.MainGirdOrientation)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(UpdateLayout)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel.StatusBarViewModel)
                .Subscribe(vm => ViewHost.Show(contentStatusBarView, vm))
                .DisposeWith(disposables);

            ViewModel.ReadTextFromClipboardInteraction.RegisterHandler(interaction =>
            {
                var clipboardData = WindowsUtils.GetClipboardData();
                interaction.SetOutput(clipboardData);
            }).DisposeWith(disposables);

            ViewModel.ScanScreenInteraction.RegisterHandler(interaction =>
            {
                ShowHideWindow(false);
                if (Application.Current?.MainWindow is { } window)
                {
                    var bytes = QRCodeWindowsUtils.CaptureScreen(window);
                    interaction.SetOutput(bytes);
                }
                ShowHideWindow(true);
            }).DisposeWith(disposables);

            ViewModel.BrowseImageFileInteraction.RegisterHandler(interaction =>
            {
                if (UI.OpenFileDialog(out var fileName, "PNG|*.png|All|*.*") != true)
                {
                    interaction.SetOutput(null);
                    return;
                }
                interaction.SetOutput(fileName);
            }).DisposeWith(disposables);

            ViewModel.ShowHideWindowInteraction.RegisterHandler(interaction =>
            {
                ShowHideWindow(interaction.Input);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            AppEvents.SendSnackMsgRequested
              .AsObservable()
              .ObserveOn(RxSchedulers.MainThreadScheduler)
              .Subscribe(async content => await DelegateSnackMsg(content))
              .DisposeWith(disposables);

            AppEvents.AppExitRequested
              .AsObservable()
              .ObserveOn(RxSchedulers.MainThreadScheduler)
              .Subscribe(_ => StorageUI())
              .DisposeWith(disposables);

            AppEvents.ShutdownRequested
             .AsObservable()
             .ObserveOn(RxSchedulers.MainThreadScheduler)
             .Subscribe(Shutdown)
             .DisposeWith(disposables);
        });

        Title = $"{Utils.GetVersion()} - {(Utils.IsAdministrator() ? ResUI.RunAsAdmin : ResUI.NotRunAsAdmin)}";
        if (_config.UiItem.AutoHideStartup)
        {
            WindowState = WindowState.Minimized;
        }

        if (!_config.GuiItem.EnableHWA)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        }

        AddHelpMenuItem();
        WindowsManager.Instance.RegisterGlobalHotkey(_config, OnHotkeyHandler, null);
    }

    #region Event

    private void WorkspaceNavigation_Changed(object sender, RoutedEventArgs e)
    {
        if (_updatingNavigation || dashboardPanel == null || workspacePanel == null)
        {
            return;
        }
        _rootOverview = navOverview.IsChecked == true;
        if (!_workspaceNavigation.CloseAll())
        {
            return;
        }
        RenderWorkspacePage();
    }

    public Task<bool> ShowWorkspacePageAsync(IWorkspacePage page)
    {
        if (page is SubSettingWindow or RoutingSettingWindow or OptionSettingWindow
            && _workspaceNavigation.ReturnTo(page.GetType()) is { } existing)
        {
            return existing;
        }
        return _workspaceNavigation.OpenAsync(page);
    }


    private async void SubscriptionsNavigation_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null) await NavigateRootAsync(typeof(SubSettingWindow), ViewModel.SubSettingAsync);
    }

    private async void RoutingNavigation_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null) await NavigateRootAsync(typeof(RoutingSettingWindow), ViewModel.RoutingSettingAsync);
    }

    private async void SettingsNavigation_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null) await NavigateRootAsync(typeof(OptionSettingWindow), ViewModel.OptionSettingAsync);
    }

    private async Task NavigateRootAsync(Type pageType, Func<Task> open)
    {
        if (_workspaceNavigation.ReturnTo(pageType) != null) return;
        if (_workspaceNavigation.CloseAll()) await open();
    }

    private void PageBack_Click(object sender, RoutedEventArgs e) => _workspaceNavigation.Back();

    private void RenderWorkspacePage()
    {
        var page = _workspaceNavigation.Current;
        if (!ReferenceEquals(pageHost.Content, page)) pageHost.Content = page;
        var family = _workspaceNavigation.Root?.GetType().Name;
        var subscriptions = family is nameof(SubSettingWindow) or nameof(SubEditWindow);
        var routing = family is nameof(RoutingSettingWindow) or nameof(RoutingRuleSettingWindow) or nameof(RoutingRuleDetailsWindow);
        var profiles = family is nameof(AddServerWindow) or nameof(AddServer2Window) or nameof(AddGroupServerWindow) or nameof(ProfilesSelectWindow);
        var settings = page != null && !subscriptions && !routing && !profiles;
        _updatingNavigation = true;
        try
        {
            navOverview.IsChecked = page == null && _rootOverview;
            navProfiles.IsChecked = (page == null && !_rootOverview) || profiles;
        }
        finally { _updatingNavigation = false; }
        btnNavSubscriptions.Tag = subscriptions ? "active" : null;
        btnNavRouting.Tag = routing ? "active" : null;
        btnQuickSettings.Tag = settings ? "active" : null;
        pagePanel.Visibility = page == null ? Visibility.Collapsed : Visibility.Visible;
        btnPageBack.Visibility = page != null && !(ReferenceEquals(page, _workspaceNavigation.Root) && page is SubSettingWindow or RoutingSettingWindow or OptionSettingWindow) ? Visibility.Visible : Visibility.Collapsed;
        if (page != null)
        {
            dashboardPanel.Visibility = Visibility.Collapsed;
            workspacePanel.Visibility = Visibility.Collapsed;
            txtWorkspaceTitle.Text = page.Title;
            txtWorkspaceDescription.Text = ResUI.TbWorkspaceDescription;
            return;
        }
        var overview = _rootOverview;
        dashboardPanel.Visibility = overview ? Visibility.Visible : Visibility.Collapsed;
        workspacePanel.Visibility = overview ? Visibility.Collapsed : Visibility.Visible;
        txtWorkspaceTitle.Text = overview ? ResUI.TbOverview : ResUI.menuServers;
        txtWorkspaceDescription.Text = overview ? ResUI.TbOverviewDescription : ResUI.TbWorkspaceDescription;
    }

    private void OnProgramStarted(object state, bool timeout)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            ShowHideWindow(true);
        });
    }

    private async Task DelegateSnackMsg(string content)
    {
        MainSnackbar.MessageQueue?.Enqueue(content);
        await Task.CompletedTask;
    }

    private void OnHotkeyHandler(EGlobalHotkey e)
    {
        switch (e)
        {
            case EGlobalHotkey.ShowForm:
                ShowHideWindow(null);
                break;

            case EGlobalHotkey.SystemProxyClear:
            case EGlobalHotkey.SystemProxySet:
            case EGlobalHotkey.SystemProxyUnchanged:
            case EGlobalHotkey.SystemProxyPac:
                AppEvents.SysProxyChangeRequested.Publish((ESysProxyType)((int)e - 1));
                break;
        }
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        ShowHideWindow(false);
    }

    private async void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        Logging.SaveLog("Current_SessionEnding");
        StorageUI();
        await AppManager.Instance.AppExitAsync(false);
    }

    private void Shutdown(bool obj)
    {
        Application.Current.Shutdown();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            switch (e.Key)
            {
                case Key.V:
                    if (Keyboard.FocusedElement is TextBox)
                    {
                        return;
                    }
                    AddServerViaClipboardAsync().ContinueWith(_ => { });

                    break;

                case Key.S:
                    ScanScreenTaskAsync().ContinueWith(_ => { });
                    break;
            }
        }
        else
        {
            if (e.Key == Key.F5)
            {
                ViewModel?.Reload();
            }
        }
    }

    private void MenuClose_Click(object sender, RoutedEventArgs e)
    {
        StorageUI();
        ShowHideWindow(false);
    }

    private void MenuPromotion_Click(object sender, RoutedEventArgs e)
    {
        ProcUtils.ProcessStart($"{Utils.Base64Decode(Global.PromotionUrl)}?t={DateTime.Now.Ticks}");
    }

    private void MenuSettingsSetUWP_Click(object sender, RoutedEventArgs e)
    {
        ProcUtils.ProcessStart(Utils.GetBinPath("EnableLoopback.exe"));
    }

    public async Task AddServerViaClipboardAsync()
    {
        var clipboardData = WindowsUtils.GetClipboardData();
        if (ViewModel != null)
        {
            await ViewModel.AddServerViaClipboardAsync(clipboardData ?? string.Empty);
        }
    }

    private async Task ScanScreenTaskAsync()
    {
        ShowHideWindow(false);

        if (Application.Current?.MainWindow is Window window)
        {
            var bytes = QRCodeWindowsUtils.CaptureScreen(window);
            await ViewModel?.ScanScreenResult(bytes);
        }

        ShowHideWindow(true);
    }

    private void MenuCheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        var content = new CheckUpdateView { ViewModel = ViewModel?.CheckUpdateViewModel };
        _ = ShowWorkspacePageAsync(new PageBase<object> { Title = ResUI.menuCheckUpdate, Content = content });

        AppEvents.HasUpdateNotified.Publish(false);
    }

    private void MenuBackupAndRestore_Click(object sender, RoutedEventArgs e)
    {
        var content = new BackupAndRestoreView { ViewModel = ViewModel?.BackupAndRestoreViewModel };
        _ = ShowWorkspacePageAsync(new PageBase<object> { Title = ResUI.menuBackupAndRestore, Content = content });
    }

    #endregion Event

    #region UI

    public void ShowHideWindow(bool? blShow)
    {
        var bl = blShow ?? !AppManager.Instance.ShowInTaskbar;
        if (bl)
        {
            this?.Show();
            if (this?.WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            this?.Activate();
            this?.Focus();
        }
        else
        {
            this?.Hide();
        }
        AppManager.Instance.ShowInTaskbar = bl;
    }

    protected override void OnLoaded(object? sender, RoutedEventArgs e)
    {
        base.OnLoaded(sender, e);
        if (_config.UiItem.AutoHideStartup)
        {
            ShowHideWindow(false);
        }
        RestoreUI();
    }

    private void RestoreUI()
    {
        if (_config.UiItem.MainGirdHeight1 > 0 && _config.UiItem.MainGirdHeight2 > 0)
        {
            if (_config.UiItem.MainGirdOrientation == EGirdOrientation.Horizontal)
            {
                gridMain.ColumnDefinitions[0].Width = new GridLength(_config.UiItem.MainGirdHeight1, GridUnitType.Star);
                gridMain.ColumnDefinitions[2].Width = new GridLength(_config.UiItem.MainGirdHeight2, GridUnitType.Star);
            }
            else if (_config.UiItem.MainGirdOrientation == EGirdOrientation.Vertical)
            {
                gridMain1.RowDefinitions[0].Height = new GridLength(_config.UiItem.MainGirdHeight1, GridUnitType.Star);
                gridMain1.RowDefinitions[2].Height = new GridLength(_config.UiItem.MainGirdHeight2, GridUnitType.Star);
            }
        }
    }

    private void StorageUI()
    {
        ConfigHandler.SaveWindowSizeItem(_config, GetType().Name, Width, Height);

        if (_config.UiItem.MainGirdOrientation == EGirdOrientation.Horizontal)
        {
            ConfigHandler.SaveMainGirdHeight(_config, gridMain.ColumnDefinitions[0].ActualWidth, gridMain.ColumnDefinitions[2].ActualWidth);
        }
        else if (_config.UiItem.MainGirdOrientation == EGirdOrientation.Vertical)
        {
            ConfigHandler.SaveMainGirdHeight(_config, gridMain1.RowDefinitions[0].ActualHeight, gridMain1.RowDefinitions[2].ActualHeight);
        }
    }

    private void UpdateLayout(EGirdOrientation orientation)
    {
        var currentLayoutDisposables = new MultipleDisposable();
        _layoutBindingsDisposable.Create(currentLayoutDisposables);

        gridMain.Visibility = orientation == EGirdOrientation.Horizontal ? Visibility.Visible : Visibility.Collapsed;
        gridMain1.Visibility = orientation == EGirdOrientation.Vertical ? Visibility.Visible : Visibility.Collapsed;
        gridMain2.Visibility = orientation == EGirdOrientation.Tab ? Visibility.Visible : Visibility.Collapsed;

        switch (orientation)
        {
            case EGirdOrientation.Horizontal:
                this.WhenAnyValue(v => v.ViewModel.ProfilesViewModel)
                    .Subscribe(vm => ViewHost.Show(tabProfiles, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.MsgViewModel)
                    .Subscribe(vm => ViewHost.Show(tabMsgView, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.ClashProxiesViewModel)
                    .Subscribe(vm => ViewHost.Show(tabClashProxies, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.ClashConnectionsViewModel)
                    .Subscribe(vm => ViewHost.Show(tabClashConnections, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabMsgView.Visibility).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashProxies.Visibility).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashConnections.Visibility).DisposeWith(currentLayoutDisposables);
                this.Bind(ViewModel, vm => vm.TabMainSelectedIndex, v => v.tabMain.SelectedIndex).DisposeWith(currentLayoutDisposables);
                break;

            case EGirdOrientation.Vertical:
                this.WhenAnyValue(v => v.ViewModel.ProfilesViewModel)
                    .Subscribe(vm => ViewHost.Show(tabProfiles1, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.MsgViewModel)
                    .Subscribe(vm => ViewHost.Show(tabMsgView1, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.ClashProxiesViewModel)
                    .Subscribe(vm => ViewHost.Show(tabClashProxies1, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.ClashConnectionsViewModel)
                    .Subscribe(vm => ViewHost.Show(tabClashConnections1, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabMsgView1.Visibility).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashProxies1.Visibility).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashConnections1.Visibility).DisposeWith(currentLayoutDisposables);
                this.Bind(ViewModel, vm => vm.TabMainSelectedIndex, v => v.tabMain1.SelectedIndex).DisposeWith(currentLayoutDisposables);
                break;

            case EGirdOrientation.Tab:
            default:
                this.WhenAnyValue(v => v.ViewModel.ProfilesViewModel)
                    .Subscribe(vm => ViewHost.Show(tabProfiles2, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.MsgViewModel)
                    .Subscribe(vm => ViewHost.Show(tabMsgView2, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.ClashProxiesViewModel)
                    .Subscribe(vm => ViewHost.Show(tabClashProxies2, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.WhenAnyValue(v => v.ViewModel.ClashConnectionsViewModel)
                    .Subscribe(vm => ViewHost.Show(tabClashConnections2, vm))
                    .DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashProxies2.Visibility).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashConnections2.Visibility).DisposeWith(currentLayoutDisposables);
                this.Bind(ViewModel, vm => vm.TabMainSelectedIndex, v => v.tabMain2.SelectedIndex).DisposeWith(currentLayoutDisposables);
                break;
        }

        RestoreUI();
    }

    private void AddHelpMenuItem()
    {
        var coreInfo = CoreInfoManager.Instance.GetCoreInfo();
        foreach (var it in coreInfo
            .Where(t => t.CoreType is not ECoreType.v2fly
                        and not ECoreType.hysteria))
        {
            var item = new Button()
            {
                Tag = it.Url.Replace(@"/releases", ""),
                Content = string.Format(ResUI.menuWebsiteItem, it.CoreType.ToString().Replace("_", " ")).UpperFirstChar(),
                Style = (Style)FindResource("WorkspaceSidebarAction")
            };
            item.Click += MenuItem_Click;
            menuHelp.Children.Add(item);
        }
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button item)
        {
            ProcUtils.ProcessStart(item.Tag.ToString());
        }
    }

    #endregion UI
}
