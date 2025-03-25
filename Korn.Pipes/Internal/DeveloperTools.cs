#define DEVELOPER_MODE

using System.Runtime.CompilerServices;

static class DeveloperTools
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(string message)
    {
#if DEVELOPER_MODE
        System.Diagnostics.Debug.WriteLine($"Korn.Pipes: {message}");
#endif
    }
}