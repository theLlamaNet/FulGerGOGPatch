using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

internal static class FulGerPatchLauncher
{
    [STAThread]
    private static void Main()
    {
        try
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string ini = Path.Combine(root, "FulGerPatch.ini");
            if (!File.Exists(ini)) throw new FileNotFoundException("FulGerPatch.ini was not found.");

            Dictionary<string, string> cfg = ReadConfig(ini);
            string gameDir = Need(cfg, "GameDirectory");
            string gameExe = Need(cfg, "GameExecutable");
            string originalExe = Need(cfg, "OriginalExecutable");
            string arguments = cfg.ContainsKey("Arguments") ? cfg["Arguments"] : "";
            string payload = Path.Combine(root, "Payload");

            if (!IsGameDirectory(gameDir, originalExe))
            {
                using (FolderBrowserDialog picker = new FolderBrowserDialog())
                {
                    picker.Description = "Select the game's installation folder (the folder containing " + originalExe + ").";
                    picker.ShowNewFolderButton = false;
                    if (Directory.Exists(gameDir)) picker.SelectedPath = gameDir;
                    if (picker.ShowDialog() != DialogResult.OK) return;
                    gameDir = picker.SelectedPath;
                }

                if (!IsGameDirectory(gameDir, originalExe))
                    throw new DirectoryNotFoundException("The selected folder does not contain " + originalExe + ":\n" + gameDir);

                cfg["GameDirectory"] = gameDir;
                WriteConfig(ini, cfg);
            }

            if (!Directory.Exists(payload)) throw new DirectoryNotFoundException("The Payload folder was not found.");

            foreach (string source in Directory.GetFiles(payload))
            {
                string destination = Path.Combine(gameDir, Path.GetFileName(source));
                File.Copy(source, destination, true);
            }

            string executable = Path.Combine(gameDir, gameExe);
            if (!File.Exists(executable)) throw new FileNotFoundException("The patched executable was not found:\n" + executable);

            List<Mutex> mutexes = new List<Mutex>();
            try
            {
                if (cfg.ContainsKey("Mutexes"))
                    foreach (string name in cfg["Mutexes"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        mutexes.Add(new Mutex(false, name.Trim()));

                ProcessStartInfo start = new ProcessStartInfo(executable, arguments);
                start.WorkingDirectory = gameDir;
                start.UseShellExecute = true;
                using (Process game = Process.Start(start)) game.WaitForExit();
            }
            finally
            {
                foreach (Mutex mutex in mutexes) mutex.Dispose();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "FulGer Patch", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Dictionary<string, string> ReadConfig(string path)
    {
        Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            int equals = line.IndexOf('=');
            if (equals > 0) result[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
        }
        return result;
    }

    private static bool IsGameDirectory(string directory, string originalExe)
    {
        return Directory.Exists(directory) && File.Exists(Path.Combine(directory, originalExe));
    }

    private static void WriteConfig(string path, Dictionary<string, string> cfg)
    {
        string[] order = { "GameDirectory", "OriginalExecutable", "GameExecutable", "Arguments", "Mutexes" };
        List<string> lines = new List<string>();
        foreach (string key in order)
            if (cfg.ContainsKey(key)) lines.Add(key + "=" + cfg[key]);
        File.WriteAllLines(path, lines.ToArray());
    }

    private static string Need(Dictionary<string, string> cfg, string key)
    {
        if (!cfg.ContainsKey(key) || cfg[key].Length == 0) throw new InvalidDataException("Missing setting: " + key);
        return cfg[key];
    }
}
