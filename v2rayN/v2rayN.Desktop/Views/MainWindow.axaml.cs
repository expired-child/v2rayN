using Avalonia.Controls.Notifications;
using DialogHostAvalonia;
using v2rayN.Desktop.Base;
using v2rayN.Desktop.Common;
using v2rayN.Desktop.Manager;

namespace v2rayN.Desktop.Views;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    private static Config _config;
    private readonly SingleReplaceableDisposable _layoutBindingsDisposable = new();
    private readonly WindowNotificationManager? _manager;
    private readonly WorkspaceNavigation _workspaceNavigation = new();
    private bool _updatingNavigation;
    private bool _rootOverview = true;
    private bool _blCloseByUser = false;

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
        _manager = new WindowNotificationManager(TopLevel.GetTopLevel(this)) { MaxItems = 3, Position = NotificationPosition.TopRight };

        KeyDown += MainWindow_KeyDown;
        menuSettingsSetUWP.Click += MenuSettingsSetUWP_Click;
        menuCheckUpdate.Click += MenuCheckUpdate_Click;
        btnNewUpdate.Click += MenuCheckUpdate_Click;
        menuBackupAndRestore.Click += MenuBackupAndRestore_Click;

        conTheme.Content ??= new ThemeSettingView();

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
            this.OneWayBind(ViewModel, vm => vm.BlNewUpdate, v => v.btnNewUpdate.IsVisible).DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.StatusBarViewModel, v => v.contentStatusBarView.Content).DisposeWith(disposables);

            _layoutBindingsDisposable.DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel.MainGirdOrientation)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(UpdateLayout)
                .DisposeWith(disposables);

            ViewModel.ReadTextFromClipboardInteraction.RegisterHandler(async interaction =>
            {
                var result = await AvaUtils.GetClipboardData(this);
                interaction.SetOutput(result);
            }).DisposeWith(disposables);

            ViewModel.ScanScreenInteraction.RegisterHandler(async interaction =>
            {
                ShowHideWindow(false);
                await Task.Delay(200);
                var result = QRCodeAvaloniaUtils.CaptureScreen();
                ShowHideWindow(true);
                interaction.SetOutput(result);
            }).DisposeWith(disposables);

            ViewModel.BrowseImageFileInteraction.RegisterHandler(async interaction =>
            {
                var result = await UI.OpenFileDialog(null);
                interaction.SetOutput(result);
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

        if (Utils.IsWindows())
        {
            Title = $"{Utils.GetVersion()} - {(Utils.IsAdministrator() ? ResUI.RunAsAdmin : ResUI.NotRunAsAdmin)}";

            if (!Design.IsDesignMode)
            {
                ThreadPool.RegisterWaitForSingleObject(Program.ProgramStarted, OnProgramStarted, null, -1, false);
                HotkeyManager.Instance.Init(_config, OnHotkeyHandler);
            }
        }
        else
        {
            Title = $"{Utils.GetVersion()}";
            menuAddServerViaScan.IsVisible = false;
        }

        if (_config.UiItem.AutoHideStartup && Utils.IsWindows())
        {
            WindowState = WindowState.Minimized;
        }

        AddHelpMenuItem();
    }

    #region Event

    private void WorkspaceNavigation_Changed(object? sender, RoutedEventArgs e)
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


    private async void SubscriptionsNavigation_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null) await NavigateRootAsync(typeof(SubSettingWindow), ViewModel.SubSettingAsync);
    }

    private async void RoutingNavigation_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null) await NavigateRootAsync(typeof(RoutingSettingWindow), ViewModel.RoutingSettingAsync);
    }

    private async void SettingsNavigation_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null) await NavigateRootAsync(typeof(OptionSettingWindow), ViewModel.OptionSettingAsync);
    }

    private async Task NavigateRootAsync(Type pageType, Func<Task> open)
    {
        if (_workspaceNavigation.ReturnTo(pageType) != null) return;
        if (_workspaceNavigation.CloseAll()) await open();
    }

    private void PageBack_Click(object? sender, RoutedEventArgs e) => _workspaceNavigation.Back();

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
        btnNavSubscriptions.Classes.Set("active", subscriptions);
        btnNavRouting.Classes.Set("active", routing);
        btnQuickSettings.Classes.Set("active", settings);
        pagePanel.IsVisible = page != null;
        btnPageBack.IsVisible = page != null && !(ReferenceEquals(page, _workspaceNavigation.Root) && page is SubSettingWindow or RoutingSettingWindow or OptionSettingWindow);
        if (page != null)
        {
            dashboardPanel.IsVisible = false;
            workspacePanel.IsVisible = false;
            txtWorkspaceTitle.Text = page.Title;
            txtWorkspaceDescription.Text = ResUI.TbWorkspaceDescription;
            return;
        }
        var overview = _rootOverview;
        dashboardPanel.IsVisible = overview;
        workspacePanel.IsVisible = !overview;
        txtWorkspaceTitle.Text = overview ? ResUI.TbOverview : ResUI.menuServers;
        txtWorkspaceDescription.Text = overview ? ResUI.TbOverviewDescription : ResUI.TbWorkspaceDescription;
    }

    private void OnProgramStarted(object state, bool timeout)
    {
        Dispatcher.UIThread.Post(() =>
                ShowHideWindow(true),
            DispatcherPriority.Default);
    }

    private async Task DelegateSnackMsg(string content)
    {
        _manager?.Show(new Avalonia.Controls.Notifications.Notification(null, content, NotificationType.Information));
        await Task.CompletedTask;
    }

    private void OnHotkeyHandler(EGlobalHotkey e)
    {
        switch (e)
        {
            case EGlobalHotkey.ShowForm:
                Dispatcher.UIThread.Post(() => ShowHideWindow(null));
                break;

            case EGlobalHotkey.SystemProxyClear:
            case EGlobalHotkey.SystemProxySet:
            case EGlobalHotkey.SystemProxyUnchanged:
            case EGlobalHotkey.SystemProxyPac:
                AppEvents.SysProxyChangeRequested.Publish((ESysProxyType)((int)e - 1));
                break;
        }
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (_blCloseByUser)
        {
            return;
        }

        Logging.SaveLog("OnClosing -> " + e.CloseReason.ToString());

        switch (e.CloseReason)
        {
            case WindowCloseReason.OwnerWindowClosing or WindowCloseReason.WindowClosing:
                e.Cancel = true;
                ShowHideWindow(false);
                break;

            case WindowCloseReason.ApplicationShutdown or WindowCloseReason.OSShutdown:
                await AppManager.Instance.AppExitAsync(false);
                break;
        }

        base.OnClosing(e);
    }

    private async void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta)
        {
            switch (e.Key)
            {
                case Key.V:
                    await AddServerViaClipboardAsync();
                    break;

                case Key.S:
                    await ScanScreenTaskAsync();
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

    private void MenuPromotion_Click(object? sender, RoutedEventArgs e)
    {
        ProcUtils.ProcessStart($"{Utils.Base64Decode(Global.PromotionUrl)}?t={DateTime.Now.Ticks}");
    }

    private void MenuSettingsSetUWP_Click(object? sender, RoutedEventArgs e)
    {
        ProcUtils.ProcessStart(Utils.GetBinPath("EnableLoopback.exe"));
    }

    public async Task AddServerViaClipboardAsync()
    {
        var clipboardData = await AvaUtils.GetClipboardData(this);
        if (ViewModel != null)
        {
            await ViewModel.AddServerViaClipboardAsync(clipboardData ?? string.Empty);
        }
    }

    public async Task ScanScreenTaskAsync()
    {
        ShowHideWindow(false);

        await Task.Delay(200);

        var bytes = QRCodeAvaloniaUtils.CaptureScreen();
        if (bytes != null && ViewModel != null)
        {
            await ViewModel.ScanScreenResult(bytes);
        }

        ShowHideWindow(true);
    }

    private void MenuCheckUpdate_Click(object? sender, RoutedEventArgs e)
    {
        var content = new CheckUpdateView { ViewModel = ViewModel?.CheckUpdateViewModel };
        _ = ShowWorkspacePageAsync(new PageBase<object> { Title = ResUI.menuCheckUpdate, Content = content });

        AppEvents.HasUpdateNotified.Publish(false);
    }

    private void MenuBackupAndRestore_Click(object? sender, RoutedEventArgs e)
    {
        var content = new BackupAndRestoreView { ViewModel = ViewModel?.BackupAndRestoreViewModel };
        _ = ShowWorkspacePageAsync(new PageBase<object> { Title = ResUI.menuBackupAndRestore, Content = content });
    }

    private async void MenuClose_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (await UI.ShowYesNo(ResUI.menuExitTips) != ButtonResult.Yes)
            {
                return;
            }

            _blCloseByUser = true;
            StorageUI();

            await AppManager.Instance.AppExitAsync(true);
        }
        catch
        {
            // Ignore
        }
    }

    private void Shutdown(bool obj)
    {
        if (obj is bool b && _blCloseByUser == false)
        {
            _blCloseByUser = b;
        }
        StorageUI();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            HotkeyManager.Instance.Dispose();
            desktop.Shutdown();
        }
    }

    #endregion Event

    #region UI

    public void ShowHideWindow(bool? blShow)
    {
        var bl = blShow ??
                    (Utils.IsLinux() || Utils.IsMacOS()
                    ? (!AppManager.Instance.ShowInTaskbar ^ (WindowState == WindowState.Minimized))
                    : !AppManager.Instance.ShowInTaskbar);
        if (bl)
        {
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
            Focus();
        }
        else
        {
            if (Utils.IsLinux() && _config.UiItem.Hide2TrayWhenClose == false)
            {
                WindowState = WindowState.Minimized;
                return;
            }

            foreach (var ownedWindow in OwnedWindows)
            {
                ownedWindow.Close();
            }
            Hide();
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

        ClearLayoutContent();

        gridMain.IsVisible = orientation == EGirdOrientation.Horizontal;
        gridMain1.IsVisible = orientation == EGirdOrientation.Vertical;
        gridMain2.IsVisible = orientation == EGirdOrientation.Tab;

        switch (orientation)
        {
            case EGirdOrientation.Horizontal:
                this.OneWayBind(ViewModel, vm => vm.ProfilesViewModel, v => v.tabProfiles.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.MsgViewModel, v => v.tabMsgView.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ClashProxiesViewModel, v => v.tabClashProxies.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ClashConnectionsViewModel, v => v.tabClashConnections.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashProxies.IsVisible).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashConnections.IsVisible).DisposeWith(currentLayoutDisposables);
                this.Bind(ViewModel, vm => vm.TabMainSelectedIndex, v => v.tabMain.SelectedIndex).DisposeWith(currentLayoutDisposables);
                break;

            case EGirdOrientation.Vertical:
                this.OneWayBind(ViewModel, vm => vm.ProfilesViewModel, v => v.tabProfiles1.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.MsgViewModel, v => v.tabMsgView1.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ClashProxiesViewModel, v => v.tabClashProxies1.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ClashConnectionsViewModel, v => v.tabClashConnections1.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashProxies1.IsVisible).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashConnections1.IsVisible).DisposeWith(currentLayoutDisposables);
                this.Bind(ViewModel, vm => vm.TabMainSelectedIndex, v => v.tabMain1.SelectedIndex).DisposeWith(currentLayoutDisposables);
                break;

            case EGirdOrientation.Tab:
            default:
                this.OneWayBind(ViewModel, vm => vm.ProfilesViewModel, v => v.tabProfiles2.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.MsgViewModel, v => v.tabMsgView2.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ClashProxiesViewModel, v => v.tabClashProxies2.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ClashConnectionsViewModel, v => v.tabClashConnections2.Content).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashProxies2.IsVisible).DisposeWith(currentLayoutDisposables);
                this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashConnections2.IsVisible).DisposeWith(currentLayoutDisposables);
                this.Bind(ViewModel, vm => vm.TabMainSelectedIndex, v => v.tabMain2.SelectedIndex).DisposeWith(currentLayoutDisposables);
                break;
        }

        RestoreUI();
    }

    private void ClearLayoutContent()
    {
        tabProfiles.Content = null;
        tabMsgView.Content = null;
        tabClashProxies.Content = null;
        tabClashConnections.Content = null;

        tabProfiles1.Content = null;
        tabMsgView1.Content = null;
        tabClashProxies1.Content = null;
        tabClashConnections1.Content = null;

        tabProfiles2.Content = null;
        tabMsgView2.Content = null;
        tabClashProxies2.Content = null;
        tabClashConnections2.Content = null;
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
                Tag = it.Url?.Replace(@"/releases", ""),
                Content = string.Format(ResUI.menuWebsiteItem, it.CoreType.ToString().Replace("_", " ")).UpperFirstChar()
            };
            item.Classes.Add("workspaceSidebarAction");
            item.Click += MenuItem_Click;
            menuHelp.Children.Add(item);
        }
    }

    private void MenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button item)
        {
            ProcUtils.ProcessStart(item.Tag?.ToString());
        }
    }

    #endregion UI
}
