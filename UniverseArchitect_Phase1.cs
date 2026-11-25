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

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "3.0.0", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    // =========================================================================
    // 1. TEST AYARLARI, VERI MODELLERI, 10 ARKETIP & 3x3 KAYDIRICI VERITABANI
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
        AltTimeline,    // Alternatif Zaman
        NonUniverse     // Evren Disi (Bagimsiz)
    }

    public enum UniverseTone
    {
        None,
        DarkMystery,    // Karanlik & Gizem
        EpicAdventure,  // Epik Macera
        SciFiTactical,  // Bilim Kurgu & Taktik
        LightComedy     // Hafif & Mizahi
    }

    public static class CategoryLabels
    {
        public static string ToTurkish(ProjectCategory cat)
        {
            switch (cat)
            {
                case ProjectCategory.SideUniverse: return "Yan Evren";
                case ProjectCategory.AltTimeline:  return "Alternatif Zaman";
                case ProjectCategory.NonUniverse:  return "Evren Dışı";
                default:                           return "Ana Evren";
            }
        }

        public static string ToneToTurkish(UniverseTone tone)
        {
            switch (tone)
            {
                case UniverseTone.DarkMystery:   return "Karanlık & Gizem";
                case UniverseTone.EpicAdventure: return "Epik Macera";
                case UniverseTone.SciFiTactical: return "Bilim Kurgu & Taktik";
                case UniverseTone.LightComedy:   return "Hafif & Mizahi";
                default:                         return "Belirlenmedi";
            }
        }
    }

    public static class BalanceCaps
    {
        public const double MaxLoreScoreBonus = 0.08;
        public const int MaxLoreBaseHype = 15;
        public const int MaxTotalModHype = 35;
        public const float StageSliderBudget = 1.80f; // Her 3'lu asamada max %180 odak butcesi
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

    public class CharacterArchetypeDef
    {
        public string Id;
        public string Name;
        public string ShortDesc;
        public UniverseTone PreferredTone;
        public UniverseTone ClashingTone;
        public Dictionary<string, double> GenreWeights;
        // 9 ideal kaydirici degeri (0.0 - 1.0):
        // [0] Kavgacilik, [1] Sanatsal Diyalog, [2] Zeka & Gizem
        // [3] Travma & Karanlik, [4] Icini Acma, [5] Mizah & Enerji
        // [6] Cevre Sevgisi, [7] Egoizm, [8] Icine Kapaniklik
        public float[] IdealSliders;
    }

    public class CharacterRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ArchetypeId { get; set; }
        public string ArchetypeName { get; set; }
        public float[] Sliders { get; set; } = new float[9];
        public string DebutGame { get; set; }
        public string Status { get; set; } = "Normal"; // Yildiz, Sevilen, Normal, Fiyasko
        public int Popularity { get; set; } = 50;
        public int GamesAppeared { get; set; } = 0;
        public int ConsecutiveUses { get; set; } = 0;
    }

    public class ActiveCharacterDraft
    {
        public bool IsReturning;
        public string ExistingCharacterId;
        public string Name;
        public string ArchetypeId;
        public float[] Sliders = new float[9];
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
        public string CharacterName { get; set; }
        public List<string> UsedModFeatures { get; set; } = new List<string>();
        public string RecordedAtUtc { get; set; }
    }

    public class UniverseSaveData
    {
        public string UniverseName { get; set; } = "Ana Evren";
        public string UniverseToneName { get; set; } = "None";
        public int CanonIntegrity { get; set; } = 100;
        public int LoreBarrier { get; set; } = 0;
        public int ConsecutiveSideProjects { get; set; } = 0;
        public int LoreLinkedGameCount { get; set; } = 0;
        public int PendingPostCreditsHype { get; set; } = 0;
        public List<CharacterRecord> Characters { get; set; } = new List<CharacterRecord>();
        public List<ReleasedGameRecord> ReleasedGames { get; set; } = new List<ReleasedGameRecord>();
    }

    public static class Phase1Database
    {
        public static readonly string[] SliderNames = new[]
        {
            "Kavgacılık",
            "Sanatsal Diyalog",
            "Zeka & Gizem",
            "Travma & Karanlık",
            "İçini Açma",
            "Mizah & Enerji",
            "Çevre Sevgisi",
            "Egoizm",
            "İçine Kapanıklık"
        };

        public static readonly string[] SliderHints = new[]
        {
            "Çatışma ve aksiyon eğilimi",
            "Edebi, şiirsel ve derin replikler (çok konuşma değil)",
            "Analitik düşünce ve gizemli duruş",
            "Geçmiş acılar ve karanlık atmosfer",
            "Duygusal samimiyet ve kırılganlık",
            "Esprili dil ve sahne enerjisi",
            "Yaşadığı dünyaya ve mekana bağlılık",
            "Bencillik ve kendini merkeze alma",
            "Yalnızlık tercihi ve mesafeli tutum"
        };

        public static readonly List<CharacterArchetypeDef> Archetypes = new List<CharacterArchetypeDef>
        {
            new CharacterArchetypeDef
            {
                Id = "classic_hero", Name = "Klasik Kahraman", ShortDesc = "Cesur, lider, umut veren",
                PreferredTone = UniverseTone.EpicAdventure, ClashingTone = UniverseTone.DarkMystery,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.6 }, { "Strategy", 0.7 }, { "Casual", 0.8 } },
                IdealSliders = new[] { 0.70f, 0.55f, 0.45f, 0.30f, 0.75f, 0.65f, 0.85f, 0.20f, 0.25f }
            },
            new CharacterArchetypeDef
            {
                Id = "dark_antihero", Name = "Karanlık Anti-Kahraman", ShortDesc = "Gri ahlaklı, sert, yaralı",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 0.9 }, { "RPG", 1.0 }, { "Simulation", 0.5 }, { "Strategy", 0.8 }, { "Casual", 0.4 } },
                IdealSliders = new[] { 0.80f, 0.55f, 0.45f, 0.90f, 0.35f, 0.20f, 0.30f, 0.65f, 0.80f }
            },
            new CharacterArchetypeDef
            {
                Id = "mystery_strategist", Name = "Gizemli Stratejist", ShortDesc = "Zeki, soğukkanlı, hesapçı",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.6 }, { "Adventure", 0.9 }, { "RPG", 0.9 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.5 } },
                IdealSliders = new[] { 0.20f, 0.70f, 0.90f, 0.50f, 0.30f, 0.25f, 0.35f, 0.65f, 0.75f }
            },
            new CharacterArchetypeDef
            {
                Id = "cheerful_mascot", Name = "Neşeli Maskot", ShortDesc = "Enerjik, esprili, sıcakkanlı",
                PreferredTone = UniverseTone.LightComedy, ClashingTone = UniverseTone.DarkMystery,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.7 }, { "Adventure", 0.8 }, { "RPG", 0.6 }, { "Simulation", 0.8 }, { "Strategy", 0.4 }, { "Casual", 1.0 } },
                IdealSliders = new[] { 0.30f, 0.45f, 0.35f, 0.10f, 0.75f, 0.95f, 0.85f, 0.35f, 0.15f }
            },
            new CharacterArchetypeDef
            {
                Id = "wise_mentor", Name = "Bilge Mentor", ShortDesc = "Yol gösterici, derin, tecrübeli",
                PreferredTone = UniverseTone.EpicAdventure, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.9 }, { "Casual", 0.6 } },
                IdealSliders = new[] { 0.20f, 0.85f, 0.75f, 0.50f, 0.75f, 0.35f, 0.85f, 0.15f, 0.45f }
            },
            new CharacterArchetypeDef
            {
                Id = "charismatic_villain", Name = "Karizmatik Kötü", ShortDesc = "Akılda kalıcı, iddialı antagonist",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.9 }, { "Adventure", 0.9 }, { "RPG", 1.0 }, { "Simulation", 0.5 }, { "Strategy", 1.0 }, { "Casual", 0.4 } },
                IdealSliders = new[] { 0.60f, 0.80f, 0.70f, 0.70f, 0.25f, 0.45f, 0.20f, 0.95f, 0.45f }
            },
            new CharacterArchetypeDef
            {
                Id = "rebel_outlaw", Name = "Asi Kaçak", ShortDesc = "Kuralsız, özgür ruhlu, sivri dilli",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.None,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.6 }, { "Strategy", 0.7 }, { "Casual", 0.7 } },
                IdealSliders = new[] { 0.75f, 0.50f, 0.50f, 0.55f, 0.45f, 0.65f, 0.50f, 0.70f, 0.45f }
            },
            new CharacterArchetypeDef
            {
                Id = "tragic_survivor", Name = "Trajik Hayatta Kalan", ShortDesc = "Duygusal derinlik, dram yüklü",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.6 }, { "Casual", 0.4 } },
                IdealSliders = new[] { 0.40f, 0.75f, 0.55f, 0.90f, 0.75f, 0.15f, 0.75f, 0.15f, 0.70f }
            },
            new CharacterArchetypeDef
            {
                Id = "cold_hunter", Name = "Soğukkanlı Avcı", ShortDesc = "Az konuşan, yüksek aksiyon",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.8 }, { "Simulation", 0.6 }, { "Strategy", 0.9 }, { "Casual", 0.5 } },
                IdealSliders = new[] { 0.90f, 0.35f, 0.55f, 0.60f, 0.15f, 0.15f, 0.35f, 0.45f, 0.90f }
            },
            new CharacterArchetypeDef
            {
                Id = "chaotic_trickster", Name = "Kaotik Düzenbaz", ShortDesc = "Öngörülemez, sürprizli, kurnaz",
                PreferredTone = UniverseTone.LightComedy, ClashingTone = UniverseTone.None,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.6 }, { "Strategy", 0.8 }, { "Casual", 0.9 } },
                IdealSliders = new[] { 0.40f, 0.65f, 0.75f, 0.35f, 0.35f, 0.90f, 0.30f, 0.85f, 0.40f }
            }
        };

        public static CharacterArchetypeDef FindArchetype(string id)
        {
            if (string.IsNullOrEmpty(id)) return Archetypes[0];
            for (int i = 0; i < Archetypes.Count; i++)
            {
                if (string.Equals(Archetypes[i].Id, id, StringComparison.OrdinalIgnoreCase))
                    return Archetypes[i];
            }
            return Archetypes[0];
        }

        public static readonly List<CustomTopicDef> Topics = new List<CustomTopicDef>
        {
            new CustomTopicDef
            {
                Id = "njg_library_defense", Name = "Kütüphane & Gece Savunması", IconTemplateKeyword = "Horror",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.8 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.9, 0.7, 1.0 }, ScoreBonusMultiplier = 1.04, BonusHype = 4,
                BonusLabel = "Gündüz/Gece Döngüsü (+%4 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_5d_shift", Name = "5D Boyut Kayması", IconTemplateKeyword = "Sci-Fi",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 0.8 }, { "Simulation", 0.8 }, { "Strategy", 0.9 }, { "Casual", 0.9 } },
                AudienceWeights = new[] { 0.6, 0.7, 1.0 }, ScoreBonusMultiplier = 1.06, BonusHype = 0,
                BonusLabel = "Eleştirmen Sanatı (+%6 Skor)"
            },
            new CustomTopicDef
            {
                Id = "njg_planet_idle", Name = "Gezegen Kolonizasyonu (Idle)", IconTemplateKeyword = "Space",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.6 }, { "Adventure", 0.7 }, { "RPG", 0.7 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.9 } },
                AudienceWeights = new[] { 0.8, 1.0, 0.8 }, ScoreBonusMultiplier = 1.03, BonusHype = 5,
                BonusLabel = "Uzun Soluklu Koloni (+%3 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_morph_animals", Name = "Şekil Değiştiren Hayvanlar", IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 1.0, 1.0, 0.7 }, ScoreBonusMultiplier = 1.02, BonusHype = 8,
                BonusLabel = "Karakter Popülaritesi (+%2 Skor, +8 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_star_run", Name = "Yıldız Koşusu", IconTemplateKeyword = "Racing",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 0.8 }, { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 1.0 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }, ScoreBonusMultiplier = 1.03, BonusHype = 6,
                BonusLabel = "Hızlı Tempo (+%3 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_sailor_adventure", Name = "Denizci Macerası", IconTemplateKeyword = "Pirate",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }, ScoreBonusMultiplier = 1.05, BonusHype = 4,
                BonusLabel = "Açık Deniz Tasarımı (+%5 Skor, +4 Hype)"
            }
        };

        public static readonly List<CustomEngineFeatureDef> EngineFeatures = new List<CustomEngineFeatureDef>
        {
            new CustomEngineFeatureDef { Id = "njg_lore_link", Name = "Evren Bağlantı Modülü", Description = "Oyunları aynı kurgusal evrene ve kronolojiye bağlar (+Skor & +Hype)." },
            new CustomEngineFeatureDef { Id = "njg_branching_narrative", Name = "Dallanan Hikaye & Çoklu Son", Description = "Oyuncu seçimlerine göre dallanan anlatı (+%6 Skor, +5 Hype)." },
            new CustomEngineFeatureDef { Id = "njg_soundtrack_rap", Name = "Stüdyo İçi Soundtrack & Rap", Description = "Özgün oyun müzikleri ve rap tanıtım parçaları (+15 Hype, +%3 Skor)." },
            new CustomEngineFeatureDef { Id = "njg_post_credits", Name = "Jenerik Sonrası Sahne", Description = "Bir sonraki oyuna +12 Hype aktarımı yapan gizli kapanış sahnesi." },
            new CustomEngineFeatureDef { Id = "njg_mod_support", Name = "Resmi Mod Desteği", Description = "Topluluk modlarıyla ekstra hayran, +%4 Skor ve +8 Hype." }
        };

        public static CustomTopicDef FindTopicById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Topics.Count; i++)
                if (string.Equals(Topics[i].Id, id, StringComparison.OrdinalIgnoreCase)) return Topics[i];
            return null;
        }
    }

    // =========================================================================
    // 2. JSON EVREN & KARAKTER KAYIT SISTEMI
    // =========================================================================
    public static class UniverseSaveManager
    {
        public static UniverseSaveData Current { get; private set; } = new UniverseSaveData();
        private static string SavePath => Path.Combine(Application.persistentDataPath, "UniverseArchitect_Save.json");

        public static UniverseTone GetTone()
        {
            if (Enum.TryParse<UniverseTone>(Current.UniverseToneName, out var tone)) return tone;
            return UniverseTone.None;
        }

        public static void SetTone(UniverseTone tone)
        {
            Current.UniverseToneName = tone.ToString();
            Save();
        }

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
                        if (Current.Characters == null) Current.Characters = new List<CharacterRecord>();
                        if (Current.ReleasedGames == null) Current.ReleasedGames = new List<ReleasedGameRecord>();
                        Current.CanonIntegrity = Math.Clamp(Current.CanonIntegrity, 0, 100);
                        Current.LoreBarrier = Math.Clamp(Current.LoreBarrier, 0, 100);
                        MelonLogger.Msg($"[Universe Architect] Evren kaydi yuklendi! ({Current.ReleasedGames.Count} oyun, {Current.Characters.Count} karakter, Tutarlilik: {Current.CanonIntegrity}/100)");
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
                File.WriteAllText(SavePath, JsonSerializer.Serialize(Current, opts));
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

        public static void AdjustLoreBarrier(int delta)
        {
            Current.LoreBarrier = Math.Clamp(Current.LoreBarrier + delta, 0, 100);
        }

        public static CharacterRecord FindCharacterById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Current.Characters.Count; i++)
                if (Current.Characters[i].Id == id) return Current.Characters[i];
            return null;
        }

        public static void RestOtherCharacters(string usedCharId)
        {
            for (int i = 0; i < Current.Characters.Count; i++)
            {
                if (Current.Characters[i].Id != usedCharId)
                    Current.Characters[i].ConsecutiveUses = 0;
            }
        }

        public static void RecordReleasedGame(Game game, string topicId, double score, ProjectCategory category, List<string> modFeatures, string charName)
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
                CharacterName = charName ?? "",
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
                if (prop != null && prop.CanRead) { try { return prop.GetValue(target); } catch { } }
                var field = AccessTools.Field(t, name);
                if (field != null) { try { return field.GetValue(target); } catch { } }
            }
            return null;
        }

        public static Il2CppSystem.Collections.Generic.List<ResearchItem> AsResearchList(object obj)
        {
            if (obj == null) return null;
            if (obj is Il2CppSystem.Collections.Generic.List<ResearchItem> direct) return direct;
            if (obj is Il2CppSystem.Object ilObj) return ilObj.TryCast<Il2CppSystem.Collections.Generic.List<ResearchItem>>();
            return null;
        }

        public static ResearchItem AsResearchItem(object obj)
        {
            if (obj == null) return null;
            if (obj is ResearchItem direct) return direct;
            if (obj is Il2CppSystem.Object ilObj) return ilObj.TryCast<ResearchItem>();
            return null;
        }

        public static bool TryGetNumericMember(object target, out double value, out string foundName, params string[] names)
        {
            value = 0; foundName = null;
            if (target == null) return false;
            var t = target.GetType();
            foreach (var name in names)
            {
                object raw = null;
                var prop = AccessTools.Property(t, name);
                if (prop != null && prop.CanRead) { try { raw = prop.GetValue(target); } catch { } }
                if (raw == null)
                {
                    var field = AccessTools.Field(t, name);
                    if (field != null) { try { raw = field.GetValue(target); } catch { } }
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
                    object c = ConvertNumeric(newValue, prop.PropertyType);
                    if (c != null) { prop.SetValue(target, c); return true; }
                }
                catch { }
            }
            var field = AccessTools.Field(t, name);
            if (field != null)
            {
                try
                {
                    object c = ConvertNumeric(newValue, field.FieldType);
                    if (c != null) { field.SetValue(target, c); return true; }
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
                    if (elem is string s) output.Add(s);
                    else
                    {
                        var idObj = GetMember(elem, "id", "Id", "name");
                        if (idObj is string idStr && !string.IsNullOrEmpty(idStr)) output.Add(idStr);
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
                if (company.notifications != null) company.notifications.Add(notif);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] Oyun ici bildirim eklenemedi: {ex.Message}");
            }
        }
    }
    // =========================================================================
    // 4. FAZ 2 & 3: EVREN KONUMU + EVREN TONU 4 BUTONLU SECIM PENCERESI
    // =========================================================================
    public static class UniverseChoicePopupSystem
    {
        private static readonly HashSet<IntPtr> _promptedGames = new HashSet<IntPtr>();
        private static readonly Dictionary<IntPtr, ProjectCategory> _gameCategories = new Dictionary<IntPtr, ProjectCategory>();
        private static readonly Dictionary<IntPtr, ActiveCharacterDraft> _gameCharacters = new Dictionary<IntPtr, ActiveCharacterDraft>();

        public static ProjectCategory GetCategoryForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero && _gameCategories.TryGetValue(game.Pointer, out var cat))
                return cat;
            return UniverseArchitectMod.ActiveProjectCategory;
        }

        public static void SetCharacterForGame(IntPtr gamePtr, ActiveCharacterDraft draft)
        {
            if (gamePtr != IntPtr.Zero) _gameCharacters[gamePtr] = draft;
        }

        public static ActiveCharacterDraft GetCharacterForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero && _gameCharacters.TryGetValue(game.Pointer, out var draft))
                return draft;
            return null;
        }

        public static void PollForNewGame(Company company)
        {
            if (company == null || !company.isCurrentlyDevelopingGame() || company.currentGame == null)
                return;
            if (CharacterWorkshopUI.IsAnyOverlayOpen)
                return;

            var game = company.currentGame;
            if (game.Pointer == IntPtr.Zero || _promptedGames.Contains(game.Pointer))
                return;

            try
            {
                var panel = UnityEngine.Object.FindObjectOfType<PanelNotifications>();
                if (panel == null || panel.IsNotificationVisible) return;

                _promptedGames.Add(game.Pointer);
                IntPtr capturedGamePtr = game.Pointer;
                string title = !string.IsNullOrEmpty(game.title) ? game.title : "Yeni Proje";
                int integrity = UniverseSaveManager.Current.CanonIntegrity;
                int barrier = UniverseSaveManager.Current.LoreBarrier;
                int sideStreak = UniverseSaveManager.Current.ConsecutiveSideProjects;

                string sideWarn = sideStreak >= 1
                    ? $" [UYARI: Hayranlar Ana Evren'i bekliyor! Ara seri: {sideStreak}]"
                    : "";

                var notif = new Notification(
                    $"Evren Konumu: {title}",
                    $"Bu proje evreninin neresinde yer alacak? (Lore Bariyeri: {barrier}/100){sideWarn}\n\n" +
                    $"• Ana Evren (Tutarlılık: {integrity}/100): Kanon bonusu ve devam gücü.\n" +
                    $"• Yan Evren: Güvenli spin-off (+%5 Skor, Lore Bariyerini düşürür).\n" +
                    $"• Alternatif Zaman: 'Ya farklı olsaydı?' (+10 Hype, Bariyeri düşürür).\n" +
                    $"• Evren Dışı: Bağımsız oyun (evren ve karakter sistemi devre dışı)."
                );

                var labels = new[] { "Ana Evren", "Yan Evren", "Alternatif Zaman", "Evren Dışı" };
                notif.options = new Il2CppSystem.Collections.Generic.List<string>();
                notif.buttonText = new Il2CppSystem.Collections.Generic.List<string>();
                for (int i = 0; i < labels.Length; i++)
                {
                    notif.options.Add(labels[i]);
                    notif.buttonText.Add(labels[i]);
                }

                Action<Notification, Il2CppSystem.Action, int> onClickAction = (n, cb, selectedOption) =>
                {
                    try
                    {
                        ProjectCategory chosen = ProjectCategory.MainUniverse;
                        if (selectedOption == 1) chosen = ProjectCategory.SideUniverse;
                        else if (selectedOption == 2) chosen = ProjectCategory.AltTimeline;
                        else if (selectedOption == 3) chosen = ProjectCategory.NonUniverse;

                        UniverseArchitectMod.ActiveProjectCategory = chosen;
                        _gameCategories[capturedGamePtr] = chosen;

                        MelonLogger.Msg($"[Faz 2/3] '{title}' icin secilen evren konumu: {CategoryLabels.ToTurkish(chosen)}");

                        try { panel.Dismiss(n); } catch { }
                        try { cb?.Invoke(); } catch { }

                        // Ilk Ana Evren oyununda Evren Tonunu belirle, ardindan Karakter Atolyesini ac
                        if (chosen == ProjectCategory.MainUniverse && UniverseSaveManager.GetTone() == UniverseTone.None)
                        {
                            ShowUniverseTonePopup(panel, capturedGamePtr, title);
                        }
                        else if (chosen != ProjectCategory.NonUniverse)
                        {
                            CharacterWorkshopUI.OpenWorkshop(capturedGamePtr, title);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        MelonLogger.Warning($"[Faz 2/3] Secim callback hatasi: {innerEx.Message}");
                    }
                };

                var il2cppDelegate = DelegateSupport.ConvertDelegate<NotificationButton.NotificationButtonDelegate>(onClickAction);
                panel.ShowGenericNotification(notif, il2cppDelegate, null);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 2/3] Secim penceresi acilamadi: {ex.Message}");
            }
        }

        private static void ShowUniverseTonePopup(PanelNotifications panel, IntPtr gamePtr, string title)
        {
            try
            {
                var notif = new Notification(
                    "Evreninin Ana Tonunu Seç",
                    "Ana Evreninin genel atmosferi nasıl olacak? Karakter arketipleri bu tonla uyumluysa ekstra bonus, zıtsa ton çatışması alır:\n\n" +
                    "• Karanlık & Gizem (Anti-Kahraman, Trajik, Kötü uyumlu)\n" +
                    "• Epik Macera (Kahraman, Bilge Mentor uyumlu)\n" +
                    "• Bilim Kurgu & Taktik (Stratejist, Avcı, Kaçak uyumlu)\n" +
                    "• Hafif & Mizahi (Maskot, Düzenbaz uyumlu)"
                );

                var labels = new[] { "Karanlık & Gizem", "Epik Macera", "Bilim Kurgu & Taktik", "Hafif & Mizahi" };
                notif.options = new Il2CppSystem.Collections.Generic.List<string>();
                notif.buttonText = new Il2CppSystem.Collections.Generic.List<string>();
                for (int i = 0; i < labels.Length; i++)
                {
                    notif.options.Add(labels[i]);
                    notif.buttonText.Add(labels[i]);
                }

                Action<Notification, Il2CppSystem.Action, int> onToneClick = (n, cb, opt) =>
                {
                    try
                    {
                        UniverseTone tone = UniverseTone.DarkMystery;
                        if (opt == 1) tone = UniverseTone.EpicAdventure;
                        else if (opt == 2) tone = UniverseTone.SciFiTactical;
                        else if (opt == 3) tone = UniverseTone.LightComedy;

                        UniverseSaveManager.SetTone(tone);
                        MelonLogger.Msg($"[Faz 3] Evren Tonu belirlendi: {CategoryLabels.ToneToTurkish(tone)}");

                        try { panel.Dismiss(n); } catch { }
                        try { cb?.Invoke(); } catch { }

                        CharacterWorkshopUI.OpenWorkshop(gamePtr, title);
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[Faz 3] Ton secim hatasi: {ex.Message}");
                    }
                };

                var del = DelegateSupport.ConvertDelegate<NotificationButton.NotificationButtonDelegate>(onToneClick);
                panel.ShowGenericNotification(notif, del, null);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 3] Ton penceresi acilamadi: {ex.Message}");
                CharacterWorkshopUI.OpenWorkshop(gamePtr, title);
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
                "researchPoints", "ResearchPoints", "_researchPoints")) return;

            _rpMemberName = memberName;

            if (!_starterBoostGiven)
            {
                double boosted = currentRP + ModSettings.TestStarterResearchBoost;
                if (SafeReflection.TrySetNumericMember(company, _rpMemberName, boosted))
                {
                    _lastRP = boosted;
                    _starterBoostGiven = true;
                    MelonLogger.Msg($"[NJG-Turbo] 10x Arastirma Turbo aktif! (+{ModSettings.TestStarterResearchBoost} RP -> {boosted:F0})");
                    NotificationHelper.Show(
                        company,
                        "Faz 3 Karakter Atölyesi + 10x Turbo Hazır!",
                        $"Yeni oyunlarında artık 10 arketip ve 3x3 kaydırıcılı Karakter Atölyesi açılacak! Sağ üstteki 'Karakterler' butonundan kadronu inceleyebilirsin."
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
            if (!ModSettings.TestMode10xResearch || company == null || string.IsNullOrEmpty(_rpMemberName)) return;
            if (!SafeReflection.TryGetNumericMember(company, out double currentRP, out _, _rpMemberName)) return;

            if (_lastRP < 0 || currentRP < _lastRP) { _lastRP = currentRP; return; }

            double delta = currentRP - _lastRP;
            if (delta >= 0.5 && delta <= 200.0)
            {
                double newTotal = currentRP + delta * (ModSettings.ResearchMultiplier - 1.0);
                if (SafeReflection.TrySetNumericMember(company, _rpMemberName, newTotal))
                    _lastRP = newTotal;
            }
            else if (delta > 200.0) _lastRP = currentRP;
        }
    }
    // =========================================================================
    // 6. FAZ 3: KARAKTER ATOLYESI (3x3 KAYDIRICI) & KARAKTERLER MENUSU (GUI)
    // =========================================================================
    public static class CharacterWorkshopUI
    {
        public static bool IsWorkshopOpen { get; private set; } = false;
        public static bool IsRosterOpen { get; private set; } = false;
        public static bool IsAnyOverlayOpen => IsWorkshopOpen || IsRosterOpen;

        private static IntPtr _targetGamePtr = IntPtr.Zero;
        private static string _targetGameTitle = "";
        private static int _workshopMode = 0; // 0: Yeni Karakter, 1: Eski Yildiz Cagir
        private static int _step = 0;         // 0: Kimlik & 10 Arketip, 1: Durus, 2: Psikoloji, 3: Sosyallik
        private static string _charName = "Kael";
        private static int _selectedArchetypeIdx = 0;
        private static float[] _sliders = new float[] { 0.6f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.6f, 0.4f, 0.5f };
        private static int _rosterPage = 0;
        private static TouchScreenKeyboard _keyboard = null;

        private static readonly string[] _quickNames = { "Kael", "Afor", "Feil", "Lyra", "Nıraz", "Saek", "Vex", "Mira", "Toren", "Sora" };
        private static int _quickNameIdx = 0;

        // Oyunun mor / krem / sari renk paleti icin tek piksellik dokular
        private static bool _stylesReady = false;
        private static Texture2D _texDim, _texPanel, _texHeader, _texBtn, _texBtnActive, _texCard;
        private static GUIStyle _stPanel, _stHeader, _stTitle, _stLabel, _stHint, _stBtn, _stBtnActive, _stCard, _stInput;

        public static void OpenWorkshop(IntPtr gamePtr, string gameTitle)
        {
            _targetGamePtr = gamePtr;
            _targetGameTitle = string.IsNullOrEmpty(gameTitle) ? "Yeni Oyun" : gameTitle;
            _workshopMode = 0;
            _step = 0;
            _selectedArchetypeIdx = 0;
            _charName = _quickNames[_quickNameIdx % _quickNames.Length];
            _quickNameIdx++;
            _sliders = new float[] { 0.60f, 0.60f, 0.50f, 0.50f, 0.55f, 0.55f, 0.60f, 0.40f, 0.50f };
            IsRosterOpen = false;
            IsWorkshopOpen = true;
            Time.timeScale = 0f;
            MelonLogger.Msg($"[Faz 3] Karakter Atolyesi acildi: '{_targetGameTitle}'");
        }

        public static void CloseAll()
        {
            IsWorkshopOpen = false;
            IsRosterOpen = false;
            _keyboard = null;
            Time.timeScale = 1f;
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private static void EnsureStyles()
        {
            if (_stylesReady && _texPanel != null) return;
            _texDim       = MakeTex(new Color(0f, 0f, 0f, 0.68f));
            _texPanel     = MakeTex(new Color(0.98f, 0.96f, 0.90f, 1f));
            _texHeader    = MakeTex(new Color(0.47f, 0.29f, 0.52f, 1f));
            _texBtn       = MakeTex(new Color(0.96f, 0.80f, 0.22f, 1f));
            _texBtnActive = MakeTex(new Color(0.35f, 0.68f, 0.38f, 1f));
            _texCard      = MakeTex(new Color(0.92f, 0.89f, 0.80f, 1f));

            _stPanel = new GUIStyle(GUI.skin.box);
            _stPanel.normal.background = _texPanel;

            _stHeader = new GUIStyle(GUI.skin.box);
            _stHeader.normal.background = _texHeader;
            _stHeader.normal.textColor = Color.white;
            _stHeader.fontSize = 24;
            _stHeader.fontStyle = FontStyle.Bold;
            _stHeader.alignment = TextAnchor.MiddleLeft;
            _stHeader.padding = new RectOffset(20, 10, 0, 0);

            _stTitle = new GUIStyle(GUI.skin.label);
            _stTitle.normal.textColor = new Color(0.25f, 0.15f, 0.30f, 1f);
            _stTitle.fontSize = 21;
            _stTitle.fontStyle = FontStyle.Bold;

            _stLabel = new GUIStyle(GUI.skin.label);
            _stLabel.normal.textColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            _stLabel.fontSize = 18;
            _stLabel.wordWrap = true;

            _stHint = new GUIStyle(GUI.skin.label);
            _stHint.normal.textColor = new Color(0.38f, 0.35f, 0.32f, 1f);
            _stHint.fontSize = 15;
            _stHint.wordWrap = true;

            _stBtn = new GUIStyle(GUI.skin.button);
            _stBtn.normal.background = _texBtn;
            _stBtn.hover.background = _texBtn;
            _stBtn.active.background = _texBtnActive;
            _stBtn.normal.textColor = new Color(0.18f, 0.12f, 0.05f, 1f);
            _stBtn.fontSize = 17;
            _stBtn.fontStyle = FontStyle.Bold;

            _stBtnActive = new GUIStyle(_stBtn);
            _stBtnActive.normal.background = _texBtnActive;
            _stBtnActive.hover.background = _texBtnActive;
            _stBtnActive.normal.textColor = Color.white;

            _stCard = new GUIStyle(GUI.skin.box);
            _stCard.normal.background = _texCard;
            _stCard.normal.textColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            _stCard.fontSize = 17;
            _stCard.alignment = TextAnchor.UpperLeft;
            _stCard.padding = new RectOffset(12, 12, 10, 10);

            _stInput = new GUIStyle(GUI.skin.textField);
            _stInput.fontSize = 21;
            _stInput.alignment = TextAnchor.MiddleLeft;

            _stylesReady = true;
        }

        public static void OnGUIDraw()
        {
            if (UniverseArchitectMod.ActiveCompany == null) return;

            EnsureStyles();

            // Android klavyesi aciksa girilen metni anlik al
            if (_keyboard != null)
            {
                try
                {
                    _charName = _keyboard.text;
                    if (_keyboard.status != TouchScreenKeyboard.Status.Visible) _keyboard = null;
                }
                catch { _keyboard = null; }
            }

            float vW = 1280f;
            float vH = 720f;
            float sx = Screen.width / vW;
            float sy = Screen.height / vH;
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(sx, sy, 1f));

            try
            {
                // 1) Sag ustte kucuk "Karakterler" butonu (baska pencere acik degilken)
                if (!IsAnyOverlayOpen)
                {
                    int charCount = UniverseSaveManager.Current.Characters.Count;
                    if (GUI.Button(new Rect(vW - 215f, 130f, 200f, 44f), $"Karakterler ({charCount})", _stBtn))
                    {
                        _rosterPage = 0;
                        IsRosterOpen = true;
                        Time.timeScale = 0f;
                    }
                }
                else if (IsWorkshopOpen)
                {
                    DrawWorkshop(vW, vH);
                }
                else if (IsRosterOpen)
                {
                    DrawRoster(vW, vH);
                }
            }
            finally
            {
                GUI.matrix = oldMatrix;
            }
        }

        private static void DrawWorkshop(float vW, float vH)
        {
            GUI.DrawTexture(new Rect(0, 0, vW, vH), _texDim);

            float pW = 1040f, pH = 620f;
            float pX = (vW - pW) * 0.5f, pY = (vH - pH) * 0.5f;

            GUI.Box(new Rect(pX, pY, pW, pH), "", _stPanel);
            GUI.Box(new Rect(pX, pY, pW, 56f), $"Karakter Atölyesi — {_targetGameTitle}", _stHeader);

            // Ust mod sekmeleri
            float tabY = pY + 68f;
            int savedCount = UniverseSaveManager.Current.Characters.Count;
            if (GUI.Button(new Rect(pX + 20f, tabY, 260f, 42f), "Yeni Karakter Yaz", _workshopMode == 0 ? _stBtnActive : _stBtn))
                _workshopMode = 0;
            if (savedCount > 0 && GUI.Button(new Rect(pX + 295f, tabY, 280f, 42f), $"Eski Yıldızı Çağır ({savedCount})", _workshopMode == 1 ? _stBtnActive : _stBtn))
            {
                _workshopMode = 1;
                _rosterPage = 0;
            }
            if (GUI.Button(new Rect(pX + pW - 220f, tabY, 200f, 42f), "Karaktersiz Geç", _stBtn))
            {
                UniverseChoicePopupSystem.SetCharacterForGame(_targetGamePtr, null);
                CloseAll();
                return;
            }

            if (_workshopMode == 1)
            {
                DrawReturningPicker(pX, pY + 122f, pW, pH - 135f);
                return;
            }

            // Asama cubugu (Kimlik -> 1. Durus -> 2. Psikoloji -> 3. Sosyallik)
            string[] stepNames = { "Kimlik & Arketip", "1. Duruş", "2. Psikoloji", "3. Sosyallik" };
            float stY = pY + 122f;
            float stW = (pW - 70f) / 4f;
            for (int i = 0; i < 4; i++)
            {
                if (GUI.Button(new Rect(pX + 20f + i * (stW + 10f), stY, stW, 38f), stepNames[i], _step == i ? _stBtnActive : _stBtn))
                    _step = i;
            }

            float bodyY = pY + 174f;
            if (_step == 0) DrawStepIdentity(pX + 20f, bodyY, pW - 40f);
            else DrawStepSliders(pX + 20f, bodyY, pW - 40f, _step - 1);

            // Alt butonlar
            float footY = pY + pH - 64f;
            if (_step > 0 && GUI.Button(new Rect(pX + 20f, footY, 180f, 48f), "< Önceki Aşama", _stBtn))
                _step--;

            if (_step < 3)
            {
                if (GUI.Button(new Rect(pX + pW - 220f, footY, 200f, 48f), "Sonraki Aşama >", _stBtnActive))
                    _step++;
            }
            else
            {
                if (GUI.Button(new Rect(pX + pW - 260f, footY, 240f, 48f), "Karakteri Onayla", _stBtnActive))
                {
                    var arch = Phase1Database.Archetypes[_selectedArchetypeIdx];
                    var draft = new ActiveCharacterDraft
                    {
                        IsReturning = false,
                        Name = string.IsNullOrWhiteSpace(_charName) ? "Adsız Kahraman" : _charName.Trim(),
                        ArchetypeId = arch.Id,
                        Sliders = (float[])_sliders.Clone()
                    };
                    UniverseChoicePopupSystem.SetCharacterForGame(_targetGamePtr, draft);
                    MelonLogger.Msg($"[Faz 3] Yeni karakter olusturuldu: '{draft.Name}' ({arch.Name})");
                    CloseAll();
                }
            }
        }

        private static void DrawStepIdentity(float x, float y, float w)
        {
            GUI.Label(new Rect(x, y, 160f, 38f), "Karakter Adı:", _stTitle);
            _charName = GUI.TextField(new Rect(x + 165f, y, 360f, 40f), _charName ?? "", 24, _stInput);

            if (GUI.Button(new Rect(x + 540f, y, 190f, 40f), "Klavye Aç", _stBtn))
            {
                try { _keyboard = TouchScreenKeyboard.Open(_charName ?? "", TouchScreenKeyboardType.Default); } catch { }
            }
            if (GUI.Button(new Rect(x + 745f, y, 180f, 40f), "Rastgele İsim", _stBtn))
            {
                _charName = _quickNames[_quickNameIdx % _quickNames.Length];
                _quickNameIdx++;
            }

            GUI.Label(new Rect(x, y + 54f, w, 30f), "Karakter Arketipi Seç (10 Arketip):", _stTitle);

            float cardW = (w - 40f) / 5f;
            float cardH = 104f;
            for (int i = 0; i < Phase1Database.Archetypes.Count; i++)
            {
                int row = i / 5;
                int col = i % 5;
                float cx = x + col * (cardW + 10f);
                float cy = y + 92f + row * (cardH + 12f);
                var a = Phase1Database.Archetypes[i];
                bool sel = (_selectedArchetypeIdx == i);
                if (GUI.Button(new Rect(cx, cy, cardW, cardH), $"{a.Name}\n\n{a.ShortDesc}", sel ? _stBtnActive : _stBtn))
                    _selectedArchetypeIdx = i;
            }

            var chosen = Phase1Database.Archetypes[_selectedArchetypeIdx];
            GUI.Label(new Rect(x, y + 322f, w, 34f),
                $"Seçili: {chosen.Name} — Uyumlu Ton: {CategoryLabels.ToneToTurkish(chosen.PreferredTone)}", _stLabel);
        }

        private static void DrawStepSliders(float x, float y, float w, int stageIdx)
        {
            string[] stageHeaders =
            {
                "Aşama 1: Duruş (Sanatsal Diyalog edebi kaliteyi belirler, çok konuşmayı değil)",
                "Aşama 2: Psikoloji (Karakterin iç dünyası ve duygusal tonu)",
                "Aşama 3: Sosyallik (Dünyaya, kendine ve insanlara yaklaşımı)"
            };

            int baseIdx = stageIdx * 3;
            float sum = _sliders[baseIdx] + _sliders[baseIdx + 1] + _sliders[baseIdx + 2];
            GUI.Label(new Rect(x, y, w - 260f, 34f), stageHeaders[stageIdx], _stTitle);
            GUI.Label(new Rect(x + w - 250f, y, 250f, 34f), $"Odak Bütçesi: {Mathf.RoundToInt(sum * 100f)} / 180", _stTitle);

            for (int i = 0; i < 3; i++)
            {
                int idx = baseIdx + i;
                float ry = y + 52f + i * 96f;
                GUI.Box(new Rect(x, ry, w, 84f), "", _stCard);

                int pct = Mathf.RoundToInt(_sliders[idx] * 100f);
                GUI.Label(new Rect(x + 16f, ry + 8f, 320f, 30f), $"{Phase1Database.SliderNames[idx]}: %{pct}", _stTitle);
                GUI.Label(new Rect(x + 16f, ry + 40f, 440f, 36f), Phase1Database.SliderHints[idx], _stHint);

                // Dokunmatik dostu - / + butonlari ve kaydirici
                float slX = x + 470f;
                if (GUI.Button(new Rect(slX, ry + 20f, 46f, 44f), "-", _stBtn))
                    SetSliderWithBudget(baseIdx, idx, _sliders[idx] - 0.10f);

                float newVal = GUI.HorizontalSlider(new Rect(slX + 58f, ry + 34f, 360f, 28f), _sliders[idx], 0f, 1f);
                if (Mathf.Abs(newVal - _sliders[idx]) > 0.005f)
                    SetSliderWithBudget(baseIdx, idx, newVal);

                if (GUI.Button(new Rect(slX + 430f, ry + 20f, 46f, 44f), "+", _stBtn))
                    SetSliderWithBudget(baseIdx, idx, _sliders[idx] + 0.10f);
            }
        }

        private static void SetSliderWithBudget(int baseIdx, int targetIdx, float desired)
        {
            desired = Mathf.Clamp01(desired);
            float others = 0f;
            for (int i = 0; i < 3; i++)
                if (baseIdx + i != targetIdx) others += _sliders[baseIdx + i];

            float maxAllowed = Mathf.Clamp01(BalanceCaps.StageSliderBudget - others);
            _sliders[targetIdx] = Mathf.Min(desired, maxAllowed);
        }

        private static void DrawReturningPicker(float pX, float startY, float pW, float areaH)
        {
            var chars = UniverseSaveManager.Current.Characters;
            int perPage = 4;
            int totalPages = Math.Max(1, (chars.Count + perPage - 1) / perPage);
            _rosterPage = Math.Clamp(_rosterPage, 0, totalPages - 1);

            int start = _rosterPage * perPage;
            for (int i = 0; i < perPage && (start + i) < chars.Count; i++)
            {
                var c = chars[start + i];
                float cy = startY + i * 98f;
                string fatigue = c.ConsecutiveUses >= 3 ? "YORGUN! (-Skor)" : $"Dinç (Seri: {c.ConsecutiveUses})";
                GUI.Box(new Rect(pX + 20f, cy, pW - 260f, 86f),
                    $"{c.Name}  [{c.Status}] — {c.ArchetypeName}\nPopülerlik: {c.Popularity}/100 | Oyun: {c.GamesAppeared} | Durum: {fatigue} | Çıkış: {c.DebutGame}", _stCard);

                if (GUI.Button(new Rect(pX + pW - 225f, cy + 14f, 205f, 58f), "Başrol Seç", _stBtnActive))
                {
                    var draft = new ActiveCharacterDraft
                    {
                        IsReturning = true,
                        ExistingCharacterId = c.Id,
                        Name = c.Name,
                        ArchetypeId = c.ArchetypeId,
                        Sliders = (c.Sliders != null && c.Sliders.Length == 9) ? (float[])c.Sliders.Clone() : new float[9]
                    };
                    UniverseChoicePopupSystem.SetCharacterForGame(_targetGamePtr, draft);
                    MelonLogger.Msg($"[Faz 3] Eski karakter geri cagrildi: '{c.Name}' (Seri: {c.ConsecutiveUses})");
                    CloseAll();
                    return;
                }
            }

            DrawPager(pX, startY + areaH - 54f, pW, totalPages);
        }

        private static void DrawRoster(float vW, float vH)
        {
            GUI.DrawTexture(new Rect(0, 0, vW, vH), _texDim);

            float pW = 1040f, pH = 620f;
            float pX = (vW - pW) * 0.5f, pY = (vH - pH) * 0.5f;
            var u = UniverseSaveManager.Current;

            GUI.Box(new Rect(pX, pY, pW, pH), "", _stPanel);
            GUI.Box(new Rect(pX, pY, pW, 56f), $"Karakterler & Evren Durumu ({u.Characters.Count} Karakter)", _stHeader);

            if (GUI.Button(new Rect(pX + pW - 140f, pY + 8f, 126f, 40f), "Kapat", _stBtn))
            {
                CloseAll();
                return;
            }

            GUI.Box(new Rect(pX + 20f, pY + 68f, pW - 40f, 52f),
                $"Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())}  |  Tutarlılık: {u.CanonIntegrity}/100  |  Lore Bariyeri: {u.LoreBarrier}/100  |  Ara Evren Serisi: {u.ConsecutiveSideProjects}", _stCard);

            if (u.Characters.Count == 0)
            {
                GUI.Label(new Rect(pX + 40f, pY + 180f, pW - 80f, 80f),
                    "Henüz evrenine kayıtlı karakter yok! Yeni bir Ana/Yan Evren veya Alternatif Zaman oyununa başladığında Karakter Atölyesi otomatik açılacak.", _stTitle);
                return;
            }

            int perPage = 4;
            int totalPages = Math.Max(1, (u.Characters.Count + perPage - 1) / perPage);
            _rosterPage = Math.Clamp(_rosterPage, 0, totalPages - 1);

            int start = _rosterPage * perPage;
            for (int i = 0; i < perPage && (start + i) < u.Characters.Count; i++)
            {
                var c = u.Characters[start + i];
                float cy = pY + 134f + i * 102f;
                string fatigue = c.ConsecutiveUses >= 3 ? "YORGUN (Dinlendir!)" : (c.ConsecutiveUses == 0 ? "Dinç" : $"Aktif (Seri {c.ConsecutiveUses})");
                string sliderSum = (c.Sliders != null && c.Sliders.Length == 9)
                    ? $"Kavg:{c.Sliders[0]*100:F0} San.Diy:{c.Sliders[1]*100:F0} Zek:{c.Sliders[2]*100:F0} | Trv:{c.Sliders[3]*100:F0} İçAç:{c.Sliders[4]*100:F0} Mzh:{c.Sliders[5]*100:F0} | Çev:{c.Sliders[6]*100:F0} Ego:{c.Sliders[7]*100:F0} Kap:{c.Sliders[8]*100:F0}"
                    : "";

                GUI.Box(new Rect(pX + 20f, cy, pW - 40f, 92f),
                    $"{c.Name}  [{c.Status}]  —  {c.ArchetypeName}  |  Popülerlik: {c.Popularity}/100  |  Oyun: {c.GamesAppeared}  |  {fatigue}\n" +
                    $"Çıkış Oyunu: {c.DebutGame}\n{sliderSum}", _stCard);
            }

            DrawPager(pX, pY + pH - 58f, pW, totalPages);
        }

        private static void DrawPager(float pX, float y, float pW, int totalPages)
        {
            if (totalPages <= 1) return;
            if (_rosterPage > 0 && GUI.Button(new Rect(pX + 20f, y, 160f, 44f), "< Önceki", _stBtn)) _rosterPage--;
            GUI.Label(new Rect(pX + pW * 0.5f - 70f, y + 8f, 140f, 34f), $"Sayfa {_rosterPage + 1} / {totalPages}", _stTitle);
            if (_rosterPage < totalPages - 1 && GUI.Button(new Rect(pX + pW - 180f, y, 160f, 44f), "Sonraki >", _stBtn)) _rosterPage++;
        }
    }

    // =========================================================================
    // 7. ANA MOD GIRISI (OnUpdate + OnGUI)
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
            LoggerInstance.Msg("Universe Architect v3.0.0 (Faz 3: 10 Arketip + 3x3 Karakter Atolyesi + Lore Bariyeri) baslatiliyor...");
            UniverseSaveManager.Load();

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v300");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v3.0.0 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        public override void OnGUI()
        {
            try { CharacterWorkshopUI.OnGUIDraw(); }
            catch (Exception ex) { MelonLogger.Warning($"[Faz 3 GUI] {ex.Message}"); }
        }

        public override void OnUpdate()
        {
            if (!TopicsInjected || !EngineFeaturesInjected)
            {
                _injectTimer += Time.unscaledDeltaTime;
                if (_injectTimer >= 2.0f)
                {
                    _injectTimer = 0f;
                    if (!TopicsInjected) TopicInjectorHelper.TryInjectTopics();
                    if (!EngineFeaturesInjected) EngineFeatureInjectorHelper.TryInjectFromScriptableObject();
                }
            }

            if (ActiveCompany == null) return;

            _popupPollTimer += Time.unscaledDeltaTime;
            if (_popupPollTimer >= 1.0f)
            {
                _popupPollTimer = 0f;
                try { UniverseChoicePopupSystem.PollForNewGame(ActiveCompany); } catch { }
            }

            if (ModSettings.TestMode10xResearch)
            {
                _turboTimer += Time.unscaledDeltaTime;
                if (_turboTimer >= 0.5f)
                {
                    _turboTimer = 0f;
                    try { ResearchTurboSystem.PollAndMultiply(ActiveCompany); } catch { }
                }
            }

            if (EngineFeaturesInjected)
            {
                _companySyncTimer += Time.unscaledDeltaTime;
                if (_companySyncTimer >= 3.0f)
                {
                    _companySyncTimer = 0f;
                    try { EngineFeatureInjectorHelper.SyncWithCompany(ActiveCompany); } catch { }
                }
            }
        }

        public static int ApplyCappedModHype(int rawModHype)
        {
            return Math.Min(rawModHype, BalanceCaps.MaxTotalModHype);
        }
    }

    // =========================================================================
    // 8. KONU & MOTOR / AR-GE ENJEKSIYONLARI (KANCASIZ)
    // =========================================================================
    public static class TopicInjectorHelper
    {
        public static void TryInjectTopics()
        {
            if (UniverseArchitectMod.TopicsInjected) return;
            try
            {
                var allTopicsAssets = UnityEngine.Resources.FindObjectsOfTypeAll<Topics>();
                if (allTopicsAssets == null || allTopicsAssets.Length == 0) return;

                var topicsAsset = allTopicsAssets[0];
                if (topicsAsset == null || topicsAsset.topics == null || topicsAsset.topics.Count == 0) return;

                var templateTopic = topicsAsset.topics[0];
                foreach (var def in Phase1Database.Topics)
                {
                    bool exists = false;
                    for (int i = 0; i < topicsAsset.topics.Count; i++)
                        if (topicsAsset.topics[i].id == def.Id) { exists = true; break; }
                    if (exists) continue;

                    var newTopic = new TopicType();
                    newTopic.id = def.Id;
                    newTopic.name = def.Name;
                    newTopic.missionOverrides = templateTopic.missionOverrides;
                    newTopic.genreWeightings = new GenreWeightings(
                        def.GenreWeights["Action"], def.GenreWeights["Adventure"], def.GenreWeights["RPG"],
                        def.GenreWeights["Simulation"], def.GenreWeights["Strategy"], def.GenreWeights["Casual"]);
                    newTopic.audienceWeightings = new AudienceWeightings(
                        def.AudienceWeights[0], def.AudienceWeights[1], def.AudienceWeights[2]);
                    topicsAsset.topics.Add(newTopic);
                }

                MelonLogger.Msg("[Universe Architect] 6 ozel konu yuklendi.");
                UniverseArchitectMod.TopicsInjected = true;
            }
            catch (Exception ex) { MelonLogger.Error($"[Universe Architect] Konu enjeksiyon hatasi: {ex}"); }
        }
    }

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
                if (researchAssets == null || researchAssets.Length == 0) return;
                var research = researchAssets[0];
                if (research == null) return;

                var privateAllItems = SafeReflection.AsResearchList(SafeReflection.GetMember(research, "allItems"));
                var getterAllItems = research.getAllItems();
                var masterList = (privateAllItems != null && privateAllItems.Count > 0) ? privateAllItems : getterAllItems;
                if (masterList == null || masterList.Count == 0) return;

                ResearchItem template = FindResearchableEngineTemplate(research, masterList);
                if (template == null) return;

                var cloneMethod = AccessTools.Method(typeof(Il2CppSystem.Object), "MemberwiseClone");
                if (cloneMethod == null) { UniverseArchitectMod.EngineFeaturesInjected = true; return; }

                CreatedItems.Clear();
                int addedCount = 0;
                foreach (var feat in Phase1Database.EngineFeatures)
                {
                    var existing = FindById(masterList, feat.Id);
                    if (existing != null) { CreatedItems.Add(existing); continue; }

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

                MelonLogger.Msg($"[Universe Architect] {addedCount} motor ozelligi '{template.id}' sablonuyla eklendi.");
                UniverseArchitectMod.EngineFeaturesInjected = true;
                if (UniverseArchitectMod.ActiveCompany != null) SyncWithCompany(UniverseArchitectMod.ActiveCompany);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] Motor enjeksiyon hatasi: {ex}");
                UniverseArchitectMod.EngineFeaturesInjected = true;
            }
        }

        public static void SyncWithCompany(Company company)
        {
            if (company == null || CreatedItems.Count == 0) return;
            var avail = SafeReflection.AsResearchList(SafeReflection.GetMember(company, "availableResearch"));
            var completed = SafeReflection.AsResearchList(SafeReflection.GetMember(company, "researchCompleted"));
            var engineParts = SafeReflection.AsResearchList(SafeReflection.GetMember(company, "engineParts"));

            int addedToAvail = 0, unlockedToEngine = 0;
            for (int i = 0; i < CreatedItems.Count; i++)
            {
                var item = CreatedItems[i];
                if (item == null) continue;
                bool isCompleted = ContainsId(completed, item.id);
                bool isEnginePart = ContainsId(engineParts, item.id);

                if (isCompleted && engineParts != null && !isEnginePart) { engineParts.Add(item); unlockedToEngine++; }
                if (!isCompleted && !isEnginePart && avail != null && !ContainsId(avail, item.id)) { avail.Add(item); addedToAvail++; }
            }

            if ((addedToAvail > 0 || unlockedToEngine > 0) || !_loggedCompanySync)
            {
                MelonLogger.Msg($"[NJG-Research] Senkronizasyon -> Arastirma: +{addedToAvail}, Ozel Motor: +{unlockedToEngine}");
                _loggedCompanySync = true;
            }
        }

        private static ResearchItem FindResearchableEngineTemplate(Research research, Il2CppSystem.Collections.Generic.List<ResearchItem> allItems)
        {
            string[] preferred = { "BetterUI", "betterDialogs", "dialogueTree", "BetterUX", "StoryQuests" };
            foreach (var f in preferred)
            {
                var cand = SafeReflection.AsResearchItem(SafeReflection.GetMember(research, f));
                if (cand != null) return cand;
            }
            return allItems[0];
        }

        private static ResearchItem FindById(Il2CppSystem.Collections.Generic.List<ResearchItem> list, string id)
        {
            if (list == null) return null;
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].id == id) return list[i];
            return null;
        }

        private static bool ContainsId(Il2CppSystem.Collections.Generic.List<ResearchItem> list, string id) => FindById(list, id) != null;
    }
    // =========================================================================
    // 9. INCELEME: FAZ 1 + FAZ 2 + FAZ 3 (KARAKTER KIMYASI, LORE BARIYERI, ARA EVREN)
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
                string genreName = ExtractGenreName(game);
                ProjectCategory category = UniverseChoicePopupSystem.GetCategoryForGame(game);
                ActiveCharacterDraft charDraft = UniverseChoicePopupSystem.GetCharacterForGame(game);

                int totalModHype = 0;
                int hypePenalty = 0;
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

                // B) 6 Yeni Konu Bonuslari
                var customTopic = Phase1Database.FindTopicById(topicId);
                if (customTopic != null)
                {
                    if (customTopic.ScoreBonusMultiplier > 1.0) totalScoreMult *= customTopic.ScoreBonusMultiplier;
                    if (customTopic.BonusHype > 0) totalModHype += customTopic.BonusHype;
                    activeBonusNotes.Add($"{customTopic.Name}: {customTopic.BonusLabel}");
                }

                // C) 5 Yeni Motor Ozelligi
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
                    totalScoreMult *= 1.06; totalModHype += 5;
                    activeBonusNotes.Add("Dallanan Hikaye (+%6 Skor, +5 Hype)");
                }
                if (featureIds.Contains("njg_soundtrack_rap"))
                {
                    usedModFeatures.Add("njg_soundtrack_rap");
                    totalScoreMult *= 1.03; totalModHype += 15;
                    activeBonusNotes.Add("Soundtrack & Rap (+15 Hype, +%3 Skor)");
                }
                if (featureIds.Contains("njg_post_credits"))
                {
                    usedModFeatures.Add("njg_post_credits");
                    UniverseSaveManager.Current.PendingPostCreditsHype = 12;
                    activeBonusNotes.Add("Jenerik Sonrası Sahne (Sıradakine +12 Hype!)");
                }
                if (featureIds.Contains("njg_mod_support"))
                {
                    usedModFeatures.Add("njg_mod_support");
                    totalScoreMult *= 1.04; totalModHype += 8;
                    double bonusFans = Math.Max(50.0, Math.Round(company.fans * 0.05));
                    company.adjustFans(bonusFans);
                    activeBonusNotes.Add($"Resmi Mod Desteği (+%4 Skor, +8 Hype, +{bonusFans:F0} Hayran)");
                }

                // D) FAZ 2 & 3: EVREN KONUMU + ARA EVREN BEKLENTISI + LORE BARIYERI
                int integrity = UniverseSaveManager.Current.CanonIntegrity;
                if (category == ProjectCategory.MainUniverse)
                {
                    UniverseSaveManager.Current.ConsecutiveSideProjects = 0;

                    if (integrity >= 80) { totalScoreMult *= 1.04; totalModHype += 5; activeBonusNotes.Add($"Ana Evren [Tutarlılık {integrity}/100] (+%4 Skor, +5 Hype)"); }
                    else if (integrity >= 50) { totalScoreMult *= 1.02; totalModHype += 2; activeBonusNotes.Add($"Ana Evren [Tutarlılık {integrity}/100] (+%2 Skor, +2 Hype)"); }
                    else { totalScoreMult *= 0.96; activeBonusNotes.Add($"Ana Evren Çatlağı! [Tutarlılık {integrity}/100] (-%4 Skor)"); }

                    if (!string.IsNullOrEmpty(game.sequelTo)) { totalScoreMult *= 1.03; activeBonusNotes.Add("Ana Evren Devam Oyunu (+%3 Skor)"); }

                    // Lore Bariyeri kontrolu ve artisi
                    if (UniverseSaveManager.Current.LoreBarrier >= 60)
                    {
                        double barrierLoss = Math.Min(company.fans * 0.03, 300.0);
                        if (barrierLoss >= 10.0) company.adjustFans(-barrierLoss);
                        activeBonusNotes.Add($"Lore Bariyeri Yüksek ({UniverseSaveManager.Current.LoreBarrier}/100)! Yeni oyuncular zorlanıyor.");
                    }
                    UniverseSaveManager.AdjustLoreBarrier(+15);
                }
                else if (category == ProjectCategory.SideUniverse || category == ProjectCategory.AltTimeline)
                {
                    UniverseSaveManager.Current.ConsecutiveSideProjects++;
                    UniverseSaveManager.AdjustLoreBarrier(-25);

                    if (category == ProjectCategory.SideUniverse)
                    {
                        totalScoreMult *= 1.05;
                        activeBonusNotes.Add("Yan Evren (+%5 Skor, Lore Bariyeri -25)");
                    }
                    else
                    {
                        totalModHype += 10;
                        activeBonusNotes.Add("Alternatif Zaman (+10 Hype, Lore Bariyeri -25)");
                    }

                    // Ust uste ara evren cikarma cezasi (Ana Evreni bekleyenler sabirsizlanir)
                    int streak = UniverseSaveManager.Current.ConsecutiveSideProjects;
                    if (streak >= 2)
                    {
                        int waitPenalty = Math.Min(20, (streak - 1) * 4);
                        hypePenalty += waitPenalty;
                        activeBonusNotes.Add($"Ana Evren Beklentisi! Üst üste {streak}. ara oyun (-{waitPenalty} Hype)");
                    }
                }
                else
                {
                    activeBonusNotes.Add("Evren Dışı (Bağımsız proje)");
                }

                // E) FAZ 3: KARAKTER KIMYASI (ARKETIP x TUR x 3x3 KAYDIRICI x EVREN TONU)
                string usedCharName = "";
                if (charDraft != null && category != ProjectCategory.NonUniverse)
                {
                    usedCharName = charDraft.Name;
                    EvaluateCharacter(company, game, title, genreName, category, charDraft,
                        ref totalScoreMult, ref totalModHype, ref hypePenalty, activeBonusNotes);
                }
                else
                {
                    UniverseSaveManager.RestOtherCharacters("");
                }

                // F) Global Denge Tavanlariyla Skor & Hype Uygula
                if (totalScoreMult != 1.0) game.score = Math.Clamp(game.score * totalScoreMult, 1.0, 10.0);

                int netHype = UniverseArchitectMod.ApplyCappedModHype(totalModHype) - hypePenalty;
                if (netHype != 0) company.adjustHype(netHype);

                // G) Ana Evren Tutarlilik Guncellemesi
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

                UniverseSaveManager.RecordReleasedGame(game, topicId, game.score, category, usedModFeatures, usedCharName);
                MelonLogger.Msg($"[Universe Architect] Kaydedildi: '{title}' | {CategoryLabels.ToTurkish(category)} | Karakter: {(string.IsNullOrEmpty(usedCharName) ? "-" : usedCharName)} | Skor: {game.score:F2}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] ReviewGame hatasi: {ex.Message}");
            }
        }

        private static void EvaluateCharacter(
            Company company, Game game, string gameTitle, string genreName, ProjectCategory category,
            ActiveCharacterDraft draft, ref double scoreMult, ref int modHype, ref int hypePenalty, List<string> notes)
        {
            var arch = Phase1Database.FindArchetype(draft.ArchetypeId);

            // 1) Arketip x Oyun Turu uyumu
            double genreFit = 0.75;
            if (!string.IsNullOrEmpty(genreName) && arch.GenreWeights.TryGetValue(genreName, out double gw))
                genreFit = gw;

            // 2) 3x3 Kaydirici x Arketip uyumu
            double diffSum = 0.0;
            for (int i = 0; i < 9; i++)
            {
                float actual = (draft.Sliders != null && i < draft.Sliders.Length) ? draft.Sliders[i] : 0.5f;
                diffSum += Math.Abs(actual - arch.IdealSliders[i]);
            }
            double sliderFit = Math.Clamp(1.0 - (diffSum / 9.0) * 1.50, 0.20, 1.0);

            // 3) Evren Tonu uyumu
            double toneMod = 0.0;
            string toneNote = "";
            var uTone = UniverseSaveManager.GetTone();
            if (uTone != UniverseTone.None)
            {
                if (arch.PreferredTone == uTone) { toneMod = +0.08; toneNote = " (Evren tonuyla tam uyum!)"; }
                else if (arch.ClashingTone == uTone) { toneMod = -0.10; toneNote = " (Evren tonuyla çatışma!)"; }
            }

            double chemistry = Math.Clamp(genreFit * 0.45 + sliderFit * 0.55 + toneMod, 0.15, 1.0);
            int chemPct = (int)Math.Round(chemistry * 100.0);

            // Kayitli karakteri bul veya yeni olustur
            CharacterRecord rec = draft.IsReturning
                ? UniverseSaveManager.FindCharacterById(draft.ExistingCharacterId)
                : null;

            if (rec == null)
            {
                rec = new CharacterRecord
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Name = draft.Name,
                    ArchetypeId = arch.Id,
                    ArchetypeName = arch.Name,
                    Sliders = (float[])draft.Sliders.Clone(),
                    DebutGame = gameTitle,
                    Popularity = chemPct
                };
                UniverseSaveManager.Current.Characters.Add(rec);
            }

            rec.GamesAppeared++;
            rec.ConsecutiveUses++;
            UniverseSaveManager.RestOtherCharacters(rec.Id);

            // Geri donen karakter yorgunluk kontrolu
            if (draft.IsReturning && rec.ConsecutiveUses >= 3)
            {
                scoreMult *= 0.95;
                hypePenalty += 5;
                rec.Popularity = Math.Clamp(rec.Popularity - 8, 10, 100);
                notes.Add($"Karakter Yorgunluğu: {rec.Name} ({rec.ConsecutiveUses}. üst üste oyun, -%5 Skor, -5 Hype)");
                NotificationHelper.Show(company, $"Karakter Yorgunluğu: {rec.Name}",
                    $"Hayranlar {rec.Name} karakterini üst üste görmekten yoruldu! Birkaç oyun dinlendirirsen yeniden özlenecek.");
                return;
            }

            // Kimya sonucuna gore tepki ve ozel bildirim
            if (chemistry >= 0.78)
            {
                scoreMult *= 1.06;
                modHype += 8;
                double fanBoost = Math.Max(40.0, Math.Round(company.fans * 0.06));
                company.adjustFans(fanBoost);
                rec.Status = "Yıldız";
                rec.Popularity = Math.Clamp(Math.Max(rec.Popularity, chemPct) + 5, 10, 100);
                notes.Add($"İkonik Karakter: {rec.Name} [Uyum %{chemPct}] (+%6 Skor, +8 Hype){toneNote}");

                NotificationHelper.Show(company, $"Hayranlar {rec.Name} Karakterine Bayıldı!",
                    $"{gameTitle} oyunundaki {rec.Name} ({arch.Name}) uyumu %{chemPct} ile efsaneleşti! Sosyal medyada replikleri paylaşılıyor (+{fanBoost:F0} Hayran).");
            }
            else if (chemistry >= 0.60)
            {
                scoreMult *= 1.02;
                modHype += 3;
                if (rec.Status != "Yıldız") rec.Status = "Sevilen";
                rec.Popularity = Math.Clamp((rec.Popularity + chemPct) / 2, 10, 100);
                notes.Add($"Sevilen Karakter: {rec.Name} [Uyum %{chemPct}] (+%2 Skor, +3 Hype){toneNote}");
            }
            else if (chemistry >= 0.45)
            {
                notes.Add($"Sıradan Karakter: {rec.Name} [Uyum %{chemPct}]{toneNote}");
            }
            else
            {
                scoreMult *= 0.93;
                hypePenalty += 6;
                rec.Status = "Fiyasko";
                rec.Popularity = Math.Clamp(chemPct, 5, 45);
                if (category == ProjectCategory.MainUniverse) UniverseSaveManager.AdjustIntegrity(-6);
                notes.Add($"Fiyasko Karakter: {rec.Name} [Uyum %{chemPct}] (-%7 Skor, -6 Hype){toneNote}");

                NotificationHelper.Show(company, $"Oyuncular {rec.Name} Karakterinden Nefret Etti!",
                    $"{gameTitle} ({genreName}) için {rec.Name} ({arch.Name}) hiç uymadı (Uyum %{chemPct}). Eleştirmenler karakterin oyunun havasını bozduğunu yazıyor!");
            }
        }

        private static string ExtractGenreName(Game game)
        {
            if (game == null) return "";
            var genreObj = SafeReflection.GetMember(game, "genre", "Genre");
            if (genreObj == null) return "";
            var idObj = SafeReflection.GetMember(genreObj, "id", "name", "Id", "Name");
            return idObj as string ?? "";
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
        static bool Prefix() { MelonLogger.Msg("[Universe Architect] InitPlayGamesPlatform durduruldu."); return false; }
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
        static bool Prefix(ref bool __result) { __result = false; return false; }
    }
}
