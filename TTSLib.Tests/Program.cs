using TTSLib.ModuleLoader;

namespace TTSLib;

internal class Program
{
    static void Main(string[] args)
    {
        var speakerLoader = new SpeakerLoader();
        if (!speakerLoader.Load("SAPI"))
        {
            Console.WriteLine("Failed to load module");
            return;
        }
        var speaker = speakerLoader.GetSpeaker();
        speaker.SynthesizeToDefaultAudioDevice("Hello, world!");
        
        // speakerLoader.Unload();
    }
}