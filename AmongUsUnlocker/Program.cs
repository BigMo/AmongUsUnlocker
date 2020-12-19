using Il2CppInspector.Reflection;
using SteamLib;
using System;
using System.Linq;

namespace AmongUsUnlocker
{
    class Program
    {
        static string ASCII = @"
=================================================================================================================
   _____                                   ____ ___         ____ ___      .__                 __                 
  /  _  \   _____   ____   ____    ____   |    |   \______ |    |   \____ |  |   ____   ____ |  | __ ___________ 
 /  /_\  \ /     \ /  _ \ /    \  / ___\  |    |   /  ___/ |    |   /    \|  |  /  _ \_/ ___\|  |/ // __ \_  __ \
/    |    \  Y Y  (  <_> )   |  \/ /_/  > |    |  /\___ \  |    |  /   |  \  |_(  <_> )  \___|    <\  ___/|  | \/
\____|__  /__|_|  /\____/|___|  /\___  /  |______//____  > |______/|___|  /____/\____/ \___  >__|_ \\___  >__|   
        \/      \/            \//_____/                \/               \/    by Zat 2020  \/     \/    \/       
=================================================================================================================";

        static BinaryPatch RET_TRUE = new BinaryPatch(3, new byte[]
        {
            0xB0, 0x01, //MOV AL, 0x1
            0x5D, //POP EBP
            0xC3 //RET
        });

        static void Main(string[] args)
        {
            var logger = new CrapLogger("AUU");
            //ASCII Art lol.
            var w = ASCII.Split('\n').Max(l => l.Length);
            Console.BufferWidth = Console.WindowWidth = w;
            Console.WriteLine(ASCII);

            try
            {

                //Locate game
                logger.Log("Initializing SteamManager...");
                var steam = new SteamManager();
                logger.Log("Locating Among Us...");
                var app = steam.SteamLibraries.SelectMany(x => x.Apps).FirstOrDefault(x => x.AppId == 945360 || x.Name == "Among Us");
                if (app == null)
                    throw new Exception("Could not find Among Us installation!");
                logger.Info("Found game at: \"{0}\"", app.Path);
                var among = new AmongUsFS(new System.IO.DirectoryInfo(app.Path));
                var ga = among.GameAssembly;
                var gm = among.GlobalMetaData;
                if (!ga.Exists || !gm.Exists)
                    throw new Exception("GameAssembly.dll or global-metadata.dat not found!");
                logger.Info("Found GameAssembly.dll and global-metadata.dat!");
                logger.Info("Creating backup of GameAssembly.dll...");
                var gaB = among.CreateBackup();

                //Init IL2CPP
                logger.Log("Parsing IL2CPP: This can take a while...");
                var il2cpp = Il2CppInspector.Il2CppInspector.LoadFromFile(gaB.FullName, gm.FullName, null, true).FirstOrDefault();

                logger.Log("Building IL2CPP TypeModel: This can take a while...");
                var model = new TypeModel(il2cpp);

                //Find methods + section
                logger.Log("Searching for \"Unlocked\" methods...");
                var methods = model.Types.Where(t => t.HasElementType).Select(t => t.ElementType).SelectMany<TypeInfo, MethodInfo>(t => t.DeclaredMethods).ToArray();
                var mUnlockPets = methods.FirstOrDefault(m => m.Name.Contains("GetUnlockedPets") && m.IsAssembly && m.ReturnType.Name == "Boolean");
                var mUnlockSkins = methods.FirstOrDefault(m => m.Name.Contains("GetUnlockedSkins") && m.IsAssembly && m.ReturnType.Name == "Boolean");
                var mUnlockHats = methods.FirstOrDefault(m => m.Name.Contains("GetUnlockedHats") && m.IsAssembly && m.ReturnType.Name == "Boolean");
                var textSection = il2cpp.BinaryImage.GetSections().FirstOrDefault(s => s.Name == ".text");


                if (mUnlockPets == null) throw new Exception($"Could not find \"GetUnlockedPets\"!");
                logger.Info(" - Found {0} at 0x{1}", mUnlockPets.Name, mUnlockPets.VirtualAddress.Value.Start.ToString("x8"));
                if (mUnlockSkins == null) throw new Exception($"Could not find \"GetUnlockedSkins\"!");
                logger.Info(" - Found {0} at 0x{1}", mUnlockSkins.Name, mUnlockSkins.VirtualAddress.Value.Start.ToString("x8"));
                if (mUnlockHats == null) throw new Exception($"Could not find \"GetUnlockedHats\"!");
                logger.Info(" - Found {0} at 0x{1}", mUnlockHats.Name, mUnlockHats.VirtualAddress.Value.Start.ToString("x8"));

                if (textSection == null) throw new Exception("Could not found \".text\" section!");
                logger.Info(" - \".text\" section at 0x{0}", textSection.ImageStart.ToString("x8"));

                //Write patches
                logger.Log("Opening GameAssembly.dll for patching...");
                using (var file = ga.OpenWrite())
                {
                    var mUnlockPetsPtr = mUnlockPets.VirtualAddress.Value.Start - il2cpp.BinaryImage.GlobalOffset - (textSection.VirtualStart - il2cpp.BinaryImage.ImageBase) + textSection.ImageStart;
                    var mUnlockSkinsPtr = mUnlockSkins.VirtualAddress.Value.Start - il2cpp.BinaryImage.GlobalOffset - (textSection.VirtualStart - il2cpp.BinaryImage.ImageBase) + textSection.ImageStart;
                    var mUnlockHatsPtr = mUnlockHats.VirtualAddress.Value.Start - il2cpp.BinaryImage.GlobalOffset - (textSection.VirtualStart - il2cpp.BinaryImage.ImageBase) + textSection.ImageStart;

                    logger.Info("Patching {0} at 0x{1}", mUnlockPets.Name, mUnlockPetsPtr.ToString("x8"));
                    RET_TRUE.Apply(file, (long)mUnlockPetsPtr);
                    logger.Info("Patching {0} at 0x{1}", mUnlockSkins.Name, mUnlockSkinsPtr.ToString("x8"));
                    RET_TRUE.Apply(file, (long)mUnlockSkinsPtr);
                    logger.Info("Patching {0} at 0x{1}", mUnlockHats.Name, mUnlockHatsPtr.ToString("x8"));
                    RET_TRUE.Apply(file, (long)mUnlockHatsPtr);
                }
                logger.Info("Patches applied successfully! Close the Unlocker and you're good to go!");

                logger.Warn("If you like the unlockables: please buy them! Support the developers of this game!");

            }
            catch(Exception ex)
            {
                logger.Error("AmongUsUnlocker ran into a problem:");
                do
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                } while (ex != null);
            }
            Console.WriteLine("[Press ENTER to exit]");
            Console.ReadLine();
        }
    }
}
