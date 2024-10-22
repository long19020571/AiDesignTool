using AiDesignTool.LCommands;
using LObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

public class MainViewModel :  INotifyPropertyChanged
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

    public ICommand SelectProfileDirectory {  get; set; }

    public ICommand AddDesignConfig { get; set; }
    public ICommand DeleteDesignConfig { get; set;}
    public ICommand AddItemConfig { get; set;}
    public ICommand DeleteItemConfig { get; set;}
    public ICommand AddMagic { get; set;}
    public ICommand DeleteMagic { get; set;}
    public ICommand MakeMapping { get; set;}
    public ICommand DeleteMapping { get; set;}

    public ICommand CreateProfile { get; set;}
    public ICommand SaveProfile { get; set;}
    public ICommand DeleteProfile { get; set; }
    public ICommand CheckProfile { get; set; }

    public ICommand LoadData { get; set;}
    public ICommand CreateArts { get; set;}
    public ICommand CreatePrintAndCut { get; set;}
    public ICommand Verify { get; set; }
    public ICommand Signaling { get; set; }
    public ICommand ClearSession { get; set; }


    public MainViewModel()
	{
        ControlSignal = new ManualResetEvent(false);

        Profiles = new ObservableCollection<Profile>(
            DBConnector.GetAllProfiles()
            );
        UniversalMesseges = new ConcurrentQueue<Messege>();
        MessegeIndex = 0; ;

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
        CreateProfile = new LCommand(OnCreateProfile );
        SaveProfile = new LCommand(OnSaveProfile );
        DeleteProfile = new LCommand(OnDeleteProfile );
        CheckProfile = new LCommand(OnCheckProfile);

        LoadData = new LCommand(OnLoadData );
        CreateArts = new LCommand(OnCreateArts );
        CreatePrintAndCut = new LCommand(OnCreatePrintAndCut );
        Verify = new LCommand(OnVerify);
        ClearSession = new LCommand(OnClearSession);



        InitializeBackgroundWorker();

        _isBusy = false;
    }
    private void OnSignaling(object parameter)
    {
        ControlSignal.Set();
    }
    private void OnClearSession(object parameter)
    {
        MainDriver.ClearSession();
    }

    private void OnSelectProfileDirectory(object parameter)
    {
        Microsoft.Win32.OpenFolderDialog dialog = new();

        dialog.Multiselect = false;
        dialog.Title = "Select a folder";

        // Show open folder dialog box
        bool? result = dialog.ShowDialog();

        // Process open folder dialog box results
        if (result == true)
        {
            SelectedProfile.FolderPath = dialog.FolderName;
        }
    }
    private void OnAddDesignConfig(object parameter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.FileName = "Document";
        dialog.DefaultExt = ".ai";
        dialog.Filter = "illustrator documents (.ai)|*.ai";
        dialog.Multiselect = true;

        bool? result = dialog.ShowDialog();

        // Process open file dialog box results
        if (result == true)
        {
            for (int i = 0; i < dialog.FileNames.Length; ++i)
            {
                DesignConfig newDesignConfirg = new DesignConfig();
                newDesignConfirg.FilePath = dialog.FileNames[i];
                newDesignConfirg.Label = dialog.SafeFileNames[i].Replace(".ai", "");
                SelectedDesignConfig = newDesignConfirg;
                DesignConfigs.Add(newDesignConfirg);
                SelectedProfile.DesignConfigs.Add(newDesignConfirg);
                DBConnector.AddDesignConfig(newDesignConfirg);
            }
            DBConnector.UpdateProfile(SelectedProfile);
        }
    }
    private void OnDeleteDesignConfig(object parameter)
    {
        DBConnector.DeleteDesignConfig(SelectedDesignConfig);
        SelectedProfile.DesignConfigs.Remove(SelectedDesignConfig);
        DesignConfigs.Remove(SelectedDesignConfig);
    }
    private void OnAddItemConfig(object parameter)
    {

        ItemConfig newItemConfig = new ItemConfig();
        ItemConfigs.Add(newItemConfig);
        SelectedDesignConfig.ItemConfigs.Add(newItemConfig);
        SelectedItemConfig = newItemConfig;
        DBConnector.AddItemConfig(newItemConfig);
        DBConnector.UpdateDesignConfig(SelectedDesignConfig);
    }
    private void OnDeleteItemConfig(object parameter)
    {
        DBConnector.DeleteItemConfig(SelectedItemConfig);
        SelectedDesignConfig.ItemConfigs.Remove(SelectedItemConfig);
        ItemConfigs.Remove(SelectedItemConfig);
    }
    private void OnAddMagic(object parameter)
    {
        Magic newMagic = new Magic();
        Magics.Add(newMagic);
        SelectedItemConfig.Magics.Add(newMagic);
        SelectedMagic = newMagic;
        DBConnector.AddMagic(newMagic);
        DBConnector.UpdateItemConfig(SelectedItemConfig);
    }
    private void OnDeleteMagic(object parameter)
    {
        DBConnector.DeleteMagic(SelectedMagic);
        SelectedItemConfig.Magics.Remove(SelectedMagic);
        Magics.Remove(SelectedMagic);
    }
    private void OnMakeMapping(object parameter)
    {
        MessageBox.Show(" OnMakeMapping");
        List<ItemMapping> mappings = MainDriver.MakeMapping();
        DBConnector.AddItemMapping(mappings);
        SelectedDesignConfig.ItemMappings = mappings;
        Mappings = new ObservableCollection<ItemMapping>(mappings);
        DBConnector.UpdateDesignConfig(SelectedDesignConfig);
    }
    private void OnDeleteMapping(object parameter) { MessageBox.Show(" OnDeleteMapping");}

    private void OnCreateProfile(object parameter)
    {
        Profile newProfile = new Profile();
        Profiles.Add(newProfile);
        SelectedProfile = newProfile;
        DBConnector.AddProfile(newProfile);

    }
    private void OnSaveProfile(object parameter)
    {
        DBConnector.UpdateProfile(SelectedProfile);
    }
    private void OnDeleteProfile(object parameter)
    {
        DBConnector.DeleteProfile(SelectedProfile);
        Profiles.Remove(SelectedProfile);
    }
    private void OnCheckProfile(object parameter)
    {
        if (WorkingMode && !IsBusy){
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
            string f1 = SelectedProfile.FolderPath + Constants.artFolderName,
                f2 = SelectedProfile.FolderPath + Constants.storageFolderName,
                f3 = SelectedProfile.FolderPath + Constants.printAndCutFolderName,
                f4 = SelectedProfile.FolderPath + Constants.dataFileName;
            if (!Directory.Exists(f1))
            {
                Directory.CreateDirectory(f1);
                EnqueueMessege(
                new Messege("Created art folder.", MessegeInfo.Notification, null)
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
        if(WorkingMode)
        {
            MainDriver.SetWorkingFolder(SelectedProfile.FolderPath);
            MainDriver.SetTemplatePanel(SelectedProfile.Panel);
            MainDriver.SetMessegeTunel(EnqueueMessege);
            MainDriver.SetSignaling(ControlSignal);
            MainDriver.SetDesignConfigs(SelectedProfile.DesignConfigs);
            MainDriver.SetProgressMessege(Progress);
            MainDriver.SetCutColor(SelectedProfile.Panel.CutColor);
            MainDriver.StartDriver();
        }
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
        }
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
    public ConcurrentQueue<Messege> UniversalMesseges { get; }
    public void EnqueueMessege(Messege m)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            UniversalMesseges.Enqueue(m);
            OnPropertyChanged(nameof(UniversalMesseges));
        });
    }
    public int MessegeIndex { get; set; }
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
                Total = v;
        });
    }

    public ManualResetEvent ControlSignal;
    //
    private BackgroundWorker _DoLoadData;
    private BackgroundWorker _DoCreateArts;
    private BackgroundWorker _DoCreatePrintAndCut;
    private BackgroundWorker _DoVerify;
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
        MainDriver.CreatePrintAndCut();
    }
    private void OnVerify(object parameter) { }
    private void StartVerify(object sender, DoWorkEventArgs e) { }

    private void RunWorkerCompletedHandler(object sender, RunWorkerCompletedEventArgs e)
    {
        IsBusy = false;
    }

}
