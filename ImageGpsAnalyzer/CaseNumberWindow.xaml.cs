using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageGpsAnalyzer
{
    /// <summary>
    /// Interaction logic for CaseNumber.xaml
    /// </summary>
    public partial class CaseNumberWindow : Window
    {
        public string CaseNumber;
        public CaseNumberWindow()
        {
            InitializeComponent();
        }

        private void saveNum_Click(object sender, RoutedEventArgs e)
        {
            CaseNumber = txtCaseNum.Text;
            this.Close();
        }
    }
}
