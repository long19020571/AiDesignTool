using LObjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.UniversalMesseges))
            {
                Messege m;
                while (ViewModel.UniversalMesseges.TryDequeue(out m))
                {
                    Run r = new Run(new string(m.messege));
                    switch(m.info)
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
                                r.Foreground= Brushes.Red;
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
    }
}