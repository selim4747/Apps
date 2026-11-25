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

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "3.0.1", "NJG Games")]
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
        public const int StageSliderBudgetPct = 180; // Her 3'lu asamada max %180 odak butcesi
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

        public static readonly string[] CharacterNamePool = new[]
        {
            "Afor", "Feil", "Nıraz", "Saek", "Yejen", "Kael",
            "Lyra", "Toren", "Mira", "Vex", "Sora", "Darian",
            "Elen", "Riven", "Kora", "Orin", "Zeph", "Nora",
            "Cyrus", "Ayla", "Juno"
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
    // 4. NATIVE 4-BUTONLU PENCERE KUYRUGU (SIFIR IMGUI / SIFIR EKSTRA DLL)
    // =========================================================================
    public class PendingPopup
    {
        public string Header;
        public string Text;
        public string[] Buttons;
        public Action<int> OnSelected;
    }

    public static class NativePopupQueue
    {
        private static PendingPopup _nextPopup = null;
        private static float _delayTimer = 0f;

        public static bool HasPending => _nextPopup != null;

        public static void Enqueue(string header, string text, string[] buttons, Action<int> onSelected)
        {
            _nextPopup = new PendingPopup
            {
                Header = header,
                Text = text,
                Buttons = buttons,
                OnSelected = onSelected
            };
            _delayTimer = 0.12f;
        }

        public static void Tick(float dt)
        {
            if (_nextPopup == null) return;

            if (_delayTimer > 0f)
            {
                _delayTimer -= dt;
                return;
            }

            try
            {
                var panel = UnityEngine.Object.FindObjectOfType<PanelNotifications>();
                if (panel == null || panel.IsNotificationVisible) return;

                var p = _nextPopup;
                _nextPopup = null;

                var notif = new Notification(p.Header, p.Text);
                notif.options = new Il2CppSystem.Collections.Generic.List<string>();
                notif.buttonText = new Il2CppSystem.Collections.Generic.List<string>();
                for (int i = 0; i < p.Buttons.Length; i++)
                {
                    notif.options.Add(p.Buttons[i]);
                    notif.buttonText.Add(p.Buttons[i]);
                }

                Action<Notification, Il2CppSystem.Action, int> onClickAction = (n, cb, selectedOption) =>
                {
                    try { panel.Dismiss(n); } catch { }
                    try { cb?.Invoke(); } catch { }
                    try { p.OnSelected?.Invoke(selectedOption); }
                    catch (Exception cbEx) { MelonLogger.Warning($"[Faz 3 Popup] Callback hatasi: {cbEx.Message}"); }
                };

                var del = DelegateSupport.ConvertDelegate<NotificationButton.NotificationButtonDelegate>(onClickAction);
                panel.ShowGenericNotification(notif, del, null);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 3 Popup] Pencere gosterim hatasi: {ex.Message}");
                _nextPopup = null;
            }
        }
    }

    // =========================================================================
    // 5. FAZ 2 & 3: EVREN KONUMU + TON + NATIVE 3x3 KARAKTER ATOLYESI SIHIRBAZI
    // =========================================================================
    public static class UniverseChoicePopupSystem
    {
        private static readonly HashSet<IntPtr> _promptedGames = new HashSet<IntPtr>();
        private static readonly Dictionary<IntPtr, ProjectCategory> _gameCategories = new Dictionary<IntPtr, ProjectCategory>();
        private static readonly Dictionary<IntPtr, ActiveCharacterDraft> _gameCharacters = new Dictionary<IntPtr, ActiveCharacterDraft>();

        // Aktif karakter olusturma taslagi (native 4 butonlu sihirbaz)
        private static IntPtr _draftGamePtr = IntPtr.Zero;
        private static string _draftGameTitle = "";
        private static string _draftName = "Afor";
        private static int _draftArchetypeIdx = 0;
        private static int[] _draftSlidersPct = new int[] { 60, 60, 40, 60, 60, 40, 60, 40, 60 };
        private static int _namePage = 0;

        public static ProjectCategory GetCategoryForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero && _gameCategories.TryGetValue(game.Pointer, out var cat))
                return cat;
            return UniverseArchitectMod.ActiveProjectCategory;
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
            if (NativePopupQueue.HasPending)
                return;

            var game = company.currentGame;
            if (game.Pointer == IntPtr.Zero || _promptedGames.Contains(game.Pointer))
                return;

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

            NativePopupQueue.Enqueue(
                $"Evren Konumu: {title}",
                $"Bu proje evreninin neresinde yer alacak? (Lore Bariyeri: {barrier}/100){sideWarn}\n\n" +
                $"• Ana Evren (Tutarlılık: {integrity}/100): Kanon bonusu ve devam gücü.\n" +
                $"• Yan Evren: Güvenli spin-off (+%5 Skor, Lore Bariyeri -25).\n" +
                $"• Alternatif Zaman: 'Ya farklı olsaydı?' (+10 Hype, Bariyer -25).\n" +
                $"• Evren Dışı: Bağımsız oyun (evren ve karakter sistemi yok).",
                new[] { "Ana Evren", "Yan Evren", "Alternatif Zaman", "Evren Dışı" },
                opt =>
                {
                    ProjectCategory chosen = ProjectCategory.MainUniverse;
                    if (opt == 1) chosen = ProjectCategory.SideUniverse;
                    else if (opt == 2) chosen = ProjectCategory.AltTimeline;
                    else if (opt == 3) chosen = ProjectCategory.NonUniverse;

                    UniverseArchitectMod.ActiveProjectCategory = chosen;
                    _gameCategories[capturedGamePtr] = chosen;
                    MelonLogger.Msg($"[Faz 2/3] '{title}' icin secilen evren konumu: {CategoryLabels.ToTurkish(chosen)}");

                    if (chosen == ProjectCategory.MainUniverse && UniverseSaveManager.GetTone() == UniverseTone.None)
                    {
                        OpenToneSelector(capturedGamePtr, title);
                    }
                    else if (chosen != ProjectCategory.NonUniverse)
                    {
                        OpenCharacterHub(capturedGamePtr, title);
                    }
                }
            );
        }

        private static void OpenToneSelector(IntPtr gamePtr, string title)
        {
            NativePopupQueue.Enqueue(
                "Evreninin Ana Tonunu Seç",
                "Ana Evreninin genel atmosferi nasıl olacak? Karakter arketipleri bu tonla uyumluysa ekstra bonus, zıtsa ton çatışması alır:\n\n" +
                "• Karanlık & Gizem (Anti-Kahraman, Trajik, Kötü uyumlu)\n" +
                "• Epik Macera (Kahraman, Bilge Mentor uyumlu)\n" +
                "• Bilim Kurgu & Taktik (Stratejist, Avcı, Kaçak uyumlu)\n" +
                "• Hafif & Mizahi (Maskot, Düzenbaz uyumlu)",
                new[] { "Karanlık & Gizem", "Epik Macera", "Bilim Kurgu & Taktik", "Hafif & Mizahi" },
                opt =>
                {
                    UniverseTone tone = UniverseTone.DarkMystery;
                    if (opt == 1) tone = UniverseTone.EpicAdventure;
                    else if (opt == 2) tone = UniverseTone.SciFiTactical;
                    else if (opt == 3) tone = UniverseTone.LightComedy;

                    UniverseSaveManager.SetTone(tone);
                    MelonLogger.Msg($"[Faz 3] Evren Tonu belirlendi: {CategoryLabels.ToneToTurkish(tone)}");
                    OpenCharacterHub(gamePtr, title);
                }
            );
        }

        private static void OpenCharacterHub(IntPtr gamePtr, string title)
        {
            _draftGamePtr = gamePtr;
            _draftGameTitle = title;

            var u = UniverseSaveManager.Current;
            int charCount = u.Characters.Count;
            string secondBtn = charCount > 0 ? $"Eski Yıldızı Çağır ({charCount})" : "Kadro Boş (Yeni Yaz)";

            NativePopupQueue.Enqueue(
                $"Karakter Atölyesi: {title}",
                $"Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())} | Tutarlılık: {u.CanonIntegrity}/100 | Lore Bariyeri: {u.LoreBarrier}/100\n" +
                $"Kayıtlı Karakter: {charCount} | Ara Evren Serisi: {u.ConsecutiveSideProjects}\n\n" +
                "Bu oyun için 10 arketip ve 3×3 kişilik kaydırıcısıyla (Duruş, Psikoloji, Sosyallik) yeni başrol yazabilir veya sevilen bir yıldızı geri çağırabilirsin.",
                new[] { "Yeni Karakter Yaz", secondBtn, $"Karakterler ({charCount})", "Karaktersiz Geç" },
                opt =>
                {
                    if (opt == 0 || (opt == 1 && charCount == 0))
                    {
                        _draftSlidersPct = new int[] { 60, 60, 40, 60, 60, 40, 60, 40, 60 };
                        OpenArchetypePage(0);
                    }
                    else if (opt == 1)
                    {
                        OpenReturningCharacterPage(0);
                    }
                    else if (opt == 2)
                    {
                        OpenRosterViewerPage(0);
                    }
                    else
                    {
                        _gameCharacters[gamePtr] = null;
                        MelonLogger.Msg($"[Faz 3] '{title}' karaktersiz devam ediyor.");
                    }
                }
            );
        }

        private static void OpenRosterViewerPage(int page)
        {
            var chars = UniverseSaveManager.Current.Characters;
            var u = UniverseSaveManager.Current;

            if (chars.Count == 0)
            {
                NativePopupQueue.Enqueue(
                    "Karakterler (0)",
                    $"Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())} | Tutarlılık: {u.CanonIntegrity}/100 | Lore Bariyeri: {u.LoreBarrier}/100\n\n" +
                    "Henüz kayıtlı karakterin yok. 'Yeni Karakter Yaz' ile ilk efsaneni oluştur!",
                    new[] { "Atölyeye Dön" },
                    _ => OpenCharacterHub(_draftGamePtr, _draftGameTitle)
                );
                return;
            }

            int perPage = 3;
            int totalPages = Math.Max(1, (chars.Count + perPage - 1) / perPage);
            page = Math.Clamp(page, 0, totalPages - 1);

            var lines = new List<string>();
            lines.Add($"Sayfa {page + 1}/{totalPages} | Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())} | Bariyer: {u.LoreBarrier}/100");
            lines.Add("");

            int start = page * perPage;
            for (int i = 0; i < perPage && (start + i) < chars.Count; i++)
            {
                var c = chars[start + i];
                string fatigue = c.ConsecutiveUses >= 3 ? "YORGUN!" : $"Dinç (Seri:{c.ConsecutiveUses})";
                lines.Add($"• {c.Name} [{c.Status}] ({c.ArchetypeName}) — Pop: {c.Popularity}/100 | Oyun: {c.GamesAppeared} | {fatigue}");
            }

            var btns = new List<string>();
            if (page > 0) btns.Add("< Önceki Sayfa");
            if (page < totalPages - 1) btns.Add("Sonraki Sayfa >");
            btns.Add("Atölyeye Dön");

            NativePopupQueue.Enqueue(
                $"Karakterler ({chars.Count})",
                string.Join("\n", lines),
                btns.ToArray(),
                opt =>
                {
                    string picked = (opt >= 0 && opt < btns.Count) ? btns[opt] : "Atölyeye Dön";
                    if (picked == "< Önceki Sayfa") OpenRosterViewerPage(page - 1);
                    else if (picked == "Sonraki Sayfa >") OpenRosterViewerPage(page + 1);
                    else OpenCharacterHub(_draftGamePtr, _draftGameTitle);
                }
            );
        }

        private static void OpenReturningCharacterPage(int page)
        {
            var chars = UniverseSaveManager.Current.Characters;
            int perPage = 3;
            int totalPages = Math.Max(1, (chars.Count + perPage - 1) / perPage);
            page = Math.Clamp(page, 0, totalPages - 1);

            int start = page * perPage;
            var lines = new List<string>();
            var btns = new List<string>();
            var pageChars = new List<CharacterRecord>();

            lines.Add($"Geri çağırılacak karakteri seç (Sayfa {page + 1}/{totalPages}):");
            lines.Add("Not: Aynı karakteri üst üste 3+ oyunda kullanmak Karakter Yorgunluğu yaratır.\n");

            for (int i = 0; i < perPage && (start + i) < chars.Count; i++)
            {
                var c = chars[start + i];
                pageChars.Add(c);
                string fatigue = c.ConsecutiveUses >= 3 ? "YORGUN!" : $"Seri: {c.ConsecutiveUses}";
                lines.Add($"• {c.Name} [{c.Status}] — {c.ArchetypeName} (Pop: {c.Popularity}, {fatigue})");
                btns.Add(c.Name);
            }

            string navBtn = (page < totalPages - 1) ? "Sonraki Sayfa >" : "Atölyeye Dön";
            btns.Add(navBtn);

            NativePopupQueue.Enqueue(
                $"Eski Yıldızı Çağır: {_draftGameTitle}",
                string.Join("\n", lines),
                btns.ToArray(),
                opt =>
                {
                    if (opt >= 0 && opt < pageChars.Count)
                    {
                        var c = pageChars[opt];
                        _gameCharacters[_draftGamePtr] = new ActiveCharacterDraft
                        {
                            IsReturning = true,
                            ExistingCharacterId = c.Id,
                            Name = c.Name,
                            ArchetypeId = c.ArchetypeId,
                            Sliders = (c.Sliders != null && c.Sliders.Length == 9) ? (float[])c.Sliders.Clone() : new float[9]
                        };
                        MelonLogger.Msg($"[Faz 3] Eski karakter geri cagrildi: '{c.Name}' (Seri: {c.ConsecutiveUses})");
                    }
                    else if (navBtn == "Sonraki Sayfa >")
                    {
                        OpenReturningCharacterPage(page + 1);
                    }
                    else
                    {
                        OpenCharacterHub(_draftGamePtr, _draftGameTitle);
                    }
                }
            );
        }

        private static void OpenArchetypePage(int page)
        {
            // 10 arketip: Sayfa 0 (0,1,2 + Diger), Sayfa 1 (3,4,5 + Diger), Sayfa 2 (6,7,8,9)
            if (page == 0)
            {
                var a0 = Phase1Database.Archetypes[0];
                var a1 = Phase1Database.Archetypes[1];
                var a2 = Phase1Database.Archetypes[2];
                NativePopupQueue.Enqueue(
                    "Arketip Seç (1/3)",
                    $"Oyun türüne ve evren tonuna uyacak başrol arketipini seç:\n\n" +
                    $"• {a0.Name}: {a0.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a0.PreferredTone)})\n" +
                    $"• {a1.Name}: {a1.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a1.PreferredTone)})\n" +
                    $"• {a2.Name}: {a2.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a2.PreferredTone)})",
                    new[] { a0.Name, a1.Name, a2.Name, "Diğer Arketipler >" },
                    opt =>
                    {
                        if (opt == 3) OpenArchetypePage(1);
                        else { _draftArchetypeIdx = Math.Clamp(opt, 0, 2); OpenNamePicker(); }
                    }
                );
            }
            else if (page == 1)
            {
                var a3 = Phase1Database.Archetypes[3];
                var a4 = Phase1Database.Archetypes[4];
                var a5 = Phase1Database.Archetypes[5];
                NativePopupQueue.Enqueue(
                    "Arketip Seç (2/3)",
                    $"Oyun türüne ve evren tonuna uyacak başrol arketipini seç:\n\n" +
                    $"• {a3.Name}: {a3.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a3.PreferredTone)})\n" +
                    $"• {a4.Name}: {a4.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a4.PreferredTone)})\n" +
                    $"• {a5.Name}: {a5.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a5.PreferredTone)})",
                    new[] { a3.Name, a4.Name, a5.Name, "Son 4 Arketip >" },
                    opt =>
                    {
                        if (opt == 3) OpenArchetypePage(2);
                        else { _draftArchetypeIdx = 3 + Math.Clamp(opt, 0, 2); OpenNamePicker(); }
                    }
                );
            }
            else
            {
                var a6 = Phase1Database.Archetypes[6];
                var a7 = Phase1Database.Archetypes[7];
                var a8 = Phase1Database.Archetypes[8];
                var a9 = Phase1Database.Archetypes[9];
                NativePopupQueue.Enqueue(
                    "Arketip Seç (3/3)",
                    $"Son 4 arketip:\n\n" +
                    $"• {a6.Name}: {a6.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a6.PreferredTone)})\n" +
                    $"• {a7.Name}: {a7.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a7.PreferredTone)})\n" +
                    $"• {a8.Name}: {a8.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a8.PreferredTone)})\n" +
                    $"• {a9.Name}: {a9.ShortDesc} (Ton: {CategoryLabels.ToneToTurkish(a9.PreferredTone)})",
                    new[] { a6.Name, a7.Name, a8.Name, a9.Name },
                    opt =>
                    {
                        _draftArchetypeIdx = 6 + Math.Clamp(opt, 0, 3);
                        OpenNamePicker();
                    }
                );
            }
        }

        private static void OpenNamePicker()
        {
            var pool = Phase1Database.CharacterNamePool;
            int idx0 = (_namePage * 3) % pool.Length;
            int idx1 = (_namePage * 3 + 1) % pool.Length;
            int idx2 = (_namePage * 3 + 2) % pool.Length;
            string n0 = pool[idx0], n1 = pool[idx1], n2 = pool[idx2];
            var arch = Phase1Database.Archetypes[_draftArchetypeIdx];

            NativePopupQueue.Enqueue(
                $"Karakter Adı Seç ({arch.Name})",
                $"Seçili Arketip: {arch.Name} ({arch.ShortDesc})\n\n" +
                "Karakterine bir isim seç veya 'Başka İsimler >' ile diğer önerilere bak:",
                new[] { n0, n1, n2, "Başka İsimler >" },
                opt =>
                {
                    if (opt == 3)
                    {
                        _namePage++;
                        OpenNamePicker();
                    }
                    else
                    {
                        _draftName = opt == 1 ? n1 : (opt == 2 ? n2 : n0);
                        OpenSliderStage(0);
                    }
                }
            );
        }

        private static void OpenSliderStage(int stageIdx)
        {
            int b = stageIdx * 3;
            int v0 = _draftSlidersPct[b];
            int v1 = _draftSlidersPct[b + 1];
            int v2 = _draftSlidersPct[b + 2];
            int sum = v0 + v1 + v2;

            string[] titles =
            {
                $"1/3 Duruş Ayarları — {_draftName}",
                $"2/3 Psikoloji Ayarları — {_draftName}",
                $"3/3 Sosyallik Ayarları — {_draftName}"
            };

            string[] nextLabels =
            {
                "Sonraki: Psikoloji >",
                "Sonraki: Sosyallik >",
                "Karakteri Onayla"
            };

            var arch = Phase1Database.Archetypes[_draftArchetypeIdx];
            string body =
                $"Arketip: {arch.Name}  |  Odak Bütçesi: {sum} / {BalanceCaps.StageSliderBudgetPct}\n" +
                "Bir özelliğe tıkladıkça +%20 artar (sınırı geçince %0'a döner):\n\n" +
                $"• {Phase1Database.SliderNames[b]}: {MakeBar(v0)} %{v0} ({Phase1Database.SliderHints[b]})\n" +
                $"• {Phase1Database.SliderNames[b + 1]}: {MakeBar(v1)} %{v1} ({Phase1Database.SliderHints[b + 1]})\n" +
                $"• {Phase1Database.SliderNames[b + 2]}: {MakeBar(v2)} %{v2} ({Phase1Database.SliderHints[b + 2]})";

            NativePopupQueue.Enqueue(
                titles[stageIdx],
                body,
                new[]
                {
                    $"{Phase1Database.SliderNames[b]} (+20)",
                    $"{Phase1Database.SliderNames[b + 1]} (+20)",
                    $"{Phase1Database.SliderNames[b + 2]} (+20)",
                    nextLabels[stageIdx]
                },
                opt =>
                {
                    if (opt >= 0 && opt <= 2)
                    {
                        int target = b + opt;
                        int others = sum - _draftSlidersPct[target];
                        int nextVal = _draftSlidersPct[target] + 20;
                        if (nextVal > 100 || others + nextVal > BalanceCaps.StageSliderBudgetPct)
                            nextVal = 0;
                        _draftSlidersPct[target] = nextVal;
                        OpenSliderStage(stageIdx);
                    }
                    else if (stageIdx < 2)
                    {
                        OpenSliderStage(stageIdx + 1);
                    }
                    else
                    {
                        var floats = new float[9];
                        for (int i = 0; i < 9; i++) floats[i] = _draftSlidersPct[i] / 100f;

                        _gameCharacters[_draftGamePtr] = new ActiveCharacterDraft
                        {
                            IsReturning = false,
                            Name = _draftName,
                            ArchetypeId = arch.Id,
                            Sliders = floats
                        };
                        MelonLogger.Msg($"[Faz 3] Yeni karakter onaylandi: '{_draftName}' ({arch.Name})");
                    }
                }
            );
        }

        private static string MakeBar(int pct)
        {
            int filled = Math.Clamp(pct / 20, 0, 5);
            return "[" + new string('#', filled) + new string('-', 5 - filled) + "]";
        }
    }

    // =========================================================================
    // 6. 10X ARASTIRMA PUANI TURBO SISTEMI (KANCASIZ - TEST MODU)
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
                        "Yeni oyunlarında Evren Konumu'nun hemen ardından 10 arketip ve 3×3 kaydırıcılı (Duruş, Psikoloji, Sosyallik) Karakter Atölyesi açılacak!"
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
    // 7. ANA MOD GIRISI
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
            LoggerInstance.Msg("Universe Architect v3.0.1 (Faz 3: Native 4-Butonlu 10 Arketip + 3x3 Karakter Atolyesi) baslatiliyor...");
            UniverseSaveManager.Load();

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v301");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v3.0.1 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        public override void OnUpdate()
        {
            float dt = Time.unscaledDeltaTime;

            // Native 4-butonlu pencere kuyrugunu islet
            NativePopupQueue.Tick(dt);

            if (!TopicsInjected || !EngineFeaturesInjected)
            {
                _injectTimer += dt;
                if (_injectTimer >= 2.0f)
                {
                    _injectTimer = 0f;
                    if (!TopicsInjected) TopicInjectorHelper.TryInjectTopics();
                    if (!EngineFeaturesInjected) EngineFeatureInjectorHelper.TryInjectFromScriptableObject();
                }
            }

            if (ActiveCompany == null) return;

            _popupPollTimer += dt;
            if (_popupPollTimer >= 1.0f)
            {
                _popupPollTimer = 0f;
                try { UniverseChoicePopupSystem.PollForNewGame(ActiveCompany); } catch { }
            }

            if (ModSettings.TestMode10xResearch)
            {
                _turboTimer += dt;
                if (_turboTimer >= 0.5f)
                {
                    _turboTimer = 0f;
                    try { ResearchTurboSystem.PollAndMultiply(ActiveCompany); } catch { }
                }
            }

            if (EngineFeaturesInjected)
            {
                _companySyncTimer += dt;
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
