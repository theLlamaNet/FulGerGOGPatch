using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
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

            string fovIniName = cfg.ContainsKey("FovIni") ? cfg["FovIni"] : "";
            string fovIni = fovIniName.Length > 0 ? Path.Combine(gameDir, fovIniName) : "";
            bool existingFovIni = fovIni.Length > 0 && File.Exists(fovIni);

            foreach (string source in Directory.GetFiles(payload))
            {
                string destination = Path.Combine(gameDir, Path.GetFileName(source));
                if (existingFovIni && string.Equals(Path.GetFileName(source), fovIniName, StringComparison.OrdinalIgnoreCase)) continue;
                File.Copy(source, destination, true);
            }

            if (!existingFovIni && cfg.ContainsKey("FovMultiplier") && fovIni.Length > 0)
            {
                float fov;
                if (!float.TryParse(cfg["FovMultiplier"], NumberStyles.Float, CultureInfo.InvariantCulture, out fov) || fov < 0.5f || fov > 2.0f)
                    throw new InvalidDataException("FovMultiplier must be between 0.5 and 2.0 (use a decimal point).");
                if (!File.Exists(fovIni)) throw new FileNotFoundException("FOV configuration was not found: " + fovIni);
                string contents = File.ReadAllText(fovIni);
                if (fov == 1.0f) fov = 1.001f;
                string updated = Regex.Replace(contents, @"(?m)^(\s*fov_multiplier\s*=\s*)[^\r\n]*", m => m.Groups[1].Value + fov.ToString("0.000", CultureInfo.InvariantCulture));
                if (updated == contents && !Regex.IsMatch(contents, @"(?m)^\s*fov_multiplier\s*="))
                    throw new InvalidDataException("fov_multiplier is missing from " + fovIni);
                File.WriteAllText(fovIni, updated);
            }
            if (existingFovIni)
            {
                string contents = File.ReadAllText(fovIni);
                File.WriteAllText(fovIni, Regex.Replace(contents, @"(?m)^(\s*fov_multiplier\s*=\s*)1(?:\.0+)?(\s*(?://[^\r\n]*)?)$", m => m.Groups[1].Value + "1.001" + m.Groups[2].Value));
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
        string[] order = { "GameDirectory", "OriginalExecutable", "GameExecutable", "Arguments", "Mutexes", "FovIni", "FovMultiplier" };
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
