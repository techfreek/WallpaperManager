using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace WallpaperManager
{
    class Wallpapers
    {
        public Wallpapers(List<string> directories, bool recursion)
        { //Sets initial values, starts allocating memory for various data
            libPath = directories;
            recursiveImport = recursion;
            imageLib = new List<List<imageNode>>();
            libFiles = new List<List<string>>();
        }

        public Wallpapers()
        {
            libPath.Clear();
            imageLib.Clear();
        }

        public void indexer()
        {
            Stopwatch fileRead = new Stopwatch(),
                XMLoutput = new Stopwatch(),
                buildTime = new Stopwatch(),
                addTime = new Stopwatch(),
                removeTime = new Stopwatch();

            List<List<imageNode>> parsedLibrary = new List<List<imageNode>>();
            List<bool> previouslyIndexed = new List<bool>();

            List<List<string>> files = new List<List<string>>();

            //I had some trouble dealing with these lists, so I ended up needing to allocate the memory inside each value of the list
            for (int i = 0; i < libPath.Count; i++)
            {
                files.Add(new List<string>());
                parsedLibrary.Add(new List<imageNode>());
                previouslyIndexed.Add(false);
                imageLib.Add(new List<imageNode>());
                libFiles.Add(new List<string>());
            }

            //I am utilizing for loops due to the number of lists I am using
            for (int i = 0; i < libPath.Count; i++)
            {
                //Gives the filename of the library, so we don't have to scan inside each filename
                string libFilePath = libPath[i] + "\\" + "wpmLib.xml";

                fileRead.Start();
                libFiles[i].AddRange(getFiles(libPath[i]));
                fileRead.Stop();

                if (libFiles[i].Contains(libFilePath))
                {
                    parsedLibrary[i] = xmlParse(libPath[i]); //At times, the library can exist, but remain empty
                    if (parsedLibrary[i] != null)
                    {
                        previouslyIndexed[i] = true;
                    }
                    
                }
            }

            for (int i = 0; i < libPath.Count(); i++)
            {
                if (!previouslyIndexed[i])
                {
                    buildTime.Start();
                    newLibrary(libPath[i], i);
                    buildTime.Stop();

                    xmlExport(libPath[i], i);
                    Console.WriteLine("Library build time: " + buildTime.Elapsed);
                }
                else
                {
                    addTime.Start();
                    List<string> libraryFiles = new List<string>();

                    foreach (var wallpaper in parsedLibrary[i])
                    { //Since I am only using filenames, I have to copy them out of the image files i parsed. Using the parsedLibrary[i] became difficult when I had to find the added files
                        libraryFiles.Add(wallpaper.filePath);
                    }

                    
                    //Algorithm: Looks at every image file that has been scanned in, and not inputted into the library
                    var addedFiles = from file in libFiles[i]
                                     where ((!libraryFiles.Contains(file)) && ((file.Contains(".png") || file.Contains(".jpg"))))
                                     select file;

                    imageLib[i].AddRange(parsedLibrary[i]);

                    if (addedFiles.Count() > 0)
                    {   //addedFiles is a temporary var, so I needed to convert it to a more permanent variable so I could add it to the library
                        List<string> aFiles = new List<string>();
                        foreach (var file in addedFiles)
                        {
                            aFiles.Add(file.ToString());
                        }
                        addToLib(aFiles, libPath[i], i);
                    }
                    addTime.Stop();

                    removeTime.Start();
                    //Algorithm: looks at every inputted filepath, and if the scanned files doesn't contain that file path, add it to a list
                    var removedFiles = from wallpaper in parsedLibrary[i]
                                       where (!libFiles[i].Contains(wallpaper.filePath))
                                       select wallpaper.filePath;

                    if (removedFiles.Count() > 0)
                    {
                        List<string> rFiles = new List<string>();
                        foreach (var file in removedFiles)
                        {
                            rFiles.Add(file.ToString());
                        }
                        removeFromLib(rFiles, libPath[i], i);
                    }
                    removeTime.Stop();

                    //Remove unneeded data
                    libraryFiles.Clear();

                    Console.WriteLine("Add time: " + addTime.Elapsed);
                    Console.WriteLine("Remove time: " + removeTime.Elapsed);
                }
                
            }
        }

        private static void newLibrary(string dirPath, int i)
        {
            imageLib[i] = new List<imageNode>();
            string fileType = "";            

            foreach (string img in libFiles[i])
            {
                fileType = Path.GetExtension(img);
                if (fileType.Equals(".png") || fileType.Equals(".jpg"))
                {
                    imageNode newImage = getEXIF(img);
                    imageLib[i].Add(newImage);
                }
            }
        }

        private static List<imageNode> xmlParse(string dirPath)
        {
            //File has already proven to exist, don't need to double check existance
            XDocument library = XDocument.Load(dirPath + libraryPath);

            //Pulls in raw data, still haven't figured out how to parse directly to a different datatype
            var wallpapers = from item in library.Descendants("wallpaper")
                             select new
                             {
                                 filepath = item.Element("filepath"),
                                 aspectRatio = item.Element("aspectRatio")
                             };

            //Not saved directly to imageLib as more processing is needed
            List<imageNode> images = new List<imageNode>();
            string tempAspect = "";

            foreach (var wallpaper in wallpapers)
            {
                imageNode image = new imageNode();
                image.filePath = wallpaper.filepath.Value.ToString();
                tempAspect = wallpaper.aspectRatio.Value.ToString();
                image.aspectRatio = Convert.ToDouble(wallpaper.aspectRatio.Value.ToString());
                images.Add(image);
            }
            if (images.Count() == 0)
                return null;

            return images;
        }

        private static bool xmlExport(string dirPath, int i)
        {
            //1 library per root folder, so exported individually (at least to avoid confusion)
            XDocument lib = new XDocument(new XElement("wallpapers"));
            var root = lib.Root;

            foreach (imageNode image in imageLib[i])
            {
                root.Add(new XElement("wallpaper",
                                new XElement("filepath", image.filePath),
                                new XElement("aspectRatio", image.aspectRatio)));
            }


            string libPath = dirPath + libraryPath;

            //Working on a better method of overwriting
            if (File.Exists(libPath))
                File.Delete(libPath);

            
            lib.Save(libPath);
            
            //Hides the file from power users
            File.SetAttributes(libPath, FileAttributes.Hidden);
            return true;

        }

        private static void addToLib(List<string> addedFiles, string dirPath, int libIndex)
        {
            string filepath = dirPath + libraryPath;
            XDocument library = XDocument.Load(filepath);
            var root = library.Root;

            foreach (string file in addedFiles)
            {
                //Don't need to save to imageNode as the data would have to be extracted it directly
                imageNode currIMG = getEXIF(file);

                //Add to image library to avoid reparsing
                imageLib[libIndex].Add(currIMG);

                //Add to XML file
                root.Add(new XElement("wallpaper",
                                new XElement("filepath", currIMG.filePath),
                                new XElement("aspectRatio", currIMG.aspectRatio)));
            }
            //overwriting operations
            File.Delete(filepath);
            library.Save(filepath);
            File.SetAttributes(filepath, FileAttributes.Hidden);
        }

        private static void removeFromLib(List<string> removedFiles, string dirPath, int libIndex)
        {
            string filepath = dirPath + libraryPath;

            XmlDocument library = new XmlDocument();
            library.Load(filepath);

            XmlElement root = library.DocumentElement;
            XmlNodeList wallpapers = root.ChildNodes;

            foreach (string file in removedFiles)
            {
                foreach (XmlNode node in wallpapers)
                {
                    //Checks all of the nodes to see if they contain the same filepath, if so, it is deleted.
                    //I intend to re-write this to minimize the time complexity here which is usually greater than removedFiles^2
                    if (node.InnerText.Contains(file))
                    {
                        node.ParentNode.RemoveChild(node);
                        break; //Cuts down on processing. There should not be duplicates

                    }
                }
                foreach (imageNode image in imageLib[libIndex])
                {
                    if (image.filePath.Equals(file))
                    {
                        imageLib[libIndex].Remove(image);
                        break; //Cuts down on processing. There should not be two files of the same name
                    }
                }
            }

            File.Delete(filepath);
            library.Save(filepath);
            File.SetAttributes(filepath, FileAttributes.Hidden);
        }

        //Gets a list of wallpapers that match a specified number of monitors
        //Designed on the notion that most people use 16:9/16:10 monitors. Wider support will come later.
        public List<string> getImageOfWidth(int screenWidth)
        {
            List<string> images = new List<string>();
            double ratio = 1.8, //calculations have shown the largest ratio is 1.8 between width and height.
                min = ratio * (screenWidth - 1),
                max = ratio * screenWidth;

            //I do not have a single unified library, so I must iterate through the individual libraries.
            for (int i = 0; i < imageLib.Count; i++) //using loops instead of queries because I have a list of libraries and that is harder to query
            {
                foreach(imageNode image in imageLib[i])
                {
                    //Checks to make sure the image is wider (monitors - 1), and narrower than (monitors + 1)
                    if ((min <= image.aspectRatio) && (image.aspectRatio < max))
                    {
                        images.Add(image.filePath);
                    }
                }
            }
            return images;
        }

        private static imageNode getEXIF(string filename)
        {
            //A few functions call on this, makes it easier than updating code in a few places.
            imageNode newImage = new imageNode();
            Image currIMG = Image.FromFile(filename);

            double ratio = (double)currIMG.Width / currIMG.Height;
            ratio = Math.Round(ratio, 2); //Accuracy is not too crucial here

            newImage.filePath = filename;
            newImage.aspectRatio = ratio;
            currIMG.Dispose(); //Free up memory
            return newImage;
        }

        private static List<string> getFiles(string directory)
        { //Implemented to get files recursively/non-recursively easier. It also converts it to a list which is currently my preferred method for accessing the filenames
            List<string> files = new List<string>();
            List<string> directories = null;
            string[] tempFiles = null;
            string[] tempDir = null;

            if (recursiveImport)
            {
                tempDir = Directory.GetDirectories(directory);
                directories = tempDir.ToList();
                
                //Add local files to list
                tempFiles = Directory.GetFiles(directory);
                files = tempFiles.ToList();

                foreach (string dir in directories)
                {
                    //Each getFiles returns a list of all the files, and we can merge to the existing list with AddRange
                    files.AddRange(getFiles(dir));
                    
                }
            }
            else
            {
                //Otherwise only local files are returned
                tempFiles = Directory.GetFiles(directory);
                files = tempFiles.ToList();
            }
            return files;
        }

        //Used in many operations when using it. Made sense to write it once instead of re-writing each time
        private static string libraryPath =  "\\" + "wpmLib.xml";

        //Variables used in the general operation of the program
        public static bool recursiveImport { get; set; }
        public static List<string> libPath { get; set; }
        public static List<List<imageNode>> imageLib { get; set; }
        public static List<List<string>> libFiles { get; set; }
    }

    

    class imageNode
    {
        public string filePath { get; set; }
        public double aspectRatio { get; set; }
    }

}