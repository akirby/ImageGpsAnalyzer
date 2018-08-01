using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageAnalyzer.Tests
{
    [TestClass]
    public class ExifReaderTests
    {
        [TestMethod]
        public void GetExifFromImageTest()
        {
            //latitude 37.096490 or 37 degrees, 5 minutes and 47.364 seconds north
            //longitude 25.1433388888889 or 25 degrees, 8 minutes and 25.862 seconds east
            double expectedLong = 25.1433388888889;
            double expectedLat = 37.0966277777778;
            string filepath = @"C:\Users\Andrew Kirby\Source\Repos\ImageAnalyzer\MVIMG_20180627_201911.jpg";
            Image testImage = Image.FromFile(filepath);
            ExifReader target = new ExifReader();

            var result = target.GetExifGpsFromImage(testImage);
            double longitude = Math.Round(result.Longitude.ToDouble(), 13);
            double latitude = Math.Round(result.Latitude.ToDouble(), 13);
            Assert.AreEqual(expectedLong, longitude);
            Assert.AreEqual(expectedLat, latitude);
        }
    }
}
