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

namespace WallpaperManager
{
    class Wallpapers
    {
        public static void indexer(List<string> directories)
        {
            List<List<imageNode>> parsedLibrary = null;
            List<bool> previouslyIndexed = null;

            List<List<string>> files = new List<List<string>>(directories.Count);

            for (int i = 0; i < directories.Count; i++)
            {
                files[i] = getFiles(directories[i]);

                if (files[i].Contains("wpmLib.xml"))
                    previouslyIndexed[i] = true;
                else
                    previouslyIndexed[i] = false;

                i++;
            }

            for (int i = 0; i < directories.Count; i++)
            {
                imageLib[i] = new List<imageNode>(files[i].Count() + 1);
            }

            for (int i = 0; i < directories.Count(); i++)
            {
                if (previouslyIndexed.Count() == 0)
                {
                    newLibrary(directories[i], i);
                }
                else
                {
                    parsedLibrary[i] = xmlParse(directories[i]);

                    var removedFiles = from wallpaper in parsedLibrary[i]
                                       where (!files[i].Contains(wallpaper.filePath))
                                       select wallpaper.filePath;

                    List<string> libraryFiles = new List<string>();

                    foreach (var wallpaper in parsedLibrary[i])
                    {
                        libraryFiles.Add(wallpaper.filePath);
                    }

                    var addedFiles = from file in files[i]
                                     where ((!libraryFiles.Contains(file)) && ((file.Contains(".png") || file.Contains(".jpg"))))
                                     select file;

                    if (addedFiles.Count() > 0)
                    {
                        List<string> aFiles = new List<string>();
                        foreach (var file in addedFiles)
                        {
                            aFiles.Add(file.ToString());
                        }
                        addToLib(aFiles, directories[i]);
                    }

                    if (removedFiles.Count() > 0)
                    {
                        List<string> rFiles = new List<string>();
                        foreach (var file in removedFiles)
                        {
                            rFiles.Add(file.ToString());
                        }
                        removeFromLib(rFiles, directories[i]);
                    }

                    libraryFiles.Clear();

                }
            }
        }

        private static void newLibrary(string dirPath, int i)
        {
            string[] files = Directory.GetFiles(dirPath);
            int height = 0,
                width = 0;
            double ratio = 0.0;
            imageLib[i] = new List<imageNode>(files[i].Count() + 1);

            foreach (string img in files)
            {
                if (img.Contains(".png") || img.Contains(".jpg"))
                {
                    imageNode newImage = new imageNode();
                    Image currIMG = Image.FromFile(img);

                    height = currIMG.Height;
                    width = currIMG.Width;
                    ratio = (double)width / height;
                    ratio = Math.Round(ratio, 2);

                    newImage.filePath = img;
                    newImage.aspectRatio = ratio;
                    imageLib[i].Insert(imageLib.Count, newImage);
                    currIMG.Dispose();

                }
            }

            xmlExport(imageLib[i], dirPath);
        }

        private static List<imageNode> xmlParse(string dirPath)
        {
            XDocument library = XDocument.Load(dirPath + libraryPath);
            int i = 0;
            var wallpapers = from item in library.Descendants("wallpaper")
                             select new
                             {
                                 filepath = item.Element("filepath"),
                                 aspectRatio = item.Element("aspectRatio")
                             };
            List<imageNode> images = new List<imageNode>(wallpapers.Count());

            foreach (var wallpaper in wallpapers)
            {
                imageNode image = new imageNode();
                image.aspectRatio = Convert.ToDouble(wallpaper.aspectRatio.Value);
                image.filePath = wallpaper.filepath.Value.ToString();
                images.Insert(i, image);
                i++;
            }

            return images;
        }

        private static bool xmlExport(List<imageNode> images, string dirPath)
        {
            XDocument lib = new XDocument(new XElement("wallpapers"));
            var root = lib.Root;

            foreach (imageNode image in images)
            {
                root.Add(new XElement("wallpaper",
                                new XElement("filepath", image.filePath),
                                new XElement("aspectRatio", image.aspectRatio)));

            }


            string libPath = dirPath + libraryPath;

            if (File.Exists(libPath))
                File.Delete(libPath);

            lib.Save(libPath);

            File.SetAttributes(libPath, FileAttributes.Hidden);
            return true;

        }

        private static void addToLib(List<string> addedFiles, string dirPath)
        {
            string filepath = dirPath + libraryPath;

            XDocument library = XDocument.Load(filepath);

            var root = library.Root;

            foreach (string file in addedFiles)
            {
                Image currIMG = Image.FromFile(file);
                double ratio = (double)currIMG.Width / currIMG.Height;
                ratio = Math.Round(ratio, 2);
                currIMG.Dispose();

                root.Add(new XElement("wallpaper",
                                new XElement("filepath", file),
                                new XElement("aspectRatio", ratio)));
            }
            File.Delete(filepath);
            library.Save(filepath);
        }

        private static void removeFromLib(List<string> removedFiles, string dirPath)
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
                    //Console.WriteLine(node.FirstChild.ToString());
                    if (node.InnerText.Contains(file))
                    {
                        node.ParentNode.RemoveChild(node);

                    }
                }
            }

            library.Save(filepath);
        }

        public List<imageNode> getImage(int screenWidth)
        {
            List<imageNode> images = null;
            double ratio = 1.8;  //calculations have shown the largest ratio is 1.8 between width and height.

            for (int i = 0; i < imageLib.Count; i++)
            {
                foreach(imageNode image in imageLib[i])
                {
                    if (((ratio * screenWidth) <= image.aspectRatio) && (image.aspectRatio < ((ratio + 1) * screenWidth)))
                    {
                        images.Add(image);
                    }
                }
            }
            return images;
        }

        private static List<string> getFiles(string directory)
        {
            List<string> files = null;
            List<string> directories = null;
            string[] tempFiles = null;
            string[] tempDir = null;

            if (recursiveImport)
            {
                tempDir = Directory.GetDirectories(directory);
                directories = tempDir.ToList();
                foreach (string dir in directories)
                {
                    files.AddRange(getFiles(dir));
                }
                tempFiles = Directory.GetFiles(directory);
                files.AddRange(tempFiles.ToList());
            }
            else
            {
                tempFiles = Directory.GetFiles(directory);
                files = tempFiles.ToList();
            }

            return files;
        }

        private static string libraryPath = "\\" + "\\" + "wpmLib.xml";
        public static bool recursiveImport { get; set; }
        public static List<string> libPath { get; set; }
        public static List<List<imageNode>> imageLib { get; set; }
    }

    

    class imageNode
    {
        public string filePath { get; set; }
        public double aspectRatio { get; set; }
    }

}