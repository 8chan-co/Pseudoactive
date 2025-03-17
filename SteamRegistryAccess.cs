using Microsoft.Win32;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pseudoactive;

internal static class SteamRegistryAccess
{
    internal static string GetSteamPathname() => GetSteamRegistryValue("SteamPath");

    internal static string GetSteamFilename() => GetSteamRegistryValue("SteamExe");

    internal static int GetSteamProcessIdentifier()
    {
        const string KeyName = "HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess";

        return Unsafe.Unbox<int>(Registry.GetValue(KeyName, "pid", -1)!);
    }

    internal static IEnumerable<string> GetFilenames()
    {
        const string KeyName = "Software\\Classes";

        using RegistryKey ClassesSubKey = Registry.CurrentUser.OpenSubKey(KeyName)!;

        foreach (string SubKeyName in ClassesSubKey.GetSubKeyNames())
        {
            string CommandKeyName = $"{SubKeyName}\\Shell\\Open\\Command";

            using RegistryKey? CommandKey = ClassesSubKey.OpenSubKey(CommandKeyName);

            if (CommandKey is null) continue;

            yield return Unsafe.As<string>(CommandKey.GetValue(string.Empty)!);
        }
    }

    internal static IEnumerable<uint> GetInstalledApplications()
    {
        const string KeyName = "Software\\Valve\\Steam\\Apps";

        using RegistryKey AppsSubKey = Registry.CurrentUser.OpenSubKey(KeyName)!;

        foreach (string SubKeyName in AppsSubKey.GetSubKeyNames())
        {
            using RegistryKey AppSubKey = AppsSubKey.OpenSubKey(SubKeyName)!;

            int Installed = Unsafe.Unbox<int>(AppSubKey.GetValue("Installed", -1));

            if (Installed is -1) continue;

            yield return uint.Parse(SubKeyName);
        }
    }

    private static string GetSteamRegistryValue(string ValueName)
    {
        const string KeyName = "HKEY_CURRENT_USER\\Software\\Valve\\Steam";

        object Value = Registry.GetValue(KeyName, ValueName, string.Empty)!;

        return Unsafe.As<string>(Value);
    }
}
