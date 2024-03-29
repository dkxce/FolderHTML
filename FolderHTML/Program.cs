﻿//
// C# 
// FolderHTML
// v 0.3, 02.07.2023
// https://github.com/dkxce/FolderHTML
// en,ru,1251,utf-8
//

using Force.Crc32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FolderHTML
{
    internal class Program
    {
        private const string DEFAULT_APP_HEADER = "(©) FolderHTML by dkxce";
        private const string DEFAULT_HTM_HEADER = "<span style=\"color:silver;\">by &copy; <b>FolderHTML</b></span>";
        private const string DEFAULT_HTM_FOOTER = "<b style=\"color:silver;\">&copy; FolderHTML by dkxce</b>";
        private const string DEFAULT_PREF_SPACE = "<span style=\"color:silver;\">.</span>   ";
        private const string DEFAULT_DELIM_SYMB = "|";
        private const string DEFAULT_PATH_SPACE = "\\---";
        private const int    DEFAULT_FILE_SPACE = 0030;
        private const int    DEFAULT_SIZE_SPACE = 0013;
        private const int    DEFAULT_SLEEP_VALU = 2500;

        private static string PREVIOUS_LINE = "";

        static void Main(string[] args)
        {            
            // HELP OUT
            if ((args != null && args.Length > 0) && (args[0] == "?" || args[0] == "/?" || args[0] == "-?" || args[0] == "--?"))
            { 
                WriteHelp(); 
                Environment.Exit(0); return; 
            };

            // INIT ARGS
            if (args != null && args.Length > 0) 
                InitArgs(args);

            // GET PATH
            if(!Directory.Exists(Parameters.path))
            {                
                Console.WriteLine($"Path: {Parameters.path}");
                Console.WriteLine("Not Found");
                System.Threading.Thread.Sleep(DEFAULT_SLEEP_VALU);
                Environment.Exit(1); return;
            };

            // GET FILES
            string[] files = Directory.GetFiles(Parameters.path, "*.*", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {                
                Console.WriteLine($"Path: {Parameters.path}");
                Console.WriteLine("No Files");
                System.Threading.Thread.Sleep(DEFAULT_SLEEP_VALU);
                Environment.Exit(2); return;
            };

            // GET ROOT PATH
            string same = FindSame(files);

            // Write Console Header
            WriteConsoleHeader(same, files.Length);

            // INIT OUT FILES
            InitOutFiles();

            // WRITE FILES HEADERS
            WriteOut($"Created at {DateTime.Now} {DEFAULT_HTM_HEADER}", Parameters.htmlFile, Parameters.textFile);
            foreach (string cl in Parameters.customLines) WriteOut(cl, Parameters.htmlFile, Parameters.textFile);
            WriteOut("|", Parameters.htmlFile, Parameters.textFile);

            // SET PREVIOUS PATH
            string prevPath = Path.GetDirectoryName(same);
            string prefSpace = "";
            
            if (Parameters.AddHeader)
            {
                string fileName = "FileName";
                string fileSpaces = ""; while (fileName.Length + fileSpaces.Length < Parameters.file_space) fileSpaces += " ";
                
                // FORMAT AND WRITE FILE
                string fileLine = $"{prefSpace}{DEFAULT_DELIM_SYMB}   ";
                fileLine += "<b>" + fileName + "</b>";
                if (Parameters.addSize || Parameters.addMdfd || Parameters.addCrtd || Parameters.addFlat) fileLine += $"{fileSpaces}";
                fileLine += "<span id=\"fileInfo\">";
                if (Parameters.addSize) fileLine += $" <b>{"FileSize",DEFAULT_SIZE_SPACE}</b>";
                if (Parameters.addMdfd) fileLine += " (LastWriteTime      )";
                if (Parameters.addCrtd) fileLine += " [CreationTime       ]";
                if (Parameters.addFlat) fileLine += " {Attributes}";
                if (Parameters.addAge) fileLine += " -LastWriteTimeUtc";
                if (Parameters.calcCRC32) fileLine += $" | CRC32SUM";
                if (Parameters.calcMD5) fileLine += $" | MD5HASH";
                fileLine += "</span>";
                WriteOut(fileLine, Parameters.htmlFile, Parameters.textFile);
                WriteOut($"{prefSpace}{DEFAULT_DELIM_SYMB}   ", Parameters.htmlFile, Parameters.textFile);
            };


            // WRITE FILES + DIRS INFOS
            foreach (string f in files)
            {
                string filePath = Path.GetDirectoryName(f);
                string fileName = Path.GetFileName(f);
                
                // another root path
                if (filePath != prevPath && !filePath.StartsWith(prevPath.Trim('\\')))
                {
                    prefSpace = "";
                    prevPath = FindSame(Path.GetDirectoryName(prevPath), Path.GetDirectoryName(filePath));
                    if (prevPath.Trim('\\') != same.Trim('\\')) 
                        foreach (string pp in prevPath.Substring(same.Length).Trim('\\').Split('\\')) 
                            prefSpace += DEFAULT_PREF_SPACE;
                };

                // new path -> write dir info
                if (filePath != prevPath && filePath.StartsWith(prevPath.Trim('\\')))
                {
                    string[] pathParts = filePath.Substring(prevPath.Length).Trim('\\').Split('\\');
                    string relx = prevPath.Trim('\\');
                    for(int i = 0;i< pathParts.Length;i++)
                    {
                        relx += "\\" + pathParts[i];
                        string pathPart = pathParts[i];
                        if(prefSpace.Length == 0 && !PREVIOUS_LINE.StartsWith(DEFAULT_DELIM_SYMB))
                            WriteOut($"{prefSpace}{DEFAULT_PREF_SPACE}", Parameters.htmlFile, Parameters.textFile);
                        else
                            WriteOut($"{prefSpace}{DEFAULT_DELIM_SYMB}", Parameters.htmlFile, Parameters.textFile);
                        string dirRelPath = relx.Substring(same.Length).Trim('\\').Replace("\\", "/");
                        WriteOut($"{prefSpace}{DEFAULT_PATH_SPACE}<a href=\"{dirRelPath}\">{pathPart}</a>", Parameters.htmlFile, Parameters.textFile);
                        prefSpace += DEFAULT_PREF_SPACE;
                        if (i == pathParts.Length - 1) WriteOut($"{prefSpace}{DEFAULT_DELIM_SYMB}", Parameters.htmlFile, Parameters.textFile);
                    };
                    prevPath = filePath;
                };                

                // skip out files
                if (f == Parameters.htmlFile) continue;
                if (f == Parameters.textFile) continue;
                if(Parameters.ExcludeFiles.Count > 0)
                {
                    string efn = Path.GetFileName(f);
                    bool excl = false;
                    foreach(string ef in Parameters.ExcludeFiles) 
                        if (efn == ef) { excl = true; break; }
                    if (excl) continue;
                };

                // get file info
                FileInfo fileInfo = new FileInfo(f);
                string fileSpaces = ""; while (fileName.Length + fileSpaces.Length < Parameters.file_space) fileSpaces += " ";
                string fileRelPath = f.Substring(same.Length).Trim('\\').Replace("\\", "/");

                // FORMAT AND WRITE FILE
                string fileLine = $"{prefSpace}{DEFAULT_DELIM_SYMB}   ";
                fileLine += $"<a href=\"{fileRelPath}\">{fileName}</a>";
                if (Parameters.addSize || Parameters.addMdfd || Parameters.addCrtd || Parameters.addFlat) fileLine += $"{fileSpaces}";
                fileLine += "<span id=\"fileInfo\">";
                if (Parameters.addSize) fileLine += $" <b>{BytesToString(fileInfo.Length),DEFAULT_SIZE_SPACE}</b>";
                if (Parameters.addMdfd) fileLine += $" ({fileInfo.LastWriteTime})";
                if (Parameters.addCrtd) fileLine += $" [{fileInfo.CreationTime}]";
                if (Parameters.addFlat) fileLine += $" {{{fileInfo.Attributes}}}";
                if (Parameters.addAge)  fileLine += $" -{DateTime.UtcNow - fileInfo.LastWriteTimeUtc}";
                if (Parameters.calcCRC32) fileLine += $" | {GetFileCRC32(fileInfo)}";
                if (Parameters.calcMD5) fileLine += $" | {GetFileMD5Hash(fileInfo)}";
                fileLine += "</span>";
                WriteOut(fileLine, Parameters.htmlFile, Parameters.textFile);
            };

            // write footer
            WriteOut($"{prefSpace}{DEFAULT_DELIM_SYMB}", Parameters.htmlFile, Parameters.textFile);
            WriteOut($"{DEFAULT_HTM_FOOTER} {DateTime.Now.Year}", Parameters.htmlFile, Parameters.textFile);

            if (Parameters.openHTML && Parameters.htmlFile != null) try { System.Diagnostics.Process.Start(Parameters.htmlFile); } catch { };
            if (Parameters.openTEXT && Parameters.textFile != null) try { System.Diagnostics.Process.Start(Parameters.textFile); } catch { };

            if (Parameters.wait) System.Threading.Thread.Sleep(DEFAULT_SLEEP_VALU);
        }

        public static string GetFileMD5Hash(FileInfo fi) => GetFileMD5Hash(fi.FullName);

        public static string GetFileMD5Hash(string fileName)
        {
            try {
                using (var md5 = MD5.Create())
                    using (var stream = File.OpenRead(fileName))
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToUpperInvariant();
            } catch { };
            return "";
        }

        public static string GetFileCRC32(FileInfo fi) => GetFileCRC32(fi.FullName);

        public static string GetFileCRC32(string fileName)
        {
            int readed = 0;
            uint initial = 0;
            FileStream fs = null;
            try
            {
                byte[] buffer = new byte[524288]; // 512 KB
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                while(true)
                {
                    readed = fs.Read(buffer, 0, buffer.Length);
                    if(readed == 0) break;
                    initial = Crc32Algorithm.Append(initial, buffer, 0, readed);
                };
                return initial.ToString("X8");
            }
            catch { return ""; }
            finally { if (fs != null) fs.Close(); };
        }

        private static void WriteHelp()
        {
            Console.WriteLine($"{DEFAULT_APP_HEADER} ( https://github.com/dkxce/FolderHTML )");
            Console.WriteLine("Usage:");
            Console.WriteLine("          > folderhtml [flags] [<path> [line] [line] ... [line]]");
            Console.WriteLine("");
            Console.WriteLine("Flags:");
            Console.WriteLine("  -?      - Help");
            Console.WriteLine("  -0      - Add Header");
            Console.WriteLine("  -s      - Add File Sizes");
            Console.WriteLine("  -m      - Add File Modified");
            Console.WriteLine("  -c      - Add File Created");
            Console.WriteLine("  -a      - Add File Attributes");
            Console.WriteLine("  -o      - File Age (Since Now)");
            Console.WriteLine("  -g      - Tabled HTML (Grid)");
            Console.WriteLine("  -G      - Tabled HTML (Grid) with border");
            Console.WriteLine("  -w      - Wait on done");
            Console.WriteLine("  -x      - Add CRC32 File Hash");
            Console.WriteLine("  -X      - Add MD5 File Hash");
            Console.WriteLine("  -h      - Write Out HTML file filelist.html");
            Console.WriteLine("  -H      - Write Out HTML file filelist.html and open it");
            Console.WriteLine("  -t      - Write Out TEXT file filelist.html");
            Console.WriteLine("  -T      - Write Out TEXT file filelist.html and open it");
            Console.WriteLine("  -d=..   - File Names Space Alignment (0..99) default = 30");
            Console.WriteLine("  -H=...  - Write Out custom HTML file filelist.html");
            Console.WriteLine("  -H=...  - Write Out custom HTML file filelist.html and open it");            
            Console.WriteLine("  -t=...  - Write Out custom TEXT file filelist.html");
            Console.WriteLine("  -T=...  - Write Out custom TEXT file filelist.html and open it");            
            Console.WriteLine("  -e=...  - Exclude file(s) with name(s)");            
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine("          > folderhtml");
            Console.WriteLine("          > folderhtml -smcawGH");
            Console.WriteLine("          > folderhtml -s -m -c -a -d=50");            
            Console.WriteLine("          > folderhtml -h %CD% \"DEMO LINE\"");
            Console.WriteLine("          > folderhtml -t -H %CD% \"LINE 1\" \"LINE 2\"");
            Console.WriteLine(DEFAULT_APP_HEADER);
            System.Threading.Thread.Sleep(DEFAULT_SLEEP_VALU);
        }

        private static void WriteConsoleHeader(string path, int files)
        {
            Console.WriteLine(DEFAULT_APP_HEADER);
            Console.WriteLine("*");
            Console.WriteLine($"* CmdL: {Environment.CommandLine}");
            Console.WriteLine($"* Path: {path}");
            if (Parameters.textFile != null) Console.WriteLine($"* TEXT: {Parameters.textFile}, open: {Parameters.openTEXT}");
            if (Parameters.htmlFile != null) Console.WriteLine($"* HTML: {Parameters.htmlFile}, open: {Parameters.openHTML}");
            Console.WriteLine($"* Files: {files}");
            Console.WriteLine("*");
        }

        private static void InitOutFiles()
        {
            if (!string.IsNullOrEmpty(Parameters.htmlFile)) Parameters.htmlFile = Parameters.htmlFile.Contains(":") ? Parameters.htmlFile : Path.Combine(Parameters.path, Parameters.htmlFile);
            if (!string.IsNullOrEmpty(Parameters.textFile)) Parameters.textFile = Parameters.textFile.Contains(":") ? Parameters.textFile : Path.Combine(Parameters.path, Parameters.textFile);
            if (Parameters.htmlFile != null && File.Exists(Parameters.htmlFile)) File.Delete(Parameters.htmlFile);
            if (Parameters.textFile != null && File.Exists(Parameters.textFile)) File.Delete(Parameters.textFile);
        }

        private static void InitArgs(string[] args)
        {
            bool flags = true;
            foreach (string a in args)
            {
                if (flags)
                {
                    if (a.StartsWith("-") && !a.Contains("="))
                    {
                        if (a.Contains("0")) Parameters.AddHeader = true;
                        if (a.Contains("s")) Parameters.addSize = true;
                        if (a.Contains("o")) Parameters.addAge = true;
                        if (a.Contains("g")) Parameters.tabled = true;
                        if (a.Contains("G")) { Parameters.tabled = true; Parameters.border = true; }
                        if (a.Contains("m")) Parameters.addMdfd = true;
                        if (a.Contains("c")) Parameters.addCrtd = true;
                        if (a.Contains("a")) Parameters.addFlat = true;
                        if (a.Contains("w")) Parameters.wait = true;
                        if (a.Contains("x")) Parameters.calcCRC32 = true;
                        if (a.Contains("X")) Parameters.calcMD5 = true;
                        if (a.Contains("h") || a.Contains("H")) Parameters.htmlFile = "filelist.html";
                        if (a.Contains("t") || a.Contains("T")) Parameters.textFile = "filelist.txt";
                        if (a.Contains("H")) Parameters.openHTML = true;
                        if (a.Contains("T")) Parameters.openTEXT = true;
                    };
                    if ((a.StartsWith("-h=") || a.StartsWith("-H=")) && a.Length > 3)
                    {
                        Parameters.htmlFile = a.Substring(3);
                        if (a.StartsWith("-H=")) Parameters.openHTML = true;
                    };
                    if ((a.StartsWith("-t=") || a.StartsWith("-T=")) && a.Length > 3)
                    {
                        Parameters.textFile = a.Substring(3);
                        if (a.StartsWith("-T=")) Parameters.openTEXT = true;
                    };
                    if ((a.StartsWith("-d=") && a.Length > 3 && a.Length < 6 && byte.TryParse(a.Substring(3), out byte val))) Parameters.file_space = val;
                    if ((a.StartsWith("-e=") && a.Length > 3)) Parameters.ExcludeFiles.Add(a.Substring(3));
                    if (!a.StartsWith("-")) { Parameters.path = a.Trim('\\','"').Replace("\\\\","\\"); flags = false; };
                    continue;
                };
                Parameters.customLines.Add(a);
            };
        }

        private static string FindSame(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return null;
            string r = "";
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
                if (a[i] == b[i]) r += a[i]; else break;
            return r;
        }

        private static string FindSame(string[] list)
        {
            if (list == null || list.Length == 0) return null;
            if (list.Length == 1) return list[0];
            string res = FindSame(list[0], list[1]);
            for (int i = 2; i < list.Length; i++)
                res = FindSame(res, list[i]);
            return res;
        }

        private static void WriteOut(string line, string htmlFile = null, string txtFile = null)
        {
            string text = Regex.Replace(line, "<.*?>", "").Replace("&copy;", "(©)");

            // CONSOLE
            Console.WriteLine(text.Replace("   "," ").Replace("---","-"));

            // TEXT
            if (!string.IsNullOrEmpty(txtFile))
            {
                FileStream fs = new FileStream(txtFile, FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine(text);
                sw.Close();
                fs.Close();
            };

            // HTML
            if (!string.IsNullOrEmpty(htmlFile))
            {
                FileStream fs = new FileStream(htmlFile, FileMode.OpenOrCreate, FileAccess.Write);
                fs.Position = fs.Length;
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                if (Parameters.tabled)
                {
                    if (fs.Position == 0)
                    {
                        if (Parameters.border)
                            sw.WriteLine("<table cellpadding=\"0\" cellspacing=\"0\">\r\n<style> td{ border-bottom: dashed 1px #E0E0E0;} </style>");
                        else
                            sw.WriteLine("<table cellpadding=\"0\" cellspacing=\"0\">");
                    }
                    else fs.Position = fs.Position - 10 /* </table> + crlf */;
                };
                while (line.IndexOf("  ") >= 0) line = line.Replace("  ", "&nbsp; ");
                int iofi = line.IndexOf("<span id=\"fileInfo\">");
                if (Parameters.tabled)
                {
                    if (iofi > 0)
                        sw.WriteLine($"<tr><td><TT>{line.Substring(0, iofi)}</TT></td><td><TT>{line.Substring(iofi)}</TT></td></tr>");
                    else
                        sw.WriteLine($"<tr><td colspan=\"2\"><TT>{line}</TT></td></tr>");
                }
                else
                    sw.WriteLine($"<TT>{line}</TT><br/>");
                if (Parameters.tabled) sw.WriteLine("</table>");
                sw.Close();
                fs.Close();
            };

            PREVIOUS_LINE = line;
        }

        /// <summary>
        ///     Readable Size (b, kb, mb, gb, tb)
        /// </summary>
        /// <param name="size">Data Size</param>
        /// <returns></returns>
        public static string BytesToString(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            };
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
        }

        private static class Parameters
        {
            public static int file_space  = DEFAULT_FILE_SPACE;
            public static string htmlFile = null;
            public static string textFile = null;
            public static string path     = Environment.CurrentDirectory;
            public static bool openHTML   = false;
            public static bool openTEXT   = false;
            public static bool addSize    = false;
            public static bool addAge     = false;
            public static bool tabled     = false;
            public static bool border     = false;
            public static bool addMdfd    = false;
            public static bool addCrtd    = false;
            public static bool addFlat    = false;
            public static bool wait       = false;
            public static bool calcMD5    = false;
            public static bool calcCRC32  = false;
            public static bool AddHeader  = false;
            public static List<string> customLines = new List<string>();
            public static List<string> ExcludeFiles = new List<string>();
        }
    }
}
