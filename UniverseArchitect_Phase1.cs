using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppRarebyte.GDT;

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "1.2.0", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    // =========================================================================
    // 1. TEST AYARLARI, VERI MODELLERI VE DENGE TAVANLARI
    // =========================================================================
    public static class ModSettings
    {
        // Test bittiginde normal hiza donmek icin bunu false yapman yeterli
        public const bool TestMode10xResearch = true;
        public const double ResearchMultiplier = 10.0;
        public const double TestStarterResearchBoost = 100.0;
    }

    public enum ProjectCategory
    {
        UH,     // Ana Kanon
        UD,     // Baloncuk Evren (+%5 skor)
        GTBT    // Alternatif Zaman Cizgisi
    }

    public static class BalanceCaps
    {
        public const double MaxLoreScoreBonus = 0.08;   // Max +%8 Skor
        public const int MaxLoreBaseHype = 15;          // Max +15 Taban Hype
        public const int MaxTotalModHype = 35;          // Tek oyunda tum mod kaynaklarindan max +35 Hype
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
        public List<string> UsedModFeatures { get; set; } = new List<string>();
        public string RecordedAtUtc { get; set; }
    }

    public class UniverseSaveData
    {
        public string UniverseName { get; set; } = "Ana Evren";
        public int CanonIntegrity { get; set; } = 100;
        public int LoreLinkedGameCount { get; set; } = 0;
        public int PendingPostCreditsHype { get; set; } = 0;
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
                Description = "Oyunları aynı kurgusal evrene ve kronolojiye bağlar (+Skor & +Hype)."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_branching_narrative",
                Name = "Dallanan Hikaye & Çoklu Son",
                Description = "Oyuncu seçimlerine göre dallanan anlatı (+%6 Skor, +5 Hype)."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_soundtrack_rap",
                Name = "Stüdyo İçi Soundtrack & Rap",
                Description = "Özgün oyun müzikleri ve rap tanıtım parçaları (+15 Hype, +%3 Skor)."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_post_credits",
                Name = "Jenerik Sonrası Sahne",
                Description = "Bir sonraki oyuna +12 Hype aktarımı yapan gizli kapanış sahnesi."
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_mod_support",
                Name = "Resmi Mod Desteği",
                Description = "Topluluk modlarıyla ekstra hayran, +%4 Skor ve +8 Hype."
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
    // 2. JSON EVREN KAYIT SISTEMI
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
                        MelonLogger.Msg($"[Universe Architect] Evren kaydi yuklendi! ({Current.ReleasedGames.Count} oyun, Lore: {Current.LoreLinkedGameCount}, Bekleyen Post-Credits Hype: {Current.PendingPostCreditsHype})");
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

        public static void RecordReleasedGame(Game game, string topicId, double score, ProjectCategory category, List<string> modFeatures)
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
                UsedModFeatures = modFeatures ?? new List<string>(),
                RecordedAtUtc = DateTime.UtcNow.ToString("o")
            });
            Save();
        }
    }

    // =========================================================================
    // 3. GUVENLI REFLECTION & BILDIRIM YARDIMCILARI
    // =========================================================================
    public static class SafeReflection
    {
        public static object GetMember(object target, params string[] names)
        {
            if (target == null) return null;
            var t = target.GetType();
            foreach (var name in names)
            {
                var prop = AccessTools.Property(t, name);
                if (prop != null && prop.CanRead)
                {
                    try { return prop.GetValue(target); } catch { }
                }
                var field = AccessTools.Field(t, name);
                if (field != null)
                {
                    try { return field.GetValue(target); } catch { }
                }
            }
            return null;
        }

        public static bool TryGetNumericMember(object target, out double value, out string foundName, params string[] names)
        {
            value = 0;
            foundName = null;
            if (target == null) return false;
            var t = target.GetType();

            foreach (var name in names)
            {
                object raw = null;
                var prop = AccessTools.Property(t, name);
                if (prop != null && prop.CanRead)
                {
                    try { raw = prop.GetValue(target); } catch { }
                }
                if (raw == null)
                {
                    var field = AccessTools.Field(t, name);
                    if (field != null)
                    {
                        try { raw = field.GetValue(target); } catch { }
                    }
                }

                if (raw is double d) { value = d; foundName = name; return true; }
                if (raw is float f)  { value = f; foundName = name; return true; }
                if (raw is int i)    { value = i; foundName = name; return true; }
                if (raw is long l)   { value = l; foundName = name; return true; }
            }
            return false;
        }

        public static bool TrySetNumericMember(object target, string name, double newValue)
        {
            if (target == null || string.IsNullOrEmpty(name)) return false;
            var t = target.GetType();

            var prop = AccessTools.Property(t, name);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    object converted = ConvertNumeric(newValue, prop.PropertyType);
                    if (converted != null) { prop.SetValue(target, converted); return true; }
                }
                catch { }
            }

            var field = AccessTools.Field(t, name);
            if (field != null)
            {
                try
                {
                    object converted = ConvertNumeric(newValue, field.FieldType);
                    if (converted != null) { field.SetValue(target, converted); return true; }
                }
                catch { }
            }

            return false;
        }

        private static object ConvertNumeric(double val, Type targetType)
        {
            if (targetType == typeof(double)) return val;
            if (targetType == typeof(float))  return (float)val;
            if (targetType == typeof(int))    return (int)Math.Round(val);
            if (targetType == typeof(long))   return (long)Math.Round(val);
            return null;
        }

        // IL2CPP List<T> uzerinden guvenli ID okuma (Count + get_Item)
        public static void CollectIdsFromIl2CppList(object listObj, HashSet<string> output)
        {
            if (listObj == null) return;
            var lt = listObj.GetType();
            var countProp = AccessTools.Property(lt, "Count");
            var getItem = AccessTools.Method(lt, "get_Item", new[] { typeof(int) });
            if (countProp == null || getItem == null) return;

            int count = 0;
            try { count = (int)countProp.GetValue(listObj); } catch { return; }

            for (int i = 0; i < count; i++)
            {
                try
                {
                    var elem = getItem.Invoke(listObj, new object[] { i });
                    if (elem == null) continue;

                    if (elem is string s)
                    {
                        output.Add(s);
                    }
                    else
                    {
                        var idObj = GetMember(elem, "id", "Id", "name");
                        if (idObj is string idStr && !string.IsNullOrEmpty(idStr))
                            output.Add(idStr);
                    }
                }
                catch { }
            }
        }
    }

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
    // 4. 10X ARASTIRMA PUANI TURBO SISTEMI (KANCASIZ - TEST MODU)
    // =========================================================================
    public static class ResearchTurboSystem
    {
        private static double _lastRP = -1;
        private static string _rpMemberName = null;
        private static bool _starterBoostGiven = false;
        private static IntPtr _trackedCompanyPtr = IntPtr.Zero;

        public static void BindCompany(Company company)
        {
            if (!ModSettings.TestMode10xResearch || company == null) return;

            if (company.Pointer != _trackedCompanyPtr)
            {
                _trackedCompanyPtr = company.Pointer;
                _lastRP = -1;
                _starterBoostGiven = false;
            }

            if (!SafeReflection.TryGetNumericMember(company, out double currentRP, out string memberName,
                "researchPoints", "ResearchPoints", "_researchPoints"))
            {
                MelonLogger.Warning("[NJG-Turbo] Company uzerinde researchPoints alani bulunamadi.");
                return;
            }

            _rpMemberName = memberName;

            // Ilk baglantida test icin +100 Ar-Ge puani hediye et
            if (!_starterBoostGiven)
            {
                double boosted = currentRP + ModSettings.TestStarterResearchBoost;
                if (SafeReflection.TrySetNumericMember(company, _rpMemberName, boosted))
                {
                    _lastRP = boosted;
                    _starterBoostGiven = true;
                    MelonLogger.Msg($"[NJG-Turbo] 10x Arastirma Turbo aktif! (+{ModSettings.TestStarterResearchBoost} baslangic RP eklendi -> Toplam: {boosted:F0})");
                    NotificationHelper.Show(
                        company,
                        "10x Araştırma Turbo Aktif!",
                        $"Test modu devrede: +{ModSettings.TestStarterResearchBoost:F0} başlangıç Ar-Ge puanı eklendi ve bundan sonra kazanılan tüm araştırma puanları 10'a katlanacak!"
                    );
                }
            }
            else
            {
                _lastRP = currentRP;
            }
        }

        public static void PollAndMultiply(Company company)
        {
            if (!ModSettings.TestMode10xResearch || company == null || string.IsNullOrEmpty(_rpMemberName))
                return;

            if (!SafeReflection.TryGetNumericMember(company, out double currentRP, out _, _rpMemberName))
                return;

            if (_lastRP < 0)
            {
                _lastRP = currentRP;
                return;
            }

            // Harcama yapildiysa sadece yeni tabani kaydet
            if (currentRP < _lastRP)
            {
                _lastRP = currentRP;
                return;
            }

            double delta = currentRP - _lastRP;
            // Oyun ici dogal kazanc (+1 ila +200 arasi) yakalandiysa 9x daha ekleyip 10x'e tamamla
            if (delta >= 0.5 && delta <= 200.0)
            {
                double extra = delta * (ModSettings.ResearchMultiplier - 1.0);
                double newTotal = currentRP + extra;
                if (SafeReflection.TrySetNumericMember(company, _rpMemberName, newTotal))
                {
                    _lastRP = newTotal;
                }
            }
            else if (delta > 200.0)
            {
                _lastRP = currentRP;
            }
        }
    }

    // =========================================================================
    // 5. ANA MOD GIRISI
    // =========================================================================
    public class UniverseArchitectMod : MelonMod
    {
        public static ProjectCategory ActiveProjectCategory = ProjectCategory.UH;
        public static Company ActiveCompany = null;
        public static bool TopicsInjected = false;
        public static bool EngineFeaturesInjected = false;
        private float _injectTimer = 0f;
        private float _turboTimer = 0f;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Universe Architect v1.2.0 (5 Motor Mekanigi + 10x Arastirma Turbo + Offline Kilit) baslatiliyor...");
            UniverseSaveManager.Load();

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v120");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v1.2.0 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        public override void OnUpdate()
        {
            // 1) Konu ve Motor enjeksiyonu (2 sn aralikla, tamamlaninca durur)
            if (!TopicsInjected || !EngineFeaturesInjected)
            {
                _injectTimer += Time.unscaledDeltaTime;
                if (_injectTimer >= 2.0f)
                {
                    _injectTimer = 0f;
                    if (!TopicsInjected)
                        TopicInjectorHelper.TryInjectTopics();

                    if (!EngineFeaturesInjected)
                        EngineFeatureInjectorHelper.TryInjectFromScriptableObject();
                }
            }

            // 2) 10x Arastirma Puan Carpani (yarim saniyede bir kancasiz kontrol)
            if (ModSettings.TestMode10xResearch && ActiveCompany != null)
            {
                _turboTimer += Time.unscaledDeltaTime;
                if (_turboTimer >= 0.5f)
                {
                    _turboTimer = 0f;
                    try { ResearchTurboSystem.PollAndMultiply(ActiveCompany); }
                    catch { }
                }
            }
        }

        public static int ApplyCappedModHype(int rawModHype)
        {
            return Math.Min(rawModHype, BalanceCaps.MaxTotalModHype);
        }
    }

    // =========================================================================
    // 6. KONU ENJEKSIYONU (KANCASIZ)
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
    // 7. MOTOR / AR-GE ENJEKSIYONU (SIFIR NULLABLE + TryCast)
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
    // 8. 5 MOTOR OZELLIGI MEKANIKLERI + KONU BONUSLARI + JSON KAYIT
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

                // 10x Arastirma Turbo icin aktif sirketi kancasiz kilitle
                UniverseArchitectMod.ActiveCompany = company;
                ResearchTurboSystem.BindCompany(company);

                var game = company.currentGame;
                if (game.Pointer != IntPtr.Zero && !_processedGames.Add(game.Pointer))
                    return;

                string title = !string.IsNullOrEmpty(game.title) ? game.title : "Yeni Oyun";
                string topicId = game.topic != null ? game.topic.id : null;

                int totalModHype = 0;
                double totalScoreMult = 1.0;
                var activeBonusNotes = new List<string>();
                var usedModFeatures = new List<string>();

                // A) Onceki oyundan kalan Post-Credits Hype aktarimini uygula
                if (UniverseSaveManager.Current.PendingPostCreditsHype > 0)
                {
                    int carried = UniverseSaveManager.Current.PendingPostCreditsHype;
                    totalModHype += carried;
                    UniverseSaveManager.Current.PendingPostCreditsHype = 0;
                    activeBonusNotes.Add($"Jenerik Sonrası Miras (+{carried} Hype)");
                }

                // B) 6 Yeni Konu Pasif Bonuslari
                var customTopic = Phase1Database.FindTopicById(topicId);
                if (customTopic != null)
                {
                    if (customTopic.ScoreBonusMultiplier > 1.0)
                        totalScoreMult *= customTopic.ScoreBonusMultiplier;
                    if (customTopic.BonusHype > 0)
                        totalModHype += customTopic.BonusHype;

                    activeBonusNotes.Add($"{customTopic.Name}: {customTopic.BonusLabel}");
                }

                // C) 5 Yeni Motor Ozelliginin Gercek Oyun Ici Mekanikleri
                var featureIds = ExtractAllFeatureIds(game);

                if (featureIds.Contains("njg_lore_link"))
                {
                    usedModFeatures.Add("njg_lore_link");
                    UniverseSaveManager.Current.LoreLinkedGameCount++;
                    int depth = UniverseSaveManager.Current.LoreLinkedGameCount;

                    double loreScoreBonus = Math.Min(BalanceCaps.MaxLoreScoreBonus, depth * 0.02);
                    int loreHypeBonus = Math.Min(BalanceCaps.MaxLoreBaseHype, depth * 3);

                    totalScoreMult *= (1.0 + loreScoreBonus);
                    totalModHype += loreHypeBonus;
                    activeBonusNotes.Add($"Evren Bağlantısı [Derinlik {depth}] (+%{loreScoreBonus * 100:F0} Skor, +{loreHypeBonus} Hype)");
                }

                if (featureIds.Contains("njg_branching_narrative"))
                {
                    usedModFeatures.Add("njg_branching_narrative");
                    totalScoreMult *= 1.06;
                    totalModHype += 5;
                    activeBonusNotes.Add("Dallanan Hikaye (+%6 Skor, +5 Hype)");
                }

                if (featureIds.Contains("njg_soundtrack_rap"))
                {
                    usedModFeatures.Add("njg_soundtrack_rap");
                    totalScoreMult *= 1.03;
                    totalModHype += 15;
                    activeBonusNotes.Add("Soundtrack & Rap (+15 Hype, +%3 Skor)");
                }

                if (featureIds.Contains("njg_post_credits"))
                {
                    usedModFeatures.Add("njg_post_credits");
                    UniverseSaveManager.Current.PendingPostCreditsHype = 12;
                    activeBonusNotes.Add("Jenerik Sonrası Sahne (Sıradaki oyuna +12 Hype depolandı!)");
                }

                if (featureIds.Contains("njg_mod_support"))
                {
                    usedModFeatures.Add("njg_mod_support");
                    totalScoreMult *= 1.04;
                    totalModHype += 8;

                    // Hayran artisi (guvenli reflection ile)
                    if (SafeReflection.TryGetNumericMember(company, out double fans, out string fansField, "fans", "Fans"))
                    {
                        double bonusFans = Math.Max(50.0, Math.Round(fans * 0.05));
                        SafeReflection.TrySetNumericMember(company, fansField, fans + bonusFans);
                        activeBonusNotes.Add($"Resmi Mod Desteği (+%4 Skor, +8 Hype, +{bonusFans:F0} Hayran)");
                    }
                    else
                    {
                        activeBonusNotes.Add("Resmi Mod Desteği (+%4 Skor, +8 Hype)");
                    }
                }

                // D) UD (Baloncuk Evren) kategorisi bonusu
                if (UniverseArchitectMod.ActiveProjectCategory == ProjectCategory.UD)
                {
                    totalScoreMult *= 1.05;
                }

                // E) Global Denge Tavanlariyla Skor & Hype Uygula
                if (totalScoreMult > 1.0)
                {
                    game.score = Math.Min(10.0, game.score * totalScoreMult);
                }

                if (totalModHype > 0)
                {
                    int cappedHype = UniverseArchitectMod.ApplyCappedModHype(totalModHype);
                    company.adjustHype(cappedHype);
                }

                if (activeBonusNotes.Count > 0)
                {
                    NotificationHelper.Show(
                        company,
                        $"NJG Mod Bonusları: {title}",
                        string.Join(" | ", activeBonusNotes)
                    );
                }

                UniverseSaveManager.RecordReleasedGame(game, topicId, game.score, UniverseArchitectMod.ActiveProjectCategory, usedModFeatures);
                MelonLogger.Msg($"[Universe Architect] Oyun kaydedildi: '{title}' (Konu: {topicId ?? "standart"}, Motor Modlari: {usedModFeatures.Count}, Skor: {game.score:F2})");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] ReviewGame hatasi: {ex.Message}");
            }
        }

        private static HashSet<string> ExtractAllFeatureIds(Game game)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (game == null) return ids;

            // 1) Oyunun dogrudan kendi ozellik listeleri
            SafeReflection.CollectIdsFromIl2CppList(SafeReflection.GetMember(game, "features", "Features"), ids);

            // 2) Oyunun kullandigi ozel oyun motorunun parcalari
            var engineObj = SafeReflection.GetMember(game, "engine", "Engine");
            if (engineObj != null)
            {
                SafeReflection.CollectIdsFromIl2CppList(SafeReflection.GetMember(engineObj, "parts", "Parts", "features", "Features"), ids);
            }

            return ids;
        }
    }

    // =========================================================================
    // 9. GOOGLE PLAY GAMES OFFLINE YAMALARI (GUVENLIK KILIDI)
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