using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppRarebyte.GDT;

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "1.1.3", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    // =========================================================================
    // 1. VERI MODELLERI VE DENGE TAVANLARI
    // =========================================================================
    public enum ProjectCategory
    {
        UH,     // Ana Kanon
        UD,     // Baloncuk Evren (+%5 skor)
        GTBT    // Alternatif Zaman Cizgisi
    }

    public static class BalanceCaps
    {
        public const int MaxTotalModHype = 35;
    }

    public class CustomTopicDef
    {
        public string Id;
        public string Name;
        public string IconTemplateKeyword;
        public Dictionary<string, double> GenreWeights;
        public double[] AudienceWeights; // [0] = Young, [1] = Everyone, [2] = Mature
        public double ScoreBonusMultiplier;
        public int BonusHype;
        public string BonusLabel;
    }

    public class CustomEngineFeatureDef
    {
        public string Id;
        public string Name;
        public string Description;
    }

    public class ReleasedGameRecord
    {
        public string GameId { get; set; }
        public string Title { get; set; }
        public string TopicId { get; set; }
        public string GameSize { get; set; }
        public string SequelTo { get; set; }
        public int ReleaseWeek { get; set; }
        public double FinalScore { get; set; }
        public string Category { get; set; }
        public string RecordedAtUtc { get; set; }
    }

    public class UniverseSaveData
    {
        public string UniverseName { get; set; } = "Ana Evren";
        public int CanonIntegrity { get; set; } = 100;
        public List<ReleasedGameRecord> ReleasedGames { get; set; } = new List<ReleasedGameRecord>();
    }

    public static class Phase1Database
    {
        public static readonly List<CustomTopicDef> Topics = new List<CustomTopicDef>
        {
            new CustomTopicDef
            {
                Id = "njg_library_defense",
                Name = "Kütüphane & Gece Savunması",
                IconTemplateKeyword = "Horror",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.8 },
                    { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.6 }
                },
                AudienceWeights = new[] { 0.9, 0.7, 1.0 },
                ScoreBonusMultiplier = 1.04,
                BonusHype = 4,
                BonusLabel = "Gündüz/Gece Döngüsü (+%4 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_5d_shift",
                Name = "5D Boyut Kayması",
                IconTemplateKeyword = "Sci-Fi",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 0.8 },
                    { "Simulation", 0.8 }, { "Strategy", 0.9 }, { "Casual", 0.9 }
                },
                AudienceWeights = new[] { 0.6, 0.7, 1.0 },
                ScoreBonusMultiplier = 1.06,
                BonusHype = 0,
                BonusLabel = "Eleştirmen Sanatı (+%6 Skor)"
            },
            new CustomTopicDef
            {
                Id = "njg_planet_idle",
                Name = "Gezegen Kolonizasyonu (Idle)",
                IconTemplateKeyword = "Space",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 0.6 }, { "Adventure", 0.7 }, { "RPG", 0.7 },
                    { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.9 }
                },
                AudienceWeights = new[] { 0.8, 1.0, 0.8 },
                ScoreBonusMultiplier = 1.03,
                BonusHype = 5,
                BonusLabel = "Uzun Soluklu Koloni (+%3 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_morph_animals",
                Name = "Şekil Değiştiren Hayvanlar",
                IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 },
                    { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 0.8 }
                },
                AudienceWeights = new[] { 1.0, 1.0, 0.7 },
                ScoreBonusMultiplier = 1.02,
                BonusHype = 8,
                BonusLabel = "Karakter Popülaritesi (+%2 Skor, +8 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_star_run",
                Name = "Yıldız Koşusu",
                IconTemplateKeyword = "Racing",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 0.8 },
                    { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 1.0 }
                },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 },
                ScoreBonusMultiplier = 1.03,
                BonusHype = 6,
                BonusLabel = "Hızlı Tempo (+%3 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_sailor_adventure",
                Name = "Denizci Macerası",
                IconTemplateKeyword = "Pirate",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 },
                    { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.8 }
                },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 },
                ScoreBonusMultiplier = 1.05,
                BonusHype = 4,
                BonusLabel = "Açık Deniz Tasarımı (+%5 Skor, +4 Hype)"
            }
        };

        public static readonly List<CustomEngineFeatureDef> EngineFeatures = new List<CustomEngineFeatureDef>
        {
            new CustomEngineFeatureDef
            {
                Id = "njg_lore_link",
                Name = "Evren Bağlantı Modülü",
                Description = "Oyunları aynı kurgusal evrene ve kronolojiye bağlar."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_branching_narrative",
                Name = "Dallanan Hikaye & Çoklu Son",
                Description = "Oyuncu seçimlerine göre evreni şekillendiren dallanan anlatı."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_soundtrack_rap",
                Name = "Stüdyo İçi Soundtrack & Rap",
                Description = "Özgün oyun müzikleri ve rap tanıtım parçalarıyla Hype patlaması."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_post_credits",
                Name = "Jenerik Sonrası Sahne",
                Description = "Bir sonraki devam oyunu için gizli ipucu ve Hype aktarımı."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_mod_support",
                Name = "Resmi Mod Desteği",
                Description = "Topluluk modlarıyla oyunun satış ömrünü ve sadakatini uzatır."
            }
        };

        public static CustomTopicDef FindTopicById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Topics.Count; i++)
            {
                if (string.Equals(Topics[i].Id, id, StringComparison.OrdinalIgnoreCase))
                    return Topics[i];
            }
            return null;
        }
    }

    // =========================================================================
    // 2. JSON EVREN KAYIT SISTEMI (FAZ 2 HAZIR)
    // =========================================================================
    public static class UniverseSaveManager
    {
        public static UniverseSaveData Current { get; private set; } = new UniverseSaveData();
        private static string SavePath => Path.Combine(Application.persistentDataPath, "UniverseArchitect_Save.json");

        public static void Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    var loaded = JsonSerializer.Deserialize<UniverseSaveData>(json);
                    if (loaded != null)
                    {
                        Current = loaded;
                        MelonLogger.Msg($"[Universe Architect] Evren kaydi yuklendi! ({Current.ReleasedGames.Count} oyun, Tutarlilik: {Current.CanonIntegrity})");
                        return;
                    }
                }
                Save();
                MelonLogger.Msg($"[Universe Architect] Yeni evren kayit dosyasi olusturuldu: {SavePath}");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] JSON yukleme hatasi: {ex.Message}");
            }
        }

        public static void Save()
        {
            try
            {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Current, opts);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] JSON kaydetme hatasi: {ex.Message}");
            }
        }

        public static void RecordReleasedGame(Game game, string topicId, double score, ProjectCategory category)
        {
            Current.ReleasedGames.Add(new ReleasedGameRecord
            {
                GameId = game != null ? (game.id ?? "") : "",
                Title = game != null && !string.IsNullOrEmpty(game.title) ? game.title : "Adsiz Oyun",
                TopicId = string.IsNullOrEmpty(topicId) ? "unknown" : topicId,
                GameSize = game != null ? (game.gameSize ?? "small") : "small",
                SequelTo = game != null ? (game.sequelTo ?? "") : "",
                ReleaseWeek = game != null ? game.releaseWeek : 0,
                FinalScore = Math.Round(score, 2),
                Category = category.ToString(),
                RecordedAtUtc = DateTime.UtcNow.ToString("o")
            });
            Save();
        }
    }

    // =========================================================================
    // 3. GERCEK OYUN ICI BILDIRIM YARDIMCISI
    // =========================================================================
    public static class NotificationHelper
    {
        public static void Show(Company company, string header, string text)
        {
            MelonLogger.Msg($"[Universe Architect Bildirim] {header}: {text}");
            if (company == null) return;

            try
            {
                var notif = new Notification(header, text);
                if (company.notifications != null)
                {
                    company.notifications.Add(notif);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] Oyun ici bildirim eklenemedi: {ex.Message}");
            }
        }
    }

    // =========================================================================
    // 4. ANA MOD GIRISI
    // =========================================================================
    public class UniverseArchitectMod : MelonMod
    {
        public static ProjectCategory ActiveProjectCategory = ProjectCategory.UH;
        public static bool TopicsInjected = false;
        public static bool EngineFeaturesInjected = false;
        private float _injectTimer = 0f;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Universe Architect v1.1.3 (Offline Kilit + 6 Konu + 5 Motor TryCast + JSON) baslatiliyor...");
            UniverseSaveManager.Load();

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v113");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v1.1.3 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        public override void OnUpdate()
        {
            if (TopicsInjected && EngineFeaturesInjected) return;

            _injectTimer += Time.unscaledDeltaTime;
            if (_injectTimer < 2.0f) return;
            _injectTimer = 0f;

            if (!TopicsInjected)
                TopicInjectorHelper.TryInjectTopics();

            if (!EngineFeaturesInjected)
                EngineFeatureInjectorHelper.TryInjectFromScriptableObject();
        }

        public static int ApplyCappedModHype(int rawModHype)
        {
            return Math.Min(rawModHype, BalanceCaps.MaxTotalModHype);
        }
    }

    // =========================================================================
    // 5. KONU ENJEKSIYONU (KANCASIZ)
    // =========================================================================
    public static class TopicInjectorHelper
    {
        public static void TryInjectTopics()
        {
            if (UniverseArchitectMod.TopicsInjected) return;

            try
            {
                var allTopicsAssets = UnityEngine.Resources.FindObjectsOfTypeAll<Topics>();
                if (allTopicsAssets == null || allTopicsAssets.Length == 0)
                    return;

                var topicsAsset = allTopicsAssets[0];
                if (topicsAsset == null || topicsAsset.topics == null || topicsAsset.topics.Count == 0)
                    return;

                var templateTopic = topicsAsset.topics[0];
                MelonLogger.Msg($"[NJG-Debug] Topics bulundu! Mevcut: {topicsAsset.topics.Count}");

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
                    newTopic.missionOverrides = templateTopic.missionOverrides;

                    newTopic.genreWeightings = new GenreWeightings(
                        def.GenreWeights["Action"],
                        def.GenreWeights["Adventure"],
                        def.GenreWeights["RPG"],
                        def.GenreWeights["Simulation"],
                        def.GenreWeights["Strategy"],
                        def.GenreWeights["Casual"]
                    );

                    newTopic.audienceWeightings = new AudienceWeightings(
                        def.AudienceWeights[0],
                        def.AudienceWeights[1],
                        def.AudienceWeights[2]
                    );

                    topicsAsset.topics.Add(newTopic);
                    MelonLogger.Msg($"[Universe Architect] Konu eklendi: {def.Name}");
                }

                UniverseArchitectMod.TopicsInjected = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] Konu enjeksiyon hatasi: {ex}");
            }
        }
    }

    // =========================================================================
    // 6. MOTOR / AR-GE ENJEKSIYONU (SIFIR NULLABLE + TryCast v1.1.3)
    // =========================================================================
    public static class EngineFeatureInjectorHelper
    {
        public static void TryInjectFromScriptableObject()
        {
            if (UniverseArchitectMod.EngineFeaturesInjected) return;

            try
            {
                var researchAssets = UnityEngine.Resources.FindObjectsOfTypeAll<Research>();
                if (researchAssets == null || researchAssets.Length == 0)
                    return;

                var research = researchAssets[0];
                if (research == null) return;

                var allItems = research.getAllItems();
                var engineItems = research.engineItems;
                if (allItems == null || allItems.Count == 0)
                    return;

                ResearchItem template = (engineItems != null && engineItems.Count > 0)
                    ? engineItems[0]
                    : allItems[0];

                if (template == null) return;

                var cloneMethod = AccessTools.Method(typeof(Il2CppSystem.Object), "MemberwiseClone");
                if (cloneMethod == null)
                {
                    MelonLogger.Warning("[NJG-Debug] MemberwiseClone bulunamadi, motor enjeksiyonu atlandi.");
                    UniverseArchitectMod.EngineFeaturesInjected = true;
                    return;
                }

                MelonLogger.Msg($"[NJG-Debug] Research hazir! allItems: {allItems.Count}, engineItems: {engineItems?.Count ?? 0}, Sablon: {template.id}");

                int addedCount = 0;
                foreach (var feat in Phase1Database.EngineFeatures)
                {
                    if (ContainsId(allItems, feat.Id))
                        continue;

                    // IL2CPP MemberwiseClone sonucunu TryCast ile guvenle ResearchItem'a cevir
                    var rawClone = cloneMethod.Invoke(template, null) as Il2CppSystem.Object;
                    var item = rawClone != null ? rawClone.TryCast<ResearchItem>() : null;
                    if (item == null) continue;

                    item.id = feat.Id;
                    item.name = feat.Name;
                    item.description = feat.Description;

                    allItems.Add(item);

                    if (engineItems != null && !ContainsId(engineItems, feat.Id))
                        engineItems.Add(item);

                    addedCount++;
                }

                MelonLogger.Msg($"[Universe Architect] {addedCount} motor ozelligi sifir Nullable ile guvenle klonlanip eklendi!");
                UniverseArchitectMod.EngineFeaturesInjected = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] Kancasiz motor enjeksiyon hatasi: {ex}");
                UniverseArchitectMod.EngineFeaturesInjected = true;
            }
        }

        private static bool ContainsId(Il2CppSystem.Collections.Generic.List<ResearchItem> list, string id)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].id == id) return true;
            }
            return false;
        }
    }

    // =========================================================================
    // 7. INCELEME PASIF BONUSLARI & CIFTE BONUS KILIDI & JSON KAYIT
    // =========================================================================
    [HarmonyPatch(typeof(Reviews), nameof(Reviews.reviewGame))]
    public static class Patch_ReviewGame
    {
        private static readonly HashSet<IntPtr> _processedGames = new HashSet<IntPtr>();

        static void Postfix(Company company)
        {
            try
            {
                if (company == null || company.currentGame == null) return;

                var game = company.currentGame;
                if (game.Pointer != IntPtr.Zero && !_processedGames.Add(game.Pointer))
                    return;

                string title = !string.IsNullOrEmpty(game.title) ? game.title : "Yeni Oyun";
                string topicId = game.topic != null ? game.topic.id : null;

                var customTopic = Phase1Database.FindTopicById(topicId);
                if (customTopic != null)
                {
                    if (customTopic.ScoreBonusMultiplier > 1.0)
                    {
                        game.score = Math.Min(10.0, game.score * customTopic.ScoreBonusMultiplier);
                    }
                    if (customTopic.BonusHype > 0)
                    {
                        int cappedHype = UniverseArchitectMod.ApplyCappedModHype(customTopic.BonusHype);
                        company.adjustHype(cappedHype);
                    }

                    NotificationHelper.Show(
                        company,
                        $"Konu Bonusu: {customTopic.Name}",
                        $"{title} için özel bonus devreye girdi: {customTopic.BonusLabel}"
                    );
                }

                if (UniverseArchitectMod.ActiveProjectCategory == ProjectCategory.UD)
                {
                    game.score = Math.Min(10.0, game.score * 1.05);
                }

                UniverseSaveManager.RecordReleasedGame(game, topicId, game.score, UniverseArchitectMod.ActiveProjectCategory);
                MelonLogger.Msg($"[Universe Architect] Oyun kaydedildi (tekil): '{title}' (Konu: {topicId ?? "standart"}, Skor: {game.score:F2})");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] ReviewGame hatasi: {ex.Message}");
            }
        }
    }

    // =========================================================================
    // 8. GOOGLE PLAY GAMES OFFLINE YAMALARI (GUVENLIK KILIDI)
    // =========================================================================
    [HarmonyPatch(typeof(SocialManagerSocialImpl), "InitPlayGamesPlatform")]
    public static class Patch_BlockInitPlayGames
    {
        static bool Prefix()
        {
            MelonLogger.Msg("[Universe Architect] InitPlayGamesPlatform durduruldu.");
            return false;
        }
    }

    [HarmonyPatch(typeof(SocialManagerSocialImpl), nameof(SocialManagerSocialImpl.Authenticate))]
    public static class Patch_BlockSocialAuthenticate
    {
        static bool Prefix(Il2CppSystem.Action<bool, string> action)
        {
            MelonLogger.Msg("[Universe Architect] SocialManager.Authenticate durduruldu -> success=false.");
            action?.Invoke(false, "Offline mod");
            return false;
        }
    }

    [HarmonyPatch(typeof(GameController), nameof(GameController.AuthenticateSocialManager))]
    public static class Patch_BlockGameControllerAuth
    {
        static bool Prefix(Il2CppSystem.Action<bool> callback, bool resetUserDeclinedFlag)
        {
            MelonLogger.Msg("[Universe Architect] AuthenticateSocialManager durduruldu -> callback(false).");
            callback?.Invoke(false);
            return false;
        }
    }

    [HarmonyPatch(typeof(SocialManagerSocialImpl), "get_IsAuthenticated")]
    public static class Patch_ForceOfflineAuthState
    {
        static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}