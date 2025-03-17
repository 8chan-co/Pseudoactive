using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pseudoactive;

internal static class Win32Functions
{
    internal static bool SuspendProcess(Process Process)
    {
        return ((NtSuspendProcess(Process.Handle) & 0xC0000000) >> 30) is 0;

        [DllImport("ntdll")] static extern int NtSuspendProcess(nint ProcessHandle);
    }

    internal static bool ResumeProcess(Process Process)
    {
        return ((NtResumeProcess(Process.Handle) & 0xC0000000) >> 30) is 0;

        [DllImport("ntdll")] static extern int NtResumeProcess(nint ProcessHandle);
    }
}
