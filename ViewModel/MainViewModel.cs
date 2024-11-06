using AiDesignTool.LCommands;
using LObjects;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

public class MainViewModel : INotifyPropertyChanged
{
    #region No1
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
    private Profile _SelectedProfile;
    public Profile SelectedProfile
    {
        get { return _SelectedProfile; }
        set
        {
            if (_SelectedProfile != value)
            {
                _SelectedProfile = value;
                OnPropertyChanged(nameof(SelectedProfile));
                DesignConfigs = new ObservableCollection<DesignConfig>(SelectedProfile?.DesignConfigs ?? new List<DesignConfig>());
                OnPropertyChanged(nameof(DesignConfigs));
            }
        }
    }
    public ObservableCollection<Profile> Profiles { get; set; }
    ///
    public ObservableCollection<DesignConfig> DesignConfigs { get; set; }
    private DesignConfig _SelectedDesignConfig;
    public DesignConfig SelectedDesignConfig
    {
        get { return _SelectedDesignConfig; }
        set
        {
            if (_SelectedDesignConfig != value)
            {
                _SelectedDesignConfig = value;
                OnPropertyChanged(nameof(SelectedDesignConfig));
                ItemConfigs = new ObservableCollection<ItemConfig>(SelectedDesignConfig?.ItemConfigs ?? new List<ItemConfig>());
                Mappings = new ObservableCollection<ItemMapping>(SelectedDesignConfig?.ItemMappings ?? new List<ItemMapping>());
                OnPropertyChanged(nameof(ItemConfigs));
                OnPropertyChanged(nameof(Mappings));
            }
        }
    }
    ///
    public ObservableCollection<ItemConfig> ItemConfigs { get; set; }
    private ItemConfig _SelectedItemConfig;
    public ItemConfig SelectedItemConfig
    {
        get { return _SelectedItemConfig; }
        set
        {
            if (_SelectedItemConfig != value)
            {
                _SelectedItemConfig = value;
                OnPropertyChanged(nameof(SelectedItemConfig));
                Magics = new ObservableCollection<Magic>(SelectedItemConfig?.Magics ?? new List<Magic>());
                OnPropertyChanged(nameof(Magics));
            }
        }
    }
    //
    public ObservableCollection<Magic> Magics { get; set; }
    private Magic _SelectedMagic;
    public Magic SelectedMagic
    {
        get { return _SelectedMagic; }
        set
        {
            if (_SelectedMagic != value)
            {
                _SelectedMagic = value;
                OnPropertyChanged(nameof(SelectedMagic));
            }
        }
    }
    //
    public ObservableCollection<ItemMapping> Mappings { get; set; }
    private ItemMapping _SelectedItemMapping;
    public ItemMapping SelectedItemMapping
    {
        get { return _SelectedItemMapping; }
        set
        {
            if (_SelectedItemMapping != value)
            {
                _SelectedItemMapping = value;
                OnPropertyChanged(nameof(SelectedItemMapping));
            }
        }
    }
    //
    public ObservableCollection<Spell> Spells { get; set; }
    public ObservableCollection<ItemType> ItemTypes { get; set; }
    //
    public ObservableCollection<Order> _Orders;
    public ObservableCollection<Order> Orders
    {
        get { return _Orders; }
        set
        {
            if (_Orders != value)
            {
                _Orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }
    }

    public ICommand SelectProfileDirectory { get; set; }

    public ICommand AddDesignConfig { get; set; }
    public ICommand DeleteDesignConfig { get; set; }
    public ICommand AddItemConfig { get; set; }
    public ICommand DeleteItemConfig { get; set; }
    public ICommand AddMagic { get; set; }
    public ICommand DeleteMagic { get; set; }
    public ICommand MakeMapping { get; set; }
    public ICommand DeleteMapping { get; set; }

    public ICommand CreateProfile { get; set; }
    public ICommand SaveProfile { get; set; }
    public ICommand DeleteProfile { get; set; }
    public ICommand CheckProfile { get; set; }

    public ICommand LoadData { get; set; }
    public ICommand CreateArts { get; set; }
    public ICommand CreatePrintAndCut { get; set; }
    public ICommand Verify { get; set; }
    public ICommand Signaling { get; set; }
    public ICommand ClearSession { get; set; }
    public ICommand PreLoad { get; set; }
    public ICommand DoWork { get; set; }

    public ICommand Copy { get; set; }
    public ICommand Paste { get; set; }

    public MainViewModel()
    {
        ControlSignal = new ManualResetEvent(false);
        Flag = false;

        Profiles = new ObservableCollection<Profile>(
            DBConnector.GetAllProfiles()
            );
        UniversalMesseges = new ConcurrentQueue<Messege>();

        Spells = new ObservableCollection<Spell>(Enum.GetValues(typeof(Spell)).Cast<Spell>());
        ItemTypes = new ObservableCollection<ItemType>(Enum.GetValues(typeof(ItemType)).Cast<ItemType>());

        SelectProfileDirectory = new LCommand(OnSelectProfileDirectory);
        AddDesignConfig = new LCommand(OnAddDesignConfig);
        DeleteDesignConfig = new LCommand(OnDeleteDesignConfig);
        AddItemConfig = new LCommand(OnAddItemConfig);
        DeleteItemConfig = new LCommand(OnDeleteItemConfig);
        AddMagic = new LCommand(OnAddMagic);
        DeleteMagic = new LCommand(OnDeleteMagic);
        MakeMapping = new LCommand(OnMakeMapping);
        DeleteMapping = new LCommand(OnDeleteMapping);

        CreateProfile = new LCommand(OnCreateProfile);
        CreateProfile = new LCommand(OnCreateProfile);
        SaveProfile = new LCommand(OnSaveProfile);
        DeleteProfile = new LCommand(OnDeleteProfile);
        CheckProfile = new LCommand(OnCheckProfile);

        LoadData = new LCommand(OnLoadData);
        CreateArts = new LCommand(OnCreateArts);
        CreatePrintAndCut = new LCommand(OnCreatePrintAndCut);
        Verify = new LCommand(OnVerify);
        ClearSession = new LCommand(OnClearSession);
        Signaling = new LCommand(OnSignaling);

        PreLoad = new LCommand(OnPreLoad);
        DoWork = new LCommand(OnDoWork);

        Copy = new LCommand(OnCopy);
        Paste = new LCommand(OnPaste);

        InitializeBackgroundWorker();

        _isBusy = false;
        SessionUnCleared = false;

        IsCtrlPressed = false;
    }

    public List<ItemConfig> SelectedItemConfigs;
    public List<dynamic> Clipboards;
    public bool IsCtrlPressed;
    private void OnCopy(object parameter)
    {
        Clipboards = new List<dynamic>(SelectedItemConfigs);
    }
    private void OnPaste(object parameter)
    {
        if (Clipboards != null)
        {

        }
    }

    private void OnSignaling(object parameter)
    {
        ControlSignal.Set();
        Flag = false;
    }
    private void OnClearSession(object parameter)
    {
        MainDriver.ClearSession();
        Orders?.Clear();
        SessionUnCleared = false;
        EnqueueMessege(new Messege("Session is cleared successfilly", MessegeInfo.Notification, null));
    }
    private void OnPreLoad(object parameter)
    {
        if (WorkingMode)
        {
            MainDriver.SetWorkingFolder(SelectedProfile.FolderPath);
            MainDriver.SetTemplatePanel(SelectedProfile.Panel);
            MainDriver.SetMessegeTunel(EnqueueMessege);
            MainDriver.SetSignaling(ControlSignal);
            MainDriver.SetDesignConfigs(SelectedProfile.DesignConfigs);
            MainDriver.SetProgressMessege(Progress);
            MainDriver.SetWavingFlags(WavingFlags);
            MainDriver.SetCutColor(SelectedProfile.Panel.CutColor);
            MainDriver.StartDriver();
            SessionUnCleared = true;
            EnqueueMessege(new Messege("Loaded all Config successfilly", MessegeInfo.Notification, null));
        }
    }
    private void OnDoWork(object parameter)
    {
        MainDriver.StartDriver();
        MainDriver.SetCutColor(SelectedProfile.Panel.CutColor);
        EnqueueMessege(new Messege("Re-start Driver successfully", MessegeInfo.Notification, null));
    }

    private void OnSelectProfileDirectory(object parameter)
    {
        Microsoft.Win32.OpenFolderDialog dialog = new();

        dialog.Multiselect = false;
        dialog.Title = "Select a folder";

        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            SelectedProfile.FolderPath = dialog.FolderName;
            EnqueueMessege(new Messege("Selected : " + dialog.FolderName, MessegeInfo.Notification, null));
        }
    }
    private void OnAddDesignConfig(object parameter)
    {
        bool CtrlPressed = IsCtrlPressed, IsDesignSelected = SelectedDesignConfig != null;
        Microsoft.Win32.OpenFileDialog dialog = new();
        dialog.FileName = "Document";
        dialog.DefaultExt = ".ai";
        dialog.Filter = "illustrator documents (.ai)|*.ai";
        dialog.Multiselect = true;

        bool? result = dialog.ShowDialog();

        List<ItemConfig> configs = null;
        List<ItemMapping> mappings = null;
        if(CtrlPressed && IsDesignSelected)
        {
            configs = SelectedDesignConfig.ItemConfigs.Select(o => o.Copy()).ToList();
            mappings = SelectedDesignConfig.ItemMappings.Select(o => o.Copy()).ToList();
        }
        if (result == true)
        {
            for (int i = 0; i < dialog.FileNames.Length; ++i)
            {
                DesignConfig newDesignConfirg = new DesignConfig();
                newDesignConfirg.FilePath = dialog.FileNames[i];
                newDesignConfirg.Label = dialog.SafeFileNames[i].Replace(".ai", "");
                if (CtrlPressed && IsDesignSelected)
                {
                    newDesignConfirg.ItemConfigs = configs;
                    newDesignConfirg.ItemMappings = mappings;
                }
                SelectedDesignConfig = newDesignConfirg;
                DesignConfigs.Add(newDesignConfirg);
                SelectedProfile.DesignConfigs.Add(newDesignConfirg);
                DBConnector.AddDesignConfig(newDesignConfirg);
            }
            DBConnector.UpdateProfile(SelectedProfile);

            EnqueueMessege(new Messege("Selected : " + dialog.FileNames.Length + " file(s)", MessegeInfo.Notification, null));
        }
    }
    private void OnDeleteDesignConfig(object parameter)
    {
        if (SelectedDesignConfig is object)
        {
            DBConnector.DeleteDesignConfig(SelectedDesignConfig);
            SelectedProfile.DesignConfigs.Remove(SelectedDesignConfig);
            DesignConfigs.Remove(SelectedDesignConfig);
            EnqueueMessege(new Messege("Design Config deleted successfilly", MessegeInfo.Notification, null));
        }
    }
    private void OnAddItemConfig(object parameter)
    {
        int indx = SelectedItemConfig is object ? ItemConfigs.IndexOf(SelectedItemConfig) + 1 : ItemConfigs.Count;
        ItemConfig newItemConfig = SelectedItemConfig is object && IsCtrlPressed ? SelectedItemConfig.Copy() : new ItemConfig();

        ItemConfigs.Insert(indx ,newItemConfig);
        SelectedDesignConfig.ItemConfigs.Insert(indx,newItemConfig);
        SelectedItemConfig = newItemConfig;
        DBConnector.AddItemConfig(newItemConfig);
        DBConnector.UpdateDesignConfig(SelectedDesignConfig);
        EnqueueMessege(new Messege("Add Item Config successfilly", MessegeInfo.Notification, null));
    }
    private void OnDeleteItemConfig(object parameter)
    {
        if (SelectedItemConfig is object)
        {
            DBConnector.DeleteItemConfig(SelectedItemConfig);
            SelectedDesignConfig.ItemConfigs.Remove(SelectedItemConfig);
            ItemConfigs.Remove(SelectedItemConfig);
            EnqueueMessege(new Messege("Delete Item Config Config successfilly", MessegeInfo.Notification, null));
        }
    }
    private void OnAddMagic(object parameter)
    {
        Magic newMagic = new Magic();
        int indx = SelectedMagic is object ? Magics.IndexOf(SelectedMagic) + 1 : Magics.Count;
        Magics.Insert(indx, newMagic);
        SelectedItemConfig.Magics.Insert(indx, newMagic);
        SelectedMagic = newMagic;
        DBConnector.AddMagic(newMagic);
        DBConnector.UpdateItemConfig(SelectedItemConfig);
    }
    private void OnDeleteMagic(object parameter)
    {
        if (SelectedMagic is object)
        {
            DBConnector.DeleteMagic(SelectedMagic);
            SelectedItemConfig.Magics.Remove(SelectedMagic);
            Magics.Remove(SelectedMagic);
        }
    }

