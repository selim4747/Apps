using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppRarebyte.GDT;

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "1.0.3", "NJG Games")]
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

    public enum GameScale
    {
        Small,
        Medium,
        Large,
        AAA
    }

    public static class BalanceCaps
    {
        public const int MaxTotalModHype = 35;
        public const float SoloBurnoutPenalty = 0.30f;
    }

    public class CustomTopicDef
    {
        public string Id;
        public string Name;
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
        public double ResearchCost;
        public double EngineCost;
        public double ResearchPoints;
        public int EnginePoints;
    }

    // JSON Evren Kayit Veri Modelleri (Faz 2 Temeli)
    public class ReleasedGameRecord
    {
        public string Title { get; set; }
        public string TopicId { get; set; }
        public double FinalScore { get; set; }
        public string Category { get; set; }
        public string RecordedAtUtc { get; set; }
    }

    public class UniverseSaveData
    {
        public string UniverseName { get; set; } = "Ana Evren";
        public int CanonIntegrity { get; set; } = 100;
        public float SoloBurnoutBar { get; set; } = 0f;
        public bool IsBurnedOut { get; set; } = false;
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
                ResearchCost = 20000, EngineCost = 10000, ResearchPoints = 15, EnginePoints = 15
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_branching_narrative",
                Name = "Dallanan Hikaye & Çoklu Son",
                ResearchCost = 25000, EngineCost = 15000, ResearchPoints = 20, EnginePoints = 20
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_soundtrack_rap",
                Name = "Stüdyo İçi Soundtrack & Rap",
                ResearchCost = 15000, EngineCost = 8000, ResearchPoints = 10, EnginePoints = 10
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_post_credits",
                Name = "Jenerik Sonrası Sahne",
                ResearchCost = 18000, EngineCost = 10000, ResearchPoints = 12, EnginePoints = 12
            },
            new CustomEngineFeatureDef
            {
                Id = "njg_mod_support",
                Name = "Resmi Mod Desteği",
                ResearchCost = 30000, EngineCost = 20000, ResearchPoints = 20, EnginePoints = 20
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
    // 2. JSON EVREN KAYIT SISTEMI (FAZ 2 TEMELI)
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
                        SoloDevSystem.RestoreState(Current.SoloBurnoutBar, Current.IsBurnedOut);
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
                Current.SoloBurnoutBar = SoloDevSystem.BurnoutBar;
                Current.IsBurnedOut = SoloDevSystem.IsBurnedOut;

                var opts = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Current, opts);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] JSON kaydetme hatasi: {ex.Message}");
            }
        }

        public static void RecordReleasedGame(string title, string topicId, double score, ProjectCategory category)
        {
            Current.ReleasedGames.Add(new ReleasedGameRecord
            {
                Title = string.IsNullOrEmpty(title) ? "Adsiz Oyun" : title,
                TopicId = string.IsNullOrEmpty(topicId) ? "unknown" : topicId,
                FinalScore = Math.Round(score, 2),
                Category = category.ToString(),
                RecordedAtUtc = DateTime.UtcNow.ToString("o")
            });
            Save();
        }
    }

    // =========================================================================
    // 3. COKMESIZ SOLO-DEV & TUKENMISLIK SISTEMI
    // =========================================================================
    public static class SoloDevSystem
    {
        public static float BurnoutBar { get; private set; } = 0f;
        public static bool IsBurnedOut { get; private set; } = false;
        private static int _lastObservedWeek = -1;
        private static bool _notifiedBurnoutOnce = false;

        public static void RestoreState(float bar, bool burnedOut)
        {
            BurnoutBar = Mathf.Clamp(bar, 0f, 100f);
            IsBurnedOut = burnedOut;
        }

        public static void PollCompanyWeek(Company company)
        {
            if (company == null) return;

            double rawWeek = SafeReflection.GetDoubleMember(company, "currentWeek", "week");
            if (rawWeek <= 0) return;

            int currentWeekInt = (int)Math.Floor(rawWeek);
            if (_lastObservedWeek == -1)
            {
                _lastObservedWeek = currentWeekInt;
                return;
            }

            if (currentWeekInt <= _lastObservedWeek) return;
            _lastObservedWeek = currentWeekInt;

            if (!company.isCurrentlyDevelopingGame() || company.currentGame == null)
                return;

            int staffCount = company.staff != null ? company.staff.Count : 1;
            if (staffCount > 1)
            {
                if (BurnoutBar > 0f || IsBurnedOut)
                {
                    ResetBurnout();
                    UniverseSaveManager.Save();
                }
                return;
            }

            string sizeStr = company.currentGame.gameSize ?? "small";
            GameScale scale = ParseScale(sizeStr);
            if (scale == GameScale.Small || scale == GameScale.Medium)
                return;

            float gain = (scale == GameScale.Large) ? 6f : 12f;
            BurnoutBar = Mathf.Clamp(BurnoutBar + gain, 0f, 100f);

            if (BurnoutBar >= 100f && !IsBurnedOut)
            {
                IsBurnedOut = true;
                UniverseSaveManager.Save();
                if (!_notifiedBurnoutOnce)
                {
                    _notifiedBurnoutOnce = true;
                    NotificationHelper.TryShow(
                        company,
                        "Tükenmişlik Uyarısı!",
                        "Tek başına büyük ölçekli oyun geliştirirken yoruldun. Yeni çalışan alana kadar geliştirme hızı %30 düştü."
                    );
                }
            }
        }

        public static void ResetBurnout()
        {
            BurnoutBar = 0f;
            IsBurnedOut = false;
            _notifiedBurnoutOnce = false;
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

    // =========================================================================
    // 4. OYUN ICI BILDIRIM (POP-UP) & GUVENLI REFLECTION YARDIMCILARI
    // =========================================================================
    public static class NotificationHelper
    {
        public static void TryShow(Company company, string header, string text)
        {
            MelonLogger.Msg($"[Universe Architect Bildirim] {header}: {text}");
            if (company == null) return;

            try
            {
                var notifType = AccessTools.TypeByName("Il2CppRarebyte.GDT.Notification");
                if (notifType == null) return;

                object notifObj = null;
                var ctor2 = AccessTools.Constructor(notifType, new[] { typeof(string), typeof(string) });
                if (ctor2 != null)
                {
                    notifObj = ctor2.Invoke(new object[] { header, text });
                }
                else
                {
                    var ctor0 = AccessTools.Constructor(notifType, Type.EmptyTypes);
                    if (ctor0 != null)
                    {
                        notifObj = ctor0.Invoke(null);
                        SafeReflection.SetMember(notifObj, "header", header);
                        SafeReflection.SetMember(notifObj, "text", text);
                    }
                }

                if (notifObj == null) return;

                var listObj = SafeReflection.GetObjectMember(company, "notifications");
                if (listObj != null)
                {
                    var addMethod = AccessTools.Method(listObj.GetType(), "Add");
                    addMethod?.Invoke(listObj, new[] { notifObj });
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] Oyun ici bildirim gonderilemedi (logla devam): {ex.Message}");
            }
        }
    }

    public static class SafeReflection
    {
        public static object GetObjectMember(object target, params string[] names)
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

        public static string GetStringMember(object target, params string[] names)
        {
            return GetObjectMember(target, names) as string;
        }

        public static double GetDoubleMember(object target, params string[] names)
        {
            var val = GetObjectMember(target, names);
            if (val is double d) return d;
            if (val is float f) return f;
            if (val is int i) return i;
            return -1;
        }

        public static void SetMember(object target, string name, object value)
        {
            if (target == null) return;
            var t = target.GetType();
            var prop = AccessTools.Property(t, name);
            if (prop != null && prop.CanWrite)
            {
                try { prop.SetValue(target, value); return; } catch { }
            }
            var field = AccessTools.Field(t, name);
            if (field != null)
            {
                try { field.SetValue(target, value); } catch { }
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
        public static bool LoggedResearchOnce = false;
        private float _pollTimer = 0f;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Universe Architect v1.0.3 (Konular + Motorlar + Bonuslar + SoloDev + JSON) baslatiliyor...");
            UniverseSaveManager.Load();

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v103");
                harmony.PatchAll();
                LoggerInstance.Msg("[Universe Architect] Tum v1.0.3 yamalari basariyla uygulandi!");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        public override void OnUpdate()
        {
            _pollTimer += Time.unscaledDeltaTime;
            if (_pollTimer < 1.5f) return;
            _pollTimer = 0f;

            if (!TopicsInjected)
                TopicInjectorHelper.TryInjectTopics();

            // Aktif sirketi bulup Solo-Dev haftalik kontrolunu kancasiz yap
            if (ActiveCompany == null)
                TryFindActiveCompany();

            if (ActiveCompany != null)
            {
                try { SoloDevSystem.PollCompanyWeek(ActiveCompany); }
                catch { }
            }
        }

        private static void TryFindActiveCompany()
        {
            try
            {
                var controllers = UnityEngine.Resources.FindObjectsOfTypeAll<GameController>();
                if (controllers == null || controllers.Length == 0) return;

                var compObj = SafeReflection.GetObjectMember(controllers[0], "company", "Company", "_company");
                if (compObj is Company c)
                {
                    ActiveCompany = c;
                }
            }
            catch { }
        }

        public static int ApplyCappedModHype(int rawModHype)
        {
            return Math.Min(rawModHype, BalanceCaps.MaxTotalModHype);
        }
    }

    // =========================================================================
    // 6. KONU ENJEKSIYONU (SABLON KOPYALAMA ILE GUVENLI)
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
                MelonLogger.Msg($"[NJG-Debug] Topics bulundu! Mevcut: {topicsAsset.topics.Count}, Sablon: {templateTopic.id}");

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
    // 7. MOTOR / AR-GE OZELLIKLERI ENJEKSIYONU
    // =========================================================================
    [HarmonyPatch(typeof(Research), nameof(Research.getAllItems))]
    public static class Patch_InjectEngineFeatures
    {
        private static readonly string[] SafeTemplateProps =
        {
            "categoryDisplayName",
            "v",
            "techLevel",
            "canResearch",
            "canUse",
            "group",
            "complete",
            "isTraining"
        };

        static void Postfix(ref Il2CppSystem.Collections.Generic.List<ResearchItem> __result)
        {
            try
            {
                if (__result == null || __result.Count == 0) return;

                ResearchItem template = null;
                for (int i = 0; i < __result.Count; i++)
                {
                    var candidate = __result[i];
                    if (candidate != null &&
                        !string.IsNullOrEmpty(candidate.category) &&
                        candidate.category.Equals("Engine", StringComparison.OrdinalIgnoreCase))
                    {
                        template = candidate;
                        break;
                    }
                }
                if (template == null) template = __result[0];

                int addedCount = 0;
                foreach (var feat in Phase1Database.EngineFeatures)
                {
                    bool exists = false;
                    for (int i = 0; i < __result.Count; i++)
                    {
                        if (__result[i] != null && __result[i].id == feat.Id)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists) continue;

                    var item = new ResearchItem();
                    foreach (var propName in SafeTemplateProps)
                    {
                        CopyPropertyIfExists(template, item, propName);
                    }

                    item.id = feat.Id;
                    item.category = template.category ?? "Engine";
                    SafeReflection.SetMember(item, "name", feat.Name);
                    item.cost = new Il2CppSystem.Nullable<double>(feat.ResearchCost);
                    item.engineCost = new Il2CppSystem.Nullable<double>(feat.EngineCost);
                    item.pointsCost = new Il2CppSystem.Nullable<double>(feat.ResearchPoints);
                    item.enginePoints = new Il2CppSystem.Nullable<int>(feat.EnginePoints);

                    __result.Add(item);
                    addedCount++;
                }

                if (!UniverseArchitectMod.LoggedResearchOnce && addedCount > 0)
                {
                    MelonLogger.Msg($"[Universe Architect] {addedCount} motor ozelligi eklendi! (Sablon: {template.id} / {template.category})");
                    UniverseArchitectMod.LoggedResearchOnce = true;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] Motor ozelligi enjeksiyon hatasi: {ex}");
            }
        }

        private static void CopyPropertyIfExists(ResearchItem src, ResearchItem dst, string propName)
        {
            var prop = AccessTools.Property(typeof(ResearchItem), propName);
            if (prop != null && prop.CanRead && prop.CanWrite)
            {
                try { prop.SetValue(dst, prop.GetValue(src)); } catch { }
            }
        }
    }

    // =========================================================================
    // 8. SOLO-DEV HIZ YAMASI & INCELEME PASIF BONUSLARI
    // =========================================================================
    [HarmonyPatch(typeof(General), nameof(General.getGameSizeDurationFactor))]
    public static class Patch_SoloDevSpeed
    {
        static void Postfix(string gameSize, ref double __result)
        {
            if (SoloDevSystem.IsBurnedOut)
            {
                __result *= (1.0 + BalanceCaps.SoloBurnoutPenalty);
            }
        }
    }

    [HarmonyPatch(typeof(Reviews), nameof(Reviews.reviewGame))]
    public static class Patch_ReviewGame
    {
        static void Postfix(Company company)
        {
            try
            {
                if (company == null || company.currentGame == null) return;
                UniverseArchitectMod.ActiveCompany = company;

                var game = company.currentGame;
                string title = SafeReflection.GetStringMember(game, "title", "name") ?? "Yeni Oyun";

                // Konu ID'sini guvenli reflection ile oku
                string topicId = null;
                var topicObj = SafeReflection.GetObjectMember(game, "topic");
                if (topicObj != null)
                    topicId = SafeReflection.GetStringMember(topicObj, "id");
                if (string.IsNullOrEmpty(topicId))
                    topicId = SafeReflection.GetStringMember(game, "topicId");

                // 6 yeni konunun ozel pasif bonuslarini uygula
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

                    NotificationHelper.TryShow(
                        company,
                        $"Konu Bonusu: {customTopic.Name}",
                        $"{title} için özel bonus devreye girdi: {customTopic.BonusLabel}"
                    );
                }

                // UD (Baloncuk Evren) bonusu
                if (UniverseArchitectMod.ActiveProjectCategory == ProjectCategory.UD)
                {
                    game.score = Math.Min(10.0, game.score * 1.05);
                }

                // Cikan oyunu JSON evren kaydina ekle
                UniverseSaveManager.RecordReleasedGame(title, topicId, game.score, UniverseArchitectMod.ActiveProjectCategory);
                MelonLogger.Msg($"[Universe Architect] Oyun kaydedildi: '{title}' (Konu: {topicId ?? "standart"}, Skor: {game.score:F2})");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] ReviewGame hatasi: {ex.Message}");
            }
        }
    }

    // =========================================================================
    // 9. GOOGLE PLAY GAMES OFFLINE YAMALARI
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