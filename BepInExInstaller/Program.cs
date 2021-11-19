using Ionic.Zip;
using Octokit;
using System.Diagnostics;
using System.Net;

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("XX        XX  XX          XXXXXXXX    XX      XX  XX      XX  XXXXXXXXXX");
Console.WriteLine("XXXX    XXXX  XX          XX      XX  XX    XX      XX  XX        XX    ");
Console.WriteLine("XX  XXXX  XX  XX          XX      XX  XXXXXX          XX          XX    ");
Console.WriteLine("XX        XX  XX          XX      XX  XXXXXX          XX          XX    ");
Console.WriteLine("XX        XX  XX          XX      XX  XX    XX        XX          XX    ");
Console.WriteLine("XX        XX  XXXXXXXXXX  XXXXXXXX    XX      XX      XX          XX    ");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("Toto může způsobit nesprávnou funkci programu.");
Console.ForegroundColor = ConsoleColor.White;
string directoryGame = new DirectoryInfo(".").FullName;
while (true) {
    Console.Write("Název složky se hrou: ");
    directoryGame = Console.ReadLine();
    if (Directory.Exists(directoryGame)) {
        break;
    }
}

directoryGame = Path.Combine(new DirectoryInfo(".").FullName, directoryGame);
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Získávání nejnovější verze BepInEx");
GitHubClient client = new GitHubClient(new ProductHeaderValue("BepInExAutoDownloader"));
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("GET /repos/BepInEx/BepInEx/releases");
Console.ForegroundColor = ConsoleColor.White;
IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("BepInEx", "BepInEx");
Console.WriteLine("Nejnovější verze BepInEx je " + releases[0].Name.Replace("BepInEx ", ""));
ReleaseAsset latestWindows64 = releases[0].Assets.First(o => o.Name.Contains("x64"));
ReleaseAsset latestWindows86 = releases[0].Assets.First(o => o.Name.Contains("x86"));
Console.WriteLine("Stahovací odkaz na 64 bit verzi je " + latestWindows64.BrowserDownloadUrl);
Console.WriteLine("Stahovací odkaz na 32 bit verzi je " + latestWindows86.BrowserDownloadUrl);
Console.WriteLine();
while (true) {
    Console.WriteLine("Jak vybrat možnost:");
    Console.WriteLine("\tPřed možností je písmeno. Toto písmeno napište po příkazového řádku.");
    Console.WriteLine("Prosím vyberte možnost: ");
    Console.WriteLine("\t1. x64");
    Console.WriteLine("\t2. x32");
    Console.WriteLine("\ts: Smazat");
    Console.WriteLine("\tz: Zavřít");
    Console.Write('>');
    ConsoleKeyInfo key = Console.ReadKey();
    if (key.KeyChar == 's') {
        Console.WriteLine();
        Console.WriteLine("Vybírání Smazat");
        Console.WriteLine("Mazání BepInEx");
        Console.WriteLine("Mazání - ./doorstop_config.ini");
        if (File.Exists(Path.Combine(directoryGame, "doorstop_config.ini"))) {
            try {
                File.Delete(Path.Combine(directoryGame, "doorstop_config.ini"));
            }
            catch (UnauthorizedAccessException) {
                Console.WriteLine("Nelze smazat /doorstop_config.ini - Nedostatečná oprávnění.");
            }
            catch (Exception) {

            }
        }
        else {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Soubor /doorstop_config.ini neexistuje");
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.WriteLine("Mazání - ./winhttp.dll");
        if (File.Exists(Path.Combine(directoryGame, "winhttp.dll"))) {
            try {
                File.Delete(Path.Combine(directoryGame, "winhttp.dll"));
            }
            catch (UnauthorizedAccessException e) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Nelze smazat {Path.Combine(directoryGame, "winhttp.dll")} - Nedostatečná oprávnění");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception) {
            }
        }
        else {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Soubor /winhttp.dll neexistuje");
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.WriteLine("Mazání - ./BepInEx");
        if (Directory.Exists(Path.Combine(directoryGame, "BepInEx"))) {
            Directory.Delete(Path.Combine(directoryGame, "BepInEx"), true);
        }
        else {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Složka {Path.Combine(directoryGame, "BepInEx")} neexistuje");
            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.WriteLine("BepInEx byl odinstalován.");
    }
    if (key.KeyChar == 'z') {
        Console.WriteLine();
        Console.WriteLine("Vybírání Zavřít aplikaci");
        Environment.Exit(0);
    }
    if (key.KeyChar == '1' || key.KeyChar == '2') {
        bool finished = false;
        WebClient downloader = new();
        Stopwatch stopwatch = Stopwatch.StartNew();
        downloader.DownloadProgressChanged += (s, e) => {
            if (e.BytesReceived == 0 || e.TotalBytesToReceive == 0)
                return;
            if (stopwatch.ElapsedMilliseconds > 20) {
                stopwatch.Restart();
                Console.WriteLine(e.BytesReceived / 1_024 + "/" + e.TotalBytesToReceive / 1_024);
            }
        };
        downloader.DownloadFileCompleted += (s, e) => {
            finished = true;
        };

        switch (key.KeyChar) {
            case '1':
                downloader.DownloadFileAsync(new Uri(latestWindows64.BrowserDownloadUrl), "./BepInEx.zip");
                break;
            case '2':
                downloader.DownloadFileAsync(new Uri(latestWindows86.BrowserDownloadUrl), "./BepInEx.zip");
                break;
        }

        while (!finished) {

        }

        downloader.Dispose();

        if (File.Exists("./BepInEx.zip")) {
            ZipFile zips = ZipFile.Read("./BepInEx.zip");
            string temp = "";
            zips.ExtractProgress += (s, e) => {
                if (e.CurrentEntry != null) {
                    if (temp != e.CurrentEntry.FileName) {
                        temp = e.CurrentEntry.FileName;
                        Console.WriteLine("Extrahování - " + e.CurrentEntry.FileName);
                    }
                }
            };
            zips.ZipErrorAction = ZipErrorAction.Skip;
            zips.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
            try {
                zips.ExtractAll(directoryGame);
            }
            catch (UnauthorizedAccessException e) {
                Console.WriteLine("Fatální chyba - Nelze přepsat soubor, chybí oprávnění.");
            }
            zips.Dispose();
        }

        File.Delete($"{directoryGame}/changelog.txt");
        File.Delete("./BepInEx.zip");

        string[] files = Directory.GetFiles(directoryGame, "*.exe");

        foreach (string file in files) {
            if (Directory.Exists(Path.Combine(directoryGame, file.Replace(".exe", "") + "_Data"))) {
                // Run the file to test it
                Console.WriteLine("Testovací běh - Spouštění programu " + file);
                Process proc = Process.Start(file);
                Thread.Sleep(5000);
                proc.CloseMainWindow();
                proc.WaitForExit();
                Console.WriteLine("Testovací běh hotov!");
                Console.WriteLine("Teď můžete nainstalovat módy.");
                Console.WriteLine("Jak nainstalovat módy:");
                Console.WriteLine("\t1.Stáhněte mód.");
                Console.WriteLine("\t2.Vložte do složky \"plugins\" pod složkou \"BepInEx\"");
                Console.WriteLine("\t3.Hotovo!");
            }
        }
    }
}