using AppKit;

namespace NeutrinoStudio.Mac.Shell
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.Main(args);
        }
    }
}
