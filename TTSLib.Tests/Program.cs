using TTSLib.ModuleLoader;

namespace TTSLib;

internal class Program
{
    static void Main(string[] args)
    {
        var speakerLoader = new SpeakerLoader();
        if (!speakerLoader.Load("FonixTalk"))
        {
            Console.WriteLine("Failed to load module");
            return;
        }
        var speaker = speakerLoader.GetSpeaker();
        speaker.SynthesizeToDefaultAudioDevice("Hello, world!");
        // speakerLoader.Unload();
    }
}