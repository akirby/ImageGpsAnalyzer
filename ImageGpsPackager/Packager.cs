using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using ImageAnalyzer.GPS;
using ImageAnalyzer;
using System.Xml.Serialization;
using System.ComponentModel;

namespace ImageGpsPackager
{
    public class Packager
    {
        public static void ExportCaseXml(Case caseFile, string folderName)
        {
            using (FileStream fs = new FileStream(Path.Combine(folderName, string.Format("CaseReport-{0}.xml", caseFile.CaseNumber)), FileMode.Create, FileAccess.ReadWrite))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Case));
                serializer.Serialize(fs, caseFile);
            }
        }

        public static Case GetCaseFromFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Case));
                Case caseFile = (Case)serializer.Deserialize(fs);
                return caseFile;
            }
        }

        public static void ExportCaseToDoc(Case caseFile, string mapImagePath, string folderName)
        {
            using (WordprocessingDocument wordprocessingDocument =
               WordprocessingDocument.Create(Path.Combine(folderName, string.Format("CaseReport-{0}.docx", caseFile.CaseNumber)), WordprocessingDocumentType.Document))
            {
                //wordprocessingDocument.AddMainDocumentPart();

                MainDocumentPart mainPart = wordprocessingDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());
                var para = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                var run = para.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());

                run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(string.Format("GPS Coordinate Report: Case #{0}", caseFile.CaseNumber)));
                ApplyStyleToParagraph(wordprocessingDocument, "Title", "Title", para);
                AddTable(body, caseFile.GPSCoordinates.Where(g => g.IncludedInMap).OrderBy(g => g.FileTime).ToList());
                using (FileStream fs = new FileStream(mapImagePath, FileMode.Open, FileAccess.Read))
                {
                    ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
                    imagePart.FeedData(fs);
                    AddImageToBody(wordprocessingDocument, mainPart.GetIdOfPart(imagePart));
                }

                //wordprocessingDocument.Save();
                wordprocessingDocument.Close();
            }

        }
        // Apply a style to a paragraph.
        public static void ApplyStyleToParagraph(WordprocessingDocument doc,
            string styleid, string stylename, Paragraph p)
        {
            // If the paragraph has no ParagraphProperties object, create one.
            if (p.Elements<ParagraphProperties>().Count() == 0)
            {
                p.PrependChild<ParagraphProperties>(new ParagraphProperties());
            }

            // Get the paragraph properties element of the paragraph.
            ParagraphProperties pPr = p.Elements<ParagraphProperties>().First();

            // Get the Styles part for this document.
            StyleDefinitionsPart part =
                doc.MainDocumentPart.StyleDefinitionsPart;

            // If the Styles part does not exist, add it and then add the style.
            if (part == null)
            {
                part = AddStylesPartToPackage(doc);
                AddNewStyle(part, styleid, stylename);
            }
            else
            {
                // If the style is not in the document, add it.
                if (IsStyleIdInDocument(doc, styleid) != true)
                {
                    // No match on styleid, so let's try style name.
                    string styleidFromName = GetStyleIdFromStyleName(doc, stylename);
                    if (styleidFromName == null)
                    {
                        AddNewStyle(part, styleid, stylename);
                    }
                    else
                        styleid = styleidFromName;
                }
            }

            // Set the style of the paragraph.
            pPr.ParagraphStyleId = new ParagraphStyleId() { Val = styleid };
        }

        // Return true if the style id is in the document, false otherwise.
        public static bool IsStyleIdInDocument(WordprocessingDocument doc,
            string styleid)
        {
            // Get access to the Styles element for this document.
            Styles s = doc.MainDocumentPart.StyleDefinitionsPart.Styles;

            // Check that there are styles and how many.
            int n = s.Elements<Style>().Count();
            if (n == 0)
                return false;

            // Look for a match on styleid.
            Style style = s.Elements<Style>()
                .Where(st => (st.StyleId == styleid) && (st.Type == StyleValues.Paragraph))
                .FirstOrDefault();
            if (style == null)
                return false;

            return true;
        }

        // Return styleid that matches the styleName, or null when there's no match.
        public static string GetStyleIdFromStyleName(WordprocessingDocument doc, string styleName)
        {
            StyleDefinitionsPart stylePart = doc.MainDocumentPart.StyleDefinitionsPart;
            string styleId = stylePart.Styles.Descendants<StyleName>()
                .Where(s => s.Val.Value.Equals(styleName) &&
                    (((Style)s.Parent).Type == StyleValues.Paragraph))
                .Select(n => ((Style)n.Parent).StyleId).FirstOrDefault();
            return styleId;
        }

        // Create a new style with the specified styleid and stylename and add it to the specified
        // style definitions part.
        private static void AddNewStyle(StyleDefinitionsPart styleDefinitionsPart,
            string styleid, string stylename)
        {
            // Get access to the root element of the styles part.
            Styles styles = styleDefinitionsPart.Styles;

            // Create a new paragraph style and specify some of the properties.
            Style style = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true
            };
            StyleName styleName1 = new StyleName() { Val = stylename };
            BasedOn basedOn1 = new BasedOn() { Val = "Normal" };
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = "Normal" };
            style.Append(styleName1);
            style.Append(basedOn1);
            style.Append(nextParagraphStyle1);

            // Create the StyleRunProperties object and specify some of the run properties.
            StyleRunProperties styleRunProperties1 = new StyleRunProperties();
            Bold bold1 = new Bold();
            Color color1 = new Color() { ThemeColor = ThemeColorValues.Text1 };
            RunFonts font1 = new RunFonts() { Ascii = "Calibri Light (Headings)" };
            // Specify a 12 point size.
            FontSize fontSize1 = new FontSize() { Val = "28" };
            styleRunProperties1.Append(bold1);
            styleRunProperties1.Append(color1);
            styleRunProperties1.Append(font1);
            styleRunProperties1.Append(fontSize1);

            // Add the run properties to the style.
            style.Append(styleRunProperties1);

            // Add the style to the styles part.
            styles.Append(style);
        }

        // Add a StylesDefinitionsPart to the document.  Returns a reference to it.
        public static StyleDefinitionsPart AddStylesPartToPackage(WordprocessingDocument doc)
        {
            StyleDefinitionsPart part;
            part = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            Styles root = new Styles();
            root.Save(part);
            return part;
        }
        // Take the data from a two-dimensional array and build a table at the 
        // end of the supplied document.
        public static void AddTable(Body body, List<GPSCoordinate> coordinates)
        {

            Table table = new Table();

            TableProperties props = new TableProperties(
                new TableBorders(
                new TopBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new BottomBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new LeftBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new RightBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new InsideHorizontalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                },
                new InsideVerticalBorder
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Size = 12
                }));
            

            table.AppendChild<TableProperties>(props);

            TableHeader header = new TableHeader();
            table.Append(GetHeaderRow());

            foreach(var gps in coordinates)
            {
                var row = new TableRow();
                row.Append(GetTableCellsFromGpsCoordinate(gps));
                table.Append(row);
            }
            
            body.Append(table);
        }

        private static TableRow GetHeaderRow()
        {
            Type gpsCoordType = typeof(GPSCoordinate);
            var props = gpsCoordType.GetProperties();
            var row = new TableRow();
            foreach (var gpsProp in props)
            {
                var tc = new TableCell();
                var attribs = gpsProp.GetCustomAttributes(typeof(DisplayNameAttribute), false);

                foreach(var a in gpsProp.CustomAttributes.Where(ca => ca.AttributeType == typeof(DisplayNameAttribute)))
                {
                    tc.Append(new Paragraph(new Run(new Text(a.ConstructorArguments[0].Value.ToString()))));
                    tc.Append(new TableCellProperties(
                        new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    row.Append(tc);
                }                
            }

            if (row.TableRowProperties == null)
                row.TableRowProperties = new TableRowProperties();

            row.TableRowProperties.AppendChild(new TableHeader());
            return row;
        }

        private static IEnumerable<TableCell> GetTableCellsFromGpsCoordinate(GPSCoordinate gps)
        {
            Type gpsCoordType = typeof(GPSCoordinate);
            var props = gpsCoordType.GetProperties();
            var result = new List<TableCell>();
            foreach(var gpsProp in props)
            {
                var attribs = gpsProp.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (attribs.Any())
                {
                    var tc = new TableCell();
                    var propVal = gpsProp.GetValue(gps);
                    if (propVal != null)
                    {
                        tc.Append(new Paragraph(new Run(new Text(propVal.ToString()))));
                        tc.Append(new TableCellProperties(
                            new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                        result.Add(tc);
                    }
                    else
                    {
                        tc.Append(new Paragraph(new Run(new Text(""))));
                        tc.Append(new TableCellProperties(
                            new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                        result.Add(tc);
                    }
                }
            }

            return result;
        }

        public static void InsertAPicture(string document, string fileName)
        {
            using (WordprocessingDocument wordprocessingDocument =
                WordprocessingDocument.Open(document, true))
            {
                MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;

                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    imagePart.FeedData(stream);
                }

                AddImageToBody(wordprocessingDocument, mainPart.GetIdOfPart(imagePart));
            }
        }

        private static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId)
        {
            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         new DW.Extent() { Cx = 3960000L, Cy = 3168000L },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = "Picture 1"
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = "New Bitmap Image.jpg"
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents() { Cx = 3960000L, Cy = 3168000L }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });

            // Append the reference to body, the element should be in a Run.
            wordDoc.MainDocumentPart.Document.Body.AppendChild(new Paragraph(new Run(element)));
        }
    }


}
