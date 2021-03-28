using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

var processes = Process.GetProcessesByName("RobloxPlayerBeta");

if (processes.Length == 0)
{
    Console.WriteLine("Process not found.");
    return -1;
}

[DllImport("user32.dll")]
static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

var HWND_TOPMOST = new IntPtr(-1);
const uint SWP_NOSIZE = 0x0001;
const uint SWP_NOMOVE = 0x0002;
const uint SWP_SHOWWINDOW = 0x0040;

// Call this way:
SetWindowPos(processes[0].MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
return 0;