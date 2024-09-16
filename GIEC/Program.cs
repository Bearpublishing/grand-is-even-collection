using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GIEC
{
    internal class Program
    {
        Int64 iterations;
        List<string> loadedMods = new List<string>();
        List<string> loadedModsWithExtentions = new List<string>();
        static void Main(string[] args)
        {
            var prog = new Program();
            Console.WriteLine("GIEC shell v1.0");
            prog.SetupDirStructure();
            prog.ExamineAndRunAutoexec();
            while(true)
            {
                Console.Write("[GIEC]> ");
                string commandRun = Console.ReadLine();
                prog.ExamineAndRunCommand(commandRun);
            }
        }
        public void ExamineAndRunCommand(string command)
        {
            var prog = new Program();
            // misc
            if (command == "c")
            {
                Console.Clear();
            }
            else if (command.StartsWith("cmdlist"))
            {
                try
                {
                    string listName = command.Remove(0, 8);
                    LoadAndExecuteCmdList(listName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                    return;
                }               
            }
            else if (command.StartsWith("echo"))
            {
                try
                {
                    string stringToEcho = command.Remove(0, 5);
                    Console.WriteLine(stringToEcho);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command == "remmod global")
            {
                try
                {
                    loadedMods.Clear();
                    loadedModsWithExtentions.Clear();
                    Console.WriteLine("All modules successfully removed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command == "listmod")
            {
                try
                {
                    foreach (string mod in loadedMods)
                    {
                        Console.WriteLine(mod);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("remmod"))
            {
                try
                {
                    string modName = command.Remove(0, 7);
                    if (loadedMods.Contains(modName))
                    {
                        loadedMods.Remove(modName);
                        loadedModsWithExtentions.Remove(modName + ".giecmod");
                        Console.WriteLine("giecmod '" + modName + "' removed successfully");
                    }
                    else
                    {
                        Console.WriteLine("'" + modName + "' is not a valid giecmod");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command == "insmod global")
            {
                try
                {
                    string[] modules = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods"));
                    foreach (string mod in modules)
                    {
                        string modName = Path.GetFileName(mod);                        
                        string modNameWithNoExt = modName.Remove(modName.Length - 8, 8);                        
                        if (!loadedMods.Contains(modNameWithNoExt))
                        {
                            loadedMods.Add(modNameWithNoExt);
                            loadedModsWithExtentions.Add(modName);
                            Console.WriteLine("Module '" + modNameWithNoExt + "' inserted successfully");
                        }
                        else
                        {
                            Console.WriteLine("Module '" + modNameWithNoExt + "' is already inserted");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }           
            else if (command.StartsWith("insmod"))
            {
                try
                {
                    string modName = command.Remove(0, 7);
                    InsertModule(modName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            // help
            else if (command == "help")
            {
                Console.WriteLine("GIEC shell v1.0");
                Console.WriteLine("is even checker usage: [time] isEvenChecker [[filein] fileinPath] [[fileout] fileoutPath] intToCheck [intToCheck] [intToCheck] ...");
                Console.WriteLine("Commands:");
                Console.WriteLine("help - shows this help");
                Console.WriteLine("c - clears the console");
                Console.WriteLine("cmdlist [cmdlist] - loads the given cmdlist found in /gieccmdlists/");
                Console.WriteLine("echo [stringToEcho] - echos the given string");
                Console.WriteLine("insmod [moduleName] - inserts the given module found in /giecmods/");
                Console.WriteLine("insmod global - inserts all modules found in /giecmods/");
                Console.WriteLine("listmod - lists all currently loaded modules");
                Console.WriteLine("remmod [moduleName] - removes the given currently inserted module");
                Console.WriteLine("remmod global - removes all inserted modules");
                Console.WriteLine("isEvenCheckers:");
                Console.WriteLine("standard - divides the number by 2 and checks if the remainder is equal to 0");
                Console.WriteLine("oliver - divides the number by 2 and then rounds it to the nearest int and the compares it to the original float");
                Console.WriteLine("oliveracc - same as oliver however it only uses the final digit of the supplied number");
                Console.WriteLine("swatch - counts down the number by one until it reaches 0 and the checks if the count is even via the same method");
                Console.WriteLine("gen - checks if the supplied int AND 1 is equal to 0");
                Console.WriteLine("sum - adds all of the digits together and then uses standard to see if that int is even");
                Console.WriteLine("Directory and file structure:");
                Console.WriteLine("autoexec.gieccmdlist - commands that are run on startup");
                Console.WriteLine("/gieccmdlists/ - contains all of the .gieccmdlist files");
                Console.WriteLine("/giecmods/ - contains all of the .giecmod files");
                Console.WriteLine("File types:");
                Console.WriteLine(".gieccmdlist - contains giec commands to be run; one line per command");
                Console.WriteLine(".giecmod - contains giec module code; must be placed inside a GIEC namespace, a class by the name of the file without the .giecmod extention and a run method containing the code to be ran when the module is ran; uses C#");
                Console.WriteLine("examples of these files can be found in their given directories");
            }
            // is even checkers
            else if (command.StartsWith("standard filein") && command.Contains("fileout"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string fileoutParm = command.Split(' ')[4];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.StandardArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Standard(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("standard filein"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }                    
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    Int64 numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.StandardArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Standard(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("standard fileout"))
            {
                try
                {
                    string fileoutParm = command.Split(' ')[2];                   
                    string isEvenString = command.Remove(0, 18 + fileoutParm.Length);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.StandardArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Standard(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }            
            else if (command.StartsWith("standard"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 9);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.StandardArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Standard(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }            
            else if (command.StartsWith("time standard"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 14);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 2;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        prog.StandardArray(isEvenArray);
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 14));
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        Console.WriteLine(prog.Standard(isEvenInt));
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliveracc filein") && command.Contains("fileout"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string fileoutParm = command.Split(' ')[4];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliveraccArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Oliveracc(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliveracc filein"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    Int64 numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliveraccArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Oliveracc(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliveracc fileout"))
            {
                try
                {
                    string fileoutParm = command.Split(' ')[2];
                    string isEvenString = command.Remove(0, 19 + fileoutParm.Length);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliveraccArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Oliveracc(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliveracc"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 10);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliveraccArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 10));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Oliveracc(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("time oliveracc"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 15);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 2;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        prog.OliveraccArray(isEvenArray);
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 15));
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        Console.WriteLine(prog.Oliveracc(isEvenInt));
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliver filein") && command.Contains("fileout"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string fileoutParm = command.Split(' ')[4];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliverArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Oliver(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliver filein"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    Int64 numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliverArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Oliver(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliver fileout"))
            {
                try
                {
                    string fileoutParm = command.Split(' ')[2];
                    string isEvenString = command.Remove(0, 15 + fileoutParm.Length);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliverArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Oliver(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("oliver"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 7);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.OliverArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 7));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Oliver(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("time oliver"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 12);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 2;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        prog.OliverArray(isEvenArray);
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 12));
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        Console.WriteLine(prog.Oliver(isEvenInt));
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("swatch"))
            {
                try
                {
                    Int64 isEvenInt = Int64.Parse(command.Remove(0, 7));
                    Console.Write("Calculating");
                    Console.WriteLine(prog.Swatch(isEvenInt));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("time swatch"))
            {
                try
                {
                    Int64 isEvenInt = Int64.Parse(command.Remove(0, 12));
                    Console.Write("Calculating");
                    Console.WriteLine(prog.Swatch(isEvenInt));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("gen filein") && command.Contains("fileout"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string fileoutParm = command.Split(' ')[4];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.GenArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Gen(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("gen filein"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    Int64 numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.GenArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Gen(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("gen fileout"))
            {
                try
                {
                    string fileoutParm = command.Split(' ')[2];
                    string isEvenString = command.Remove(0, 13 + fileoutParm.Length);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.GenArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Gen(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("gen"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 4);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.GenArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 4));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Gen(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("time gen"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 9);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 2;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        prog.GenArray(isEvenArray);
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        Console.WriteLine(prog.Gen(isEvenInt));
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("sum filein") && command.Contains("fileout"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string fileoutParm = command.Split(' ')[4];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.SumArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Sum(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("sum filein"))
            {
                try
                {
                    string fileinParm = command.Split(' ')[2];
                    string isEvenString = "";
                    if (File.Exists(fileinParm))
                    {
                        isEvenString = @File.ReadAllText(fileinParm);
                    }
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    Int64 numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.SumArray(isEvenArray);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Console.WriteLine(prog.Sum(isEvenInt));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("sum fileout"))
            {
                try
                {
                    string fileoutParm = command.Split(' ')[2];
                    string isEvenString = command.Remove(0, 14 + fileoutParm.Length);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        prog.SumArrayFileOut(isEvenArray, fileoutParm);
                        Console.WriteLine("Done");
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        StreamWriter sw = new StreamWriter(fileoutParm);
                        sw.WriteLine(prog.Sum(isEvenInt));
                        sw.Close();
                        Console.WriteLine("Done");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("sum"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 4);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 1;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");                        
                        prog.SumArray(isEvenArray);                        
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 4));
                        Console.Write("Calculating");                        
                        Console.WriteLine(prog.Sum(isEvenInt));                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (command.StartsWith("time sum"))
            {
                try
                {
                    string isEvenString = command.Remove(0, 9);
                    char[] delimiters = new char[] { ' ', '\r', '\n' };
                    int numOfInts = (command.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length) - 2;
                    if (numOfInts > 1)
                    {
                        Int64[] isEvenArray = isEvenString.Split(' ').Select(n => Convert.ToInt64(n)).ToArray();
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        prog.SumArray(isEvenArray);
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                    else
                    {
                        Int64 isEvenInt = Int64.Parse(command.Remove(0, 9));
                        Console.Write("Calculating");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        Console.WriteLine(prog.Sum(isEvenInt));
                        sw.Stop();
                        TimeSpan ts = sw.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine(elapsedTime);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else if (loadedMods.Contains(command))
            {
                try
                {
                    string fullModPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods", command + ".giecmod");
                    StreamReader sr = new StreamReader(fullModPath);
                    string codeToRun = sr.ReadToEnd();
                    sr.Close();
                    var provider = CodeDomProvider.CreateProvider("C#");
                    var result = provider.CompileAssemblyFromSource(new CompilerParameters(), codeToRun);
                    if (result.Errors.Count == 0)
                    {
                        var type = result.CompiledAssembly.GetType("GIEC." + command);
                        var instance = Activator.CreateInstance(type);
                        type.GetMethod("Run").Invoke(instance, null);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine(error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else
            {                                               
                Console.WriteLine("'" + command + "' is not recognised as an internal command");                                
            }
        }
        public void ExamineAndRunAutoexec()
        {
            try
            {
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoexec.gieccmdlist")))
                {
                    using(File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoexec.gieccmdlist"))) {}
                }
                string[] commands = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autoexec.gieccmdlist"));
                foreach (string command in commands)
                {
                    ExamineAndRunCommand(command);
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        public void SetupDirStructure()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gieccmdlists")))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gieccmdlists"));
                }
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods")))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods"));
                }
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods", "example.giecmod")))
                {
                    string stringToWrite = "namespace GIEC\r\n{\r\n    using System;\r\n    public class example\r\n    {\r\n        public void Run()\r\n        {\r\n            Console.WriteLine(\"Example\");\r\n        }\r\n    }\r\n}";
                    using (File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods", "example.giecmod"))){}
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods", "example.giecmod"), stringToWrite);
                }
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gieccmdlists", "example.gieccmdlist")))
                {
                    string stringToWrite = "echo example cmd list\r\nstandard 2\r\ninsmod example";
                    using (File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gieccmdlists", "example.gieccmdlist"))) { }
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gieccmdlists", "example.gieccmdlist"), stringToWrite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        public void LoadAndExecuteCmdList(string listName)
        {
            try
            {
                string listNameWithExtention;
                if (!listName.EndsWith(".gieccmdlist"))
                {
                    listNameWithExtention = listName + ".gieccmdlist";
                }
                else
                {
                    listNameWithExtention = listName;
                }      
                string[] commands = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gieccmdlists", listNameWithExtention));
                foreach (string command in commands)
                {
                    ExamineAndRunCommand(command);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        public void InsertModule(string moduleName)
        {
            try
            {
                string moduleNameWithExtention;
                if (!moduleName.EndsWith(".giecmod"))
                {
                    moduleNameWithExtention = moduleName + ".giecmod";
                }
                else
                {
                    moduleNameWithExtention = moduleName;
                }
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "giecmods", moduleNameWithExtention)))
                {
                    if (!loadedMods.Contains(moduleName))
                    {
                        loadedMods.Add(moduleName);
                        loadedModsWithExtentions.Add(moduleNameWithExtention);
                        Console.WriteLine("giecmod '" + moduleName + "' inserted successfully");
                    }                    
                    else
                    {
                        Console.WriteLine("giecmod '" + moduleName + "' is already inserted");
                    }
                }
                else
                {
                    Console.WriteLine("'" + moduleName + "' is not a valid giecmod");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
        public string Standard(Int64 isEvenInt)
        {
            Console.WriteLine(".");
            if (isEvenInt % 2 == 0)
            {
                return "Even";
            }
            else
            {
                return "Odd";
            }
        }
        public void StandardArray(Int64[] isEvenArray)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                if (suppliedInt % 2 == 0)
                {
                    Console.WriteLine("Even");
                }
                else
                {
                    Console.WriteLine("Odd");
                }
            }
        }
        public void StandardArrayFileOut(Int64[] isEvenArray, string fileoutParm)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                if (suppliedInt % 2 == 0)
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Even\n");
                    }
                }
                else
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Odd\n");
                    }
                }
            }
        }
        public string Oliver(Int64 isEvenInt)
        {
            Console.WriteLine(".");
            float dividedInt = (float)isEvenInt / 2;
            Int64 roundedInt = (Int64)Math.Round(dividedInt); 
            if (dividedInt == roundedInt)
            {
                return "Even";
            }
            else
            {
                return "Odd";
            }
        }
        public void OliverArray(Int64[] isEvenArray)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                float dividedInt = (float)suppliedInt / 2;
                Int64 roundedInt = (Int64)Math.Round(dividedInt);
                if (dividedInt == roundedInt)
                {
                    Console.WriteLine("Even");
                }
                else
                {
                    Console.WriteLine("Odd");
                }
            }
        }
        public void OliverArrayFileOut(Int64[] isEvenArray, string fileoutParm)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                float dividedInt = (float)suppliedInt / 2;
                Int64 roundedInt = (Int64)Math.Round(dividedInt);
                if (dividedInt == roundedInt)
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Even\n");
                    }
                }
                else
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Odd\n");
                    }
                }
            }
        }
        public string Oliveracc(Int64 isEvenInt)
        {
            Console.WriteLine(".");
            Int64 lastDigit = Math.Abs(isEvenInt) % 10;
            float dividedInt = (float)lastDigit / 2;
            Int64 roundedInt = (Int64)Math.Round(dividedInt);
            if (dividedInt == roundedInt)
            {
                return "Even";
            }
            else
            {
                return "Odd";
            }
        }
        public void OliveraccArray(Int64[] isEvenArray)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                Int64 lastDigit = Math.Abs(suppliedInt) % 10;
                float dividedInt = (float)lastDigit / 2;
                Int64 roundedInt = (Int64)Math.Round(dividedInt);
                if (dividedInt == roundedInt)
                {
                    Console.WriteLine("Even");
                }
                else
                {
                    Console.WriteLine("Odd");
                }
            }
        }
        public void OliveraccArrayFileOut(Int64[] isEvenArray, string fileoutParm)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                Int64 lastDigit = Math.Abs(suppliedInt) % 10;
                float dividedInt = (float)lastDigit / 2;
                Int64 roundedInt = (Int64)Math.Round(dividedInt);
                if (dividedInt == roundedInt)
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Even\n");
                    }
                }
                else
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Odd\n");
                    }
                }
            }
        }
        public string Gen(Int64 isEvenInt)
        {
            Console.WriteLine(".");
            if ((isEvenInt & 1) == 0)
            {
                return "Even";
            }
            else
            {
                return "Odd";
            }
        }
        public void GenArray(Int64[] isEvenArray)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                if ((suppliedInt & 1) == 0)
                {
                    Console.WriteLine("Even");
                }
                else
                {
                    Console.WriteLine("Odd");
                }
            }
        }
        public void GenArrayFileOut(Int64[] isEvenArray, string fileoutParm)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                if ((suppliedInt & 1) == 0)
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Even\n");
                    }
                }
                else
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Odd\n");
                    }
                }
            }
        }
        public string Sum(Int64 isEvenInt)
        {
            Console.WriteLine(".");
            int temp;
            int sum = 0;
            while(isEvenInt > 0)
            {
                temp = (int)isEvenInt % 10;
                sum += temp;
                isEvenInt /= 10;
            }
            if (sum % 2 == 0)
            {
                return "Even";
            }
            else
            {
                return "Odd";
            }
        }
        public void SumArray(Int64[] isEvenArray)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                int temp;
                int sum = 0;
                while (suppliedInt > 0)
                {
                    temp = (int)suppliedInt % 10;
                    sum += temp;
                    suppliedInt /= 10;
                }
                if (sum % 2 == 0)
                {
                    Console.WriteLine("Even");
                }
                else
                {
                    Console.WriteLine("Odd");
                }
            }            
        }
        public void SumArrayFileOut(Int64[] isEvenArray, string fileoutParm)
        {
            Console.WriteLine(".");
            foreach (Int64 i in isEvenArray)
            {
                Int64 suppliedInt = i;
                int temp;
                int sum = 0;
                while (suppliedInt > 0)
                {
                    temp = (int)suppliedInt % 10;
                    sum += temp;
                    suppliedInt /= 10;
                }
                if (sum % 2 == 0)
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Even\n");
                    }
                }
                else
                {
                    if (File.Exists(fileoutParm))
                    {
                        File.AppendAllText(fileoutParm, "Even\n");
                    }
                }
            }
        }
        public string Swatch(Int64 isEvenInt)
        {
            Thread.Sleep(500);
            Console.Write(".");
            Int64 count = 0;
            while (isEvenInt > 0)
            {
                isEvenInt--;
                count++;
            }
            iterations++;
            if (iterations == 25)
            {
                iterations = 0;
                return "Operation timed out!";
            }
            else if (Swatch(isEvenInt) == "Operation timed out!")
            {
                iterations = 0;               
                return "Operation timed out!";
            }
            else if (Swatch(isEvenInt) == "Even")
            {                
                return "Even";
            }
            else
            {                
                return "Odd";
            }
        }        
    }
}
