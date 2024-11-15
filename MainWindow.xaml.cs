using LObjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AiDesignTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();
            DataContext = ViewModel;

            // Lắng nghe sự kiện PropertyChanged từ ViewModel
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            KeyDown += App_KeyDown;
            KeyUp += App_KeyUp;
        }
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.UniversalMesseges))
            {
                Messege m;
                while (ViewModel.UniversalMesseges.TryDequeue(out m))
                {
                    StringBuilder printBuilder = new StringBuilder();
                    printBuilder.Append(m.messege);
                    if(m.status != null)
                        printBuilder.AppendJoin('\n', m.status);
                    Run r = new Run(printBuilder.ToString());
                    switch (m.info)
                    {
                        case MessegeInfo.Notification:
                            {
                                r.Foreground = Brushes.Blue;
                                break;
                            }
                        case MessegeInfo.Warning:
                            {
                                r.Foreground = Brushes.Orange;
                                break;
                            }
                        case MessegeInfo.Error:
                            {
                                r.Foreground = Brushes.Red;
                                break;
                            }
                        case MessegeInfo.Exception:
                            {
                                r.Foreground = Brushes.OrangeRed;
                                break;
                            }
                        case MessegeInfo.Complete:
                            {
                                r.Foreground = Brushes.Green;
                                break;
                            }
                        case MessegeInfo.Unhandled:
                            {
                                r.Foreground = Brushes.Gray;
                                break;
                            }
                    }
                    RTBPublisher.Document.Blocks.Add(new Paragraph(r));
                    RTBPublisher.ScrollToEnd();
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            var selectedItems = dataGrid.SelectedItems;

            ViewModel.SelectedItemConfigs = selectedItems.Cast<ItemConfig>().ToList();
        }
        private void App_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ViewModel.IsCtrlPressed = true;
            }
        }
        private void App_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                ViewModel.IsCtrlPressed = false;
            }
        }
    }
}