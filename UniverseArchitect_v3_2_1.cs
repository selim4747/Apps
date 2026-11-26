using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppInterop.Runtime;
using Il2CppRarebyte.GDT;

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "3.2.1", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    // =========================================================================
    // 1. TEST AYARLARI, VERI MODELLERI, 4 ROL, 10 ARKETIP & 3x3 KAYDIRICI
    // =========================================================================
    public static class ModSettings
    {
        public static bool TestMode10xResearch = true;
        public const double ResearchMultiplier = 10.0;
        public const int TestStarterResearchBoost = 100;
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

    public enum CharacterRole
    {
        Protagonist,    // Basrol
        Villain,        // Kotu Adam
        Sidekick,       // Yardimci
        Minor           // Herhangi Bir Adam
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

        public static string RoleToTurkish(CharacterRole role)
        {
            switch (role)
            {
                case CharacterRole.Villain:  return "Kötü Adam";
                case CharacterRole.Sidekick: return "Yardımcı";
                case CharacterRole.Minor:    return "Herhangi Bir Adam";
                default:                     return "Başrol";
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
        public string LastRole { get; set; } = "Başrol";
        public float[] Sliders { get; set; } = new float[9];
        public string DebutGame { get; set; }
        public string Status { get; set; } = "Normal"; // Yildiz, Kult, Sevilen, Normal, Fiyasko
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
        public CharacterRole Role = CharacterRole.Protagonist;
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
        public List<string> CastSummary { get; set; } = new List<string>();
        public List<string> UsedModFeatures { get; set; } = new List<string>();
        public string RecordedAtUtc { get; set; }
    }

    public class UniverseSaveData
    {
        public string CompanyUid { get; set; } = "";
        public string CompanyName { get; set; } = "";
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
                if (string.Equals(Archetypes[i].Id, id, StringComparison.OrdinalIgnoreCase)) return Archetypes[i];
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
    // 2. KAYIT SLOTU BAZLI JSON EVREN & KARAKTER KAYIT SISTEMI (company.uid)
    // =========================================================================
    public static class UniverseSaveManager
    {
        public static UniverseSaveData Current { get; private set; } = new UniverseSaveData();
        private static string _activeSaveKey = "";

        private static string LegacySavePath => Path.Combine(Application.persistentDataPath, "UniverseArchitect_Save.json");
        private static string ActiveSavePath => string.IsNullOrEmpty(_activeSaveKey)
            ? LegacySavePath
            : Path.Combine(Application.persistentDataPath, $"UniverseArchitect_Save_{_activeSaveKey}.json");

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

        private static string SanitizeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            var sb = new StringBuilder(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                char ch = raw[i];
                if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '-')
                    sb.Append(ch);
            }
            string res = sb.ToString();
            return res.Length > 36 ? res.Substring(0, 36) : res;
        }

        public static void LoadForCompany(Company company)
        {
            try
            {
                string uid = company != null ? SanitizeKey(company.uid) : "";
                if (string.IsNullOrEmpty(uid) && company != null) uid = SanitizeKey(company.slot);
                if (string.IsNullOrEmpty(uid) && company != null) uid = SanitizeKey(company.name);

                _activeSaveKey = uid;
                string targetPath = ActiveSavePath;

                // 1. Bu kayit slotuna ait ozel JSON varsa yukle
                if (File.Exists(targetPath))
                {
                    if (TryLoadFromPath(targetPath))
                    {
                        if (company != null)
                        {
                            Current.CompanyUid = company.uid ?? "";
                            Current.CompanyName = company.name ?? "";
                        }
                        MelonLogger.Msg($"[Universe Architect] Evren kaydi yuklendi [{_activeSaveKey}]! ({Current.ReleasedGames.Count} oyun, {Current.Characters.Count} karakter, Tutarlilik: {Current.CanonIntegrity}/100)");
                        return;
                    }
                }

                // 2. Eski bir kayit (onceden yayinlanmis oyunlari var) ise ve eski tekil JSON mevcutsa bir kerelik tasi
                bool isExistingCareer = company != null && ((company.gameLog != null && company.gameLog.Count > 0) || company.currentWeek >= 12.0);
                if (isExistingCareer && !string.IsNullOrEmpty(_activeSaveKey) && File.Exists(LegacySavePath))
                {
                    if (TryLoadFromPath(LegacySavePath))
                    {
                        Current.CompanyUid = company.uid ?? "";
                        Current.CompanyName = company.name ?? "";
                        Save();
                        MelonLogger.Msg($"[Universe Architect] Eski evren kaydi bu sirkete tasindi [{_activeSaveKey}]! ({Current.ReleasedGames.Count} oyun, {Current.Characters.Count} karakter)");
                        return;
                    }
                }

                // 3. Yepyeni oyun/sirket -> tertemiz evrenle basla!
                Current = new UniverseSaveData
                {
                    CompanyUid = company != null ? (company.uid ?? "") : "",
                    CompanyName = company != null ? (company.name ?? "") : ""
                };
                Save();
                MelonLogger.Msg($"[Universe Architect] Yeni evren kaydi baslatildi [{(string.IsNullOrEmpty(_activeSaveKey) ? "default" : _activeSaveKey)}] (Sifir tarihce, Tutarlilik: 100/100).");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] JSON yukleme hatasi: {ex.Message}");
            }
        }

        private static bool TryLoadFromPath(string path)
        {
            string json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<UniverseSaveData>(json);
            if (loaded == null) return false;
            Current = loaded;
            if (Current.Characters == null) Current.Characters = new List<CharacterRecord>();
            if (Current.ReleasedGames == null) Current.ReleasedGames = new List<ReleasedGameRecord>();
            Current.CanonIntegrity = Math.Clamp(Current.CanonIntegrity, 0, 100);
            Current.LoreBarrier = Math.Clamp(Current.LoreBarrier, 0, 100);
            return true;
        }

        public static void Save()
        {
            try
            {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(ActiveSavePath, JsonSerializer.Serialize(Current, opts));
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] JSON kaydetme hatasi: {ex.Message}");
            }
        }

        public static void AdjustIntegrity(int delta) => Current.CanonIntegrity = Math.Clamp(Current.CanonIntegrity + delta, 0, 100);
        public static void AdjustLoreBarrier(int delta) => Current.LoreBarrier = Math.Clamp(Current.LoreBarrier + delta, 0, 100);

        public static CharacterRecord FindCharacterById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Current.Characters.Count; i++)
                if (Current.Characters[i].Id == id) return Current.Characters[i];
            return null;
        }

        public static void RestUnusedCharacters(HashSet<string> usedCharIds)
        {
            for (int i = 0; i < Current.Characters.Count; i++)
            {
                if (usedCharIds == null || !usedCharIds.Contains(Current.Characters[i].Id))
                    Current.Characters[i].ConsecutiveUses = 0;
            }
        }

        public static void RecordReleasedGame(Game game, string topicId, double score, ProjectCategory category, List<string> modFeatures, List<string> castSummary)
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
                CharacterName = (castSummary != null && castSummary.Count > 0) ? string.Join(", ", castSummary) : "",
                CastSummary = castSummary ?? new List<string>(),
                UsedModFeatures = modFeatures ?? new List<string>(),
                RecordedAtUtc = DateTime.UtcNow.ToString("o")
            });
            Save();
        }
    }

    // =========================================================================
    // 3. RESEARCH SABLON REFLECTION & BILDIRIM YARDIMCILARI
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
    // 4. NATIVE 4-BUTONLU PENCERE KUYRUGU (EVREN KONUMU & EVREN TONU ICIN)
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
            _nextPopup = new PendingPopup { Header = header, Text = text, Buttons = buttons, OnSelected = onSelected };
            _delayTimer = 0.12f;
        }

        public static void Clear()
        {
            _nextPopup = null;
            _delayTimer = 0f;
        }

        public static void Tick(float dt)
        {
            if (_nextPopup == null) return;
            if (_delayTimer > 0f) { _delayTimer -= dt; return; }

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
                    catch (Exception cbEx) { MelonLogger.Warning($"[Popup] Callback hatasi: {cbEx.Message}"); }
                };

                var del = DelegateSupport.ConvertDelegate<NotificationButton.NotificationButtonDelegate>(onClickAction);
                panel.ShowGenericNotification(notif, del, null);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Popup] Pencere gosterim hatasi: {ex.Message}");
                _nextPopup = null;
            }
        }
    }

    // =========================================================================
    // 5. GERCEK UNITY UI (uGUI): NATIVE PAUSE + SIFIR LAYOUTGROUP + UnityEngine.UI.Slider
    // =========================================================================
    public static class CharacterWorkshopUGUI
    {
        public static bool IsOpen { get; private set; } = false;

        // Renk paleti (Game Dev Tycoon mor / krem / sari)
        private static readonly Color ColDim       = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color ColPanel     = new Color(0.98f, 0.96f, 0.90f, 1f);
        private static readonly Color ColHeader    = new Color(0.47f, 0.29f, 0.52f, 1f);
        private static readonly Color ColCard      = new Color(0.92f, 0.89f, 0.80f, 1f);
        private static readonly Color ColYellow    = new Color(0.96f, 0.80f, 0.22f, 1f);
        private static readonly Color ColGreen     = new Color(0.30f, 0.68f, 0.36f, 1f);
        private static readonly Color ColRed       = new Color(0.82f, 0.30f, 0.28f, 1f);
        private static readonly Color ColDarkText  = new Color(0.16f, 0.13f, 0.18f, 1f);
        private static readonly Color ColMutedText = new Color(0.38f, 0.34f, 0.36f, 1f);
        private static readonly Color ColTrack     = new Color(0.76f, 0.72f, 0.65f, 1f);

        private static Font _font;

        // Oyunun kendi en ust Canvas'ina baglanan kok & HUD butonu
        private static GameObject _rootObj;
        private static GameObject _hudBtnObj;
        private static UnityEngine.UI.Text _hudBtnText;
        private static GameObject _modalRoot;
        private static RectTransform _boxRt;
        private static UnityEngine.UI.Text _headerTitle;
        private static Transform _contentRoot;
        private static float _uiScale = 1f;
        private static bool _pausedViaGameManager = false;

        // Aktif calisma durumu
        private static bool _isRosterOnlyMode = false;
        private static IntPtr _targetGamePtr = IntPtr.Zero;
        private static string _targetGameTitle = "";
        private static List<ActiveCharacterDraft> _workingCast = new List<ActiveCharacterDraft>();

        // Yeni karakter formu
        private static string _draftName = "Afor";
        private static CharacterRole _draftRole = CharacterRole.Protagonist;
        private static int _draftArchetypeIdx = 0;
        private static int _sliderStage = 0; // 0: Durus, 1: Psikoloji, 2: Sosyallik
        private static float[] _draftSliders = new float[] { 0.6f, 0.6f, 0.4f, 0.6f, 0.6f, 0.4f, 0.6f, 0.4f, 0.6f };
        private static CharacterRole _returningRole = CharacterRole.Protagonist;
        private static int _namePoolIdx = 0;
        private static bool _updatingSliders = false;

        // Canli kaydirici referanslari (tam tip: UnityEngine.UI.Slider)
        private static UnityEngine.UI.Slider[] _activeSliders = new UnityEngine.UI.Slider[3];
        private static UnityEngine.UI.Text[] _activeSliderPctTexts = new UnityEngine.UI.Text[3];
        private static UnityEngine.UI.Text _activeBudgetText;
        private static UnityEngine.UI.InputField _activeNameInput;
        private static UnityEngine.UI.Text _fallbackNameText;

        private static Canvas FindTopRootCanvasFrom(Transform start)
        {
            Canvas highest = null;
            Transform t = start;
            while (t != null)
            {
                var c = t.GetComponent<Canvas>();
                if (c != null) highest = c;
                t = t.parent;
            }
            return highest;
        }

        private static Canvas FindGameTopCanvas()
        {
            try
            {
                var panel = UnityEngine.Object.FindObjectOfType<PanelNotifications>();
                if (panel != null)
                {
                    var rootC = FindTopRootCanvasFrom(panel.transform);
                    if (rootC != null) return rootC;
                }
            }
            catch { }

            try
            {
                var all = UnityEngine.Object.FindObjectsOfType<Canvas>();
                Canvas best = null;
                int bestOrder = int.MinValue;
                if (all != null)
                {
                    for (int i = 0; i < all.Length; i++)
                    {
                        var c = all[i];
                        if (c == null || !c.gameObject.activeInHierarchy) continue;
                        var rootC = FindTopRootCanvasFrom(c.transform) ?? c;
                        int ord = 0;
                        try { ord = rootC.sortingOrder; } catch { }
                        if (best == null || ord >= bestOrder)
                        {
                            best = rootC;
                            bestOrder = ord;
                        }
                    }
                }
                return best;
            }
            catch { }

            return null;
        }

        public static void EnsureHUD()
        {
            if (UniverseArchitectMod.ActiveCompany == null)
            {
                SetHudVisible(false);
                return;
            }

            if (_modalRoot != null && _rootObj != null)
            {
                UpdateHudState();
                return;
            }

            try
            {
                InitAssets();

                var hostCanvas = FindGameTopCanvas();
                if (hostCanvas == null) return;

                _uiScale = 1f;
                float cw = 1280f, ch = 720f;
                try
                {
                    var hostRt = hostCanvas.GetComponent<RectTransform>();
                    if (hostRt != null)
                    {
                        var r = hostRt.rect;
                        if (r.width > 100f && r.height > 100f)
                        {
                            cw = r.width;
                            ch = r.height;
                            _uiScale = Mathf.Min((cw * 0.94f) / 1120f, (ch * 0.92f) / 650f);
                        }
                    }
                }
                catch { }

                _rootObj = new GameObject("NJG_UniverseArchitect_Root");
                var rootRt = _rootObj.AddComponent<RectTransform>();
                _rootObj.transform.SetParent(hostCanvas.transform, false);
                StretchFull(rootRt);
                try { _rootObj.transform.SetAsLastSibling(); } catch { }

                // Sag alt "Karakterler" HUD butonu (ust bar ve satis kartlariyla cakismaz)
                var hudBtn = CreateButton(_rootObj.transform, "Karakterler (0)", ColYellow, ColDarkText, 18, () =>
                {
                    if (!IsOpen) OpenRosterOnly();
                });
                _hudBtnObj = hudBtn.gameObject;
                _hudBtnText = hudBtn.GetComponentInChildren<UnityEngine.UI.Text>();
                var hudRt = hudBtn.GetComponent<RectTransform>();
                hudRt.anchorMin = new Vector2(1f, 0f);
                hudRt.anchorMax = new Vector2(1f, 0f);
                hudRt.pivot = new Vector2(1f, 0f);
                hudRt.anchoredPosition = new Vector2(-18f * _uiScale, 18f * _uiScale);
                hudRt.sizeDelta = new Vector2(215f, 48f);
                try { hudRt.localScale = new Vector3(_uiScale, _uiScale, 1f); } catch { }

                // Modal cerceve (tam ekran koyu perde + ortali olcekli kutu)
                _modalRoot = CreatePanel(_rootObj.transform, ColDim);
                StretchFull(_modalRoot.GetComponent<RectTransform>());

                var box = CreatePanel(_modalRoot.transform, ColPanel);
                _boxRt = box.GetComponent<RectTransform>();
                _boxRt.anchorMin = new Vector2(0.5f, 0.5f);
                _boxRt.anchorMax = new Vector2(0.5f, 0.5f);
                _boxRt.pivot = new Vector2(0.5f, 0.5f);
                _boxRt.anchoredPosition = Vector2.zero;
                _boxRt.sizeDelta = new Vector2(1120f, 650f);
                try { _boxRt.localScale = new Vector3(_uiScale, _uiScale, 1f); } catch { }

                var header = CreatePanel(box.transform, ColHeader);
                var hRt = header.GetComponent<RectTransform>();
                hRt.anchorMin = new Vector2(0f, 1f);
                hRt.anchorMax = new Vector2(1f, 1f);
                hRt.pivot = new Vector2(0.5f, 1f);
                hRt.anchoredPosition = Vector2.zero;
                hRt.sizeDelta = new Vector2(0f, 56f);

                _headerTitle = CreateText(header.transform, "Karakter Atölyesi", 24, Color.white, TextAnchor.MiddleLeft, true);
                var htRt = _headerTitle.GetComponent<RectTransform>();
                StretchFull(htRt);
                htRt.offsetMin = new Vector2(22f, 0f);
                htRt.offsetMax = new Vector2(-22f, 0f);

                var body = new GameObject("ContentRoot");
                var bodyRt = body.AddComponent<RectTransform>();
                body.transform.SetParent(box.transform, false);
                bodyRt.anchorMin = Vector2.zero;
                bodyRt.anchorMax = Vector2.one;
                bodyRt.offsetMin = new Vector2(18f, 16f);
                bodyRt.offsetMax = new Vector2(-18f, -66f);
                _contentRoot = body.transform;

                _modalRoot.SetActive(false);
                UpdateHudState();
                string fName = _font != null ? _font.name : "NULL";
                MelonLogger.Msg($"[Faz 3.2] uGUI Karakter Atolyesi & HUD kuruldu! (Canvas: '{hostCanvas.name}', Boyut: {cw:F0}x{ch:F0}, Olcek: {_uiScale:F2}, Font: '{fName}')");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 3.2] uGUI kurulum hatasi: {ex}");
                if (_rootObj != null) { try { UnityEngine.Object.Destroy(_rootObj); } catch { } }
                _rootObj = null;
                _modalRoot = null;
                _hudBtnObj = null;
                _hudBtnText = null;
            }
        }

        public static void SetHudVisible(bool visible)
        {
            if (_hudBtnObj != null)
            {
                try { _hudBtnObj.SetActive(visible); } catch { }
            }
        }

        public static void UpdateHudState()
        {
            if (_hudBtnText != null)
                _hudBtnText.text = $"Karakterler ({UniverseSaveManager.Current.Characters.Count})";

            bool showBtn = !IsOpen && UniverseArchitectMod.IsGameplayReadyForHud();
            SetHudVisible(showBtn);
        }

        private static void PauseGameSimulation()
        {
            _pausedViaGameManager = false;
            try
            {
                var gm = UniverseArchitectMod.GetGameManager();
                if (gm != null)
                {
                    gm.pause(true, false);
                    _pausedViaGameManager = true;
                    return;
                }
            }
            catch { }

            try { Time.timeScale = 0f; } catch { }
        }

        private static void ResumeGameSimulation()
        {
            if (_pausedViaGameManager)
            {
                _pausedViaGameManager = false;
                try
                {
                    var gm = UniverseArchitectMod.GetGameManager();
                    if (gm != null) gm.resume(true, false);
                }
                catch { }
            }
            try { Time.timeScale = 1f; } catch { }
        }

        private static void BringToFront()
        {
            try { if (_rootObj != null) _rootObj.transform.SetAsLastSibling(); } catch { }
            try { if (_modalRoot != null) _modalRoot.transform.SetAsLastSibling(); } catch { }
        }

        public static void OpenCastManager(IntPtr gamePtr, string gameTitle)
        {
            EnsureHUD();
            if (_modalRoot == null) return;

            try
            {
                _isRosterOnlyMode = false;
                _targetGamePtr = gamePtr;
                _targetGameTitle = string.IsNullOrEmpty(gameTitle) ? "Yeni Oyun" : gameTitle;
                _workingCast = new List<ActiveCharacterDraft>();

                IsOpen = true;
                _modalRoot.SetActive(true);
                BringToFront();
                UpdateHudState();
                PauseGameSimulation();

                ShowCastOverview();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 3.2] OpenCastManager hatasi (oyun kurtarildi): {ex}");
                CloseModal();
            }
        }

        public static void OpenRosterOnly()
        {
            EnsureHUD();
            if (_modalRoot == null) return;

            try
            {
                _isRosterOnlyMode = true;
                IsOpen = true;
                _modalRoot.SetActive(true);
                BringToFront();
                UpdateHudState();
                PauseGameSimulation();

                ShowFullRosterView();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 3.2] OpenRosterOnly hatasi (oyun kurtarildi): {ex}");
                CloseModal();
            }
        }

        public static void CloseModal()
        {
            IsOpen = false;
            if (_modalRoot != null)
            {
                try { _modalRoot.SetActive(false); } catch { }
            }
            ResumeGameSimulation();
            UpdateHudState();
        }

        private static void ClearContent()
        {
            if (_contentRoot == null) return;
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_contentRoot.GetChild(i).gameObject);
        }

        // ---------------------------------------------------------------------
        // GORUNUM 1: OYUNUN KARAKTER KADROSU (COKLU KARAKTER EKLEME MERKEZI)
        // ---------------------------------------------------------------------
        private static void ShowCastOverview()
        {
            try
            {
                ClearContent();
                _headerTitle.text = $"Oyun Kadrosu — {_targetGameTitle} ({_workingCast.Count} Karakter)";

                var u = UniverseSaveManager.Current;
                var infoCard = CreatePanel(_contentRoot, ColCard);
                PlaceRect(infoCard.GetComponent<RectTransform>(), 0f, 0f, 1084f, 52f);
                var infoTxt = CreateText(infoCard.transform,
                    $"Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())}  |  Tutarlılık: {u.CanonIntegrity}/100  |  Lore Bariyeri: {u.LoreBarrier}/100  |  Ara Evren Serisi: {u.ConsecutiveSideProjects}",
                    18, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(infoTxt.GetComponent<RectTransform>(), 16f, 0f);

                // Ust aksiyon butonlari
                var btnNew = CreateButton(_contentRoot, "+ Yeni Karakter Yaz", ColGreen, Color.white, 19, () =>
                {
                    _draftName = Phase1Database.CharacterNamePool[_namePoolIdx % Phase1Database.CharacterNamePool.Length];
                    _namePoolIdx++;
                    _draftRole = _workingCast.Count == 0 ? CharacterRole.Protagonist : CharacterRole.Villain;
                    _draftArchetypeIdx = 0;
                    _sliderStage = 0;
                    _draftSliders = new float[] { 0.6f, 0.6f, 0.4f, 0.6f, 0.6f, 0.4f, 0.6f, 0.4f, 0.6f };
                    ShowCreateCharacterView();
                });
                PlaceRect(btnNew.GetComponent<RectTransform>(), 0f, 64f, 340f, 50f);

                int savedCount = u.Characters.Count;
                var btnReturn = CreateButton(_contentRoot, $"+ Eski Yıldız Çağır ({savedCount})", savedCount > 0 ? ColYellow : ColTrack, ColDarkText, 19, () =>
                {
                    if (savedCount > 0) ShowReturningPickerView();
                });
                PlaceRect(btnReturn.GetComponent<RectTransform>(), 356f, 64f, 356f, 50f);

                var btnRoster = CreateButton(_contentRoot, $"Tüm Karakterler ({savedCount})", ColYellow, ColDarkText, 19, () => ShowFullRosterView());
                PlaceRect(btnRoster.GetComponent<RectTransform>(), 728f, 64f, 356f, 50f);

                // Bu oyuna eklenen karakterlerin manuel konumlandirilmis kaydirilabilir listesi
                var scroll = CreateScrollView(_contentRoot, 0f, 126f, 1084f, 368f);
                var listContent = scroll.content;
                float innerW = 1068f;
                float yCursor = 0f;

                if (_workingCast.Count == 0)
                {
                    var emptyCard = CreatePanel(listContent, ColCard);
                    PlaceRect(emptyCard.GetComponent<RectTransform>(), 0f, 0f, innerW, 120f);
                    var txt = CreateText(emptyCard.transform,
                        "Bu oyun için henüz karakter eklemedin.\n\nYukarıdan Başrol, Kötü Adam, Yardımcı veya Herhangi Bir Adam ekleyebilir ya da doğrudan 'Karaktersiz Devam Et' diyebilirsin.",
                        19, ColMutedText, TextAnchor.MiddleCenter, false);
                    StretchWithPad(txt.GetComponent<RectTransform>(), 20f, 10f);
                    yCursor = 128f;
                }
                else
                {
                    for (int i = 0; i < _workingCast.Count; i++)
                    {
                        int capturedIdx = i;
                        var d = _workingCast[i];
                        var arch = Phase1Database.FindArchetype(d.ArchetypeId);

                        var card = CreatePanel(listContent, ColCard);
                        PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 78f);

                        string tag = d.IsReturning ? "Eski Yıldız" : "Yeni";
                        var lbl = CreateText(card.transform,
                            $"[{CategoryLabels.RoleToTurkish(d.Role)}]  {d.Name}  —  {arch.Name} ({tag})\n" +
                            $"Duruş: Kavg %{d.Sliders[0]*100:F0} / San.Diy %{d.Sliders[1]*100:F0} / Zek %{d.Sliders[2]*100:F0}  |  Sosyallik: Çev %{d.Sliders[6]*100:F0} / Ego %{d.Sliders[7]*100:F0} / Kap %{d.Sliders[8]*100:F0}",
                            16, ColDarkText, TextAnchor.MiddleLeft, false);
                        var lrt = lbl.GetComponent<RectTransform>();
                        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                        lrt.offsetMin = new Vector2(16f, 4f); lrt.offsetMax = new Vector2(-140f, -4f);

                        var delBtn = CreateButton(card.transform, "Çıkar", ColRed, Color.white, 17, () =>
                        {
                            _workingCast.RemoveAt(capturedIdx);
                            ShowCastOverview();
                        });
                        var drt = delBtn.GetComponent<RectTransform>();
                        drt.anchorMin = new Vector2(1f, 0.5f); drt.anchorMax = new Vector2(1f, 0.5f);
                        drt.pivot = new Vector2(1f, 0.5f);
                        drt.anchoredPosition = new Vector2(-14f, 0f);
                        drt.sizeDelta = new Vector2(110f, 46f);

                        yCursor += 88f;
                    }
                }
                listContent.sizeDelta = new Vector2(0f, Mathf.Max(352f, yCursor));

                // Alt onay butonu
                string confirmLabel = _workingCast.Count > 0
                    ? $"Kadroyu Onayla ({_workingCast.Count} Karakter) ve Oyuna Başla"
                    : "Karaktersiz Devam Et";
                var btnConfirm = CreateButton(_contentRoot, confirmLabel, ColGreen, Color.white, 21, () =>
                {
                    UniverseChoicePopupSystem.SetCastForGame(_targetGamePtr, new List<ActiveCharacterDraft>(_workingCast));
                    MelonLogger.Msg($"[Faz 3.2] '{_targetGameTitle}' kadrosu onaylandi ({_workingCast.Count} karakter).");
                    CloseModal();
                });
                PlaceRect(btnConfirm.GetComponent<RectTransform>(), 0f, 506f, 1084f, 56f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 3.2] ShowCastOverview hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // GORUNUM 2: YENI KARAKTER YAZ (MANUEL 2-SUTUNLU ARKETIP + SURUKLEMELI 3x3 SLIDER)
        // ---------------------------------------------------------------------
        private static void ShowCreateCharacterView()
        {
            try
            {
                ClearContent();
                _headerTitle.text = $"Yeni Karakter Yaz — {_targetGameTitle}";
                _activeNameInput = null;
                _fallbackNameText = null;

                // SOL SUTUN (0..520): Isim, 4 Rol, Kaydirilabilir 2-Sutunlu Arketip Izgarasi
                var lblName = CreateText(_contentRoot, "İsim:", 18, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblName.GetComponent<RectTransform>(), 0f, 4f, 65f, 42f);

                _activeNameInput = CreateInputFieldSafe(_contentRoot, _draftName, "Karakter adı yaz...", val => _draftName = val, out _fallbackNameText);
                if (_activeNameInput != null)
                    PlaceRect(_activeNameInput.GetComponent<RectTransform>(), 70f, 4f, 305f, 42f);
                else if (_fallbackNameText != null)
                    PlaceRect(_fallbackNameText.transform.parent.GetComponent<RectTransform>(), 70f, 4f, 305f, 42f);

                var btnRand = CreateButton(_contentRoot, "Rastgele", ColYellow, ColDarkText, 16, () =>
                {
                    _draftName = Phase1Database.CharacterNamePool[_namePoolIdx % Phase1Database.CharacterNamePool.Length];
                    _namePoolIdx++;
                    if (_activeNameInput != null) _activeNameInput.text = _draftName;
                    if (_fallbackNameText != null) _fallbackNameText.text = _draftName;
                });
                PlaceRect(btnRand.GetComponent<RectTransform>(), 385f, 4f, 130f, 42f);

                // 4 Rol secimi
                var lblRole = CreateText(_contentRoot, "Rol Seç:", 17, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblRole.GetComponent<RectTransform>(), 0f, 54f, 515f, 26f);

                CharacterRole[] roles = { CharacterRole.Protagonist, CharacterRole.Villain, CharacterRole.Sidekick, CharacterRole.Minor };
                string[] roleShort = { "Başrol", "Kötü Adam", "Yardımcı", "Herhangi Biri" };
                for (int r = 0; r < 4; r++)
                {
                    CharacterRole capRole = roles[r];
                    bool isSel = (_draftRole == capRole);
                    var rb = CreateButton(_contentRoot, roleShort[r], isSel ? ColGreen : ColYellow, isSel ? Color.white : ColDarkText, 15, () =>
                    {
                        _draftRole = capRole;
                        ShowCreateCharacterView();
                    });
                    PlaceRect(rb.GetComponent<RectTransform>(), r * 130f, 82f, 124f, 40f);
                }

                // Kaydirilabilir Arketip Izgarasi (GridLayoutGroup olmadan %100 manuel)
                var lblArch = CreateText(_contentRoot, $"Arketip Seç (Kaydırılabilir — {Phase1Database.Archetypes.Count} Arketip):", 17, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblArch.GetComponent<RectTransform>(), 0f, 130f, 515f, 28f);

                var archScroll = CreateScrollView(_contentRoot, 0f, 160f, 515f, 336f);
                float cellW = 244f;
                float cellH = 76f;
                float gapX = 10f;
                float gapY = 10f;

                for (int i = 0; i < Phase1Database.Archetypes.Count; i++)
                {
                    int capIdx = i;
                    int col = i % 2;
                    int row = i / 2;
                    var a = Phase1Database.Archetypes[i];
                    bool sel = (_draftArchetypeIdx == i);
                    var ab = CreateButton(archScroll.content, $"{a.Name}\n{a.ShortDesc}", sel ? ColGreen : ColYellow, sel ? Color.white : ColDarkText, 14, () =>
                    {
                        _draftArchetypeIdx = capIdx;
                        ShowCreateCharacterView();
                    });
                    PlaceRect(ab.GetComponent<RectTransform>(), col * (cellW + gapX), row * (cellH + gapY), cellW, cellH);
                    try
                    {
                        var txt = ab.GetComponentInChildren<UnityEngine.UI.Text>();
                        if (txt != null) txt.lineSpacing = 1.05f;
                    }
                    catch { }
                }
                int totalRows = (Phase1Database.Archetypes.Count + 1) / 2;
                archScroll.content.sizeDelta = new Vector2(0f, Mathf.Max(320f, totalRows * (cellH + gapY)));

                // SAG SUTUN (538..1084): 3 Asamali Suruklemeli Kaydiricilar (Durus / Psikoloji / Sosyallik)
                float rx = 538f;
                float rw = 546f;

                string[] stageTitles = { "1. Duruş", "2. Psikoloji", "3. Sosyallik" };
                for (int s = 0; s < 3; s++)
                {
                    int capS = s;
                    bool sel = (_sliderStage == s);
                    var sb = CreateButton(_contentRoot, stageTitles[s], sel ? ColGreen : ColYellow, sel ? Color.white : ColDarkText, 18, () =>
                    {
                        _sliderStage = capS;
                        ShowCreateCharacterView();
                    });
                    PlaceRect(sb.GetComponent<RectTransform>(), rx + s * 184f, 4f, 178f, 44f);
                }

                var selArch = Phase1Database.Archetypes[_draftArchetypeIdx];
                var archInfo = CreatePanel(_contentRoot, ColCard);
                PlaceRect(archInfo.GetComponent<RectTransform>(), rx, 56f, rw, 54f);
                var archInfoTxt = CreateText(archInfo.transform,
                    $"Seçili: {selArch.Name} — Uyumlu Ton: {CategoryLabels.ToneToTurkish(selArch.PreferredTone)}",
                    16, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(archInfoTxt.GetComponent<RectTransform>(), 12f, 4f);

                int baseIdx = _sliderStage * 3;
                float curSum = _draftSliders[baseIdx] + _draftSliders[baseIdx + 1] + _draftSliders[baseIdx + 2];
                _activeBudgetText = CreateText(_contentRoot,
                    $"Odak Bütçesi (Sürükleyerek Ayarla): {Mathf.RoundToInt(curSum * 100f)} / 180",
                    17, ColHeader, TextAnchor.MiddleLeft, true);
                PlaceRect(_activeBudgetText.GetComponent<RectTransform>(), rx, 118f, rw, 30f);

                for (int i = 0; i < 3; i++)
                {
                    int localI = i;
                    int sIdx = baseIdx + i;
                    float cy = 156f + i * 112f;

                    var card = CreatePanel(_contentRoot, ColCard);
                    PlaceRect(card.GetComponent<RectTransform>(), rx, cy, rw, 102f);

                    var pctTxt = CreateText(card.transform, $"{Phase1Database.SliderNames[sIdx]}: %{Mathf.RoundToInt(_draftSliders[sIdx] * 100f)}",
                        19, ColDarkText, TextAnchor.MiddleLeft, true);
                    PlaceRect(pctTxt.GetComponent<RectTransform>(), 14f, 6f, rw - 28f, 28f);
                    _activeSliderPctTexts[localI] = pctTxt;

                    var hintTxt = CreateText(card.transform, Phase1Database.SliderHints[sIdx],
                        14, ColMutedText, TextAnchor.MiddleLeft, false);
                    PlaceRect(hintTxt.GetComponent<RectTransform>(), 14f, 34f, rw - 28f, 22f);

                    var slider = CreateSlider(card.transform, _draftSliders[sIdx], val =>
                    {
                        if (_updatingSliders) return;
                        _updatingSliders = true;
                        try
                        {
                            float others = 0f;
                            for (int k = 0; k < 3; k++)
                                if (baseIdx + k != sIdx) others += _draftSliders[baseIdx + k];

                            float maxAllowed = Mathf.Clamp01(BalanceCaps.StageSliderBudget - others);
                            float clamped = Mathf.Min(Mathf.Clamp01(val), maxAllowed);
                            _draftSliders[sIdx] = clamped;

                            if (_activeSliders[localI] != null && Math.Abs(_activeSliders[localI].value - clamped) > 0.001f)
                            {
                                try { _activeSliders[localI].SetValueWithoutNotify(clamped); }
                                catch { _activeSliders[localI].value = clamped; }
                            }

                            if (_activeSliderPctTexts[localI] != null)
                                _activeSliderPctTexts[localI].text = $"{Phase1Database.SliderNames[sIdx]}: %{Mathf.RoundToInt(clamped * 100f)}";

                            float newSum = _draftSliders[baseIdx] + _draftSliders[baseIdx + 1] + _draftSliders[baseIdx + 2];
                            if (_activeBudgetText != null)
                                _activeBudgetText.text = $"Odak Bütçesi (Sürükleyerek Ayarla): {Mathf.RoundToInt(newSum * 100f)} / 180";
                        }
                        finally { _updatingSliders = false; }
                    });
                    PlaceRect(slider.GetComponent<RectTransform>(), 18f, 64f, rw - 36f, 28f);
                    _activeSliders[localI] = slider;
                }

                // ALT BUTONLAR
                var btnBack = CreateButton(_contentRoot, "< Kadroya Dön", ColYellow, ColDarkText, 19, () => ShowCastOverview());
                PlaceRect(btnBack.GetComponent<RectTransform>(), 0f, 508f, 260f, 52f);

                var btnAdd = CreateButton(_contentRoot, "Karakteri Kadroya Ekle", ColGreen, Color.white, 20, () =>
                {
                    var arch = Phase1Database.Archetypes[_draftArchetypeIdx];
                    string finalName = (_activeNameInput != null && !string.IsNullOrWhiteSpace(_activeNameInput.text))
                        ? _activeNameInput.text.Trim()
                        : (string.IsNullOrWhiteSpace(_draftName) ? "Adsız Karakter" : _draftName.Trim());

                    _workingCast.Add(new ActiveCharacterDraft
                    {
                        IsReturning = false,
                        Name = finalName,
                        ArchetypeId = arch.Id,
                        Role = _draftRole,
                        Sliders = (float[])_draftSliders.Clone()
                    });
                    ShowCastOverview();
                });
                PlaceRect(btnAdd.GetComponent<RectTransform>(), 538f, 508f, 546f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 3.2] ShowCreateCharacterView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // GORUNUM 3: ESKI YILDIZ CAGIR (ROL SEC + KAYDIRILABILIR LISTE)
        // ---------------------------------------------------------------------
        private static void ShowReturningPickerView()
        {
            try
            {
                ClearContent();
                _headerTitle.text = $"Eski Yıldız Çağır — {_targetGameTitle}";

                var lbl = CreateText(_contentRoot, "Çağrılacak Karakterin Bu Oyundaki Rolü:", 18, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lbl.GetComponent<RectTransform>(), 0f, 4f, 420f, 40f);

                CharacterRole[] roles = { CharacterRole.Protagonist, CharacterRole.Villain, CharacterRole.Sidekick, CharacterRole.Minor };
                string[] roleShort = { "Başrol", "Kötü Adam", "Yardımcı", "Herhangi Biri" };
                for (int r = 0; r < 4; r++)
                {
                    CharacterRole capR = roles[r];
                    bool sel = (_returningRole == capR);
                    var rb = CreateButton(_contentRoot, roleShort[r], sel ? ColGreen : ColYellow, sel ? Color.white : ColDarkText, 16, () =>
                    {
                        _returningRole = capR;
                        ShowReturningPickerView();
                    });
                    PlaceRect(rb.GetComponent<RectTransform>(), 430f + r * 165f, 4f, 158f, 42f);
                }

                var scroll = CreateScrollView(_contentRoot, 0f, 58f, 1084f, 438f);
                var chars = UniverseSaveManager.Current.Characters;
                float innerW = 1068f;
                float yCursor = 0f;

                for (int i = 0; i < chars.Count; i++)
                {
                    var c = chars[i];
                    var card = CreatePanel(scroll.content, ColCard);
                    PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 80f);

                    string fatigue = c.ConsecutiveUses >= 3 ? "YORGUN! (-Skor)" : $"Dinç (Seri: {c.ConsecutiveUses})";
                    var txt = CreateText(card.transform,
                        $"{c.Name}  [{c.Status}]  —  {c.ArchetypeName}\nPopülerlik: {c.Popularity}/100  |  Oyun: {c.GamesAppeared}  |  {fatigue}  |  Çıkış: {c.DebutGame}",
                        17, ColDarkText, TextAnchor.MiddleLeft, false);
                    var trt = txt.GetComponent<RectTransform>();
                    trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                    trt.offsetMin = new Vector2(16f, 4f); trt.offsetMax = new Vector2(-180f, -4f);

                    var pickBtn = CreateButton(card.transform, "Kadroya Ekle", ColGreen, Color.white, 17, () =>
                    {
                        _workingCast.Add(new ActiveCharacterDraft
                        {
                            IsReturning = true,
                            ExistingCharacterId = c.Id,
                            Name = c.Name,
                            ArchetypeId = c.ArchetypeId,
                            Role = _returningRole,
                            Sliders = (c.Sliders != null && c.Sliders.Length == 9) ? (float[])c.Sliders.Clone() : new float[9]
                        });
                        ShowCastOverview();
                    });
                    var prt = pickBtn.GetComponent<RectTransform>();
                    prt.anchorMin = new Vector2(1f, 0.5f); prt.anchorMax = new Vector2(1f, 0.5f);
                    prt.pivot = new Vector2(1f, 0.5f);
                    prt.anchoredPosition = new Vector2(-14f, 0f);
                    prt.sizeDelta = new Vector2(154f, 48f);

                    yCursor += 90f;
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(422f, yCursor));

                var btnBack = CreateButton(_contentRoot, "< Kadroya Dön", ColYellow, ColDarkText, 19, () => ShowCastOverview());
                PlaceRect(btnBack.GetComponent<RectTransform>(), 0f, 508f, 260f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 3.2] ShowReturningPickerView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // GORUNUM 4: TUM KARAKTERLER & EVREN DURUMU (HUD BUTONUNDAN DA ACILIR)
        // ---------------------------------------------------------------------
        private static void ShowFullRosterView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                _headerTitle.text = $"Karakterler & Evren Durumu ({u.Characters.Count} Karakter)";

                var summary = CreatePanel(_contentRoot, ColCard);
                PlaceRect(summary.GetComponent<RectTransform>(), 0f, 0f, 1084f, 52f);
                var sumTxt = CreateText(summary.transform,
                    $"Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())}  |  Tutarlılık: {u.CanonIntegrity}/100  |  Lore Bariyeri: {u.LoreBarrier}/100  |  Ara Evren Serisi: {u.ConsecutiveSideProjects}",
                    18, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(sumTxt.GetComponent<RectTransform>(), 16f, 0f);

                var scroll = CreateScrollView(_contentRoot, 0f, 64f, 1084f, 432f);
                float innerW = 1068f;
                float yCursor = 0f;

                if (u.Characters.Count == 0)
                {
                    var empty = CreatePanel(scroll.content, ColCard);
                    PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 120f);
                    var t = CreateText(empty.transform,
                        "Henüz evrenine kayıtlı karakter yok. Yeni bir Ana/Yan Evren veya Alternatif Zaman oyunu başlattığında Karakter Atölyesi otomatik açılacak.",
                        19, ColMutedText, TextAnchor.MiddleCenter, false);
                    StretchWithPad(t.GetComponent<RectTransform>(), 20f, 10f);
                    yCursor = 128f;
                }
                else
                {
                    for (int i = 0; i < u.Characters.Count; i++)
                    {
                        var c = u.Characters[i];
                        var card = CreatePanel(scroll.content, ColCard);
                        PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 92f);

                        string fatigue = c.ConsecutiveUses >= 3 ? "YORGUN (Dinlendir!)" : (c.ConsecutiveUses == 0 ? "Dinç" : $"Aktif (Seri {c.ConsecutiveUses})");
                        string sLine = (c.Sliders != null && c.Sliders.Length == 9)
                            ? $"Duruş: Kavg {c.Sliders[0]*100:F0} / San.Diy {c.Sliders[1]*100:F0} / Zek {c.Sliders[2]*100:F0}  |  Psik: Trv {c.Sliders[3]*100:F0} / İçAç {c.Sliders[4]*100:F0} / Mzh {c.Sliders[5]*100:F0}  |  Sosy: Çev {c.Sliders[6]*100:F0} / Ego {c.Sliders[7]*100:F0} / Kap {c.Sliders[8]*100:F0}"
                            : "";

                        var txt = CreateText(card.transform,
                            $"{c.Name}  [{c.Status}]  —  {c.ArchetypeName} (Son Rol: {c.LastRole})  |  Pop: {c.Popularity}/100  |  Oyun: {c.GamesAppeared}  |  {fatigue}\n" +
                            $"Çıkış Oyunu: {c.DebutGame}\n{sLine}",
                            15, ColDarkText, TextAnchor.MiddleLeft, false);
                        StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 6f);

                        yCursor += 102f;
                    }
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(416f, yCursor));

                string closeLabel = _isRosterOnlyMode ? "Kapat" : "< Oyun Kadrosuna Dön";
                var btnClose = CreateButton(_contentRoot, closeLabel, ColYellow, ColDarkText, 19, () =>
                {
                    if (_isRosterOnlyMode) CloseModal();
                    else ShowCastOverview();
                });
                PlaceRect(btnClose.GetComponent<RectTransform>(), 0f, 508f, 300f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 3.2] ShowFullRosterView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // uGUI FABRIKA METOTLARI (TAM TIPLI UnityEngine.UI + SIFIR LAYOUTGROUP)
        // ---------------------------------------------------------------------
        private static void InitAssets()
        {
            if (_font != null) return;

            try
            {
                var existingText = UnityEngine.Object.FindObjectOfType<UnityEngine.UI.Text>();
                if (existingText != null && existingText.font != null)
                {
                    _font = existingText.font;
                    return;
                }
            }
            catch { }

            try
            {
                var allFonts = UnityEngine.Resources.FindObjectsOfTypeAll<Font>();
                if (allFonts != null)
                {
                    for (int i = 0; i < allFonts.Length; i++)
                    {
                        if (allFonts[i] != null)
                        {
                            _font = allFonts[i];
                            return;
                        }
                    }
                }
            }
            catch { }

            try { _font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (_font == null)
                try { _font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void StretchWithPad(RectTransform rt, float px, float py)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(px, py); rt.offsetMax = new Vector2(-px, -py);
        }

        private static void PlaceRect(RectTransform rt, float x, float yTop, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -yTop);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static GameObject CreatePanel(Transform parent, Color color)
        {
            var go = new GameObject("Panel");
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            return go;
        }

        private static UnityEngine.UI.Text CreateText(Transform parent, string content, int size, Color color, TextAnchor align, bool bold)
        {
            var go = new GameObject("Text");
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<UnityEngine.UI.Text>();
            if (_font != null) txt.font = _font;
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = align;
            try { txt.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; } catch { }
            try { txt.horizontalOverflow = HorizontalWrapMode.Wrap; } catch { }
            try { txt.verticalOverflow = VerticalWrapMode.Overflow; } catch { }
            return txt;
        }

        private static UnityEngine.UI.Button CreateButton(Transform parent, string label, Color bg, Color textCol, int fontSize, Action onClick)
        {
            var go = CreatePanel(parent, bg);
            var btn = go.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();

            var txt = CreateText(go.transform, label, fontSize, textCol, TextAnchor.MiddleCenter, true);
            StretchWithPad(txt.GetComponent<RectTransform>(), 8f, 4f);

            if (onClick != null)
                btn.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(onClick));
            return btn;
        }

        private static UnityEngine.UI.InputField CreateInputFieldSafe(Transform parent, string initial, string placeholderText, Action<string> onChanged, out UnityEngine.UI.Text fallbackText)
        {
            fallbackText = null;
            try
            {
                var go = CreatePanel(parent, Color.white);
                var input = go.AddComponent<UnityEngine.UI.InputField>();
                input.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();

                var ph = CreateText(go.transform, placeholderText, 18, ColMutedText, TextAnchor.MiddleLeft, false);
                StretchWithPad(ph.GetComponent<RectTransform>(), 10f, 2f);

                var tx = CreateText(go.transform, initial ?? "", 19, ColDarkText, TextAnchor.MiddleLeft, true);
                try { tx.supportRichText = false; } catch { }
                StretchWithPad(tx.GetComponent<RectTransform>(), 10f, 2f);

                input.textComponent = tx;
                input.placeholder = ph;
                try { input.characterLimit = 24; } catch { }
                input.text = initial ?? "";

                if (onChanged != null)
                    input.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<string>>(onChanged));
                return input;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 3.2] InputField kullanilamadi, salt-metin moduna gecildi: {ex.Message}");
                var box = CreatePanel(parent, Color.white);
                fallbackText = CreateText(box.transform, initial ?? "", 19, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(fallbackText.GetComponent<RectTransform>(), 10f, 2f);
                return null;
            }
        }

        private static UnityEngine.UI.Slider CreateSlider(Transform parent, float initialVal, Action<float> onChanged)
        {
            var go = new GameObject("Slider");
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);

            var bg = CreatePanel(go.transform, ColTrack);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.30f); bgRt.anchorMax = new Vector2(1f, 0.70f);
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            var fillArea = new GameObject("FillArea");
            var faRt = fillArea.AddComponent<RectTransform>();
            fillArea.transform.SetParent(go.transform, false);
            faRt.anchorMin = new Vector2(0f, 0.30f); faRt.anchorMax = new Vector2(1f, 0.70f);
            faRt.offsetMin = new Vector2(6f, 0f); faRt.offsetMax = new Vector2(-6f, 0f);

            var fill = CreatePanel(fillArea.transform, ColHeader);
            var fillRt = fill.GetComponent<RectTransform>();
            StretchFull(fillRt);

            var handleArea = new GameObject("HandleArea");
            var haRt = handleArea.AddComponent<RectTransform>();
            handleArea.transform.SetParent(go.transform, false);
            StretchFull(haRt);
            haRt.offsetMin = new Vector2(14f, 0f); haRt.offsetMax = new Vector2(-14f, 0f);

            var handle = CreatePanel(handleArea.transform, ColYellow);
            var hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(30f, 30f);

            var slider = go.AddComponent<UnityEngine.UI.Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = hRt;
            slider.targetGraphic = handle.GetComponent<UnityEngine.UI.Image>();
            try { slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight; } catch { }
            slider.minValue = 0f;
            slider.maxValue = 1f;
            try { slider.SetValueWithoutNotify(Mathf.Clamp01(initialVal)); }
            catch { slider.value = Mathf.Clamp01(initialVal); }

            if (onChanged != null)
                slider.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<float>>(onChanged));
            return slider;
        }

        private static UnityEngine.UI.ScrollRect CreateScrollView(Transform parent, float x, float yTop, float w, float h)
        {
            var go = CreatePanel(parent, new Color(0.95f, 0.92f, 0.85f, 1f));
            PlaceRect(go.GetComponent<RectTransform>(), x, yTop, w, h);

            var sr = go.AddComponent<UnityEngine.UI.ScrollRect>();
            try { sr.horizontal = false; } catch { }
            try { sr.vertical = true; } catch { }
            try { sr.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic; } catch { }
            try { sr.scrollSensitivity = 25f; } catch { }

            var vp = CreatePanel(go.transform, new Color(1f, 1f, 1f, 0.01f));
            var vpRt = vp.GetComponent<RectTransform>();
            StretchWithPad(vpRt, 8f, 8f);
            try { vp.AddComponent<UnityEngine.UI.RectMask2D>(); } catch { }

            var content = new GameObject("Content");
            var cRt = content.AddComponent<RectTransform>();
            content.transform.SetParent(vp.transform, false);
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, h - 16f);

            sr.viewport = vpRt;
            sr.content = cRt;
            return sr;
        }
    }

    // =========================================================================
    // 6. FAZ 2 & 3: EVREN KONUMU + TON + COKLU KARAKTER KADROSU TETIKLEYICI
    // =========================================================================
    public static class UniverseChoicePopupSystem
    {
        private static readonly HashSet<IntPtr> _promptedGames = new HashSet<IntPtr>();
        private static readonly Dictionary<IntPtr, ProjectCategory> _gameCategories = new Dictionary<IntPtr, ProjectCategory>();
        private static readonly Dictionary<IntPtr, List<ActiveCharacterDraft>> _gameCasts = new Dictionary<IntPtr, List<ActiveCharacterDraft>>();

        public static void ResetSessionCache()
        {
            _promptedGames.Clear();
            _gameCategories.Clear();
            _gameCasts.Clear();
            NativePopupQueue.Clear();
        }

        public static ProjectCategory GetCategoryForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero && _gameCategories.TryGetValue(game.Pointer, out var cat))
                return cat;
            return UniverseArchitectMod.ActiveProjectCategory;
        }

        public static void SetCastForGame(IntPtr gamePtr, List<ActiveCharacterDraft> cast)
        {
            if (gamePtr != IntPtr.Zero) _gameCasts[gamePtr] = cast ?? new List<ActiveCharacterDraft>();
        }

        public static List<ActiveCharacterDraft> GetCastForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero && _gameCasts.TryGetValue(game.Pointer, out var cast))
                return cast;
            return null;
        }

        public static void PollForNewGame(Company company)
        {
            if (company == null || !company.isCurrentlyDevelopingGame() || company.currentGame == null)
                return;
            if (NativePopupQueue.HasPending || CharacterWorkshopUGUI.IsOpen)
                return;
            if (UniverseArchitectMod.IsNativeModalOpen())
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
                        CharacterWorkshopUGUI.OpenCastManager(capturedGamePtr, title);
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
                    CharacterWorkshopUGUI.OpenCastManager(gamePtr, title);
                }
            );
        }
    }

    // =========================================================================
    // 7. 10X ARASTIRMA PUANI TURBO SISTEMI (DOGRUDAN company.researchPoints ALANI)
    // =========================================================================
    public static class ResearchTurboSystem
    {
        private static int _lastRP = -1;
        private static bool _starterBoostGiven = false;
        private static IntPtr _trackedCompanyPtr = IntPtr.Zero;

        public static void BindCompany(Company company)
        {
            if (company == null) return;

            bool isNewCompany = (company.Pointer != _trackedCompanyPtr);
            if (isNewCompany)
            {
                _trackedCompanyPtr = company.Pointer;
                _lastRP = -1;
                _starterBoostGiven = false;
                UniverseChoicePopupSystem.ResetSessionCache();
                UniverseSaveManager.LoadForCompany(company);
                MelonLogger.Msg($"[Universe Architect] Aktif sirket yakalandi: '{company.name ?? "?"}' (UID: '{company.uid ?? "?"}', Hafta: {company.currentWeek:F1}, RP: {company.researchPoints})");
            }

            EngineFeatureInjectorHelper.SyncWithCompany(company);
            CharacterWorkshopUGUI.EnsureHUD();

            if (!ModSettings.TestMode10xResearch) return;

            int currentRP = company.researchPoints;
            if (!_starterBoostGiven)
            {
                int boosted = currentRP + ModSettings.TestStarterResearchBoost;
                company.researchPoints = boosted;
                _lastRP = boosted;
                _starterBoostGiven = true;
                MelonLogger.Msg($"[NJG-Turbo] 10x Arastirma Turbo aktif! (+{ModSettings.TestStarterResearchBoost} RP -> {boosted})");
                NotificationHelper.Show(
                    company,
                    "Faz 3.2 Karakter Atölyesi Hazır!",
                    "Artık ilk oyunundan itibaren Evren Konumu, 3×3 sürüklemeli Karakter Atölyesi ve sağ alttaki Karakterler ekranı emrinde!"
                );
            }
            else
            {
                _lastRP = currentRP;
            }
        }

        public static void PollAndMultiply(Company company)
        {
            if (!ModSettings.TestMode10xResearch || company == null) return;

            int currentRP = company.researchPoints;
            if (_lastRP < 0 || currentRP < _lastRP) { _lastRP = currentRP; return; }

            int delta = currentRP - _lastRP;
            if (delta >= 1 && delta <= 200)
            {
                int newTotal = currentRP + (int)Math.Round(delta * (ModSettings.ResearchMultiplier - 1.0));
                company.researchPoints = newTotal;
                _lastRP = newTotal;
            }
            else if (delta > 200) _lastRP = currentRP;
        }
    }

    // =========================================================================
    // 8. ANA MOD GIRISI + DOGRUDAN GameManager.company YAKALAMA
    // =========================================================================
    public class UniverseArchitectMod : MelonMod
    {
        public static ProjectCategory ActiveProjectCategory = ProjectCategory.MainUniverse;
        public static GameController ActiveGameController = null;
        public static Company ActiveCompany = null;
        public static bool TopicsInjected = false;
        public static bool EngineFeaturesInjected = false;
        private float _injectTimer = 0f;
        private float _companyDiscoverTimer = 0f;
        private float _turboTimer = 0f;
        private float _companySyncTimer = 0f;
        private float _popupPollTimer = 0f;

        public static GameManager GetGameManager()
        {
            try
            {
                if (ActiveGameController == null)
                    ActiveGameController = UnityEngine.Object.FindObjectOfType<GameController>();
                return ActiveGameController != null ? ActiveGameController.gameManager : null;
            }
            catch { return null; }
        }

        public static bool IsNativeModalOpen()
        {
            try
            {
                if (ActiveGameController != null && ActiveGameController.ui != null)
                {
                    if (ActiveGameController.ui.isModalContentOpen()) return true;
                }
            }
            catch { }

            try
            {
                var panel = UnityEngine.Object.FindObjectOfType<PanelNotifications>();
                if (panel != null && panel.IsNotificationVisible) return true;
            }
            catch { }

            return false;
        }

        public static bool IsGameplayReadyForHud()
        {
            if (ActiveCompany == null) return false;
            var gm = GetGameManager();
            if (gm == null) return false;
            try
            {
                if (!gm.IsValidCompany || gm.loadInProgress) return false;
            }
            catch { }
            return !IsNativeModalOpen();
        }

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Universe Architect v3.2.1 (Direct GameManager/Genre/Engine API + Kayit Bazli JSON + Native Pause) baslatiliyor...");

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v321");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v3.2.1 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        private static void TryDiscoverActiveCompany()
        {
            try
            {
                var gm = GetGameManager();
                if (gm == null || gm.loadInProgress || !gm.IsValidCompany || gm.company == null)
                {
                    if (ActiveCompany != null)
                    {
                        ActiveCompany = null;
                        if (CharacterWorkshopUGUI.IsOpen) CharacterWorkshopUGUI.CloseModal();
                        CharacterWorkshopUGUI.SetHudVisible(false);
                    }
                    return;
                }

                var comp = gm.company;
                if (ActiveCompany == null || ActiveCompany.Pointer != comp.Pointer)
                {
                    ActiveCompany = comp;
                    ResearchTurboSystem.BindCompany(comp);
                }
            }
            catch { }
        }

        public override void OnUpdate()
        {
            float dt = Time.unscaledDeltaTime;
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

            _companyDiscoverTimer += dt;
            if (_companyDiscoverTimer >= 1.0f)
            {
                _companyDiscoverTimer = 0f;
                TryDiscoverActiveCompany();
            }

            if (ActiveCompany == null) return;

            _popupPollTimer += dt;
            if (_popupPollTimer >= 0.6f)
            {
                _popupPollTimer = 0f;
                try { CharacterWorkshopUGUI.EnsureHUD(); } catch { }
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

        public static int ApplyCappedModHype(int rawModHype) => Math.Min(rawModHype, BalanceCaps.MaxTotalModHype);
    }

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
            var avail = company.availableResearch;
            var completed = company.researchCompleted;
            var engineParts = company.engineParts;

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
    // 9. INCELEME: FAZ 1 + FAZ 2 + FAZ 3.2 (DOGRUDAN GameGenreType & EngineType)
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
                string genreKey = ResolveGenreKey(game.genre);
                string secondGenreKey = ResolveGenreKey(game.secondGenre);
                string displayGenre = !string.IsNullOrEmpty(genreKey) ? genreKey : (game.getGenreDisplayName() ?? "");

                ProjectCategory category = UniverseChoicePopupSystem.GetCategoryForGame(game);
                var cast = UniverseChoicePopupSystem.GetCastForGame(game);

                int totalModHype = 0;
                int hypePenalty = 0;
                double totalScoreMult = 1.0;
                var activeBonusNotes = new List<string>();
                var usedModFeatures = new List<string>();
                var castSummary = new List<string>();

                // A) Post-Credits Hype aktarimi
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

                // C) 5 Yeni Motor Ozelligi (Dogrudan game.features, featureLog, engine.partsIds, engine.convertedParts)
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

                // E) FAZ 3.2: COKLU KARAKTER KADROSU DEGERLENDIRMESI (4 ROL + CIFT TUR DESTEGI)
                if (cast != null && cast.Count > 0 && category != ProjectCategory.NonUniverse)
                {
                    EvaluateCast(company, title, genreKey, secondGenreKey, displayGenre, category, cast, castSummary,
                        ref totalScoreMult, ref totalModHype, ref hypePenalty, activeBonusNotes);
                }
                else
                {
                    UniverseSaveManager.RestUnusedCharacters(null);
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
                    NotificationHelper.Show(company, $"[{CategoryLabels.ToTurkish(category)}] {title}", string.Join(" | ", activeBonusNotes));
                }

                UniverseSaveManager.RecordReleasedGame(game, topicId, game.score, category, usedModFeatures, castSummary);
                CharacterWorkshopUGUI.UpdateHudState();
                MelonLogger.Msg($"[Universe Architect] Kaydedildi: '{title}' | {CategoryLabels.ToTurkish(category)} | Kadro: {castSummary.Count} | Skor: {game.score:F2}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] ReviewGame hatasi: {ex.Message}");
            }
        }

        private static void EvaluateCast(
            Company company, string gameTitle, string genreKey, string secondGenreKey, string displayGenre, ProjectCategory category,
            List<ActiveCharacterDraft> cast, List<string> castSummary,
            ref double scoreMult, ref int modHype, ref int hypePenalty, List<string> notes)
        {
            double ensembleScoreDelta = 0.0;
            var usedIds = new HashSet<string>();

            for (int cIdx = 0; cIdx < cast.Count; cIdx++)
            {
                var draft = cast[cIdx];
                var arch = Phase1Database.FindArchetype(draft.ArchetypeId);

                double roleWeight = draft.Role switch
                {
                    CharacterRole.Protagonist => 1.00,
                    CharacterRole.Villain     => 0.75,
                    CharacterRole.Sidekick    => 0.55,
                    _                         => 0.30
                };

                double genreFit = 0.75;
                double gw1 = 0.75, gw2 = 0.75;
                bool hasPrimary = !string.IsNullOrEmpty(genreKey) && arch.GenreWeights.TryGetValue(genreKey, out gw1);
                bool hasSecondary = !string.IsNullOrEmpty(secondGenreKey) && arch.GenreWeights.TryGetValue(secondGenreKey, out gw2);
                if (hasPrimary && hasSecondary) genreFit = Math.Max(gw1, (gw1 * 0.65) + (gw2 * 0.35));
                else if (hasPrimary) genreFit = gw1;
                else if (hasSecondary) genreFit = gw2;

                double diffSum = 0.0;
                for (int i = 0; i < 9; i++)
                {
                    float actual = (draft.Sliders != null && i < draft.Sliders.Length) ? draft.Sliders[i] : 0.5f;
                    diffSum += Math.Abs(actual - arch.IdealSliders[i]);
                }
                double sliderFit = Math.Clamp(1.0 - (diffSum / 9.0) * 1.50, 0.20, 1.0);

                double toneMod = 0.0;
                var uTone = UniverseSaveManager.GetTone();
                if (uTone != UniverseTone.None)
                {
                    if (arch.PreferredTone == uTone) toneMod = +0.08;
                    else if (arch.ClashingTone == uTone) toneMod = -0.10;
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
                        DebutGame = gameTitle,
                        Sliders = (float[])draft.Sliders.Clone(),
                        Popularity = chemPct
                    };
                    UniverseSaveManager.Current.Characters.Add(rec);
                }

                rec.LastRole = CategoryLabels.RoleToTurkish(draft.Role);
                rec.GamesAppeared++;
                rec.ConsecutiveUses++;
                usedIds.Add(rec.Id);
                castSummary.Add($"{rec.Name} ({rec.LastRole})");

                // Yorgunluk kontrolu
                if (draft.IsReturning && rec.ConsecutiveUses >= 3)
                {
                    ensembleScoreDelta -= 0.04 * roleWeight;
                    hypePenalty += (int)Math.Round(5 * roleWeight);
                    rec.Popularity = Math.Clamp(rec.Popularity - 8, 10, 100);
                    notes.Add($"Yorgun Karakter: {rec.Name} ({rec.ConsecutiveUses}. seri)");
                    NotificationHelper.Show(company, $"Karakter Yorgunluğu: {rec.Name}",
                        $"Hayranlar {rec.Name} karakterini üst üste görmekten yoruldu! Birkaç oyun dinlendirirsen yeniden özlenecek.");
                    continue;
                }

                if (chemistry >= 0.78)
                {
                    ensembleScoreDelta += 0.06 * roleWeight;
                    int hypeAdd = (int)Math.Max(2, Math.Round((draft.Role == CharacterRole.Villain ? 10 : 8) * roleWeight));
                    modHype += hypeAdd;
                    double fanBoost = Math.Max(25.0, Math.Round(company.fans * 0.05 * roleWeight));
                    company.adjustFans(fanBoost);

                    rec.Status = draft.Role == CharacterRole.Minor ? "Kült" : "Yıldız";
                    rec.Popularity = Math.Clamp(Math.Max(rec.Popularity, chemPct) + 5, 10, 100);
                    notes.Add($"{rec.LastRole} {rec.Name} [%{chemPct}] (+{hypeAdd} Hype)");

                    if (draft.Role == CharacterRole.Minor)
                    {
                        NotificationHelper.Show(company, $"Sürpriz Kült Karakter: {rec.Name}!",
                            $"{gameTitle} oyununda sadece yan rol olarak geçen {rec.Name} ({arch.Name}) %{chemPct} uyumla internette viral oldu! (+{fanBoost:F0} Hayran)");
                    }
                    else
                    {
                        NotificationHelper.Show(company, $"Hayranlar {rec.Name} ({rec.LastRole}) Karakterine Bayıldı!",
                            $"{gameTitle} oyunundaki {rec.Name} ({arch.Name}) uyumu %{chemPct} ile efsaneleşti! (+{fanBoost:F0} Hayran)");
                    }
                }
                else if (chemistry >= 0.60)
                {
                    ensembleScoreDelta += 0.02 * roleWeight;
                    modHype += (int)Math.Max(1, Math.Round(3 * roleWeight));
                    if (rec.Status != "Yıldız" && rec.Status != "Kült") rec.Status = "Sevilen";
                    rec.Popularity = Math.Clamp((rec.Popularity + chemPct) / 2, 10, 100);
                    notes.Add($"Sevilen {rec.LastRole}: {rec.Name} [%{chemPct}]");
                }
                else if (chemistry < 0.45)
                {
                    ensembleScoreDelta -= 0.07 * roleWeight;
                    hypePenalty += (int)Math.Round(6 * roleWeight);
                    rec.Status = "Fiyasko";
                    rec.Popularity = Math.Clamp(chemPct, 5, 45);
                    if (category == ProjectCategory.MainUniverse && draft.Role != CharacterRole.Minor)
                        UniverseSaveManager.AdjustIntegrity(-5);
                    notes.Add($"Fiyasko {rec.LastRole}: {rec.Name} [%{chemPct}]");

                    if (draft.Role != CharacterRole.Minor)
                    {
                        NotificationHelper.Show(company, $"Oyuncular {rec.Name} ({rec.LastRole}) Karakterinden Nefret Etti!",
                            $"{gameTitle} ({displayGenre}) için {rec.Name} ({arch.Name}) hiç uymadı (%{chemPct}). Eleştirmenler karakterin oyunun havasını bozduğunu yazıyor!");
                    }
                }
            }

            // Coklu kadro denge tavani (-%10 ile +%12 arasi)
            ensembleScoreDelta = Math.Clamp(ensembleScoreDelta, -0.10, 0.12);
            scoreMult *= (1.0 + ensembleScoreDelta);

            UniverseSaveManager.RestUnusedCharacters(usedIds);
        }

        private static string ResolveGenreKey(GameGenreType genreType)
        {
            if (genreType == null) return "";
            var sampleWeights = Phase1Database.Archetypes[0].GenreWeights;
            if (!string.IsNullOrEmpty(genreType.id) && sampleWeights.ContainsKey(genreType.id)) return genreType.id;
            if (!string.IsNullOrEmpty(genreType.name) && sampleWeights.ContainsKey(genreType.name)) return genreType.name;
            return !string.IsNullOrEmpty(genreType.id) ? genreType.id : (genreType.name ?? "");
        }

        private static HashSet<string> ExtractAllFeatureIds(Game game)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (game == null) return ids;

            try
            {
                var featList = game.features;
                if (featList != null)
                {
                    for (int i = 0; i < featList.Count; i++)
                        if (featList[i] != null && !string.IsNullOrEmpty(featList[i].id))
                            ids.Add(featList[i].id);
                }
            }
            catch { }

            try
            {
                if (game.featureLog != null)
                {
                    for (int i = 0; i < game.featureLog.Count; i++)
                        if (game.featureLog[i] != null && !string.IsNullOrEmpty(game.featureLog[i].id))
                            ids.Add(game.featureLog[i].id);
                }
            }
            catch { }

            try
            {
                if (game.engine != null)
                {
                    if (game.engine.partsIds != null)
                    {
                        for (int i = 0; i < game.engine.partsIds.Count; i++)
                            if (!string.IsNullOrEmpty(game.engine.partsIds[i]))
                                ids.Add(game.engine.partsIds[i]);
                    }
                    if (game.engine.convertedParts != null)
                    {
                        for (int i = 0; i < game.engine.convertedParts.Count; i++)
                            if (game.engine.convertedParts[i] != null && !string.IsNullOrEmpty(game.engine.convertedParts[i].id))
                                ids.Add(game.engine.convertedParts[i].id);
                    }
                }
            }
            catch { }

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
