using System.Globalization;
using System.Runtime.InteropServices;

namespace TTSLib.Module;

/// <summary>
/// Defines the methods and properties a Speaker module must have
/// </summary>
public abstract class Speaker
{
    /// <summary>
    /// Defines the name of where the module is stored. This is used to determine where to load the module from
    /// </summary>
    public abstract string FilePath { get; }

    /// <summary>
    /// Module description
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Version
    /// </summary>
    public abstract Version Version { get; }

    /// <summary>
    /// Author(s)
    /// </summary>
    public abstract string Authors { get; }

    /// <summary>
    /// An optional link to a project page. This can be left empty
    /// </summary>
    public virtual Uri Link { get; }

    /// <summary>
    /// For projects with multiple assemblies, this defines the order in which they should be loaded.
    /// This can be left empty
    /// </summary>
    public virtual List<string> LoadOrder { get; }

    /// <summary>
    /// A set of supported operating systems this module can run on
    /// </summary>
    public abstract HashSet<PlatformID> SupportedOperatingSystems { get; }

    /// <summary>
    /// A collection of languages this module supports. Maps a unified culture to the specific language schemas
    /// individual TTS engines use
    /// </summary>
    public abstract Dictionary<CultureInfo, string> SupportedLanguages { get; }

    /// <summary>
    /// If the Speaker is in a functional state or not
    /// </summary>
    public bool IsReady;

    /// <summary>
    /// Initializes the speaker. You should specify what operating systems and languages are supported here.
    /// </summary>
    /// <returns>If the initialization was successful</returns>
    public abstract bool Initialize();

    /// <summary>
    /// Generates audio per individual speaker
    /// </summary>
    /// <param name="text">The text to be spoken</param>
    /// <returns>An array of floats corresponding to the audio samples of the synthesized speech</returns>
    public abstract byte[] SynthesizeBytes(string text);

    public float[] SynthesizeFloats(string text)
    {
        if (!IsReady) return Array.Empty<float>();

        byte[] audioBytes = SynthesizeBytes(text);
        float[] audioFloats = new float[audioBytes.Length / 2]; // Each sample is 2 bytes
        for (int i = 0; i < audioFloats.Length; i++)
        {
            // Convert two bytes to an integer
            short sample = BitConverter.ToInt16(audioBytes, i * 2);

            // Normalize the integer to a float between -1.0 and 1.0
            audioFloats[i] = sample / 32768.0f;
        }
        return audioFloats;
    }

    /// <summary>
    /// Speaks audio to the default audio device
    /// </summary>
    /// <param name="text"></param>
    /// <returns>True if the volume was successfully set, false otherwise</returns>
    public abstract bool SynthesizeToDefaultAudioDevice(string text);

    /// <summary>
    /// Saves the file to a file on the user's computer
    /// </summary>
    /// <param name="text"></param>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns>True if the volume was successfully set, false otherwise.</returns>
    public abstract bool SynthesizeToFile(string text, string path, string fileName);

    /// <summary>
    /// Change the spoken language of the module
    /// </summary>
    /// <param name="culture"></param>
    /// <returns>True if the volume was successfully set, false otherwise.</returns>
    public abstract bool ChangeLanguage(CultureInfo culture);

    /// <summary>
    /// Sets the volume of the current speaker
    /// </summary>
    /// <param name="volume">The new volume to set.</param>
    /// <returns>True if the volume was successfully set, false otherwise.</returns>
    public abstract bool SetVolume(float volume);

    /// <summary>
    /// Sets the gender of the current speaker
    /// </summary>
    /// <param name="gender">The new gender to set.</param>
    /// <returns>True if the gender was successfully set, false otherwise.</returns>
    public abstract bool SetGender(Gender gender);

    /// <summary>
    /// Sets the speaking rate of the current speaker
    /// </summary>
    /// <param name="rate">The new speaking rate to set.</param>
    /// <returns>True if the speaking rate was successfully set, false otherwise.</returns>
    public abstract bool SetSpeakingRate(int rate);

    /// <summary>
    /// Sets the pitch of the current speaker
    /// </summary>
    /// <param name="pitch">The new pitch to set.</param>
    /// <returns>True if the pitch was successfully set, false otherwise.</returns>
    public abstract bool SetPitch(int pitch);

    /// <summary>
    /// Sets the age of the current speaker
    /// </summary>
    /// <param name="age">The new age to set.</param>
    /// <returns>True if the age was successfully set, false otherwise.</returns>
    public abstract bool SetAge(int age);

    /// <summary>
    /// Sets the accent of the current speaker
    /// </summary>
    /// <param name="accent">The new accent to set.</param>
    /// <returns>True if the accent was successfully set, false otherwise.</returns>
    public abstract bool SetAccent(string accent);

    /// <summary>
    /// Loads a Windows specific DLL from the embedded resources and extracts it to the user's AppData folder
    /// </summary>
    /// <param name="library"></param>
    /// <returns>True if the lib was extracted and loaded, false otherwise</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public bool LoadWindowsLibrary(string library, Stream manifestResourceStream)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            throw new PlatformNotSupportedException("This operation is only supported on Windows");
        }

        // Extract the Embedded DLL
        var dirName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TTSLib",
            FilePath);

        var dllPath = Path.Combine(dirName, library);

        // TODO embed this in this assembly
        try
        {
            using (Stream outFile = File.Create(dllPath))
            {
                const int sz = 4096;
                var buf = new byte[sz];
                while (true)
                {
                    if (manifestResourceStream == null) continue;
                    var nRead = manifestResourceStream.Read(buf, 0, sz);
                    if (nRead < 1)
                        break;
                    outFile.Write(buf, 0, nRead);
                }
            }

            LoadLibrary(dllPath);
            return true;
        }
        catch (Exception)
        {
            return false;
        } 
    }

#if OS_WINDOWS
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);
#endif

    /// <summary>
    /// Stops the individual speaker and frees all resources associates with it
    /// </summary>
    /// <returns>True if the teardown was successful, false otherwise.</returns>
    public abstract bool Teardown();
}

public enum Gender
{
    Male,
    Female
}