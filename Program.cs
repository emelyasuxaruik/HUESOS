using System.Text.Json;

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("HUESOS v0.2");
Console.ResetColor();

const string DEFAULT_REPO = "https://twgood.serv00.net/huesos/pkgs/pkg4l.json";
string CURRENT_REPO = DEFAULT_REPO;

string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string huesosDir = Path.Combine(appData, "huesos");
string reposFile = Path.Combine(huesosDir, "repos.json");

var repos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["main"] = DEFAULT_REPO };

if (File.Exists(reposFile))
{
    try
    {
        var json = File.ReadAllText(reposFile);
        var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (saved != null) foreach (var kv in saved) repos[kv.Key] = kv.Value;
    }
    catch { }
}

void SaveRepos() {
    Directory.CreateDirectory(huesosDir);
    File.WriteAllText(reposFile, JsonSerializer.Serialize(repos, new JsonSerializerOptions { WriteIndented = true }));
}

if (args.Length == 0) {
    Console.WriteLine("Использование:");
    Console.WriteLine("  huesos eblan.browser");
    Console.WriteLine("  huesos add <url>");
    Console.WriteLine("  huesos add имя <url>");
    Console.WriteLine("  huesos list | use main | remove имя | -v");
    return;
}

string cmd = args[0].ToLower();

if (cmd is "-v" or "--version") {
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("HUESOS v0.2");
    Console.ResetColor(); return;
}

if (cmd == "add") {
    if (args.Length < 2) { Console.WriteLine("Дай ссылку, дебил"); return; }
    string name = "repo" + (repos.Count + 1);
    string url = args[1];
    if (args.Length >= 3 && !args[1].StartsWith("http")) { name = args[1].ToLower(); url = args[2]; }
    repos[name] = url; SaveRepos();
    Console.WriteLine($"Добавлен: {name} → {url}");
    return;
}

if (cmd == "list") {
    Console.WriteLine("Репозитории:");
    foreach (var r in repos)
        Console.WriteLine($"  {(CURRENT_REPO == r.Value ? ">" : " ")} {r.Key} → {r.Value}");
    return;
}

if (cmd == "use" && args.Length > 1) {
    if (repos.TryGetValue(args[1].ToLower(), out var url)) {
        CURRENT_REPO = url; Console.WriteLine($"Теперь юзаем: {args[1]}");
    } else Console.WriteLine("Такого репо нет, лох");
    return;
}

if (cmd == "remove" && args.Length > 1) {
    string n = args[1].ToLower();
    if (n == "main") { Console.WriteLine("main нельзя удалить, долбоёб"); return; }
    if (repos.Remove(n)) { SaveRepos(); Console.WriteLine($"Удалён: {n}"); }
    else Console.WriteLine("Такого и не было");
    return;
}

if (cmd == "creative") {
    Console.WriteLine("пошел нахуй хуй те");
    return;
}

string id = args[0].ToLower();
Console.WriteLine($"Ищу {id}...");

try {
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("User-Agent", "HUESOS/0.2");

    string json = await client.GetStringAsync(CURRENT_REPO);
    var data = JsonSerializer.Deserialize<JsonElement>(json);

    if (!data.GetProperty("packages").TryGetProperty(id, out var pkg)) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ПАКЕТА НЕТ ☠️"); Console.ResetColor(); return;
    }

    string url = pkg.GetProperty("url").GetString()!;
    string name = pkg.GetProperty("name").GetString()!;

    Console.Write("Скачиваю ");
    var spin = Task.Run(() => { var c = "/-\\|"; int i = 0; while(true){ Console.Write($"\b{c[i++%4]}"); Thread.Sleep(100); } });
    byte[] file = await client.GetByteArrayAsync(url);
    Console.Write("\b\n");

    string fileName = Path.GetFileName(new Uri(url).LocalPath);
    if (string.IsNullOrEmpty(Path.GetExtension(fileName))) fileName += ".AppImage";
    string path = Path.Combine(Path.GetTempPath(), "huesos_" + fileName);

    await File.WriteAllBytesAsync(path, file);

    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
        FileName = "chmod", Arguments = $"+x \"{path}\"", CreateNoWindow = true, UseShellExecute = false
    })?.WaitForExit();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Запускаю {name} нахуй...");

    var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
        FileName = path, UseShellExecute = true
    });
    p?.WaitForExit();

    Console.ResetColor();
    Console.WriteLine("ГОТОВО");
}
catch (Exception ex) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ПИЗДЕЦ: {ex.Message}");
    Console.ResetColor();
}
