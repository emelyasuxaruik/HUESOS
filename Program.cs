﻿using System.Text.Json;

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("HUESOS - Best Packge Manager");
Console.ResetColor();

const string HUESOS_VERSION = "0.2";
const string DEFAULT_REPO = "https://twgood.serv00.net/huesos/pkgs/pkg.json";
string CURRENT_REPO = DEFAULT_REPO;


string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string huesosDir = Path.Combine(appData, "huesos");
string reposFile = Path.Combine(huesosDir, "repos.json");

Dictionary<string, string> repos = new(StringComparer.OrdinalIgnoreCase)
{
    ["main"] = DEFAULT_REPO
};


if (File.Exists(reposFile))
{
    try
    {
        var json = File.ReadAllText(reposFile);
        var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (saved != null) foreach (var r in saved) repos[r.Key] = r.Value;
    }
    catch { }
}


void SaveRepos()
{
    Directory.CreateDirectory(huesosDir);
    var json = JsonSerializer.Serialize(repos, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(reposFile, json);
}


if (args.Length == 0)
{
    Console.WriteLine("Пиши: huesos eblan.browser");
    Console.WriteLine("      huesos add <url>            — добавить репозиторий");
    Console.WriteLine("      huesos add name <url>       — с именем");
    Console.WriteLine("      huesos list                 — все репо");
    Console.WriteLine("      huesos use main             — переключить");
    Console.WriteLine("      huesos remove name          — удалить");
    return;
}

string cmd = args[0].ToLower();

if (cmd == "-v" || cmd == "-ver" || cmd == "--version")
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("HUESOS v0.2 — установить приложения стало очень просто!");
    Console.ResetColor();
    return;
}

if (cmd == "add")
{
    if (args.Length < 2)
    {
        Console.WriteLine("Добавь ссылку еблан");
        return;
    }

    string name = "repo" + (repos.Count + 1);
    string url = args[1];

    if (args.Length >= 3 && !args[1].StartsWith("http"))
    {
        name = args[1].ToLower();
        url = args[2];
    }

    repos[name] = url;
    SaveRepos();
    Console.WriteLine($"✓ Добавлен репозиторий: {name} → {url}");
    return;
}

if (cmd == "list")
{
    Console.WriteLine("Доступные репозитории:");
    foreach (var r in repos)
        Console.WriteLine($"  {(CURRENT_REPO == r.Value ? "►" : " ")} {r.Key} → {r.Value}");
    return;
}

if (cmd == "creative")
{
    Console.WriteLine("My dear friends!\nTV is all about escapism, yet you want to escape?\nC'mon and stay a while as I share with you\nThe who, what, when, where, why and how of my genius plan\nOur scene opens on a little Mr. Puzzles\nNow cut to him having no friends\nIt was a struggle to find anybody who could be my buddy\nSo instead, I watched TV all day to forget about my troubles (ooh, ooh, ooh, ah)\nI was obsessed, I couldn't stop, I wouldn't stop (no)\nUntil I'd seen every moving picture that exists\nSo, he made the decision to get into television\nI cut off my face and put a TV in its place (ooh, ooh, ooh, ah)\nPatience is (ooh—ah) a virtue\nGood things come to those who wait (ooh—ah)\nProverbs uttered by utter fools (ooh—ah)\nI'll do anything it takes\nAre you ready for trouble?\nGot you binge watching\nYou're stuck with Mr. Puzzles\nYou can't stop me from cooking up this instant classic (ooh—ah)\nYou and your friends will look fantastic (ooh, ooh, ooh, ah)\nThe red carpet is rolled out\nCome and get your tickets\nBefore they're all sold out\nDon't know how to miss this moment (ooh—ah)\nWhen those stars hit five\nI get creative control of your real life\nLadies and gentlemen, excuse the interruption\nThe following message goes out to the stars of our show\nThe SMG4 crew\nAnd we're rolling, let's go\nDo you wanna know why I chose you?\nDo you wanna know why I chose you?\n'Cause you're stupidest show that I've ever seen\nAnd if I can make you entertaining, I can do anything\nAre you ready for trouble?\nGot you binge watching\nYou're stuck with Mr. Puzzles\nYou can't stop me from cooking up this instant classic (ooh—ah)\nYou and your friends will look fantastic (ooh, ooh, ooh, ah)\nThe red carpet is rolled out\nGotta get your tickets\nBefore they're all sold out\nDon't know how to miss this moment (ooh—ah)\nWhen those stars hit five\nI get creative control of your real\nGrab a seat and let's seal the deal\nWhen those gorgeous stars hit five (ooh—ah)\nI get creative control of your real life\nAnd scene (hmm)");
    return;
}

if (cmd == "use" && args.Length > 1)
{
    string name = args[1].ToLower();
    if (repos.TryGetValue(name, out var url))
    {
        CURRENT_REPO = url;
        Console.WriteLine($"✓ Теперь используется: {name}");
    }
    else Console.WriteLine("☠️ Нет такого репо, дебил");
    return;
}

if (cmd == "remove" && args.Length > 1)
{
    string name = args[1].ToLower();
    if (name == "main")
    {
        Console.WriteLine("Хочешь значит удалить мейн\nНу ты и долбаеб");
        return;
    }
    if (repos.Remove(name))
    {
        SaveRepos();
        Console.WriteLine($" Удалён репозиторий: {name}");
    }
    else Console.WriteLine("Ты хуйню пишешь");
    return;
}


string id = args[0].ToLower();
if (args.Length > 1 && args[0].StartsWith("install_"))
    id = args[0]["install_".Length..].ToLower();

Console.WriteLine($"Ищу в репозитории: {id}...");

try
{
    using HttpClient client = new();
    client.DefaultRequestHeaders.Add("User-Agent", "Huesos/5.1");

    string json = await client.GetStringAsync(CURRENT_REPO);
    var data = JsonSerializer.Deserialize<JsonElement>(json);

    if (!data.GetProperty("packages").TryGetProperty(id, out JsonElement pkg))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("нет пакета");
        Console.ResetColor();
        return;
    }

    string version = pkg.GetProperty("version").GetString()!;
    string url = pkg.GetProperty("url").GetString()!;
    string name = pkg.GetProperty("name").GetString()!;
    string silent = pkg.TryGetProperty("silent", out var s) ? s.GetString()! : "";

    Console.WriteLine($"НАШОЛ: {name} v{version}");
    Console.Write("Скачиваю ");

    var spinner = Task.Run(() =>
    {
        var chars = "/-\\|";
        int i = 0;
        while (true)
        {
            Console.Write($"\b{chars[i++ % 4]}");
            Thread.Sleep(150);
        }
    });

    byte[] file = await client.GetByteArrayAsync(url);
    Console.Write("\b✓\n");

    string fileName = Path.GetFileName(new Uri(url).LocalPath);
    if (string.IsNullOrEmpty(Path.GetExtension(fileName))) fileName += ".exe";
    string path = Path.Combine(Path.GetTempPath(), "huesos_" + fileName);
    await File.WriteAllBytesAsync(path, file);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Запускаю нахуй...");

    var startInfo = new System.Diagnostics.ProcessStartInfo(path)
    {
        Arguments = silent,
        UseShellExecute = true
    };
    System.Diagnostics.Process.Start(startInfo)!.WaitForExit();

    Console.WriteLine("\nСКОЧАЛОСЬ");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nПИЗДЕЦ: {ex.Message}");
}
Console.ResetColor();
