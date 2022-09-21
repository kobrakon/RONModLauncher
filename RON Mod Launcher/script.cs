using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Reflection;

string gamePath = string.Empty;
string configpath = string.Empty;
string paks = string.Empty;
string gamePaks = string.Empty;
string vo = string.Empty;
string banks = string.Empty;
string me = string.Empty;
string importantfileithinkpath = string.Empty;
string modvo = string.Empty;
string modbanks = string.Empty;
string gamevotemp = string.Empty;
string gamebanktemp = string.Empty;
List<string> gamevoli = new();
List<string> mevoli = new();
List<string> gamebankli = new();
List<string> mebankli = new();
Process gameProcess = new();
bool IsOverride = false;

Main();

void Main()
{
    me = Assembly.GetExecutingAssembly().Location.TrimEnd(@"RON Mod Launcher.dll".ToCharArray());
    importantfileithinkpath = $@"{me}\importantfile.txt";
    configpath = $@"{me.Replace("\n", "").Replace("\r", "")}\config.xml";
    Console.WriteLine("RON Mod Launcher Started");
    Console.WriteLine(Environment.NewLine);

    if (!File.Exists(@$"{me}\importantfile.txt"))
    {
        Console.WriteLine("Data file not found! Making another...");
        HandleFirstTime();
        HandleIO();
        Console.ReadLine();
    } else
    HandleIO();
    Console.ReadLine(); // seriously who decided that the console should just auto kill itself when Main() is done regardless of if other stuff is running?
    // cant async it either
}

void StartDaGame()
{
    Console.WriteLine("Starting Game...");
    if (!File.Exists($@"{gamePath}\ReadyOrNot.exe".Replace("\n", "").Replace("\r", ""))) { Console.WriteLine("EXE not found! Probable invalid file pointer; seeking..."); HandleFirstTime(); return; } else
    Console.WriteLine("Start in DX12 Y/N");
    ConsoleKey response = Console.ReadKey(false).Key;
    while (response != ConsoleKey.Y && response != ConsoleKey.N) { response = Console.ReadKey(false).Key; }
    try
    {
        var proc = new ProcessStartInfo()
        {
            FileName = $@"{gamePath}\ReadyOrNot.exe",
            Arguments = response == ConsoleKey.Y ? "dx12" : "dx11",
            UseShellExecute = true
        };
        gameProcess = Process.Start(proc);
        CleanupAsync();
        return;
    }
    catch (Exception e)
    {
        ThrowException(e, $@"{gamePath}\ReadyOrNot.exe", "StartDaGame() => Process.Start()", "An attempt to start a new game process resulted in an exception; Ensure that the game is installed.");
        Console.ReadLine();
        return;
    }
}

