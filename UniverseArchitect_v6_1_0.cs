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

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "6.1.0", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    // =========================================================================
    // 1. TEST AYARLARI, VERI MODELLERI, FAZ 1-6 SABITLERI & 4 KUTUPLU EKSEN
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

    public enum CosmologyLaw
    {
        None,
        StrictCanon,        // Kati Kanon
        FlexibleMultiverse, // Esnek Coklu Evren
        CharacterLegendEra  // Efsane Karakter Cagi
    }

    public enum CharacterRole
    {
        Protagonist,    // Basrol
        Villain,        // Kotu Adam
        Sidekick,       // Yardimci
        Minor           // Herhangi Bir Adam
    }

    public enum CharacterTrait
    {
        None,
        SceneStealer,   // Sahne Hirsizi: Yan rolde bile kolayca viral/Kult olur (+4 Hype), basrolden ufak pay calar
        MysteriousPast, // Gizemli Gecmis: +5 Teori Hype'i, Ana Evren'de Lore Bariyeri -5 freni
        LocationBound,  // Mekana Bagli: Cikis yaptigi konuda +%12 ekstra kimya & +%2 Skor
        CanonAnchor     // Kanon Capasi: Ana Evren kotu skor alirsa Lore Hasarini yariya dusurur (+2 Tutarlilik kalkani)
    }

    public enum CharacterBondType
    {
        ArchRivalry,    // Ezeli Rekabet
        Comrades,       // Silah Arkadasi
        MentorStudent   // Usta-Cirak (Miras Devri)
    }

    public enum CharacterFate
    {
        Alive,          // Hayatta
        Sacrificed,     // Final Fedasi (Olumsuzlesti)
        Retired         // Onurlu Emeklilik
    }

    public enum CrossoverMode
    {
        None,
        HighRiskCanon,  // Tam Kanon Carpismasi
        BalancedPortal, // Paralel Boyut Gecidi
        SafeCameo       // Konuk Gorunum / Cameo
    }

    public enum TransmediaType
    {
        ComicSeries,    // Cizgi Roman Serisi
        Novel,          // Evren Romani
        ConceptAlbum    // Konsept Album & Rap EP
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

        public static string LawToTurkish(CosmologyLaw law)
        {
            switch (law)
            {
                case CosmologyLaw.StrictCanon:        return "Katı Kanon";
                case CosmologyLaw.FlexibleMultiverse: return "Esnek Çoklu Evren";
                case CosmologyLaw.CharacterLegendEra: return "Efsane Karakter Çağı";
                default:                              return "Belirlenmedi";
            }
        }

        public static string LawShortDesc(CosmologyLaw law)
        {
            switch (law)
            {
                case CosmologyLaw.StrictCanon:
                    return "Ana Evren & devam oyunları ekstra +%3 Skor; ama 5.5 altı skor 2x Lore Hasarı verir, Yan Evren bariyeri -15 düşürür.";
                case CosmologyLaw.FlexibleMultiverse:
                    return "Ara Evren Beklentisi cezası yarıya iner, Boyut Geçidi Crossover daha güvenli; ama Ana Evren kanon bonusu %2 düşük kalır.";
                case CosmologyLaw.CharacterLegendEra:
                    return "Karakterler 4 oyun üst üste yorulmadan oynar, Transmedya +5 ekstra popülerlik verir; ama Fiyasko karakter -10 Tutarlılık kırar.";
                default:
                    return "Henüz bir kozmoloji yasası seçilmedi. İlk Ana Evren oyununda veya buradan belirleyebilirsin.";
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

        public static string TraitToTurkish(CharacterTrait trait)
        {
            switch (trait)
            {
                case CharacterTrait.SceneStealer:   return "Sahne Hırsızı";
                case CharacterTrait.MysteriousPast: return "Gizemli Geçmiş";
                case CharacterTrait.LocationBound:  return "Mekana Bağlı";
                case CharacterTrait.CanonAnchor:    return "Kanon Çapası";
                default:                            return "Damga Yok";
            }
        }

        public static string TraitShortDesc(CharacterTrait trait)
        {
            switch (trait)
            {
                case CharacterTrait.SceneStealer:
                    return "Yan rollerde bile kolayca viral/Kült olur (+4 Hype); başrolün skor payından %1 çalar.";
                case CharacterTrait.MysteriousPast:
                    return "Teori fırtınası başlatır (+5 Hype) ve Ana Evren'de Lore Bariyerini -5 frenler.";
                case CharacterTrait.LocationBound:
                    return "Çıkış yaptığı konudaki oyunlarda +%12 kimya ve +%2 Skor parlaması kazanır.";
                case CharacterTrait.CanonAnchor:
                    return "Ana Evren'de 5.5 altı skor gelse bile Lore Hasarını yarıya indirir (+2 Tutarlılık kalkanı).";
                default:
                    return "Standart karakter — ekstra özel kural veya yan etki yok.";
            }
        }

        public static CharacterTrait ParseTrait(string name)
        {
            CharacterTrait t;
            if (!string.IsNullOrEmpty(name) && Enum.TryParse<CharacterTrait>(name, out t)) return t;
            return CharacterTrait.None;
        }

        public static string BondToTurkish(CharacterBondType b)
        {
            switch (b)
            {
                case CharacterBondType.ArchRivalry:   return "Ezeli Rekabet";
                case CharacterBondType.MentorStudent: return "Usta–Çırak";
                default:                              return "Silah Arkadaşı";
            }
        }

        public static string BondShortDesc(CharacterBondType b)
        {
            switch (b)
            {
                case CharacterBondType.ArchRivalry:
                    return "Aynı oyunda karşılaştıkça Seviye 1→3 güçlenir (+%2-4 Skor, +5-12 Hype).";
                case CharacterBondType.MentorStudent:
                    return "Kıdemli yıldız, yeni/çaylak karaktere popülerliğinden Miras Devri yapar (+%2-4 Skor).";
                default:
                    return "Omuz omuza savaştıkça Seviye 1→3 sinerji ve ekstra hayran kazandırır (+%2-4 Skor).";
            }
        }

        public static CharacterBondType ParseBond(string name)
        {
            CharacterBondType b;
            if (!string.IsNullOrEmpty(name) && Enum.TryParse<CharacterBondType>(name, out b)) return b;
            return CharacterBondType.Comrades;
        }

        public static string FateToTurkish(CharacterFate f)
        {
            switch (f)
            {
                case CharacterFate.Sacrificed: return "☠ Final Fedası";
                case CharacterFate.Retired:    return "★ Emeklilik";
                default:                       return "Hayatta";
            }
        }

        public static CharacterFate ParseFate(string name)
        {
            CharacterFate f;
            if (!string.IsNullOrEmpty(name) && Enum.TryParse<CharacterFate>(name, out f)) return f;
            return CharacterFate.Alive;
        }

        public static string TransmediaToTurkish(TransmediaType type)
        {
            switch (type)
            {
                case TransmediaType.ComicSeries:  return "Çizgi Roman Serisi";
                case TransmediaType.Novel:        return "Evren Romanı";
                case TransmediaType.ConceptAlbum: return "Konsept Albüm & Rap EP";
                default:                          return "Transmedya";
            }
        }
    }

    public static class BalanceCaps
    {
        public const double MaxLoreScoreBonus = 0.08;
        public const int MaxLoreBaseHype = 15;
        public const int MaxTotalModHype = 35;
        public const int MaxCrossoverModHype = 48;
        public const double RetconCashCost = 50000.0;
        public const int RetconIntegrityCost = 15;
    }

    public static class CharacterAxisSystem
    {
        public static readonly string[] AxisNames = new[]
        {
            "Çatışma Tarzı",
            "Atmosfer",
            "Sosyal Duruş",
            "Motivasyon"
        };

        public static readonly string[] LeftPoles = new[]
        {
            "Taktik & Gizem",
            "Karanlık & Travmatik",
            "Mesafeli & Gizemli",
            "Fedakâr & İdealist"
        };

        public static readonly string[] RightPoles = new[]
        {
            "Aksiyon & Kavgacı",
            "Esprili & Enerjik",
            "Samimi & Dışa Dönük",
            "Egoist & Çıkarcı"
        };

        public static float[] NormalizeAxes(float[] axes)
        {
            if (axes != null && axes.Length == 4)
            {
                return new[]
                {
                    Mathf.Clamp01(axes[0]),
                    Mathf.Clamp01(axes[1]),
                    Mathf.Clamp01(axes[2]),
                    Mathf.Clamp01(axes[3])
                };
            }
            return new[] { 0.50f, 0.50f, 0.50f, 0.50f };
        }

        public static float[] ConvertLegacySlidersToAxes(float[] s9)
        {
            if (s9 == null || s9.Length != 9) return new[] { 0.50f, 0.50f, 0.50f, 0.50f };
            return new[]
            {
                Mathf.Clamp01((s9[0] + (1f - s9[2])) * 0.5f), // Kavgacilik vs Zeka & Gizem
                Mathf.Clamp01((s9[5] + (1f - s9[3])) * 0.5f), // Mizah vs Travma
                Mathf.Clamp01((s9[4] + (1f - s9[8])) * 0.5f), // Icini Acma vs Icine Kapaniklik
                Mathf.Clamp01((s9[7] + (1f - s9[6])) * 0.5f)  // Egoizm vs Cevre Sevgisi
            };
        }

        public static string FormatAxisReading(int axisIdx, float val)
        {
            float v = Mathf.Clamp01(val);
            if (v >= 0.45f && v <= 0.55f) return "Dengeli (%50)";
            if (v < 0.45f)
            {
                int pct = Mathf.RoundToInt((1f - v) * 100f);
                return $"%{pct} {LeftPoles[axisIdx]}";
            }
            else
            {
                int pct = Mathf.RoundToInt(v * 100f);
                return $"%{pct} {RightPoles[axisIdx]}";
            }
        }

        public static string FormatCompactSummary(float[] axes)
        {
            var a = NormalizeAxes(axes);
            string s0 = a[0] < 0.45f ? $"Taktik %{(1f - a[0]) * 100f:F0}" : (a[0] > 0.55f ? $"Aksiyon %{a[0] * 100f:F0}" : "Dengeli Çatışma");
            string s1 = a[1] < 0.45f ? $"Karanlık %{(1f - a[1]) * 100f:F0}" : (a[1] > 0.55f ? $"Mizah %{a[1] * 100f:F0}" : "Dengeli Ton");
            string s2 = a[2] < 0.45f ? $"Mesafeli %{(1f - a[2]) * 100f:F0}" : (a[2] > 0.55f ? $"Samimi %{a[2] * 100f:F0}" : "Dengeli Sosyal");
            string s3 = a[3] < 0.45f ? $"İdealist %{(1f - a[3]) * 100f:F0}" : (a[3] > 0.55f ? $"Egoist %{a[3] * 100f:F0}" : "Dengeli Amaç");
            return $"{s0}  |  {s1}  |  {s2}  |  {s3}";
        }

        public static CharacterArchetypeDef DetectClosestArchetype(float[] axes)
        {
            var a = NormalizeAxes(axes);
            CharacterArchetypeDef best = Phase1Database.Archetypes[0];
            float bestDist = float.MaxValue;

            for (int i = 0; i < Phase1Database.Archetypes.Count; i++)
            {
                var cand = Phase1Database.Archetypes[i];
                float d = 0f;
                for (int k = 0; k < 4; k++)
                {
                    float diff = a[k] - cand.PresetAxes[k];
                    d += diff * diff;
                }
                if (d < bestDist)
                {
                    bestDist = d;
                    best = cand;
                }
            }
            return best;
        }

        private static double SingleGenreFit(float[] a, string genre)
        {
            double a0 = a[0], a1 = a[1], a2 = a[2], a3 = a[3];
            double distinct = (Math.Abs(a0 - 0.5) + Math.Abs(a1 - 0.5) + Math.Abs(a2 - 0.5) + Math.Abs(a3 - 0.5)) * 0.25;

            if (string.Equals(genre, "Action", StringComparison.OrdinalIgnoreCase))
                return 0.48 + 0.44 * a0 + 0.16 * distinct;
            if (string.Equals(genre, "Adventure", StringComparison.OrdinalIgnoreCase))
                return 0.58 + 0.22 * (1.0 - Math.Abs(a0 - 0.55)) + 0.14 * (1.0 - a3) + 0.12 * distinct;
            if (string.Equals(genre, "RPG", StringComparison.OrdinalIgnoreCase))
                return 0.60 + 0.60 * Math.Min(0.45, distinct) + 0.10 * (1.0 - Math.Abs(a1 - 0.45));
            if (string.Equals(genre, "Simulation", StringComparison.OrdinalIgnoreCase))
                return 0.50 + 0.34 * (1.0 - a0) + 0.14 * (1.0 - Math.Abs(a1 - 0.50)) + 0.08 * distinct;
            if (string.Equals(genre, "Strategy", StringComparison.OrdinalIgnoreCase))
                return 0.44 + 0.48 * (1.0 - a0) + 0.16 * distinct;
            if (string.Equals(genre, "Casual", StringComparison.OrdinalIgnoreCase))
                return 0.40 + 0.34 * a1 + 0.22 * a2 + 0.08 * distinct;

            return 0.72 + 0.18 * distinct;
        }

        // Organik kimya hesabi: Arketip sablonundan saptigin icin ASLA gizli ceza yemezsin!
        public static double ComputeChemistry(float[] rawAxes, string genreKey, string secondGenreKey, UniverseTone tone, CharacterRole role)
        {
            var a = NormalizeAxes(rawAxes);

            double genreFit;
            bool has1 = !string.IsNullOrEmpty(genreKey);
            bool has2 = !string.IsNullOrEmpty(secondGenreKey);
            if (has1 && has2)
            {
                double g1 = SingleGenreFit(a, genreKey);
                double g2 = SingleGenreFit(a, secondGenreKey);
                genreFit = Math.Max(g1, g1 * 0.65 + g2 * 0.35);
            }
            else if (has1) genreFit = SingleGenreFit(a, genreKey);
            else if (has2) genreFit = SingleGenreFit(a, secondGenreKey);
            else genreFit = 0.76;

            double toneMod = 0.0;
            if (tone == UniverseTone.DarkMystery)
                toneMod = (0.50 - a[1]) * 0.22 + (0.50 - a[2]) * 0.08;
            else if (tone == UniverseTone.EpicAdventure)
                toneMod = (0.50 - a[3]) * 0.16 + (a[0] - 0.40) * 0.08;
            else if (tone == UniverseTone.SciFiTactical)
                toneMod = (0.55 - a[0]) * 0.20 + (0.50 - a[1]) * 0.06;
            else if (tone == UniverseTone.LightComedy)
                toneMod = (a[1] - 0.50) * 0.22 + (a[2] - 0.50) * 0.08;
            toneMod = Math.Clamp(toneMod, -0.14, 0.12);

            double roleFlavor = 0.0;
            if (role == CharacterRole.Villain && (a[3] >= 0.55f || a[1] <= 0.40f)) roleFlavor = 0.03;
            else if (role == CharacterRole.Protagonist && (Math.Abs(a[3] - 0.5f) >= 0.20f || a[0] >= 0.55f)) roleFlavor = 0.02;
            else if (role == CharacterRole.Sidekick && (a[2] >= 0.50f || a[1] >= 0.55f)) roleFlavor = 0.02;

            return Math.Clamp(genreFit + toneMod + roleFlavor, 0.20, 0.99);
        }
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

    public class TransmediaDef
    {
        public TransmediaType Type;
        public string Name;
        public double Cost;
        public int DurationWeeks;
        public string ShortEffect;
        public string DefaultTitlePrefix;
    }

    public class RivalStudioTemplate
    {
        public string Id;
        public string Name;
        public string UniverseName;
        public UniverseTone Tone;
        public string ArchetypeStyle;
        public double BaseSkill;
        public double Variance;
        public int ReleaseCadenceWeeks;
        public long StartingFans;
        public string[] TitlePool;
    }

    public class RivalStudioRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UniverseName { get; set; }
        public string ToneName { get; set; }
        public string ArchetypeStyle { get; set; }
        public long Fans { get; set; }
        public int CanonIntegrity { get; set; } = 85;
        public int GamesReleased { get; set; } = 0;
        public double LastScore { get; set; } = 0.0;
        public double AverageScore { get; set; } = 0.0;
        public string LastGameTitle { get; set; } = "Henüz yok";
        public double NextReleaseWeek { get; set; } = 10.0;
        public int CrossoversDone { get; set; } = 0;
    }

    public class RivalNewsEntry
    {
        public double Week { get; set; }
        public string StudioName { get; set; }
        public string Headline { get; set; }
        public string Detail { get; set; }
        public string Tag { get; set; } = "HABER";
    }

    public class CharacterArchetypeDef
    {
        public string Id;
        public string Name;
        public string ShortDesc;
        public UniverseTone PreferredTone;
        public UniverseTone ClashingTone;
        public Dictionary<string, double> GenreWeights;
        public float[] PresetAxes; // 4 Kutuplu Eksen hazir sablonu (ceza anahtari DEGIL!)
    }

    public class CharacterRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsNpc { get; set; } = false;
        public string NpcType { get; set; } = "";
        public string LinkedSpecialCharId { get; set; } = "";
        public string LinkedSpecialCharName { get; set; } = "";
        public string ArchetypeId { get; set; } = "classic_hero";
        public string ArchetypeName { get; set; } = "Klasik Kahraman";
        public string LastRole { get; set; } = "Başrol";
        public string TraitName { get; set; } = "None";
        public string TeamName { get; set; } = "";
        public string DebutTopicId { get; set; } = "";
        public float[] Axes { get; set; } = null;
        public float[] Sliders { get; set; } = null; // Eski v5.0.1 kayitlariyla tam uyumluluk
        public string DebutGame { get; set; } = "";
        public string Status { get; set; } = "Normal";
        public int Popularity { get; set; } = 50;
        public int GamesAppeared { get; set; } = 0;
        public int ConsecutiveUses { get; set; } = 0;
        public int ProtagonistGames { get; set; } = 0;
        public int VillainGames { get; set; } = 0;
        public string ArcTag { get; set; } = "";
        public string FateName { get; set; } = "Alive";
        public string FateGame { get; set; } = "";
        public int GamesMissed { get; set; } = 0;
        public int SpinOffDemandRemaining { get; set; } = 0;
    }

    public class CharacterBondRecord
    {
        public string Id { get; set; }
        public string CharAId { get; set; }
        public string CharAName { get; set; }
        public string CharBId { get; set; }
        public string CharBName { get; set; }
        public string BondTypeName { get; set; } = "Comrades";
        public int Level { get; set; } = 1;
        public int GamesTogether { get; set; } = 0;
    }

    public class PendingBondDraft
    {
        public string CharAId;
        public string CharAName;
        public string CharBId;
        public string CharBName;
        public CharacterBondType BondType;
    }

    public class ActiveCharacterDraft
    {
        public bool IsReturning;
        public bool IsNpc;
        public string ExistingCharacterId;
        public string Name;
        public string NpcType = "Tüccar";
        public string LinkedSpecialCharId = "";
        public string LinkedSpecialCharName = "";
        public string ArchetypeId = "classic_hero";
        public CharacterRole Role = CharacterRole.Protagonist;
        public CharacterTrait Trait = CharacterTrait.None;
        public string TeamName = "";
        public float[] Axes = new float[] { 0.50f, 0.50f, 0.50f, 0.50f };
        public CharacterFate PlannedFate = CharacterFate.Alive;
        public bool AxesEvolved = false;
    }

    public class TransmediaProjectRecord
    {
        public string Id { get; set; }
        public string TypeName { get; set; }
        public string Title { get; set; }
        public string FocusCharacterId { get; set; } = "";
        public string FocusCharacterName { get; set; } = "Genel Evren";
        public double Cost { get; set; }
        public double StartWeek { get; set; }
        public double FinishWeek { get; set; }
        public bool IsCompleted { get; set; }
        public double RevenueEarned { get; set; }
        public int FansGained { get; set; }
        public string ResultSummary { get; set; } = "";
    }

    public class GameEventOutcome
    {
        public double ScoreDelta = 0.0;
        public int HypeDelta = 0;
        public int IntegrityDelta = 0;
        public int BarrierDelta = 0;
        public bool IsCrossover = false;
        public CrossoverMode CrossoverStyle = CrossoverMode.None;
        public string CrossoverSummary = "";
        public List<string> Notes = new List<string>();
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
        public bool WasCrossover { get; set; }
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
        public string CosmologyLawName { get; set; } = "None";
        public int CanonIntegrity { get; set; } = 100;
        public int LoreBarrier { get; set; } = 0;
        public int ConsecutiveSideProjects { get; set; } = 0;
        public int LoreLinkedGameCount { get; set; } = 0;
        public int PendingPostCreditsHype { get; set; } = 0;
        public int PendingTransmediaHype { get; set; } = 0;
        public double PendingTransmediaScoreBonus { get; set; } = 0.0;
        public int CompletedCrossovers { get; set; } = 0;
        public int SoloBurnout { get; set; } = 0;
        public double LastSoloRestWeek { get; set; } = 0.0;
        public double LastRivalChallengeWeek { get; set; } = -10.0;
        public List<string> Teams { get; set; } = new List<string>();
        public List<CharacterRecord> Characters { get; set; } = new List<CharacterRecord>();
        public List<CharacterBondRecord> Bonds { get; set; } = new List<CharacterBondRecord>();
        public List<TransmediaProjectRecord> TransmediaProjects { get; set; } = new List<TransmediaProjectRecord>();
        public List<ReleasedGameRecord> ReleasedGames { get; set; } = new List<ReleasedGameRecord>();
        public List<RivalStudioRecord> Rivals { get; set; } = new List<RivalStudioRecord>();
        public List<RivalNewsEntry> RivalNews { get; set; } = new List<RivalNewsEntry>();
    }

    public static class Phase1Database
    {
        public static readonly string[] CharacterNamePool = new[]
        {
            "Afor", "Feil", "Nıraz", "Saek", "Yejen", "Kael",
            "Lyra", "Toren", "Mira", "Vex", "Sora", "Darian",
            "Elen", "Riven", "Kora", "Orin", "Zeph", "Nora",
            "Cyrus", "Ayla", "Juno"
        };

        public static readonly string[] NpcNamePool = new[]
        {
            "Handaki Barmen", "Kütüphane Bekçisi", "Yaşlı Saatçi", "Gece Kuryesi",
            "İstasyon Memuru", "Tüccar Riko", "Liman Kılavuzu", "Gizemli Yolcu",
            "Arşiv Çırağı", "Fener Ustası", "Mahalle Bakkalı", "Sokak Çalgıcısı"
        };

        public static readonly string[] NpcTypes = new[]
        {
            "Tüccar", "Muhafız", "Komşu / Halk",
            "Gizemli Yabancı", "Bilgin / Arşivci", "Korsan / Kaçakçı"
        };

        public static readonly List<RivalStudioTemplate> RivalTemplates = new List<RivalStudioTemplate>
        {
            new RivalStudioTemplate
            {
                Id = "rival_njg",
                Name = "NJG Games",
                UniverseName = "Tentiforverse",
                Tone = UniverseTone.DarkMystery,
                ArchetypeStyle = "Evren Mimarı",
                BaseSkill = 7.7,
                Variance = 1.2,
                ReleaseCadenceWeeks = 18,
                StartingFans = 180,
                TitlePool = new[] { "Delilik: Gece Vardiyası", "Beyaz Taş", "Durak E25", "Saek Luyot", "Kütüphane Nöbeti", "Siyah Ruh", "Yıldız Koşusu" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_megacorp",
                Name = "Hyperion Interactive",
                UniverseName = "Hyperion Saga",
                Tone = UniverseTone.EpicAdventure,
                ArchetypeStyle = "Dev Holding",
                BaseSkill = 7.1,
                Variance = 1.8,
                ReleaseCadenceWeeks = 13,
                StartingFans = 260,
                TitlePool = new[] { "Hyperion: Şafak", "Altın Kılıç IV", "İmparatorluk Çarpışması", "Kraliyet İttifakı", "Sonsuz Cephe", "Efsaneler Ligi" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_pixelforge",
                Name = "Pixel Forge Indie",
                UniverseName = "Küller ve Yıldızlar",
                Tone = UniverseTone.EpicAdventure,
                ArchetypeStyle = "Sanatçı Indie",
                BaseSkill = 8.2,
                Variance = 0.9,
                ReleaseCadenceWeeks = 26,
                StartingFans = 120,
                TitlePool = new[] { "Küllerin Şarkısı", "Fener Bekçisi", "Sessiz Liman", "Yıldız Haritacısı", "Kırık Pusula", "Gölge Bahçesi" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_quantum",
                Name = "Quantum Core Labs",
                UniverseName = "Protokol-9",
                Tone = UniverseTone.SciFiTactical,
                ArchetypeStyle = "Taktik Deha",
                BaseSkill = 7.5,
                Variance = 1.1,
                ReleaseCadenceWeeks = 20,
                StartingFans = 150,
                TitlePool = new[] { "Protokol-9", "Yörünge Kuşatması", "Siber Çekirdek", "Zaman Kırılması", "Vektör Sıfır", "Koloni Komutanı" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_kaos",
                Name = "Kaos Stüdyo",
                UniverseName = "Gülmece Çoklu Evreni",
                Tone = UniverseTone.LightComedy,
                ArchetypeStyle = "Viral Kaos",
                BaseSkill = 6.7,
                Variance = 2.1,
                ReleaseCadenceWeeks = 15,
                StartingFans = 200,
                TitlePool = new[] { "Çılgın Kurye", "Uzayda Çay Molası", "Kaos Akademisi", "Süper Ördekler", "Yanlış Boyut!", "Tersine Dünya" }
            }
        };

        public static RivalStudioTemplate FindRivalTemplate(string id)
        {
            for (int i = 0; i < RivalTemplates.Count; i++)
                if (RivalTemplates[i].Id == id) return RivalTemplates[i];
            return RivalTemplates[0];
        }

        public static readonly List<TransmediaDef> TransmediaCatalog = new List<TransmediaDef>
        {
            new TransmediaDef
            {
                Type = TransmediaType.ComicSeries,
                Name = "Çizgi Roman Serisi",
                Cost = 25000.0,
                DurationWeeks = 4,
                ShortEffect = "Lore Bariyeri -20, sıradaki oyuna +10 Hype, yeni hayran & karakter +10 Popülerlik",
                DefaultTitlePrefix = "Çizgi Roman"
            },
            new TransmediaDef
            {
                Type = TransmediaType.Novel,
                Name = "Evren Romanı",
                Cost = 40000.0,
                DurationWeeks = 6,
                ShortEffect = "Tutarlılık +10, sıradaki oyuna +%3 Skor & +6 Hype, karakter +12 Popülerlik & kitap geliri",
                DefaultTitlePrefix = "Roman"
            },
            new TransmediaDef
            {
                Type = TransmediaType.ConceptAlbum,
                Name = "Konsept Albüm & Rap EP",
                Cost = 30000.0,
                DurationWeeks = 5,
                ShortEffect = "Sıradaki oyuna +16 Hype, dev hayran akışı, karakter +8 Popülerlik & dinlenme geliri",
                DefaultTitlePrefix = "Albüm"
            }
        };

        public static readonly List<CharacterArchetypeDef> Archetypes = new List<CharacterArchetypeDef>
        {
            new CharacterArchetypeDef
            {
                Id = "classic_hero", Name = "Klasik Kahraman", ShortDesc = "Cesur, lider, idealist",
                PreferredTone = UniverseTone.EpicAdventure, ClashingTone = UniverseTone.DarkMystery,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.6 }, { "Strategy", 0.7 }, { "Casual", 0.8 } },
                PresetAxes = new[] { 0.72f, 0.65f, 0.72f, 0.18f }
            },
            new CharacterArchetypeDef
            {
                Id = "dark_antihero", Name = "Karanlık Anti-Kahraman", ShortDesc = "Sert, yaralı, gri ahlak",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 0.9 }, { "RPG", 1.0 }, { "Simulation", 0.5 }, { "Strategy", 0.8 }, { "Casual", 0.4 } },
                PresetAxes = new[] { 0.78f, 0.15f, 0.25f, 0.62f }
            },
            new CharacterArchetypeDef
            {
                Id = "mystery_strategist", Name = "Gizemli Stratejist", ShortDesc = "Zeki, soğukkanlı, hesapçı",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.6 }, { "Adventure", 0.9 }, { "RPG", 0.9 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.5 } },
                PresetAxes = new[] { 0.18f, 0.35f, 0.28f, 0.58f }
            },
            new CharacterArchetypeDef
            {
                Id = "cheerful_mascot", Name = "Neşeli Maskot", ShortDesc = "Enerjik, esprili, sıcak",
                PreferredTone = UniverseTone.LightComedy, ClashingTone = UniverseTone.DarkMystery,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.7 }, { "Adventure", 0.8 }, { "RPG", 0.6 }, { "Simulation", 0.8 }, { "Strategy", 0.4 }, { "Casual", 1.0 } },
                PresetAxes = new[] { 0.35f, 0.92f, 0.85f, 0.28f }
            },
            new CharacterArchetypeDef
            {
                Id = "wise_mentor", Name = "Bilge Mentor", ShortDesc = "Yol gösterici, derin, bilge",
                PreferredTone = UniverseTone.EpicAdventure, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.9 }, { "Casual", 0.6 } },
                PresetAxes = new[] { 0.22f, 0.45f, 0.65f, 0.15f }
            },
            new CharacterArchetypeDef
            {
                Id = "charismatic_villain", Name = "Karizmatik Kötü", ShortDesc = "İddialı, bencil, akılda kalıcı",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.9 }, { "Adventure", 0.9 }, { "RPG", 1.0 }, { "Simulation", 0.5 }, { "Strategy", 1.0 }, { "Casual", 0.4 } },
                PresetAxes = new[] { 0.58f, 0.32f, 0.48f, 0.92f }
            },
            new CharacterArchetypeDef
            {
                Id = "rebel_outlaw", Name = "Asi Kaçak", ShortDesc = "Kuralsız, özgür ruhlu",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.None,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.6 }, { "Strategy", 0.7 }, { "Casual", 0.7 } },
                PresetAxes = new[] { 0.75f, 0.58f, 0.45f, 0.68f }
            },
            new CharacterArchetypeDef
            {
                Id = "tragic_survivor", Name = "Trajik Hayatta Kalan", ShortDesc = "Dram yüklü, fedakâr",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.6 }, { "Casual", 0.4 } },
                PresetAxes = new[] { 0.42f, 0.12f, 0.48f, 0.22f }
            },
            new CharacterArchetypeDef
            {
                Id = "cold_hunter", Name = "Soğukkanlı Avcı", ShortDesc = "Az konuşan, yüksek aksiyon",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.8 }, { "Simulation", 0.6 }, { "Strategy", 0.9 }, { "Casual", 0.5 } },
                PresetAxes = new[] { 0.88f, 0.22f, 0.15f, 0.48f }
            },
            new CharacterArchetypeDef
            {
                Id = "chaotic_trickster", Name = "Kaotik Düzenbaz", ShortDesc = "Sürprizli, kurnaz, esprili",
                PreferredTone = UniverseTone.LightComedy, ClashingTone = UniverseTone.None,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.6 }, { "Strategy", 0.8 }, { "Casual", 0.9 } },
                PresetAxes = new[] { 0.40f, 0.85f, 0.52f, 0.80f }
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
            new CustomEngineFeatureDef { Id = "njg_branching_narrative", Name = "Dallanan Hikaye & Çoklu Son", Description = "Oyuncu seçimlerine göre dallanan anlatı (+%6 Skor, +5 Hype & seçimli olay)." },
            new CustomEngineFeatureDef { Id = "njg_soundtrack_rap", Name = "Stüdyo İçi Soundtrack & Rap", Description = "Özgün oyun müzikleri ve rap tanıtım parçaları (+15 Hype, +%3 Skor & olay)." },
            new CustomEngineFeatureDef { Id = "njg_post_credits", Name = "Jenerik Sonrası Sahne", Description = "Bir sonraki oyuna +12 Hype aktarımı yapan gizli kapanış sahnesi." },
            new CustomEngineFeatureDef { Id = "njg_mod_support", Name = "Resmi Mod Desteği", Description = "Topluluk modlarıyla ekstra hayran, +%4 Skor, +8 Hype & topluluk olayı." }
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
    // 2. KAYIT SLOTU BAZLI JSON EVREN, KARAKTER, EKIP, TRANSMEDYA & RAKIP KAYDI
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
            UniverseTone tone;
            if (Enum.TryParse<UniverseTone>(Current.UniverseToneName, out tone)) return tone;
            return UniverseTone.None;
        }

        public static void SetTone(UniverseTone tone)
        {
            Current.UniverseToneName = tone.ToString();
            Save();
        }

        public static CosmologyLaw GetLaw()
        {
            CosmologyLaw law;
            if (Enum.TryParse<CosmologyLaw>(Current.CosmologyLawName, out law)) return law;
            return CosmologyLaw.None;
        }

        public static void SetLaw(CosmologyLaw law)
        {
            Current.CosmologyLawName = law.ToString();
            Save();
        }

        public static string RegisterTeam(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return "";
            string clean = rawName.Trim();
            if (string.Equals(clean, "Bağımsız", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clean, "Yok", StringComparison.OrdinalIgnoreCase))
                return "";

            if (clean.Length > 22) clean = clean.Substring(0, 22);
            if (Current.Teams == null) Current.Teams = new List<string>();
            for (int i = 0; i < Current.Teams.Count; i++)
                if (string.Equals(Current.Teams[i], clean, StringComparison.OrdinalIgnoreCase))
                    return Current.Teams[i];

            Current.Teams.Add(clean);
            return clean;
        }

        public static List<CharacterRecord> GetSpecialCharacters()
        {
            var res = new List<CharacterRecord>();
            for (int i = 0; i < Current.Characters.Count; i++)
                if (!Current.Characters[i].IsNpc) res.Add(Current.Characters[i]);
            return res;
        }

        public static List<CharacterRecord> GetNpcCharacters()
        {
            var res = new List<CharacterRecord>();
            for (int i = 0; i < Current.Characters.Count; i++)
                if (Current.Characters[i].IsNpc) res.Add(Current.Characters[i]);
            return res;
        }

        public static float[] EnsureCharacterAxes(CharacterRecord rec)
        {
            if (rec == null) return new[] { 0.5f, 0.5f, 0.5f, 0.5f };
            if (rec.Axes != null && rec.Axes.Length == 4) return rec.Axes;
            if (rec.Sliders != null && rec.Sliders.Length == 9)
                rec.Axes = CharacterAxisSystem.ConvertLegacySlidersToAxes(rec.Sliders);
            else
                rec.Axes = (float[])Phase1Database.FindArchetype(rec.ArchetypeId).PresetAxes.Clone();
            return rec.Axes;
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

                if (File.Exists(targetPath))
                {
                    if (TryLoadFromPath(targetPath))
                    {
                        if (company != null)
                        {
                            Current.CompanyUid = company.uid ?? "";
                            Current.CompanyName = company.name ?? "";
                        }
                        RivalSimulationSystem.EnsureRivalsInitialized(company);
                        MelonLogger.Msg($"[Universe Architect] Evren kaydi yuklendi [{_activeSaveKey}]! ({Current.ReleasedGames.Count} oyun, {GetSpecialCharacters().Count} ozel karakter, {GetNpcCharacters().Count} NPC, {Current.Rivals.Count} rakip)");
                        return;
                    }
                }

                bool isExistingCareer = company != null && ((company.gameLog != null && company.gameLog.Count > 0) || company.currentWeek >= 12.0);
                if (isExistingCareer && !string.IsNullOrEmpty(_activeSaveKey) && File.Exists(LegacySavePath))
                {
                    if (TryLoadFromPath(LegacySavePath))
                    {
                        Current.CompanyUid = company.uid ?? "";
                        Current.CompanyName = company.name ?? "";
                        RivalSimulationSystem.EnsureRivalsInitialized(company);
                        Save();
                        MelonLogger.Msg($"[Universe Architect] Eski evren kaydi bu sirkete tasindi [{_activeSaveKey}]!");
                        return;
                    }
                }

                Current = new UniverseSaveData
                {
                    CompanyUid = company != null ? (company.uid ?? "") : "",
                    CompanyName = company != null ? (company.name ?? "") : ""
                };
                RivalSimulationSystem.EnsureRivalsInitialized(company);
                Save();
                MelonLogger.Msg($"[Universe Architect] Yeni evren kaydi baslatildi [{(string.IsNullOrEmpty(_activeSaveKey) ? "default" : _activeSaveKey)}] ({Current.Rivals.Count} rakip studyo aktif).");
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
            if (Current.Teams == null) Current.Teams = new List<string>();
            if (Current.Characters == null) Current.Characters = new List<CharacterRecord>();
            if (Current.Bonds == null) Current.Bonds = new List<CharacterBondRecord>();
            if (Current.ReleasedGames == null) Current.ReleasedGames = new List<ReleasedGameRecord>();
            if (Current.TransmediaProjects == null) Current.TransmediaProjects = new List<TransmediaProjectRecord>();
            if (Current.Rivals == null) Current.Rivals = new List<RivalStudioRecord>();
            if (Current.RivalNews == null) Current.RivalNews = new List<RivalNewsEntry>();
            Current.CanonIntegrity = Math.Clamp(Current.CanonIntegrity, 0, 100);
            Current.LoreBarrier = Math.Clamp(Current.LoreBarrier, 0, 100);
            Current.SoloBurnout = Math.Clamp(Current.SoloBurnout, 0, 100);

            // Eski v5.0.1 karakterlerini yeni 4 eksen yapisina ve ekip listesine guvenlice tasi
            for (int i = 0; i < Current.Characters.Count; i++)
            {
                var c = Current.Characters[i];
                if (!c.IsNpc) EnsureCharacterAxes(c);
                if (!string.IsNullOrWhiteSpace(c.TeamName)) RegisterTeam(c.TeamName);
            }
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
        public static void AdjustSoloBurnout(int delta) => Current.SoloBurnout = Math.Clamp(Current.SoloBurnout + delta, 0, 100);

        public static CharacterRecord FindCharacterById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Current.Characters.Count; i++)
                if (Current.Characters[i].Id == id) return Current.Characters[i];
            return null;
        }

        public static int GetActiveTransmediaCount()
        {
            int count = 0;
            for (int i = 0; i < Current.TransmediaProjects.Count; i++)
                if (!Current.TransmediaProjects[i].IsCompleted) count++;
            return count;
        }

        public static void AddRivalNews(double week, string studioName, string headline, string detail, string tag)
        {
            Current.RivalNews.Add(new RivalNewsEntry
            {
                Week = Math.Round(week, 1),
                StudioName = studioName,
                Headline = headline,
                Detail = detail,
                Tag = tag
            });
            if (Current.RivalNews.Count > 50)
                Current.RivalNews.RemoveRange(0, Current.RivalNews.Count - 50);
        }

        public static CharacterBondRecord FindBond(string idA, string idB)
        {
            if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB) || Current.Bonds == null) return null;
            for (int i = 0; i < Current.Bonds.Count; i++)
            {
                var b = Current.Bonds[i];
                if ((b.CharAId == idA && b.CharBId == idB) || (b.CharAId == idB && b.CharBId == idA))
                    return b;
            }
            return null;
        }

        public static CharacterBondRecord UpsertBond(string idA, string nameA, string idB, string nameB, CharacterBondType type)
        {
            if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB) || idA == idB) return null;
            if (Current.Bonds == null) Current.Bonds = new List<CharacterBondRecord>();
            var existing = FindBond(idA, idB);
            if (existing != null)
            {
                existing.BondTypeName = type.ToString();
                existing.CharAName = existing.CharAId == idA ? nameA : nameB;
                existing.CharBName = existing.CharBId == idB ? nameB : nameA;
                return existing;
            }
            var rec = new CharacterBondRecord
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                CharAId = idA,
                CharAName = nameA,
                CharBId = idB,
                CharBName = nameB,
                BondTypeName = type.ToString(),
                Level = 1,
                GamesTogether = 0
            };
            Current.Bonds.Add(rec);
            return rec;
        }

        public static void RestUnusedCharacters(HashSet<string> usedCharIds)
        {
            for (int i = 0; i < Current.Characters.Count; i++)
            {
                var c = Current.Characters[i];
                if (usedCharIds == null || !usedCharIds.Contains(c.Id))
                {
                    c.ConsecutiveUses = 0;
                    if (!c.IsNpc && CategoryLabels.ParseFate(c.FateName) == CharacterFate.Alive)
                        c.GamesMissed = Math.Min(10, c.GamesMissed + 1);
                    if (c.SpinOffDemandRemaining > 0)
                        c.SpinOffDemandRemaining--;
                }
            }
        }

        public static void RecordReleasedGame(Game game, string topicId, double score, ProjectCategory category, List<string> modFeatures, List<string> castSummary, bool wasCrossover)
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
                WasCrossover = wasCrossover,
                CharacterName = (castSummary != null && castSummary.Count > 0) ? string.Join(", ", castSummary) : "",
                CastSummary = castSummary ?? new List<string>(),
                UsedModFeatures = modFeatures ?? new List<string>(),
                RecordedAtUtc = DateTime.UtcNow.ToString("o")
            });
            Save();
        }
    }

    // =========================================================================
    // 3. RESEARCH SABLON REFLECTION, FEATURE OKUYUCU & BILDIRIM YARDIMCILARI
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

    public static class GameFeatureReader
    {
        public static HashSet<string> ExtractAllFeatureIds(Game game)
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

            try
            {
                var gc = UniverseArchitectMod.ActiveGameController;
                if (gc != null && gc.selectedGameFeatures != null)
                {
                    for (int i = 0; i < gc.selectedGameFeatures.Count; i++)
                        if (gc.selectedGameFeatures[i] != null && !string.IsNullOrEmpty(gc.selectedGameFeatures[i].id))
                            ids.Add(gc.selectedGameFeatures[i].id);
                }
            }
            catch { }

            return ids;
        }
    }

    public static class NotificationHelper
    {
        public static void Show(Company company, string header, string text, bool isStoryAlert = false)
        {
            MelonLogger.Msg($"[Universe Architect Bildirim] {header}: {text}");
            if (company == null) return;
            try
            {
                var notif = new Notification(header, text);
                if (isStoryAlert)
                {
                    try { notif.isStoryAlert = true; } catch { }
                }
                if (company.notifications != null) company.notifications.Add(notif);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] Oyun ici bildirim eklenemedi: {ex.Message}");
            }
        }
    }

    // =========================================================================
    // 4. NATIVE COK-BUTONLU PENCERE KUYRUGU (FIFO KUYRUK)
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
        private static readonly Queue<PendingPopup> _queue = new Queue<PendingPopup>();
        private static float _delayTimer = 0f;

        public static bool HasPending => _queue.Count > 0;

        public static void Enqueue(string header, string text, string[] buttons, Action<int> onSelected)
        {
            _queue.Enqueue(new PendingPopup { Header = header, Text = text, Buttons = buttons, OnSelected = onSelected });
            if (_queue.Count == 1) _delayTimer = 0.14f;
        }

        public static void Clear()
        {
            _queue.Clear();
            _delayTimer = 0f;
        }

        public static void Tick(float dt)
        {
            if (_queue.Count == 0 || CharacterWorkshopUGUI.IsOpen) return;
            if (_delayTimer > 0f) { _delayTimer -= dt; return; }

            try
            {
                var panel = UnityEngine.Object.FindObjectOfType<PanelNotifications>();
                if (panel == null || panel.IsNotificationVisible) return;

                var p = _queue.Dequeue();
                _delayTimer = 0.25f;

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
            }
        }
    }

    // =========================================================================
    // 5. FAZ 5: 6 OZEL KONU NATIVE IKON ESLESTIRICI (ANDROID 15 SIGSEGV KORUMALI)
    // =========================================================================
    public static class TopicIconInjector
    {
        public static bool IconsInjected = false;

        public static void TryInjectIcons()
        {
            if (IconsInjected) return;
            try
            {
                GameConfig cfg = null;
                if (UniverseArchitectMod.ActiveGameController != null)
                    cfg = UniverseArchitectMod.ActiveGameController.gameConfig;
                if (cfg == null)
                {
                    var allCfgs = UnityEngine.Resources.FindObjectsOfTypeAll<GameConfig>();
                    if (allCfgs != null && allCfgs.Length > 0) cfg = allCfgs[0];
                }
                if (cfg == null) return;

                var mapObj = SafeReflection.GetMember(cfg, "nameToSpriteMap");
                var spriteMap = mapObj as Il2CppSystem.Collections.Generic.Dictionary<string, Sprite>;
                if (spriteMap == null && mapObj is Il2CppSystem.Object ilObj)
                    spriteMap = ilObj.TryCast<Il2CppSystem.Collections.Generic.Dictionary<string, Sprite>>();
                if (spriteMap == null || spriteMap.Count == 0) return;

                // Oyunun mevcut konu ikon isim desenini guvenli sekilde tespit et
                string detectedPrefix = "";
                try
                {
                    var sampleSprite = cfg.GetTopicIconSprite("Space");
                    if (sampleSprite != null && !string.IsNullOrEmpty(sampleSprite.name))
                    {
                        int idx = sampleSprite.name.IndexOf("Space", StringComparison.OrdinalIgnoreCase);
                        if (idx > 0) detectedPrefix = sampleSprite.name.Substring(0, idx);
                    }
                }
                catch { }

                // Android 15 IL2CPP kurali: Texture2D / Sprite.Create cagrilmaz!
                // Oyunun hafizadaki mevcut native ikonlari sablon anahtarina gore eslestirilir.
                int mappedCount = 0;
                for (int i = 0; i < Phase1Database.Topics.Count; i++)
                {
                    var topic = Phase1Database.Topics[i];
                    Sprite nativeSprite = FindExistingTopicSprite(cfg, topic.IconTemplateKeyword);
                    if (nativeSprite == null) continue;

                    string primaryKey = string.IsNullOrEmpty(detectedPrefix) ? topic.Id : (detectedPrefix + topic.Id);
                    spriteMap[topic.Id] = nativeSprite;
                    spriteMap[primaryKey] = nativeSprite;
                    spriteMap["topic_" + topic.Id] = nativeSprite;
                    spriteMap["icon_topic_" + topic.Id] = nativeSprite;
                    mappedCount++;
                }

                IconsInjected = true;
                MelonLogger.Msg($"[Faz 5] {mappedCount} ozel konu native ikonlarla eslestirildi (Desen: '{detectedPrefix}').");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 5] Konu ikonu eslestirme hatasi: {ex.Message}");
                IconsInjected = true;
            }
        }

        private static Sprite FindExistingTopicSprite(GameConfig cfg, string keyword)
        {
            if (cfg == null || string.IsNullOrEmpty(keyword)) return null;

            try
            {
                var direct = cfg.GetTopicIconSprite(keyword);
                if (direct != null) return direct;
            }
            catch { }

            try
            {
                if (cfg.allSpriteIcons != null)
                {
                    for (int i = 0; i < cfg.allSpriteIcons.Count; i++)
                    {
                        var sp = cfg.allSpriteIcons[i];
                        if (sp != null && !string.IsNullOrEmpty(sp.name) &&
                            sp.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                            return sp;
                    }
                }
            }
            catch { }

            return null;
        }
    }

    // =========================================================================
    // 6. FAZ 5: 5 YASAYAN RAKIP STUDYO SIMULASYONU, SIRALAMA & RAKIP HABERLERI
    // =========================================================================
    public class LeaderboardEntry
    {
        public string StudioName;
        public string UniverseName;
        public string StyleOrLaw;
        public bool IsPlayer;
        public long Fans;
        public int Integrity;
        public double AvgScore;
        public int GamesCount;
        public int PowerScore;
    }

    public static class RivalSimulationSystem
    {
        public static void EnsureRivalsInitialized(Company company)
        {
            var u = UniverseSaveManager.Current;
            if (u.Rivals == null) u.Rivals = new List<RivalStudioRecord>();
            if (u.RivalNews == null) u.RivalNews = new List<RivalNewsEntry>();

            double baseWeek = company != null ? company.currentWeek : 0.0;
            bool addedAny = false;

            for (int i = 0; i < Phase1Database.RivalTemplates.Count; i++)
            {
                var tpl = Phase1Database.RivalTemplates[i];
                bool exists = false;
                for (int j = 0; j < u.Rivals.Count; j++)
                    if (u.Rivals[j].Id == tpl.Id) { exists = true; break; }
                if (exists) continue;

                u.Rivals.Add(new RivalStudioRecord
                {
                    Id = tpl.Id,
                    Name = tpl.Name,
                    UniverseName = tpl.UniverseName,
                    ToneName = tpl.Tone.ToString(),
                    ArchetypeStyle = tpl.ArchetypeStyle,
                    Fans = tpl.StartingFans,
                    CanonIntegrity = 85,
                    GamesReleased = 0,
                    LastScore = 0.0,
                    AverageScore = 0.0,
                    LastGameTitle = "İlk proje geliştiriliyor",
                    NextReleaseWeek = baseWeek + 6.0 + (i * 3.5),
                    CrossoversDone = 0
                });
                addedAny = true;
            }

            if (addedAny && u.RivalNews.Count == 0)
            {
                UniverseSaveManager.AddRivalNews(
                    baseWeek,
                    "Sektör Bülteni",
                    "5 Rakip Evren Stüdyosu Sahneye Çıktı!",
                    "NJG Games (Tentiforverse), Hyperion Interactive, Pixel Forge Indie, Quantum Core Labs ve Kaos Stüdyo kendi kurgusal evrenlerini inşa etmeye başladı.",
                    "HABER"
                );
            }
        }

        public static void Tick(Company company)
        {
            if (company == null) return;
            EnsureRivalsInitialized(company);

            var u = UniverseSaveManager.Current;
            double week = company.currentWeek;
            bool anyChange = false;

            for (int i = 0; i < u.Rivals.Count; i++)
            {
                var r = u.Rivals[i];
                if (week < r.NextReleaseWeek) continue;

                SimulateRivalRelease(company, r, week);
                anyChange = true;
            }

            // Zayif karin saldirisi (Nadir: en az 20 hafta arayla ve bariyer/tutarlilik kritikse)
            if (week - u.LastRivalChallengeWeek >= 20.0 && u.ReleasedGames.Count >= 2 &&
                !NativePopupQueue.HasPending && !CharacterWorkshopUGUI.IsOpen && !UniverseArchitectMod.IsNativeModalOpen())
            {
                if (u.LoreBarrier >= 65 || u.CanonIntegrity <= 60)
                {
                    u.LastRivalChallengeWeek = week;
                    TriggerRivalWeaknessAttack(company);
                    anyChange = true;
                }
            }

            if (anyChange) UniverseSaveManager.Save();
        }

        private static void SimulateRivalRelease(Company company, RivalStudioRecord r, double week)
        {
            var tpl = Phase1Database.FindRivalTemplate(r.Id);
            double rnd1 = 0.5, rnd2 = 0.5;
            try { rnd1 = company.getRandom(); rnd2 = company.getRandom(); } catch { }

            // Rakip skoru: temel yetenek + varyans + yillar ilerledikce hafif ustalik
            double eraBonus = Math.Min(0.8, week / 250.0);
            double rawScore = tpl.BaseSkill + eraBonus + (rnd1 - 0.5) * 2.0 * tpl.Variance;
            bool isCrossover = r.GamesReleased >= 2 && rnd2 > 0.78;
            if (isCrossover) rawScore += (rnd1 > 0.45 ? 0.6 : -0.7);
            double score = Math.Round(Math.Clamp(rawScore, 3.8, 9.7), 2);

            int titleIdx = r.GamesReleased % tpl.TitlePool.Length;
            int cycle = (r.GamesReleased / tpl.TitlePool.Length) + 1;
            string gameTitle = cycle > 1 ? $"{tpl.TitlePool[titleIdx]} {cycle}" : tpl.TitlePool[titleIdx];

            r.AverageScore = r.GamesReleased == 0
                ? score
                : Math.Round(((r.AverageScore * r.GamesReleased) + score) / (r.GamesReleased + 1), 2);
            r.GamesReleased++;
            r.LastScore = score;
            r.LastGameTitle = gameTitle;
            if (isCrossover) r.CrossoversDone++;

            // Hayran & Tutarlilik degisimi
            double fanScale = Math.Max(1.0, Math.Pow(Math.Max(1.0, week / 25.0), 1.35));
            long fanDelta = 0;
            string tag = "HABER";
            string headline;

            if (score >= 8.4)
            {
                fanDelta = (long)Math.Round((140 + rnd2 * 180) * fanScale);
                r.CanonIntegrity = Math.Clamp(r.CanonIntegrity + 5, 10, 100);
                tag = isCrossover ? "CROSSOVER" : "HIT";
                headline = isCrossover
                    ? $"{r.Name} dev Crossover '{gameTitle}' ile {score:F1} puan topladı!"
                    : $"{r.Name} başyapıt çıkardı: '{gameTitle}' ({score:F1}/10)";
            }
            else if (score < 5.8)
            {
                fanDelta = -(long)Math.Round(Math.Min(r.Fans * 0.12, 120 * fanScale));
                r.CanonIntegrity = Math.Clamp(r.CanonIntegrity - 10, 10, 100);
                tag = "FIYASKO";
                headline = $"{r.Name} tökezledi: '{gameTitle}' sadece {score:F1} alabildi!";
            }
            else
            {
                fanDelta = (long)Math.Round((45 + rnd2 * 70) * fanScale);
                r.CanonIntegrity = Math.Clamp(r.CanonIntegrity + 1, 10, 100);
                headline = $"{r.Name} yeni oyunu '{gameTitle}' ({score:F1}/10) ile sahnede.";
            }

            r.Fans = Math.Max(50, r.Fans + fanDelta);
            r.NextReleaseWeek = week + tpl.ReleaseCadenceWeeks + Math.Round((rnd2 - 0.5) * 6.0);

            string detail = $"Evren: {r.UniverseName} ({CategoryLabels.ToneToTurkish(tpl.Tone)}) | Tutarlılık: {r.CanonIntegrity}/100 | Hayran: {r.Fans:N0}";

            // Ayni Evren Tonu'nda rekabet etkisi!
            var playerTone = UniverseSaveManager.GetTone();
            if (playerTone != UniverseTone.None && tpl.Tone == playerTone && score >= 8.5)
            {
                company.adjustHype(-4);
                detail += " | Aynı tonda çıkardığı hit senin Hype'ından biraz çaldı (-4 Hype).";
                tag = "REKABET";
            }

            UniverseSaveManager.AddRivalNews(week, r.Name, headline, detail, tag);
            MelonLogger.Msg($"[Faz 5 Rakip] {headline} (Hayran: {r.Fans})");
        }

        public static void OnPlayerGameReleased(Company company, string title, double score, ProjectCategory category, bool wasCrossover)
        {
            if (company == null) return;
            EnsureRivalsInitialized(company);
            var u = UniverseSaveManager.Current;
            double week = company.currentWeek;

            // Ayni tonda bir rakibin en yuksek puanini gectin mi -> Taht ele gecirme!
            var playerTone = UniverseSaveManager.GetTone();
            if (score >= 8.0 && category != ProjectCategory.NonUniverse)
            {
                RivalStudioRecord beatenRival = null;
                for (int i = 0; i < u.Rivals.Count; i++)
                {
                    if (u.Rivals[i].ToneName == playerTone.ToString() && u.Rivals[i].GamesReleased > 0 && score > u.Rivals[i].LastScore)
                    {
                        beatenRival = u.Rivals[i];
                        break;
                    }
                }

                if (beatenRival != null)
                {
                    long stolenFans = Math.Max(40, (long)Math.Round(beatenRival.Fans * 0.08));
                    beatenRival.Fans = Math.Max(50, beatenRival.Fans - stolenFans);
                    company.adjustFans(stolenFans);
                    UniverseSaveManager.AddRivalNews(
                        week,
                        beatenRival.Name,
                        $"{title} sonrası {beatenRival.Name} hayranları saf değiştirdi!",
                        $"Aynı tonda ({CategoryLabels.ToneToTurkish(playerTone)}) {score:F2} puanla rakibini geçtin ve +{stolenFans} hayranı kendi evrenine çektin!",
                        "REKABET"
                    );
                }

                // Nadir Klon Dalgasi tepkisi (en az 16 hafta arayla)
                if (week - u.LastRivalChallengeWeek >= 16.0 && !NativePopupQueue.HasPending)
                {
                    u.LastRivalChallengeWeek = week;
                    var copier = u.Rivals[u.ReleasedGames.Count % u.Rivals.Count];
                    TriggerCloneWaveEvent(company, copier, title);
                }
            }

            if (wasCrossover && score >= 7.5)
            {
                UniverseSaveManager.AddRivalNews(
                    week,
                    "Sektör Basını",
                    $"'{title}' Crossover'ı rakipleri panikletti!",
                    "Diğer stüdyolar kendi evrenlerini hızla birleştirme planlarını öne çekti.",
                    "CROSSOVER"
                );
            }
        }

        private static void TriggerCloneWaveEvent(Company company, RivalStudioRecord rival, string hitTitle)
        {
            NativePopupQueue.Enqueue(
                $"Rakip Hamlesi: {rival.Name} Seni Kopyalıyor!",
                $"'{hitTitle}' hitinden sonra {rival.Name} benzer formülle acele bir proje duyurdu! Sektör senin tepkini bekliyor:\n\n" +
                $"• Karşı Tanıtım Patlat (-12K): +15 Hype ve rakibin rüzgarını kes (+hayran).\n" +
                $"• Kaliteye Odaklan (Ücretsiz): +4 Tutarlılık, polemiğe girme.\n" +
                $"• Sert Gönderme Yap (Ücretsiz): +10 Hype ama rakiple rekabet kızışır.",
                new[] { "Karşı Tanıtım (-12K)", "Kaliteye Odaklan", "Sert Gönderme Yap" },
                opt =>
                {
                    if (opt == 0)
                    {
                        company.adjustCash(-12000.0, "Rakibe Karşı Tanıtım");
                        company.adjustHype(15);
                        double gain = Math.Max(60.0, Math.Round(company.fans * 0.06));
                        company.adjustFans(gain);
                        rival.Fans = Math.Max(50, rival.Fans - (long)(gain / 2));
                        UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                            $"{rival.Name}'in klon hamlesi karşı tanıtımla boşa düştü!",
                            $"Oyuncular orijinal evrenin yanında durdu (+{gain:F0} Hayran).", "REKABET");
                    }
                    else if (opt == 1)
                    {
                        UniverseSaveManager.AdjustIntegrity(+4);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                            $"{rival.Name} taklit peşindeyken sen kanonunu güçlendirdin.",
                            "Eleştirmenler bu soğukkanlı duruşu övdü (+4 Tutarlılık).", "HABER");
                    }
                    else
                    {
                        company.adjustHype(10);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                            $"Sektörde söz düellosu! {rival.Name} ile gerilim tırmandı.",
                            "Sosyal medyada iki evrenin hayranları kapıştı (+10 Hype).", "REKABET");
                    }
                    UniverseSaveManager.Save();
                }
            );
        }

        private static void TriggerRivalWeaknessAttack(Company company)
        {
            var u = UniverseSaveManager.Current;
            var attacker = u.Rivals[(int)(company.currentWeek) % u.Rivals.Count];
            string reason = u.LoreBarrier >= 65
                ? $"Lore Bariyerin ({u.LoreBarrier}/100) çok yükseldi"
                : $"Kanon Tutarlılığın ({u.CanonIntegrity}/100) zayıfladı";

            NativePopupQueue.Enqueue(
                $"Rakip Fırsatçılığı: {attacker.Name}",
                $"{reason}! {attacker.Name} bunu fırsat bilip 'Sıfırdan, karmaşasız yeni evren!' sloganıyla senin yorulan kitlene oynuyor:\n\n" +
                $"• Ücretsiz Hikaye Özeti Yayınla (-8K): Lore Bariyeri -15, +5 Tutarlılık.\n" +
                $"• Hayranlara Güven (Ücretsiz): Para harcama ama -6 Hype.",
                new[] { "Hikaye Özeti (-8K)", "Hayranlara Güven" },
                opt =>
                {
                    if (opt == 0)
                    {
                        company.adjustCash(-8000.0, "Evren Özeti Kampanyası");
                        UniverseSaveManager.AdjustLoreBarrier(-15);
                        UniverseSaveManager.AdjustIntegrity(+5);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, attacker.Name,
                            $"{attacker.Name}'in atağını Evren Özeti ile savuşturdun!",
                            "Yeni oyuncular hikayeye kolayca ısındı (Bariyer -15, Tutarlılık +5).", "REKABET");
                    }
                    else
                    {
                        company.adjustHype(-6);
                        attacker.Fans += 80;
                        UniverseSaveManager.AddRivalNews(company.currentWeek, attacker.Name,
                            $"{attacker.Name} kararsız oyunculardan bir kısmını kaptı.",
                            "Kısa vadede -6 Hype yaşandı.", "REKABET");
                    }
                    UniverseSaveManager.Save();
                }
            );
        }

        public static List<LeaderboardEntry> BuildLeaderboard(Company playerCompany)
        {
            var list = new List<LeaderboardEntry>();
            var u = UniverseSaveManager.Current;

            // 1. Oyuncu Studyosu
            double playerAvg = 0.0;
            if (u.ReleasedGames.Count > 0)
            {
                double sum = 0.0;
                for (int i = 0; i < u.ReleasedGames.Count; i++) sum += u.ReleasedGames[i].FinalScore;
                playerAvg = Math.Round(sum / u.ReleasedGames.Count, 2);
            }
            long pFans = playerCompany != null ? playerCompany.fans : 0;
            int pPower = CalcPower(pFans, u.CanonIntegrity, playerAvg, u.ReleasedGames.Count, u.CompletedCrossovers);

            list.Add(new LeaderboardEntry
            {
                StudioName = (playerCompany != null && !string.IsNullOrEmpty(playerCompany.name)) ? playerCompany.name + " (SEN)" : "Senin Stüdyon (SEN)",
                UniverseName = u.UniverseName,
                StyleOrLaw = CategoryLabels.LawToTurkish(UniverseSaveManager.GetLaw()),
                IsPlayer = true,
                Fans = pFans,
                Integrity = u.CanonIntegrity,
                AvgScore = playerAvg,
                GamesCount = u.ReleasedGames.Count,
                PowerScore = pPower
            });

            // 2. 5 Rakip Studyo
            for (int i = 0; i < u.Rivals.Count; i++)
            {
                var r = u.Rivals[i];
                list.Add(new LeaderboardEntry
                {
                    StudioName = r.Name,
                    UniverseName = r.UniverseName,
                    StyleOrLaw = r.ArchetypeStyle,
                    IsPlayer = false,
                    Fans = r.Fans,
                    Integrity = r.CanonIntegrity,
                    AvgScore = r.AverageScore,
                    GamesCount = r.GamesReleased,
                    PowerScore = CalcPower(r.Fans, r.CanonIntegrity, r.AverageScore, r.GamesReleased, r.CrossoversDone)
                });
            }

            list.Sort((a, b) => b.PowerScore.CompareTo(a.PowerScore));
            return list;
        }

        private static int CalcPower(long fans, int integrity, double avgScore, int gamesCount, int crossovers)
        {
            double fanScore = Math.Log10(Math.Max(10, fans)) * 180.0;
            double qualityScore = avgScore * 45.0;
            double loreScore = integrity * 2.5 + gamesCount * 15.0 + crossovers * 60.0;
            return (int)Math.Round(fanScore + qualityScore + loreScore);
        }
    }

    // =========================================================================
    // 7. FAZ 5: SOLO-DEV TUKENMISLIK DENGESI (Company.staff + Character.efficiency)
    // =========================================================================
    public static class SoloBurnoutSystem
    {
        public static bool IsSoloDev(Company company)
        {
            if (company == null || company.staff == null) return true;
            return company.staff.Count <= 1;
        }

        public static void OnWorkCompleted(Company company, int burnoutIncrease)
        {
            if (company == null) return;
            if (!IsSoloDev(company))
            {
                // Ekip kuruldugunda solo tukenmislik hizla erir
                UniverseSaveManager.AdjustSoloBurnout(-25);
                return;
            }

            UniverseSaveManager.AdjustSoloBurnout(burnoutIncrease);
            int b = UniverseSaveManager.Current.SoloBurnout;

            // Yuksek tukenmislik -> Oyunun kendi Character.adjustEfficiency mekanigini tetikle!
            if (b >= 70)
            {
                try
                {
                    var playerChar = company.PlayerCharacter;
                    if (playerChar != null) playerChar.adjustEfficiency(-0.22);
                }
                catch { }

                NotificationHelper.Show(
                    company,
                    $"Solo-Dev Tükenmişlik Uyarısı! (%{b})",
                    "Tek başına aralıksız oyun ve evren yönetimi verimliliğini düşürüyor! Mod İçeriği > Kozmoloji sekmesinden mola vererek veya yeni ekip arkadaşı alarak rahatlayabilirsin.",
                    true
                );
            }
        }

        public static void TickPassiveRecovery(Company company)
        {
            if (company == null) return;
            var u = UniverseSaveManager.Current;
            if (u.SoloBurnout <= 0) return;

            if (!IsSoloDev(company))
            {
                UniverseSaveManager.AdjustSoloBurnout(-10);
                return;
            }

            // Bosta gecen her 4 haftada pasif dinlenme
            if (!company.isCurrentlyDevelopingGame() && (company.currentWeek - u.LastSoloRestWeek) >= 4.0)
            {
                u.LastSoloRestWeek = company.currentWeek;
                UniverseSaveManager.AdjustSoloBurnout(-12);
            }
        }

        public static string TakeSoloBreak(Company company)
        {
            if (company == null) return "Şirket hazır değil.";
            if (UniverseSaveManager.Current.SoloBurnout <= 5) return "Zaten dinçsin, molaya gerek yok!";

            UniverseSaveManager.AdjustSoloBurnout(-45);
            UniverseSaveManager.Current.LastSoloRestWeek = company.currentWeek;
            try
            {
                var pChar = company.PlayerCharacter;
                if (pChar != null)
                {
                    pChar.adjustEfficiency(+0.30);
                    if (pChar.isIdle()) pChar.goOnVacation();
                }
            }
            catch { }

            UniverseSaveManager.Save();
            return $"Kısa bir çay & nefes molası verdin! Tükenmişlik -> %{UniverseSaveManager.Current.SoloBurnout}";
        }
    }

    // =========================================================================
    // 8. FAZ 4: SECIMLI MOTOR OLAYLARI & CROSSOVER ALGILAMA MOTORU
    // =========================================================================
    public static class InteractiveEventSystem
    {
        private static readonly Dictionary<IntPtr, GameEventOutcome> _gameOutcomes = new Dictionary<IntPtr, GameEventOutcome>();
        private static readonly Dictionary<IntPtr, HashSet<string>> _firedEvents = new Dictionary<IntPtr, HashSet<string>>();

        public static void ResetSessionCache()
        {
            _gameOutcomes.Clear();
            _firedEvents.Clear();
        }

        public static GameEventOutcome GetOrCreateOutcome(IntPtr gamePtr)
        {
            if (gamePtr == IntPtr.Zero) return new GameEventOutcome();
            GameEventOutcome outcome;
            if (!_gameOutcomes.TryGetValue(gamePtr, out outcome))
            {
                outcome = new GameEventOutcome();
                _gameOutcomes[gamePtr] = outcome;
            }
            return outcome;
        }

        public static GameEventOutcome GetOutcomeForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero)
            {
                GameEventOutcome outcome;
                if (_gameOutcomes.TryGetValue(game.Pointer, out outcome)) return outcome;
            }
            return null;
        }

        private static bool MarkFired(IntPtr gamePtr, string eventKey)
        {
            HashSet<string> set;
            if (!_firedEvents.TryGetValue(gamePtr, out set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _firedEvents[gamePtr] = set;
            }
            return set.Add(eventKey);
        }

        public static bool DetectCrossoverCast(List<ActiveCharacterDraft> cast, out string pairSummary)
        {
            pairSummary = "";
            if (cast == null || cast.Count < 2) return false;

            var names = new List<string>();
            var seenDebuts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < cast.Count; i++)
            {
                var d = cast[i];
                if (d.IsNpc || !d.IsReturning || string.IsNullOrEmpty(d.ExistingCharacterId)) continue;
                var rec = UniverseSaveManager.FindCharacterById(d.ExistingCharacterId);
                if (rec == null || string.IsNullOrEmpty(rec.DebutGame)) continue;

                if (seenDebuts.Add(rec.DebutGame))
                    names.Add($"{rec.Name} [{rec.DebutGame}]");
            }

            if (seenDebuts.Count >= 2)
            {
                pairSummary = $"{names[0]} × {names[1]}";
                return true;
            }
            return false;
        }

        public static void RegisterCastForEvents(IntPtr gamePtr, string gameTitle, List<ActiveCharacterDraft> cast)
        {
            if (gamePtr == IntPtr.Zero) return;
            string pairSummary;
            if (DetectCrossoverCast(cast, out pairSummary))
            {
                var outcome = GetOrCreateOutcome(gamePtr);
                outcome.IsCrossover = true;
                outcome.CrossoverSummary = pairSummary;
                MelonLogger.Msg($"[Faz 4] CROSSOVER ALGILANDI: '{gameTitle}' -> {pairSummary}");
            }
        }

        public static void PollDuringDevelopment(Company company)
        {
            if (company == null || !company.isCurrentlyDevelopingGame() || company.currentGame == null) return;
            if (NativePopupQueue.HasPending || CharacterWorkshopUGUI.IsOpen || UniverseArchitectMod.IsNativeModalOpen()) return;

            var game = company.currentGame;
            if (game.Pointer == IntPtr.Zero) return;
            if (!UniverseChoicePopupSystem.HasCategoryChosen(game.Pointer)) return;

            double progress = 0.0;
            try
            {
                var gm = UniverseArchitectMod.GetGameManager();
                if (gm != null) progress = gm.getCurrentGameProgress();
            }
            catch { }

            bool hasEnoughPoints = (game.designPoints + game.technologyPoints) >= 4;
            if (progress < 0.25 && !hasEnoughPoints) return;
            if (progress > 0.92) return;

            IntPtr gPtr = game.Pointer;
            string title = !string.IsNullOrEmpty(game.title) ? game.title : "Yeni Oyun";

            var existingOutcome = GetOutcomeForGame(game);
            if (existingOutcome != null && existingOutcome.IsCrossover && existingOutcome.CrossoverStyle == CrossoverMode.None)
            {
                if (MarkFired(gPtr, "crossover_event"))
                {
                    TriggerCrossoverEvent(company, gPtr, title, existingOutcome);
                    return;
                }
            }

            var featIds = GameFeatureReader.ExtractAllFeatureIds(game);
            if (featIds.Contains("njg_branching_narrative") && MarkFired(gPtr, "evt_branching"))
            {
                TriggerBranchingNarrativeEvent(company, gPtr, title);
                return;
            }
            if (featIds.Contains("njg_soundtrack_rap") && MarkFired(gPtr, "evt_soundtrack"))
            {
                TriggerSoundtrackRapEvent(company, gPtr, title);
                return;
            }
            if (featIds.Contains("njg_mod_support") && MarkFired(gPtr, "evt_mod_support"))
            {
                TriggerModSupportEvent(company, gPtr, title);
                return;
            }
        }

        private static void TriggerCrossoverEvent(Company company, IntPtr gPtr, string title, GameEventOutcome outcome)
        {
            NativePopupQueue.Enqueue(
                $"DEV CROSSOVER ZİRVESİ: {title}",
                $"Farklı oyunlardan doğan yıldızlar ({outcome.CrossoverSummary}) aynı projede buluştu! İnternet alev aldı, beklenti tavan.\n\nBu buluşmayı nasıl yöneteceksin?\n\n" +
                $"• Tam Kanon Çarpışması (Yüksek Risk): 7.5+ skorla +%8 Skor, +20 Hype, dev hayran akışı; 6.5 altı skorla ağır Kanon Faciası!\n" +
                $"• Paralel Boyut Geçidi (Dengeli): 7.0+ skorla +%5 Skor, +14 Hype; orta risk.\n" +
                $"• Sürpriz Cameo (Güvenli): Sıfır tutarlılık riski, sabit +%2 Skor & +8 Hype.",
                new[] { "Tam Kanon (Riskli)", "Boyut Geçidi (Dengeli)", "Güvenli Cameo" },
                opt =>
                {
                    if (opt == 0) outcome.CrossoverStyle = CrossoverMode.HighRiskCanon;
                    else if (opt == 1) outcome.CrossoverStyle = CrossoverMode.BalancedPortal;
                    else outcome.CrossoverStyle = CrossoverMode.SafeCameo;
                    MelonLogger.Msg($"[Faz 4] Crossover tarzi secildi: {outcome.CrossoverStyle} ({title})");
                }
            );
        }

        private static void TriggerBranchingNarrativeEvent(Company company, IntPtr gPtr, string title)
        {
            NativePopupQueue.Enqueue(
                $"Dallanan Hikaye Krizi: {title}",
                $"Yazarlar kendini kaptırdı: {title} için 12 farklı son yazıldı ama senaryo bütçesi zorlanıyor! Ne emrediyorsun?\n\n" +
                $"• Tüm Sonları Cilala (-15K): +%5 Skor, +4 Tutarlılık.\n" +
                $"• Gizli Sürpriz Son Ekle (Ücretsiz): +%2 Skor, +12 Hype.\n" +
                $"• Tek Sona Kırp (+5K Tasarruf): -%3 Skor, -4 Tutarlılık.",
                new[] { "Cilala (-15K)", "Gizli Son Ekle", "Tek Sona Kırp (+5K)" },
                opt =>
                {
                    var o = GetOrCreateOutcome(gPtr);
                    if (opt == 0)
                    {
                        company.adjustCash(-15000.0, "Dallanan Hikaye Cilası");
                        o.ScoreDelta += 0.05; o.IntegrityDelta += 4;
                        o.Notes.Add("Olay: Kusursuz Dallanan Final (+%5 Skor, +4 Tutarlılık)");
                    }
                    else if (opt == 1)
                    {
                        o.ScoreDelta += 0.02; o.HypeDelta += 12;
                        o.Notes.Add("Olay: Gizli Final Teorileri (+%2 Skor, +12 Hype)");
                    }
                    else
                    {
                        company.adjustCash(5000.0, "Senaryo Tasarrufu");
                        o.ScoreDelta -= 0.03; o.IntegrityDelta -= 4;
                        o.Notes.Add("Olay: Kırpılmış Hikaye (-%3 Skor, -4 Tutarlılık)");
                    }
                }
            );
        }

        private static void TriggerSoundtrackRapEvent(Company company, IntPtr gPtr, string title)
        {
            NativePopupQueue.Enqueue(
                $"Soundtrack & Rap Stüdyosu: {title}",
                $"{title} için kaydedilen özgün rap tanıtım parçası stüdyo içinde bile bağımlılık yaptı! Nasıl değerlendirelim?\n\n" +
                $"• Klip Çek & Yayınla (-10K): +18 Hype ve anında hayran patlaması.\n" +
                $"• Dinamik Oyun İçi Mix (Ücretsiz): +%4 Skor, +6 Hype.\n" +
                $"• Yayıncı Dostu Telifsiz Mod: +8 Hype, Lore Bariyeri -10 & yeni hayranlar.",
                new[] { "Rap Klibi Çek (-10K)", "Dinamik Mix", "Telifsiz Yayıncı Modu" },
                opt =>
                {
                    var o = GetOrCreateOutcome(gPtr);
                    if (opt == 0)
                    {
                        company.adjustCash(-10000.0, "Rap Tanıtım Klibi");
                        o.HypeDelta += 18;
                        double bonusFans = Math.Max(45.0, Math.Round(company.fans * 0.05));
                        company.adjustFans(bonusFans);
                        o.Notes.Add($"Olay: Viral Rap Klibi (+18 Hype, +{bonusFans:F0} Hayran)");
                    }
                    else if (opt == 1)
                    {
                        o.ScoreDelta += 0.04; o.HypeDelta += 6;
                        o.Notes.Add("Olay: Dinamik Müzik Miksajı (+%4 Skor, +6 Hype)");
                    }
                    else
                    {
                        o.HypeDelta += 8; o.BarrierDelta -= 10;
                        double bonusFans = Math.Max(60.0, Math.Round(company.fans * 0.06));
                        company.adjustFans(bonusFans);
                        o.Notes.Add($"Olay: Telifsiz Yayıncı Dalgası (+8 Hype, Bariyer -10, +{bonusFans:F0} Hayran)");
                    }
                }
            );
        }

        private static void TriggerModSupportEvent(Company company, IntPtr gPtr, string title)
        {
            NativePopupQueue.Enqueue(
                $"Mod Topluluğu Hareketlendi: {title}",
                $"Erken mod araçlarını kurcalayan oyuncular şimdiden çılgın içerikler üretiyor ama bazıları evrenin kurallarını büküyor!\n\n" +
                $"• Mod Yarışması Düzenle (-12K): +%3 Skor, +12 Hype & büyük hayran artışı.\n" +
                $"• En İyi Modu Kanona Al: +5 Tutarlılık, +6 Hype, Lore Bariyeri -10.\n" +
                $"• Sıkı Lore Lisansı Koy: +3 Tutarlılık ama topluluk kırılır (-6 Hype).",
                new[] { "Mod Yarışması (-12K)", "Modu Kanona Al", "Sıkı Lisans Koy" },
                opt =>
                {
                    var o = GetOrCreateOutcome(gPtr);
                    if (opt == 0)
                    {
                        company.adjustCash(-12000.0, "Resmi Mod Yarışması");
                        o.ScoreDelta += 0.03; o.HypeDelta += 12;
                        double bonusFans = Math.Max(75.0, Math.Round(company.fans * 0.07));
                        company.adjustFans(bonusFans);
                        o.Notes.Add($"Olay: Resmi Mod Yarışması (+%3 Skor, +12 Hype, +{bonusFans:F0} Hayran)");
                    }
                    else if (opt == 1)
                    {
                        o.IntegrityDelta += 5; o.BarrierDelta -= 10; o.HypeDelta += 6;
                        o.Notes.Add("Olay: Topluluk Modu Kanonlaştı (+5 Tutarlılık, Bariyer -10)");
                    }
                    else
                    {
                        o.IntegrityDelta += 3; o.HypeDelta -= 6;
                        o.Notes.Add("Olay: Sıkı Mod Kuralları (+3 Tutarlılık, -6 Hype)");
                    }
                }
            );
        }
    }

    // =========================================================================
    // 9. FAZ 4: TRANSMEDYA STUDYO SISTEMI (CIZGI ROMAN, ROMAN, KONSEPT ALBUM)
    // =========================================================================
    public static class TransmediaSystem
    {
        public static bool CanStartProject(Company company, TransmediaDef def, out string reason)
        {
            reason = "";
            if (company == null || def == null) { reason = "Şirket hazır değil."; return false; }
            if (UniverseSaveManager.GetActiveTransmediaCount() >= 2)
            {
                reason = "Aynı anda en fazla 2 transmedya projesi yürütülebilir.";
                return false;
            }
            if (company.cash < def.Cost)
            {
                reason = $"Yetersiz nakit! (Gereken: {def.Cost / 1000.0:F0}K)";
                return false;
            }
            return true;
        }

        public static bool StartProject(Company company, TransmediaDef def, CharacterRecord focusChar, out string feedback)
        {
            if (!CanStartProject(company, def, out feedback)) return false;

            string focusName = focusChar != null ? focusChar.Name : "Genel Evren";
            string focusId = focusChar != null ? focusChar.Id : "";
            int projectNum = UniverseSaveManager.Current.TransmediaProjects.Count + 1;
            string title = focusChar != null
                ? $"{focusChar.Name}: {def.DefaultTitlePrefix} #{projectNum}"
                : $"{UniverseSaveManager.Current.UniverseName} — {def.DefaultTitlePrefix} #{projectNum}";

            company.adjustCash(-def.Cost, $"Transmedya: {def.Name}");
            SoloBurnoutSystem.OnWorkCompleted(company, 10);

            var rec = new TransmediaProjectRecord
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                TypeName = def.Type.ToString(),
                Title = title,
                FocusCharacterId = focusId,
                FocusCharacterName = focusName,
                Cost = def.Cost,
                StartWeek = company.currentWeek,
                FinishWeek = company.currentWeek + def.DurationWeeks,
                IsCompleted = false
            };

            UniverseSaveManager.Current.TransmediaProjects.Add(rec);
            UniverseSaveManager.Save();

            MelonLogger.Msg($"[Faz 4 Transmedya] Baslatildi: '{title}' (Bitis Haftasi: {rec.FinishWeek:F1}, Maliyet: {def.Cost:F0})");
            feedback = $"'{title}' üretimi başladı! ({def.DurationWeeks} hafta içinde tamamlanacak)";
            return true;
        }

        public static void Tick(Company company)
        {
            if (company == null) return;
            var list = UniverseSaveManager.Current.TransmediaProjects;
            if (list == null || list.Count == 0) return;

            bool anyFinished = false;
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (p == null || p.IsCompleted) continue;
                if (company.currentWeek < p.FinishWeek) continue;

                CompleteProject(company, p);
                anyFinished = true;
            }

            if (anyFinished)
            {
                UniverseSaveManager.Save();
                CharacterWorkshopUGUI.UpdateHudState();
            }
        }

        private static void CompleteProject(Company company, TransmediaProjectRecord p)
        {
            p.IsCompleted = true;

            TransmediaType tType = TransmediaType.ComicSeries;
            Enum.TryParse<TransmediaType>(p.TypeName, out tType);

            var focusChar = UniverseSaveManager.FindCharacterById(p.FocusCharacterId);
            int charPop = focusChar != null ? focusChar.Popularity : 60;
            int integrity = UniverseSaveManager.Current.CanonIntegrity;
            int lawPopBonus = UniverseSaveManager.GetLaw() == CosmologyLaw.CharacterLegendEra ? 5 : 0;

            double rnd = 0.5;
            try { rnd = company.getRandom(); } catch { }

            double qualityFactor = Math.Clamp((integrity * 0.45 + charPop * 0.55) / 100.0 + (rnd - 0.45) * 0.30, 0.50, 1.50);
            bool isHit = qualityFactor >= 1.02;

            double revenue = Math.Round(p.Cost * (0.65 + qualityFactor * 0.85));
            int fansGain = 0;
            string summary = "";

            if (tType == TransmediaType.ComicSeries)
            {
                UniverseSaveManager.AdjustLoreBarrier(-20);
                int hypeAdd = isHit ? 12 : 8;
                UniverseSaveManager.Current.PendingTransmediaHype = Math.Min(25, UniverseSaveManager.Current.PendingTransmediaHype + hypeAdd);
                fansGain = (int)Math.Max(60, Math.Round(company.fans * (isHit ? 0.08 : 0.05)));
                if (focusChar != null) focusChar.Popularity = Math.Clamp(focusChar.Popularity + 10 + lawPopBonus, 10, 100);
                summary = $"Bariyer -20, Sıradakine +{hypeAdd} Hype, +{fansGain} Hayran, +{revenue / 1000.0:F1}K Gelir";
            }
            else if (tType == TransmediaType.Novel)
            {
                UniverseSaveManager.AdjustIntegrity(+10);
                double scoreAdd = isHit ? 0.03 : 0.02;
                UniverseSaveManager.Current.PendingTransmediaScoreBonus = Math.Min(0.05, UniverseSaveManager.Current.PendingTransmediaScoreBonus + scoreAdd);
                UniverseSaveManager.Current.PendingTransmediaHype = Math.Min(25, UniverseSaveManager.Current.PendingTransmediaHype + 6);
                fansGain = (int)Math.Max(50, Math.Round(company.fans * (isHit ? 0.07 : 0.04)));
                if (focusChar != null) focusChar.Popularity = Math.Clamp(focusChar.Popularity + 12 + lawPopBonus, 10, 100);
                summary = $"Tutarlılık +10, Sıradakine +%{scoreAdd * 100:F0} Skor & +6 Hype, +{fansGain} Hayran, +{revenue / 1000.0:F1}K Gelir";
            }
            else
            {
                int hypeAdd = isHit ? 18 : 14;
                UniverseSaveManager.Current.PendingTransmediaHype = Math.Min(25, UniverseSaveManager.Current.PendingTransmediaHype + hypeAdd);
                fansGain = (int)Math.Max(100, Math.Round(company.fans * (isHit ? 0.10 : 0.06)));
                if (focusChar != null) focusChar.Popularity = Math.Clamp(focusChar.Popularity + 8 + lawPopBonus, 10, 100);
                summary = $"Sıradakine +{hypeAdd} Hype, +{fansGain} Hayran, +{revenue / 1000.0:F1}K Gelir";
            }

            p.RevenueEarned = revenue;
            p.FansGained = fansGain;
            p.ResultSummary = (isHit ? "[ÇOK SATAN] " : "[YAYINLANDI] ") + summary;

            company.adjustCash(revenue, $"Transmedya Geliri: {p.Title}");
            company.adjustFans(fansGain);

            NotificationHelper.Show(
                company,
                $"Transmedya Yayında: {p.Title}",
                $"{(isHit ? "Büyük ses getirdi! " : "Hayranlarla buluştu! ")}Odak: {p.FocusCharacterName} | {summary}"
            );
        }
    }

    // =========================================================================
    // 10. GERCEK UNITY UI (uGUI): FAZ 6.0 "MOD ICERIGI" + 4 EKSEN + OZEL/NPC + EKIP
    // =========================================================================
    public static class CharacterWorkshopUGUI
    {
        public static bool IsOpen { get; private set; } = false;

        private static readonly Color ColDim       = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color ColPanel     = new Color(0.98f, 0.96f, 0.90f, 1f);
        private static readonly Color ColHeader    = new Color(0.47f, 0.29f, 0.52f, 1f);
        private static readonly Color ColCard      = new Color(0.92f, 0.89f, 0.80f, 1f);
        private static readonly Color ColHighlight = new Color(0.99f, 0.93f, 0.70f, 1f);
        private static readonly Color ColYellow    = new Color(0.96f, 0.80f, 0.22f, 1f);
        private static readonly Color ColGreen     = new Color(0.30f, 0.68f, 0.36f, 1f);
        private static readonly Color ColRed       = new Color(0.82f, 0.30f, 0.28f, 1f);
        private static readonly Color ColPurple    = new Color(0.55f, 0.26f, 0.66f, 1f);
        private static readonly Color ColBlue      = new Color(0.24f, 0.48f, 0.74f, 1f);
        private static readonly Color ColTeal      = new Color(0.18f, 0.56f, 0.54f, 1f);
        private static readonly Color ColDarkText  = new Color(0.16f, 0.13f, 0.18f, 1f);
        private static readonly Color ColMutedText = new Color(0.38f, 0.34f, 0.36f, 1f);
        private static readonly Color ColTrack     = new Color(0.76f, 0.72f, 0.65f, 1f);

        private static Font _font;

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
        private static List<PendingBondDraft> _workingBonds = new List<PendingBondDraft>();

        // Ozel Karakter Editoru (yeni yazim, NPC terfisi veya eski karakter evrimi)
        private static string _promotingNpcId = null;
        private static string _evolvingCharId = null;

        // Iliski Agi secici durumu
        private static int _bondPickA = 0;
        private static int _bondPickB = 1;
        private static CharacterBondType _bondDraftType = CharacterBondType.ArchRivalry;
        private static string _draftName = "Afor";
        private static CharacterRole _draftRole = CharacterRole.Protagonist;
        private static CharacterTrait _draftTrait = CharacterTrait.None;
        private static string _draftTeamName = "";
        private static int _draftTeamCycleIdx = -1;
        private static float[] _draftAxes = new float[] { 0.72f, 0.65f, 0.72f, 0.18f };
        private static int _namePoolIdx = 0;

        // Hizli NPC formu
        private static string _npcDraftName = "Kütüphane Bekçisi";
        private static int _npcTypeIdx = 0;
        private static string _npcLinkedCharId = "";
        private static string _npcLinkedCharName = "";
        private static int _npcNamePoolIdx = 0;

        // Eski karakter cagirma & Karakterler alt sekmesi
        private static CharacterRole _returningRole = CharacterRole.Protagonist;
        private static bool _returningShowNpcs = false;
        private static int _rosterSubTab = 0;

        // Mod Icerigi sekme durumlari
        private static int _transmediaFocusCharIdx = -1;
        private static string _statusBannerMsg = "";

        // Canli uGUI referanslari (4 eksen)
        private static UnityEngine.UI.Text[] _activeAxisValueTexts = new UnityEngine.UI.Text[4];
        private static UnityEngine.UI.Text _activeArchetypeMatchText;
        private static UnityEngine.UI.InputField _activeNameInput;
        private static UnityEngine.UI.Text _fallbackNameText;
        private static UnityEngine.UI.InputField _activeTeamInput;
        private static UnityEngine.UI.Text _fallbackTeamText;

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

                var hudBtn = CreateButton(_rootObj.transform, "Mod İçeriği", ColYellow, ColDarkText, 18, () =>
                {
                    if (!IsOpen) OpenModContentHub();
                });
                _hudBtnObj = hudBtn.gameObject;
                _hudBtnText = hudBtn.GetComponentInChildren<UnityEngine.UI.Text>();
                var hudRt = hudBtn.GetComponent<RectTransform>();
                hudRt.anchorMin = new Vector2(1f, 0f);
                hudRt.anchorMax = new Vector2(1f, 0f);
                hudRt.pivot = new Vector2(1f, 0f);
                hudRt.anchoredPosition = new Vector2(-18f * _uiScale, 18f * _uiScale);
                hudRt.sizeDelta = new Vector2(230f, 48f);
                try { hudRt.localScale = new Vector3(_uiScale, _uiScale, 1f); } catch { }

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

                _headerTitle = CreateText(header.transform, "Mod İçeriği", 24, Color.white, TextAnchor.MiddleLeft, true);
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
                MelonLogger.Msg($"[Faz 6.1] 'Mod Icerigi' uGUI HUD kuruldu! (Canvas: '{hostCanvas.name}', Boyut: {cw:F0}x{ch:F0}, Olcek: {_uiScale:F2}, Font: '{fName}')");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] uGUI kurulum hatasi: {ex}");
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
            {
                int activeTm = UniverseSaveManager.GetActiveTransmediaCount();
                string suffix = activeTm > 0 ? $" ({activeTm} TM)" : "";
                _hudBtnText.text = $"Mod İçeriği{suffix}";
            }

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
                _promotingNpcId = null;
                _evolvingCharId = null;
                _targetGamePtr = gamePtr;
                _targetGameTitle = string.IsNullOrEmpty(gameTitle) ? "Yeni Oyun" : gameTitle;
                _workingCast = new List<ActiveCharacterDraft>();
                _workingBonds = new List<PendingBondDraft>();

                IsOpen = true;
                _modalRoot.SetActive(true);
                BringToFront();
                UpdateHudState();
                PauseGameSimulation();

                ShowCastOverview();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] OpenCastManager hatasi: {ex}");
                CloseModal();
            }
        }

        public static void OpenModContentHub()
        {
            EnsureHUD();
            if (_modalRoot == null) return;

            try
            {
                _isRosterOnlyMode = true;
                _promotingNpcId = null;
                _evolvingCharId = null;
                _statusBannerMsg = "";
                IsOpen = true;
                _modalRoot.SetActive(true);
                BringToFront();
                UpdateHudState();
                PauseGameSimulation();

                ShowFullRosterView();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] OpenModContentHub hatasi: {ex}");
                CloseModal();
            }
        }

        public static void CloseModal()
        {
            IsOpen = false;
            _promotingNpcId = null;
            _evolvingCharId = null;
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

        private static void DrawModContentTabs(int activeTab)
        {
            var u = UniverseSaveManager.Current;
            string[] labels =
            {
                $"Karakterler ({u.Characters.Count})",
                $"Transmedya ({u.TransmediaProjects.Count})",
                "Kozmoloji",
                $"Sıralama ({u.Rivals.Count + 1})",
                $"Rakip Haberleri ({u.RivalNews.Count})"
            };

            float tabW = 210f;
            float gap = 8.5f;
            for (int t = 0; t < 5; t++)
            {
                int capT = t;
                bool isSel = (activeTab == t);
                Color bg = isSel ? ColGreen : (t == 1 ? ColPurple : (t == 3 || t == 4 ? ColBlue : ColYellow));
                Color fg = isSel || t == 1 || t == 3 || t == 4 ? Color.white : ColDarkText;

                var btn = CreateButton(_contentRoot, labels[t], bg, fg, 15, () =>
                {
                    _statusBannerMsg = "";
                    if (capT == 0) ShowFullRosterView();
                    else if (capT == 1) ShowTransmediaStudioView();
                    else if (capT == 2) ShowCosmologyView();
                    else if (capT == 3) ShowLeaderboardView();
                    else ShowRivalNewsView();
                });
                PlaceRect(btn.GetComponent<RectTransform>(), t * (tabW + gap), 0f, tabW, 44f);
            }
        }

        private static void DrawHubCloseButton()
        {
            string closeLabel = _isRosterOnlyMode ? "Kapat" : "< Oyun Kadrosuna Dön";
            var btnClose = CreateButton(_contentRoot, closeLabel, ColYellow, ColDarkText, 19, () =>
            {
                if (_isRosterOnlyMode) CloseModal();
                else ShowCastOverview();
            });
            PlaceRect(btnClose.GetComponent<RectTransform>(), 0f, 508f, 300f, 52f);
        }

        private static string GetTeamSynergyPreview(List<ActiveCharacterDraft> cast)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cast.Count; i++)
            {
                if (cast[i].IsNpc || string.IsNullOrWhiteSpace(cast[i].TeamName)) continue;
                string t = cast[i].TeamName.Trim();
                int cur = 0;
                counts.TryGetValue(t, out cur);
                counts[t] = cur + 1;
            }

            string fullSquadTeam = null;
            var teamNames = new List<string>();
            foreach (var kv in counts)
            {
                teamNames.Add(kv.Key);
                if (kv.Value >= 3 && fullSquadTeam == null) fullSquadTeam = kv.Key;
            }

            var badges = new List<string>();
            if (fullSquadTeam != null) badges.Add($"★ TAM KADRO ({fullSquadTeam})");
            if (teamNames.Count >= 2) badges.Add($"⚔ İÇ SAVAŞ ({teamNames[0]} vs {teamNames[1]})");
            return badges.Count > 0 ? string.Join("  |  ", badges) : "";
        }

        // ---------------------------------------------------------------------
        // GORUNUM 1: OYUNUN KARAKTER KADROSU (+ KADER BUTONU + ILISKI AGI)
        // ---------------------------------------------------------------------
        private static void ShowCastOverview()
        {
            try
            {
                ClearContent();
                int specialCount = 0, npcCount = 0;
                for (int i = 0; i < _workingCast.Count; i++)
                {
                    if (_workingCast[i].IsNpc) npcCount++;
                    else specialCount++;
                }
                _headerTitle.text = $"Oyun Kadrosu — {_targetGameTitle} ({specialCount} Özel, {npcCount} NPC)";

                var u = UniverseSaveManager.Current;
                string pairSummary;
                bool isCrossover = InteractiveEventSystem.DetectCrossoverCast(_workingCast, out pairSummary);
                string teamPreview = GetTeamSynergyPreview(_workingCast);
                int bondCountInCast = CountActiveBondsInWorkingCast();

                Color bannerBg = isCrossover ? ColPurple : (!string.IsNullOrEmpty(teamPreview) ? ColTeal : ColCard);
                Color bannerFg = (isCrossover || !string.IsNullOrEmpty(teamPreview)) ? Color.white : ColDarkText;
                string infoLine;
                if (isCrossover)
                    infoLine = $"★ CROSSOVER! ({pairSummary})" + (string.IsNullOrEmpty(teamPreview) ? "" : $"  |  {teamPreview}");
                else if (!string.IsNullOrEmpty(teamPreview))
                    infoLine = $"{teamPreview}  |  Aktif Bağ: {bondCountInCast}  |  Tutarlılık: {u.CanonIntegrity}/100";
                else
                    infoLine = $"Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())}  |  Yasa: {CategoryLabels.LawToTurkish(UniverseSaveManager.GetLaw())}  |  Tutarlılık: {u.CanonIntegrity}/100  |  Aktif Bağ: {bondCountInCast}";

                var infoCard = CreatePanel(_contentRoot, bannerBg);
                PlaceRect(infoCard.GetComponent<RectTransform>(), 0f, 0f, 1084f, 52f);
                var infoTxt = CreateText(infoCard.transform, infoLine, 16, bannerFg, TextAnchor.MiddleLeft, true);
                StretchWithPad(infoTxt.GetComponent<RectTransform>(), 16f, 0f);

                // 5 Ust Aksiyon Butonu (Özel, NPC, Eski Çağır, Bağ Kur, Mod İçeriği)
                var btnNew = CreateButton(_contentRoot, "+ Özel Yaz", ColGreen, Color.white, 16, () =>
                {
                    _promotingNpcId = null;
                    _evolvingCharId = null;
                    _draftName = Phase1Database.CharacterNamePool[_namePoolIdx % Phase1Database.CharacterNamePool.Length];
                    _namePoolIdx++;
                    _draftRole = specialCount == 0 ? CharacterRole.Protagonist : CharacterRole.Villain;
                    _draftTrait = CharacterTrait.None;
                    _draftTeamName = "";
                    _draftTeamCycleIdx = -1;
                    _draftAxes = (float[])Phase1Database.Archetypes[0].PresetAxes.Clone();
                    ShowCreateCharacterView();
                });
                PlaceRect(btnNew.GetComponent<RectTransform>(), 0f, 62f, 200f, 48f);

                var btnNpc = CreateButton(_contentRoot, "+ NPC (Hızlı)", ColTeal, Color.white, 16, () =>
                {
                    _npcDraftName = Phase1Database.NpcNamePool[_npcNamePoolIdx % Phase1Database.NpcNamePool.Length];
                    _npcNamePoolIdx++;
                    _npcTypeIdx = 0;
                    _npcLinkedCharId = "";
                    _npcLinkedCharName = "";
                    ShowCreateNpcView();
                });
                PlaceRect(btnNpc.GetComponent<RectTransform>(), 210f, 62f, 190f, 48f);

                int savedCount = u.Characters.Count;
                var btnReturn = CreateButton(_contentRoot, $"+ Eski ({savedCount})", savedCount > 0 ? ColYellow : ColTrack, ColDarkText, 16, () =>
                {
                    if (savedCount > 0)
                    {
                        _returningShowNpcs = false;
                        ShowReturningPickerView();
                    }
                });
                PlaceRect(btnReturn.GetComponent<RectTransform>(), 410f, 62f, 205f, 48f);

                bool canBond = specialCount >= 2;
                var btnBond = CreateButton(_contentRoot, $"Bağ Kur ({bondCountInCast})", canBond ? ColPurple : ColTrack, Color.white, 16, () =>
                {
                    if (canBond)
                    {
                        _bondPickA = 0;
                        _bondPickB = 1;
                        ShowBondEditorView();
                    }
                });
                PlaceRect(btnBond.GetComponent<RectTransform>(), 625f, 62f, 215f, 48f);

                var btnHub = CreateButton(_contentRoot, "Mod İçeriği", ColBlue, Color.white, 16, () => ShowFullRosterView());
                PlaceRect(btnHub.GetComponent<RectTransform>(), 850f, 62f, 234f, 48f);

                // Kadro listesi
                var scroll = CreateScrollView(_contentRoot, 0f, 120f, 1084f, 374f);
                var listContent = scroll.content;
                float innerW = 1068f;
                float yCursor = 0f;

                if (_workingCast.Count == 0)
                {
                    var emptyCard = CreatePanel(listContent, ColCard);
                    PlaceRect(emptyCard.GetComponent<RectTransform>(), 0f, 0f, innerW, 146f);
                    var txt = CreateText(emptyCard.transform,
                        "Bu oyun için henüz karakter eklemedin.\n\n• 2+ Özel Karakter ekleyip 'Bağ Kur' ile Ezeli Rekabet, Silah Arkadaşı veya Usta–Çırak (Miras Devri) tanımlayabilirsin.\n• En az 2 oyuna çıkmış eski yıldızlara bu ekrandan '☠ Final Fedası' veya '★ Emeklilik' kaderi yazabilirsin!\n• 2+ oyun dinlenen yıldızlar '🔥 Özlem Bonusu' ile geri döner.",
                        16, ColMutedText, TextAnchor.MiddleCenter, false);
                    StretchWithPad(txt.GetComponent<RectTransform>(), 20f, 10f);
                    yCursor = 154f;
                }
                else
                {
                    for (int i = 0; i < _workingCast.Count; i++)
                    {
                        int capturedIdx = i;
                        var d = _workingCast[i];
                        var card = CreatePanel(listContent, d.IsNpc ? new Color(0.88f, 0.92f, 0.92f, 1f) : ColCard);
                        PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 82f);

                        string line1, line2;
                        bool showFateBtn = false;

                        if (d.IsNpc)
                        {
                            string linkTxt = string.IsNullOrEmpty(d.LinkedSpecialCharName) ? "Bağımsız NPC" : $"Bağlı: {d.LinkedSpecialCharName}";
                            string warn = (!string.IsNullOrEmpty(d.LinkedSpecialCharName) && !IsSpecialCharInWorkingCast(d.LinkedSpecialCharId, d.LinkedSpecialCharName))
                                ? "  [⚠ Bağlı karakter kadroda yok!]"
                                : "";
                            line1 = $"[NPC — {d.NpcType}]  {d.Name}  ({(d.IsReturning ? "Eski NPC" : "Yeni NPC")})";
                            line2 = $"{linkTxt}{warn}";
                        }
                        else
                        {
                            var matchedArch = CharacterAxisSystem.DetectClosestArchetype(d.Axes);
                            string teamTxt = string.IsNullOrWhiteSpace(d.TeamName) ? "Bağımsız" : $"Ekip: {d.TeamName}";
                            string traitTxt = d.Trait == CharacterTrait.None ? "" : $" | {CategoryLabels.TraitToTurkish(d.Trait)}";
                            string tag = "Yeni";
                            if (d.IsReturning)
                            {
                                var rec = UniverseSaveManager.FindCharacterById(d.ExistingCharacterId);
                                if (rec != null)
                                {
                                    tag = $"Eski ({rec.GamesAppeared} oyun)";
                                    if (rec.GamesAppeared >= 2 && CategoryLabels.ParseFate(rec.FateName) == CharacterFate.Alive)
                                        showFateBtn = true;
                                }
                                else tag = "Eski Yıldız";
                            }
                            string evoTag = d.AxesEvolved ? " [Evrim]" : "";
                            string fateTag = d.PlannedFate != CharacterFate.Alive ? $"  [{CategoryLabels.FateToTurkish(d.PlannedFate)}!]" : "";
                            line1 = $"[ÖZEL — {CategoryLabels.RoleToTurkish(d.Role)}]  {d.Name} ({matchedArch.Name}, {tag}{evoTag}) | {teamTxt}{traitTxt}{fateTag}";
                            line2 = CharacterAxisSystem.FormatCompactSummary(d.Axes);
                        }

                        var lbl = CreateText(card.transform, $"{line1}\n{line2}", 14, ColDarkText, TextAnchor.MiddleLeft, false);
                        var lrt = lbl.GetComponent<RectTransform>();
                        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                        lrt.offsetMin = new Vector2(14f, 4f); lrt.offsetMax = new Vector2(showFateBtn ? -295f : -125f, -4f);

                        if (showFateBtn)
                        {
                            Color fCol = d.PlannedFate == CharacterFate.Sacrificed ? ColRed : (d.PlannedFate == CharacterFate.Retired ? ColPurple : ColYellow);
                            Color fTxt = d.PlannedFate == CharacterFate.Alive ? ColDarkText : Color.white;
                            string fLabel = d.PlannedFate == CharacterFate.Alive ? "Kader: Normal" : CategoryLabels.FateToTurkish(d.PlannedFate);

                            var fateBtn = CreateButton(card.transform, fLabel, fCol, fTxt, 14, () =>
                            {
                                if (d.PlannedFate == CharacterFate.Alive) d.PlannedFate = CharacterFate.Sacrificed;
                                else if (d.PlannedFate == CharacterFate.Sacrificed) d.PlannedFate = CharacterFate.Retired;
                                else d.PlannedFate = CharacterFate.Alive;
                                ShowCastOverview();
                            });
                            var frt = fateBtn.GetComponent<RectTransform>();
                            frt.anchorMin = new Vector2(1f, 0.5f); frt.anchorMax = new Vector2(1f, 0.5f);
                            frt.pivot = new Vector2(1f, 0.5f);
                            frt.anchoredPosition = new Vector2(-126f, 0f);
                            frt.sizeDelta = new Vector2(160f, 46f);
                        }

                        var delBtn = CreateButton(card.transform, "Çıkar", ColRed, Color.white, 16, () =>
                        {
                            _workingCast.RemoveAt(capturedIdx);
                            ShowCastOverview();
                        });
                        var drt = delBtn.GetComponent<RectTransform>();
                        drt.anchorMin = new Vector2(1f, 0.5f); drt.anchorMax = new Vector2(1f, 0.5f);
                        drt.pivot = new Vector2(1f, 0.5f);
                        drt.anchoredPosition = new Vector2(-12f, 0f);
                        drt.sizeDelta = new Vector2(104f, 46f);

                        yCursor += 90f;
                    }
                }
                listContent.sizeDelta = new Vector2(0f, Mathf.Max(358f, yCursor));

                string confirmLabel = _workingCast.Count > 0
                    ? (isCrossover
                        ? $"★ CROSSOVER Kadrosunu Onayla ({specialCount} Özel, {npcCount} NPC) ve Oyuna Başla"
                        : $"Kadroyu Onayla ({specialCount} Özel, {npcCount} NPC) ve Oyuna Başla")
                    : "Karaktersiz Devam Et";
                var btnConfirm = CreateButton(_contentRoot, confirmLabel, isCrossover ? ColPurple : ColGreen, Color.white, 20, () =>
                {
                    var finalCast = new List<ActiveCharacterDraft>(_workingCast);
                    var finalBonds = new List<PendingBondDraft>(_workingBonds);
                    UniverseChoicePopupSystem.SetCastForGame(_targetGamePtr, finalCast);
                    UniverseChoicePopupSystem.SetPendingBondsForGame(_targetGamePtr, finalBonds);
                    InteractiveEventSystem.RegisterCastForEvents(_targetGamePtr, _targetGameTitle, finalCast);
                    MelonLogger.Msg($"[Faz 6.1] '{_targetGameTitle}' kadrosu onaylandi ({specialCount} ozel, {npcCount} NPC, {finalBonds.Count} yeni bag, Crossover={isCrossover}).");
                    CloseModal();
                });
                PlaceRect(btnConfirm.GetComponent<RectTransform>(), 0f, 506f, 1084f, 56f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowCastOverview hatasi: {ex}");
                CloseModal();
            }
        }

        private static bool IsSpecialCharInWorkingCast(string charId, string charName)
        {
            for (int i = 0; i < _workingCast.Count; i++)
            {
                var d = _workingCast[i];
                if (d.IsNpc) continue;
                if (!string.IsNullOrEmpty(charId) && d.ExistingCharacterId == charId) return true;
                if (!string.IsNullOrEmpty(charName) && string.Equals(d.Name, charName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static List<ActiveCharacterDraft> GetSpecialDraftsInWorkingCast()
        {
            var res = new List<ActiveCharacterDraft>();
            for (int i = 0; i < _workingCast.Count; i++)
                if (!_workingCast[i].IsNpc) res.Add(_workingCast[i]);
            return res;
        }

        private static int CountActiveBondsInWorkingCast()
        {
            var specials = GetSpecialDraftsInWorkingCast();
            if (specials.Count < 2) return 0;
            int total = _workingBonds.Count;
            for (int i = 0; i < specials.Count; i++)
            {
                for (int j = i + 1; j < specials.Count; j++)
                {
                    var a = specials[i];
                    var b = specials[j];
                    if (a.IsReturning && b.IsReturning && UniverseSaveManager.FindBond(a.ExistingCharacterId, b.ExistingCharacterId) != null)
                        total++;
                }
            }
            return total;
        }

        // ---------------------------------------------------------------------
        // GORUNUM 1B (FAZ 6.1): ILISKI AGI / BAG KURMA EKRANI
        // ---------------------------------------------------------------------
        private static void ShowBondEditorView()
        {
            try
            {
                ClearContent();
                _headerTitle.text = $"İlişki Ağı & Bağ Kur — {_targetGameTitle}";

                var specials = GetSpecialDraftsInWorkingCast();
                if (specials.Count < 2)
                {
                    ShowCastOverview();
                    return;
                }
                if (_bondPickA >= specials.Count) _bondPickA = 0;
                if (_bondPickB >= specials.Count || _bondPickB == _bondPickA) _bondPickB = (_bondPickA + 1) % specials.Count;

                var pickA = specials[_bondPickA];
                var pickB = specials[_bondPickB];

                // 1. Karakter A & B Secici
                var cardA = CreatePanel(_contentRoot, ColCard);
                PlaceRect(cardA.GetComponent<RectTransform>(), 0f, 4f, 520f, 58f);
                var txtA = CreateText(cardA.transform, $"1. Karakter: {pickA.Name} ({CategoryLabels.RoleToTurkish(pickA.Role)})", 16, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(txtA.GetComponent<RectTransform>(), 14f, 0f);
                var btnNextA = CreateButton(cardA.transform, "Değiştir >", ColYellow, ColDarkText, 15, () =>
                {
                    _bondPickA = (_bondPickA + 1) % specials.Count;
                    if (_bondPickA == _bondPickB) _bondPickB = (_bondPickA + 1) % specials.Count;
                    ShowBondEditorView();
                });
                var art = btnNextA.GetComponent<RectTransform>();
                art.anchorMin = new Vector2(1f, 0.5f); art.anchorMax = new Vector2(1f, 0.5f);
                art.pivot = new Vector2(1f, 0.5f); art.anchoredPosition = new Vector2(-10f, 0f); art.sizeDelta = new Vector2(126f, 40f);

                var cardB = CreatePanel(_contentRoot, ColCard);
                PlaceRect(cardB.GetComponent<RectTransform>(), 564f, 4f, 520f, 58f);
                var txtB = CreateText(cardB.transform, $"2. Karakter: {pickB.Name} ({CategoryLabels.RoleToTurkish(pickB.Role)})", 16, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(txtB.GetComponent<RectTransform>(), 14f, 0f);
                var btnNextB = CreateButton(cardB.transform, "Değiştir >", ColYellow, ColDarkText, 15, () =>
                {
                    _bondPickB = (_bondPickB + 1) % specials.Count;
                    if (_bondPickB == _bondPickA) _bondPickB = (_bondPickB + 1) % specials.Count;
                    ShowBondEditorView();
                });
                var brt = btnNextB.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(1f, 0.5f); brt.anchorMax = new Vector2(1f, 0.5f);
                brt.pivot = new Vector2(1f, 0.5f); brt.anchoredPosition = new Vector2(-10f, 0f); brt.sizeDelta = new Vector2(126f, 40f);

                // 2. Bag Turu Secimi
                CharacterBondType[] bTypes = { CharacterBondType.ArchRivalry, CharacterBondType.Comrades, CharacterBondType.MentorStudent };
                for (int i = 0; i < 3; i++)
                {
                    CharacterBondType capB = bTypes[i];
                    bool sel = (_bondDraftType == capB);
                    var bb = CreateButton(_contentRoot, CategoryLabels.BondToTurkish(capB), sel ? ColPurple : ColYellow, sel ? Color.white : ColDarkText, 17, () =>
                    {
                        _bondDraftType = capB;
                        ShowBondEditorView();
                    });
                    PlaceRect(bb.GetComponent<RectTransform>(), i * 366f, 74f, 352f, 46f);
                }

                var descCard = CreatePanel(_contentRoot, ColHighlight);
                PlaceRect(descCard.GetComponent<RectTransform>(), 0f, 130f, 1084f, 48f);
                var descTxt = CreateText(descCard.transform, CategoryLabels.BondShortDesc(_bondDraftType), 15, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(descTxt.GetComponent<RectTransform>(), 14f, 0f);

                var btnAddBond = CreateButton(_contentRoot, $"★ {pickA.Name} ↔ {pickB.Name} Arasında '{CategoryLabels.BondToTurkish(_bondDraftType)}' Bağı Kur", ColGreen, Color.white, 18, () =>
                {
                    // Ayni cift varsa guncelle, yoksa ekle
                    for (int k = _workingBonds.Count - 1; k >= 0; k--)
                    {
                        var ex = _workingBonds[k];
                        bool same = (string.Equals(ex.CharAName, pickA.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(ex.CharBName, pickB.Name, StringComparison.OrdinalIgnoreCase)) ||
                                    (string.Equals(ex.CharAName, pickB.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(ex.CharBName, pickA.Name, StringComparison.OrdinalIgnoreCase));
                        if (same) _workingBonds.RemoveAt(k);
                    }
                    _workingBonds.Add(new PendingBondDraft
                    {
                        CharAId = pickA.ExistingCharacterId ?? "",
                        CharAName = pickA.Name,
                        CharBId = pickB.ExistingCharacterId ?? "",
                        CharBName = pickB.Name,
                        BondType = _bondDraftType
                    });
                    ShowBondEditorView();
                });
                PlaceRect(btnAddBond.GetComponent<RectTransform>(), 0f, 188f, 1084f, 48f);

                // 3. Bu Oyundaki Aktif & Yeni Baglar Listesi
                var scroll = CreateScrollView(_contentRoot, 0f, 246f, 1084f, 250f);
                float innerW = 1068f;
                float yCursor = 0f;

                for (int i = 0; i < _workingBonds.Count; i++)
                {
                    int capIdx = i;
                    var b = _workingBonds[i];
                    var row = CreatePanel(scroll.content, ColCard);
                    PlaceRect(row.GetComponent<RectTransform>(), 0f, yCursor, innerW, 56f);
                    var rTxt = CreateText(row.transform, $"[YENİ BAĞ]  {b.CharAName}  ↔  {b.CharBName}  —  {CategoryLabels.BondToTurkish(b.BondType)}", 16, ColDarkText, TextAnchor.MiddleLeft, true);
                    StretchWithPad(rTxt.GetComponent<RectTransform>(), 14f, 0f);

                    var del = CreateButton(row.transform, "Sil", ColRed, Color.white, 15, () =>
                    {
                        _workingBonds.RemoveAt(capIdx);
                        ShowBondEditorView();
                    });
                    var drt = del.GetComponent<RectTransform>();
                    drt.anchorMin = new Vector2(1f, 0.5f); drt.anchorMax = new Vector2(1f, 0.5f);
                    drt.pivot = new Vector2(1f, 0.5f); drt.anchoredPosition = new Vector2(-10f, 0f); drt.sizeDelta = new Vector2(88f, 38f);
                    yCursor += 64f;
                }

                // Kayitli mevcut baglari da goster
                for (int i = 0; i < specials.Count; i++)
                {
                    for (int j = i + 1; j < specials.Count; j++)
                    {
                        if (!specials[i].IsReturning || !specials[j].IsReturning) continue;
                        var savedB = UniverseSaveManager.FindBond(specials[i].ExistingCharacterId, specials[j].ExistingCharacterId);
                        if (savedB == null) continue;

                        var row = CreatePanel(scroll.content, ColHighlight);
                        PlaceRect(row.GetComponent<RectTransform>(), 0f, yCursor, innerW, 56f);
                        var bType = CategoryLabels.ParseBond(savedB.BondTypeName);
                        var rTxt = CreateText(row.transform,
                            $"[KAYITLI — Sv.{savedB.Level}]  {savedB.CharAName}  ↔  {savedB.CharBName}  —  {CategoryLabels.BondToTurkish(bType)} ({savedB.GamesTogether} ortak oyun)",
                            15, ColDarkText, TextAnchor.MiddleLeft, true);
                        StretchWithPad(rTxt.GetComponent<RectTransform>(), 14f, 0f);
                        yCursor += 64f;
                    }
                }

                if (yCursor == 0f)
                {
                    var empty = CreatePanel(scroll.content, ColCard);
                    PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 90f);
                    var et = CreateText(empty.transform, "Bu kadroda henüz kurulmuş bağ yok. Yukarıdan iki karakter seçip bağ kurabilirsin!", 17, ColMutedText, TextAnchor.MiddleCenter, false);
                    StretchWithPad(et.GetComponent<RectTransform>(), 16f, 8f);
                    yCursor = 98f;
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(234f, yCursor));

                var btnBack = CreateButton(_contentRoot, "< Kadroya Dön", ColYellow, ColDarkText, 19, () => ShowCastOverview());
                PlaceRect(btnBack.GetComponent<RectTransform>(), 0f, 508f, 300f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowBondEditorView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // GORUNUM 2: TEK EKRAN 4 KUTUPLU OZEL KARAKTER EDITORU (YENI / TERFI / EVRIM)
        // ---------------------------------------------------------------------
        private static void ShowCreateCharacterView()
        {
            try
            {
                ClearContent();
                bool isPromote = !string.IsNullOrEmpty(_promotingNpcId);
                bool isEvolve = !string.IsNullOrEmpty(_evolvingCharId);
                _headerTitle.text = isPromote
                    ? $"★ NPC'yi Özel Karaktere Terfi Ettir — {_draftName}"
                    : (isEvolve
                        ? $"Karakter Arkı & Evrim — {_draftName} ({_targetGameTitle})"
                        : $"Özel Karakter Yaz — {_targetGameTitle}");

                _activeNameInput = null;
                _fallbackNameText = null;
                _activeTeamInput = null;
                _fallbackTeamText = null;

                // ===== SOL SUTUN (x=0..515) =====
                var lblName = CreateText(_contentRoot, "İsim:", 17, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblName.GetComponent<RectTransform>(), 0f, 2f, 56f, 38f);

                _activeNameInput = CreateInputFieldSafe(_contentRoot, _draftName, "Karakter adı...", val => _draftName = val, out _fallbackNameText);
                if (_activeNameInput != null)
                    PlaceRect(_activeNameInput.GetComponent<RectTransform>(), 60f, 2f, 326f, 38f);
                else if (_fallbackNameText != null)
                    PlaceRect(_fallbackNameText.transform.parent.GetComponent<RectTransform>(), 60f, 2f, 326f, 38f);

                var btnRand = CreateButton(_contentRoot, isEvolve ? "Eski Hal" : "Rastgele", ColYellow, ColDarkText, 15, () =>
                {
                    if (isEvolve)
                    {
                        var rec = UniverseSaveManager.FindCharacterById(_evolvingCharId);
                        if (rec != null)
                        {
                            _draftAxes = (float[])UniverseSaveManager.EnsureCharacterAxes(rec).Clone();
                            ShowCreateCharacterView();
                        }
                    }
                    else
                    {
                        _draftName = Phase1Database.CharacterNamePool[_namePoolIdx % Phase1Database.CharacterNamePool.Length];
                        _namePoolIdx++;
                        if (_activeNameInput != null) _activeNameInput.text = _draftName;
                        if (_fallbackNameText != null) _fallbackNameText.text = _draftName;
                    }
                });
                PlaceRect(btnRand.GetComponent<RectTransform>(), 394f, 2f, 121f, 38f);

                // Rol secimi
                var lblRole = CreateText(_contentRoot, isPromote ? "Varsayılan Rol:" : (isEvolve ? "Yeni Rolü (İhanet / Kefaret Arkı için değiştir!):" : "Bu Oyundaki Rolü:"), 15, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblRole.GetComponent<RectTransform>(), 0f, 46f, 515f, 22f);

                CharacterRole[] roles = { CharacterRole.Protagonist, CharacterRole.Villain, CharacterRole.Sidekick, CharacterRole.Minor };
                string[] roleShort = { "Başrol", "Kötü Adam", "Yardımcı", "Herhangi Biri" };
                for (int r = 0; r < 4; r++)
                {
                    CharacterRole capRole = roles[r];
                    bool isSel = (_draftRole == capRole);
                    var rb = CreateButton(_contentRoot, roleShort[r], isSel ? ColGreen : ColYellow, isSel ? Color.white : ColDarkText, 14, () =>
                    {
                        _draftRole = capRole;
                        ShowCreateCharacterView();
                    });
                    PlaceRect(rb.GetComponent<RectTransform>(), r * 130f, 70f, 124f, 36f);
                }

                // Ekip / Hizip Etiketi
                var lblTeam = CreateText(_contentRoot, "Ekip:", 17, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblTeam.GetComponent<RectTransform>(), 0f, 114f, 56f, 38f);

                _activeTeamInput = CreateInputFieldSafe(_contentRoot, _draftTeamName, "Ekip / Hizip (opsiyonel)", val => _draftTeamName = val, out _fallbackTeamText);
                if (_activeTeamInput != null)
                    PlaceRect(_activeTeamInput.GetComponent<RectTransform>(), 60f, 114f, 286f, 38f);
                else if (_fallbackTeamText != null)
                    PlaceRect(_fallbackTeamText.transform.parent.GetComponent<RectTransform>(), 60f, 114f, 286f, 38f);

                var savedTeams = UniverseSaveManager.Current.Teams;
                string teamCycleLabel = (savedTeams != null && savedTeams.Count > 0) ? $"Kayıtlı ({savedTeams.Count}) >" : "Bağımsız";
                var btnTeamCycle = CreateButton(_contentRoot, teamCycleLabel, ColBlue, Color.white, 14, () =>
                {
                    if (savedTeams == null || savedTeams.Count == 0) _draftTeamName = "";
                    else
                    {
                        _draftTeamCycleIdx++;
                        if (_draftTeamCycleIdx >= savedTeams.Count) { _draftTeamCycleIdx = -1; _draftTeamName = ""; }
                        else _draftTeamName = savedTeams[_draftTeamCycleIdx];
                    }
                    if (_activeTeamInput != null) _activeTeamInput.text = _draftTeamName;
                    if (_fallbackTeamText != null) _fallbackTeamText.text = _draftTeamName;
                });
                PlaceRect(btnTeamCycle.GetComponent<RectTransform>(), 354f, 114f, 161f, 38f);

                // Karakter Damgasi (5 secenek)
                var lblTrait = CreateText(_contentRoot, "Karakter Damgası (Özel Yetenek):", 16, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblTrait.GetComponent<RectTransform>(), 0f, 158f, 515f, 22f);

                CharacterTrait[] traits =
                {
                    CharacterTrait.None, CharacterTrait.SceneStealer, CharacterTrait.MysteriousPast,
                    CharacterTrait.LocationBound, CharacterTrait.CanonAnchor
                };
                for (int t = 0; t < traits.Length; t++)
                {
                    CharacterTrait capT = traits[t];
                    bool isSel = (_draftTrait == capT);
                    int row = t < 3 ? 0 : 1;
                    int col = t < 3 ? t : (t - 3);
                    float bw = row == 0 ? 166f : 252f;
                    float bx = col * (bw + 8f);
                    float by = 182f + row * 40f;

                    var tb = CreateButton(_contentRoot, CategoryLabels.TraitToTurkish(capT), isSel ? ColPurple : ColYellow, isSel ? Color.white : ColDarkText, 14, () =>
                    {
                        _draftTrait = capT;
                        ShowCreateCharacterView();
                    });
                    PlaceRect(tb.GetComponent<RectTransform>(), bx, by, bw, 35f);
                }

                var traitDesc = CreateText(_contentRoot, CategoryLabels.TraitShortDesc(_draftTrait), 13, ColHeader, TextAnchor.MiddleLeft, true);
                PlaceRect(traitDesc.GetComponent<RectTransform>(), 2f, 260f, 512f, 32f);

                // Hazir Sablonlar
                var lblPreset = CreateText(_contentRoot, "Hazır Arketip Şablonları (Opsiyonel — Sadece Eksenleri Doldurur):", 15, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblPreset.GetComponent<RectTransform>(), 0f, 296f, 515f, 24f);

                var presetScroll = CreateScrollView(_contentRoot, 0f, 322f, 515f, 174f);
                float pCellW = 244f, pCellH = 54f, pGapX = 10f, pGapY = 8f;
                var currentClosest = CharacterAxisSystem.DetectClosestArchetype(_draftAxes);

                for (int i = 0; i < Phase1Database.Archetypes.Count; i++)
                {
                    var a = Phase1Database.Archetypes[i];
                    int col = i % 2;
                    int row = i / 2;
                    bool isMatch = (currentClosest.Id == a.Id);
                    var pb = CreateButton(presetScroll.content, $"{a.Name}\n{a.ShortDesc}", isMatch ? ColGreen : ColCard, isMatch ? Color.white : ColDarkText, 13, () =>
                    {
                        _draftAxes = (float[])a.PresetAxes.Clone();
                        ShowCreateCharacterView();
                    });
                    PlaceRect(pb.GetComponent<RectTransform>(), col * (pCellW + pGapX), row * (pCellH + pGapY), pCellW, pCellH);
                }
                int pRows = (Phase1Database.Archetypes.Count + 1) / 2;
                presetScroll.content.sizeDelta = new Vector2(0f, Mathf.Max(158f, pRows * (pCellH + pGapY)));

                // ===== SAG SUTUN (x=538, w=546): 4 KUTUPLU EKSEN =====
                float rx = 538f;
                float rw = 546f;

                var matchCard = CreatePanel(_contentRoot, ColHighlight);
                PlaceRect(matchCard.GetComponent<RectTransform>(), rx, 2f, rw, 50f);
                string subBanner = isEvolve
                    ? "Karakterin yeni oyundaki evrimini ayarla (+%1 Evrim bonusu + İhanet/Kefaret arkı!)"
                    : "Eksenleri özgürce çek — arketipten saptığın için gizli ceza yok!";
                _activeArchetypeMatchText = CreateText(matchCard.transform,
                    $"Yakışan Tarz: {currentClosest.Name}  |  Evren Tonu: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())}\n{subBanner}",
                    14, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(_activeArchetypeMatchText.GetComponent<RectTransform>(), 12f, 2f);

                for (int i = 0; i < 4; i++)
                {
                    int axIdx = i;
                    float cy = 62f + i * 110f;

                    var card = CreatePanel(_contentRoot, ColCard);
                    PlaceRect(card.GetComponent<RectTransform>(), rx, cy, rw, 102f);

                    var valTxt = CreateText(card.transform,
                        $"{CharacterAxisSystem.AxisNames[axIdx]}:  {CharacterAxisSystem.FormatAxisReading(axIdx, _draftAxes[axIdx])}",
                        17, ColHeader, TextAnchor.MiddleLeft, true);
                    PlaceRect(valTxt.GetComponent<RectTransform>(), 14f, 6f, rw - 28f, 26f);
                    _activeAxisValueTexts[axIdx] = valTxt;

                    var leftLbl = CreateText(card.transform, $"◄ {CharacterAxisSystem.LeftPoles[axIdx]}", 13, ColDarkText, TextAnchor.MiddleLeft, true);
                    PlaceRect(leftLbl.GetComponent<RectTransform>(), 14f, 34f, (rw - 28f) * 0.5f, 20f);

                    var rightLbl = CreateText(card.transform, $"{CharacterAxisSystem.RightPoles[axIdx]} ►", 13, ColDarkText, TextAnchor.MiddleRight, true);
                    PlaceRect(rightLbl.GetComponent<RectTransform>(), 14f + (rw - 28f) * 0.5f, 34f, (rw - 28f) * 0.5f, 20f);

                    var slider = CreateSlider(card.transform, _draftAxes[axIdx], val =>
                    {
                        float c = Mathf.Clamp01(val);
                        _draftAxes[axIdx] = c;
                        if (_activeAxisValueTexts[axIdx] != null)
                            _activeAxisValueTexts[axIdx].text = $"{CharacterAxisSystem.AxisNames[axIdx]}:  {CharacterAxisSystem.FormatAxisReading(axIdx, c)}";
                        if (_activeArchetypeMatchText != null)
                        {
                            var m = CharacterAxisSystem.DetectClosestArchetype(_draftAxes);
                            _activeArchetypeMatchText.text = $"Yakışan Tarz: {m.Name}  |  Evren Tonu: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())}\n{subBanner}";
                        }
                    });
                    PlaceRect(slider.GetComponent<RectTransform>(), 18f, 62f, rw - 36f, 30f);
                }

                // ALT BUTONLAR
                var btnBack = CreateButton(_contentRoot, isPromote ? "< İptal (NPC Listesi)" : (isEvolve ? "< Eski Listeye Dön" : "< Kadroya Dön"), ColYellow, ColDarkText, 18, () =>
                {
                    if (isPromote)
                    {
                        _promotingNpcId = null;
                        _rosterSubTab = 1;
                        ShowFullRosterView();
                    }
                    else if (isEvolve)
                    {
                        _evolvingCharId = null;
                        ShowReturningPickerView();
                    }
                    else ShowCastOverview();
                });
                PlaceRect(btnBack.GetComponent<RectTransform>(), 0f, 508f, 260f, 52f);

                string saveLabel = isPromote ? "★ Özel Karaktere Terfi Ettir!" : (isEvolve ? "★ Evrimi Onayla ve Kadroya Ekle" : "Özel Karakteri Kadroya Ekle");
                var btnAdd = CreateButton(_contentRoot, saveLabel, (isPromote || isEvolve) ? ColPurple : ColGreen, Color.white, 19, () =>
                {
                    string finalName = (_activeNameInput != null && !string.IsNullOrWhiteSpace(_activeNameInput.text))
                        ? _activeNameInput.text.Trim()
                        : (string.IsNullOrWhiteSpace(_draftName) ? "Adsız Karakter" : _draftName.Trim());

                    string rawTeam = (_activeTeamInput != null) ? _activeTeamInput.text : _draftTeamName;
                    string cleanTeam = UniverseSaveManager.RegisterTeam(rawTeam);
                    var matchedArch = CharacterAxisSystem.DetectClosestArchetype(_draftAxes);

                    if (isPromote)
                    {
                        var npcRec = UniverseSaveManager.FindCharacterById(_promotingNpcId);
                        if (npcRec != null)
                        {
                            npcRec.IsNpc = false;
                            npcRec.Name = finalName;
                            npcRec.ArchetypeId = matchedArch.Id;
                            npcRec.ArchetypeName = matchedArch.Name;
                            npcRec.LastRole = CategoryLabels.RoleToTurkish(_draftRole);
                            npcRec.TraitName = _draftTrait.ToString();
                            npcRec.TeamName = cleanTeam;
                            npcRec.Axes = (float[])_draftAxes.Clone();
                            if (npcRec.Status == "Normal") npcRec.Status = "Terfi";
                            UniverseSaveManager.Save();

                            var comp = UniverseArchitectMod.ActiveCompany;
                            if (comp != null)
                            {
                                NotificationHelper.Show(comp, $"NPC Terfi Etti: {npcRec.Name}!",
                                    $"Eski {npcRec.NpcType} NPC'si {npcRec.Name} artık tam yetkili bir Özel Karakter ({matchedArch.Name})!");
                            }
                            _statusBannerMsg = $"★ {npcRec.Name} artık Özel Karakterler kadrosunda!";
                        }
                        _promotingNpcId = null;
                        _rosterSubTab = 0;
                        ShowFullRosterView();
                    }
                    else if (isEvolve)
                    {
                        _workingCast.Add(new ActiveCharacterDraft
                        {
                            IsReturning = true,
                            IsNpc = false,
                            ExistingCharacterId = _evolvingCharId,
                            Name = finalName,
                            ArchetypeId = matchedArch.Id,
                            Role = _draftRole,
                            Trait = _draftTrait,
                            TeamName = cleanTeam,
                            Axes = (float[])_draftAxes.Clone(),
                            AxesEvolved = true
                        });
                        _evolvingCharId = null;
                        ShowCastOverview();
                    }
                    else
                    {
                        _workingCast.Add(new ActiveCharacterDraft
                        {
                            IsReturning = false,
                            IsNpc = false,
                            Name = finalName,
                            ArchetypeId = matchedArch.Id,
                            Role = _draftRole,
                            Trait = _draftTrait,
                            TeamName = cleanTeam,
                            Axes = (float[])_draftAxes.Clone()
                        });
                        ShowCastOverview();
                    }
                });
                PlaceRect(btnAdd.GetComponent<RectTransform>(), 538f, 508f, 546f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowCreateCharacterView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // GORUNUM 2B (FAZ 6): HIZLI NPC EKLEME EKRANI (3 SANIYEDE!)
        // ---------------------------------------------------------------------
        private static void ShowCreateNpcView()
        {
            try
            {
                ClearContent();
                _headerTitle.text = $"Hızlı NPC Ekle — {_targetGameTitle}";
                _activeNameInput = null;
                _fallbackNameText = null;

                var lblName = CreateText(_contentRoot, "NPC Adı:", 18, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblName.GetComponent<RectTransform>(), 0f, 6f, 100f, 42f);

                _activeNameInput = CreateInputFieldSafe(_contentRoot, _npcDraftName, "NPC adı yaz...", val => _npcDraftName = val, out _fallbackNameText);
                if (_activeNameInput != null)
                    PlaceRect(_activeNameInput.GetComponent<RectTransform>(), 106f, 6f, 790f, 42f);
                else if (_fallbackNameText != null)
                    PlaceRect(_fallbackNameText.transform.parent.GetComponent<RectTransform>(), 106f, 6f, 790f, 42f);

                var btnRand = CreateButton(_contentRoot, "Rastgele İsim", ColYellow, ColDarkText, 16, () =>
                {
                    _npcDraftName = Phase1Database.NpcNamePool[_npcNamePoolIdx % Phase1Database.NpcNamePool.Length];
                    _npcNamePoolIdx++;
                    if (_activeNameInput != null) _activeNameInput.text = _npcDraftName;
                    if (_fallbackNameText != null) _fallbackNameText.text = _npcDraftName;
                });
                PlaceRect(btnRand.GetComponent<RectTransform>(), 910f, 6f, 174f, 42f);

                // 6 NPC Tipi (2 satir x 3 sutun)
                var lblType = CreateText(_contentRoot, "NPC Tipi Seç:", 17, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblType.GetComponent<RectTransform>(), 0f, 60f, 1084f, 26f);

                for (int i = 0; i < Phase1Database.NpcTypes.Length; i++)
                {
                    int capI = i;
                    int row = i / 3;
                    int col = i % 3;
                    bool sel = (_npcTypeIdx == i);
                    var tb = CreateButton(_contentRoot, Phase1Database.NpcTypes[i], sel ? ColTeal : ColYellow, sel ? Color.white : ColDarkText, 17, () =>
                    {
                        _npcTypeIdx = capI;
                        ShowCreateNpcView();
                    });
                    PlaceRect(tb.GetComponent<RectTransform>(), col * 366f, 92f + row * 50f, 352f, 42f);
                }

                // Bagli oldugu ozel karakter secimi
                var lblLink = CreateText(_contentRoot,
                    "Bağlı Olduğu Özel Karakter (Opsiyonel — Tekrar kullandığında bu özel karakter yanında olmazsa hafif risk oluşur):",
                    16, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblLink.GetComponent<RectTransform>(), 0f, 202f, 1084f, 28f);

                var scroll = CreateScrollView(_contentRoot, 0f, 234f, 1084f, 262f);
                float innerW = 1068f;
                float yCursor = 0f;

                // Secenek 0: Bagimsiz NPC
                bool noneSel = string.IsNullOrEmpty(_npcLinkedCharName);
                var noneBtn = CreateButton(scroll.content,
                    noneSel ? "✓ Bağımsız NPC (Hiçbir özel karaktere bağlı değil)" : "Bağımsız NPC (Hiçbir özel karaktere bağlı değil)",
                    noneSel ? ColGreen : ColCard, noneSel ? Color.white : ColDarkText, 16, () =>
                    {
                        _npcLinkedCharId = "";
                        _npcLinkedCharName = "";
                        ShowCreateNpcView();
                    });
                PlaceRect(noneBtn.GetComponent<RectTransform>(), 0f, yCursor, innerW, 44f);
                yCursor += 52f;

                var listedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 1) Bu kadrodaki Ozel Karakterler
                for (int i = 0; i < _workingCast.Count; i++)
                {
                    var d = _workingCast[i];
                    if (d.IsNpc || !listedNames.Add(d.Name)) continue;
                    string capId = d.ExistingCharacterId ?? "";
                    string capName = d.Name;
                    bool sel = string.Equals(_npcLinkedCharName, capName, StringComparison.OrdinalIgnoreCase);

                    var b = CreateButton(scroll.content,
                        $"{(sel ? "✓ " : "")}[Bu Kadroda]  {capName} ({CategoryLabels.RoleToTurkish(d.Role)})",
                        sel ? ColGreen : ColHighlight, sel ? Color.white : ColDarkText, 16, () =>
                        {
                            _npcLinkedCharId = capId;
                            _npcLinkedCharName = capName;
                            ShowCreateNpcView();
                        });
                    PlaceRect(b.GetComponent<RectTransform>(), 0f, yCursor, innerW, 44f);
                    yCursor += 52f;
                }

                // 2) Evrende kayitli Ozel Karakterler
                var specials = UniverseSaveManager.GetSpecialCharacters();
                for (int i = 0; i < specials.Count; i++)
                {
                    var c = specials[i];
                    if (!listedNames.Add(c.Name)) continue;
                    string capId = c.Id;
                    string capName = c.Name;
                    bool sel = string.Equals(_npcLinkedCharName, capName, StringComparison.OrdinalIgnoreCase);

                    var b = CreateButton(scroll.content,
                        $"{(sel ? "✓ " : "")}{capName}  [{c.Status}]  —  Çıkış: {c.DebutGame}",
                        sel ? ColGreen : ColCard, sel ? Color.white : ColDarkText, 16, () =>
                        {
                            _npcLinkedCharId = capId;
                            _npcLinkedCharName = capName;
                            ShowCreateNpcView();
                        });
                    PlaceRect(b.GetComponent<RectTransform>(), 0f, yCursor, innerW, 44f);
                    yCursor += 52f;
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(246f, yCursor));

                var btnBack = CreateButton(_contentRoot, "< Kadroya Dön", ColYellow, ColDarkText, 19, () => ShowCastOverview());
                PlaceRect(btnBack.GetComponent<RectTransform>(), 0f, 508f, 260f, 52f);

                var btnAdd = CreateButton(_contentRoot, "NPC'yi Kadroya Ekle", ColTeal, Color.white, 20, () =>
                {
                    string finalName = (_activeNameInput != null && !string.IsNullOrWhiteSpace(_activeNameInput.text))
                        ? _activeNameInput.text.Trim()
                        : (string.IsNullOrWhiteSpace(_npcDraftName) ? "Adsız NPC" : _npcDraftName.Trim());

                    _workingCast.Add(new ActiveCharacterDraft
                    {
                        IsReturning = false,
                        IsNpc = true,
                        Name = finalName,
                        NpcType = Phase1Database.NpcTypes[_npcTypeIdx % Phase1Database.NpcTypes.Length],
                        LinkedSpecialCharId = _npcLinkedCharId,
                        LinkedSpecialCharName = _npcLinkedCharName
                    });
                    ShowCastOverview();
                });
                PlaceRect(btnAdd.GetComponent<RectTransform>(), 538f, 508f, 546f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowCreateNpcView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // GORUNUM 3: ESKI KARAKTER & NPC CAGIR (+ EVRIM, OZLEM, SPIN-OFF, KADER)
        // ---------------------------------------------------------------------
        private static void ShowReturningPickerView()
        {
            try
            {
                ClearContent();
                _headerTitle.text = $"Eski Karakter / NPC Çağır — {_targetGameTitle}";

                var specials = UniverseSaveManager.GetSpecialCharacters();
                var npcs = UniverseSaveManager.GetNpcCharacters();
                bool isMainUniverse = UniverseArchitectMod.ActiveProjectCategory == ProjectCategory.MainUniverse;

                var tabSp = CreateButton(_contentRoot, $"★ Özel Karakterler ({specials.Count})", !_returningShowNpcs ? ColGreen : ColYellow, !_returningShowNpcs ? Color.white : ColDarkText, 16, () =>
                {
                    _returningShowNpcs = false;
                    ShowReturningPickerView();
                });
                PlaceRect(tabSp.GetComponent<RectTransform>(), 0f, 0f, 260f, 42f);

                var tabNpc = CreateButton(_contentRoot, $"NPC'ler ({npcs.Count})", _returningShowNpcs ? ColTeal : ColYellow, _returningShowNpcs ? Color.white : ColDarkText, 16, () =>
                {
                    _returningShowNpcs = true;
                    ShowReturningPickerView();
                });
                PlaceRect(tabNpc.GetComponent<RectTransform>(), 270f, 0f, 210f, 42f);

                if (!_returningShowNpcs)
                {
                    CharacterRole[] roles = { CharacterRole.Protagonist, CharacterRole.Villain, CharacterRole.Sidekick, CharacterRole.Minor };
                    string[] roleShort = { "Başrol", "Kötü Adam", "Yardımcı", "Herhangi Biri" };
                    for (int r = 0; r < 4; r++)
                    {
                        CharacterRole capR = roles[r];
                        bool sel = (_returningRole == capR);
                        var rb = CreateButton(_contentRoot, roleShort[r], sel ? ColPurple : ColCard, sel ? Color.white : ColDarkText, 14, () =>
                        {
                            _returningRole = capR;
                            ShowReturningPickerView();
                        });
                        PlaceRect(rb.GetComponent<RectTransform>(), 496f + r * 149f, 0f, 142f, 42f);
                    }
                }
                else
                {
                    var tipTxt = CreateText(_contentRoot, "İpucu: NPC'yi bağlı olduğu özel karakter olmadan eklemek hafif bağlam riski taşır.", 14, ColMutedText, TextAnchor.MiddleLeft, true);
                    PlaceRect(tipTxt.GetComponent<RectTransform>(), 496f, 0f, 588f, 42f);
                }

                var scroll = CreateScrollView(_contentRoot, 0f, 54f, 1084f, 442f);
                var list = _returningShowNpcs ? npcs : specials;
                int fatigueThreshold = UniverseSaveManager.GetLaw() == CosmologyLaw.CharacterLegendEra ? 4 : 3;
                float innerW = 1068f;
                float yCursor = 0f;

                if (list.Count == 0)
                {
                    var empty = CreatePanel(scroll.content, ColCard);
                    PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 100f);
                    var et = CreateText(empty.transform, _returningShowNpcs ? "Kayıtlı NPC yok." : "Kayıtlı Özel Karakter yok.", 18, ColMutedText, TextAnchor.MiddleCenter, false);
                    StretchWithPad(et.GetComponent<RectTransform>(), 16f, 8f);
                    yCursor = 108f;
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var c = list[i];
                        var fate = CategoryLabels.ParseFate(c.FateName);
                        bool isMissedStar = !c.IsNpc && fate == CharacterFate.Alive && c.GamesMissed >= 2 && (c.Status == "Yıldız" || c.Status == "Kült" || c.Popularity >= 75);

                        Color cardBg = c.IsNpc
                            ? new Color(0.88f, 0.92f, 0.92f, 1f)
                            : (fate != CharacterFate.Alive ? new Color(0.95f, 0.87f, 0.90f, 1f) : (isMissedStar ? ColHighlight : ColCard));

                        var card = CreatePanel(scroll.content, cardBg);
                        PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 88f);

                        string line;
                        if (c.IsNpc)
                        {
                            string linkInfo = string.IsNullOrEmpty(c.LinkedSpecialCharName)
                                ? "Bağımsız NPC"
                                : (IsSpecialCharInWorkingCast(c.LinkedSpecialCharId, c.LinkedSpecialCharName)
                                    ? $"Bağlı: {c.LinkedSpecialCharName} (✓ Kadroda)"
                                    : $"Bağlı: {c.LinkedSpecialCharName} (⚠ Kadroda yok — hafif risk!)");
                            line = $"{c.Name}  [{c.Status}]  —  NPC ({c.NpcType})  |  Oyun: {c.GamesAppeared}\n{linkInfo}  |  Çıkış: {c.DebutGame}";
                        }
                        else
                        {
                            var badges = new List<string>();
                            if (fate == CharacterFate.Sacrificed)
                                badges.Add(isMainUniverse ? "☠ ÖLDÜ (⚠ Ana Evren'de Diriltme Cezası!)" : "☠ ÖLDÜ (✓ Prequel/Alt.Zaman Güvenli)");
                            else if (fate == CharacterFate.Retired)
                                badges.Add(isMainUniverse ? "★ EMEKLİ (⚠ Ana Evren'de Bozulur)" : "★ EMEKLİ (✓ Yan/Alt.Zaman Güvenli)");
                            if (isMissedStar) badges.Add($"🔥 ÖZLENDİ ({c.GamesMissed} oyun yoktu!)");
                            if (c.SpinOffDemandRemaining > 0) badges.Add($"🎬 SPİN-OFF İSTENİYOR ({c.SpinOffDemandRemaining} oyun kaldı)");

                            string badgeStr = badges.Count > 0 ? "  |  " + string.Join("  ", badges) : "";
                            string fatigue = c.ConsecutiveUses >= fatigueThreshold ? "YORGUN!" : $"Dinç (Seri {c.ConsecutiveUses})";
                            string teamTxt = string.IsNullOrWhiteSpace(c.TeamName) ? "Bağımsız" : c.TeamName;

                            line = $"{c.Name}  [{c.Status}]  —  {c.ArchetypeName}  |  Ekip: {teamTxt}{badgeStr}\n" +
                                   $"Pop: {c.Popularity}/100  |  Oyun: {c.GamesAppeared} (Başrol:{c.ProtagonistGames}, Kötü:{c.VillainGames})  |  {fatigue}  |  Çıkış: {c.DebutGame}";
                        }

                        var txt = CreateText(card.transform, line, 14, ColDarkText, TextAnchor.MiddleLeft, false);
                        var trt = txt.GetComponent<RectTransform>();
                        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                        trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(c.IsNpc ? -175f : -310f, -4f);

                        if (!c.IsNpc)
                        {
                            var evoBtn = CreateButton(card.transform, "Ark / Evrim", ColPurple, Color.white, 15, () =>
                            {
                                _promotingNpcId = null;
                                _evolvingCharId = c.Id;
                                _draftName = c.Name;
                                _draftRole = _returningRole;
                                _draftTrait = CategoryLabels.ParseTrait(c.TraitName);
                                _draftTeamName = c.TeamName ?? "";
                                _draftAxes = (float[])UniverseSaveManager.EnsureCharacterAxes(c).Clone();
                                ShowCreateCharacterView();
                            });
                            var ert = evoBtn.GetComponent<RectTransform>();
                            ert.anchorMin = new Vector2(1f, 0.5f); ert.anchorMax = new Vector2(1f, 0.5f);
                            ert.pivot = new Vector2(1f, 0.5f);
                            ert.anchoredPosition = new Vector2(-172f, 0f);
                            ert.sizeDelta = new Vector2(126f, 48f);
                        }

                        var pickBtn = CreateButton(card.transform, "Kadroya Ekle", c.IsNpc ? ColTeal : ColGreen, Color.white, 15, () =>
                        {
                            if (c.IsNpc)
                            {
                                _workingCast.Add(new ActiveCharacterDraft
                                {
                                    IsReturning = true,
                                    IsNpc = true,
                                    ExistingCharacterId = c.Id,
                                    Name = c.Name,
                                    NpcType = c.NpcType,
                                    LinkedSpecialCharId = c.LinkedSpecialCharId,
                                    LinkedSpecialCharName = c.LinkedSpecialCharName
                                });
                            }
                            else
                            {
                                _workingCast.Add(new ActiveCharacterDraft
                                {
                                    IsReturning = true,
                                    IsNpc = false,
                                    ExistingCharacterId = c.Id,
                                    Name = c.Name,
                                    ArchetypeId = c.ArchetypeId,
                                    Role = _returningRole,
                                    Trait = CategoryLabels.ParseTrait(c.TraitName),
                                    TeamName = c.TeamName ?? "",
                                    Axes = (float[])UniverseSaveManager.EnsureCharacterAxes(c).Clone()
                                });
                            }
                            ShowCastOverview();
                        });
                        var prt = pickBtn.GetComponent<RectTransform>();
                        prt.anchorMin = new Vector2(1f, 0.5f); prt.anchorMax = new Vector2(1f, 0.5f);
                        prt.pivot = new Vector2(1f, 0.5f);
                        prt.anchoredPosition = new Vector2(-12f, 0f);
                        prt.sizeDelta = new Vector2(150f, 48f);

                        yCursor += 96f;
                    }
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(426f, yCursor));

                var btnBack = CreateButton(_contentRoot, "< Kadroya Dön", ColYellow, ColDarkText, 19, () => ShowCastOverview());
                PlaceRect(btnBack.GetComponent<RectTransform>(), 0f, 508f, 260f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowReturningPickerView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // MOD ICERIGI SEKME 0: KARAKTERLER (3 ALT SEKME: OZEL / NPC / BAGLAR & MUZE)
        // ---------------------------------------------------------------------
        private static void ShowFullRosterView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                var specials = UniverseSaveManager.GetSpecialCharacters();
                var npcs = UniverseSaveManager.GetNpcCharacters();
                int bondCount = u.Bonds != null ? u.Bonds.Count : 0;

                _headerTitle.text = $"Mod İçeriği — Karakterler ({specials.Count} Özel, {npcs.Count} NPC, {bondCount} Bağ)";

                DrawModContentTabs(0);

                var summary = CreatePanel(_contentRoot, ColCard);
                PlaceRect(summary.GetComponent<RectTransform>(), 0f, 52f, 1084f, 40f);
                var sumTxt = CreateText(summary.transform,
                    $"Ton: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())}  |  Yasa: {CategoryLabels.LawToTurkish(UniverseSaveManager.GetLaw())}  |  Tutarlılık: {u.CanonIntegrity}/100  |  Bariyer: {u.LoreBarrier}/100  |  Ekip: {u.Teams.Count}",
                    15, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(sumTxt.GetComponent<RectTransform>(), 14f, 0f);

                // 3 alt sekme: Ozel Karakterler / NPC'ler / Baglar & Muze
                var subSpecial = CreateButton(_contentRoot, $"★ Özel Karakterler ({specials.Count})", _rosterSubTab == 0 ? ColGreen : ColYellow, _rosterSubTab == 0 ? Color.white : ColDarkText, 15, () =>
                {
                    _rosterSubTab = 0; _statusBannerMsg = ""; ShowFullRosterView();
                });
                PlaceRect(subSpecial.GetComponent<RectTransform>(), 0f, 98f, 354f, 38f);

                var subNpc = CreateButton(_contentRoot, $"NPC'ler ({npcs.Count}) — Terfi", _rosterSubTab == 1 ? ColTeal : ColYellow, _rosterSubTab == 1 ? Color.white : ColDarkText, 15, () =>
                {
                    _rosterSubTab = 1; _statusBannerMsg = ""; ShowFullRosterView();
                });
                PlaceRect(subNpc.GetComponent<RectTransform>(), 365f, 98f, 354f, 38f);

                var subBonds = CreateButton(_contentRoot, $"Bağlar & Efsane Müzesi ({bondCount})", _rosterSubTab == 2 ? ColPurple : ColYellow, _rosterSubTab == 2 ? Color.white : ColDarkText, 15, () =>
                {
                    _rosterSubTab = 2; _statusBannerMsg = ""; ShowFullRosterView();
                });
                PlaceRect(subBonds.GetComponent<RectTransform>(), 730f, 98f, 354f, 38f);

                float listTop = 144f;
                float listHeight = string.IsNullOrEmpty(_statusBannerMsg) ? 354f : 326f;
                var scroll = CreateScrollView(_contentRoot, 0f, listTop, 1084f, listHeight);
                int fatigueThreshold = UniverseSaveManager.GetLaw() == CosmologyLaw.CharacterLegendEra ? 4 : 3;
                float innerW = 1068f;
                float yCursor = 0f;

                if (_rosterSubTab == 0)
                {
                    if (specials.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 110f);
                        var t = CreateText(empty.transform,
                            "Henüz kayıtlı Özel Karakter yok. Yeni bir Ana/Yan Evren veya Alternatif Zaman oyunu başlattığında 4 eksenli editör otomatik açılacak.",
                            18, ColMutedText, TextAnchor.MiddleCenter, false);
                        StretchWithPad(t.GetComponent<RectTransform>(), 20f, 10f);
                        yCursor = 118f;
                    }
                    else
                    {
                        for (int i = 0; i < specials.Count; i++)
                        {
                            var c = specials[i];
                            var fate = CategoryLabels.ParseFate(c.FateName);
                            var card = CreatePanel(scroll.content, fate != CharacterFate.Alive ? new Color(0.95f, 0.87f, 0.90f, 1f) : ColCard);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 96f);

                            string fatigue = fate != CharacterFate.Alive
                                ? CategoryLabels.FateToTurkish(fate)
                                : (c.ConsecutiveUses >= fatigueThreshold ? "YORGUN" : (c.GamesMissed >= 2 ? $"🔥 Özlendi ({c.GamesMissed})" : $"Seri {c.ConsecutiveUses}"));
                            string teamTxt = string.IsNullOrWhiteSpace(c.TeamName) ? "Bağımsız" : c.TeamName;
                            var tr = CategoryLabels.ParseTrait(c.TraitName);
                            string traitTxt = tr == CharacterTrait.None ? "Damga Yok" : CategoryLabels.TraitToTurkish(tr);
                            string arcStr = string.IsNullOrEmpty(c.ArcTag) ? "" : $" | {c.ArcTag}";
                            string spStr = c.SpinOffDemandRemaining > 0 ? $" | 🎬 Spin-Off İsteniyor!" : "";
                            string axLine = CharacterAxisSystem.FormatCompactSummary(UniverseSaveManager.EnsureCharacterAxes(c));

                            var txt = CreateText(card.transform,
                                $"{c.Name}  [{c.Status}]  —  {c.ArchetypeName} ({c.LastRole})  |  Pop: {c.Popularity}/100  |  Oyun: {c.GamesAppeared}  |  {fatigue}{arcStr}{spStr}\n" +
                                $"Ekip: {teamTxt}  |  Damga: {traitTxt}  |  Çıkış: {c.DebutGame}\n{axLine}",
                                14, ColDarkText, TextAnchor.MiddleLeft, false);
                            var trt = txt.GetComponent<RectTransform>();
                            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                            trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(-175f, -4f);

                            var teamBtn = CreateButton(card.transform, u.Teams.Count > 0 ? "Ekip Değiştir" : "Ekip Yok",
                                u.Teams.Count > 0 ? ColBlue : ColTrack, Color.white, 14, () =>
                                {
                                    if (u.Teams.Count == 0)
                                    {
                                        _statusBannerMsg = "Yeni bir karakter yazarken 'Ekip' kutusuna ekip adı girerek ilk ekibini kurabilirsin!";
                                    }
                                    else
                                    {
                                        int curIdx = -1;
                                        for (int k = 0; k < u.Teams.Count; k++)
                                            if (string.Equals(u.Teams[k], c.TeamName, StringComparison.OrdinalIgnoreCase)) { curIdx = k; break; }
                                        int nextIdx = curIdx + 1;
                                        c.TeamName = nextIdx >= u.Teams.Count ? "" : u.Teams[nextIdx];
                                        UniverseSaveManager.Save();
                                        _statusBannerMsg = string.IsNullOrEmpty(c.TeamName)
                                            ? $"{c.Name} artık Bağımsız."
                                            : $"{c.Name} -> '{c.TeamName}' ekibine atandı!";
                                    }
                                    ShowFullRosterView();
                                });
                            var brt = teamBtn.GetComponent<RectTransform>();
                            brt.anchorMin = new Vector2(1f, 0.5f); brt.anchorMax = new Vector2(1f, 0.5f);
                            brt.pivot = new Vector2(1f, 0.5f);
                            brt.anchoredPosition = new Vector2(-12f, 0f);
                            brt.sizeDelta = new Vector2(150f, 44f);

                            yCursor += 104f;
                        }
                    }
                }
                else if (_rosterSubTab == 1)
                {
                    if (npcs.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 110f);
                        var t = CreateText(empty.transform,
                            "Henüz kayıtlı NPC yok. Oyun kadrosu ekranında '+ NPC (Hızlı)' butonundan tüccar, muhafız veya gizemli yabancılar ekleyebilir; sevilenleri buradan Özel Karaktere terfi ettirebilirsin!",
                            17, ColMutedText, TextAnchor.MiddleCenter, false);
                        StretchWithPad(t.GetComponent<RectTransform>(), 20f, 10f);
                        yCursor = 118f;
                    }
                    else
                    {
                        for (int i = 0; i < npcs.Count; i++)
                        {
                            var c = npcs[i];
                            Color bg = c.Status == "Kült" ? ColHighlight : (c.Status == "Nefret Edilen" ? new Color(0.96f, 0.85f, 0.84f, 1f) : new Color(0.88f, 0.92f, 0.92f, 1f));
                            var card = CreatePanel(scroll.content, bg);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 84f);

                            string linkTxt = string.IsNullOrEmpty(c.LinkedSpecialCharName) ? "Bağımsız NPC" : $"Bağlı Özel Karakter: {c.LinkedSpecialCharName}";
                            var txt = CreateText(card.transform,
                                $"{c.Name}  [{c.Status}]  —  NPC ({c.NpcType})  |  Popülerlik: {c.Popularity}/100  |  Oyun: {c.GamesAppeared}\n" +
                                $"{linkTxt}  |  Çıkış Oyunu: {c.DebutGame}",
                                15, ColDarkText, TextAnchor.MiddleLeft, false);
                            var trt = txt.GetComponent<RectTransform>();
                            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                            trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(-210f, -4f);

                            var promoteBtn = CreateButton(card.transform, "★ Özel Karakter Yap", ColPurple, Color.white, 15, () =>
                            {
                                _promotingNpcId = c.Id;
                                _evolvingCharId = null;
                                _draftName = c.Name;
                                _draftRole = CharacterRole.Sidekick;
                                _draftTrait = c.Status == "Kült" ? CharacterTrait.SceneStealer : CharacterTrait.None;
                                _draftTeamName = "";
                                _draftTeamCycleIdx = -1;
                                _draftAxes = new[] { 0.50f, 0.55f, 0.60f, 0.40f };
                                ShowCreateCharacterView();
                            });
                            var prt = promoteBtn.GetComponent<RectTransform>();
                            prt.anchorMin = new Vector2(1f, 0.5f); prt.anchorMax = new Vector2(1f, 0.5f);
                            prt.pivot = new Vector2(1f, 0.5f);
                            prt.anchoredPosition = new Vector2(-12f, 0f);
                            prt.sizeDelta = new Vector2(186f, 48f);

                            yCursor += 92f;
                        }
                    }
                }
                else // _rosterSubTab == 2: Baglar & Olumsuz Efsaneler Muzesi
                {
                    if (u.Bonds != null && u.Bonds.Count > 0)
                    {
                        for (int i = 0; i < u.Bonds.Count; i++)
                        {
                            var b = u.Bonds[i];
                            var bType = CategoryLabels.ParseBond(b.BondTypeName);
                            var card = CreatePanel(scroll.content, ColHighlight);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 62f);
                            var txt = CreateText(card.transform,
                                $"[BAĞ — Sv.{b.Level}]  {b.CharAName}  ↔  {b.CharBName}  —  {CategoryLabels.BondToTurkish(bType)} ({b.GamesTogether} ortak oyun)",
                                15, ColDarkText, TextAnchor.MiddleLeft, true);
                            StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 4f);
                            yCursor += 70f;
                        }
                    }

                    // Olumsuz / Emekli Efsaneler Muzesi
                    int museumCount = 0;
                    for (int i = 0; i < specials.Count; i++)
                    {
                        var c = specials[i];
                        var f = CategoryLabels.ParseFate(c.FateName);
                        if (f == CharacterFate.Alive) continue;
                        museumCount++;
                        var card = CreatePanel(scroll.content, new Color(0.95f, 0.87f, 0.90f, 1f));
                        PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 68f);
                        var txt = CreateText(card.transform,
                            $"[MÜZE — {CategoryLabels.FateToTurkish(f)}]  {c.Name} ({c.ArchetypeName})  |  Pop: {c.Popularity}/100  |  Oyun: {c.GamesAppeared}\nVeda Oyunu: {c.FateGame}  —  Yan Evren / Alternatif Zaman'da güvenle kullanılabilir.",
                            14, ColDarkText, TextAnchor.MiddleLeft, false);
                        StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 4f);
                        yCursor += 76f;
                    }

                    if (yCursor == 0f)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 110f);
                        var t = CreateText(empty.transform,
                            "Henüz kayıtlı İlişki Bağı veya Müze Efsanesi yok.\nOyun kadrosunda 'Bağ Kur' ile Ezeli Rekabet / Usta–Çırak bağları kurabilir, kıdemli yıldızlara Final Fedası yazabilirsin!",
                            17, ColMutedText, TextAnchor.MiddleCenter, false);
                        StretchWithPad(t.GetComponent<RectTransform>(), 20f, 10f);
                        yCursor = 118f;
                    }
                }

                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(listHeight - 16f, yCursor));

                if (!string.IsNullOrEmpty(_statusBannerMsg))
                {
                    var msgTxt = CreateText(_contentRoot, _statusBannerMsg, 15, ColPurple, TextAnchor.MiddleLeft, true);
                    PlaceRect(msgTxt.GetComponent<RectTransform>(), 4f, 474f, 1076f, 26f);
                }

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowFullRosterView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // MOD ICERIGI SEKME 1: TRANSMEDYA STUDYOSU
        // ---------------------------------------------------------------------
        private static void ShowTransmediaStudioView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                var comp = UniverseArchitectMod.ActiveCompany;
                double cashK = comp != null ? comp.cash / 1000.0 : 0.0;
                double curWeek = comp != null ? comp.currentWeek : 0.0;

                _headerTitle.text = $"Mod İçeriği — Transmedya Stüdyosu (Kasa: {cashK:F1}K | Miras: +{u.PendingTransmediaHype} Hype)";

                DrawModContentTabs(1);

                if (_transmediaFocusCharIdx >= u.Characters.Count) _transmediaFocusCharIdx = -1;
                CharacterRecord focusChar = (_transmediaFocusCharIdx >= 0 && _transmediaFocusCharIdx < u.Characters.Count)
                    ? u.Characters[_transmediaFocusCharIdx]
                    : null;

                string focusLabel = focusChar != null
                    ? $"Odak Karakter: {focusChar.Name} {(focusChar.IsNpc ? "(NPC)" : "(Özel)")} — Popülerlik: {focusChar.Popularity}/100"
                    : "Odak: Genel Evren (Karakter seçmek için sağdaki butona dokun)";

                var focusCard = CreatePanel(_contentRoot, ColCard);
                PlaceRect(focusCard.GetComponent<RectTransform>(), 0f, 54f, 1084f, 44f);
                var focusTxt = CreateText(focusCard.transform, focusLabel, 17, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(focusTxt.GetComponent<RectTransform>(), 16f, 0f);

                if (u.Characters.Count > 0)
                {
                    var btnCycleFocus = CreateButton(focusCard.transform, "Odak Değiştir >", ColYellow, ColDarkText, 16, () =>
                    {
                        _transmediaFocusCharIdx++;
                        if (_transmediaFocusCharIdx >= u.Characters.Count) _transmediaFocusCharIdx = -1;
                        ShowTransmediaStudioView();
                    });
                    var brt = btnCycleFocus.GetComponent<RectTransform>();
                    brt.anchorMin = new Vector2(1f, 0.5f); brt.anchorMax = new Vector2(1f, 0.5f);
                    brt.pivot = new Vector2(1f, 0.5f);
                    brt.anchoredPosition = new Vector2(-10f, 0f);
                    brt.sizeDelta = new Vector2(180f, 34f);
                }

                float cardW = 352f;
                for (int i = 0; i < Phase1Database.TransmediaCatalog.Count; i++)
                {
                    var def = Phase1Database.TransmediaCatalog[i];
                    float cx = i * (cardW + 14f);

                    var pCard = CreatePanel(_contentRoot, ColCard);
                    PlaceRect(pCard.GetComponent<RectTransform>(), cx, 108f, cardW, 166f);

                    var pTitle = CreateText(pCard.transform, $"{def.Name}\nMaliyet: {def.Cost / 1000.0:F0}K  |  Süre: {def.DurationWeeks} Hafta",
                        17, ColHeader, TextAnchor.UpperLeft, true);
                    PlaceRect(pTitle.GetComponent<RectTransform>(), 12f, 8f, cardW - 24f, 46f);

                    var pDesc = CreateText(pCard.transform, def.ShortEffect, 14, ColDarkText, TextAnchor.UpperLeft, false);
                    PlaceRect(pDesc.GetComponent<RectTransform>(), 12f, 58f, cardW - 24f, 56f);

                    string blockReason;
                    bool canStart = TransmediaSystem.CanStartProject(comp, def, out blockReason);
                    var startBtn = CreateButton(pCard.transform, canStart ? "Üretimi Başlat" : "Kilitli / Bakiye Yok",
                        canStart ? ColGreen : ColTrack, canStart ? Color.white : ColDarkText, 16, () =>
                        {
                            string msg;
                            TransmediaSystem.StartProject(comp, def, focusChar, out msg);
                            _statusBannerMsg = msg;
                            UpdateHudState();
                            ShowTransmediaStudioView();
                        });
                    PlaceRect(startBtn.GetComponent<RectTransform>(), 12f, 118f, cardW - 24f, 38f);
                }

                if (!string.IsNullOrEmpty(_statusBannerMsg))
                {
                    var msgTxt = CreateText(_contentRoot, _statusBannerMsg, 16, ColPurple, TextAnchor.MiddleLeft, true);
                    PlaceRect(msgTxt.GetComponent<RectTransform>(), 4f, 278f, 1076f, 26f);
                }

                var scroll = CreateScrollView(_contentRoot, 0f, 308f, 1084f, 190f);
                float innerW = 1068f;
                float yCursor = 0f;

                if (u.TransmediaProjects.Count == 0)
                {
                    var empty = CreatePanel(scroll.content, ColCard);
                    PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 90f);
                    var t = CreateText(empty.transform,
                        "Henüz başlattığın bir çizgi roman, roman veya albüm yok. Yukarıdan evrenini oyun dışına taşıyabilirsin!",
                        17, ColMutedText, TextAnchor.MiddleCenter, false);
                    StretchWithPad(t.GetComponent<RectTransform>(), 16f, 8f);
                    yCursor = 98f;
                }
                else
                {
                    for (int i = u.TransmediaProjects.Count - 1; i >= 0; i--)
                    {
                        var p = u.TransmediaProjects[i];
                        var row = CreatePanel(scroll.content, ColCard);
                        PlaceRect(row.GetComponent<RectTransform>(), 0f, yCursor, innerW, 64f);

                        string statusLine = p.IsCompleted
                            ? p.ResultSummary
                            : $"ÜRETİMDE — Kalan: ~{Math.Max(0.1, p.FinishWeek - curWeek):F1} hafta | Odak: {p.FocusCharacterName}";

                        var rTxt = CreateText(row.transform, $"{p.Title}\n{statusLine}", 15, ColDarkText, TextAnchor.MiddleLeft, false);
                        StretchWithPad(rTxt.GetComponent<RectTransform>(), 14f, 4f);
                        yCursor += 72f;
                    }
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(174f, yCursor));

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowTransmediaStudioView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // MOD ICERIGI SEKME 2: KOZMOLOJI ANAYASASI & SOLO-DEV TUKENMISLIK
        // ---------------------------------------------------------------------
        private static void ShowCosmologyView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                var comp = UniverseArchitectMod.ActiveCompany;
                var currentLaw = UniverseSaveManager.GetLaw();

                _headerTitle.text = "Mod İçeriği — Kozmoloji Kuralları & Solo-Dev Dengesi";
                DrawModContentTabs(2);

                var curCard = CreatePanel(_contentRoot, ColHighlight);
                PlaceRect(curCard.GetComponent<RectTransform>(), 0f, 54f, 1084f, 62f);
                var curTxt = CreateText(curCard.transform,
                    $"Aktif Kozmoloji Yasası: {CategoryLabels.LawToTurkish(currentLaw)}\n{CategoryLabels.LawShortDesc(currentLaw)}",
                    15, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(curTxt.GetComponent<RectTransform>(), 16f, 6f);

                CosmologyLaw[] laws = { CosmologyLaw.StrictCanon, CosmologyLaw.FlexibleMultiverse, CosmologyLaw.CharacterLegendEra };
                float cardW = 352f;
                for (int i = 0; i < 3; i++)
                {
                    CosmologyLaw capLaw = laws[i];
                    bool isActive = (currentLaw == capLaw);
                    float cx = i * (cardW + 14f);

                    var lCard = CreatePanel(_contentRoot, ColCard);
                    PlaceRect(lCard.GetComponent<RectTransform>(), cx, 126f, cardW, 200f);

                    var lTitle = CreateText(lCard.transform, CategoryLabels.LawToTurkish(capLaw), 18, ColHeader, TextAnchor.UpperLeft, true);
                    PlaceRect(lTitle.GetComponent<RectTransform>(), 12f, 10f, cardW - 24f, 28f);

                    var lDesc = CreateText(lCard.transform, CategoryLabels.LawShortDesc(capLaw), 14, ColDarkText, TextAnchor.UpperLeft, false);
                    PlaceRect(lDesc.GetComponent<RectTransform>(), 12f, 44f, cardW - 24f, 96f);

                    string btnLabel;
                    Color btnColor;
                    if (isActive) { btnLabel = "Aktif Yasa"; btnColor = ColGreen; }
                    else if (currentLaw == CosmologyLaw.None) { btnLabel = "Yasayı İlan Et (Ücretsiz)"; btnColor = ColPurple; }
                    else { btnLabel = $"Büyük Retcon (-{BalanceCaps.RetconCashCost / 1000.0:F0}K, -{BalanceCaps.RetconIntegrityCost} Tut.)"; btnColor = ColRed; }

                    var lBtn = CreateButton(lCard.transform, btnLabel, btnColor, Color.white, 14, () =>
                    {
                        if (isActive) return;
                        if (currentLaw == CosmologyLaw.None)
                        {
                            UniverseSaveManager.SetLaw(capLaw);
                            _statusBannerMsg = $"Kozmoloji Yasası ilan edildi: {CategoryLabels.LawToTurkish(capLaw)}!";
                        }
                        else
                        {
                            if (comp == null || comp.cash < BalanceCaps.RetconCashCost)
                            {
                                _statusBannerMsg = $"Büyük Retcon için {BalanceCaps.RetconCashCost / 1000.0:F0}K nakit gerekiyor!";
                            }
                            else
                            {
                                comp.adjustCash(-BalanceCaps.RetconCashCost, "Kozmoloji: Büyük Retcon");
                                UniverseSaveManager.AdjustIntegrity(-BalanceCaps.RetconIntegrityCost);
                                UniverseSaveManager.SetLaw(capLaw);
                                UniverseSaveManager.AddRivalNews(comp.currentWeek, comp.name ?? "Sen",
                                    "BÜYÜK RETCON! Evrenin kuralları baştan yazıldı!",
                                    $"Yeni Kozmoloji Yasası: {CategoryLabels.LawToTurkish(capLaw)}. Hayranlar şaşkın (-{BalanceCaps.RetconIntegrityCost} Tutarlılık).", "HABER");
                                _statusBannerMsg = $"Büyük Retcon tamamlandı! Yeni yasa: {CategoryLabels.LawToTurkish(capLaw)}";
                            }
                        }
                        ShowCosmologyView();
                    });
                    PlaceRect(lBtn.GetComponent<RectTransform>(), 12f, 148f, cardW - 24f, 40f);
                }

                bool isSolo = SoloBurnoutSystem.IsSoloDev(comp);
                int burnout = u.SoloBurnout;
                string burnoutState = !isSolo
                    ? "Ekip Modu Aktif (Tükenmişlik riski düşük)"
                    : (burnout >= 70 ? "KRİTİK TÜKENMİŞLİK! (Verimlilik düşüyor)" : (burnout >= 40 ? "Yorgunluk Birikiyor" : "Dinç & Odaklı"));

                var bCard = CreatePanel(_contentRoot, burnout >= 70 && isSolo ? new Color(0.96f, 0.84f, 0.82f, 1f) : ColCard);
                PlaceRect(bCard.GetComponent<RectTransform>(), 0f, 340f, 1084f, 118f);

                var bTxt = CreateText(bCard.transform,
                    $"Solo-Dev Tükenmişlik Dengesi: %{burnout} — {burnoutState}\n" +
                    "Tek başına aralıksız oyun ve transmedya üretmek zamanla tükenmişlik biriktirir. %70'i geçersen oyun içi verimliliğin düşer; mola vererek veya ekip kurarak sıfırlayabilirsin.",
                    15, ColDarkText, TextAnchor.MiddleLeft, false);
                var brt = bTxt.GetComponent<RectTransform>();
                brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
                brt.offsetMin = new Vector2(16f, 8f); brt.offsetMax = new Vector2(-240f, -8f);

                var restBtn = CreateButton(bCard.transform, "Çay Molası Ver\n(-%45 Tükenmişlik)", ColGreen, Color.white, 15, () =>
                {
                    _statusBannerMsg = SoloBurnoutSystem.TakeSoloBreak(comp);
                    ShowCosmologyView();
                });
                var rrt = restBtn.GetComponent<RectTransform>();
                rrt.anchorMin = new Vector2(1f, 0.5f); rrt.anchorMax = new Vector2(1f, 0.5f);
                rrt.pivot = new Vector2(1f, 0.5f);
                rrt.anchoredPosition = new Vector2(-16f, 0f);
                rrt.sizeDelta = new Vector2(210f, 68f);

                if (!string.IsNullOrEmpty(_statusBannerMsg))
                {
                    var msgTxt = CreateText(_contentRoot, _statusBannerMsg, 16, ColPurple, TextAnchor.MiddleLeft, true);
                    PlaceRect(msgTxt.GetComponent<RectTransform>(), 4f, 466f, 1076f, 30f);
                }

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowCosmologyView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // MOD ICERIGI SEKME 3: SIRALAMA (CANLI EVREN LIDERLIK TABLOSU)
        // ---------------------------------------------------------------------
        private static void ShowLeaderboardView()
        {
            try
            {
                ClearContent();
                var comp = UniverseArchitectMod.ActiveCompany;
                var board = RivalSimulationSystem.BuildLeaderboard(comp);

                _headerTitle.text = $"Mod İçeriği — Evren Liderlik Sıralaması ({board.Count} Stüdyo)";
                DrawModContentTabs(3);

                var headCard = CreatePanel(_contentRoot, ColHeader);
                PlaceRect(headCard.GetComponent<RectTransform>(), 0f, 54f, 1084f, 38f);
                var headTxt = CreateText(headCard.transform,
                    "  SIRA   |   STÜDYO & EVREN                                       |   HAYRAN         |   ORT. SKOR   |   TUTARLILIK   |   EVREN GÜCÜ",
                    15, Color.white, TextAnchor.MiddleLeft, true);
                StretchWithPad(headTxt.GetComponent<RectTransform>(), 12f, 0f);

                var scroll = CreateScrollView(_contentRoot, 0f, 98f, 1084f, 398f);
                float innerW = 1068f;
                float yCursor = 0f;

                for (int i = 0; i < board.Count; i++)
                {
                    var e = board[i];
                    var row = CreatePanel(scroll.content, e.IsPlayer ? ColHighlight : ColCard);
                    PlaceRect(row.GetComponent<RectTransform>(), 0f, yCursor, innerW, 64f);

                    string rankBadge = i == 0 ? "#1 ★" : $"#{i + 1}";
                    string line =
                        $"{rankBadge}   {e.StudioName}  —  {e.UniverseName}  [{e.StyleOrLaw}]\n" +
                        $"       Hayran: {e.Fans:N0}   |   Oyun: {e.GamesCount}   |   Ort. Skor: {e.AvgScore:F2}   |   Tutarlılık: {e.Integrity}/100   |   Güç Puanı: {e.PowerScore}";

                    var txt = CreateText(row.transform, line, 16, ColDarkText, TextAnchor.MiddleLeft, e.IsPlayer);
                    StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 4f);
                    yCursor += 72f;
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(382f, yCursor));

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowLeaderboardView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // MOD ICERIGI SEKME 4: RAKIP HABERLERI (SEKTOR GAZETESI)
        // ---------------------------------------------------------------------
        private static void ShowRivalNewsView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                _headerTitle.text = $"Mod İçeriği — Rakip Haberleri & Sektör Bülteni ({u.RivalNews.Count})";

                DrawModContentTabs(4);

                var scroll = CreateScrollView(_contentRoot, 0f, 56f, 1084f, 440f);
                float innerW = 1068f;
                float yCursor = 0f;

                if (u.RivalNews.Count == 0)
                {
                    var empty = CreatePanel(scroll.content, ColCard);
                    PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 110f);
                    var t = CreateText(empty.transform,
                        "Sektör şimdilik sakin. Haftalar ilerledikçe 5 rakip stüdyonun oyunları, hitleri ve rekabet hamleleri burada akacak.",
                        18, ColMutedText, TextAnchor.MiddleCenter, false);
                    StretchWithPad(t.GetComponent<RectTransform>(), 18f, 8f);
                    yCursor = 118f;
                }
                else
                {
                    for (int i = u.RivalNews.Count - 1; i >= 0; i--)
                    {
                        var n = u.RivalNews[i];
                        Color cardBg = ColCard;
                        if (n.Tag == "HIT" || n.Tag == "CROSSOVER") cardBg = ColHighlight;
                        else if (n.Tag == "REKABET") cardBg = new Color(0.95f, 0.86f, 0.96f, 1f);
                        else if (n.Tag == "FIYASKO") cardBg = new Color(0.96f, 0.86f, 0.85f, 1f);

                        var card = CreatePanel(scroll.content, cardBg);
                        PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 74f);

                        var txt = CreateText(card.transform,
                            $"[{n.Tag}]  Hafta {n.Week:F0}  —  {n.Headline}\n{n.Detail}",
                            15, ColDarkText, TextAnchor.MiddleLeft, false);
                        StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 6f);
                        yCursor += 82f;
                    }
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(424f, yCursor));

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 6.1] ShowRivalNewsView hatasi: {ex}");
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

                var ph = CreateText(go.transform, placeholderText, 16, ColMutedText, TextAnchor.MiddleLeft, false);
                StretchWithPad(ph.GetComponent<RectTransform>(), 10f, 2f);

                var tx = CreateText(go.transform, initial ?? "", 17, ColDarkText, TextAnchor.MiddleLeft, true);
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
                MelonLogger.Warning($"[Faz 6.1] InputField kullanilamadi, salt-metin moduna gecildi: {ex.Message}");
                var box = CreatePanel(parent, Color.white);
                fallbackText = CreateText(box.transform, initial ?? "", 17, ColDarkText, TextAnchor.MiddleLeft, true);
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
    // 11. FAZ 2, 3 & 5: EVREN KONUMU + TON + KOZMOLOJI YASASI + KADRO TETIKLEYICI
    // =========================================================================
    public static class UniverseChoicePopupSystem
    {
        private static readonly HashSet<IntPtr> _promptedGames = new HashSet<IntPtr>();
        private static readonly Dictionary<IntPtr, ProjectCategory> _gameCategories = new Dictionary<IntPtr, ProjectCategory>();
        private static readonly Dictionary<IntPtr, List<ActiveCharacterDraft>> _gameCasts = new Dictionary<IntPtr, List<ActiveCharacterDraft>>();
        private static readonly Dictionary<IntPtr, List<PendingBondDraft>> _gamePendingBonds = new Dictionary<IntPtr, List<PendingBondDraft>>();

        public static void ResetSessionCache()
        {
            _promptedGames.Clear();
            _gameCategories.Clear();
            _gameCasts.Clear();
            _gamePendingBonds.Clear();
            NativePopupQueue.Clear();
        }

        public static bool HasCategoryChosen(IntPtr gamePtr) => gamePtr != IntPtr.Zero && _gameCategories.ContainsKey(gamePtr);

        public static ProjectCategory GetCategoryForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero)
            {
                ProjectCategory cat;
                if (_gameCategories.TryGetValue(game.Pointer, out cat)) return cat;
            }
            return UniverseArchitectMod.ActiveProjectCategory;
        }

        public static void SetCastForGame(IntPtr gamePtr, List<ActiveCharacterDraft> cast)
        {
            if (gamePtr != IntPtr.Zero) _gameCasts[gamePtr] = cast ?? new List<ActiveCharacterDraft>();
        }

        public static List<ActiveCharacterDraft> GetCastForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero)
            {
                List<ActiveCharacterDraft> cast;
                if (_gameCasts.TryGetValue(game.Pointer, out cast)) return cast;
            }
            return null;
        }

        public static void SetPendingBondsForGame(IntPtr gamePtr, List<PendingBondDraft> bonds)
        {
            if (gamePtr != IntPtr.Zero) _gamePendingBonds[gamePtr] = bonds ?? new List<PendingBondDraft>();
        }

        public static List<PendingBondDraft> GetPendingBondsForGame(Game game)
        {
            if (game != null && game.Pointer != IntPtr.Zero)
            {
                List<PendingBondDraft> bonds;
                if (_gamePendingBonds.TryGetValue(game.Pointer, out bonds)) return bonds;
            }
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
                $"• Yan Evren: Güvenli spin-off (+%5 Skor, Lore Bariyeri düşer).\n" +
                $"• Alternatif Zaman: 'Ya farklı olsaydı?' (+10 Hype, Bariyer düşer).\n" +
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
                    else if (chosen == ProjectCategory.MainUniverse && UniverseSaveManager.GetLaw() == CosmologyLaw.None)
                    {
                        OpenCosmologySelector(capturedGamePtr, title);
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

                    if (UniverseSaveManager.GetLaw() == CosmologyLaw.None)
                        OpenCosmologySelector(gamePtr, title);
                    else
                        CharacterWorkshopUGUI.OpenCastManager(gamePtr, title);
                }
            );
        }

        private static void OpenCosmologySelector(IntPtr gamePtr, string title)
        {
            NativePopupQueue.Enqueue(
                "Kozmoloji Yasasını Belirle (Faz 5)",
                "Evreninin üst anayasası ne olacak? (İleride 'Mod İçeriği > Kozmoloji' sekmesinden Büyük Retcon ile değiştirebilirsin):\n\n" +
                "• Katı Kanon: Ana Evren & devam oyunları ekstra +%3 Skor; ama kötü oyunlar 2x Lore Hasarı verir.\n" +
                "• Esnek Çoklu Evren: Yan Evren/Alternatif Zaman cezası yarıya iner, Boyut Geçidi Crossover daha güvenli.\n" +
                "• Efsane Karakter Çağı: Karakterler 4 oyun üst üste yorulmaz, Transmedya güçlenir; ama Fiyasko ağır hasar verir.",
                new[] { "Katı Kanon", "Esnek Çoklu Evren", "Efsane Karakter Çağı" },
                opt =>
                {
                    CosmologyLaw law = CosmologyLaw.StrictCanon;
                    if (opt == 1) law = CosmologyLaw.FlexibleMultiverse;
                    else if (opt == 2) law = CosmologyLaw.CharacterLegendEra;

                    UniverseSaveManager.SetLaw(law);
                    MelonLogger.Msg($"[Faz 5] Kozmoloji Yasasi belirlendi: {CategoryLabels.LawToTurkish(law)}");
                    CharacterWorkshopUGUI.OpenCastManager(gamePtr, title);
                }
            );
        }
    }

    // =========================================================================
    // 12. 10X ARASTIRMA PUANI TURBO SISTEMI
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
                InteractiveEventSystem.ResetSessionCache();
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
                    "Universe Architect v6.1.0 Hazır!",
                    "İlişki Ağı & Miras Devri, Karakter Arkı (İhanet/Kefaret/Evrim), Final Fedası (Ölüm/Emeklilik/Diriltme), Özlem Bonusu ve Kült Spin-Off talepleri aktif!"
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
    // 13. ANA MOD GIRISI + FAZ 1-5 DONGULERI
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
        private float _worldSimTimer = 0f;

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
            LoggerInstance.Msg("Universe Architect v6.1.0 (Faz 6.1: Iliski Agi & Miras Devri + Karakter Arki + Kader/Olum/Diriltme + Ozlem & Spin-Off) baslatiliyor...");

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v610");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v6.1.0 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
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
                if (!TopicIconInjector.IconsInjected)
                {
                    try { TopicIconInjector.TryInjectIcons(); } catch { }
                }
                try { CharacterWorkshopUGUI.EnsureHUD(); } catch { }
                try { UniverseChoicePopupSystem.PollForNewGame(ActiveCompany); } catch { }
            }

            _worldSimTimer += dt;
            if (_worldSimTimer >= 1.5f)
            {
                _worldSimTimer = 0f;
                try { InteractiveEventSystem.PollDuringDevelopment(ActiveCompany); } catch { }
                try { TransmediaSystem.Tick(ActiveCompany); } catch { }
                try { RivalSimulationSystem.Tick(ActiveCompany); } catch { }
                try { SoloBurnoutSystem.TickPassiveRecovery(ActiveCompany); } catch { }
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

        public static int ApplyCappedModHype(int rawModHype, bool isCrossover)
        {
            int cap = isCrossover ? BalanceCaps.MaxCrossoverModHype : BalanceCaps.MaxTotalModHype;
            return Math.Min(rawModHype, cap);
        }
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
    // 14. INCELEME: FAZ 1-6 TUM MEKANIKLER (4 EKSEN + OZEL/NPC + EKIPLER + DAMGALAR)
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
                CosmologyLaw law = UniverseSaveManager.GetLaw();
                var cast = UniverseChoicePopupSystem.GetCastForGame(game);
                var eventOutcome = InteractiveEventSystem.GetOutcomeForGame(game);
                bool wasCrossover = eventOutcome != null && eventOutcome.IsCrossover;

                int totalModHype = 0;
                int hypePenalty = 0;
                double totalScoreMult = 1.0;
                bool hasCanonAnchor = false;
                var activeBonusNotes = new List<string>();
                var usedModFeatures = new List<string>();
                var castSummary = new List<string>();

                // A) Post-Credits & Transmedya Miraslari
                if (UniverseSaveManager.Current.PendingPostCreditsHype > 0)
                {
                    int carried = UniverseSaveManager.Current.PendingPostCreditsHype;
                    totalModHype += carried;
                    UniverseSaveManager.Current.PendingPostCreditsHype = 0;
                    activeBonusNotes.Add($"Jenerik Sonrası Miras (+{carried} Hype)");
                }
                if (category != ProjectCategory.NonUniverse)
                {
                    if (UniverseSaveManager.Current.PendingTransmediaHype > 0)
                    {
                        int tmHype = UniverseSaveManager.Current.PendingTransmediaHype;
                        totalModHype += tmHype;
                        UniverseSaveManager.Current.PendingTransmediaHype = 0;
                        activeBonusNotes.Add($"Transmedya Rüzgarı (+{tmHype} Hype)");
                    }
                    if (UniverseSaveManager.Current.PendingTransmediaScoreBonus > 0.001)
                    {
                        double tmScore = UniverseSaveManager.Current.PendingTransmediaScoreBonus;
                        totalScoreMult *= (1.0 + tmScore);
                        UniverseSaveManager.Current.PendingTransmediaScoreBonus = 0.0;
                        activeBonusNotes.Add($"Transmedya Derinliği (+%{tmScore * 100:F0} Skor)");
                    }
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
                var featureIds = GameFeatureReader.ExtractAllFeatureIds(game);
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

                // D) FAZ 4: SECIMLI MOTOR OLAYLARI SONUCLARI
                if (eventOutcome != null)
                {
                    if (Math.Abs(eventOutcome.ScoreDelta) > 0.0001)
                        totalScoreMult *= (1.0 + Math.Clamp(eventOutcome.ScoreDelta, -0.10, 0.12));
                    if (eventOutcome.HypeDelta > 0) totalModHype += eventOutcome.HypeDelta;
                    else if (eventOutcome.HypeDelta < 0) hypePenalty += -eventOutcome.HypeDelta;
                    if (eventOutcome.IntegrityDelta != 0) UniverseSaveManager.AdjustIntegrity(eventOutcome.IntegrityDelta);
                    if (eventOutcome.BarrierDelta != 0) UniverseSaveManager.AdjustLoreBarrier(eventOutcome.BarrierDelta);

                    for (int n = 0; n < eventOutcome.Notes.Count; n++)
                        activeBonusNotes.Add(eventOutcome.Notes[n]);
                }

                // E) FAZ 2, 3 & 5: EVREN KONUMU + KOZMOLOJI YASASI ETKILERI
                int integrity = UniverseSaveManager.Current.CanonIntegrity;
                if (category == ProjectCategory.MainUniverse)
                {
                    UniverseSaveManager.Current.ConsecutiveSideProjects = 0;

                    double canonBonus = integrity >= 80 ? 0.04 : (integrity >= 50 ? 0.02 : -0.04);
                    if (law == CosmologyLaw.StrictCanon && integrity >= 80) canonBonus += 0.03;
                    else if (law == CosmologyLaw.FlexibleMultiverse && canonBonus > 0) canonBonus = Math.Max(0.01, canonBonus - 0.02);

                    totalScoreMult *= (1.0 + canonBonus);
                    if (integrity >= 80) totalModHype += 5;
                    else if (integrity >= 50) totalModHype += 2;
                    activeBonusNotes.Add($"Ana Evren [Tutarlılık {integrity}/100] ({(canonBonus >= 0 ? "+" : "")}%{canonBonus * 100:F0} Skor)");

                    if (!string.IsNullOrEmpty(game.sequelTo))
                    {
                        double seqBonus = law == CosmologyLaw.StrictCanon ? 0.05 : 0.03;
                        totalScoreMult *= (1.0 + seqBonus);
                        activeBonusNotes.Add($"Ana Evren Devam Oyunu (+%{seqBonus * 100:F0} Skor)");
                    }

                    if (UniverseSaveManager.Current.LoreBarrier >= 60)
                    {
                        double barrierLoss = Math.Min(company.fans * 0.03, 300.0);
                        if (barrierLoss >= 10.0) company.adjustFans(-barrierLoss);
                        activeBonusNotes.Add($"Lore Bariyeri Yüksek ({UniverseSaveManager.Current.LoreBarrier}/100)!");
                    }
                    UniverseSaveManager.AdjustLoreBarrier(+15);
                }
                else if (category == ProjectCategory.SideUniverse || category == ProjectCategory.AltTimeline)
                {
                    UniverseSaveManager.Current.ConsecutiveSideProjects++;
                    int barrierDrop = law == CosmologyLaw.StrictCanon ? -15 : -25;
                    UniverseSaveManager.AdjustLoreBarrier(barrierDrop);

                    if (category == ProjectCategory.SideUniverse)
                    {
                        totalScoreMult *= 1.05;
                        activeBonusNotes.Add($"Yan Evren (+%5 Skor, Bariyer {barrierDrop})");
                    }
                    else
                    {
                        totalModHype += 10;
                        activeBonusNotes.Add($"Alternatif Zaman (+10 Hype, Bariyer {barrierDrop})");
                    }

                    int streak = UniverseSaveManager.Current.ConsecutiveSideProjects;
                    if (streak >= 2)
                    {
                        int waitPenalty = Math.Min(20, (streak - 1) * 4);
                        if (law == CosmologyLaw.FlexibleMultiverse) waitPenalty = Math.Max(1, waitPenalty / 2);
                        hypePenalty += waitPenalty;
                        activeBonusNotes.Add($"Ana Evren Beklentisi! Üst üste {streak}. ara oyun (-{waitPenalty} Hype)");
                    }
                }
                else
                {
                    activeBonusNotes.Add("Evren Dışı (Bağımsız proje)");
                }

                // F) FAZ 6.1: OZEL KARAKTERLER (4 EKSEN + ARK + KADER + OZLEM + BAGLAR + EKIPLER) & NPC
                var pendingBonds = UniverseChoicePopupSystem.GetPendingBondsForGame(game);
                if (cast != null && cast.Count > 0 && category != ProjectCategory.NonUniverse)
                {
                    EvaluateCast(company, title, topicId, genreKey, secondGenreKey, displayGenre, category, law, cast, pendingBonds, castSummary,
                        ref totalScoreMult, ref totalModHype, ref hypePenalty, ref hasCanonAnchor, activeBonusNotes);
                }
                else
                {
                    UniverseSaveManager.RestUnusedCharacters(null);
                }

                // G) FAZ 4 & 5: CROSSOVER FINAL KARARI
                if (wasCrossover && category != ProjectCategory.NonUniverse)
                {
                    double projectedScore = Math.Clamp(game.score * totalScoreMult, 1.0, 10.0);
                    CrossoverMode style = eventOutcome.CrossoverStyle == CrossoverMode.None
                        ? CrossoverMode.BalancedPortal
                        : eventOutcome.CrossoverStyle;

                    UniverseSaveManager.Current.CompletedCrossovers++;

                    if (style == CrossoverMode.HighRiskCanon)
                    {
                        if (projectedScore >= 7.5)
                        {
                            totalScoreMult *= 1.08;
                            totalModHype += 20;
                            double xFans = Math.Max(150.0, Math.Round(company.fans * 0.12));
                            company.adjustFans(xFans);
                            UniverseSaveManager.AdjustIntegrity(+8);
                            UniverseSaveManager.AdjustLoreBarrier(-15);
                            activeBonusNotes.Add($"★ EFSANEVİ CROSSOVER! (+%8 Skor, +20 Hype, +{xFans:F0} Hayran)");
                            NotificationHelper.Show(company, $"EFSANEVİ CROSSOVER ZAFERİ: {title}!",
                                $"{eventOutcome.CrossoverSummary} buluşması oyun tarihine geçti! (+{xFans:F0} Hayran, +8 Tutarlılık, Bariyer -15)", true);
                        }
                        else if (projectedScore < 6.5)
                        {
                            totalScoreMult *= 0.94;
                            hypePenalty += 12;
                            double lostFans = Math.Min(company.fans * 0.08, 600.0);
                            if (lostFans >= 15.0) company.adjustFans(-lostFans);
                            UniverseSaveManager.AdjustIntegrity(-15);
                            activeBonusNotes.Add("CROSSOVER FACİASI! (-%6 Skor, -15 Tutarlılık)");
                            NotificationHelper.Show(company, $"CROSSOVER FACİASI: {title}!",
                                $"{eventOutcome.CrossoverSummary} çarpışması hayranları ikiye böldü! Tutarlılık ağır darbe aldı (-15).", true);
                        }
                        else
                        {
                            totalScoreMult *= 1.03;
                            totalModHype += 10;
                            activeBonusNotes.Add("Crossover Başarılı (+%3 Skor, +10 Hype)");
                        }
                    }
                    else if (style == CrossoverMode.BalancedPortal)
                    {
                        double portalThreshold = law == CosmologyLaw.FlexibleMultiverse ? 6.5 : 7.0;
                        if (projectedScore >= portalThreshold)
                        {
                            totalScoreMult *= 1.05;
                            totalModHype += 14;
                            double xFans = Math.Max(80.0, Math.Round(company.fans * 0.07));
                            company.adjustFans(xFans);
                            UniverseSaveManager.AdjustLoreBarrier(-10);
                            activeBonusNotes.Add($"★ Boyut Geçidi Crossover (+%5 Skor, +14 Hype, +{xFans:F0} Hayran)");
                        }
                        else if (projectedScore < 5.5 && law != CosmologyLaw.FlexibleMultiverse)
                        {
                            UniverseSaveManager.AdjustIntegrity(-6);
                            activeBonusNotes.Add("Boyut Geçidi Pürüzü (-6 Tutarlılık)");
                        }
                        else
                        {
                            totalModHype += 8;
                            activeBonusNotes.Add("Crossover İlgi Gördü (+8 Hype)");
                        }
                    }
                    else
                    {
                        totalScoreMult *= 1.02;
                        totalModHype += 8;
                        activeBonusNotes.Add("Sürpriz Cameo Crossover (+%2 Skor, +8 Hype)");
                    }
                }

                // H) Global Denge Tavanlariyla Skor & Hype Uygula
                if (totalScoreMult != 1.0) game.score = Math.Clamp(game.score * totalScoreMult, 1.0, 10.0);

                int netHype = UniverseArchitectMod.ApplyCappedModHype(totalModHype, wasCrossover) - hypePenalty;
                if (netHype != 0) company.adjustHype(netHype);

                // I) Ana Evren Tutarlilik Guncellemesi (Kanon Capasi kalkani dahil!)
                if (category == ProjectCategory.MainUniverse)
                {
                    if (game.score >= 7.5)
                    {
                        UniverseSaveManager.AdjustIntegrity(+6);
                        activeBonusNotes.Add($"Kanon Güçlendi! (Tutarlılık -> {UniverseSaveManager.Current.CanonIntegrity}/100)");
                    }
                    else if (game.score < 5.5)
                    {
                        int damage = law == CosmologyLaw.StrictCanon ? -22 : -12;
                        if (hasCanonAnchor)
                        {
                            damage = damage / 2;
                            activeBonusNotes.Add("Kanon Çapası hasarı yarıya düşürdü!");
                        }
                        UniverseSaveManager.AdjustIntegrity(damage);
                        double lostFans = Math.Min(company.fans * 0.05, 500.0);
                        if (lostFans >= 10.0) company.adjustFans(-lostFans);
                        activeBonusNotes.Add($"Lore Hasarı! (Tutarlılık -> {UniverseSaveManager.Current.CanonIntegrity}/100)");
                    }
                    else if (hasCanonAnchor)
                    {
                        UniverseSaveManager.AdjustIntegrity(+2);
                        activeBonusNotes.Add("Kanon Çapası (+2 Tutarlılık)");
                    }
                }

                // J) FAZ 5: SOLO-DEV TUKENMISLIK & RAKIP TEPKILERI
                SoloBurnoutSystem.OnWorkCompleted(company, category == ProjectCategory.NonUniverse ? 12 : 20);
                RivalSimulationSystem.OnPlayerGameReleased(company, title, game.score, category, wasCrossover);

                if (activeBonusNotes.Count > 0)
                {
                    NotificationHelper.Show(company, $"[{CategoryLabels.ToTurkish(category)}] {title}", string.Join(" | ", activeBonusNotes));
                }

                UniverseSaveManager.RecordReleasedGame(game, topicId, game.score, category, usedModFeatures, castSummary, wasCrossover);
                CharacterWorkshopUGUI.UpdateHudState();
                MelonLogger.Msg($"[Universe Architect] Kaydedildi: '{title}' | {CategoryLabels.ToTurkish(category)} | Kadro: {castSummary.Count} | Crossover: {wasCrossover} | Skor: {game.score:F2}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Universe Architect] ReviewGame hatasi: {ex.Message}");
            }
        }

        private static void EvaluateCast(
            Company company, string gameTitle, string topicId, string genreKey, string secondGenreKey, string displayGenre,
            ProjectCategory category, CosmologyLaw law, List<ActiveCharacterDraft> cast, List<PendingBondDraft> pendingBonds,
            List<string> castSummary,
            ref double scoreMult, ref int modHype, ref int hypePenalty, ref bool hasCanonAnchor, List<string> notes)
        {
            double ensembleScoreDelta = 0.0;
            var usedIds = new HashSet<string>();
            var presentSpecialIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var presentSpecialNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var presentSpecialRecs = new List<CharacterRecord>();
            var teamCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            int fatigueLimit = law == CosmologyLaw.CharacterLegendEra ? 4 : 3;
            bool sceneStealerActive = false;

            // 1. GECIS: OZEL KARAKTERLER (4 EKSEN + EVRIM + ROL KIRILMASI + OZLEM + SPIN-OFF + KADER)
            for (int cIdx = 0; cIdx < cast.Count; cIdx++)
            {
                var draft = cast[cIdx];
                if (draft.IsNpc) continue;

                var axes = CharacterAxisSystem.NormalizeAxes(draft.Axes);
                var matchedArch = CharacterAxisSystem.DetectClosestArchetype(axes);
                var uTone = UniverseSaveManager.GetTone();

                double roleWeight = draft.Role switch
                {
                    CharacterRole.Protagonist => 1.00,
                    CharacterRole.Villain     => 0.75,
                    CharacterRole.Sidekick    => 0.55,
                    _                         => 0.30
                };

                double chemistry = CharacterAxisSystem.ComputeChemistry(axes, genreKey, secondGenreKey, uTone, draft.Role);

                CharacterRecord rec = draft.IsReturning
                    ? UniverseSaveManager.FindCharacterById(draft.ExistingCharacterId)
                    : null;

                bool isBrandNew = (rec == null);
                if (isBrandNew)
                {
                    string cleanTeam = UniverseSaveManager.RegisterTeam(draft.TeamName);
                    rec = new CharacterRecord
                    {
                        Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                        Name = draft.Name,
                        IsNpc = false,
                        ArchetypeId = matchedArch.Id,
                        ArchetypeName = matchedArch.Name,
                        TraitName = draft.Trait.ToString(),
                        TeamName = cleanTeam,
                        DebutGame = gameTitle,
                        DebutTopicId = topicId ?? "",
                        Axes = (float[])axes.Clone(),
                        Popularity = (int)Math.Round(chemistry * 100.0)
                    };
                    UniverseSaveManager.Current.Characters.Add(rec);
                }

                // A) Eksen Evrimi (Eski karakteri yeni oyunda evrimlestirdiyse)
                if (!isBrandNew && draft.AxesEvolved)
                {
                    rec.Axes = (float[])axes.Clone();
                    rec.ArchetypeId = matchedArch.Id;
                    rec.ArchetypeName = matchedArch.Name;
                    if (!string.IsNullOrWhiteSpace(draft.TeamName))
                        rec.TeamName = UniverseSaveManager.RegisterTeam(draft.TeamName);
                    ensembleScoreDelta += 0.01;
                    notes.Add($"Karakter Evrimi: {rec.Name} (+%1 Skor)");
                }

                // B) Ozlem Bonusu (2+ oyun dinlenen yildizin buyuk geri donusu!)
                var currentFate = CategoryLabels.ParseFate(rec.FateName);
                if (!isBrandNew && currentFate == CharacterFate.Alive && rec.GamesMissed >= 2 &&
                    (rec.Status == "Yıldız" || rec.Status == "Kült" || rec.Popularity >= 75))
                {
                    ensembleScoreDelta += 0.04;
                    modHype += 14;
                    double missFans = Math.Max(45.0, Math.Round(company.fans * 0.06));
                    company.adjustFans(missFans);
                    notes.Add($"🔥 Özlem Bonusu: {rec.Name} geri döndü! (+%4 Skor, +14 Hype)");
                    NotificationHelper.Show(company, $"BÜYÜK GERİ DÖNÜŞ: {rec.Name}!",
                        $"{rec.GamesMissed} oyundur gözükmeyen {rec.Name} sahneye geri dönünce hayranlar çıldırdı! (+14 Hype, +{missFans:F0} Hayran)", true);
                }
                rec.GamesMissed = 0;

                // C) Kult Solo Spin-Off Baskisi karsilandi mi?
                if (!isBrandNew && rec.SpinOffDemandRemaining > 0 && category == ProjectCategory.SideUniverse && draft.Role == CharacterRole.Protagonist)
                {
                    rec.SpinOffDemandRemaining = 0;
                    ensembleScoreDelta += 0.06;
                    modHype += 15;
                    double spFans = Math.Max(70.0, Math.Round(company.fans * 0.08));
                    company.adjustFans(spFans);
                    notes.Add($"🎬 Solo Spin-Off Zaferi: {rec.Name} (+%6 Skor, +15 Hype)");
                    NotificationHelper.Show(company, $"HAYRANLARIN İSTEDİĞİ SOLO SPİN-OFF: {rec.Name}!",
                        $"Kült karakter {rec.Name} kendi Yan Evren oyununda Başrol olunca sosyal medya yıkıldı! (+15 Hype, +{spFans:F0} Hayran)", true);
                }

                // D) Karakter Arki & Rol Kirilmasi (Ihanet / Kefaret)
                if (!isBrandNew)
                {
                    if (draft.Role == CharacterRole.Villain && rec.ProtagonistGames >= 2 && rec.ArcTag != "İhanet Arkı")
                    {
                        rec.ArcTag = "İhanet Arkı";
                        ensembleScoreDelta += 0.04;
                        modHype += 14;
                        notes.Add($"⚡ İhanet Arkı: {rec.Name} karanlığa geçti! (+%4 Skor, +14 Hype)");
                        NotificationHelper.Show(company, $"ŞOK EDİCİ İHANET ARKI: {rec.Name}!",
                            $"{rec.ProtagonistGames} oyundur Başrol olan {rec.Name}, {gameTitle} oyununda Kötü Adam olunca oyuncular neye uğradığını şaşırdı! (+14 Hype)", true);
                    }
                    else if (draft.Role == CharacterRole.Villain && rec.ProtagonistGames == 1 && rec.GamesAppeared == 1)
                    {
                        ensembleScoreDelta -= 0.02;
                        UniverseSaveManager.AdjustIntegrity(-4);
                        notes.Add($"Aceleci Karakter Bozumu: {rec.Name} (-%2 Skor, -4 Tutarlılık)");
                    }
                    else if ((draft.Role == CharacterRole.Protagonist || draft.Role == CharacterRole.Sidekick) && rec.VillainGames >= 2 && rec.ArcTag != "Kefaret Arkı")
                    {
                        rec.ArcTag = "Kefaret Arkı";
                        ensembleScoreDelta += 0.04;
                        modHype += 12;
                        UniverseSaveManager.AdjustIntegrity(+3);
                        notes.Add($"★ Kefaret Arkı: {rec.Name} iyiliğe döndü! (+%4 Skor, +12 Hype, +3 Tut.)");
                        NotificationHelper.Show(company, $"DUYGUSAL KEFARET ARKI: {rec.Name}!",
                            $"Eski Kötü Adam {rec.Name}, {gameTitle} oyununda kefaret ödeyerek hayranların gönlünü fethetti! (+12 Hype, +3 Tutarlılık)", true);
                    }
                    else if ((draft.Role == CharacterRole.Protagonist || draft.Role == CharacterRole.Sidekick) && rec.VillainGames == 1 && rec.GamesAppeared == 1)
                    {
                        ensembleScoreDelta -= 0.02;
                        UniverseSaveManager.AdjustIntegrity(-3);
                        notes.Add($"İnandırıcı Olmayan Kefaret: {rec.Name} (-%2 Skor, -3 Tutarlılık)");
                    }
                }

                // E) Kader Kontrolu: Ucuz Diriltme Tuzagi (Ana Evren) vs Prequel (Yan/Alt Evren)
                if (!isBrandNew && currentFate != CharacterFate.Alive)
                {
                    if (category == ProjectCategory.MainUniverse)
                    {
                        int tutPenalty = currentFate == CharacterFate.Sacrificed ? -14 : -6;
                        UniverseSaveManager.AdjustIntegrity(tutPenalty);
                        ensembleScoreDelta -= 0.03;
                        modHype += 10;
                        rec.FateName = CharacterFate.Alive.ToString();
                        rec.Status = "Diriltildi";
                        notes.Add($"⚠ Ucuz Diriltme: {rec.Name} (+10 Hype, -%3 Skor, {tutPenalty} Tutarlılık)");
                        NotificationHelper.Show(company, $"UCUZ DİRİLTME TEPKİSİ: {rec.Name}!",
                            $"'{rec.FateGame}' oyununda veda eden {rec.Name} Ana Evren'de geri getirilince hayranlar 'Ölümün anlamı kalmadı!' diye tepki gösterdi ({tutPenalty} Tutarlılık)!", true);
                    }
                    else
                    {
                        modHype += 4;
                        notes.Add($"Nostaljik Prequel / Alt. Görünüm: {rec.Name} (+4 Hype)");
                    }
                }

                // F) Damga (Trait) Etkileri
                var trait = CategoryLabels.ParseTrait(rec.TraitName);
                if (trait == CharacterTrait.None && draft.Trait != CharacterTrait.None)
                {
                    trait = draft.Trait;
                    rec.TraitName = trait.ToString();
                }

                if (trait == CharacterTrait.LocationBound && !string.IsNullOrEmpty(topicId) &&
                    string.Equals(rec.DebutTopicId, topicId, StringComparison.OrdinalIgnoreCase) && rec.GamesAppeared >= 1)
                {
                    chemistry = Math.Clamp(chemistry + 0.12, 0.20, 0.99);
                    ensembleScoreDelta += 0.02;
                    notes.Add($"Mekana Bağlı: {rec.Name} kendi mekanında parladı (+%2 Skor)");
                }
                else if (trait == CharacterTrait.MysteriousPast)
                {
                    modHype += 5;
                    if (category == ProjectCategory.MainUniverse) UniverseSaveManager.AdjustLoreBarrier(-5);
                    notes.Add($"Gizemli Geçmiş: {rec.Name} (+5 Teori Hype)");
                }
                else if (trait == CharacterTrait.CanonAnchor)
                {
                    hasCanonAnchor = true;
                }
                else if (trait == CharacterTrait.SceneStealer && (draft.Role == CharacterRole.Sidekick || draft.Role == CharacterRole.Minor))
                {
                    sceneStealerActive = true;
                }

                int chemPct = (int)Math.Round(chemistry * 100.0);

                rec.LastRole = CategoryLabels.RoleToTurkish(draft.Role);
                rec.GamesAppeared++;
                rec.ConsecutiveUses++;
                if (draft.Role == CharacterRole.Protagonist) rec.ProtagonistGames++;
                else if (draft.Role == CharacterRole.Villain) rec.VillainGames++;

                usedIds.Add(rec.Id);
                presentSpecialIds.Add(rec.Id);
                presentSpecialNames.Add(rec.Name);
                presentSpecialRecs.Add(rec);
                castSummary.Add($"{rec.Name} ({rec.LastRole})");

                if (!string.IsNullOrWhiteSpace(rec.TeamName))
                {
                    int tc = 0;
                    teamCounts.TryGetValue(rec.TeamName, out tc);
                    teamCounts[rec.TeamName] = tc + 1;
                }

                // G) Yorgunluk veya Kimya Sonucu
                if (draft.IsReturning && rec.ConsecutiveUses >= fatigueLimit && draft.PlannedFate == CharacterFate.Alive)
                {
                    ensembleScoreDelta -= 0.04 * roleWeight;
                    hypePenalty += (int)Math.Round(5 * roleWeight);
                    rec.Popularity = Math.Clamp(rec.Popularity - 8, 10, 100);
                    notes.Add($"Yorgun Karakter: {rec.Name} ({rec.ConsecutiveUses}. seri)");
                    NotificationHelper.Show(company, $"Karakter Yorgunluğu: {rec.Name}",
                        $"Hayranlar {rec.Name} karakterini üst üste görmekten yoruldu! 2 oyun dinlendirirsen 'Özlem Bonusu' ile patlama yapar.");
                }
                else
                {
                    double starThreshold = (trait == CharacterTrait.SceneStealer && (draft.Role == CharacterRole.Sidekick || draft.Role == CharacterRole.Minor))
                        ? 0.68
                        : 0.78;

                    if (chemistry >= starThreshold)
                    {
                        ensembleScoreDelta += 0.06 * roleWeight;
                        int hypeAdd = (int)Math.Max(2, Math.Round((draft.Role == CharacterRole.Villain ? 10 : 8) * roleWeight));
                        if (trait == CharacterTrait.SceneStealer && draft.Role != CharacterRole.Protagonist) hypeAdd += 4;
                        modHype += hypeAdd;
                        double fanBoost = Math.Max(25.0, Math.Round(company.fans * 0.05 * roleWeight));
                        company.adjustFans(fanBoost);

                        bool becameCultSideRole = (draft.Role == CharacterRole.Minor || draft.Role == CharacterRole.Sidekick) && rec.Status != "Kült";
                        rec.Status = draft.Role == CharacterRole.Minor ? "Kült" : "Yıldız";
                        rec.Popularity = Math.Clamp(Math.Max(rec.Popularity, chemPct) + 5, 10, 100);
                        notes.Add($"{rec.LastRole} {rec.Name} [%{chemPct}] (+{hypeAdd} Hype)");

                        if (draft.Role == CharacterRole.Minor || (becameCultSideRole && trait == CharacterTrait.SceneStealer))
                        {
                            rec.SpinOffDemandRemaining = 2;
                            NotificationHelper.Show(company, $"Kült Patlama & Spin-Off Talebi: {rec.Name}!",
                                $"{gameTitle} oyununda {rec.Name} %{chemPct} uyumla viral oldu! Hayranlar sıradaki 2 oyun içinde {rec.Name} için bir Yan Evren Solo Spin-Off istiyor! (+{fanBoost:F0} Hayran)");
                        }
                        else
                        {
                            NotificationHelper.Show(company, $"Hayranlar {rec.Name} ({rec.LastRole}) Karakterine Bayıldı!",
                                $"{gameTitle} oyunundaki {rec.Name} ({rec.ArchetypeName}) uyumu %{chemPct} ile efsaneleşti! (+{fanBoost:F0} Hayran)");
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
                        {
                            int fiyaskoDmg = law == CosmologyLaw.CharacterLegendEra ? -10 : -5;
                            UniverseSaveManager.AdjustIntegrity(fiyaskoDmg);
                        }
                        notes.Add($"Fiyasko {rec.LastRole}: {rec.Name} [%{chemPct}]");

                        if (draft.Role != CharacterRole.Minor)
                        {
                            NotificationHelper.Show(company, $"Oyuncular {rec.Name} ({rec.LastRole}) Karakterinden Nefret Etti!",
                                $"{gameTitle} ({displayGenre}) için {rec.Name} ({rec.ArchetypeName}) hiç uymadı (%{chemPct}). Eleştirmenler karakterin oyunun havasını bozduğunu yazıyor!");
                        }
                    }
                }

                // H) Planlanan Final Fedasi (Olum) veya Onurlu Emeklilik!
                if (!isBrandNew && currentFate == CharacterFate.Alive && draft.PlannedFate != CharacterFate.Alive && rec.GamesAppeared >= 3)
                {
                    if (draft.PlannedFate == CharacterFate.Sacrificed)
                    {
                        rec.FateName = CharacterFate.Sacrificed.ToString();
                        rec.FateGame = gameTitle;
                        rec.Status = "Ölümsüz Efsane";
                        ensembleScoreDelta += 0.06;
                        modHype += 16;
                        UniverseSaveManager.AdjustIntegrity(+6);
                        double sacFans = Math.Max(90.0, Math.Round(company.fans * 0.10));
                        company.adjustFans(sacFans);
                        notes.Add($"☠ FİNAL FEDASI: {rec.Name} ölümsüzleşti! (+%6 Skor, +16 Hype, +6 Tut.)");
                        NotificationHelper.Show(company, $"UNUTULMAZ FİNAL FEDASI: {rec.Name}!",
                            $"{rec.Name}, {gameTitle} finalinde kendini feda ederek Efsane Müzesi'ne girdi! Hayranlar gözyaşları içinde alkışlıyor (+{sacFans:F0} Hayran, +6 Tutarlılık).", true);
                    }
                    else if (draft.PlannedFate == CharacterFate.Retired)
                    {
                        rec.FateName = CharacterFate.Retired.ToString();
                        rec.FateGame = gameTitle;
                        rec.Status = "Emekli Efsane";
                        ensembleScoreDelta += 0.03;
                        modHype += 10;
                        UniverseSaveManager.AdjustIntegrity(+4);
                        double retFans = Math.Max(50.0, Math.Round(company.fans * 0.05));
                        company.adjustFans(retFans);
                        notes.Add($"★ ONURLU EMEKLİLİK: {rec.Name} (+%3 Skor, +10 Hype, +4 Tut.)");
                        NotificationHelper.Show(company, $"ONURLU VEDA: {rec.Name} Emekli Oldu!",
                            $"{rec.Name}, {gameTitle} ile efsanevi kariyerine yakışır bir veda yaptı (+{retFans:F0} Hayran, +4 Tutarlılık).");
                    }
                }
            }

            if (sceneStealerActive) ensembleScoreDelta -= 0.01;

            // 1.5 GECIS (FAZ 6.1): ILISKI AGI & MIRAS DEVRI
            if (pendingBonds != null && pendingBonds.Count > 0)
            {
                for (int bIdx = 0; bIdx < pendingBonds.Count; bIdx++)
                {
                    var pb = pendingBonds[bIdx];
                    CharacterRecord recA = FindSpecialInList(presentSpecialRecs, pb.CharAId, pb.CharAName);
                    CharacterRecord recB = FindSpecialInList(presentSpecialRecs, pb.CharBId, pb.CharBName);
                    if (recA != null && recB != null && recA.Id != recB.Id)
                        UniverseSaveManager.UpsertBond(recA.Id, recA.Name, recB.Id, recB.Name, pb.BondType);
                }
            }

            for (int i = 0; i < presentSpecialRecs.Count; i++)
            {
                for (int j = i + 1; j < presentSpecialRecs.Count; j++)
                {
                    var rA = presentSpecialRecs[i];
                    var rB = presentSpecialRecs[j];
                    var bond = UniverseSaveManager.FindBond(rA.Id, rB.Id);
                    if (bond == null) continue;

                    bond.GamesTogether++;
                    bond.Level = Math.Clamp(1 + (bond.GamesTogether / 2), 1, 3);
                    var bType = CategoryLabels.ParseBond(bond.BondTypeName);
                    double bScore = 0.01 + bond.Level * 0.01; // +2% / +3% / +4%
                    ensembleScoreDelta += bScore;

                    if (bType == CharacterBondType.ArchRivalry)
                    {
                        int bHype = bond.Level == 3 ? 12 : (bond.Level == 2 ? 8 : 5);
                        modHype += bHype;
                        notes.Add($"⚔ Ezeli Rekabet Sv.{bond.Level} ({rA.Name} × {rB.Name}): +%{bScore * 100:F0} Skor, +{bHype} Hype");
                    }
                    else if (bType == CharacterBondType.Comrades)
                    {
                        int bHype = bond.Level == 3 ? 9 : (bond.Level == 2 ? 6 : 4);
                        modHype += bHype;
                        double cFans = Math.Max(20.0, Math.Round(company.fans * 0.02 * bond.Level));
                        company.adjustFans(cFans);
                        notes.Add($"🤝 Silah Arkadaşı Sv.{bond.Level} ({rA.Name} & {rB.Name}): +%{bScore * 100:F0} Skor, +{bHype} Hype");
                    }
                    else // MentorStudent -> Miras Devri!
                    {
                        int bHype = 4 + bond.Level * 2;
                        modHype += bHype;
                        notes.Add($"🎓 Usta–Çırak Sv.{bond.Level} ({rA.Name} → {rB.Name}): +%{bScore * 100:F0} Skor, +{bHype} Hype");

                        CharacterRecord mentor = null, rookie = null;
                        if (rA.GamesAppeared >= 2 && rA.Popularity >= 65 && rB.GamesAppeared == 1) { mentor = rA; rookie = rB; }
                        else if (rB.GamesAppeared >= 2 && rB.Popularity >= 65 && rA.GamesAppeared == 1) { mentor = rB; rookie = rA; }

                        if (mentor != null && rookie != null)
                        {
                            int inherited = Math.Clamp(mentor.Popularity / 5, 10, 20);
                            rookie.Popularity = Math.Clamp(rookie.Popularity + inherited, 10, 100);
                            if (rookie.Status == "Normal") rookie.Status = "Sevilen";
                            notes.Add($"★ Miras Devri: {mentor.Name} → {rookie.Name} (+{inherited} Pop)");
                            NotificationHelper.Show(company, $"MİRAS DEVRİ: {mentor.Name} → {rookie.Name}!",
                                $"Kıdemli yıldız {mentor.Name}, yeni çaylak {rookie.Name}'a meşaleyi devretti! {rookie.Name} daha ilk oyununda +{inherited} popülerlik miras aldı.");
                        }
                    }
                }
            }

            // 2. GECIS: EKIP / HIZIP SINERJISI (TAM KADRO & IC SAVAS)
            string fullSquadTeam = null;
            var activeTeams = new List<string>();
            foreach (var kv in teamCounts)
            {
                activeTeams.Add(kv.Key);
                if (kv.Value >= 3 && fullSquadTeam == null) fullSquadTeam = kv.Key;
            }

            if (fullSquadTeam != null)
            {
                ensembleScoreDelta += 0.04;
                modHype += 8;
                double teamFans = Math.Max(40.0, Math.Round(company.fans * 0.05));
                company.adjustFans(teamFans);
                notes.Add($"★ Tam Kadro ({fullSquadTeam}): +%4 Skor, +8 Hype");
                NotificationHelper.Show(company, $"TAM KADRO SİNERJİSİ: {fullSquadTeam}!",
                    $"{fullSquadTeam} ekibinin üyeleri {gameTitle} oyununda bir araya geldi! Hayranlar takım kimyasına bayıldı (+{teamFans:F0} Hayran).");
            }

            if (activeTeams.Count >= 2)
            {
                ensembleScoreDelta += 0.03;
                modHype += 12;
                notes.Add($"⚔ Evren İç Savaşı ({activeTeams[0]} vs {activeTeams[1]}): +%3 Skor, +12 Hype");
                UniverseSaveManager.AddRivalNews(
                    company.currentWeek,
                    company.name ?? "Sen",
                    $"EVREN İÇ SAVAŞI: {activeTeams[0]} vs {activeTeams[1]}!",
                    $"{gameTitle} oyununda iki büyük hizip karşı karşıya geldi ve sosyal medyayı ikiye böldü (+12 Hype).",
                    "CROSSOVER"
                );
            }

            // 3. GECIS: NPC DEGERLENDIRMESI
            for (int cIdx = 0; cIdx < cast.Count; cIdx++)
            {
                var draft = cast[cIdx];
                if (!draft.IsNpc) continue;

                CharacterRecord npcRec = draft.IsReturning
                    ? UniverseSaveManager.FindCharacterById(draft.ExistingCharacterId)
                    : null;

                if (npcRec == null)
                {
                    string resolvedLinkId = draft.LinkedSpecialCharId;
                    if (string.IsNullOrEmpty(resolvedLinkId) && !string.IsNullOrEmpty(draft.LinkedSpecialCharName))
                    {
                        for (int k = 0; k < UniverseSaveManager.Current.Characters.Count; k++)
                        {
                            var ch = UniverseSaveManager.Current.Characters[k];
                            if (!ch.IsNpc && string.Equals(ch.Name, draft.LinkedSpecialCharName, StringComparison.OrdinalIgnoreCase))
                            {
                                resolvedLinkId = ch.Id;
                                break;
                            }
                        }
                    }

                    npcRec = new CharacterRecord
                    {
                        Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                        Name = draft.Name,
                        IsNpc = true,
                        NpcType = string.IsNullOrEmpty(draft.NpcType) ? "Tüccar" : draft.NpcType,
                        LinkedSpecialCharId = resolvedLinkId ?? "",
                        LinkedSpecialCharName = draft.LinkedSpecialCharName ?? "",
                        DebutGame = gameTitle,
                        DebutTopicId = topicId ?? "",
                        Status = "Normal",
                        Popularity = 45
                    };
                    UniverseSaveManager.Current.Characters.Add(npcRec);
                }

                npcRec.GamesAppeared++;
                usedIds.Add(npcRec.Id);
                castSummary.Add($"{npcRec.Name} (NPC)");

                bool hasLink = !string.IsNullOrEmpty(npcRec.LinkedSpecialCharId) || !string.IsNullOrEmpty(npcRec.LinkedSpecialCharName);
                bool linkPresent = false;
                if (hasLink)
                {
                    if (!string.IsNullOrEmpty(npcRec.LinkedSpecialCharId) && presentSpecialIds.Contains(npcRec.LinkedSpecialCharId))
                        linkPresent = true;
                    else if (!string.IsNullOrEmpty(npcRec.LinkedSpecialCharName) && presentSpecialNames.Contains(npcRec.LinkedSpecialCharName))
                        linkPresent = true;
                }

                if (hasLink && !linkPresent)
                {
                    ensembleScoreDelta -= 0.02;
                    hypePenalty += 3;
                    notes.Add($"Bağlamsız NPC: {npcRec.Name} ({npcRec.LinkedSpecialCharName} yok, -%2 Skor, -3 Hype)");
                }

                double roll = 0.5;
                try { roll = company.getRandom(); } catch { }

                if (roll >= 0.90)
                {
                    npcRec.Status = "Kült";
                    npcRec.SpinOffDemandRemaining = 2;
                    npcRec.Popularity = Math.Clamp(npcRec.Popularity + 25, 65, 96);
                    modHype += 5;
                    double fanBoost = Math.Max(20.0, Math.Round(company.fans * 0.03));
                    company.adjustFans(fanBoost);
                    notes.Add($"★ Viral NPC: {npcRec.Name} (+5 Hype)");
                    NotificationHelper.Show(company, $"Sürpriz Viral NPC: {npcRec.Name}!",
                        $"Sadece sıradan bir {npcRec.NpcType} NPC'si olan {npcRec.Name} internette meme olup herkesin sevgilisi haline geldi! Onu Özel Karakter yapıp Yan Evren Spin-Off'u çıkarabilirsin (+{fanBoost:F0} Hayran).");
                }
                else if (roll <= 0.06)
                {
                    npcRec.Status = "Nefret Edilen";
                    npcRec.Popularity = Math.Clamp(npcRec.Popularity - 20, 10, 35);
                    ensembleScoreDelta -= 0.02;
                    hypePenalty += 3;
                    notes.Add($"Nefret Edilen NPC: {npcRec.Name} (-%2 Skor)");
                    NotificationHelper.Show(company, $"Sinir Bozucu NPC: {npcRec.Name}",
                        $"Oyuncular {gameTitle} içindeki {npcRec.NpcType} NPC'si {npcRec.Name}'in sürekli ayağa dolanmasından nefret etti!");
                }
            }

            ensembleScoreDelta = Math.Clamp(ensembleScoreDelta, -0.10, 0.16);
            scoreMult *= (1.0 + ensembleScoreDelta);

            UniverseSaveManager.RestUnusedCharacters(usedIds);
        }

        private static CharacterRecord FindSpecialInList(List<CharacterRecord> list, string id, string name)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!string.IsNullOrEmpty(id) && list[i].Id == id) return list[i];
                if (!string.IsNullOrEmpty(name) && string.Equals(list[i].Name, name, StringComparison.OrdinalIgnoreCase)) return list[i];
            }
            return null;
        }

        private static string ResolveGenreKey(GameGenreType genreType)
        {
            if (genreType == null) return "";
            var sampleWeights = Phase1Database.Archetypes[0].GenreWeights;
            if (!string.IsNullOrEmpty(genreType.id) && sampleWeights.ContainsKey(genreType.id)) return genreType.id;
            if (!string.IsNullOrEmpty(genreType.name) && sampleWeights.ContainsKey(genreType.name)) return genreType.name;
            return !string.IsNullOrEmpty(genreType.id) ? genreType.id : (genreType.name ?? "");
        }
    }

    // =========================================================================
    // 15. GOOGLE PLAY GAMES OFFLINE YAMALARI (GUVENLIK KILIDI)
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
