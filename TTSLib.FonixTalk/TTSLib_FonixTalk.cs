using SharpTalk;
using System.Globalization;
using TTSLib.Module;

namespace TTSLib.FonixTalk;

public class TTSLib_FonixTalk : Speaker
{
    public override string Description => "FonixTalk TTS module";

    public override Version Version => new Version(1, 0, 0, 0);

    public override string Authors => "dfgHiatus";

    public override Uri Link => new Uri("https://github.com/dfgHiatus/TTSLib");

    public override HashSet<PlatformID> SupportedOperatingSystems => new() { PlatformID.Win32NT };

    public override Dictionary<CultureInfo, string> SupportedLanguages => new()
    {
        { new CultureInfo("en-US"), LanguageCode.EnglishUS },
        { new CultureInfo("en-GB"), LanguageCode.EnglishUK },
        { new CultureInfo("es-ES"), LanguageCode.SpanishCastilian },
        { new CultureInfo("es-MX"), LanguageCode.SpanishLatinAmerican },
        { new CultureInfo("de-DE"), LanguageCode.German },
        { new CultureInfo("fr-FR"), LanguageCode.French }
    };

    private FonixTalkEngine fonix;

    public override bool Initialize()
    {
        fonix = new FonixTalkEngine();
        IsReady = true;
        return true;
    }

    public override byte[] SynthesizeBytes(string text)
    {
        if (!IsReady) return Array.Empty<byte>();

        return fonix!.SpeakToMemory(text);
    }

    public override bool SynthesizeToFile(string text, string path, string fileName)
    {
        if (!IsReady) return false;

        var combinedPath = Path.Combine(path, fileName);
        fonix!.SpeakToWavFile(combinedPath, text);

        return true;
    }

    public override bool Teardown()
    {
        if (!IsReady) return false;
        fonix!.Dispose();
        IsReady = false;
        
        return true;
    }

    public override bool ChangeLanguage(CultureInfo culture)
    {
        if (!IsReady) return false;
        
        // TODO: Do we need to preserve the speaker params?
        var rate = fonix!.Rate;
        var voice = fonix!.Voice;
        var speakerParams = fonix!.SpeakerParams;
        fonix!.Dispose();

        try
        {
            // If the user provides a culture Fonix does not have, handle this
            fonix = new FonixTalkEngine(SupportedLanguages[culture], rate, voice);
            fonix.SpeakerParams = speakerParams;
        }
        catch
        {
            return false;
        }
        
        return true;
    }

    public override bool SetSpeakingRate(int rate)
    {
        if (!IsReady) return false;

        fonix.Rate = (uint) rate;

        return true;
    }

    public override bool SetGender(Gender gender)
    {
        if (!IsReady) return false;
        
        var sp = fonix.SpeakerParams;
        sp.Sex = gender == Gender.Male ? Sex.Male : Sex.Female;
        fonix.SpeakerParams = sp;

        return true;
    }

    public override bool SetVolume(float volume)
    {
        if (!IsReady) return false;

        var sp = fonix.SpeakerParams;
        sp.Loudness = (short) volume; // TODO: Figure out how to convert this
        fonix.SpeakerParams = sp;

        return true;
    }

    public override bool SynthesizeToDefaultAudioDevice(string text)
    {
        if (!IsReady) return false;

        fonix!.Speak(text);

        return true;
    }


    // We can't set these, so leave them as stubs
    public override bool SetPitch(int pitch) => false;

    public override bool SetAge(int age) => false;

    public override bool SetAccent(string accent) => false;
}