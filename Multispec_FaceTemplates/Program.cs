using System;
using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Licensing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multispec_FaceTemplates
{
    class Program
    {
        public const int Port = 5000;
        public const string Address = "/local";
        private string[] faceLicenseComponents = { "Biometrics.FaceExtractionFast" }; // "Biometrics.FaceMatchingFast", "Biometrics.FaceSegmentsDetection"
        private NBiometricClient client;

        private const string basePath = @"BASE PATH OF THE FOLDER HOLDING ALL SETS/SCENARIOS";
        private const string baseTemplatePath = @"C:\Users\John\Desktop\C#_Projects-Solutions\MultispecFiles\Templates\";

        static void Main(string[] args)
        {
            var obj = new Program();

            // Activate Licenses
            obj.StartClient();
            
            Console.ReadLine(); // Pause for testing

            // Get all sets
            string[] dirSets = Directory.GetDirectories(basePath + @"Sets\");
                
            // For every scenario, create templates for every image
            foreach(string set in dirSets)
            {
                // Mimic set/scenario folder structure with respect to the base template data folder
                string savePath = baseTemplatePath + @"Sets\" + (new DirectoryInfo(set).Name) + @"\";

                // Create directory path if it doesn't already exist
                Directory.CreateDirectory(savePath);

                // Create templates for the scenario
                obj.GenerateTemplates(set,savePath);
            }

            // Close out client
            obj.EndClient();

            Console.ReadLine(); // Pause for testing
        }

        private void StartClient()
        {
            foreach(string license in faceLicenseComponents)
            {
                if(NLicense.ObtainComponents(Address, Port, license))
                {
                    Console.WriteLine(string.Format("License was obtained: {0}",license));
                }
                else
                {
                    Console.WriteLine(string.Format("License was not obtained: {0}",license));
                }
            }

            Console.WriteLine();
            Console.Write("Initializing Client . . . . ");

            client = new NBiometricClient();
            client.BiometricTypes = NBiometricType.Face;
            client.Initialize();

            Console.WriteLine("Client Initialized!");
            Console.WriteLine();
        }

        private void EndClient()
        {
            Console.WriteLine();

            foreach(string license in faceLicenseComponents)
            {
                Console.WriteLine(string.Format("Releasing license: {0}",license));
                NLicense.ReleaseComponents(license);
            }

            Console.WriteLine();
            Console.Write("Disposing Client . . . . ");

            client.Dispose();

            Console.WriteLine("Client Disposed!");
        }

        private void GenerateTemplates(string directoryPath, string savePath)
        {
            // Get all files inside directory
            string[] fileList = Directory.GetFiles(directoryPath,"*.jpg",SearchOption.TopDirectoryOnly);

            // Loop through files
            foreach(string img in fileList)
            {
                // Create new NSubject and NFace objects
                using(var subject = new NSubject())
                using(var face = new NFace())
                {
                    // Set image to face
                    face.FileName = img;

                    // Add face to subject
                    subject.Faces.Add(face);

                    // Specify that we want a large template size
                    client.FacesTemplateSize = NTemplateSize.Large;

                    string filename = Path.GetFileNameWithoutExtension(img);
                    Console.Write(string.Format("Creating template for: {0} . . . . ", filename));

                    var status = client.CreateTemplate(subject);
                    if(status == NBiometricStatus.Ok)
                    {
                        File.WriteAllBytes(savePath + filename + ".dat", subject.GetTemplateBuffer().ToArray());
                        
                        Console.WriteLine("Template Saved");
                    }
                    else
                    {
                        Console.WriteLine("Template Failed");
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
