using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Pseudoactive;

internal static class Procat
{
    private record struct SteamApplication(uint Identifier, string Title, string Directory);

    private static void Main(string[] Arguments)
    {
        string SteamPathname = Path.Join(SteamRegistryAccess.GetSteamPathname(), "steamapps");

        Span<byte> TextBuffer = stackalloc byte[0x200];

        List<SteamApplication> Applications = [];

        foreach (string Filename in Directory.EnumerateFiles(SteamPathname, "appmanifest_*.acf"))
        {
            using FileStream Filestream = File.OpenRead(Filename);

            int BytesRead = Filestream.Read(TextBuffer);

            ReadOnlySpan<char> TextRead = Encoding.UTF8.GetString(TextBuffer[..BytesRead]).AsSpan();

            SteamApplication Temporary = default;

            foreach (ReadOnlySpan<char> Line in TextRead.EnumerateLines())
            {
                if (Line.Contains("\"appid\"", StringComparison.InvariantCultureIgnoreCase))
                {
                    Temporary.Identifier = uint.Parse(GetValueUnquoted(Line));
                }
                else
                if (Line.Contains("\"name\"", StringComparison.InvariantCultureIgnoreCase))
                {
                    Temporary.Title = GetValueUnquoted(Line).ToString();
                }
                else
                if (Line.Contains("\"installdir\"", StringComparison.InvariantCultureIgnoreCase))
                {
                    Temporary.Directory = GetValueUnquoted(Line).ToString();
                }
                else
                if (Temporary is { Identifier: not 0, Title: not null, Directory: not null })
                {
                    Applications.Add(Temporary);
                    break;
                }

                static ReadOnlySpan<char> GetValueUnquoted(ReadOnlySpan<char> Value)
                {
                    int LastIndexOfDoubleQuote = Value.Length - 1;

                    ReadOnlySpan<char> Processed = Value[..LastIndexOfDoubleQuote];

                    LastIndexOfDoubleQuote = Processed.LastIndexOf('"') + 1;

                    return Processed[LastIndexOfDoubleQuote..];
                }
            }
        }

        for (int Index = Applications.Count - 1; Index >= 0; --Index)
        {
            Console.WriteLine($"[{-(Index - Applications.Count)}] {Applications[Index].Title}");
        }

        SteamApplication Application = Applications[-(Console.Read() - Applications.Count - 48)];

        Process.Start(SteamRegistryAccess.GetSteamFilename(), $"steam://run/{Application.Identifier}").WaitForExit();

        ImmutableHashSet<int> ExistingProcesses = [.. Process.GetProcesses().Select(Process => Process.Id)];

        int PotentialProcess = 0;

        for (int Attempts = sbyte.MaxValue; Attempts >= 0; --Attempts)
        {
            Thread.Sleep(sbyte.MaxValue);

            ImmutableHashSet<int> NewProcesses = [.. Process.GetProcesses().Select(Process => Process.Id)];

            NewProcesses = NewProcesses.Except(ExistingProcesses);

            if (NewProcesses.Count is 0) continue;

            PotentialProcess = NewProcesses.First();

            break;
        }

        Console.WriteLine(Win32Functions.SuspendProcess(Process.GetProcessById(PotentialProcess)));
    }
}