async void CleanupAsync()
{
    Console.WriteLine(Environment.NewLine); // weird format fix
    Console.WriteLine("Game is running...");
    bool timepassed = false;
    System.Timers.Timer timer = new(1000);
    timer.Start();
    timer.Elapsed += (sender, e) =>
    {
        timepassed = true;
    };

    await gameProcess.WaitForExitAsync();

    if (!timepassed) Console.WriteLine("steam has to be started first dumbass");
    Console.WriteLine("Game exited, cleaning up...");
    gamevoli = Directory.GetDirectories(gamevotemp).ToList();
    mevoli = Directory.GetDirectories(vo).ToList();

    if (!IsOverride)
    {
        mevoli.ForEach((string f) =>
        {
            Directory.EnumerateFiles(f).ToList().ForEach((string b) =>
            {
                string[] filei = b.Split(@"\");
                string direc = filei[^2];
                string file = filei.Last();
                if (File.Exists($@"{modvo}\{direc}\{file}")) Directory.Move(b, $@"{modvo}\{direc}\{file}");
                if (File.Exists($@"{gamebanktemp}\{direc}\{file}")) Directory.Move($@"{gamebanktemp}\{direc}\{file}", $@"{vo}\{direc}\{file}");
                Console.WriteLine($@"I/O => Returned VO {direc}\{file}");
            });
        });
        gamevoli.ForEach((string f) => Directory.Delete(f));
    } else
    {
        mevoli.ForEach((string f) =>
        {
            Directory.EnumerateFiles(f).ToList().ForEach((string b) =>
            {
                string[] filei = b.Split(@"\");
                string direc = filei[^2];
                string file = filei.Last();
                if (Directory.Exists($@"{gamevotemp}\{direc}"))
                {
                    Directory.Move(b, $@"{modvo}\{direc}\{file}");
                    Console.WriteLine($@"I/O => Returned VO {direc}\{file}");
                }
            });
        });
        gamevoli.ForEach((string f) =>
        {
            Directory.EnumerateFiles(f).ToList().ForEach((string b) =>
            {
                string[] filei = b.Split(@"\");
                string direc = filei[^2];
                string file = filei.Last();
                Directory.Move(b, $@"{vo}\{direc}\{file}");
            });
            Directory.Delete(f);
        });
    }


    Directory.EnumerateFiles(gamebanktemp).ToList().ForEach((string s) =>
    {
        string f = s.Split(@"\").Last();
        Directory.Move($@"{banks}\{f}", $@"{modbanks}\{f}");
        Directory.Move(s, $@"{banks}\{f}");
        Console.WriteLine($"I/O => Returned FMOD bank {f}");
    });
    
    Directory.EnumerateFiles(gamePaks).ToList().ForEach((string s) =>
    {
        if (s.Contains("pakchunk99") || s.Contains("pakchunk-99"))
        {
            Directory.Move(s, $@"{paks}\{s.Split(@"\").Last()}");
            Console.WriteLine($@"I/O => Returned PAK {s.Split(@"\").Last()}");
        }
    });

    Console.WriteLine("All done, press ENTER to terminate");
    Console.ReadLine();
    Environment.Exit(0);
}

void HandleIO()
{
    var file = File.ReadAllText(importantfileithinkpath);
    DeclarePaths(file);
    try 
    { 
        gamevoli = Directory.GetDirectories(vo).ToList();
        gamebankli = Directory.GetFiles(banks).ToList();
        mevoli = Directory.GetDirectories(modvo).ToList();
        mebankli = Directory.GetFiles(modbanks).ToList();
        Console.WriteLine("I/O => Moving files...");
    } catch (Exception e)
    {
        ThrowException(e, $@"{gamePath}", "HandleIO() => Directory.GetDirectories/GetFiles()", "I/O attempted to read game/mod files and encountered an exception");
    }

    bool[] configvals = GetConfigValues();

    switch (configvals[1])
    {
        case true:
            Console.WriteLine("I/O => Skip VO is set to true, VO loading skipped!");
        break;
        case false:
        if (configvals[0])
        {
            IsOverride = true;
            Console.WriteLine("I/O => Override all is set to true, all VO files will be moved");
            Console.WriteLine("This may result in silent lines caused by missing files");
            gamevoli.ForEach((string f) => 
            {
                Directory.EnumerateFiles(f).ToList().ForEach((string b) =>
                {
                    string[] e = b.Split(@"\");
                    string direc = e[^2];
                    string file = e.Last();
                    if (Directory.Exists($@"{modvo}\{direc}"))
                    {
                        Directory.CreateDirectory($@"{gamevotemp}\{direc}");

                        Directory.Move(b, $@"{gamevotemp}\{direc}\{file}");
                    }
                });
            });
            mevoli.ForEach((string f) => 
            {
                Directory.EnumerateFiles(f).ToList().ForEach((string b) =>
                {
                    string[] e = b.Split(@"\");
                    string direc = e[^2];
                    string file = e.Last();
                    if (Directory.Exists($@"{vo}\{direc}"))
                    {
                        Directory.Move($@"{modvo}\{direc}\{file}", $@"{vo}\{direc}\{file}");
                        Console.WriteLine($@"I/O => Moved {direc}\{file} from mod VO to game VO");
                    }
                });
            });
            break;
        }
        mevoli.ForEach((string f) =>
        {
            Directory.EnumerateFiles(f).ToList().ForEach((string b) => 
            {
                string[] filei = b.Split(@"\");
                string filef = filei[^2];
                string fileo = filei.Last();
                Directory.CreateDirectory($@"{gamevotemp}\{filef}");
                Directory.Move($@"{vo}\{filef}\{fileo}", $@"{gamevotemp}\{filef}\{fileo}");
                Directory.Move(b, $@"{vo}\{filef}\{fileo}");
                Console.WriteLine($@"I/O => Moved {filef}\{fileo} from mod VO to game VO");
            });
        });
        break;
    }

    switch (configvals[2])
    {
        case true:
            Console.WriteLine("I/O => Skip FMOD is set to true, FMOD loading skipped!");
        break;
        case false:
            mebankli.ForEach((string f) => 
            {
                string filei = f.Split(@"\").Last();

                Directory.Move($@"{banks}\{filei}", $@"{gamebanktemp}\{filei}");
                Directory.Move(f, $@"{banks}\{filei}");
                Console.WriteLine($"I/O => Moved {filei} from mod banks to game FMOD banks");
            });
        break;
    }

    switch (configvals[3])
    {
        case true:
            Console.WriteLine("I/O => Skip PAK is set to true, PAK loading skipped!");
        break;
        case false:
            Directory.EnumerateFiles(paks).ToList().ForEach((string f) => 
            { 
                Directory.Move(f, $@"{gamePaks}\{f.Split(@"\").Last()}"); 
                Console.WriteLine($@"I/O => Moved PAK {f.Split(@"").Last()}"); 
            });
        break;
    }
    
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("Files have been moved, if you close the console at this point kittens will die");
    StartDaGame();
    return;
}

void HandleFirstTime() // kinky
{
    if (!File.Exists(configpath)) CreateConfig();
    var file = File.CreateText(importantfileithinkpath);
    Console.WriteLine("Seeking game path...");

    if (Directory.Exists(@"C:\Program Files (x86)\Steam\steamapps\common\Ready Or Not"))
    {
        Console.WriteLine("Path found!");
        file.WriteLine(@"C:\Program Files (x86)\Steam\steamapps\common\Ready Or Not");
        file.Close();
        string f = File.ReadAllText(importantfileithinkpath);
        if (string.IsNullOrWhiteSpace(f)) { Console.WriteLine("Data file found but contains nothing; rebuilding..."); HandleFirstTime(); return; } else
        DeclarePaths(f);
        Directory.CreateDirectory(paks);
        Directory.CreateDirectory(modvo);
        Directory.CreateDirectory(modbanks);
        Directory.CreateDirectory(gamevotemp);
        Directory.CreateDirectory(gamebanktemp);
        Console.WriteLine(@$"Mod directories created at {gamePath}\modcontent\, place your mod files in there");
        Console.WriteLine("Console is now on standby so you may place your files inside their respective folders");
        Console.WriteLine("Press ENTER to re-awaken the console");
        Console.Read();
        return;
    }
    file.Close();
    HandleCustomPath();
    return;
}

void HandleCustomPath()
{
    var file = File.CreateText(importantfileithinkpath);
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("Couldn't find gamepath, please input path...");
    string input = Console.ReadLine();
    string inputParse = input.Split(@"\").Last();

    if (inputParse != "Ready Or Not" || string.IsNullOrEmpty(input))
    {
        Console.WriteLine("Invalid path, try again");
        HandleCustomPath();
        return;
    }
    file.WriteLine(input);
    file.Close();
    DeclarePaths(File.ReadAllText(importantfileithinkpath));
    Directory.CreateDirectory(modvo);
    Directory.CreateDirectory(modbanks);
    Directory.CreateDirectory(gamevotemp);
    Directory.CreateDirectory(gamebanktemp);
    Console.WriteLine(@$"Mod directories created at {gamePath}\modcontent\, place your mod files in there");
    Console.WriteLine("Console is now on standby so you may place your files inside their respective folders");
    Console.WriteLine("Press ENTER to re-awaken the console");
    Console.Read();
    return;
}

void CreateConfig()
{
    XElement ops = new XElement("config",
    new XElement("overrideall", false),
    new XElement("skipvo", false),
    new XElement("skipfmod", false),
    new XElement("skippak", false)
    );
    ops.Save(configpath);
}

void DeclarePaths(string rootpath)
{
    gamePath = rootpath.Replace("\n", "").Replace("\r", "");
    paks = $@"{gamePath}\modcontent\PAKs\";
    modvo = $@"{gamePath}\modcontent\VO\";
    modbanks = $@"{gamePath}\modcontent\FMOD\";
    gamevotemp = $@"{gamePath}\modcontent\VO\GameTemp";
    gamebanktemp = $@"{gamePath}\modcontent\FMOD\GameTemp";
    vo = $@"{gamePath}\ReadyOrNot\Content\VO";
    banks = $@"{gamePath}\ReadyOrNot\Content\FMOD\Desktop";
    gamePaks = $@"{gamePath}\ReadyOrNot\Content\Paks";
}

void ThrowException(Exception e, string path, string method, string message)
{
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("Attempted I/O Process Failed!");
    Console.WriteLine(message);
    Console.WriteLine($"TRACE: {method} => {path}");
    Console.WriteLine($"!!EXCEPTION THROWN!! => {e}");
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("It is recommended to purge all launcher files (importanttext / modcontent) so that new ones can be generated");
    System.Timers.Timer timer = new System.Timers.Timer(5000);
    timer.Start();
    timer.Elapsed += (sender, e) => { Environment.Exit(1); };
}

bool[] GetConfigValues()
{
    XmlDocument file = new XmlDocument();
    List<bool> confvals = new();
    file.Load(configpath);
    XmlNodeList node = file.GetElementsByTagName("overrideall");
    confvals.Add(bool.Parse(node[0].InnerText));
    node = file.GetElementsByTagName("skipvo");
    confvals.Add(bool.Parse(node[0].InnerText));
    node = file.GetElementsByTagName("skipfmod");
    confvals.Add(bool.Parse(node[0].InnerText));
    node = file.GetElementsByTagName("skippak");
    confvals.Add(bool.Parse(node[0].InnerText));
    return confvals.ToArray();
}
