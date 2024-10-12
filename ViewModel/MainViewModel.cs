using AiDesignTool.LCommands;
using LObjects;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            _SelectedProfile = value;
            OnPropertyChanged(nameof(SelectedProfile));
        }
    }
    public ObservableCollection<Profile> Proflies { get; set; }
    public ICommand CreateProfile {  get; set; }
    public ICommand SaveProfile { get; set; }
    public ICommand DeleteProfile { get; set; }
    public ICommand LoadData { get; set; }
    public ICommand CreateArts { get; set; }
    public ICommand CreatePrintAndCut { get; set; }
    public ICommand Verify { get; set; }
    public MainViewModel()
	{
	}
}
