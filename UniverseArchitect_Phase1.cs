using System;
using System.Collections.Generic;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppRarebyte.GDT; // Derlemede bulunamazsa "Rarebyte.GDT" olarak degistir

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "1.0.0", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    // =========================================================================
    // 1. VERI MODELLERI VE DENGE TAVANLARI
    // =========================================================================
    public enum ProjectCategory
    {
        UH,     // Ana Kanon (Tutarlilik ve Derinlik kurallarina tabi)
        UD,     // Baloncuk Evren (Tutarlilik riski 0, Core Fandom 0, +%5 Oynanis)
        GTBT    // Alternatif Zaman Cizgisi (Olu/yipranmis karakteri cezasiz kullanma, +%15 maliyet)
    }

    public enum GameScale
    {
        Small,
        Medium,
        Large,
        AAA
    }

    public static class BalanceCaps
    {
        public const float MaxLoreStoryBonus = 0.08f;      // Max +%8 Hikaye
        public const int MaxLoreBaseHype = 15;             // Max +15 Taban Hype
        public const int MaxTotalModHype = 35;             // Tum mod kaynaklarindan max +35 Hype
        public const float SoloBurnoutPenalty = 0.30f;     // Tukenmislikte -%30 hiz
    }

    public class CustomTopicDef
    {
        public string Id;
        public string Name;
        // Tur adina gore agirlik (0.6 = zayif, 0.8 = orta, 1.0 = mukemmel)
        public Dictionary<string, double> GenreWeights;
        // [0] = Young, [1] = Everyone, [2] = Mature
        public double[] AudienceWeights;
        public string SpecialBonusKey;
    }

    public class CustomEngineFeatureDef
    {
        public string Id;
        public string Name;
        public float CostMultiplier;
        public float BugMultiplier;
        public float DurationMultiplier;
        public int HypeBonus;
        public float SalesLifeMultiplier;
    }

    public static class Phase1Database
    {
        // 6 Yeni Konu ve Gercek GDT Tur/Kitle Agirliklari
        public static readonly List<CustomTopicDef> Topics = new List<CustomTopicDef>
        {
            new CustomTopicDef
            {
                Id = "njg_library_defense",
                Name = "Kütüphane & Gece Savunması",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.8 },
                    { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.6 }
                },
                AudienceWeights = new[] { 0.9, 0.7, 1.0 },
                SpecialBonusKey = "DayNightGameplayPlus10"
            },
            new CustomTopicDef
            {
                Id = "njg_5d_shift",
                Name = "5D Boyut Kayması",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 0.8 },
                    { "Simulation", 0.8 }, { "Strategy", 0.9 }, { "Casual", 0.9 }
                },
                AudienceWeights = new[] { 0.6, 0.7, 1.0 },
                SpecialBonusKey = "ArtPieceCriticMultiplier"
            },
            new CustomTopicDef
            {
                Id = "njg_planet_idle",
                Name = "Gezegen Kolonizasyonu (Idle)",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 0.6 }, { "Adventure", 0.7 }, { "RPG", 0.7 },
                    { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.9 }
                },
                AudienceWeights = new[] { 0.8, 1.0, 0.8 },
                SpecialBonusKey = "LongTailLowBudget"
            },
            new CustomTopicDef
            {
                Id = "njg_morph_animals",
                Name = "Şekil Değiştiren Hayvanlar",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 },
                    { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 0.8 }
                },
                AudienceWeights = new[] { 1.0, 1.0, 0.7 },
                SpecialBonusKey = "HighCharacterPopularity"
            },
            new CustomTopicDef
            {
                Id = "njg_star_run",
                Name = "Yıldız Koşusu",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 0.8 },
                    { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 1.0 }
                },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 },
                SpecialBonusKey = "FastDevLowBug"
            },
            new CustomTopicDef
            {
                Id = "njg_sailor_adventure",
                Name = "Denizci Macerası",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 },
                    { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.8 }
                },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 },
                SpecialBonusKey = "WorldDesignBonus"
            }
        };

        // 3 Ana Motor Ozelligi + Post-Credits & Mod Destegi
        public static readonly List<CustomEngineFeatureDef> EngineFeatures = new List<CustomEngineFeatureDef>
        {
            new CustomEngineFeatureDef
            {
                Id = "njg_lore_link",
                Name = "Evren Bağlantı Modülü (Lore Link)",
                CostMultiplier = 1.05f, BugMultiplier = 1.0f, DurationMultiplier = 1.0f,
                HypeBonus = 0, SalesLifeMultiplier = 1.0f
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_branching_narrative",
                Name = "Dallanan Hikaye & Çoklu Son",
                CostMultiplier = 1.25f, BugMultiplier = 1.20f, DurationMultiplier = 1.0f,
                HypeBonus = 0, SalesLifeMultiplier = 1.20f
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_soundtrack_rap",
                Name = "Stüdyo İçi Soundtrack & Rap Tanıtım",
                CostMultiplier = 0.70f, BugMultiplier = 1.0f, DurationMultiplier = 1.0f,
                HypeBonus = 15, SalesLifeMultiplier = 1.0f
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_post_credits",
                Name = "Jenerik Sonrası Sahne (Post-Credits)",
                CostMultiplier = 1.05f, BugMultiplier = 1.05f, DurationMultiplier = 1.04f,
                HypeBonus = 0, SalesLifeMultiplier = 1.0f
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_mod_support",
                Name = "Resmi Mod Desteği",
                CostMultiplier = 1.10f, BugMultiplier = 1.15f, DurationMultiplier = 1.15f,
                HypeBonus = 0, SalesLifeMultiplier = 1.20f
            }
        };
    }

    // =========================================================================
    // 2. TEK KISILIK ORDU (SOLO-DEV) & TUKENMISLIK SISTEMI
    // =========================================================================
    public static class SoloDevSystem
    {
        public static float BurnoutBar { get; private set; } = 0f;
        public static bool IsBurnedOut { get; private set; } = false;

        public static void OnDevelopmentWeekTick(int staffCount, GameScale scale)
        {
            if (staffCount > 1)
            {
                ResetBurnout();
                return;
            }

            if (scale == GameScale.Small || scale == GameScale.Medium)
                return;

            float gain = (scale == GameScale.Large) ? 6f : 12f;
            BurnoutBar = Mathf.Clamp(BurnoutBar + gain, 0f, 100f);

            if (BurnoutBar >= 100f)
                IsBurnedOut = true;
        }

        public static void ResetBurnout()
        {
            BurnoutBar = 0f;
            IsBurnedOut = false;
        }
    }

    // =========================================================================
    // 3. ANA MOD GIRISI
    // =========================================================================
    public class UniverseArchitectMod : MelonMod
    {
        public static ProjectCategory ActiveProjectCategory = ProjectCategory.UH;
        public static int PendingPostCreditsHype = 0;

        public static bool TopicsInjected = false;
        public static bool ResearchInjected = false;
        public static bool LoggedGameSizeOnce = false;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Universe Architect (NJG Edition) - Faz 1 yuklendi!");
            // Güvenli temel sürüm: Play Games akışına müdahale edilmez ve
            // gameplay patch'leri uygulanmaz. Böylece native kapanma riski
            // oluşturan sahte oturum durumu üretilmez.
            LoggerInstance.Msg("[Universe Architect] Temel mod aktif; Play Games ve gameplay patch'leri degistirilmedi.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            // Temel tanı sürümünde oyun sahnesine hiçbir gameplay verisi enjekte edilmez.
            LoggerInstance.Msg($"[Universe Architect] Temel mod aktif; gameplay enjeksiyonu kapali. Sahne: {sceneName}");
        }

        public static int ApplyCappedModHype(int rawModHype)
        {
            return Math.Min(rawModHype, BalanceCaps.MaxTotalModHype);
        }
    }

    // =========================================================================
    // 4. HARMONY YAMALARI (Rarebyte.GDT)
    // =========================================================================
    [HarmonyPatch(typeof(General), nameof(General.proceedOneWeek))]
    public static class Patch_WeekTickAndInit
    {
        static void Postfix(Company company, Il2CppSystem.Nullable<double> fractionalWeek)
        {
            if (company == null || !company.isCurrentlyDevelopingGame() || company.currentGame == null)
                return;

            int staffCount = company.staff != null ? company.staff.Count : 1;
            string sizeStr = company.currentGame.gameSize ?? "small";

            if (!UniverseArchitectMod.LoggedGameSizeOnce)
            {
                MelonLogger.Msg($"[NJG-Debug] Aktif gameSize degeri: '{sizeStr}'");
                UniverseArchitectMod.LoggedGameSizeOnce = true;
            }

            GameScale scale = ParseScale(sizeStr);
            SoloDevSystem.OnDevelopmentWeekTick(staffCount, scale);

            if (SoloDevSystem.IsBurnedOut && staffCount == 1)
            {
                MelonLogger.Warning("[Universe Architect] Tukenmislik aktif! Gelistirme hizi -%30.");
            }
        }

        private static GameScale ParseScale(string raw)
        {
            switch (raw.ToLowerInvariant())
            {
                case "medium": return GameScale.Medium;
                case "large":  return GameScale.Large;
                case "aaa":    return GameScale.AAA;
                default:       return GameScale.Small;
            }
        }
    }

    [HarmonyPatch(typeof(General), nameof(General.getGameSizeDurationFactor))]
    public static class Patch_SoloDevSpeed
    {
        static void Postfix(string gameSize, ref double __result)
        {
            if (SoloDevSystem.IsBurnedOut)
            {
                __result *= 1.30; // Sure %30 uzar
            }
        }
    }

    [HarmonyPatch(typeof(Research), nameof(Research.getAllItems))]
    public static class Patch_InjectEngineFeatures
    {
        static void Postfix(ref Il2CppSystem.Collections.Generic.List<ResearchItem> __result)
        {
            if (__result == null || UniverseArchitectMod.ResearchInjected) return;

            foreach (var feat in Phase1Database.EngineFeatures)
            {
                bool exists = false;
                for (int i = 0; i < __result.Count; i++)
                {
                    if (__result[i].id == feat.Id) { exists = true; break; }
                }
                if (exists) continue;

                var item = new ResearchItem();
                item.id = feat.Id;
                item.category = "Engine";
                item.cost = new Il2CppSystem.Nullable<double>(50000 * feat.CostMultiplier);
                item.engineCost = new Il2CppSystem.Nullable<double>(25000 * feat.CostMultiplier);
                item.pointsCost = new Il2CppSystem.Nullable<double>(30);
                item.enginePoints = new Il2CppSystem.Nullable<int>(20);

                __result.Add(item);
                MelonLogger.Msg($"[Universe Architect] Motor ozelligi eklendi: {feat.Name}");
            }

            UniverseArchitectMod.ResearchInjected = true;
        }
    }

    public static class TopicInjectorHelper
    {
        public static void TryInjectTopics()
        {
            if (UniverseArchitectMod.TopicsInjected) return;

            var allTopicsAssets = UnityEngine.Resources.FindObjectsOfTypeAll<Topics>();
            if (allTopicsAssets == null || allTopicsAssets.Length == 0) return;

            var topicsAsset = allTopicsAssets[0];
            if (topicsAsset.topics == null) return;

            var genreIndexMap = BuildRuntimeGenreIndexMap();

            foreach (var def in Phase1Database.Topics)
            {
                bool exists = false;
                for (int i = 0; i < topicsAsset.topics.Count; i++)
                {
                    if (topicsAsset.topics[i].id == def.Id) { exists = true; break; }
                }
                if (exists) continue;

                var newTopic = new TopicType();
                newTopic.id = def.Id;
                newTopic.name = def.Name;

                double[] g = { 0.8, 0.8, 0.8, 0.8, 0.8, 0.8 };
                foreach (var kvp in def.GenreWeights)
                {
                    if (genreIndexMap.TryGetValue(kvp.Key, out int idx) && idx >= 0 && idx < 6)
                    {
                        g[idx] = kvp.Value;
                    }
                }
                newTopic.genreWeightings = new GenreWeightings(g[0], g[1], g[2], g[3], g[4], g[5]);

                newTopic.audienceWeightings = new AudienceWeightings(
                    def.AudienceWeights[0],
                    def.AudienceWeights[1],
                    def.AudienceWeights[2]
                );

                topicsAsset.topics.Add(newTopic);
                MelonLogger.Msg($"[Universe Architect] Konu enjekte edildi: {def.Name}");
            }

            UniverseArchitectMod.TopicsInjected = true;
        }

                private static Dictionary<string, int> BuildRuntimeGenreIndexMap()
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var genreManager = Patch_CaptureGameGenre.Instance;
                if (genreManager != null)
                {
                    var allGenres = genreManager.getAll();
                    if (allGenres != null)
                    {
                        for (int i = 0; i < allGenres.Count; i++)
                        {
                            var genre = allGenres[i];
                            int idx = genreManager.getIndexOf(genre);
                            string key = !string.IsNullOrEmpty(genre.id) ? genre.id : genre.name;
                            if (!string.IsNullOrEmpty(key))
                            {
                                map[key] = idx;
                                MelonLogger.Msg($"[NJG-Debug] Genre Indeks -> {key} = {idx}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[NJG-Debug] Dinamik genre indeksi okunamadi: {ex.Message}");
            }

            if (map.Count == 0)
            {
                map["Action"] = 0; map["Adventure"] = 1; map["RPG"] = 2;
                map["Simulation"] = 3; map["Strategy"] = 4; map["Casual"] = 5;
            }
            return map;
        }
    }

    [HarmonyPatch(typeof(GameGenre), nameof(GameGenre.getAll))]
    public static class Patch_CaptureGameGenre
    {
        public static GameGenre Instance;

        static void Postfix(GameGenre __instance)
        {
            if (Instance == null && __instance != null)
                Instance = __instance;
        }
    }

    // Google Play Games, APK yeniden imzalandigi icin SIGN_IN_REQUIRED donuyor.
    // Orijinal oyun bu sonucu GamesResolutionActivity ile zorunlu giris ekranina
    // tasiyor. Giris basarisiz olsa da oyunun offline acilabilmesi icin bu akisi
    // durduruyoruz; LVL lisans patch'i bundan bagimsiz calismaya devam eder.
    [HarmonyPatch(typeof(SocialManagerSocialImpl), nameof(SocialManagerSocialImpl.Authenticate))]
    public static class Patch_OptionalPlayGamesSignIn
    {
        public static bool Prefix()
        {
            MelonLogger.Msg("[Universe Architect] Google Play Games girisi devre disi; offline devam ediliyor.");
            return false;
        }
    }

    [HarmonyPatch(typeof(Reviews), nameof(Reviews.reviewGame))]
    public static class Patch_ReviewGame
    {
        static void Postfix(Company company)
        {
            if (company == null || company.currentGame == null) return;
            var game = company.currentGame;

            MelonLogger.Msg($"[NJG-Debug] Oyun puanlandi! Orijinal Skor: {game.score}, Hype: {game.hypePoints}");

            if (UniverseArchitectMod.ActiveProjectCategory == ProjectCategory.UD)
            {
                game.score = Math.Min(10.0, game.score * 1.05);
                MelonLogger.Msg($"[Universe Architect] UD bonusu uygulandi -> Yeni Skor: {game.score:F2}");
            }

            if (UniverseArchitectMod.PendingPostCreditsHype > 0)
            {
                int capped = UniverseArchitectMod.ApplyCappedModHype(UniverseArchitectMod.PendingPostCreditsHype);
                company.adjustHype(capped);
                UniverseArchitectMod.PendingPostCreditsHype = 0;
                MelonLogger.Msg($"[Universe Architect] Post-Credits Hype aktarildi: +{capped}");
            }
        }
    }
}
