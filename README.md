# ImageGpsAnalyzer
This project was originally produced as a Masters project related to digital forensics. The full paper can be found <a href="https://github.com/akirby/ImageGpsAnalyzer/blob/master/ImageGpsAnalyzer.pdf">here</a>

##Overview

This project reads JPEG images and parses the EXIF information in order to plot the images as events on a geographic timeline. Version 1 uses the Bing Maps API, discussed in the next section, to plot the points. This is a C# .NET solution requiring .NET Version 4.6.1.

###Bing Maps API
The Bing Maps API, at the time of this writing requires that each instance be deployed with a license key.  To generate a key, you can create a log in for the <a href = "https://www.bingmapsportal.com">Bing Maps API Dev Center </a>

