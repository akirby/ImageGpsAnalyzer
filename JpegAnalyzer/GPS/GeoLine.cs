using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImageAnalyzer.GPS
{
    /// <summary>
    /// This class will convert from double to latitude and longitude.  
    /// This class is a modification of a class found at https://stackoverflow.com/questions/4504956/formatting-double-to-latitude-longitude-human-readable-format
    /// </summary>
    [Serializable]
    public class GeoLine
    {
        [XmlAttribute]
        public double Degrees { get;  set; }
        [XmlAttribute]
        public double MaxDegrees { get;  set; }
        [XmlAttribute]
        public double Minutes { get;  set; }
        [XmlAttribute]
        public double Seconds { get;  set; }
        [XmlAttribute]
        public CardinalDirection CardinalDirection { get; set; }

        public GeoLine() { }
        public GeoLine(double value, GeoLineType lineType, CardinalDirection cardinalDirection)
        {
            if(lineType == GeoLineType.Latitude)
            {
                MaxDegrees = 90;
            }
            else
            {
                MaxDegrees = 180;
            }

            

            if(value < 0 && lineType == GeoLineType.Longitude && cardinalDirection == CardinalDirection.E)
            {
                throw new ArgumentException("A negative Longitude can only represent Western Meridians.");
            }

            if (value < 0 && lineType == GeoLineType.Latitude && cardinalDirection == CardinalDirection.N)
            {
                throw new ArgumentException("A negative Latitude can only represent Southern Parallels.");
            }

            if(lineType == GeoLineType.Latitude && (cardinalDirection == CardinalDirection.E || cardinalDirection == CardinalDirection.W))
            {
                throw new ArgumentException("Latitude only measures location relative to North and South.");
            }

            if (lineType == GeoLineType.Longitude && (cardinalDirection == CardinalDirection.N || cardinalDirection == CardinalDirection.S))
            {
                throw new ArgumentException("Longitude only measures location relative to East and West.");
            }

            if (Math.Abs(value) > Math.Abs(MaxDegrees))
            {
                throw new ArgumentException("Absolute Value of the measurement cannot be greater than the max");
            }
            var decimalValue = Convert.ToDecimal(value);

            decimalValue = Math.Abs(decimalValue);

            var degrees = Decimal.Truncate(decimalValue);
            decimalValue = (decimalValue - degrees) * 60;

            var minutes = Decimal.Truncate(decimalValue);
            var seconds = (decimalValue - minutes) * 60;

            Degrees = Convert.ToDouble(degrees);
            Minutes = Convert.ToDouble(minutes);
            Seconds = Convert.ToDouble(seconds);
            CardinalDirection = cardinalDirection;
        }
        public GeoLine(double degrees, double minutes, double seconds, GeoLineType lineType, CardinalDirection cardinalDirection)
        {
            if (lineType == GeoLineType.Latitude)
            {
                MaxDegrees = 90;
            }
            else
            {
                MaxDegrees = 180;
            }



            if (degrees < 0 && lineType == GeoLineType.Longitude && cardinalDirection == CardinalDirection.W)
            {
                throw new ArgumentException("A negative Longitude can only represent Eastern Meridians.");
            }

            if (degrees < 0 && lineType == GeoLineType.Latitude && cardinalDirection == CardinalDirection.N)
            {
                throw new ArgumentException("A negative Latitude can only represent Southern Parallels.");
            }

            if (lineType == GeoLineType.Latitude && (cardinalDirection == CardinalDirection.E || cardinalDirection == CardinalDirection.W))
            {
                throw new ArgumentException("Latitude only measures location relative to North and South.");
            }

            if (lineType == GeoLineType.Longitude && (cardinalDirection == CardinalDirection.N || cardinalDirection == CardinalDirection.S))
            {
                throw new ArgumentException("Longitude only measures location relative to East and West.");
            }

            if (Math.Abs(degrees) > Math.Abs(MaxDegrees))
            {
                throw new ArgumentException("Absolute Value of the measurement cannot be greater than the max");
            }
            Degrees = Math.Abs(degrees);
            Minutes = Math.Abs(minutes);
            Seconds = Math.Abs(seconds);
            CardinalDirection = cardinalDirection;
        }
        public double ToDouble()
        {
            var result = (Degrees) + (Minutes) / 60.0 + (Seconds) / 3600.0;
            return CardinalDirection == CardinalDirection.W || CardinalDirection == CardinalDirection.S ? -result : result;
        }
        public override string ToString()
        {
            return Degrees + "º " + Minutes + "' " + Seconds + "\"" + CardinalDirection;
        }
    }

    public enum GeoLineType
    {
        Latitude,
        Longitude
    }

    public enum CardinalDirection
    {
        N, E, S, W
    }    
}
