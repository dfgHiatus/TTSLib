using System.Reflection;
using TTSLib.Module;

namespace TTSLib.ModuleLoader;

/// <summary>
/// Loads a dll containing an implementation of the abstract Speaker class.
/// </summary>
public class SpeakerLoader
{
    private const string TTSPathName = "TTSLib";

    private Speaker speaker;

    /// <summary>
    /// Constructor for the SpeakerLoader class.
    /// </summary>
    public SpeakerLoader()
    {
        speaker = null;
    }

    /// <summary>
    /// Loads a dll containing an implementation of the Speaker class. Gets the first module from
    /// the Environment.SpecialFolder.ApplicationData directory
    /// </summary>
    /// <returns>True if the speaker was successfully loaded, false otherwise.</returns>
    public bool Load()
    {
        // Get all folders in the ApplicationData/TTSLib directory
        var dirs = Directory.GetDirectories(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            TTSPathName);

        if (dirs.Count() == 0)
        {
            return false;
        }

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            TTSPathName,
            dirs[0]);

        var dllFiles = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllFile);

                var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Speaker)));

                foreach (var type in types)
                {
                    try
                    {
                        var speakerInstance = Activator.CreateInstance(type) as Speaker;

                        if (speakerInstance.SupportedOperatingSystems.Contains(Environment.OSVersion.Platform))
                        {
                            if (speakerInstance.Initialize())
                            {
                                speaker = speakerInstance;
                                return true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error creating instance of {type.FullName}: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading {dllFile}: {e.Message}");
            }
        }

        return false;
    }


    // <summary>
    /// Loads the named speaker module from the default directory.
    /// </summary>
    /// <param name="moduleName">The name of the speaker module to load.</param>
    /// <returns>True if the speaker module was successfully loaded, false otherwise.</returns>
    public bool Load(string folderName)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            TTSPathName,
            folderName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return false;
        }

        var dllFiles = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllFile); // Throws when loading an unmanaged assembly?

                var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Speaker)));

                foreach (var type in types)
                {
                    try
                    {
                        var speakerInstance = Activator.CreateInstance(type) as Speaker;

                        if (speakerInstance.SupportedOperatingSystems.Contains(Environment.OSVersion.Platform))
                        {
                            if (speakerInstance.Initialize())
                            {
                                speaker = speakerInstance;
                                return true;
                            }
                        }
                    }
                    catch (BadImageFormatException) { } // Ignore unmanaged file errors
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error creating instance of {type.FullName}: {e.Message}");
                    }
                }
            }
            catch (BadImageFormatException) { } // Ignore unmanaged file errors
            catch (Exception e)
            {
                Console.WriteLine($"Error loading {dllFile}: {e.Message}");
            }
        }

        return false;
    }


    /// <summary>
    /// Gets the loaded speaker.
    /// </summary>
    /// <returns>The loaded speaker, or null if no speaker is loaded.</returns>
    public Speaker GetSpeaker()
    {
        return speaker;
    }

    /// <summary>
    /// Unloads the loaded speaker.
    /// </summary>
    public void Unload()
    {
        if (speaker != null)
        {
            speaker.Teardown();
            speaker = null;
        }
    }
}
