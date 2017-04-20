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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;

namespace Alligner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Funtions For Getting Necessary MetaData for an Image

        public string getRigRelatives(Dictionary<string, string> imgMetaData)
        {
            string rigRelatives = "";
            foreach (var item in imgMetaData)
            {
                if (item.Key.ToLower() == "camera:rigrelatives")
                    rigRelatives = item.Value;
            }
            return rigRelatives;
        }
        public string getPrincipalPoint(Dictionary<string, string> imgMetaData)
        {
            string PrincipalPoint = "";
            foreach (var item in imgMetaData)
            {
                if (item.Key.ToLower() == "camera:principalpoint")
                    PrincipalPoint = item.Value;
            }
            return PrincipalPoint;
        }
        public string getBlackLevelVector(Dictionary<string, string> imgMetaData)
        {
            string BlackLevelVector = "";
            foreach (var item in imgMetaData)
            {
                if (item.Key.ToLower() == "unknown tag (0xc61a)")
                    BlackLevelVector = item.Value;
            }
            return BlackLevelVector;
        }
        public string getVignettingPolynomial(Dictionary<string, string> imgMetaData)
        {
            string VignettingPolynomial = "";
            foreach (var item in imgMetaData)
            {
                if (item.Key.ToLower() == "camera:vignettingpolynomial2d[1]")
                    VignettingPolynomial = item.Value;
            }
            return VignettingPolynomial;
        }
        public string getVignettingPolynomialName(Dictionary<string, string> imgMetaData)
        {
            string VignettingPolynomialName = "";
            foreach (var item in imgMetaData)
            {
                if (item.Key.ToLower() == "camera:vignettingpolynomial2dname[1]")
                    VignettingPolynomialName = item.Value;
            }
            return VignettingPolynomialName;
        }

        #endregion

        public Dictionary<string, string> getImgMetaData(string path)
        {
            //string path = @"C:\Users\Awais Ali\Desktop\RedStickGolfCourse_151216\raw\IMG_161215_163145_0004_RED.tif";
            Dictionary<string, string> imgMetaData = new Dictionary<string, string>();
            var directories = ImageMetadataReader.ReadMetadata(path);
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    if (tag.Name != null & tag.Description != null)
                    {
                        imgMetaData.Add(tag.Name, tag.Description);
                    }
                }
                if (directory.HasError)
                {
                    foreach (var error in directory.Errors)
                        Console.WriteLine($"ERROR: {error}");
                }
            }

            var xmpDirectory = ImageMetadataReader.ReadMetadata(path).OfType<XmpDirectory>().FirstOrDefault();
            foreach (var property in xmpDirectory.XmpMeta.Properties)
            {
                if (property.Path != null & property.Value != null)
                {
                    imgMetaData.Add(property.Path, property.Value);
                }
            }

            return imgMetaData;
        }

        static void progress(float arg)
        {
            Console.WriteLine("PROGRESS = {0}", arg);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> imgMetaDataNIR = new Dictionary<string, string>();
            Dictionary<string, string> imgMetaDataRED = new Dictionary<string, string>();

            var inputDir = @"C:\Users\Awais Ali\Desktop\RedStickGolfCourse_151216\raw\NEW\";

            var inputPath = inputDir + "IMG_161215_163145_0004_RED.TIF";
            var referencePath = inputDir + "IMG_161215_163145_0004_NIR.TIF";
            var outputPath = inputDir + "IMG_161215_163145_0004_Output.TIF";

            var aligner = new IntelliGolf.Aligner(inputPath, referencePath, outputPath, progress, null);


            imgMetaDataNIR = getImgMetaData(referencePath);
            imgMetaDataRED = getImgMetaData(inputPath);

            aligner.RotationAngle = 180;
            string rigRelatives_input = getRigRelatives(imgMetaDataRED); // RED
            string rigRelatives_reference = getRigRelatives(imgMetaDataNIR); // NIR

            string principalPoint_input = getPrincipalPoint(imgMetaDataRED);// RED
            string principalPoint_reference = getPrincipalPoint(imgMetaDataNIR);// NIR

            // RED
            string InputBlackLevelVector = getBlackLevelVector(imgMetaDataRED);
            string InputVignettingPolynomial = getVignettingPolynomial(imgMetaDataRED);
            string InputVignettingPolynomialName = getVignettingPolynomialName(imgMetaDataRED);

            //NIR
            string ReferenceBlackLevelVector = getBlackLevelVector(imgMetaDataNIR);
            string ReferenceVignettingPolynomial = getVignettingPolynomial(imgMetaDataNIR);
            string ReferenceVignettingPolynomialName = getVignettingPolynomialName(imgMetaDataNIR);


            aligner.InputImageRigRelatives = rigRelatives_input;
            aligner.ReferenceImageRigRelatives = rigRelatives_reference;

            aligner.InputPrincipalPoint = principalPoint_input;
            aligner.ReferencePrincipalPoint = principalPoint_reference;

            aligner.InputBlackLevelVector = InputBlackLevelVector;
            aligner.InputVignettingPolynomial = InputVignettingPolynomial;
            aligner.InputVignettingPolynomialName = InputVignettingPolynomialName;

            aligner.ReferenceBlackLevelVector = ReferenceBlackLevelVector;
            aligner.ReferenceVignettingPolynomial = ReferenceVignettingPolynomial;
            aligner.ReferenceVignettingPolynomialName = ReferenceVignettingPolynomialName;

            try
            {
                aligner.Process();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
