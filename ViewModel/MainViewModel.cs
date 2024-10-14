using AiDesignTool.LCommands;
using LObjects;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
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
            }
        }
    }
    public ObservableCollection<Profile> Profiles { get; set; }
    public ICommand CreateProfile {  get; set; }
    public ICommand SaveProfile { get; set; }
    public ICommand DeleteProfile { get; set; }
    public ICommand LoadData { get; set; }
    public ICommand CreateArts { get; set; }
    public ICommand CreatePrintAndCut { get; set; }
    public ICommand Verify { get; set; }
    public ICommand SelectProfilePath { get; set; }
    public ICommand SelectDesignConfig { get; set; }
    public ICommand SelectDesignConfigPath { get; set; }
    public ICommand Update { get; set; }
    public MainViewModel()
	{
        Profiles = new ObservableCollection<Profile>(
            MainDriver.GetAllProfiles()
            );
        CreateProfile           = new LCommand(OnCreateProfile);
        SaveProfile             = new LCommand(OnSaveProfile);
        DeleteProfile           = new LCommand(OnDeleteProfile);
        LoadData                = new LCommand(OnLoadData);
        CreateArts              = new LCommand(OnCreateArts);
        CreatePrintAndCut       = new LCommand(OnCreatePrintAndCut);
        Verify                  = new LCommand(OnVerify);
        SelectProfilePath       = new LCommand(OnSelectProfilePath);
        SelectDesignConfig      = new LCommand(OnSelectDesignConfig);
        SelectDesignConfigPath  = new LCommand(OnSelectDesigConfigPath);
        Update                  = new LCommand(OnUpdate);
}
    private void OnCreateProfile(object parameter)
    {
        Profile newProfile = new Profile();
        Profiles.Add(newProfile);
        SelectedProfile = newProfile;
        newProfile.Id = MainDriver.AddProfile(newProfile);
    }
    private void OnSaveProfile(object parameter)
    {
        OnUpdate(parameter);
        MainDriver.UpdateProfile(SelectedProfile);
    }
    private void OnDeleteProfile(object parameter)
    {
        MainDriver.DeleteProfile(SelectedProfile);
        Profiles.Remove(SelectedProfile);
    }
    private void OnLoadData(object parameter)
    {

    }
    private void OnCreateArts(object parameter)
    {

    }
    private void OnCreatePrintAndCut(object parameter)
    {

    }
    private void OnVerify(object parameter)
    {

    }
    private void OnSelectProfilePath(object parameter)
    {

    }
    private void OnUpdate(object parameter)
    {
    }
    private void OnSelectDesignConfig(object parameter)
    {

    }
    private void OnSelectDesigConfigPath(object parameter)
    {

    }
}
