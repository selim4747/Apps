# Universe Architect (NJG Edition) - Faz 1

## GitHub Actions ile Tek Tikla Derleme Adimlari

1. GitHub'da **Private (Ozel)** bir depo olustur (oyunun DLL'lerini yukleyecegin icin ozel olmasi onemli).
2. Bu ZIP icindeki tum dosyalari (`.github/workflows/build.yml` dahil) deponun ana dizinine yukle.
3. Telefonundaki oyunun klasorunden asagidaki DLL dosyalarini deponun `libs/` klasorune ekle:
   - `MelonLoader/net6/`: `MelonLoader.dll`, `0Harmony.dll`, `Il2CppInterop.Runtime.dll`
   - `MelonLoader/Il2CppAssemblies/`: `Assembly-CSharp.dll`, `Il2Cppmscorlib.dll`, `UnityEngine.CoreModule.dll`
4. GitHub'da **Actions** sekmesine gir -> **Build Universe Architect Mod** -> **Run workflow** butonuna bas.
5. Derleme bittiginde **Artifacts** kismindan `UniverseArchitect-Phase1-DLL` paketini indir.
6. Icindeki `UniverseArchitect.dll` dosyasini telefondaki `Android/data/com.greenheartgames.gdt/files/Mods/` klasorune at ve oyunu ac!
