using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImageAnalyzer
{
    [Serializable]
    public class Case
    {
        [XmlAttribute]
        public string CaseNumber { get; set; }
        public List<GPS.GPSCoordinate> GPSCoordinates { get; set; }
    }
}
