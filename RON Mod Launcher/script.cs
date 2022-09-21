using System.Diagnostics;
using System.Reflection;

string gamePath;
string paks;
string gamePaks;
string vo;
string banks;
string me;
string importantfileithinkpath;
string modvo;
string modbanks;
string gamevotemp;
string gamebanktemp;
IEnumerable<string> gamevoli;
IEnumerable<string> mevoli;
IEnumerable<string> gamebankli;
IEnumerable<string> mebankli;
Process gameProcess;
Main();

void Main()
{
    me = Assembly.GetExecutingAssembly().Location.TrimEnd(@"RON Mod Launcher.dll".ToCharArray());
    importantfileithinkpath = me + @"\importantfile.txt";
    Console.WriteLine("RON Mod Launcher Started");
    Console.WriteLine(Environment.NewLine);

    if (!File.Exists(@$"{me}\importantfile.txt"))
    {
        Console.WriteLine("Data file not found! Making another...");
        HandleFirstTime();
        HandleIOShit();
        Console.ReadLine();
    } else
    HandleIOShit();
    Console.ReadLine(); // seriously who decided that the console should just auto kill itself when Main() is done regardless of if other stuff is running?
    // cant async it either
}

void StartDaGame()
{
    Console.WriteLine("Starting Game...");
    if (!File.Exists($@"{gamePath}\ReadyOrNot.exe".Replace("\n", "").Replace("\r", ""))) { Console.WriteLine("EXE not found! Probable invalid file pointer; seeking..."); HandleFirstTime(); return; } else
    try
    {
        Console.WriteLine("Start in DX12 Y/N");
        ConsoleKey response = Console.ReadKey(false).Key;
        while (response != ConsoleKey.Y && response != ConsoleKey.N) { response = Console.ReadKey(false).Key; }
        var proc = new ProcessStartInfo()
        {
            FileName = $@"{gamePath}\ReadyOrNot.exe".Replace("\n", "").Replace("\r", ""),
            Arguments = response == ConsoleKey.Y ? "dx12" : "dx11"
        };
        gameProcess = Process.Start(proc);
        CleanupAsync();
        return;
    }
    catch (Exception e)
    {
        ThrowException(e, $@"{gamePath}\ReadyOrNot.exe".Replace("\n", "").Replace("\r", ""), "StartDaGame() => Process.Start()");
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
    gamevoli = Directory.GetDirectories(gamevotemp);
    mevoli = Directory.GetDirectories(vo);
    
    gamevoli.ToList().ForEach((string f) =>
    {
        Directory.EnumerateFiles(f).ToList().ForEach((string b) =>
        {
            string[] filei = b.Split(@"\");
            string filef = filei[^2];
            string fileo = filei.Last();
            Directory.Move($@"{vo}\{filef}\{fileo}", $@"{modvo}\{filef}\{fileo}");
            Directory.Move(b, $@"{vo}\{filef}\{fileo}");
            Console.WriteLine($@"I/O => Returned VO {filef}\{fileo}");
        });
        Directory.Delete(f);
    });

    Directory.EnumerateFiles(gamebanktemp).ToList().ForEach((string s) =>
    {
        string f = s.Split(@"\").Last();
        Directory.Move($@"{banks}\{f}", $@"{modbanks}\{f}");
        Directory.Move(s, $@"{banks}\{f}");
        Console.WriteLine($"I/O => Returned FMOD bank {f}");
    });

    Directory.EnumerateFiles(gamePaks).ToList().ForEach((string s) =>
    {
        if (s.Contains("pakchunk99"))
        {
            Directory.Move(s, $@"{paks}\{s.Split(@"\").Last()}");
            Console.WriteLine($@"I/O => Returned PAK {s.Split(@"\").Last()}");
        }
    });

    Console.WriteLine("All done, press ENTER to terminate");
    Console.ReadLine();
    Environment.Exit(0);
}

void HandleIOShit()
{
    var file = File.ReadAllText(importantfileithinkpath);

    DeclarePaths(file);
    try { gamevoli = Directory.GetDirectories(vo); } catch (Exception e) { ThrowException(e, vo, "HandleIOShit() => Directory.GetDirectories()"); return; }
    try { gamebankli = Directory.GetFiles(banks); } catch (Exception e) { ThrowException(e, banks, "HandleIOShit() => Directory.GetFiles()"); return; }
    try { mevoli = Directory.GetDirectories(modvo); } catch (Exception e) { ThrowException(e, modvo, "HandleIOShit() => Directory.GetDirectories()"); return; }
    try { mebankli = Directory.GetFiles(modbanks); } catch (Exception e) { ThrowException(e, modbanks, "HandleIOShit() => Directory.GetFiles()"); return; }
    Console.WriteLine("I/O => Moving files...");

    mevoli.ToList().ForEach((string f) =>
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

    mebankli.ToList().ForEach((string f) => 
    {
        string filei = f.Split(@"\").Last();

        Directory.Move($@"{banks}\{filei}", $@"{gamebanktemp}\{filei}");
        Directory.Move(f, $@"{banks}\{filei}");
        Console.WriteLine($"I/O => Moved {filei} from mod banks to game FMOD banks");
    });

    Directory.EnumerateFiles(paks).ToList().ForEach((string f) => 
    { 
        Directory.Move(f, $@"{gamePaks}\{f.Split(@"\").Last()}"); 
        Console.WriteLine($@"I/O => Moved PAK {f.Split(@"").Last()}"); 
    });

    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("Files have been moved, if you close the console at this point kittens will die");
    StartDaGame();
    return;
}

void HandleFirstTime() // kinky
{        
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

void DeclarePaths(string rootpath)
{
    gamePath = rootpath;
    paks = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\modcontent\PAKs\";
    modvo = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\modcontent\VO\";
    modbanks = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\modcontent\FMOD\";
    gamevotemp = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\modcontent\VO\GameTemp";
    gamebanktemp = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\modcontent\FMOD\GameTemp";
    vo = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\ReadyOrNot\Content\VO";
    banks = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\ReadyOrNot\Content\FMOD\Desktop";
    gamePaks = $@"{rootpath.Replace("\n", "").Replace("\r", "")}\ReadyOrNot\Content\Paks";
}

void ThrowException(Exception e, string path, string method)
{
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("Attempted I/O Process Failed!");
    Console.WriteLine($"TRACE: {method} => {path}");
    Console.WriteLine($"!!EXCEPTION THROWN!! => {e}");
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("It is recommended to purge all launcher files so that new ones can be generated");
    System.Timers.Timer timer = new System.Timers.Timer(5000);
    timer.Start();
    timer.Elapsed += (sender, e) => { Environment.Exit(1); };
}
