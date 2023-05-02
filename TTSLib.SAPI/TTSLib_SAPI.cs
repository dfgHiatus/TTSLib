using System.Globalization;
using System.Speech.Synthesis;
using TTSLib.Module;

namespace TTSLib.SAPI;

/// <summary>
/// Defines the methods and properties a Speaker module must have
/// </summary>
public class SapiSpeaker : Speaker
{
    private SpeechSynthesizer synthesizer;

    /// <summary>
    /// Module description
    /// </summary>
    public override string Description => "Microsoft Speech API (SAPI) speaker";

    /// <summary>
    /// Version
    /// </summary>
    public override Version Version => new Version(1, 0, 0, 0);

    /// <summary>
    /// Author(s)
    /// </summary>
    public override string Authors => "dfgHiatus";

    /// <summary>
    /// A set of supported operating systems this module can run on
    /// </summary>
    public override HashSet<PlatformID> SupportedOperatingSystems => new() { PlatformID.Win32NT };

    public override Dictionary<CultureInfo, string> SupportedLanguages
    {
        get
        {
            var languages = new Dictionary<CultureInfo, string>();
            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                VoiceInfo info = voice.VoiceInfo;
                CultureInfo culture = info.Culture;
                if (!languages.ContainsKey(culture))
                {
                    languages.TryAdd(culture, info.Name);
                }
            }
            return languages;
        }
    }

    public override bool ChangeLanguage(CultureInfo culture)
    {
        try
        {
            synthesizer.SelectVoice(SupportedLanguages[culture]);
            return true;
        }
        catch
        {
            return false;
        }
        
    }

    public override bool Initialize()
    {
        synthesizer = new SpeechSynthesizer(); // Dies here
        synthesizer.SetOutputToDefaultAudioDevice();
        return true;
    }

    public override bool SetAccent(string accent) => false;

    public override bool SetAge(int age) => false;

    public override bool SetGender(Gender gender)
    {
        synthesizer.SelectVoiceByHints(
            gender == Gender.Male ? VoiceGender.Male : VoiceGender.Female);
        return true;
    }

    public override bool SetPitch(int pitch) => false;

    public override bool SetSpeakingRate(int rate)
    {
        synthesizer.Rate = rate;
        return true;
    }

    public override bool SetVolume(float volume)
    {
        synthesizer.Volume = (int) volume;
        return true;
    }
    
    public override byte[] SynthesizeBytes(string text)
    {
        using (var stream = new MemoryStream())
        {
            synthesizer.SetOutputToWaveStream(stream);
            synthesizer.Speak(text);
            synthesizer.SetOutputToDefaultAudioDevice();
            return stream.ToArray();
        }
    }

    public override bool SynthesizeToDefaultAudioDevice(string text)
    {
        synthesizer.SetOutputToDefaultAudioDevice();
        synthesizer.Speak(text);
        return true;
    }

    // TODO Return audio stream to *what*
    public override bool SynthesizeToFile(string text, string path, string fileName)
    {
        var combinedPath = Path.Combine(path, fileName);
        synthesizer.SetOutputToWaveFile(combinedPath);
        synthesizer.Speak(text);
        synthesizer.SetOutputToDefaultAudioDevice();
        return true;
    }

    public override bool Teardown()
    {
        synthesizer.Dispose();
        return true;
    }
}