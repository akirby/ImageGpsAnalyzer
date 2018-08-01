using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageAnalyzer.GPS;

namespace ImageAnalyzer
{
    public class ExifReader
    {
        public List<GPSCoordinate> GetExifGpsFromImagesInFolder(string directory)
        {
            //string[] files = Directory.GetFiles(directory).Where(f => !f.ToUpper().EndsWith(".MOV")).ToArray();
            string[] files = Directory.GetFiles(directory, "*.jpg");
            List<GPSCoordinate> gPSCoordinates = new List<GPSCoordinate>();
            foreach (var filePath in files)
            {
                using (var imageFile = Image.FromFile(filePath))
                {
                    var gpsCoordinate = GetExifGpsFromImage(imageFile);
                    gpsCoordinate.FileName = filePath;
                    gPSCoordinates.Add(gpsCoordinate);
                    imageFile.Dispose();
                }
            }
            return gPSCoordinates;
        }
        public GPSCoordinate GetExifGpsFromImage(Image image)
        {
            var latRef = image.PropertyItems.FirstOrDefault(p => p.Id == 1);
            var lat = image.PropertyItems.FirstOrDefault(p => p.Id == 2);
            var lonRef = image.PropertyItems.FirstOrDefault(p => p.Id == 3);
            var lon = image.PropertyItems.FirstOrDefault(p => p.Id == 4);
            var gpsUtc = image.PropertyItems.FirstOrDefault(p => p.Id == 7);
            var utc = image.PropertyItems.FirstOrDefault(p => p.Id == 306);

            if (lat == null && lon == null && gpsUtc == null)
            {
                return new GPSCoordinate("No GPS information for this image.") { FileTime = GetDateTimeFromProperty(utc) };

            }

            var gpsUtcParts = GetRationalNumbersFromValue(gpsUtc);
            GPSCoordinate gPSCoordinate = GetGPSCoordinateFromProperties(latRef, lat, lonRef, lon);
            
            gPSCoordinate.FileTime = GetDateTimeFromProperty(utc);
            gPSCoordinate.UtcTime = new DateTime(gPSCoordinate.FileTime.Year, gPSCoordinate.FileTime.Month,
                gPSCoordinate.FileTime.Day, (int)gpsUtcParts[0], (int)gpsUtcParts[1], (int)gpsUtcParts[2]);

            return gPSCoordinate;
        }

        private DateTime GetDateTimeFromProperty(PropertyItem utc)
        {
            if (utc == null)
            {
                return DateTime.MinValue;
            }
            string dateVal = Encoding.ASCII.GetString(utc.Value);
            dateVal = dateVal.Remove(dateVal.Length - 1);
            return DateTime.ParseExact(dateVal, "yyyy:MM:dd H:mm:ss", null);
        }

        private GPSCoordinate GetGPSCoordinateFromProperties(PropertyItem latRef, PropertyItem lat, PropertyItem lonRef, PropertyItem lon)
        {
            GPSCoordinate result = new GPSCoordinate(
                GetGeoLineFromProperties(latRef, lat, GeoLineType.Latitude),
                GetGeoLineFromProperties(lonRef, lon, GeoLineType.Longitude));
            return result;
        }

        private GeoLine GetGeoLineFromProperties(PropertyItem lineRef, PropertyItem line, GeoLineType lineType)
        {
            var lineDir = Encoding.ASCII.GetString(lineRef.Value).Substring(0, 1);
            CardinalDirection direction = (CardinalDirection)Enum.Parse(typeof(CardinalDirection), lineDir);
            var measurements = GetRationalNumbersFromValue(line);
            return new GeoLine(measurements[0], measurements[1], measurements[2], lineType, direction);
        }

        private List<double> GetRationalNumbersFromValue(PropertyItem item)
        {
            int pairCount = item.Len / 8;
            List<double> result = new List<double>();
            using (MemoryStream ms = new MemoryStream(item.Value))
            {
                for (int i = 0; i < pairCount; i++)
                {
                    //for 32 bit integers
                    byte[] nBytes = new byte[4];
                    byte[] dBytes = new byte[4];
                    ms.Read(nBytes, 0, 4);
                    ms.Read(dBytes, 0, 4);
                    int numerator = BitConverter.ToInt32(nBytes, 0);
                    int denominator = BitConverter.ToInt32(dBytes, 0);
                    double rationalNum = Convert.ToDouble(numerator) / Convert.ToDouble(denominator);
                    result.Add(rationalNum);
                }
            }

            return result;
        }
    }




}
