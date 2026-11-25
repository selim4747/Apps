using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime;
using Il2CppRarebyte.GDT;

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "2.0.0", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    // =========================================================================
    // 1. TEST AYARLARI, VERI MODELLERI VE DENGE TAVANLARI
    // =========================================================================
    public static class ModSettings
    {
        public const bool TestMode10xResearch = true;
        public const double ResearchMultiplier = 10.0;
        public const double TestStarterResearchBoost = 100.0;
    }

    public enum ProjectCategory
    {
        MainUniverse,   // Ana Evren
        SideUniverse,   // Yan Evren
        AltTimeline     // Alternatif Zaman
    }

    public static class CategoryLabels
    {
        public static string ToTurkish(ProjectCategory cat)
        {
            switch (cat)
            {
                case ProjectCategory.SideUniverse: return "Yan Evren";
                case ProjectCategory.AltTimeline:  return "Alternatif Zaman";
                default:                           return "Ana Evren";
            }
        }
    }

    public static class BalanceCaps
    {
        public const double MaxLoreScoreBonus = 0.08;
        public const int MaxLoreBaseHype = 15;
        public const int MaxTotalModHype = 35;
    }

    public class CustomTopicDef
    {
        public string Id;
        public string Name;
        public string IconTemplateKeyword;
        public Dictionary<string, double> GenreWeights;
        public double[] AudienceWeights;
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
        public int IntegrityAfter { get; set; }
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
                        Current.CanonIntegrity = Math.Clamp(Current.CanonIntegrity, 0, 100);
                        MelonLogger.Msg($"[Universe Architect] Evren kaydi yuklendi! ({Current.ReleasedGames.Count} oyun, Tutarlilik: {Current.CanonIntegrity}/100, Lore: {Current.LoreLinkedGameCount})");
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

        public static void AdjustIntegrity(int delta)
        {
            Current.CanonIntegrity = Math.Clamp(Current.CanonIntegrity + delta, 0, 100);
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
                Category = CategoryLabels.ToTurkish(category),
                IntegrityAfter = Current.CanonIntegrity,
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

        public static Il2CppSystem.Collections.Generic.List<ResearchItem> AsResearchList(object obj)
        {
            if (obj == null) return null;
            if (obj is Il2CppSystem.Collections.Generic.List<ResearchItem> direct) return direct;
            if (obj is Il2CppSystem.Object ilObj)
                return ilObj.TryCast<Il2CppSystem.Collections.Generic.List<ResearchItem>>();
            return null;
        }

        public static ResearchItem AsResearchItem(object obj)
        {
            if (obj == null) return null;
            if (obj is ResearchItem direct) return direct;
            if (obj is Il2CppSystem.Object ilObj)
                return ilObj.TryCast<ResearchItem>();
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
    // 4. FAZ 2: EVREN CEKIRDEGI 3 BUTONLU SECIM PENCERESI (KANCASIZ)
    // =========================================================================
    public static class UniverseChoicePopupSystem
    {
        private static readonly HashSet<IntPtr> _promptedGames = new HashSet<IntPtr>();
        private static readonly Dictionary<IntPtr, ProjectCategory> _gameCategories = new Dictionary<IntPtr, ProjectCategory>();

        public static ProjectCategory GetCategoryForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero && _gameCategories.TryGetValue(game.Pointer, out var cat))
                return cat;
            return UniverseArchitectMod.ActiveProjectCategory;
        }

        public static void PollForNewGame(Company company)
        {
            if (company == null || !company.isCurrentlyDevelopingGame() || company.currentGame == null)
                return;

            var game = company.currentGame;
            if (game.Pointer == IntPtr.Zero || _promptedGames.Contains(game.Pointer))
                return;

            try
            {
                var panel = UnityEngine.Object.FindObjectOfType<PanelNotifications>();
                if (panel == null) return;

                // Oyunun kendi acik bir penceresi varsa bitmesini bekle
                if (panel.IsNotificationVisible) return;

                _promptedGames.Add(game.Pointer);
                IntPtr capturedGamePtr = game.Pointer;
                string title = !string.IsNullOrEmpty(game.title) ? game.title : "Yeni Proje";
                int integrity = UniverseSaveManager.Current.CanonIntegrity;

                var notif = new Notification(
                    $"Evren Konumu: {title}",
                    $"Bu proje evreninin neresinde yer alacak?\n\n" +
                    $"• Ana Evren (Tutarlılık: {integrity}/100): Kanon bonusu ve devam oyunu gücü, kötü puan tutarlılığı düşürür.\n" +
                    $"• Yan Evren: Güvenli spin-off (+%5 Skor, tutarlılık etkilenmez).\n" +
                    $"• Alternatif Zaman: 'Ya farklı olsaydı?' (+10 Hype, tutarlılık etkilenmez)."
                );

                notif.buttonText = new Il2CppSystem.Collections.Generic.List<string>();
                notif.buttonText.Add("Ana Evren");
                notif.buttonText.Add("Yan Evren");
                notif.buttonText.Add("Alternatif Zaman");

                Action<Notification, Il2CppSystem.Action, int> onClickAction = (n, cb, selectedOption) =>
                {
                    try
                    {
                        ProjectCategory chosen = ProjectCategory.MainUniverse;
                        if (selectedOption == 1) chosen = ProjectCategory.SideUniverse;
                        else if (selectedOption == 2) chosen = ProjectCategory.AltTimeline;

                        UniverseArchitectMod.ActiveProjectCategory = chosen;
                        _gameCategories[capturedGamePtr] = chosen;

                        MelonLogger.Msg($"[Faz 2] '{title}' icin secilen evren konumu: {CategoryLabels.ToTurkish(chosen)} (Buton: {selectedOption})");

                        try { panel.Dismiss(n); } catch { }
                        try { cb?.Invoke(); } catch { }
                    }
                    catch (Exception innerEx)
                    {
                        MelonLogger.Warning($"[Faz 2] Secim callback hatasi: {innerEx.Message}");
                    }
                };

                var il2cppDelegate = DelegateSupport.ConvertDelegate<NotificationButton.NotificationButtonDelegate>(onClickAction);
                panel.ShowGenericNotification(notif, il2cppDelegate, null);
                MelonLogger.Msg($"[Faz 2] 3 butonlu Evren Secim penceresi acildi: '{title}'");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 2] Secim penceresi acilamadi (varsayilan Ana Evren ile devam): {ex.Message}");
            }
        }
    }

    // =========================================================================
    // 5. 10X ARASTIRMA PUANI TURBO SISTEMI (KANCASIZ - TEST MODU)
    // =========================================================================
    public static class ResearchTurboSystem
    {
        private static double _lastRP = -1;
        private static string _rpMemberName = null;
        private static bool _starterBoostGiven = false;
        private static IntPtr _trackedCompanyPtr = IntPtr.Zero;

        public static void BindCompany(Company company)
        {
            if (company == null) return;

            if (company.Pointer != _trackedCompanyPtr)
            {
                _trackedCompanyPtr = company.Pointer;
                _lastRP = -1;
                _starterBoostGiven = false;
            }

            EngineFeatureInjectorHelper.SyncWithCompany(company);

            if (!ModSettings.TestMode10xResearch) return;

            if (!SafeReflection.TryGetNumericMember(company, out double currentRP, out string memberName,
                "researchPoints", "ResearchPoints", "_researchPoints"))
            {
                return;
            }

            _rpMemberName = memberName;

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
                        "Faz 2 Evren Çekirdeği + 10x Turbo Hazır!",
                        $"Bundan sonraki her yeni oyununda Ana Evren / Yan Evren / Alternatif Zaman seçim penceresi açılacak! (+{ModSettings.TestStarterResearchBoost:F0} Ar-Ge puanı eklendi.)"
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

            if (currentRP < _lastRP)
            {
                _lastRP = currentRP;
                return;
            }

            double delta = currentRP - _lastRP;
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
    // 6. ANA MOD GIRISI
    // =========================================================================
    public class UniverseArchitectMod : MelonMod
    {
        public static ProjectCategory ActiveProjectCategory = ProjectCategory.MainUniverse;
        public static Company ActiveCompany = null;
        public static bool TopicsInjected = false;
        public static bool EngineFeaturesInjected = false;
        private float _injectTimer = 0f;
        private float _turboTimer = 0f;
        private float _companySyncTimer = 0f;
        private float _popupPollTimer = 0f;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Universe Architect v2.0.0 (Faz 2: Evren Cekirdegi + 3 Butonlu Secim + Tutarlilik) baslatiliyor...");
            UniverseSaveManager.Load();

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v200");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v2.0.0 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        public override void OnUpdate()
        {
            // 1) Konu ve Motor enjeksiyonu
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

            if (ActiveCompany == null) return;

            // 2) Faz 2: Yeni oyun basladiginda 3 butonlu Evren Secim penceresini ac (1 sn aralikla)
            _popupPollTimer += Time.unscaledDeltaTime;
            if (_popupPollTimer >= 1.0f)
            {
                _popupPollTimer = 0f;
                try { UniverseChoicePopupSystem.PollForNewGame(ActiveCompany); }
                catch { }
            }

            // 3) 10x Arastirma Puan Carpani
            if (ModSettings.TestMode10xResearch)
            {
                _turboTimer += Time.unscaledDeltaTime;
                if (_turboTimer >= 0.5f)
                {
                    _turboTimer = 0f;
                    try { ResearchTurboSystem.PollAndMultiply(ActiveCompany); }
                    catch { }
                }
            }

            // 4) Arastirilan 5 motor ozelliginin engineParts listesine senkronizasyonu
            if (EngineFeaturesInjected)
            {
                _companySyncTimer += Time.unscaledDeltaTime;
                if (_companySyncTimer >= 3.0f)
                {
                    _companySyncTimer = 0f;
                    try { EngineFeatureInjectorHelper.SyncWithCompany(ActiveCompany); }
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
    // 7. KONU ENJEKSIYONU (KANCASIZ)
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
    // 8. MOTOR / AR-GE ENJEKSIYONU (GERCEK allItems + Company.availableResearch)
    // =========================================================================
    public static class EngineFeatureInjectorHelper
    {
        public static readonly List<ResearchItem> CreatedItems = new List<ResearchItem>();
        private static bool _loggedCompanySync = false;

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

                var privateAllItems = SafeReflection.AsResearchList(SafeReflection.GetMember(research, "allItems"));
                var getterAllItems = research.getAllItems();
                var masterList = (privateAllItems != null && privateAllItems.Count > 0) ? privateAllItems : getterAllItems;

                if (masterList == null || masterList.Count == 0)
                    return;

                ResearchItem template = FindResearchableEngineTemplate(research, masterList);
                if (template == null) return;

                var cloneMethod = AccessTools.Method(typeof(Il2CppSystem.Object), "MemberwiseClone");
                if (cloneMethod == null)
                {
                    MelonLogger.Warning("[NJG-Debug] MemberwiseClone bulunamadi.");
                    UniverseArchitectMod.EngineFeaturesInjected = true;
                    return;
                }

                MelonLogger.Msg($"[NJG-Debug] Research hazir! allItems: {masterList.Count}, Sablon: {template.id} (Kategori: {template.category})");

                CreatedItems.Clear();
                int addedCount = 0;

                foreach (var feat in Phase1Database.EngineFeatures)
                {
                    var existing = FindById(masterList, feat.Id);
                    if (existing != null)
                    {
                        CreatedItems.Add(existing);
                        continue;
                    }

                    var rawClone = cloneMethod.Invoke(template, null) as Il2CppSystem.Object;
                    var item = rawClone != null ? rawClone.TryCast<ResearchItem>() : null;
                    if (item == null) continue;

                    item.id = feat.Id;
                    item.name = feat.Name;
                    item.description = feat.Description;

                    masterList.Add(item);
                    if (getterAllItems != null && getterAllItems.Pointer != masterList.Pointer && !ContainsId(getterAllItems, feat.Id))
                        getterAllItems.Add(item);

                    CreatedItems.Add(item);
                    addedCount++;
                }

                MelonLogger.Msg($"[Universe Architect] {addedCount} motor ozelligi '{template.id}' sablonuyla allItems'a eklendi!");
                UniverseArchitectMod.EngineFeaturesInjected = true;

                if (UniverseArchitectMod.ActiveCompany != null)
                    SyncWithCompany(UniverseArchitectMod.ActiveCompany);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] Kancasiz motor enjeksiyon hatasi: {ex}");
                UniverseArchitectMod.EngineFeaturesInjected = true;
            }
        }

        public static void SyncWithCompany(Company company)
        {
            if (company == null || CreatedItems.Count == 0) return;

            var avail = SafeReflection.AsResearchList(SafeReflection.GetMember(company, "availableResearch"));
            var completed = SafeReflection.AsResearchList(SafeReflection.GetMember(company, "researchCompleted"));
            var engineParts = SafeReflection.AsResearchList(SafeReflection.GetMember(company, "engineParts"));

            int addedToAvail = 0;
            int unlockedToEngine = 0;

            for (int i = 0; i < CreatedItems.Count; i++)
            {
                var item = CreatedItems[i];
                if (item == null) continue;

                bool isCompleted = ContainsId(completed, item.id);
                bool isEnginePart = ContainsId(engineParts, item.id);

                if (isCompleted && engineParts != null && !isEnginePart)
                {
                    engineParts.Add(item);
                    unlockedToEngine++;
                }

                if (!isCompleted && !isEnginePart && avail != null && !ContainsId(avail, item.id))
                {
                    avail.Add(item);
                    addedToAvail++;
                }
            }

            if ((addedToAvail > 0 || unlockedToEngine > 0) || !_loggedCompanySync)
            {
                MelonLogger.Msg($"[NJG-Research] Sirket senkronizasyonu -> Arastirma menusune eklenen: {addedToAvail}, Ozel Motora acilan: {unlockedToEngine}");
                _loggedCompanySync = true;
            }
        }

        private static ResearchItem FindResearchableEngineTemplate(Research research, Il2CppSystem.Collections.Generic.List<ResearchItem> allItems)
        {
            string[] preferredFields = { "BetterUI", "betterDialogs", "dialogueTree", "BetterUX", "StoryQuests" };
            foreach (var fName in preferredFields)
            {
                var candidate = SafeReflection.AsResearchItem(SafeReflection.GetMember(research, fName));
                if (candidate != null) return candidate;
            }

            for (int i = 0; i < allItems.Count; i++)
            {
                var c = allItems[i];
                if (c == null || string.IsNullOrEmpty(c.id)) continue;
                if (c.id.Equals("Savegame", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(c.category) &&
                    (c.category.Equals("Gameplay", StringComparison.OrdinalIgnoreCase) ||
                     c.category.Equals("Dialogs", StringComparison.OrdinalIgnoreCase) ||
                     c.category.Equals("Story/Quests", StringComparison.OrdinalIgnoreCase) ||
                     c.category.Equals("Engine", StringComparison.OrdinalIgnoreCase)))
                {
                    return c;
                }
            }

            return allItems[0];
        }

        private static ResearchItem FindById(Il2CppSystem.Collections.Generic.List<ResearchItem> list, string id)
        {
            if (list == null) return null;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].id == id) return list[i];
            }
            return null;
        }

        private static bool ContainsId(Il2CppSystem.Collections.Generic.List<ResearchItem> list, string id)
        {
            return FindById(list, id) != null;
        }
    }
    // =========================================================================
    // 9. INCELEME: FAZ 2 EVREN CEKIRDEGI + 5 MOTOR + 6 KONU + JSON KAYIT
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

                UniverseArchitectMod.ActiveCompany = company;
                ResearchTurboSystem.BindCompany(company);

                var game = company.currentGame;
                if (game.Pointer != IntPtr.Zero && !_processedGames.Add(game.Pointer))
                    return;

                string title = !string.IsNullOrEmpty(game.title) ? game.title : "Yeni Oyun";
                string topicId = game.topic != null ? game.topic.id : null;
                ProjectCategory category = UniverseChoicePopupSystem.GetCategoryForGame(game);

                int totalModHype = 0;
                double totalScoreMult = 1.0;
                var activeBonusNotes = new List<string>();
                var usedModFeatures = new List<string>();

                // A) Onceki oyundan kalan Post-Credits Hype aktarimi
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

                    double bonusFans = Math.Max(50.0, Math.Round(company.fans * 0.05));
                    company.adjustFans(bonusFans);
                    activeBonusNotes.Add($"Resmi Mod Desteği (+%4 Skor, +8 Hype, +{bonusFans:F0} Hayran)");
                }

                // D) FAZ 2: ANA EVREN / YAN EVREN / ALTERNATIF ZAMAN MEKANIKLERI
                int integrity = UniverseSaveManager.Current.CanonIntegrity;
                if (category == ProjectCategory.MainUniverse)
                {
                    if (integrity >= 80)
                    {
                        totalScoreMult *= 1.04;
                        totalModHype += 5;
                        activeBonusNotes.Add($"Ana Evren [Tutarlılık {integrity}/100] (+%4 Skor, +5 Hype)");
                    }
                    else if (integrity >= 50)
                    {
                        totalScoreMult *= 1.02;
                        totalModHype += 2;
                        activeBonusNotes.Add($"Ana Evren [Tutarlılık {integrity}/100] (+%2 Skor, +2 Hype)");
                    }
                    else
                    {
                        totalScoreMult *= 0.96;
                        activeBonusNotes.Add($"Ana Evren Çatlağı! [Tutarlılık {integrity}/100] (-%4 Skor)");
                    }

                    // Ana Evren devam oyunu bonusu
                    if (!string.IsNullOrEmpty(game.sequelTo))
                    {
                        totalScoreMult *= 1.03;
                        activeBonusNotes.Add("Ana Evren Devam Oyunu (+%3 Skor)");
                    }
                }
                else if (category == ProjectCategory.SideUniverse)
                {
                    totalScoreMult *= 1.05;
                    activeBonusNotes.Add("Yan Evren (+%5 Skor, Tutarlılık korundu)");
                }
                else if (category == ProjectCategory.AltTimeline)
                {
                    totalModHype += 10;
                    activeBonusNotes.Add("Alternatif Zaman (+10 Hype, Tutarlılık korundu)");
                }

                // E) Global Denge Tavanlariyla Skor & Hype Uygula
                if (totalScoreMult != 1.0)
                {
                    game.score = Math.Clamp(game.score * totalScoreMult, 1.0, 10.0);
                }

                if (totalModHype > 0)
                {
                    int cappedHype = UniverseArchitectMod.ApplyCappedModHype(totalModHype);
                    company.adjustHype(cappedHype);
                }

                // F) Ana Evren Tutarlilik Guncellemesi (Oyun puanina gore)
                if (category == ProjectCategory.MainUniverse)
                {
                    if (game.score >= 7.5)
                    {
                        UniverseSaveManager.AdjustIntegrity(+6);
                        activeBonusNotes.Add($"Kanon Güçlendi! (Tutarlılık -> {UniverseSaveManager.Current.CanonIntegrity}/100)");
                    }
                    else if (game.score < 5.5)
                    {
                        UniverseSaveManager.AdjustIntegrity(-12);
                        double lostFans = Math.Min(company.fans * 0.05, 500.0);
                        if (lostFans >= 10.0) company.adjustFans(-lostFans);
                        activeBonusNotes.Add($"Lore Hasarı! (Tutarlılık -> {UniverseSaveManager.Current.CanonIntegrity}/100)");
                    }
                }

                if (activeBonusNotes.Count > 0)
                {
                    NotificationHelper.Show(
                        company,
                        $"[{CategoryLabels.ToTurkish(category)}] {title}",
                        string.Join(" | ", activeBonusNotes)
                    );
                }

                UniverseSaveManager.RecordReleasedGame(game, topicId, game.score, category, usedModFeatures);
                MelonLogger.Msg($"[Universe Architect] Oyun kaydedildi: '{title}' | {CategoryLabels.ToTurkish(category)} | Tutarlilik: {UniverseSaveManager.Current.CanonIntegrity}/100 | Skor: {game.score:F2}");
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

            SafeReflection.CollectIdsFromIl2CppList(SafeReflection.GetMember(game, "features", "featuresInternal"), ids);
            SafeReflection.CollectIdsFromIl2CppList(SafeReflection.GetMember(game, "featureLog"), ids);

            var engineObj = SafeReflection.GetMember(game, "engine");
            if (engineObj != null)
            {
                SafeReflection.CollectIdsFromIl2CppList(SafeReflection.GetMember(engineObj, "partsIds"), ids);
                SafeReflection.CollectIdsFromIl2CppList(SafeReflection.GetMember(engineObj, "convertedParts"), ids);
            }

            return ids;
        }
    }

    // =========================================================================
    // 10. GOOGLE PLAY GAMES OFFLINE YAMALARI (GUVENLIK KILIDI)
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
