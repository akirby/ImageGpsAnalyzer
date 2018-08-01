using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalyzer.GPS
{
    [Serializable]
    public class GPSCoordinate
    {
        [DisplayName("Latitude")]
        public GeoLine Latitude { get; set; }
        [DisplayName("Longitude")]
        public GeoLine Longitude { get; set; }
        /// <summary>
        /// This is the time from the GPS properties adjusted to be utc.
        /// </summary>
        [DisplayName("GPS UTC Time")]
        public DateTime UtcTime { get; set; }
        /// <summary>
        /// This is the local time that the file was created.
        /// </summary>
        [DisplayName("File Local Time")]
        public DateTime FileTime { get; set; }
        [DisplayName("File Name")]
        public string FileName { get; set; }
        [DisplayName("Notes")]
        public string Notes { get; set; }
        private bool _includedInMap;
        public bool IncludedInMap
        {
            get
            {
                return _includedInMap;
            }
            set
            {
                _includedInMap = value;
                OnIncludedInMapChanged(EventArgs.Empty);
            }
        }
        protected virtual void OnIncludedInMapChanged(EventArgs e)
        {
            EventHandler handler = IncludedInMapChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler IncludedInMapChanged;
        public GPSCoordinate()
        {

        }
        public GPSCoordinate(string notes)
        {
            Notes = notes;
        }
        public GPSCoordinate(GeoLine latitude, GeoLine longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            IncludedInMap = true;
        }

        public GPSCoordinate(double latitude, double longitude)
        {
            Longitude = new GeoLine(longitude, GeoLineType.Longitude, longitude < 0 ? CardinalDirection.S : CardinalDirection.N);
            Latitude = new GeoLine(latitude, GeoLineType.Latitude, latitude < 0 ? CardinalDirection.W : CardinalDirection.S);
            IncludedInMap = true;
        }

        public override string ToString()
        {
            return Latitude.ToString() + ", " + Longitude.ToString();
        }
    }
}
