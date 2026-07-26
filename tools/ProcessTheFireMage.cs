using System;
using System.IO;

class ProcessTheFireMage
{
    static int Main(string[] args)
    {
        string root = args.Length > 0 ? args[0] : ".";
        string themes = Path.Combine(root, "Media", "Themes");
        string src = Path.Combine(themes, "TheFireMage", "source");
        string dest = Path.Combine(themes, "TheFireMage");
        ProcessPreserveThemeIcons.ProcessTheme("TheFireMage", src, dest, 22, 10, 6);
        Console.WriteLine("TheFireMage done.");
        return 0;
    }
}
