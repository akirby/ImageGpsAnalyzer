using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageGpsPackager;
using ImageAnalyzer;
using ImageAnalyzer.GPS;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace ImageGpsAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Case CaseFile { get; set; }
        public List<GPSCoordinate> Coordinates
        {
            get
            {
                return CaseFile.GPSCoordinates;
            }
            set
            {
                CaseFile.GPSCoordinates = value;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            
            CaseFile = new Case();
            selectAll.Checked += SelectAll_Checked;
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox ckbox = sender as CheckBox;
            if (ckbox.IsChecked.Value && CaseFile != null && CaseFile.GPSCoordinates.Any(g => !g.IncludedInMap))
            {
                CaseFile.GPSCoordinates.ForEach(g => g.IncludedInMap = true);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            CheckForLoadedCaseFile();

            CaseFile = new Case();
            SelectCaseNumber();
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.DefaultDirectory = string.Format(@"C:\Users\{0}\Pictures", Environment.UserName);
            openFileDialog.InitialDirectory = string.Format(@"C:\Users\{0}\Pictures", Environment.UserName);
            openFileDialog.IsFolderPicker = true;

            var result = openFileDialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var folder = openFileDialog.FileName;
                ExifReader reader = new ExifReader();

                CaseFile.GPSCoordinates = reader.GetExifGpsFromImagesInFolder(folder).OrderBy(g => g.FileTime).ToList();
                CaseFile.GPSCoordinates.ForEach(g => g.IncludedInMapChanged += G_IncludedInMapChanged);
                dgPicData.ItemsSource = CaseFile.GPSCoordinates;

                selectAll.IsChecked = CaseFile.GPSCoordinates.All(g => g.IncludedInMap);
                //dgPicData.Columns["IncludedInMap"].
                PlotMapData(CaseFile.GPSCoordinates.Where(g => g.IncludedInMap).ToList());
            }

        }

        private void G_IncludedInMapChanged(object sender, EventArgs e)
        {
            GPSCoordinate coordinate = sender as GPSCoordinate;
            if (!coordinate.IncludedInMap && selectAll.IsChecked.Value)
            {
                selectAll.IsChecked = false;
            }
            else if (CaseFile.GPSCoordinates.All(g => g.IncludedInMap) && !selectAll.IsChecked.Value)
            {
                selectAll.IsChecked = true;
            }
            PlotMapData(CaseFile.GPSCoordinates.Where(g => g.IncludedInMap).ToList());
        }

        private void SelectCaseNumber()
        {
            CaseNumberWindow caseNumberWindow = new CaseNumberWindow();
            var result = caseNumberWindow.ShowDialog();
            if (result != null && !string.IsNullOrEmpty(caseNumberWindow.CaseNumber))
            {
                CaseFile.CaseNumber = caseNumberWindow.CaseNumber;
            }
        }

        private void CheckForLoadedCaseFile()
        {
            if (CaseFile != null && !string.IsNullOrEmpty(CaseFile.CaseNumber))
            {
                //Messagebox to check if they want to save changes to the existing case file.
                MessageBoxResult svChkResult = MessageBox.Show(string.Format("Would you like to save changes to the open Case #{0}?", CaseFile.CaseNumber), "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (svChkResult == MessageBoxResult.Yes)
                {
                    SaveCase();
                }
            }
        }

        private void PlotMapData(List<GPSCoordinate> gpsData)
        {
            mapPreview.Children.Clear();
            DateTime minDate = gpsData.FirstOrDefault().FileTime;
            DateTime maxDate = gpsData.LastOrDefault().FileTime;
            fromDate.SelectedDate = minDate;
            toDate.SelectedDate = maxDate;
            MapPolyline line = new MapPolyline();
            line.Stroke = new SolidColorBrush(Colors.Blue);
            line.StrokeThickness = 5;
            line.Locations = new LocationCollection();
            List<double> latitudes = new List<double>();
            List<double> longitudes = new List<double>();
            int i = 1;
            foreach (var gps in gpsData)
            {
                Pushpin pin = new Pushpin();
                latitudes.Add(gps.Latitude.ToDouble());
                longitudes.Add(gps.Longitude.ToDouble());
                pin.Location = new Location(gps.Latitude.ToDouble(), gps.Longitude.ToDouble());
                mapPreview.Children.Add(pin);
                pin.Content = i.ToString();
                i++;
                line.Locations.Add(pin.Location);
            }
            mapPreview.SetView(line.Locations, new Thickness(50), 0);
            mapPreview.Children.Add(line);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            CheckForLoadedCaseFile();

            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.DefaultDirectory = string.Format(@"C:\Users\{0}\Documents\Image GPS Analyzer", Environment.UserName);
            openFileDialog.InitialDirectory = string.Format(@"C:\Users\{0}\Documents\Image GPS Analyzer", Environment.UserName);
            openFileDialog.IsFolderPicker = false;
            openFileDialog.Filters.Add(new CommonFileDialogFilter("Case Files", ".xml"));

            var result = openFileDialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                CaseFile = Packager.GetCaseFromFile(openFileDialog.FileName);

                selectAll.IsChecked = CaseFile.GPSCoordinates.All(g => g.IncludedInMap);
                this.Title = string.Format("Image GPS Analyzer - Case {0}", CaseFile.CaseNumber);
                dgPicData.ItemsSource = CaseFile.GPSCoordinates;
                PlotMapData(CaseFile.GPSCoordinates.Where(g => g.IncludedInMap).ToList());
            }
        }

        private void SetZoom()
        {
            var mapPoints = mapPreview.Children.OfType<Pushpin>().ToList();
            while (mapPoints.All(p => p.IsVisible))
            {
                mapPreview.ZoomLevel++;

            }
            mapPreview.ZoomLevel--;
        }

        private void DataGridRow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = (DataGridRow)sender;
            GPSCoordinate gps = row.Item as GPSCoordinate;
            imgPreview.Source = new BitmapImage(new Uri(gps.FileName));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveCase();
        }

        private void SaveCase()
        {
            string folder = string.Format(@"C:\Users\{0}\Documents\Image GPS Analyzer\{1}", Environment.UserName, CaseFile.CaseNumber);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)mapPreview.ActualWidth, (int)mapPreview.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(mapPreview);
            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));
            string mapFileName = System.IO.Path.Combine(folder, string.Format("CaseReport-{0}.png", CaseFile.CaseNumber));

            using (FileStream ms = new FileStream(mapFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                png.Save(ms);
                ms.Flush();
            }

            Packager.ExportCaseToDoc(CaseFile, mapFileName, folder);

            Packager.ExportCaseXml(CaseFile, folder);
            VerifyFileCreation(folder);
        }

        private void VerifyFileCreation(string folder)
        {
            if (File.Exists(System.IO.Path.Combine(folder, string.Format("CaseReport-{0}.xml", CaseFile.CaseNumber))) &&
                File.Exists(System.IO.Path.Combine(folder, string.Format("CaseReport-{0}.docx", CaseFile.CaseNumber))))
            {
                MessageBox.Show(string.Format("Case Files saved to {0}", folder), "Case File Saved");
            }
            else
            {
                var result = MessageBox.Show("Case Files not saved, try again?", "Case File Saved", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SaveCase();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckForLoadedCaseFile();
        }

        private void fromDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker picker = sender as DatePicker;
            CaseFile.GPSCoordinates
                .Where(g => DateTime.Compare(g.FileTime, picker.SelectedDate.Value) < 0).ToList()
                .ForEach(g => g.IncludedInMap = false);
        }

        private void toDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker picker = sender as DatePicker;
            CaseFile.GPSCoordinates
                .Where(g => DateTime.Compare(g.FileTime, picker.SelectedDate.Value) > 0).ToList()
                .ForEach(g => g.IncludedInMap = false);
        }
    }
}