    private void OnDeleteMapping(object parameter)
    {
        MessageBoxResult r = MessageBox.Show("Delete all Mappings ?", "Warning", MessageBoxButton.YesNo);
        if (r == MessageBoxResult.Yes)
        {
            DBConnector.DeleteItemMapping(Mappings);
            SelectedDesignConfig.ItemMappings.Clear();
            Mappings.Clear();
            DBConnector.UpdateDesignConfig(SelectedDesignConfig);
            EnqueueMessege(new Messege("Delete Mapping successfilly", MessegeInfo.Notification, null));
        }
    }

    private void OnCreateProfile(object parameter)
    {
        Profile newProfile = new Profile();
        Profiles.Add(newProfile);
        SelectedProfile = newProfile;
        DBConnector.AddProfile(newProfile);
        EnqueueMessege(new Messege("Profile created successfilly", MessegeInfo.Notification, null));

    }
    private void OnSaveProfile(object parameter)
    {
        DBConnector.UpdateProfile(SelectedProfile);
        EnqueueMessege(new Messege("Profile save successfilly", MessegeInfo.Notification, null));
    }
    private void OnDeleteProfile(object parameter)
    {
        MessageBoxResult r = MessageBox.Show("Delete this Profile ?", "Warning", MessageBoxButton.YesNo);
        if (r == MessageBoxResult.Yes)
        {
            DBConnector.DeleteProfile(SelectedProfile);
            Profiles.Remove(SelectedProfile);
            EnqueueMessege(new Messege("Profile deleted successfilly", MessegeInfo.Notification, null));
        }
    }
    private void OnCheckProfile(object parameter)
    {
        if (WorkingMode && !IsBusy)
        {
            WorkingMode = false;
            return;
        }
        bool t = true;
        if (SelectedProfile == null)
        {
            t = false;
        }
        else if (!Directory.Exists(SelectedProfile.FolderPath))
        {
            EnqueueMessege(
            new Messege("Selected working directory does not exist", MessegeInfo.Error, null)
                );
            t = false;
        }
        else
        {
            string f1 = SelectedProfile.FolderPath + LObjects.Constants.artFolderName,
                f2 = SelectedProfile.FolderPath + LObjects.Constants.storageFolderName,
                f3 = SelectedProfile.FolderPath + LObjects.Constants.printAndCutFolderName,
                f4 = SelectedProfile.FolderPath + LObjects.Constants.dataFileName;
            if (!Directory.Exists(f1))
            {
                Directory.CreateDirectory(f1);
                EnqueueMessege(
                new Messege("Created art folder.", MessegeInfo.Notification, null)
                );
            }
            else
            {
                int i = 0;
                if (MessageBox.Show("Delete all Created Arts ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                    foreach (string item in Directory.GetFiles(f1))
                    {
                        File.Delete(item);
                        ++i;
                    }
                EnqueueMessege(
                new Messege("Deleted " + i + " File(s)", MessegeInfo.Warning, null)
                );
            }
            if (!Directory.Exists(f2))
            {
                Directory.CreateDirectory(f2);
                EnqueueMessege(
                new Messege("Created storage folder.", MessegeInfo.Notification, null)
                );
            }
            if (!Directory.Exists(f3))
            {
                Directory.CreateDirectory(f3);
                EnqueueMessege(
                new Messege("Created print and cut Folder.", MessegeInfo.Notification, null)
                );
            }
            if (!File.Exists(f4))
            {
                File.Create(f4);
                EnqueueMessege(
                new Messege("Created data file.", MessegeInfo.Notification, null)
                );
            }
            foreach (DesignConfig item in SelectedProfile.DesignConfigs)
            {
                if (!File.Exists(item.FilePath))
                {
                    EnqueueMessege(
                        new Messege(item.Label + "\'s file does not exist", MessegeInfo.Error, null)
                        );
                    t = false;
                }
            }
        }
        WorkingMode = t;
    }
    //
    private bool _WorkingMode;
    public bool WorkingMode
    {
        get => _WorkingMode;
        set
        {
            _WorkingMode = value;
            OnPropertyChanged(nameof(WorkingMode));
            OnPropertyChanged(nameof(NonWorkingMode));
            OnPropertyChanged(nameof(SessionUnCleared));
            OnPropertyChanged(nameof(PreLoaded));
        }
    }
    private bool _SessionUnCleared;
    public bool SessionUnCleared
    {
        get => (_SessionUnCleared && _WorkingMode);
        set
        {
            _SessionUnCleared = value;
            OnPropertyChanged(nameof(SessionUnCleared));
            OnPropertyChanged(nameof(PreLoaded));
        }
    }
    public bool PreLoaded
    {
        get => (!_SessionUnCleared && _WorkingMode);
    }
    public bool NonWorkingMode
    {
        get => !_WorkingMode;
    }
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
        }
    }
    private bool _AutoExportPC;
    public bool AutoExportPC
    {
        get => _AutoExportPC;
        set
        {
            _AutoExportPC = value;
            OnPropertyChanged(nameof(AutoExportPC));
        }
    }
    private bool _AutoRestartWork;
    public bool AutoRestartWork
    {
        get => _AutoRestartWork;
        set
        {
            _AutoRestartWork = value;
            OnPropertyChanged(nameof(AutoRestartWork));
        }
    }
    public ConcurrentQueue<Messege> UniversalMesseges { get; }
    public void EnqueueMessege(Messege m)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            UniversalMesseges.Enqueue(m);
            OnPropertyChanged(nameof(UniversalMesseges));
        });
    }
    private int _ProgressIndex;
    public int ProgressIndex
    {
        get => _ProgressIndex;
        set
        {
            _ProgressIndex = value;
            OnPropertyChanged(nameof(ProgressIndex));
            ProgressPercentage = (double)ProgressIndex / Total * 100;
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }
    private int _Total;
    public int Total
    {
        get => _Total;
        set
        {
            _Total = value;
            OnPropertyChanged(nameof(Total));
        }
    }
    public double ProgressPercentage { get; set; }
    public void Progress(bool G, int v)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {

            if (G)
                ProgressIndex = v;
            else
            {
                ProgressIndex = 0;
                Total = v;
            }
        });
    }

    public ManualResetEvent ControlSignal { get; set; }
    private bool _Flag;
    public bool Flag
    {
        get => _Flag;
        set
        {
            _Flag = value;
            OnPropertyChanged(nameof(Flag));
        }
    }
    public void WavingFlags(bool waveing)
    {
        Flag = waveing;
    }
    //
    private BackgroundWorker _DoLoadData;
    private BackgroundWorker _DoCreateArts;
    private BackgroundWorker _DoCreatePrintAndCut;
    private BackgroundWorker _DoVerify;
    private BackgroundWorker _MakeMapping;
    private void InitializeBackgroundWorker()
    {

        _DoLoadData = new BackgroundWorker();
        _DoLoadData.WorkerReportsProgress = true;
        _DoLoadData.RunWorkerCompleted += RunWorkerCompletedHandler;
        _DoLoadData.DoWork += StartLoadData;

        _DoCreateArts = new BackgroundWorker();
        _DoCreateArts.WorkerReportsProgress = true;
        _DoCreateArts.RunWorkerCompleted += RunWorkerCompletedHandler;
        _DoCreateArts.DoWork += StartCreateArts;

        _DoCreatePrintAndCut = new BackgroundWorker();
        _DoCreatePrintAndCut.WorkerReportsProgress = true;
        _DoCreatePrintAndCut.RunWorkerCompleted += RunWorkerCompletedHandler;
        _DoCreatePrintAndCut.DoWork += StartCreatePrintAndCut;

        _DoVerify = new BackgroundWorker();
        _DoVerify.WorkerReportsProgress = true;
        _DoVerify.RunWorkerCompleted += RunWorkerCompletedHandler;
        _DoVerify.DoWork += StartVerify;

        _MakeMapping = new BackgroundWorker();
        _MakeMapping.WorkerReportsProgress = true;
        _MakeMapping.RunWorkerCompleted += RunWorkerCompletedHandler;
        _MakeMapping.DoWork += StartMakeMapping;

    }
    private void OnMakeMapping(object parameter)
    {
        if (IsBusy)
            return;
        IsBusy = true;
        ProgressIndex = 0;
        _MakeMapping.RunWorkerAsync();
    }
    private void StartMakeMapping(object sender, DoWorkEventArgs e)
    {
        EnqueueMessege(new Messege("Start Make Mappings", MessegeInfo.Notification, null));
        List<ItemMapping> mappings = null;


        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.FileName = "Document";
        dialog.Multiselect = false;

        bool? result = dialog.ShowDialog();

        // Process open file dialog box results
        if (result == true)
        {
            mappings = MainDriver.MakeMapping(dialog.FileName);
        }
        else
        {
            mappings = MainDriver.MakeMapping();
        }

        DBConnector.AddItemMapping(mappings);
        SelectedDesignConfig.ItemMappings.AddRange(mappings);
        Mappings = new ObservableCollection<ItemMapping>(SelectedDesignConfig.ItemMappings);
        DBConnector.UpdateDesignConfig(SelectedDesignConfig);
        EnqueueMessege(new Messege("Finish Make Mappings", MessegeInfo.Notification, null));
    }

    private void OnLoadData(object parameter)
    {
        if (IsBusy)
            return;
        IsBusy = true;
        ProgressIndex = 0;
        _DoLoadData.RunWorkerAsync();
    }
    private void StartLoadData(object sender, DoWorkEventArgs e)
    {
        Orders = new ObservableCollection<Order>(MainDriver.LoadOrder());
        MainDriver.LoadArts();
    }
    private void OnCreateArts(object parameter)
    {
        if (IsBusy)
            return;
        IsBusy = true;
        ProgressIndex = 0;
        _DoCreateArts.RunWorkerAsync();
    }
    private void StartCreateArts(object sender, DoWorkEventArgs e)
    {
        MainDriver.CreateArts();
    }
    private void OnCreatePrintAndCut(object parameter)
    {
        if (IsBusy)
            return;
        IsBusy = true;
        ProgressIndex = 0;
        _DoCreatePrintAndCut.RunWorkerAsync();
    }
    private void StartCreatePrintAndCut(object sender, DoWorkEventArgs e)
    {
        string today = Interaction.InputBox("Storage Folder by Session :");
        MainDriver.CreatePrintAndCut(today, AutoExportPC);
    }
    private void OnVerify(object parameter) { }
    private void StartVerify(object sender, DoWorkEventArgs e) { }
    private void RunWorkerCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            EnqueueMessege(new Messege(e.Error.Message, MessegeInfo.Exception, null));
            if (AutoExportPC)
                ((BackgroundWorker)sender).RunWorkerAsync();
        }

        IsBusy = false;
    }

}
