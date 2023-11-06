using static BittyMud.Mud;

namespace BittyMud;

public class HelpData
{
    public string Keyword { get; set; }
    public string Text { get; set; }
    public DateTime LoadTime { get; set; }
}

public static class HelpSystem
{
    private static List<HelpData> _helpList;
    public static string Greeting { get; private set; }
    public static string Motd { get; private set; }
    public static string GetHelpText(string helpfile)
    {
        string hFile = char.ToUpper(helpfile[0]) + helpfile.Substring(1).ToLower();
        var pHelp = _helpList.FirstOrDefault(h => h.Keyword.Equals(hFile, StringComparison.OrdinalIgnoreCase));

        return pHelp?.Text;
    }
    
    private static int ComputeLevenshteinDistance(string s, string t)
    {
        int[,] d = new int[s.Length + 1, t.Length + 1];
        for (int i = 0; i <= s.Length; i++)
            d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++)
            d[0, j] = j;

        for (int j = 1; j <= t.Length; j++)
        {
            for (int i = 1; i <= s.Length; i++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[s.Length, t.Length];
    }
    static HelpSystem()
    {
        Console.WriteLine("Load_helps: getting all help files.");

        _helpList = new List<HelpData>();
        string helpDir = "./help";

        foreach (var entry in Directory.EnumerateFiles(helpDir))
        {
            string fileName = Path.GetFileName(entry);
            if (fileName == "." || fileName == "..")
                continue;

            string s;
            try
            {
                s = File.ReadAllText(entry);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"load_helps: Helpfile {entry} does not exist.");
                continue;
            }

            var newHelp = new HelpData
            {
                Keyword = fileName,
                Text = s,
                LoadTime = DateTime.Now
            };

            _helpList.Add(newHelp);

            if (string.Equals("GREETING", newHelp.Keyword, StringComparison.OrdinalIgnoreCase))
                Greeting = newHelp.Text;
            else if (string.Equals("MOTD", newHelp.Keyword, StringComparison.OrdinalIgnoreCase))
                Motd = newHelp.Text;
        }
    }

    public static void HelpCommand(User user, string helpfile)
    {
        if (string.IsNullOrEmpty(helpfile))
        {
            // Try to get the "TOPICS" help file
            string topicsText = GetHelpText("TOPICS");
            if (topicsText != null)
            {
                WriteLine(user, topicsText);
            }
            else
            {
                // If "TOPICS" does not exist, send a list of all available help files
                var topics = _helpList.Select(h => h.Keyword.PadRight(20)).ToList();
                WriteLine(user, $"Here is a list of all available topics:\n");
                for (int i = 0; i < topics.Count; i += 5)
                {
                    var line = string.Join(" ", topics.Skip(i).Take(4));
                    WriteLine(user, line);
                }
            }
            return;
        }
        string hFile = char.ToUpper(helpfile[0]) + helpfile.Substring(1).ToLower();
        HelpData pHelp = null;

        int minDistance = int.MaxValue;
        List<HelpData> closeMatches = new List<HelpData>();
        foreach (var help in _helpList)
        {
            int distance = ComputeLevenshteinDistance(help.Keyword, helpfile);
            if (distance < minDistance)
            {
                minDistance = distance;
                pHelp = help;
            }

            if (distance <= 3)
            {
                closeMatches.Add(help);
            }
        }

        if (minDistance > 3) // max allowed distance
        {
            string closeMatchesList = string.Join(", ", closeMatches.Select(h => h.Keyword));
            WriteLine(user,$"No help file was found for that topic. These are close matches: {closeMatchesList}");
            return;
        }

        string message = pHelp.Keyword.Equals(hFile, StringComparison.OrdinalIgnoreCase)
            ? $"Displaying help for {hFile}"
            : $"You searched for {hFile}, displaying {pHelp.Keyword}, which seems to be a match instead";

        WriteLine(user,$"{message}\n\r=== {pHelp.Keyword} ===\n\r{pHelp.Text}");
    }

    public static void InitializeCommands()
    {
            Mud.AddCommand("help", HelpCommand, "Player");
    }
}

public class DMobile
{
    public void TextToMobile(string text)
    {
        // Implementation of TextToMobile
    }
}
