/* This program fills info to a form pdf we use with a certain customers
 * request for quotes
 *
 * Copyright(C) 2018  Ryan S. Hake
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ManifoldLister
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> manifolds = new List<string>();                //Create a list to store manifolds passed to this program
            for(int i=0; i<args.Count(); i++)                           //For loop that adds manifolds to this list
            {
                manifolds.Add(args[i]);
            }
            Console.WriteLine("Working...");
            var output = Search(manifolds);
            menu(output);                                               //Menu fills the screen and opens the directories
        }
        
        #region Menu writer
        /// <summary>
        /// This function will draw the menu on the screen and take input from the user, it calls another function to open the folder.
        /// </summary>
        /// <param name="manifoldList"></param>
        static void menu(Dictionary<string,string> manifoldList)
        {
            Console.Clear();                                        //Clear the working message from the console
            Console.WriteLine("Type the number for the folder you would like to open");
            Console.WriteLine("1.  All");


            for (int i=0; i < manifoldList.Count(); i++)            //Write the options to select
            {
                if(manifoldList.ElementAt(i).Value.Equals("Null"))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(i + 2 + ". {0}  -- Not Found", manifoldList.ElementAt(i).Key.ToUpper());
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(i + 2 + ".  {0}", manifoldList.ElementAt(i).Key.ToUpper());
                }
            }
            Console.WriteLine("To exit type e");                    
            
            while (true)                                            //While loop to read options from the console
            {
                string a = Console.ReadLine();                      //Get input from the console into a string.
                if (a.ToLower().Contains('e')) { return; }          //character test to see if the user entered e, will then exit loop
                Byte x = 0;
                if (!byte.TryParse(a, out x))                       //Will print message if a could not be converted to an integer byte.
                {
                    Console.WriteLine("Enter an integer please, this program isn't very smart");
                    goto Skip;
                }
                if (x<= manifoldList.Count()+1 && x>1)              //If value selected is one of the displayed manifolds then open that folder
                {
                    if (!manifoldList.ElementAt(x-2).Value.Equals("Null"))
                    {
                        Process.Start(manifoldList.ElementAt(x - 2).Value);
                    }
                }
                else if (x==1)                                      //Loop through and open all the folders
                {
                    foreach(var item in manifoldList)
                    {
                        if (!item.Value.Equals("Null"))
                        {
                            Process.Start(item.Value);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Try again");
                }
            Skip:;
                //Function to use process.start
            }
        }
        #endregion

        #region File Reader
        /// <summary>
        /// Function to read the tempfile, returns list of manifolds that were extracted from the e-mail program
        /// </summary>
        /// <param name="file"></param>
        /// <returnsname ="manifolds"></returns>
        public static List<string> fileReader(string file)
        {
            List<string> manifolds = new List<string>();
            using (StreamReader r = new StreamReader(file))
            {
                string line;
                while((line=r.ReadLine()) != null)
                {
                    manifolds.Add(line);
                }
            }
            return manifolds;
        }
        #endregion


        

        private static Dictionary<string,string> Search(List<string> matches)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            Parallel.ForEach(matches, e =>
            {
                // Single aluminum manifold
                string searchPath = string.Empty;
                string searchPattern = string.Empty;
                if (Regex.Match(e, @"\d\d?\w?sa-.+", RegexOptions.IgnoreCase).Success)
                {
                    searchPath = @"P:\Master Customer Files\Smartflow\0 -Assemblies\Manifolds\Aluminum\Single Aluminum Manifolds";
                    searchPattern = Regex.Match(e, @"\d\d?\w?sa-.+", RegexOptions.IgnoreCase).ToString();
                }
                // Parallel SST Manifolds
                else if (Regex.Match(e, @"\d\d?pss-.+", RegexOptions.IgnoreCase).Success || Regex.Match(e, @"\d\d?psl-.+", RegexOptions.IgnoreCase).Success)
                {
                    searchPath = @"P:\Master Customer Files\Smartflow\0 -Assemblies\Manifolds\Stainless Steel\Parallel";
                    searchPattern = Regex.Match(e, @"\d\d?ps\w-.+", RegexOptions.IgnoreCase).ToString();
                    searchPattern = Regex.Replace(searchPattern, @"-\w$", "");
                }
                // Single SST Manifolds
                else if (Regex.Match(e, @"\d\d?ss-.+", RegexOptions.IgnoreCase).Success || Regex.Match(e, @"\d\d?sl-.+", RegexOptions.IgnoreCase).Success)
                {
                    searchPath = @"P:\Master Customer Files\Smartflow\0 -Assemblies\Manifolds\Stainless Steel\Single";
                    searchPattern = Regex.Match(e, @"\d\d?s\w-.+", RegexOptions.IgnoreCase).ToString();
                    searchPattern = Regex.Replace(searchPattern, @"-\w$", "");
                }
                // DuoFlow Manifolds
                else if (Regex.Match(e, @"\d\d?\w?sda-.+", RegexOptions.IgnoreCase).Success)
                {
                    searchPath = @"P:\Master Customer Files\Smartflow\0 -Assemblies\Manifolds\Aluminum\DuoFlow";
                    searchPattern = Regex.Match(e, @"\d\d?\w?sda-.+", RegexOptions.IgnoreCase).ToString();
                }
                // SST High Temp Manifolds
                else if (Regex.Match(e, @"\d\d?\w?ssht-.+", RegexOptions.IgnoreCase).Success)
                {
                    Dictionary<string, string> highTempDict = new Dictionary<string, string>();
                    try
                    {
                        highTempDict = SearchHighTemp(e);
                    }
                    catch { }
                    if (highTempDict.Count() > 0)
                    {
                        try
                        {
                            foreach (var pair in highTempDict)
                            {
                                dict.Add(pair.Key, pair.Value);
                            }
                        }
                        catch { }
                    }
                }

                if (!string.IsNullOrEmpty(searchPath) && !string.IsNullOrEmpty(searchPattern))
                {
                    try
                    {
                        // Search kind of indexed folders for the manifolds
                        searchPattern = searchPattern + "*";
                        string[] fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                        if (fileArray.Count() > 0)
                        {
                            Array.Sort(fileArray);
                            dict.Add(e, Path.GetDirectoryName(fileArray.Last()));
                        }
                        else
                        {
                            searchPattern = searchPattern.Replace('r', 'Y').Replace('R', 'Y').Replace('b', 'Z').Replace('B', 'Z');
                            fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                            if (fileArray.Count() > 0)
                            {
                                Array.Sort(fileArray);
                                dict.Add(e, Path.GetDirectoryName(fileArray.Last()));
                            }
                            else
                            {
                                searchPattern = searchPattern.Replace('Y', 'R').Replace('y', 'R').Replace('Z', 'B').Replace('z', 'B');
                                fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                                if (fileArray.Count() > 0)
                                {
                                    Array.Sort(fileArray);
                                    dict.Add(e, Path.GetDirectoryName(fileArray.Last()));
                                }
                                else
                                {
                                    searchPattern = searchPattern.Replace('R', '*').Replace('B', '*');
                                    fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                                    if (fileArray.Count() > 0)
                                    {
                                        Array.Sort(fileArray);
                                        dict.Add(e, Path.GetDirectoryName(fileArray.Last()));
                                    }
                                    else
                                    {
                                        Dictionary<string, string> engineeringFolderDict = new Dictionary<string, string>();
                                        try
                                        {
                                          engineeringFolderDict=SearchEngineeringFolder(e);
                                        }
                                        catch { }
                                        if (engineeringFolderDict.Count() > 0)
                                        {
                                            try
                                            {
                                                foreach(var pair in engineeringFolderDict)
                                                {
                                                    dict.Add(pair.Key, pair.Value);
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            });
            return dict;
        }

        private static Dictionary<string,string> SearchEngineeringFolder(string a)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                string searchPattern = a;
                searchPattern = searchPattern + "*";
                string searchPath = @"P:\Engineering\Smartflow Products\Manifolds";
                string[] fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                if (fileArray.Count() > 0)
                {
                    Array.Sort(fileArray);
                    dict.Add(a, Path.GetDirectoryName(fileArray.Last()));
                }
                else
                {
                    searchPattern = searchPattern.Replace('r', 'Y').Replace('R', 'Y').Replace('b', 'Z').Replace('B', 'Z');
                    fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                    if (fileArray.Count() > 0)
                    {
                        Array.Sort(fileArray);
                        dict.Add(a, Path.GetDirectoryName(fileArray.Last()));
                    }
                    else
                    {
                        searchPattern = searchPattern.Replace('Y', '*').Replace('Z', '*');
                        fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                        if (fileArray.Count() > 0)
                        {
                            Array.Sort(fileArray);
                            dict.Add(a, Path.GetDirectoryName(fileArray.Last()));
                        }
                        else
                        {
                            dict.Add(a, "Null");                        //If nothing is found add a null key to display on menu
                        }
                    }
                }
            }
            catch (Exception) { }
            return dict;
        }

        static Dictionary<string,string> SearchHighTemp(string a)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            try
            {
                string searchPattern = a;
                //searchPattern = Regex.Replace(searchPattern, @"\t|\n|\r", "");
                searchPattern = Regex.Replace(searchPattern, @"-[a,A]$", "");
                searchPattern = searchPattern + "*";
                string searchPath = (@"P:\Engineering\Smartflow Products\Manifolds\Stainless Steel\NPE Show");
                string[] fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.TopDirectoryOnly);
                if (fileArray.Count() > 0)
                {
                    Array.Sort(fileArray);
                    dict.Add(a, Path.GetDirectoryName(fileArray.Last()));
                }
                else
                {
                    searchPath = (@"P:\Engineering\Smartflow Products\Manifolds\Stainless Steel\High Temp High Pressure");
                    fileArray = Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories);
                    if (fileArray.Count() > 0)
                    {
                        Array.Sort(fileArray);
                        dict.Add(a, Path.GetDirectoryName(fileArray.Last()));
                    }
                    else
                    {
                        dict.Add(a, "Null");
                    }
                }
            }
            catch (Exception) { }
            return dict;
        }

    }
}
