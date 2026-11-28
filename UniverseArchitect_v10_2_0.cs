using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppInterop.Runtime;
using Il2CppRarebyte.GDT;

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "10.2.0", "NJG Games")]
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
        DarkMystery,          // 1. Karanlik & Gizem
        EpicAdventure,        // 2. Epik Fantastik
        SciFiTactical,        // 3. Bilim Kurgu & Taktik
        LightComedy,          // 4. Hafif & Mizahi
        CyberpunkDystopia,    // 5. Siberpunk & Distopya
        CosmicHorror,         // 6. Kozmik Korku & Anomali
        MultiverseDimensions, // 7. Coklu Evren & Boyutlar
        PostApocalyptic,      // 8. Kiyamet Sonrasi & Hayatta Kalma
        MythologyLegend,      // 9. Mitoloji & Kadim Efsane
        SuperheroAcademy,     // 10. Super Kahraman & Akademi
        SteampunkClockwork,   // 11. Buhar & Disli (Steampunk)
        SpaceOpera,           // 12. Uzay Operasi & Galaksi
        NoirDetective,        // 13. Dedektif & Noir Sehir
        OccultAcademy,        // 14. Dogaustu Akademi & Okult
        PrehistoricWild       // 15. Tarih Oncesi & Vahsi Cag
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

    public enum StudioCulture
    {
        None,
        ArtisanIndie,   // Sanatci Indie
        HypeMachine,    // Hype Makinesi
        CanonGuardian,  // Kanon Muhafizi
        CommunityFirst  // Topluluk Dostu
    }

    public enum DiplomaticStance
    {
        Hostile,        // Kanli Bicakli (<= -45)
        Tense,          // Gergin (-44 .. -15)
        Neutral,        // Notr (-14 .. +14)
        Friendly,       // Dostane (+15 .. +44)
        Allied          // Muttefik (>= +45)
    }

    public enum SubsidiaryDirective
    {
        SideGames,      // Yan Evren Uret (Gelir + Hayran + Bariyer -10)
        Transmedia,     // Transmedya Destek (+4 Tut. + +6 Hype + Gelir)
        Profit          // Kar Odakli (Yuksek Temettu)
    }

    public enum UniverseRule
    {
        None,
        MysteriousArchive,  // Gizemli Arsiv: Ana Evren Lore Bariyeri -4 yavas birikir
        LethalWorld,        // Olumcul Dunya: Final Fedasi +%2 ekstra Skor & +3 Itibar, Ucuz Diriltme ekstra sert
        TeamSpirit,         // Takim Ruhu: Tam Kadro & Iliski Baglari ekstra +%1 Skor
        ChaosUnleashed      // Kaos Serbest: Alternatif Zaman +4 Hype, Crossover riski biraz duser
    }

    public enum SectorEventType
    {
        None,
        Recession,          // Ekonomik Durgunluk: Satin alma -%20 indirim, hafif Hype freni
        GoldenAge,          // Altin Cag: +%15 Merch/Transmedya, +%10 Hayran
        TrendWave           // Tur/Cag Modasi: Secilen evren turune +%3 Skor, +8 Hype
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
                case UniverseTone.DarkMystery:          return "Karanlık & Gizem";
                case UniverseTone.EpicAdventure:        return "Epik Fantastik";
                case UniverseTone.SciFiTactical:        return "Bilim Kurgu & Taktik";
                case UniverseTone.LightComedy:          return "Hafif & Mizahi";
                case UniverseTone.CyberpunkDystopia:    return "Siberpunk & Distopya";
                case UniverseTone.CosmicHorror:         return "Kozmik Korku & Anomali";
                case UniverseTone.MultiverseDimensions: return "Çoklu Evren & Boyutlar";
                case UniverseTone.PostApocalyptic:      return "Kıyamet Sonrası";
                case UniverseTone.MythologyLegend:      return "Mitoloji & Efsane";
                case UniverseTone.SuperheroAcademy:     return "Süper Kahraman";
                case UniverseTone.SteampunkClockwork:   return "Buhar & Dişli";
                case UniverseTone.SpaceOpera:           return "Uzay Operası";
                case UniverseTone.NoirDetective:        return "Dedektif & Noir";
                case UniverseTone.OccultAcademy:        return "Doğaüstü & Okült";
                case UniverseTone.PrehistoricWild:      return "Tarih Öncesi & Vahşi";
                default:                                return "Belirlenmedi";
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

        public static string CultureToTurkish(StudioCulture c)
        {
            switch (c)
            {
                case StudioCulture.ArtisanIndie:   return "Sanatçı Indie";
                case StudioCulture.HypeMachine:    return "Hype Makinesi";
                case StudioCulture.CanonGuardian:  return "Kanon Muhafızı";
                case StudioCulture.CommunityFirst: return "Topluluk Dostu";
                default:                           return "Belirlenmedi";
            }
        }

        public static string CultureShortDesc(StudioCulture c)
        {
            switch (c)
            {
                case StudioCulture.ArtisanIndie:
                    return "Tüm oyunlara +%4 Eleştirmen Skoru & +%3 karakter kimyası; ama her oyun sonrası 8K sanat cilası gideri.";
                case StudioCulture.HypeMachine:
                    return "Her oyuna +10 Hype, Transmedya & Konuk DLC gelirine +%15; ama 6.0 altı skorda ağır linç (-8 İtibar, -10 Hype).";
                case StudioCulture.CanonGuardian:
                    return "Tutarlılık hasarı %35 azalır, Ana Evren devam oyunlarına +%2 Skor ve Lore Bariyeri artışı +15 yerine +10 olur.";
                case StudioCulture.CommunityFirst:
                    return "Oyunlardan +%15 hayran akışı, Mod Desteği +6 ekstra Hype, karakter yorgunluk cezası yarıya iner ve İtibar hızlı artar.";
                default:
                    return "Şirketinin çalışma DNA'sını seç! İlk seçim ücretsiz; sonradan değiştirmek 35K tutar ve -4 İtibar kaybettirir.";
            }
        }

        public static string ReputationTier(int rep)
        {
            if (rep >= 85) return "Efsanevi";
            if (rep >= 65) return "Güvenilir";
            if (rep >= 40) return "Gelişen";
            if (rep >= 20) return "Tartışmalı";
            return "Para Göz";
        }

        public static DiplomaticStance RelationToStance(int rel)
        {
            if (rel >= 45) return DiplomaticStance.Allied;
            if (rel >= 15) return DiplomaticStance.Friendly;
            if (rel <= -45) return DiplomaticStance.Hostile;
            if (rel <= -15) return DiplomaticStance.Tense;
            return DiplomaticStance.Neutral;
        }

        public static string StanceToTurkish(DiplomaticStance st)
        {
            switch (st)
            {
                case DiplomaticStance.Allied:   return "★ Müttefik";
                case DiplomaticStance.Friendly: return "Dostane";
                case DiplomaticStance.Tense:    return "Gergin";
                case DiplomaticStance.Hostile:  return "⚔ Kanlı Bıçaklı";
                default:                        return "Nötr";
            }
        }

        public static string DirectiveToTurkish(SubsidiaryDirective d)
        {
            switch (d)
            {
                case SubsidiaryDirective.Transmedia: return "Transmedya Destek";
                case SubsidiaryDirective.Profit:     return "Kâr Odaklı";
                default:                             return "Yan Evren Üret";
            }
        }

        public static SubsidiaryDirective ParseDirective(string name)
        {
            SubsidiaryDirective d;
            if (!string.IsNullOrEmpty(name) && Enum.TryParse<SubsidiaryDirective>(name, out d)) return d;
            return SubsidiaryDirective.SideGames;
        }

        public static string UniverseRuleToTurkish(UniverseRule r)
        {
            switch (r)
            {
                case UniverseRule.MysteriousArchive: return "Gizemli Arşiv";
                case UniverseRule.LethalWorld:       return "Ölümcül Dünya";
                case UniverseRule.TeamSpirit:        return "Takım Ruhu";
                case UniverseRule.ChaosUnleashed:    return "Kaos Serbest";
                default:                             return "Standart Kural";
            }
        }

        public static string UniverseRuleDesc(UniverseRule r)
        {
            switch (r)
            {
                case UniverseRule.MysteriousArchive:
                    return "Derin ama anlaşılır anlatı: Ana Evren Lore Bariyeri her oyunda -4 daha yavaş birikir.";
                case UniverseRule.LethalWorld:
                    return "Ciddi sonuçlar: Final Fedası +%2 ekstra Skor & +3 İtibar verir; ama Ucuz Diriltme ekstra -6 Tutarlılık kırar.";
                case UniverseRule.TeamSpirit:
                    return "Kolektif efsane: Tam Kadro ve İlişki Bağları ekstra +%1 Skor sinerjisi kazanır.";
                case UniverseRule.ChaosUnleashed:
                    return "Sürprizlere açık çoklu evren: Alternatif Zaman +4 ekstra Hype alır, Crossover fiyasko eşiği gevşer.";
                default:
                    return "Özel ekstra kural yok — dengeli klasik evren mimarisi.";
            }
        }

        public static UniverseRule ParseUniverseRule(string name)
        {
            UniverseRule r;
            if (!string.IsNullOrEmpty(name) && Enum.TryParse<UniverseRule>(name, out r)) return r;
            return UniverseRule.None;
        }
    }

    public static class BalanceCaps
    {
        public const double MaxLoreScoreBonus = 0.08;
        public const int MaxLoreBaseHype = 15;
        public const int MaxTotalModHype = 32;
        public const int MaxCrossoverModHype = 44;
        public const double MaxModScoreMult = 1.18;
        public const double MaxCrossoverScoreMult = 1.24;
        public const double MinModScoreMult = 0.80;
        public const double RetconCashCost = 50000.0;
        public const int RetconIntegrityCost = 15;
    }

    public static class UniverseAxisSystem
    {
        public const int AxisCount = 6;

        public static readonly string[] AxisNames = new[]
        {
            "Atmosfer",
            "Dünya Yapısı",
            "Hikaye Ölçeği",
            "Anlatım Temposu",
            "Zaman Çizgisi / Çağ",
            "Kurallar & Absürtlük"
        };

        public static readonly string[] LeftPoles = new[]
        {
            "Karanlık & Kasvetli",
            "Gerçekçi & Bilimsel",
            "Kişisel / Sokak Dramı",
            "Yavaş Gizem & Lore",
            "Dinozorlar / İlk İnsan",
            "Ciddi & Mantıklı"
        };

        public static readonly string[] RightPoles = new[]
        {
            "Aydınlık & Mizahi",
            "Fantastik & Kozmik",
            "Galaktik / Çoklu Evren",
            "Hızlı Aksiyon & Kaos",
            "Uzay / Zaman Ötesi",
            "Kuralsız & Absürt"
        };

        // 12 Farkli Donem iceren cok-noktali Zaman Cizgisi!
        public static readonly string[] EraNames = new[]
        {
            "Dinozorlar Çağı",
            "İlk İnsan & Taş Devri",
            "Antik / Kadim Çağ",
            "Orta Çağ & Krallıklar",
            "Keşifler & Buhar Çağı",
            "1920–50'ler Noir Dönemi",
            "1980'ler Retro & Synth",
            "Günümüz (Modern Çağ)",
            "Yakın Gelecek & Siber",
            "Uzay Çağı & Koloniler",
            "Galaktik İmparatorluk",
            "Zaman Ötesi / Çoklu Çağ"
        };

        public static readonly UniverseTone[] AllPresets = new[]
        {
            UniverseTone.DarkMystery,
            UniverseTone.EpicAdventure,
            UniverseTone.SciFiTactical,
            UniverseTone.LightComedy,
            UniverseTone.CyberpunkDystopia,
            UniverseTone.CosmicHorror,
            UniverseTone.MultiverseDimensions,
            UniverseTone.PostApocalyptic,
            UniverseTone.MythologyLegend,
            UniverseTone.SuperheroAcademy,
            UniverseTone.SteampunkClockwork,
            UniverseTone.SpaceOpera,
            UniverseTone.NoirDetective,
            UniverseTone.OccultAcademy,
            UniverseTone.PrehistoricWild
        };

        public static readonly string[] UniverseNamePool = new[]
        {
            "Tentiforverse", "Aethelgard", "Küller Çağı", "Durak E25 Evreni",
            "Karanlık Arşiv", "Gölge Kuşağı", "Yıldız Çemberi", "Sıfır Protokolü",
            "Kırık Pusula", "Sonsuz Kütüphane", "Enta Boşluğu", "Krom Ufuk"
        };

        public static readonly string[] FandomNamePool = new[]
        {
            "YıldızTaym", "EvrenTaym", "Arşivciler", "Gece Nöbetçileri",
            "BoyutGezerler", "Kanon Bekçileri", "Kozmik Tayfa", "Piksel Klanı",
            "Gölge Okurları", "Kırık Pusula Ekibi"
        };

        public static int GetEraIndex(float eraVal)
        {
            float v = Mathf.Clamp01(eraVal);
            int idx = Mathf.FloorToInt(v * EraNames.Length);
            if (idx >= EraNames.Length) idx = EraNames.Length - 1;
            if (idx < 0) idx = 0;
            return idx;
        }

        public static string GetEraName(float eraVal) => EraNames[GetEraIndex(eraVal)];

        public static float[] NormalizeAxes(float[] axes)
        {
            if (axes != null && axes.Length == AxisCount)
            {
                var r = new float[AxisCount];
                for (int i = 0; i < AxisCount; i++) r[i] = Mathf.Clamp01(axes[i]);
                return r;
            }
            if (axes != null && axes.Length == 4)
            {
                return new[]
                {
                    Mathf.Clamp01(axes[0]), Mathf.Clamp01(axes[1]), Mathf.Clamp01(axes[2]), Mathf.Clamp01(axes[3]),
                    0.62f, 0.35f // Gunumuz + Dengeli Ciddiyet
                };
            }
            return new[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.62f, 0.35f };
        }

        public static float[] PresetForTone(UniverseTone tone)
        {
            // [0]=Atmosfer, [1]=Dunya, [2]=Olcek, [3]=Tempo, [4]=Zaman Cizgisi(0..1), [5]=Absurtluk
            switch (tone)
            {
                case UniverseTone.DarkMystery:          return new[] { 0.16f, 0.45f, 0.38f, 0.25f, 0.62f, 0.22f };
                case UniverseTone.EpicAdventure:        return new[] { 0.62f, 0.82f, 0.72f, 0.62f, 0.28f, 0.30f };
                case UniverseTone.SciFiTactical:        return new[] { 0.35f, 0.22f, 0.68f, 0.32f, 0.78f, 0.18f };
                case UniverseTone.LightComedy:          return new[] { 0.88f, 0.60f, 0.40f, 0.72f, 0.55f, 0.88f };
                case UniverseTone.CyberpunkDystopia:    return new[] { 0.22f, 0.25f, 0.45f, 0.65f, 0.72f, 0.35f };
                case UniverseTone.CosmicHorror:         return new[] { 0.08f, 0.90f, 0.88f, 0.20f, 0.46f, 0.42f };
                case UniverseTone.MultiverseDimensions: return new[] { 0.50f, 0.94f, 0.96f, 0.65f, 0.96f, 0.65f };
                case UniverseTone.PostApocalyptic:      return new[] { 0.18f, 0.20f, 0.42f, 0.55f, 0.70f, 0.25f };
                case UniverseTone.MythologyLegend:      return new[] { 0.45f, 0.88f, 0.78f, 0.52f, 0.20f, 0.28f };
                case UniverseTone.SuperheroAcademy:     return new[] { 0.72f, 0.78f, 0.68f, 0.80f, 0.63f, 0.52f };
                case UniverseTone.SteampunkClockwork:   return new[] { 0.48f, 0.58f, 0.50f, 0.48f, 0.37f, 0.40f };
                case UniverseTone.SpaceOpera:           return new[] { 0.58f, 0.74f, 0.92f, 0.70f, 0.88f, 0.38f };
                case UniverseTone.NoirDetective:        return new[] { 0.15f, 0.14f, 0.20f, 0.28f, 0.46f, 0.15f };
                case UniverseTone.OccultAcademy:        return new[] { 0.32f, 0.84f, 0.48f, 0.45f, 0.55f, 0.38f };
                case UniverseTone.PrehistoricWild:      return new[] { 0.42f, 0.30f, 0.35f, 0.76f, 0.06f, 0.30f };
                default:                                return new[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.62f, 0.35f };
            }
        }

        public static UniverseTone DeriveToneFromAxes(float[] axes)
        {
            var a = NormalizeAxes(axes);
            UniverseTone best = UniverseTone.EpicAdventure;
            float bestDist = float.MaxValue;
            for (int i = 0; i < AllPresets.Length; i++)
            {
                var p = PresetForTone(AllPresets[i]);
                float d = 0f;
                for (int k = 0; k < AxisCount; k++)
                {
                    float diff = a[k] - p[k];
                    d += diff * diff;
                }
                if (d < bestDist) { bestDist = d; best = AllPresets[i]; }
            }
            return best;
        }

        public static string FormatAxisReading(int axisIdx, float val)
        {
            if (axisIdx == 4) return GetEraName(val);
            float v = Mathf.Clamp01(val);
            if (v >= 0.45f && v <= 0.55f) return "Dengeli (%50)";
            if (v < 0.45f) return $"%{Mathf.RoundToInt((1f - v) * 100f)} {LeftPoles[axisIdx]}";
            return $"%{Mathf.RoundToInt(v * 100f)} {RightPoles[axisIdx]}";
        }

        public static string FormatCompactSummary(float[] axes)
        {
            var a = NormalizeAxes(axes);
            string era = GetEraName(a[4]);
            string s0 = a[0] < 0.45f ? $"Karanlık %{(1f - a[0]) * 100f:F0}" : (a[0] > 0.55f ? $"Aydınlık %{a[0] * 100f:F0}" : "Dengeli Ton");
            string s1 = a[1] < 0.45f ? $"Gerçekçi %{(1f - a[1]) * 100f:F0}" : (a[1] > 0.55f ? $"Kozmik %{a[1] * 100f:F0}" : "Melez");
            string s5 = a[5] < 0.42f ? "Ciddi" : (a[5] > 0.60f ? "Absürt" : "Yarı Ciddi");
            return $"{era}  |  {s0}  |  {s1}  |  {s5}";
        }

        public static bool CheckEraTopicSynergy(float eraVal, string topicId, out string label)
        {
            label = "";
            if (string.IsNullOrEmpty(topicId)) return false;
            int era = GetEraIndex(eraVal);
            string id = topicId.ToLowerInvariant();

            if (era == 11 && (id.Contains("5d") || id.Contains("parallel") || id.Contains("time") || id.Contains("dream")))
            { label = $"Çağ Uyumu ({EraNames[era]})"; return true; }
            if ((era == 0 || era == 1) && (id.Contains("dino") || id.Contains("prehist") || id.Contains("evolution") || id.Contains("wild") || id.Contains("morph") || id.Contains("micro")))
            { label = $"Çağ Uyumu ({EraNames[era]})"; return true; }
            if ((era == 2 || era == 3) && (id.Contains("myth") || id.Contains("gladiator") || id.Contains("alchemy") || id.Contains("medieval") || id.Contains("fantasy") || id.Contains("arch") || id.Contains("fairy")))
            { label = $"Çağ Uyumu ({EraNames[era]})"; return true; }
            if (era == 4 && (id.Contains("steam") || id.Contains("rail") || id.Contains("sailor") || id.Contains("sky") || id.Contains("pirate")))
            { label = $"Çağ Uyumu ({EraNames[era]})"; return true; }
            if ((era == 5 || era == 6 || era == 7) && (id.Contains("heist") || id.Contains("music") || id.Contains("radio") || id.Contains("urban") || id.Contains("library") || id.Contains("hero") || id.Contains("detective") || id.Contains("cult")))
            { label = $"Çağ Uyumu ({EraNames[era]})"; return true; }
            if (era == 8 && (id.Contains("ai_") || id.Contains("bio") || id.Contains("cyber") || id.Contains("bunker") || id.Contains("under") || id.Contains("mecha")))
            { label = $"Çağ Uyumu ({EraNames[era]})"; return true; }
            if ((era == 9 || era == 10) && (id.Contains("space") || id.Contains("planet") || id.Contains("star") || id.Contains("galactic") || id.Contains("mecha") || id.Contains("sci")))
            { label = $"Çağ Uyumu ({EraNames[era]})"; return true; }

            return false;
        }
    }

    public static class PopularitySystem
    {
        // 4 Bolgeli Hassas Populerlik Egrisi:
        // 0-30: Zor dusus (0.30x asagi, 0.90x yukari)
        // 31-74: Kolay / akici orta bant (1.00x)
        // 75-89: Zor yildiz tirmanisi (0.38x yukari, 0.95x asagi)
        // 90-100: Neredeyse imkansiz efsane bolgesi (0.10x yukari, max +1 tek adimda)
        public static int ApplyDelta(int currentPop, double rawDelta)
        {
            int cur = Math.Clamp(currentPop, 0, 100);
            if (Math.Abs(rawDelta) < 0.25) return cur;

            double val = cur;
            int steps = (int)Math.Abs(Math.Round(rawDelta * 2.0));
            double sign = rawDelta >= 0.0 ? 1.0 : -1.0;

            for (int i = 0; i < steps; i++)
            {
                double mult;
                if (sign > 0.0)
                {
                    if (val < 31.0) mult = 0.90;
                    else if (val < 75.0) mult = 1.00;
                    else if (val < 90.0) mult = 0.38;
                    else mult = 0.10;
                }
                else
                {
                    if (val <= 30.0) mult = 0.30;
                    else if (val < 75.0) mult = 1.00;
                    else if (val < 90.0) mult = 0.95;
                    else mult = 1.00;
                }
                val = Math.Clamp(val + sign * 0.5 * mult, 0.0, 100.0);
            }

            int next = (int)Math.Round(val);

            // 90+ bolgesinde tek hamlede en fazla +1 (ve sadece cok guclu olaylarda >= +9 ham)
            if (cur >= 90 && next > cur + 1) next = cur + 1;
            if (cur >= 90 && rawDelta >= 9.0 && next == cur && cur < 100) next = cur + 1;

            return Math.Clamp(next, 0, 100);
        }

        public static int ComputeDebutPopularity(double chemistry, double gameScore)
        {
            // Yeni karakterler artik direkt 99 baslamaz — 38..62 orta bandindan yola cikar!
            double chemPart = (chemistry - 0.55) * 26.0;
            double scorePart = (gameScore - 6.5) * 3.5;
            return Math.Clamp((int)Math.Round(48.0 + chemPart + scorePart), 34, 64);
        }

        public static double ComputeGamePerformanceRawDelta(double chemistry, double gameScore, double roleWeight)
        {
            // Hem oyunla karakter uyumu HEM oyunun gercek elestirmen puani populerligi belirler!
            double chemTerm = (chemistry - 0.62) * 24.0;
            double scoreTerm = (gameScore - 6.8) * 3.8;
            double roleScale = 0.55 + 0.45 * roleWeight;
            return Math.Clamp((chemTerm + scoreTerm) * roleScale, -18.0, 15.0);
        }

        public static string ZoneLabel(int pop)
        {
            if (pop >= 90) return "Efsane";
            if (pop >= 75) return "Yıldız";
            if (pop >= 31) return "Yükselen";
            return "Dip";
        }
    }

    public static class BalancePressureSystem
    {
        // Studyo buyudukce oyuncularin ve elestirmenlerin beklentisi artar -> skor citasi yukselir!
        public static double ComputeExpectationPenalty(Company company)
        {
            if (company == null) return 0.0;
            var u = UniverseSaveManager.Current;
            long fans = Math.Max(0, company.fans);
            int lvl = 1;
            try { lvl = Math.Max(1, company.currentLevel); } catch { }
            int rep = u.StudioReputation;
            int games = u.ReleasedGames != null ? u.ReleasedGames.Count : 0;
            int subs = HoldingEndgameSystem.GetPlayerSubsidiaries().Count;

            double fanP = fans > 350 ? Math.Max(0.0, (Math.Log10(fans) - 2.55) * 0.032) : 0.0;
            double lvlP = Math.Max(0, lvl - 1) * 0.015;
            double repP = Math.Max(0, rep - 62) * 0.0007;
            double gameP = Math.Min(0.035, Math.Max(0, games - 3) * 0.003);
            double subP = Math.Min(0.030, subs * 0.007);

            return Math.Clamp(fanP + lvlP + repP + gameP + subP, 0.0, 0.14);
        }

        public static void SynchronizeReviewCards(Game game, double oldScore, double newScore)
        {
            if (game == null || game.reviews == null || game.reviews.Count == 0) return;
            try
            {
                int n = game.reviews.Count;
                var origScores = new int[n];
                double origSum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    origScores[i] = game.reviews[i] != null ? game.reviews[i].score : (int)Math.Round(oldScore);
                    origSum += origScores[i];
                }
                double origAvg = origSum / n;

                // GDT ReviewType.score 1..10 araligindadir; 10.00 alinca tum kartlar 10 gozuksun!
                var newScores = new int[n];
                for (int i = 0; i < n; i++)
                {
                    if (game.reviews[i] == null) continue;
                    int adjusted;
                    if (newScore >= 9.85)
                    {
                        adjusted = 10;
                    }
                    else
                    {
                        double spread = Math.Clamp(origScores[i] - origAvg, -1.0, 1.0);
                        adjusted = (int)Math.Clamp(Math.Round(newScore + spread), 1.0, 10.0);
                    }
                    game.reviews[i].score = adjusted;
                    newScores[i] = adjusted;
                }

                // Ortalama tam otursun diye tek bir karti +-1 ince ayarla (10 disinda)
                if (newScore < 9.85)
                {
                    int targetSum = (int)Math.Clamp(Math.Round(newScore * n), n, n * 10);
                    int curSum = 0;
                    for (int i = 0; i < n; i++) curSum += newScores[i];
                    int safety = 0;
                    while (curSum != targetSum && safety < n * 2)
                    {
                        int idx = safety % n;
                        if (curSum < targetSum && newScores[idx] < 10)
                        {
                            newScores[idx]++;
                            game.reviews[idx].score = newScores[idx];
                            curSum++;
                        }
                        else if (curSum > targetSum && newScores[idx] > 1)
                        {
                            newScores[idx]--;
                            game.reviews[idx].score = newScores[idx];
                            curSum--;
                        }
                        safety++;
                    }
                }

                MelonLogger.Msg($"[Faz 8.0 Skor Senkron] '{game.title ?? "?"}': {oldScore:F2} [{string.Join(",", origScores)}] -> {newScore:F2} [{string.Join(",", newScores)}]");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 8.0 Skor Senkron] ReviewType guncelleme hatasi: {ex.Message}");
            }
        }
    }

    public static class CharacterAxisSystem
    {
        public const int AxisCount = 8;

        public static readonly string[] AxisNames = new[]
        {
            "Çatışma Tarzı",
            "Atmosfer",
            "Sosyal Duruş",
            "Motivasyon",
            "Ahlak Pusulası",
            "Güç / Köken",
            "Karar Tarzı",
            "Ekip Ruhu"
        };

        public static readonly string[] LeftPoles = new[]
        {
            "Taktik & Gizem",
            "Karanlık & Travmatik",
            "Mesafeli & Gizemli",
            "Fedakâr & İdealist",
            "Kurallara Sadık",
            "Sıradan & İnsanî",
            "Duygusal & İçgüdüsel",
            "Yalnız Kurt"
        };

        public static readonly string[] RightPoles = new[]
        {
            "Aksiyon & Kavgacı",
            "Esprili & Enerjik",
            "Samimi & Dışa Dönük",
            "Egoist & Çıkarcı",
            "Kaotik & Kuralsız",
            "Doğaüstü & Kozmik",
            "Soğukkanlı & Mantıkçı",
            "Ekip Lideri"
        };

        public static float[] NormalizeAxes(float[] axes)
        {
            if (axes != null && axes.Length == AxisCount)
            {
                var r = new float[AxisCount];
                for (int i = 0; i < AxisCount; i++) r[i] = Mathf.Clamp01(axes[i]);
                return r;
            }
            if (axes != null && axes.Length == 7)
            {
                return new[]
                {
                    Mathf.Clamp01(axes[0]), Mathf.Clamp01(axes[1]), Mathf.Clamp01(axes[2]), Mathf.Clamp01(axes[3]),
                    Mathf.Clamp01(axes[4]), Mathf.Clamp01(axes[5]), Mathf.Clamp01(axes[6]), 0.50f
                };
            }
            if (axes != null && axes.Length == 4)
            {
                return new[]
                {
                    Mathf.Clamp01(axes[0]), Mathf.Clamp01(axes[1]), Mathf.Clamp01(axes[2]), Mathf.Clamp01(axes[3]),
                    0.50f, 0.50f, 0.50f, 0.50f
                };
            }
            return new[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f };
        }

        public static float[] ConvertLegacySlidersToAxes(float[] s9)
        {
            if (s9 == null || s9.Length != 9) return NormalizeAxes(null);
            return new[]
            {
                Mathf.Clamp01((s9[0] + (1f - s9[2])) * 0.5f),
                Mathf.Clamp01((s9[5] + (1f - s9[3])) * 0.5f),
                Mathf.Clamp01((s9[4] + (1f - s9[8])) * 0.5f),
                Mathf.Clamp01((s9[7] + (1f - s9[6])) * 0.5f),
                0.50f, 0.50f, 0.50f, 0.50f
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
            string s0 = a[0] < 0.45f ? $"Taktik %{(1f - a[0]) * 100f:F0}" : (a[0] > 0.55f ? $"Aksiyon %{a[0] * 100f:F0}" : "Dengeli");
            string s1 = a[1] < 0.45f ? $"Karanlık %{(1f - a[1]) * 100f:F0}" : (a[1] > 0.55f ? $"Mizah %{a[1] * 100f:F0}" : "Orta Ton");
            string s5 = a[5] < 0.45f ? "İnsanî" : (a[5] > 0.55f ? "Kozmik" : "Melez");
            string s7 = a[7] < 0.40f ? "Yalnız Kurt" : (a[7] > 0.60f ? "Ekip Lideri" : "Uyumlu");
            return $"{s0}  |  {s1}  |  {s5}  |  {s7}";
        }

        public static CharacterArchetypeDef DetectClosestArchetype(float[] axes)
        {
            var a = NormalizeAxes(axes);
            CharacterArchetypeDef best = Phase1Database.Archetypes[0];
            float bestDist = float.MaxValue;

            for (int i = 0; i < Phase1Database.Archetypes.Count; i++)
            {
                var cand = Phase1Database.Archetypes[i];
                var pa = NormalizeAxes(cand.PresetAxes);
                float d = 0f;
                for (int k = 0; k < AxisCount; k++)
                {
                    float diff = a[k] - pa[k];
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
            double a0 = a[0], a1 = a[1], a2 = a[2], a3 = a[3], a4 = a[4], a5 = a[5], a6 = a[6], a7 = a[7];
            double distinct = (Math.Abs(a0 - 0.5) + Math.Abs(a1 - 0.5) + Math.Abs(a2 - 0.5) +
                               Math.Abs(a3 - 0.5) + Math.Abs(a4 - 0.5) + Math.Abs(a5 - 0.5) +
                               Math.Abs(a6 - 0.5) + Math.Abs(a7 - 0.5)) / 8.0;

            if (string.Equals(genre, "Action", StringComparison.OrdinalIgnoreCase))
                return 0.46 + 0.34 * a0 + 0.10 * (1.0 - a6) + 0.08 * a5 + 0.14 * distinct;
            if (string.Equals(genre, "Adventure", StringComparison.OrdinalIgnoreCase))
                return 0.55 + 0.18 * (1.0 - Math.Abs(a0 - 0.55)) + 0.12 * (1.0 - a3) + 0.08 * (1.0 - a6) + 0.14 * distinct;
            if (string.Equals(genre, "RPG", StringComparison.OrdinalIgnoreCase))
                return 0.58 + 0.48 * Math.Min(0.45, distinct) + 0.10 * a5 + 0.08 * a7;
            if (string.Equals(genre, "Simulation", StringComparison.OrdinalIgnoreCase))
                return 0.48 + 0.24 * (1.0 - a0) + 0.14 * (1.0 - a5) + 0.12 * a6 + 0.08 * distinct;
            if (string.Equals(genre, "Strategy", StringComparison.OrdinalIgnoreCase))
                return 0.42 + 0.32 * (1.0 - a0) + 0.16 * a6 + 0.10 * a7 + 0.12 * distinct;
            if (string.Equals(genre, "Casual", StringComparison.OrdinalIgnoreCase))
                return 0.40 + 0.30 * a1 + 0.20 * a2 + 0.08 * (1.0 - a3) + 0.08 * distinct;

            return 0.70 + 0.18 * distinct;
        }

        // Organik 8 Eksen <-> Oyun Turu <-> 6 Eksenli Evren kimya hesabi!
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
            else genreFit = 0.74;

            // Evrenin 6 ekseniyle dogrudan uyum!
            var uAx = UniverseSaveManager.GetUniverseAxes();
            double atmosMatch  = 1.0 - Math.Abs(a[1] - uAx[0]); // Atmosfer
            double worldMatch  = 1.0 - Math.Abs(a[5] - uAx[1]); // Guc/Koken <-> Dunya Yapisi
            double tempoMatch  = 1.0 - Math.Abs(a[0] - uAx[3]); // Catisma <-> Tempo
            double absurdMatch = 1.0 - Math.Abs(a[4] - uAx[5]); // Ahlak/Kaos <-> Absurtluk
            double scaleMatch  = 1.0 - Math.Abs(((a[5] + a[7]) * 0.5) - uAx[2]); // Olcek uyumu

            double universeAvg = atmosMatch * 0.30 + worldMatch * 0.26 + tempoMatch * 0.18 + absurdMatch * 0.16 + scaleMatch * 0.10;
            double universeMod = Math.Clamp((universeAvg - 0.62) * 0.34, -0.15, 0.13);

            double roleFlavor = 0.0;
            if (role == CharacterRole.Villain && (a[3] >= 0.55f || a[4] >= 0.60f || a[1] <= 0.38f)) roleFlavor = 0.03;
            else if (role == CharacterRole.Protagonist && (Math.Abs(a[3] - 0.5f) >= 0.20f || a[0] >= 0.55f)) roleFlavor = 0.02;
            else if (role == CharacterRole.Sidekick && (a[2] >= 0.50f || a[7] >= 0.55f)) roleFlavor = 0.02;

            return Math.Clamp(genreFit + universeMod + roleFlavor, 0.20, 0.99);
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
        public string FandomName = "";
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
        public string FandomName { get; set; } = "";
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
        // Faz 7.0: Studyo Diplomasisi (-100..+100) & Motor Lisansi
        public int Relation { get; set; } = 0;
        public double LastDiplomacyWeek { get; set; } = -12.0;
        public bool HasEngineLicense { get; set; } = false;
        public string LicensedEngineName { get; set; } = "";
        public double LicensedEngineTech { get; set; } = 1.0;
        public double NextLicensePayoutWeek { get; set; } = 0.0;
        // Faz 7.1: Alt Studyo, AI Birlesme & Gise Carpismasi
        public bool IsAcquiredByPlayer { get; set; } = false;
        public string SubsidiaryDirectiveName { get; set; } = "SideGames";
        public double NextSubsidiaryTickWeek { get; set; } = 0.0;
        public bool IsAbsorbedByRival { get; set; } = false;
        public string ParentRivalName { get; set; } = "";
        public double LastReleaseWeek { get; set; } = -20.0;
        public int AbsorbedCount { get; set; } = 0;
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
        public double LastAppearedWeek { get; set; } = 0.0;
        public double LastGuestLeaseWeek { get; set; } = -30.0;
        // Faz 9.0: Coklu Evren & IP Sahibi
        public string UniverseId { get; set; } = "uni_primary";
        public string UniverseName { get; set; } = "";
        public bool IsRivalGuestHire { get; set; } = false;
        public string SourceRivalId { get; set; } = "";
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
        public float[] Axes = new float[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f, 0.50f };
        public CharacterFate PlannedFate = CharacterFate.Alive;
        public bool AxesEvolved = false;
        public string UniverseId = "";
        public bool IsRivalGuestHire = false;
        public string SourceRivalId = "";
        public string SourceRivalName = "";
        public string SourceRivalStarId = "";
    }

    public class PublishingDealRecord
    {
        public string Id { get; set; }
        public string RivalId { get; set; }
        public string RivalName { get; set; }
        public string GameTitle { get; set; }
        public double Investment { get; set; }
        public double StartWeek { get; set; }
        public double ReleaseWeek { get; set; }
        public bool IsResolved { get; set; }
        public double FinalScore { get; set; }
        public double Payout { get; set; }
        public string ResultSummary { get; set; } = "";
    }

    public class AwardEntry
    {
        public int Year { get; set; }
        public string UniverseWinner { get; set; } = "";
        public string CharacterWinner { get; set; } = "";
        public bool PlayerWonUniverse { get; set; }
        public bool PlayerWonCharacter { get; set; }
        public string Summary { get; set; } = "";
    }

    public class GameSalesTrackEntry
    {
        public string GameKey { get; set; }
        public double LastSeenSales { get; set; }
    }

    public class GuestLeaseRecord
    {
        public string Id { get; set; }
        public string RivalId { get; set; }
        public string RivalName { get; set; }
        public string RivalGameTitle { get; set; }
        public string CharacterId { get; set; }
        public string CharacterName { get; set; }
        public double UpfrontFee { get; set; }
        public double StartWeek { get; set; }
        public double ResolveWeek { get; set; }
        public bool IsResolved { get; set; }
        public double RivalScore { get; set; }
        public int FansGained { get; set; }
        public string ResultSummary { get; set; } = "";
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
        public bool IsMultiIpCrisis = false;
        public string MultiIpSummary = "";
        public bool IsSagaFinale = false;
        public bool HasTwistGamble = false;
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

    public class OwnedUniverseRecord
    {
        public string Id { get; set; } = "uni_primary";
        public string Name { get; set; } = "Ana Evren";
        public string FandomName { get; set; } = "";
        public string OwnerStudioId { get; set; } = "player";
        public string OwnerStudioName { get; set; } = "Özgün IP";
        public bool IsOriginal { get; set; } = true;
        public bool Configured { get; set; } = false;
        public float[] Axes { get; set; } = new float[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.62f, 0.35f };
        public string ToneName { get; set; } = "None";
        public string RuleName { get; set; } = "None";
        public string LawName { get; set; } = "None";
        public int CanonIntegrity { get; set; } = 100;
        public int LoreBarrier { get; set; } = 0;
        public int ConsecutiveSideProjects { get; set; } = 0;
        public int SagaNumber { get; set; } = 1;
        public int SagaGamesCount { get; set; } = 0;
    }

    public class RivalStarDelta
    {
        public string K { get; set; } // RivalId|Name
        public int P { get; set; }    // Degisen Populerlik
        public bool T { get; set; }   // Oyuncuya transfer edildi mi
    }

    public class RivalStarRecord
    {
        public string Id { get; set; }
        public string RivalId { get; set; }
        public string RivalName { get; set; }
        public string UniverseName { get; set; }
        public string Name { get; set; }
        public string ArchetypeId { get; set; } = "classic_hero";
        public string RoleName { get; set; } = "Protagonist";
        public string TraitName { get; set; } = "None";
        public string TeamName { get; set; } = "";
        public float[] Axes { get; set; } = null;
        public int Popularity { get; set; } = 68;
        public bool TransferredToPlayer { get; set; } = false;
    }

    public class PendingRivalGuestHire
    {
        public string StarId { get; set; }
        public string StarName { get; set; }
        public string RivalId { get; set; }
        public string RivalName { get; set; }
        public string ArchetypeId { get; set; }
        public int Popularity { get; set; }
        public float[] Axes { get; set; }
    }

    public class ChronologyEntry
    {
        public double Week { get; set; }
        public int Year { get; set; }
        public string Category { get; set; } = "EVREN";
        public string Title { get; set; }
        public string Detail { get; set; }
    }

    public class RivalStarSeed
    {
        public string Name;
        public string ArchetypeId;
        public CharacterRole Role;
        public CharacterTrait Trait;
        public string TeamName;
        public int StartPop;
        public float[] CustomAxes;
    }

    public class RivalBondSeed
    {
        public string NameA;
        public string NameB;
        public CharacterBondType Type;
    }

    public class UniverseSaveData
    {
        public string CompanyUid { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string UniverseName { get; set; } = "Ana Evren";
        public string UniverseFandomName { get; set; } = "";
        public string UniverseToneName { get; set; } = "None";
        public string CosmologyLawName { get; set; } = "None";
        public bool UniverseConfigured { get; set; } = false;
        public float[] UniverseAxes { get; set; } = new float[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.62f, 0.35f };
        public string UniverseRuleName { get; set; } = "None";
        public bool Enable10xResearchTurbo { get; set; } = true;
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
        public double LastGuestOfferWeek { get; set; } = -10.0;
        // Faz 7.0: Sirket DNA, Departmanlar & Itibar
        public string StudioCultureName { get; set; } = "None";
        public int StudioReputation { get; set; } = 50;
        public int DeptLoreLevel { get; set; } = 0;
        public int DeptPrLevel { get; set; } = 0;
        public int DeptMerchLevel { get; set; } = 0;
        public double LastDeptTickWeek { get; set; } = 0.0;
        public double LastCultureChangeWeek { get; set; } = -30.0;
        public int LastLoggedOfficeLevel { get; set; } = -1;
        public double TotalMerchIncome { get; set; } = 0.0;
        public double TotalEngineLicenseIncome { get; set; } = 0.0;
        // Faz 7.1: Holding, Yayincilik, Ortak Yapim, Yatirimci, Studio Direct & Oduller
        public double LastPublishOfferWeek { get; set; } = -10.0;
        public double LastAiMergerWeek { get; set; } = 20.0;
        public string ActiveCoProdRivalId { get; set; } = "";
        public string ActiveCoProdRivalName { get; set; } = "";
        public string ActiveCoProdUniverse { get; set; } = "";
        public bool HasInvestor { get; set; } = false;
        public string InvestorName { get; set; } = "";
        public double InvestorPrincipal { get; set; } = 0.0;
        public int InvestorEquityPct { get; set; } = 0;
        public double InvestorBuyoutCost { get; set; } = 0.0;
        public double InvestorTargetScore { get; set; } = 7.5;
        public double InvestorTotalPaid { get; set; } = 0.0;
        public double LastStudioDirectWeek { get; set; } = -50.0;
        public int PendingDirectHype { get; set; } = 0;
        public bool HasActiveDirectPromise { get; set; } = false;
        public int LastAwardYear { get; set; } = 0;
        public int TrophyCount { get; set; } = 0;
        public List<PublishingDealRecord> PublishingDeals { get; set; } = new List<PublishingDealRecord>();
        public List<AwardEntry> AwardsHistory { get; set; } = new List<AwardEntry>();
        public List<GameSalesTrackEntry> SalesTrack { get; set; } = new List<GameSalesTrackEntry>();
        public List<string> Teams { get; set; } = new List<string>();
        public List<CharacterRecord> Characters { get; set; } = new List<CharacterRecord>();
        public List<CharacterBondRecord> Bonds { get; set; } = new List<CharacterBondRecord>();
        public List<GuestLeaseRecord> GuestLeases { get; set; } = new List<GuestLeaseRecord>();
        public List<TransmediaProjectRecord> TransmediaProjects { get; set; } = new List<TransmediaProjectRecord>();
        public List<ReleasedGameRecord> ReleasedGames { get; set; } = new List<ReleasedGameRecord>();
        // Faz 9.0: Coklu IP, Rakip Yildizlari, Tersine DLC, Kronoloji, Sektor Depremleri & Anti-Tekel
        public string ActiveUniverseId { get; set; } = "uni_primary";
        public string SectorEventName { get; set; } = "None";
        public string SectorTrendToneName { get; set; } = "None";
        public double SectorEventEndWeek { get; set; } = 0.0;
        public double NextSectorEventWeek { get; set; } = 24.0;
        public double LastAntiMonopolyWeek { get; set; } = 0.0;
        public double LastFandomEventWeek { get; set; } = -10.0;
        public double LastLeakEventWeek { get; set; } = -12.0;
        public List<OwnedUniverseRecord> OwnedUniverses { get; set; } = new List<OwnedUniverseRecord>();
        public List<RivalStarDelta> RivalStarDeltas { get; set; } = new List<RivalStarDelta>();
        public List<RivalStarRecord> RivalStars { get; set; } = new List<RivalStarRecord>();
        public List<PendingRivalGuestHire> PendingRivalGuests { get; set; } = new List<PendingRivalGuestHire>();
        public List<ChronologyEntry> Chronology { get; set; } = new List<ChronologyEntry>();
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
                Id = "rival_njg", Name = "NJG Games", UniverseName = "Tentiforverse", FandomName = "TentiTaym",
                Tone = UniverseTone.DarkMystery, ArchetypeStyle = "Evren Mimarı",
                BaseSkill = 7.7, Variance = 1.2, ReleaseCadenceWeeks = 20, StartingFans = 180,
                TitlePool = new[] { "Delilik", "Delilik 2: Afor'un Yolu", "Delilik 26: Sessiz Nöbet", "Saek Luyot: İşte İlk Gün", "Beyaz Taş", "Durak E25", "Tömyed Kütüphanesi", "Yıldız Koşusu: Thief Comp.", "Yıldız Koşusu: Lost Memory", "Kyldo'nun Günlüğü" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_entakos", Name = "Entakos Games", UniverseName = "EntaVerse", FandomName = "EntaTaym",
                Tone = UniverseTone.LightComedy, ArchetypeStyle = "Kült Kaos",
                BaseSkill = 8.2, Variance = 1.3, ReleaseCadenceWeeks = 15, StartingFans = 570,
                TitlePool = new[] { "Fenoc: İlk Şafak", "Zıkkım Vadisi", "Maloz İmparatorluğu", "Çonçur'un İntikamı", "Enta: Kırık Boyut", "Fenoc vs Zıkkım", "Maloz & Çonçur: Son Durak", "Enta: Zıkkım Protokolü", "Fenoc'un Dönüşü", "Çonçur: Maloz Savaşları", "Enta Kos:Fenoc Geldi", "Kuriow vs Curio", "LarpTakos: Çonça Operasyonu", "Alzhe&İmer: Neredeyiz Biz?", "Fenta & maykıl: Küriyo Kuşatması" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_megacorp", Name = "Hyperion Interactive", UniverseName = "Hyperion Saga", FandomName = "Hyperion Muhafızları",
                Tone = UniverseTone.EpicAdventure, ArchetypeStyle = "Dev Holding",
                BaseSkill = 7.1, Variance = 1.8, ReleaseCadenceWeeks = 15, StartingFans = 260,
                TitlePool = new[] { "Hyperion: Şafak", "Altın Kılıç IV", "İmparatorluk Çarpışması", "Kraliyet İttifakı", "Sonsuz Cephe", "Efsaneler Ligi", "Valerius'un Yükselişi", "Malakor'un Kuşatması", "Kraliyet Muhafızları" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_pixelforge", Name = "Pixel Forge Indie", UniverseName = "Küller ve Yıldızlar", FandomName = "Kül Ailesi",
                Tone = UniverseTone.EpicAdventure, ArchetypeStyle = "Sanatçı Indie",
                BaseSkill = 8.2, Variance = 0.9, ReleaseCadenceWeeks = 28, StartingFans = 120,
                TitlePool = new[] { "Küllerin Şarkısı", "Fener Bekçisi", "Sessiz Liman", "Yıldız Haritacısı", "Kırık Pusula", "Gölge Bahçesi", "Elara'nın Feneri", "Kül Rüzgârı", "Son Deniz Feneri" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_quantum", Name = "Quantum Core Labs", UniverseName = "Protokol-9", FandomName = "",
                Tone = UniverseTone.SciFiTactical, ArchetypeStyle = "Taktik Deha",
                BaseSkill = 7.5, Variance = 1.1, ReleaseCadenceWeeks = 22, StartingFans = 150,
                TitlePool = new[] { "Protokol-9", "Yörünge Kuşatması", "Siber Çekirdek", "Zaman Kırılması", "Vektör Sıfır", "Koloni Komutanı", "Kuantum Operasyonu", "Kalkan-7", "Sıfır Hata" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_kaos", Name = "Kaos Stüdyo", UniverseName = "Gülmece Çoklu Evreni", FandomName = "Kaos Tayfa",
                Tone = UniverseTone.LightComedy, ArchetypeStyle = "Viral Kaos",
                BaseSkill = 6.7, Variance = 2.1, ReleaseCadenceWeeks = 17, StartingFans = 200,
                TitlePool = new[] { "Çılgın Kurye", "Uzayda Çay Molası", "Kaos Akademisi", "Süper Ördekler", "Yanlış Boyut!", "Tersine Dünya", "Ördek Kaptan Görevde", "Kaos Partisi 3000", "Patlak Roket" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_obsidian", Name = "Obsidian Crown", UniverseName = "Karanlık Taç", FandomName = "Taçsızlar",
                Tone = UniverseTone.MythologyLegend, ArchetypeStyle = "Gotik RPG Devi",
                BaseSkill = 8.0, Variance = 1.0, ReleaseCadenceWeeks = 26, StartingFans = 210,
                TitlePool = new[] { "Kül Tahtı", "Karanlık Yemin", "Unutulan Katedral", "Gölge Şövalyesi", "Kızıl Ayin", "Taçsız Kral", "Katedralin Çanları", "Siyah Kılıç", "Kül ve Kemik" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_sakura", Name = "Sakura Pulse", UniverseName = "Aether Blade", FandomName = "Aether Nakama",
                Tone = UniverseTone.SuperheroAcademy, ArchetypeStyle = "Anime & JRPG Ustası",
                BaseSkill = 7.8, Variance = 1.1, ReleaseCadenceWeeks = 21, StartingFans = 230,
                TitlePool = new[] { "Aether Blade Zero", "Yıldız Akademisi", "Kılıç ve Çiçek", "Kızıl Şimşek", "Ruh Muhafızları", "Son Mezuniyet", "Sakura Turnuvası", "Aether: Uyanış", "Şimşek Kılıcı" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_ironclad", Name = "Ironclad Foundry", UniverseName = "Çelik Cephe", FandomName = "",
                Tone = UniverseTone.PostApocalyptic, ArchetypeStyle = "Strateji & Savaş",
                BaseSkill = 7.3, Variance = 1.2, ReleaseCadenceWeeks = 22, StartingFans = 175,
                TitlePool = new[] { "Çelik Cephe 1999", "Demir Siper", "Kül Fırtınası", "Zırhlı Konvoy", "Son Sığınak", "Kuzey Hattı", "Paslı Topraklar", "Siper Hattı 4", "Kömür ve Barut" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_velvet", Name = "Velvet Nightmare", UniverseName = "Kâbus Arşivi", FandomName = "03:17 Tayfası",
                Tone = UniverseTone.CosmicHorror, ArchetypeStyle = "Psikolojik Korku",
                BaseSkill = 7.6, Variance = 1.4, ReleaseCadenceWeeks = 24, StartingFans = 145,
                TitlePool = new[] { "Gece 03:17", "Boş Koridor", "Fısıldayan Duvar", "Yüzsüz Tanık", "Kayıp Kaset", "Uyurgezer", "Aynadaki Kat", "Kör Pencere", "Sessiz Çığlık" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_nova", Name = "Nova Horizon", UniverseName = "Yıldızlararası Konsey", FandomName = "Nebula Yolcuları",
                Tone = UniverseTone.SpaceOpera, ArchetypeStyle = "Uzay Operası",
                BaseSkill = 7.6, Variance = 1.3, ReleaseCadenceWeeks = 23, StartingFans = 225,
                TitlePool = new[] { "Nebula Sınırı", "Andromeda Elçisi", "Yıldız Gemisi V", "Galaksi Meclisi", "Karanlık Madde", "Süpernova", "Orion Antlaşması", "Sınır Filosu", "Yıldızların Ötesi" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_pebble", Name = "Pebble & Pine", UniverseName = "Yosunlu Kasaba", FandomName = "Yosuncular",
                Tone = UniverseTone.LightComedy, ArchetypeStyle = "Huzurlu Indie",
                BaseSkill = 7.9, Variance = 0.8, ReleaseCadenceWeeks = 27, StartingFans = 135,
                TitlePool = new[] { "Yosunlu Vadi", "Sonbahar Fırını", "Göl Kenarı Günlüğü", "Küçük Postacı", "Çay ve Yağmur", "Kış Kulübesi", "Papatya Şenliği", "Balkabağı Pazarı", "Sıcak Ekmek" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_valkyrie", Name = "Valkyrie Works", UniverseName = "Kuzey Destanı", FandomName = "Runik Klan",
                Tone = UniverseTone.MythologyLegend, ArchetypeStyle = "Mitolojik Aksiyon",
                BaseSkill = 7.7, Variance = 1.2, ReleaseCadenceWeeks = 21, StartingFans = 190,
                TitlePool = new[] { "Buzdan Mızrak", "Fırtına Tanrısı", "Runik Çağ", "Kuzeyin Kılıcı", "Gök Köprüsü", "Ragnarok Gölgesi", "Valkür Yeminleri", "Buzul Tahtı", "Kuzgunun Şarkısı" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_neonjackal", Name = "Neon Jackal", UniverseName = "Siber Gece 2099", FandomName = "Krom Çakallar",
                Tone = UniverseTone.CyberpunkDystopia, ArchetypeStyle = "Siberpunk Noir",
                BaseSkill = 7.4, Variance = 1.5, ReleaseCadenceWeeks = 19, StartingFans = 205,
                TitlePool = new[] { "Neon Çakal", "Krom Yağmuru", "Siber Kaçak", "Sentetik Rüya", "Alt Şehir 99", "Veri Hırsızı", "Devre Kesici", "Kızıl Hologram", "Gece Protokolü" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_clockwork", Name = "Clockwork Guild", UniverseName = "Buhar ve Dişli", FandomName = "",
                Tone = UniverseTone.SteampunkClockwork, ArchetypeStyle = "Steampunk Macera",
                BaseSkill = 7.5, Variance = 1.1, ReleaseCadenceWeeks = 24, StartingFans = 140,
                TitlePool = new[] { "Saatçi Loncası", "Hava Gemisi Kaptanı", "Pirinç Şehir", "Buharlı Ekspres", "Zemberek Savaşları", "Kayıp Çark", "Bulut Tersanesi", "Bakır Pusula", "Büyük Saat Kulesi" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_titan", Name = "Titan Entertainment", UniverseName = "Titan Evreni", FandomName = "Titan Ordusu",
                Tone = UniverseTone.SuperheroAcademy, ArchetypeStyle = "Agresif Mega Holding",
                BaseSkill = 7.0, Variance = 1.9, ReleaseCadenceWeeks = 14, StartingFans = 310,
                TitlePool = new[] { "Titan Birliği", "Süper Cephe 2", "Altın Kalkan", "Mega Çarpışma", "Yıldırım Takımı", "Sonsuz Gişe", "Titan: İç Savaş", "Kalkan Kırıldı", "Kıyamet Takımı" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_lumen", Name = "Lumen Storylab", UniverseName = "Kırık Ayna", FandomName = "",
                Tone = UniverseTone.NoirDetective, ArchetypeStyle = "Ödül Avcısı",
                BaseSkill = 8.3, Variance = 0.8, ReleaseCadenceWeeks = 30, StartingFans = 125,
                TitlePool = new[] { "Yağmurlu Sokak", "Son İtiraf", "Kırık Ayna", "Dedektifin Defteri", "Kayıp Tanık", "Siyah Beyaz Gece", "Üçüncü Dosya", "Sisle Gelen Mektup", "Son Sigara" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_bosphorus", Name = "Bosphorus Interactive", UniverseName = "Kadim Boğaz", FandomName = "Yedi Tepe Gezginleri",
                Tone = UniverseTone.DarkMystery, ArchetypeStyle = "Tarihî Gizem",
                BaseSkill = 7.6, Variance = 1.1, ReleaseCadenceWeeks = 22, StartingFans = 165,
                TitlePool = new[] { "Sarnıçtaki Gölge", "Kadim Harita", "Yedi Tepe Gizemi", "Gece Kervanı", "Sırlı Kubbe", "Fener Alayı", "Kız Kulesi Sırrı", "Kapalıçarşı Gecesi", "Boğazın Fısıltısı" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_turbodrift", Name = "Turbo Drift Co.", UniverseName = "Asfalt Kralları", FandomName = "",
                Tone = UniverseTone.LightComedy, ArchetypeStyle = "Hız & Arcade",
                BaseSkill = 7.0, Variance = 1.6, ReleaseCadenceWeeks = 16, StartingFans = 185,
                TitlePool = new[] { "Nitro Gece", "Asfalt Efsanesi", "Retro Yarış 88", "Viraj Ustası", "Süper Ralli", "Kızıl Egzoz", "Gece Otoyolu", "Turbo Kupası", "Tam Gaz İstanbul" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_gloom", Name = "Gloom & Lantern", UniverseName = "Sisli Fener", FandomName = "",
                Tone = UniverseTone.OccultAcademy, ArchetypeStyle = "Okült & Anomali",
                BaseSkill = 7.7, Variance = 1.2, ReleaseCadenceWeeks = 24, StartingFans = 135,
                TitlePool = new[] { "Sisli Fener", "Okült Kütüphane", "Gölge Akademisi", "Mühürlü Kapı", "Gece Simyacısı", "Ruh Çağrısı", "Mum Işığı Ayini", "Kayıp Grimoire", "Alacakaranlık Dersi" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_starlight", Name = "Starlight Syndicate", UniverseName = "Galaksi Kaçakçıları", FandomName = "",
                Tone = UniverseTone.SpaceOpera, ArchetypeStyle = "Uzay Macerası",
                BaseSkill = 7.3, Variance = 1.4, ReleaseCadenceWeeks = 19, StartingFans = 170,
                TitlePool = new[] { "Yıldız Korsanı", "Kaçak Rota", "Asteroit Hanı", "Kızıl Gezegen Soygunu", "Uzay Kervanı", "Son Sıçrama", "Kara Delik Baskını", "Korsan Filosu", "Nebula Hazinesi" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_midas", Name = "Midas Softworks", UniverseName = "Altın Kasa", FandomName = "",
                Tone = UniverseTone.CyberpunkDystopia, ArchetypeStyle = "Kâr Odaklı Dev",
                BaseSkill = 6.6, Variance = 2.0, ReleaseCadenceWeeks = 15, StartingFans = 250,
                TitlePool = new[] { "Altın Kasa Online", "Mega Pazar", "Elmas Sezonu", "VIP İmparatorluk", "Gişe Makinesi", "Kraliyet Paketi", "Borsa Savaşları 2090", "Altın Kartel", "Süper Sezon Pass" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_karpuz", Name = "Karpuz & Kod", UniverseName = "Mahalle Süperleri", FandomName = "",
                Tone = UniverseTone.PrehistoricWild, ArchetypeStyle = "Samimi Indie",
                BaseSkill = 7.5, Variance = 1.3, ReleaseCadenceWeeks = 21, StartingFans = 130,
                TitlePool = new[] { "Dinozor Bakkalı", "Taş Devri Kuryesi", "Mahalle Kahramanı", "Mamut Yarışı", "Vahşi Ada", "Ateş Yakanlar", "Mağara Komşuları", "Süper Çakmaktaş", "Karpuz Festivali" }
            },
            new RivalStudioTemplate
            {
                Id = "rival_aetherium", Name = "Aetherium Collective", UniverseName = "Sonsuz Döngü", FandomName = "Döngücüler",
                Tone = UniverseTone.MultiverseDimensions, ArchetypeStyle = "Metafizik & Deneysel",
                BaseSkill = 8.0, Variance = 1.1, ReleaseCadenceWeeks = 25, StartingFans = 160,
                TitlePool = new[] { "Sonsuz Döngü", "Paralel Kapı", "Zaman Kırığı", "7. Boyut", "Rüya Mimarları", "Çoklu Evren Sıfır", "Yankı Paradoksu", "Kırık Kronometre", "Son Eşik" }
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
                PresetAxes = new[] { 0.72f, 0.65f, 0.72f, 0.18f, 0.22f, 0.55f, 0.35f, 0.85f }
            },
            new CharacterArchetypeDef
            {
                Id = "dark_antihero", Name = "Karanlık Anti-Kahraman", ShortDesc = "Sert, yaralı, gri ahlak",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 0.9 }, { "RPG", 1.0 }, { "Simulation", 0.5 }, { "Strategy", 0.8 }, { "Casual", 0.4 } },
                PresetAxes = new[] { 0.78f, 0.15f, 0.25f, 0.62f, 0.74f, 0.45f, 0.58f, 0.18f }
            },
            new CharacterArchetypeDef
            {
                Id = "mystery_strategist", Name = "Gizemli Stratejist", ShortDesc = "Zeki, soğukkanlı, hesapçı",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.6 }, { "Adventure", 0.9 }, { "RPG", 0.9 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.5 } },
                PresetAxes = new[] { 0.18f, 0.35f, 0.28f, 0.58f, 0.42f, 0.30f, 0.92f, 0.65f }
            },
            new CharacterArchetypeDef
            {
                Id = "cheerful_mascot", Name = "Neşeli Maskot", ShortDesc = "Enerjik, esprili, sıcak",
                PreferredTone = UniverseTone.LightComedy, ClashingTone = UniverseTone.DarkMystery,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.7 }, { "Adventure", 0.8 }, { "RPG", 0.6 }, { "Simulation", 0.8 }, { "Strategy", 0.4 }, { "Casual", 1.0 } },
                PresetAxes = new[] { 0.35f, 0.92f, 0.85f, 0.28f, 0.55f, 0.62f, 0.20f, 0.78f }
            },
            new CharacterArchetypeDef
            {
                Id = "wise_mentor", Name = "Bilge Mentor", ShortDesc = "Yol gösterici, derin, bilge",
                PreferredTone = UniverseTone.EpicAdventure, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.9 }, { "Casual", 0.6 } },
                PresetAxes = new[] { 0.22f, 0.45f, 0.65f, 0.15f, 0.20f, 0.68f, 0.82f, 0.72f }
            },
            new CharacterArchetypeDef
            {
                Id = "charismatic_villain", Name = "Karizmatik Kötü", ShortDesc = "İddialı, bencil, akılda kalıcı",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.9 }, { "Adventure", 0.9 }, { "RPG", 1.0 }, { "Simulation", 0.5 }, { "Strategy", 1.0 }, { "Casual", 0.4 } },
                PresetAxes = new[] { 0.58f, 0.32f, 0.48f, 0.92f, 0.72f, 0.75f, 0.78f, 0.58f }
            },
            new CharacterArchetypeDef
            {
                Id = "rebel_outlaw", Name = "Asi Kaçak", ShortDesc = "Kuralsız, özgür ruhlu",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.None,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.6 }, { "Strategy", 0.7 }, { "Casual", 0.7 } },
                PresetAxes = new[] { 0.75f, 0.58f, 0.45f, 0.68f, 0.88f, 0.32f, 0.30f, 0.25f }
            },
            new CharacterArchetypeDef
            {
                Id = "tragic_survivor", Name = "Trajik Hayatta Kalan", ShortDesc = "Dram yüklü, fedakâr",
                PreferredTone = UniverseTone.DarkMystery, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.6 }, { "Casual", 0.4 } },
                PresetAxes = new[] { 0.42f, 0.12f, 0.48f, 0.22f, 0.38f, 0.20f, 0.25f, 0.40f }
            },
            new CharacterArchetypeDef
            {
                Id = "cold_hunter", Name = "Soğukkanlı Avcı", ShortDesc = "Az konuşan, yüksek aksiyon",
                PreferredTone = UniverseTone.SciFiTactical, ClashingTone = UniverseTone.LightComedy,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.8 }, { "Simulation", 0.6 }, { "Strategy", 0.9 }, { "Casual", 0.5 } },
                PresetAxes = new[] { 0.88f, 0.22f, 0.15f, 0.48f, 0.55f, 0.40f, 0.85f, 0.12f }
            },
            new CharacterArchetypeDef
            {
                Id = "chaotic_trickster", Name = "Kaotik Düzenbaz", ShortDesc = "Sürprizli, kurnaz, esprili",
                PreferredTone = UniverseTone.LightComedy, ClashingTone = UniverseTone.None,
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.6 }, { "Strategy", 0.8 }, { "Casual", 0.9 } },
                PresetAxes = new[] { 0.40f, 0.85f, 0.52f, 0.80f, 0.94f, 0.78f, 0.42f, 0.32f }
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
            // --- ORIJINAL 6 NJG KONUSU ---
            new CustomTopicDef
            {
                Id = "njg_library_defense", Name = "Kütüphane & Gece Savunması", IconTemplateKeyword = "Horror",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.8 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.9, 0.7, 1.0 }, ScoreBonusMultiplier = 1.04, BonusHype = 4, BonusLabel = "Gündüz/Gece Döngüsü (+%4 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_5d_shift", Name = "5D Boyut Kayması", IconTemplateKeyword = "Sci-Fi",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 0.8 }, { "Simulation", 0.8 }, { "Strategy", 0.9 }, { "Casual", 0.9 } },
                AudienceWeights = new[] { 0.6, 0.7, 1.0 }, ScoreBonusMultiplier = 1.05, BonusHype = 2, BonusLabel = "Eleştirmen Sanatı (+%5 Skor, +2 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_planet_idle", Name = "Gezegen Kolonizasyonu (Idle)", IconTemplateKeyword = "Space",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.6 }, { "Adventure", 0.7 }, { "RPG", 0.7 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.9 } },
                AudienceWeights = new[] { 0.8, 1.0, 0.8 }, ScoreBonusMultiplier = 1.03, BonusHype = 4, BonusLabel = "Uzun Soluklu Koloni (+%3 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_morph_animals", Name = "Şekil Değiştiren Hayvanlar", IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 1.0, 1.0, 0.7 }, ScoreBonusMultiplier = 1.02, BonusHype = 6, BonusLabel = "Karakter Popülaritesi (+%2 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_star_run", Name = "Yıldız Koşusu", IconTemplateKeyword = "Racing",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 0.8 }, { "Simulation", 0.7 }, { "Strategy", 0.7 }, { "Casual", 1.0 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }, ScoreBonusMultiplier = 1.03, BonusHype = 5, BonusLabel = "Hızlı Tempo (+%3 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_sailor_adventure", Name = "Denizci Macerası", IconTemplateKeyword = "Pirate",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }, ScoreBonusMultiplier = 1.04, BonusHype = 4, BonusLabel = "Açık Deniz Tasarımı (+%4 Skor, +4 Hype)"
            },
            // --- FAZ 8B: 30 YENI OZEL KONU ---
            new CustomTopicDef
            {
                Id = "njg_parallel_universes", Name = "Paralel Evrenler", IconTemplateKeyword = "Sci-Fi",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.9 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.7, 1.0, 1.0 }, ScoreBonusMultiplier = 1.04, BonusHype = 6, BonusLabel = "Çoklu Boyut Teorisi (+%4 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_ancient_mythology", Name = "Antik Mitoloji", IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.6 }, { "Strategy", 0.9 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.8, 1.0, 0.9 }, ScoreBonusMultiplier = 1.04, BonusHype = 4, BonusLabel = "Destansı Efsane (+%4 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_secret_cults", Name = "Gizli Tarikatlar & Örgütler", IconTemplateKeyword = "Mystery",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.7 }, { "Strategy", 1.0 }, { "Casual", 0.5 } },
                AudienceWeights = new[] { 0.5, 0.8, 1.0 }, ScoreBonusMultiplier = 1.04, BonusHype = 4, BonusLabel = "Komplo Derinliği (+%4 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_ai_uprising", Name = "Yapay Zekâ İsyanı", IconTemplateKeyword = "Cyberpunk",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 0.9 }, { "Simulation", 0.8 }, { "Strategy", 1.0 }, { "Casual", 0.5 } },
                AudienceWeights = new[] { 0.6, 0.9, 1.0 }, ScoreBonusMultiplier = 1.03, BonusHype = 6, BonusLabel = "Makine Çağı Gerilimi (+%3 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_underground_cities", Name = "Yeraltı Şehirleri", IconTemplateKeyword = "Dungeon",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 1.0 }, { "Strategy", 0.9 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.8, 1.0, 0.9 }, ScoreBonusMultiplier = 1.04, BonusHype = 3, BonusLabel = "Derin Atmosfer (+%4 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_soul_hunter", Name = "Ruh Avcısı & Okült", IconTemplateKeyword = "Vampire",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.5 }, { "Strategy", 0.7 }, { "Casual", 0.5 } },
                AudienceWeights = new[] { 0.6, 0.9, 1.0 }, ScoreBonusMultiplier = 1.03, BonusHype = 5, BonusLabel = "Karanlık Av (+%3 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_hero_academy", Name = "Süper Kahraman Akademisi", IconTemplateKeyword = "Superheroes",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 0.9 }, { "RPG", 1.0 }, { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 1.0, 1.0, 0.8 }, ScoreBonusMultiplier = 1.03, BonusHype = 6, BonusLabel = "Genç Yıldız Kadrosu (+%3 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_dream_architect", Name = "Rüya Mimarlığı", IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.7 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.9 }, { "Strategy", 1.0 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 0.7, 1.0, 1.0 }, ScoreBonusMultiplier = 1.05, BonusHype = 2, BonusLabel = "Sürreal Sanat (+%5 Skor, +2 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_time_loop", Name = "Zaman Döngüsü", IconTemplateKeyword = "Time Travel",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.9 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.9 }, { "Casual", 0.7 } },
                AudienceWeights = new[] { 0.7, 1.0, 1.0 }, ScoreBonusMultiplier = 1.05, BonusHype = 4, BonusLabel = "Kelebek Etkisi (+%5 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_steampunk_era", Name = "Buhar Çağı (Steampunk)", IconTemplateKeyword = "History",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.9 }, { "Strategy", 1.0 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.7, 1.0, 0.9 }, ScoreBonusMultiplier = 1.04, BonusHype = 3, BonusLabel = "Mekanik Estetik (+%4 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_deep_ocean", Name = "Derin Okyanus & Denizaltı", IconTemplateKeyword = "Pirate",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 0.8 }, { "Simulation", 1.0 }, { "Strategy", 0.9 }, { "Casual", 0.7 } },
                AudienceWeights = new[] { 0.8, 1.0, 0.9 }, ScoreBonusMultiplier = 1.04, BonusHype = 3, BonusLabel = "Karanlık Derinlik (+%4 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_mecha_robots", Name = "Meka & Dev Robotlar", IconTemplateKeyword = "Sci-Fi",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 0.7 }, { "RPG", 0.9 }, { "Simulation", 0.9 }, { "Strategy", 1.0 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.9 }, ScoreBonusMultiplier = 1.03, BonusHype = 5, BonusLabel = "Dev Çarpışma (+%3 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_gladiator_arena", Name = "Gladyatör Arenası", IconTemplateKeyword = "Medieval",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 0.7 }, { "RPG", 0.9 }, { "Simulation", 0.9 }, { "Strategy", 1.0 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.7, 0.9, 1.0 }, ScoreBonusMultiplier = 1.03, BonusHype = 4, BonusLabel = "Arena Coşkusu (+%3 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_space_pirates", Name = "Uzay Korsanlığı", IconTemplateKeyword = "Space",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.8 }, { "Strategy", 0.8 }, { "Casual", 0.7 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.9 }, ScoreBonusMultiplier = 1.03, BonusHype = 5, BonusLabel = "Galaktik Yağma (+%3 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_railroad_empire", Name = "Demiryolu İmparatorluğu", IconTemplateKeyword = "Transport",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.5 }, { "Adventure", 0.7 }, { "RPG", 0.6 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 0.7, 1.0, 0.8 }, ScoreBonusMultiplier = 1.04, BonusHype = 2, BonusLabel = "Lojistik Ustalığı (+%4 Skor, +2 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_alchemy_shop", Name = "Simya & İksir Atölyesi", IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.5 }, { "Adventure", 0.8 }, { "RPG", 1.0 }, { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.9 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }, ScoreBonusMultiplier = 1.04, BonusHype = 3, BonusLabel = "Zanaat Bağımlılığı (+%4 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_monster_tamer", Name = "Canavar Eğitmenliği", IconTemplateKeyword = "Evolution",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.9 }, { "Strategy", 0.9 }, { "Casual", 1.0 } },
                AudienceWeights = new[] { 1.0, 1.0, 0.7 }, ScoreBonusMultiplier = 1.03, BonusHype = 6, BonusLabel = "Koleksiyon Çılgınlığı (+%3 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_grand_heist", Name = "Büyük Soygun (Heist)", IconTemplateKeyword = "Detective",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 0.9 }, { "RPG", 0.8 }, { "Simulation", 0.8 }, { "Strategy", 1.0 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.6, 0.9, 1.0 }, ScoreBonusMultiplier = 1.04, BonusHype = 5, BonusLabel = "Kusursuz Plan (+%4 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_sky_islands", Name = "Gökyüzü Adaları & Hava Gemileri", IconTemplateKeyword = "Airplane",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.9 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.9 }, { "Strategy", 0.8 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }, ScoreBonusMultiplier = 1.04, BonusHype = 4, BonusLabel = "Bulut Keşfi (+%4 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_biopunk_mutation", Name = "Biyopunk & Mutasyon", IconTemplateKeyword = "Zombies",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 1.0 }, { "Adventure", 0.8 }, { "RPG", 1.0 }, { "Simulation", 0.8 }, { "Strategy", 0.9 }, { "Casual", 0.4 } },
                AudienceWeights = new[] { 0.5, 0.8, 1.0 }, ScoreBonusMultiplier = 1.03, BonusHype = 4, BonusLabel = "Genetik Evrim (+%3 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_micro_world", Name = "Mikro Dünya (Böcek & Hücre)", IconTemplateKeyword = "Evolution",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.7 }, { "Adventure", 0.9 }, { "RPG", 0.7 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.9 } },
                AudienceWeights = new[] { 1.0, 1.0, 0.7 }, ScoreBonusMultiplier = 1.04, BonusHype = 3, BonusLabel = "Ekosistem Tasarımı (+%4 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_dark_fairytales", Name = "Karanlık Masallar", IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.6 }, { "Strategy", 0.7 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.6, 0.9, 1.0 }, ScoreBonusMultiplier = 1.05, BonusHype = 3, BonusLabel = "Gotik Anlatı (+%5 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_archaeology_ruins", Name = "Arkeoloji & Kayıp Uygarlıklar", IconTemplateKeyword = "History",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.8 } },
                AudienceWeights = new[] { 0.8, 1.0, 0.8 }, ScoreBonusMultiplier = 1.04, BonusHype = 3, BonusLabel = "Kayıp Hazine (+%4 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_apocalypse_bunker", Name = "Kıyamet Sığınağı Yönetimi", IconTemplateKeyword = "Post Apocalyptic",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.7 }, { "Adventure", 0.8 }, { "RPG", 0.9 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.6, 0.9, 1.0 }, ScoreBonusMultiplier = 1.04, BonusHype = 4, BonusLabel = "Hayatta Kalma Dengesi (+%4 Skor, +4 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_urban_legends", Name = "Şehir Efsaneleri & Anomali", IconTemplateKeyword = "Horror",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.9 }, { "Adventure", 1.0 }, { "RPG", 0.9 }, { "Simulation", 0.7 }, { "Strategy", 0.8 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.7, 0.9, 1.0 }, ScoreBonusMultiplier = 1.03, BonusHype = 6, BonusLabel = "Viral Gizem (+%3 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_galactic_diplomacy", Name = "Galaktik Diplomasi", IconTemplateKeyword = "Space",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.5 }, { "Adventure", 0.8 }, { "RPG", 0.9 }, { "Simulation", 1.0 }, { "Strategy", 1.0 }, { "Casual", 0.5 } },
                AudienceWeights = new[] { 0.5, 0.9, 1.0 }, ScoreBonusMultiplier = 1.05, BonusHype = 2, BonusLabel = "Stratejik İttifak (+%5 Skor, +2 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_boardgame_universe", Name = "Kart & Masa Oyunu Evreni", IconTemplateKeyword = "Casual",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.5 }, { "Adventure", 0.7 }, { "RPG", 0.9 }, { "Simulation", 0.9 }, { "Strategy", 1.0 }, { "Casual", 1.0 } },
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }, ScoreBonusMultiplier = 1.03, BonusHype = 5, BonusLabel = "Deste Sinerjisi (+%3 Skor, +5 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_night_radio", Name = "Gece Radyosu & Gözetleme Kulesi", IconTemplateKeyword = "Mystery",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.6 }, { "Adventure", 1.0 }, { "RPG", 0.8 }, { "Simulation", 1.0 }, { "Strategy", 0.8 }, { "Casual", 0.7 } },
                AudienceWeights = new[] { 0.6, 0.9, 1.0 }, ScoreBonusMultiplier = 1.05, BonusHype = 3, BonusLabel = "Yalnız Nöbet Gerilimi (+%5 Skor, +3 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_music_label", Name = "Plak Şirketi & Müzik Festivali", IconTemplateKeyword = "Music",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.5 }, { "Adventure", 0.7 }, { "RPG", 0.7 }, { "Simulation", 1.0 }, { "Strategy", 0.9 }, { "Casual", 1.0 } },
                AudienceWeights = new[] { 1.0, 1.0, 0.8 }, ScoreBonusMultiplier = 1.03, BonusHype = 6, BonusLabel = "Sahne Ateşi (+%3 Skor, +6 Hype)"
            },
            new CustomTopicDef
            {
                Id = "njg_reincarnation_cycle", Name = "Ölümsüzlük & Reenkarnasyon", IconTemplateKeyword = "Fantasy",
                GenreWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { { "Action", 0.8 }, { "Adventure", 1.0 }, { "RPG", 1.0 }, { "Simulation", 0.7 }, { "Strategy", 0.8 }, { "Casual", 0.6 } },
                AudienceWeights = new[] { 0.7, 1.0, 1.0 }, ScoreBonusMultiplier = 1.05, BonusHype = 3, BonusLabel = "Ruh Döngüsü (+%5 Skor, +3 Hype)"
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
        public static string ActiveSaveKey => _activeSaveKey;
        public static readonly JsonSerializerOptions CompactJsonOpts = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

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

        public static StudioCulture GetCulture()
        {
            StudioCulture c;
            if (Enum.TryParse<StudioCulture>(Current.StudioCultureName, out c)) return c;
            return StudioCulture.None;
        }

        public static void SetCulture(StudioCulture culture)
        {
            Current.StudioCultureName = culture.ToString();
            Save();
        }

        public static void AdjustReputation(int delta)
        {
            if (delta < 0 && Current.DeptPrLevel > 0)
            {
                double mult = Math.Max(0.40, 1.0 - Current.DeptPrLevel * 0.18);
                delta = (int)Math.Round(delta * mult);
                if (delta == 0) delta = -1;
            }
            else if (delta > 0 && GetCulture() == StudioCulture.CommunityFirst)
            {
                delta += 1;
            }
            Current.StudioReputation = Math.Clamp(Current.StudioReputation + delta, 0, 100);
        }

        public static void AdjustRivalRelation(RivalStudioRecord rival, int delta)
        {
            if (rival == null) return;
            rival.Relation = Math.Clamp(rival.Relation + delta, -100, 100);
            if (rival.HasEngineLicense && rival.Relation <= -15)
            {
                rival.HasEngineLicense = false;
                rival.LicensedEngineName = "";
            }
        }

        public static RivalStudioRecord FindRivalById(string rivalId)
        {
            if (string.IsNullOrEmpty(rivalId) || Current.Rivals == null) return null;
            for (int i = 0; i < Current.Rivals.Count; i++)
                if (Current.Rivals[i].Id == rivalId) return Current.Rivals[i];
            return null;
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

        public static float[] GetUniverseAxes()
        {
            if (Current.UniverseAxes == null || Current.UniverseAxes.Length != UniverseAxisSystem.AxisCount)
            {
                if (Current.UniverseAxes != null && Current.UniverseAxes.Length == 4)
                    Current.UniverseAxes = UniverseAxisSystem.NormalizeAxes(Current.UniverseAxes);
                else
                    Current.UniverseAxes = UniverseAxisSystem.PresetForTone(GetTone());
            }
            return Current.UniverseAxes;
        }

        public static UniverseRule GetUniverseRule()
        {
            return CategoryLabels.ParseUniverseRule(Current.UniverseRuleName);
        }

        public static OwnedUniverseRecord GetActiveOwnedUniverse()
        {
            EnsureOwnedUniversesReady();
            for (int i = 0; i < Current.OwnedUniverses.Count; i++)
                if (Current.OwnedUniverses[i].Id == Current.ActiveUniverseId) return Current.OwnedUniverses[i];
            Current.ActiveUniverseId = Current.OwnedUniverses[0].Id;
            return Current.OwnedUniverses[0];
        }

        public static OwnedUniverseRecord FindOwnedUniverseById(string uniId)
        {
            EnsureOwnedUniversesReady();
            if (string.IsNullOrEmpty(uniId)) return GetActiveOwnedUniverse();
            for (int i = 0; i < Current.OwnedUniverses.Count; i++)
                if (Current.OwnedUniverses[i].Id == uniId) return Current.OwnedUniverses[i];
            return null;
        }

        public static void EnsureOwnedUniversesReady()
        {
            if (Current.OwnedUniverses == null) Current.OwnedUniverses = new List<OwnedUniverseRecord>();
            if (Current.OwnedUniverses.Count == 0)
            {
                Current.OwnedUniverses.Add(new OwnedUniverseRecord
                {
                    Id = "uni_primary",
                    Name = string.IsNullOrWhiteSpace(Current.UniverseName) ? "Ana Evren" : Current.UniverseName,
                    FandomName = Current.UniverseFandomName ?? "",
                    OwnerStudioId = "player",
                    OwnerStudioName = "Özgün IP #1",
                    IsOriginal = true,
                    Configured = Current.UniverseConfigured,
                    Axes = UniverseAxisSystem.NormalizeAxes(Current.UniverseAxes),
                    ToneName = Current.UniverseToneName,
                    RuleName = Current.UniverseRuleName,
                    LawName = Current.CosmologyLawName,
                    CanonIntegrity = Current.CanonIntegrity,
                    LoreBarrier = Current.LoreBarrier,
                    ConsecutiveSideProjects = Current.ConsecutiveSideProjects,
                    SagaNumber = 1,
                    SagaGamesCount = Current.ReleasedGames != null ? Current.ReleasedGames.Count : 0
                });
                Current.ActiveUniverseId = "uni_primary";
            }
            if (string.IsNullOrEmpty(Current.ActiveUniverseId)) Current.ActiveUniverseId = Current.OwnedUniverses[0].Id;

            // Karakterlerin UniverseId alani bossa birincil evrene ata
            if (Current.Characters != null)
            {
                var prim = Current.OwnedUniverses[0];
                for (int i = 0; i < Current.Characters.Count; i++)
                {
                    var c = Current.Characters[i];
                    if (string.IsNullOrEmpty(c.UniverseId))
                    {
                        c.UniverseId = prim.Id;
                        c.UniverseName = prim.Name;
                    }
                }
            }
        }

        public static void SyncTopLevelToActiveUniverse()
        {
            var act = GetActiveOwnedUniverse();
            act.Name = string.IsNullOrWhiteSpace(Current.UniverseName) ? act.Name : Current.UniverseName;
            act.FandomName = Current.UniverseFandomName ?? "";
            act.Axes = UniverseAxisSystem.NormalizeAxes(Current.UniverseAxes);
            act.ToneName = Current.UniverseToneName;
            act.RuleName = Current.UniverseRuleName;
            act.LawName = Current.CosmologyLawName;
            act.Configured = Current.UniverseConfigured;
            act.CanonIntegrity = Current.CanonIntegrity;
            act.LoreBarrier = Current.LoreBarrier;
            act.ConsecutiveSideProjects = Current.ConsecutiveSideProjects;
        }

        public static void LoadUniverseIntoTopLevel(OwnedUniverseRecord uRec)
        {
            if (uRec == null) return;
            Current.ActiveUniverseId = uRec.Id;
            Current.UniverseName = uRec.Name;
            Current.UniverseFandomName = uRec.FandomName ?? "";
            Current.UniverseAxes = UniverseAxisSystem.NormalizeAxes(uRec.Axes);
            Current.UniverseToneName = uRec.ToneName;
            Current.UniverseRuleName = uRec.RuleName;
            Current.CosmologyLawName = uRec.LawName;
            Current.UniverseConfigured = uRec.Configured;
            Current.CanonIntegrity = Math.Clamp(uRec.CanonIntegrity, 0, 100);
            Current.LoreBarrier = Math.Clamp(uRec.LoreBarrier, 0, 100);
            Current.ConsecutiveSideProjects = uRec.ConsecutiveSideProjects;
        }

        public static void SwitchActiveUniverse(string targetUniId)
        {
            EnsureOwnedUniversesReady();
            SyncTopLevelToActiveUniverse();
            var target = FindOwnedUniverseById(targetUniId);
            if (target == null) return;
            LoadUniverseIntoTopLevel(target);
            Save();
        }

        public static void AddChronology(double week, string category, string title, string detail)
        {
            if (Current.Chronology == null) Current.Chronology = new List<ChronologyEntry>();
            int yr = Math.Max(1, (int)Math.Floor(week / 48.0) + 1);
            Current.Chronology.Add(new ChronologyEntry
            {
                Week = Math.Round(week, 1),
                Year = yr,
                Category = category,
                Title = title,
                Detail = detail
            });
            if (Current.Chronology.Count > 120)
                Current.Chronology.RemoveRange(0, Current.Chronology.Count - 120);
        }

        public static void ApplyUniverseConfig(string name, float[] axes, UniverseRule rule, CosmologyLaw law, string fandomName = null)
        {
            EnsureOwnedUniversesReady();
            bool wasFirst = !Current.UniverseConfigured;
            Current.UniverseName = string.IsNullOrWhiteSpace(name) ? "Ana Evren" : name.Trim();
            if (fandomName != null) Current.UniverseFandomName = fandomName.Trim();
            Current.UniverseAxes = UniverseAxisSystem.NormalizeAxes(axes);
            Current.UniverseToneName = UniverseAxisSystem.DeriveToneFromAxes(Current.UniverseAxes).ToString();
            Current.UniverseRuleName = rule.ToString();
            if (law != CosmologyLaw.None) Current.CosmologyLawName = law.ToString();
            Current.UniverseConfigured = true;
            SyncTopLevelToActiveUniverse();

            // Ayni evrene ait karakterlerin UniverseName etiketini guncelle
            for (int i = 0; i < Current.Characters.Count; i++)
                if (Current.Characters[i].UniverseId == Current.ActiveUniverseId)
                    Current.Characters[i].UniverseName = Current.UniverseName;

            if (wasFirst)
            {
                double w = UniverseArchitectMod.ActiveCompany != null ? UniverseArchitectMod.ActiveCompany.currentWeek : 0.0;
                AddChronology(w, "EVREN", $"'{Current.UniverseName}' Evreni Kuruldu!",
                    $"{UniverseAxisSystem.FormatCompactSummary(Current.UniverseAxes)} | Kural: {CategoryLabels.UniverseRuleToTurkish(rule)}");
            }
            Save();
        }

        public static void SetTurboEnabled(bool enabled)
        {
            Current.Enable10xResearchTurbo = enabled;
            ModSettings.TestMode10xResearch = enabled;
            Save();
        }

        public static float[] EnsureCharacterAxes(CharacterRecord rec)
        {
            if (rec == null) return CharacterAxisSystem.NormalizeAxes(null);
            if (rec.Axes != null && rec.Axes.Length == CharacterAxisSystem.AxisCount) return rec.Axes;
            var preset = Phase1Database.FindArchetype(rec.ArchetypeId).PresetAxes;
            if (rec.Axes != null && rec.Axes.Length == 7)
            {
                rec.Axes = new[] { rec.Axes[0], rec.Axes[1], rec.Axes[2], rec.Axes[3], rec.Axes[4], rec.Axes[5], rec.Axes[6], preset[7] };
                return rec.Axes;
            }
            if (rec.Axes != null && rec.Axes.Length == 4)
            {
                rec.Axes = new[] { rec.Axes[0], rec.Axes[1], rec.Axes[2], rec.Axes[3], preset[4], preset[5], preset[6], preset[7] };
                return rec.Axes;
            }
            if (rec.Sliders != null && rec.Sliders.Length == 9)
                rec.Axes = CharacterAxisSystem.ConvertLegacySlidersToAxes(rec.Sliders);
            else
                rec.Axes = (float[])preset.Clone();
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
                        MultiverseEmpireSystem.EnsureEmpireInitialized(company);
                        SocialSaveManager.LoadForCompany(company, _activeSaveKey);
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
                        MultiverseEmpireSystem.EnsureEmpireInitialized(company);
                        SocialSaveManager.LoadForCompany(company, _activeSaveKey);
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
                        MultiverseEmpireSystem.EnsureEmpireInitialized(company);
                        SocialSaveManager.LoadForCompany(company, _activeSaveKey);
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
            if (Current.GuestLeases == null) Current.GuestLeases = new List<GuestLeaseRecord>();
            if (Current.PublishingDeals == null) Current.PublishingDeals = new List<PublishingDealRecord>();
            if (Current.AwardsHistory == null) Current.AwardsHistory = new List<AwardEntry>();
            if (Current.SalesTrack == null) Current.SalesTrack = new List<GameSalesTrackEntry>();
            if (Current.ReleasedGames == null) Current.ReleasedGames = new List<ReleasedGameRecord>();
            if (Current.TransmediaProjects == null) Current.TransmediaProjects = new List<TransmediaProjectRecord>();
            if (Current.OwnedUniverses == null) Current.OwnedUniverses = new List<OwnedUniverseRecord>();
            if (Current.RivalStarDeltas == null) Current.RivalStarDeltas = new List<RivalStarDelta>();
            if (Current.RivalStars == null) Current.RivalStars = new List<RivalStarRecord>();
            if (Current.PendingRivalGuests == null) Current.PendingRivalGuests = new List<PendingRivalGuestHire>();
            if (Current.Chronology == null) Current.Chronology = new List<ChronologyEntry>();
            if (Current.Rivals == null) Current.Rivals = new List<RivalStudioRecord>();
            if (Current.RivalNews == null) Current.RivalNews = new List<RivalNewsEntry>();
            Current.CanonIntegrity = Math.Clamp(Current.CanonIntegrity, 0, 100);
            Current.LoreBarrier = Math.Clamp(Current.LoreBarrier, 0, 100);
            Current.SoloBurnout = Math.Clamp(Current.SoloBurnout, 0, 100);
            Current.StudioReputation = Math.Clamp(Current.StudioReputation, 0, 100);
            Current.DeptLoreLevel = Math.Clamp(Current.DeptLoreLevel, 0, 3);
            Current.DeptPrLevel = Math.Clamp(Current.DeptPrLevel, 0, 3);
            Current.DeptMerchLevel = Math.Clamp(Current.DeptMerchLevel, 0, 3);

            // Eski v5.0.1 karakterlerini yeni 4 eksen yapisina ve ekip listesine guvenlice tasi
            if (Current.UniverseAxes == null || Current.UniverseAxes.Length != UniverseAxisSystem.AxisCount)
                Current.UniverseAxes = (Current.UniverseAxes != null && Current.UniverseAxes.Length == 4)
                    ? UniverseAxisSystem.NormalizeAxes(Current.UniverseAxes)
                    : UniverseAxisSystem.PresetForTone(GetTone());
            if (!Current.UniverseConfigured && GetTone() != UniverseTone.None)
                Current.UniverseConfigured = true;
            ModSettings.TestMode10xResearch = Current.Enable10xResearchTurbo;

            for (int i = 0; i < Current.Characters.Count; i++)
            {
                var c = Current.Characters[i];
                if (!c.IsNpc) EnsureCharacterAxes(c);
                if (!string.IsNullOrWhiteSpace(c.TeamName)) RegisterTeam(c.TeamName);
            }
            return true;
        }

        private static void TrimCompletedHistory()
        {
            if (Current == null) return;
            if (Current.TransmediaProjects != null && Current.TransmediaProjects.Count > 28)
            {
                int done = 0;
                for (int i = Current.TransmediaProjects.Count - 1; i >= 0; i--)
                {
                    if (!Current.TransmediaProjects[i].IsCompleted) continue;
                    done++;
                    if (done > 25) Current.TransmediaProjects.RemoveAt(i);
                }
            }
            if (Current.GuestLeases != null && Current.GuestLeases.Count > 28)
            {
                int done = 0;
                for (int i = Current.GuestLeases.Count - 1; i >= 0; i--)
                {
                    if (!Current.GuestLeases[i].IsResolved) continue;
                    done++;
                    if (done > 25) Current.GuestLeases.RemoveAt(i);
                }
            }
            if (Current.PublishingDeals != null && Current.PublishingDeals.Count > 28)
            {
                int done = 0;
                for (int i = Current.PublishingDeals.Count - 1; i >= 0; i--)
                {
                    if (!Current.PublishingDeals[i].IsResolved) continue;
                    done++;
                    if (done > 25) Current.PublishingDeals.RemoveAt(i);
                }
            }
            if (Current.Chronology != null && Current.Chronology.Count > 80)
                Current.Chronology.RemoveRange(0, Current.Chronology.Count - 80);
            if (Current.RivalNews != null && Current.RivalNews.Count > 60)
                Current.RivalNews.RemoveRange(0, Current.RivalNews.Count - 60);
        }

        public static void Save()
        {
            List<RivalStarRecord> runtimeStars = null;
            try
            {
                if (Current == null) return;
                if (Current.OwnedUniverses != null && Current.OwnedUniverses.Count > 0)
                    SyncTopLevelToActiveUniverse();
                TrimCompletedHistory();

                // FAZ 10 SAVE DIYETI: 141 rakip yildizin tamami yerine SADECE degisen/transfer olan delta'lari kaydet!
                MultiverseEmpireSystem.BuildRivalStarDeltasForSave();
                runtimeStars = Current.RivalStars;
                Current.RivalStars = null; // WhenWritingDefault sayesinde JSON'a hic yazilmaz!

                File.WriteAllText(ActiveSavePath, JsonSerializer.Serialize(Current, CompactJsonOpts));
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Universe Architect] JSON kaydetme hatasi: {ex.Message}");
            }
            finally
            {
                if (Current != null && runtimeStars != null)
                    Current.RivalStars = runtimeStars;
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
            if (Current.RivalNews.Count > 80)
                Current.RivalNews.RemoveRange(0, Current.RivalNews.Count - 80);
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
            if (cfg == null) return null;

            if (!string.IsNullOrEmpty(keyword))
            {
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
            }

            // Fallback: Hicbir ozel konu ikonsuz kalmasin (Android 15 guvenli mevcut native sprite)
            string[] safeFallbacks = { "Space", "Fantasy", "Sci-Fi", "Adventure" };
            for (int f = 0; f < safeFallbacks.Length; f++)
            {
                try
                {
                    var fb = cfg.GetTopicIconSprite(safeFallbacks[f]);
                    if (fb != null) return fb;
                }
                catch { }
            }
            try
            {
                if (cfg.allSpriteIcons != null && cfg.allSpriteIcons.Count > 0)
                    return cfg.allSpriteIcons[0];
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
        public string FandomName;
        public string StyleOrLaw;
        public string DiplomacyOrRep;
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
                if (exists)
                {
                    // Eski save gocu: FandomName & Entakos guncellemesi
                    for (int j = 0; j < u.Rivals.Count; j++)
                    {
                        var ex = u.Rivals[j];
                        if (ex.Id != tpl.Id) continue;
                        if (string.IsNullOrEmpty(ex.FandomName) && !string.IsNullOrEmpty(tpl.FandomName))
                            ex.FandomName = tpl.FandomName;
                        if (ex.Id == "rival_entakos" && ex.GamesReleased == 0)
                        {
                            if (ex.Fans < tpl.StartingFans) ex.Fans = tpl.StartingFans;
                            if (ex.AverageScore < 8.2) ex.AverageScore = 8.2;
                        }
                    }
                    continue;
                }

                u.Rivals.Add(new RivalStudioRecord
                {
                    Id = tpl.Id,
                    Name = tpl.Name,
                    UniverseName = tpl.UniverseName,
                    FandomName = tpl.FandomName ?? "",
                    ToneName = tpl.Tone.ToString(),
                    ArchetypeStyle = tpl.ArchetypeStyle,
                    Fans = tpl.StartingFans,
                    CanonIntegrity = 85,
                    GamesReleased = 0,
                    LastScore = 0.0,
                    AverageScore = tpl.Id == "rival_entakos" ? 8.2 : 0.0,
                    LastGameTitle = "İlk proje geliştiriliyor",
                    NextReleaseWeek = baseWeek + 5.0 + (i * 1.9),
                    CrossoversDone = 0
                });
                addedAny = true;
            }

            if (addedAny && u.RivalNews.Count == 0)
            {
                UniverseSaveManager.AddRivalNews(
                    baseWeek,
                    "Sektör Bülteni",
                    "24 Rakip Evren Stüdyosu Sahneye Çıktı!",
                    "NJG Games, Entakos Games, Hyperion, Titan Entertainment, Obsidian Crown ve 19 dev stüdyo daha kendi kurgusal evrenlerini inşa etmeye başladı!",
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
                if (r.IsAcquiredByPlayer || r.IsAbsorbedByRival) continue;
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
            double engineBoost = r.HasEngineLicense ? 0.25 : 0.0;
            double rawScore = tpl.BaseSkill + eraBonus + engineBoost + (rnd1 - 0.5) * 2.0 * tpl.Variance;
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
            r.LastReleaseWeek = week;
            r.NextReleaseWeek = week + tpl.ReleaseCadenceWeeks + Math.Round((rnd2 - 0.5) * 6.0);

            string fanTag = !string.IsNullOrEmpty(r.FandomName)
                ? (score >= 8.4 ? $" | {r.FandomName} coştu!" : (score < 5.8 ? $" | {r.FandomName} isyanda!" : $" | Topluluk: {r.FandomName}"))
                : "";
            string detail = $"Evren: {r.UniverseName} ({CategoryLabels.ToneToTurkish(tpl.Tone)}){fanTag} | Tut: {r.CanonIntegrity}/100 | Hayran: {r.Fans:N0}";

            // Ayni Evren Tonu'nda rekabet etkisi (Diplomasi durusuna bagli!)
            var playerTone = UniverseSaveManager.GetTone();
            var stance = CategoryLabels.RelationToStance(r.Relation);
            if (playerTone != UniverseTone.None && tpl.Tone == playerTone && score >= 8.8)
            {
                if (stance == DiplomaticStance.Allied)
                {
                    detail += " | Müttefik olduğunuz için Hype'ına dokunmadı (Dostane rekabet).";
                }
                else
                {
                    int rawSteal = stance == DiplomaticStance.Hostile ? 7 : 4;
                    int prBlock = UniverseSaveManager.Current.DeptPrLevel > 0
                        ? (int)Math.Floor(rawSteal * UniverseSaveManager.Current.DeptPrLevel * 0.20)
                        : 0;
                    int finalSteal = Math.Max(1, rawSteal - prBlock);
                    company.adjustHype(-finalSteal);
                    detail += $" | Aynı tonda çıkardığı hit Hype'ından çaldı (-{finalSteal} Hype).";
                    tag = "REKABET";
                }
            }

            MultiverseEmpireSystem.OnRivalGameReleasedUpdateStars(r, score);
            TivitirSystem.OnRivalGameReleased(company, r, gameTitle, score);
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
                    if (!u.Rivals[i].IsAcquiredByPlayer && !u.Rivals[i].IsAbsorbedByRival &&
                        u.Rivals[i].ToneName == playerTone.ToString() && u.Rivals[i].GamesReleased > 0 && score > u.Rivals[i].LastScore)
                    {
                        beatenRival = u.Rivals[i];
                        break;
                    }
                }

                if (beatenRival != null)
                {
                    var st = CategoryLabels.RelationToStance(beatenRival.Relation);
                    double stealRate = st == DiplomaticStance.Hostile ? 0.12 : 0.08;
                    long stolenFans = Math.Max(40, (long)Math.Round(beatenRival.Fans * stealRate));
                    beatenRival.Fans = Math.Max(50, beatenRival.Fans - stolenFans);
                    company.adjustFans(stolenFans);
                    UniverseSaveManager.AdjustRivalRelation(beatenRival, -14);
                    UniverseSaveManager.AddRivalNews(
                        week,
                        beatenRival.Name,
                        $"{title} sonrası {beatenRival.Name} hayranları saf değiştirdi!",
                        $"Aynı tonda ({CategoryLabels.ToneToTurkish(playerTone)}) {score:F2} puanla rakibini geçtin ve +{stolenFans} hayran çektin! (İlişki -14 -> {beatenRival.Relation:+#;-#;0})",
                        "REKABET"
                    );
                }

                // Nadir Klon Dalgasi tepkisi (en az 16 hafta arayla)
                double cloneRoll = 1.0;
                try { cloneRoll = company.getRandom(); } catch { }
                if (cloneRoll < CloneWaveChance && week - u.LastRivalChallengeWeek >= 16.0 && !NativePopupQueue.HasPending)
                {
                    var activeRivals = HoldingEndgameSystem.GetIndependentRivals();
                    activeRivals.RemoveAll(x => x.Id == "rival_entakos"); // Entakos asla klonlamaz
                    if (activeRivals.Count > 0)
                    {
                        u.LastRivalChallengeWeek = week;
                        var copier = activeRivals[u.ReleasedGames.Count % activeRivals.Count];
                        TriggerCloneWaveEvent(company, copier, title);
                    }
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

        // Rakiplerin oyununu klonlama ihtimali (eskiden her hitte %100 idi)
        private const double CloneWaveChance = 0.12;

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
                        company.adjustHype(15 + UniverseSaveManager.Current.DeptPrLevel * 2);
                        double gain = Math.Max(60.0, Math.Round(company.fans * 0.06));
                        company.adjustFans(gain);
                        rival.Fans = Math.Max(50, rival.Fans - (long)(gain / 2));
                        UniverseSaveManager.AdjustRivalRelation(rival, -10);
                        UniverseSaveManager.AdjustReputation(+2);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                            $"{rival.Name}'in klon hamlesi karşı tanıtımla boşa düştü!",
                            $"Oyuncular orijinal evrenin yanında durdu (+{gain:F0} Hayran, İlişki -10).", "REKABET");
                    }
                    else if (opt == 1)
                    {
                        UniverseSaveManager.AdjustIntegrity(+4);
                        UniverseSaveManager.AdjustRivalRelation(rival, +4);
                        UniverseSaveManager.AdjustReputation(+3);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                            $"{rival.Name} taklit peşindeyken sen kanonunu güçlendirdin.",
                            "Eleştirmenler bu soğukkanlı duruşu övdü (+4 Tutarlılık, +3 İtibar).", "HABER");
                    }
                    else
                    {
                        company.adjustHype(10 + UniverseSaveManager.Current.DeptPrLevel * 2);
                        UniverseSaveManager.AdjustRivalRelation(rival, -18);
                        UniverseSaveManager.AdjustReputation(-2);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                            $"Sektörde söz düellosu! {rival.Name} ile gerilim tırmandı.",
                            $"Sosyal medyada iki evrenin hayranları kapıştı (+10 Hype, İlişki -18 -> {rival.Relation:+#;-#;0}).", "REKABET");
                    }
                    UniverseSaveManager.Save();
                }
            );
        }

        private static void TriggerRivalWeaknessAttack(Company company)
        {
            var u = UniverseSaveManager.Current;
            var activeRivals = HoldingEndgameSystem.GetIndependentRivals();
            if (activeRivals.Count == 0) return;
            var attacker = activeRivals[(int)(company.currentWeek) % activeRivals.Count];
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
                        UniverseSaveManager.AdjustRivalRelation(attacker, -6);
                        UniverseSaveManager.AdjustReputation(+2);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, attacker.Name,
                            $"{attacker.Name}'in atağını Evren Özeti ile savuşturdun!",
                            "Yeni oyuncular hikayeye kolayca ısındı (Bariyer -15, Tutarlılık +5, +2 İtibar).", "REKABET");
                    }
                    else
                    {
                        int hLoss = Math.Max(2, 6 - UniverseSaveManager.Current.DeptPrLevel);
                        company.adjustHype(-hLoss);
                        attacker.Fans += 80;
                        UniverseSaveManager.AdjustRivalRelation(attacker, -10);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, attacker.Name,
                            $"{attacker.Name} kararsız oyunculardan bir kısmını kaptı.",
                            $"Kısa vadede -{hLoss} Hype yaşandı (İlişki -10).", "REKABET");
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
                FandomName = u.UniverseFandomName ?? "",
                StyleOrLaw = CategoryLabels.LawToTurkish(UniverseSaveManager.GetLaw()),
                DiplomacyOrRep = $"İtibar: {u.StudioReputation}/100 ({CategoryLabels.ReputationTier(u.StudioReputation)})",
                IsPlayer = true,
                Fans = pFans,
                Integrity = u.CanonIntegrity,
                AvgScore = playerAvg,
                GamesCount = u.ReleasedGames.Count,
                PowerScore = pPower
            });

            // 2. Rakip & Alt Studyolar (Yutulanlar gizlenir)
            for (int i = 0; i < u.Rivals.Count; i++)
            {
                var r = u.Rivals[i];
                if (r.IsAbsorbedByRival) continue;
                string dipTxt = r.IsAcquiredByPlayer
                    ? $"🏛 ALT STÜDYO ({CategoryLabels.DirectiveToTurkish(CategoryLabels.ParseDirective(r.SubsidiaryDirectiveName))})"
                    : $"{CategoryLabels.StanceToTurkish(CategoryLabels.RelationToStance(r.Relation))} ({r.Relation:+#;-#;0})";
                list.Add(new LeaderboardEntry
                {
                    StudioName = r.IsAcquiredByPlayer ? r.Name + " [HOLDİNG]" : r.Name,
                    UniverseName = r.UniverseName,
                    FandomName = r.FandomName ?? "",
                    StyleOrLaw = r.ArchetypeStyle,
                    DiplomacyOrRep = dipTxt,
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

        public static bool DetectMultiIpCrisis(List<ActiveCharacterDraft> cast, out string summary)
        {
            summary = "";
            if (cast == null || cast.Count < 2) return false;
            var seenUniIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenUniNames = new List<string>();
            for (int i = 0; i < cast.Count; i++)
            {
                var d = cast[i];
                if (d.IsNpc) continue;
                string uid = d.UniverseId;
                string uname = "";
                if (d.IsRivalGuestHire)
                {
                    uid = "guest_" + d.SourceRivalId;
                    uname = d.SourceRivalName;
                }
                else if (d.IsReturning && !string.IsNullOrEmpty(d.ExistingCharacterId))
                {
                    var rec = UniverseSaveManager.FindCharacterById(d.ExistingCharacterId);
                    if (rec != null)
                    {
                        uid = string.IsNullOrEmpty(rec.UniverseId) ? "uni_primary" : rec.UniverseId;
                        var uObj = UniverseSaveManager.FindOwnedUniverseById(uid);
                        uname = uObj != null ? uObj.Name : (string.IsNullOrEmpty(rec.UniverseName) ? "Ana Evren" : rec.UniverseName);
                    }
                }
                else
                {
                    var act = UniverseSaveManager.GetActiveOwnedUniverse();
                    uid = act.Id;
                    uname = act.Name;
                }
                if (!string.IsNullOrEmpty(uid) && seenUniIds.Add(uid))
                    seenUniNames.Add(string.IsNullOrEmpty(uname) ? uid : uname);
            }
            if (seenUniNames.Count >= 2)
            {
                summary = $"{seenUniNames[0]} × {seenUniNames[1]}";
                return true;
            }
            return false;
        }

        public static void RegisterCastForEvents(IntPtr gamePtr, string gameTitle, List<ActiveCharacterDraft> cast, bool isSagaFinale = false)
        {
            if (gamePtr == IntPtr.Zero) return;
            var outcome = GetOrCreateOutcome(gamePtr);
            outcome.IsSagaFinale = isSagaFinale;

            string multiSummary;
            if (DetectMultiIpCrisis(cast, out multiSummary))
            {
                outcome.IsMultiIpCrisis = true;
                outcome.MultiIpSummary = multiSummary;
                outcome.IsCrossover = true;
                outcome.CrossoverSummary = multiSummary;
                MelonLogger.Msg($"[Faz 10.0] COKLU IP KRIZI ALGILANDI: '{gameTitle}' -> {multiSummary}");
                return;
            }

            string pairSummary;
            if (DetectCrossoverCast(cast, out pairSummary))
            {
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

            // FAZ 9.0: CANLI FANDOM TEORI MOTORU & SENARYO SIZINTISI (LEAK KRIZI)
            var u = UniverseSaveManager.Current;
            var cat = UniverseChoicePopupSystem.GetCategoryForGame(game);
            if (cat != ProjectCategory.NonUniverse)
            {
                if (company.currentWeek - u.LastFandomEventWeek >= 12.0 && progress >= 0.42 && MarkFired(gPtr, "evt_fan_theory"))
                {
                    var uAx = UniverseSaveManager.GetUniverseAxes();
                    var cast = UniverseChoicePopupSystem.GetCastForGame(game);
                    bool hasMyst = uAx[3] <= 0.48f;
                    if (!hasMyst && cast != null)
                        for (int c = 0; c < cast.Count; c++)
                            if (cast[c].Trait == CharacterTrait.MysteriousPast) { hasMyst = true; break; }

                    if (hasMyst)
                    {
                        u.LastFandomEventWeek = company.currentWeek;
                        TriggerFanTheoryEvent(company, gPtr, title);
                        return;
                    }
                }

                if (u.ReleasedGames.Count >= 2 && company.currentWeek - u.LastLeakEventWeek >= 16.0 && progress >= 0.55 && MarkFired(gPtr, "evt_leak_crisis"))
                {
                    double lkRoll = 1.0;
                    try { lkRoll = company.getRandom(); } catch { }
                    if (lkRoll <= 0.35)
                    {
                        u.LastLeakEventWeek = company.currentWeek;
                        TriggerLeakCrisisEvent(company, gPtr, title);
                        return;
                    }
                }
            }
        }

        private static void TriggerFanTheoryEvent(Company company, IntPtr gPtr, string title)
        {
            string uniName = UniverseSaveManager.Current.UniverseName;
            string fandom = UniverseSaveManager.Current.UniverseFandomName;
            string fanWho = !string.IsNullOrEmpty(fandom) ? $"{fandom} ({uniName} topluluğu)" : $"'{uniName}' hayranları";
            NativePopupQueue.Enqueue(
                $"🔮 Hayran Teorisi Patladı: {title}",
                $"{fanWho} forumlarda '{title}' hakkında çılgın bir teori uydurdu ve milyonlarca izlendi! Senarist olarak ne yapacaksın?\n\n" +
                $"• Teoriyi Kanona Al (Ücretsiz): Hayranlar çıldırır (+10 Hype, +3 İtibar).\n" +
                $"• Ters Köşe Yap (Kumar): Oyun 7.5+ alırsa +%5 Skor & +12 Hype şok zaferi; 6.5 altı alırsa -6 Hype!\n" +
                $"• Sessiz Kal & Gizemi Koru: +4 Tutarlılık.",
                new[] { "Teoriyi Kanona Al", "Ters Köşe Yap (Riskli)", "Sessiz Kal (+4 Tut.)" },
                opt =>
                {
                    var o = GetOrCreateOutcome(gPtr);
                    if (opt == 0)
                    {
                        o.HypeDelta += 10;
                        UniverseSaveManager.AdjustReputation(+3);
                        o.Notes.Add("🔮 Hayran Teorisi Kanonlaştı (+10 Hype, +3 İtibar)");
                    }
                    else if (opt == 1)
                    {
                        o.HasTwistGamble = true;
                        o.Notes.Add("🔮 Ters Köşe Final Planlandı");
                    }
                    else
                    {
                        o.IntegrityDelta += 4;
                        o.Notes.Add("🔮 Gizem Korundu (+4 Tutarlılık)");
                    }
                }
            );
        }

        private static void TriggerLeakCrisisEvent(Company company, IntPtr gPtr, string title)
        {
            int prBonus = UniverseSaveManager.Current.DeptPrLevel * 2;
            NativePopupQueue.Enqueue(
                $"🚨 SENARYO SIZINTISI (LEAK KRİZİ): {title}",
                $"Kötü haber! '{title}' oyununun büyük final sahnesi internete sızdı ve sosyal medyada dönüyor! Acil kriz kararı:\n\n" +
                $"• Finali Baştan Yaz (-18K): Sızıntıyı boşa çıkar (+%4 Skor, +5 Tutarlılık).\n" +
                $"• Sahte Sızıntılar Sal (-6K): Kafa karıştırıp merakı körükle (+10 Hype, Bariyer -5).\n" +
                $"• PR ile Fırsata Çevir (Ücretsiz): +{6 + prBonus} Hype ama -3 Tutarlılık.",
                new[] { "Finali Yenile (-18K)", "Sahte Sızıntı (-6K)", "PR ile Çevir" },
                opt =>
                {
                    var o = GetOrCreateOutcome(gPtr);
                    if (opt == 0)
                    {
                        company.adjustCash(-18000.0, "Sızıntı Krizi: Yeni Final");
                        o.ScoreDelta += 0.04; o.IntegrityDelta += 5;
                        o.Notes.Add("🚨 Sızıntı Aşıldı: Yeni Final (+%4 Skor, +5 Tut.)");
                        UniverseSaveManager.AddChronology(company.currentWeek, "KRIZ", $"🚨 '{title}' Sızıntı Krizi Aşıldı", "Final baştan yazıldı.");
                    }
                    else if (opt == 1)
                    {
                        company.adjustCash(-6000.0, "Sahte Sızıntı Operasyonu");
                        o.HypeDelta += 10; o.BarrierDelta -= 5;
                        o.Notes.Add("🚨 Sahte Sızıntı Hamlesi (+10 Hype)");
                    }
                    else
                    {
                        o.HypeDelta += (6 + prBonus); o.IntegrityDelta -= 3;
                        o.Notes.Add($"🚨 PR Sızıntı Yönetimi (+{6 + prBonus} Hype, -3 Tut.)");
                    }
                }
            );
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

            double merchMult = 1.0 + (UniverseSaveManager.Current.DeptMerchLevel * 0.05) + (UniverseSaveManager.GetCulture() == StudioCulture.HypeMachine ? 0.15 : 0.0) + (MultiverseEmpireSystem.GetActiveSectorEvent(company.currentWeek) == SectorEventType.GoldenAge ? 0.15 : 0.0) + (TivitirSystem.IsPlayerTrendingNo1(company) ? 0.20 : 0.0);
            double revenue = Math.Round(p.Cost * (0.65 + qualityFactor * 0.85) * merchMult);
            int fansGain = 0;
            string summary = "";

            if (tType == TransmediaType.ComicSeries)
            {
                UniverseSaveManager.AdjustLoreBarrier(-20);
                int hypeAdd = isHit ? 12 : 8;
                UniverseSaveManager.Current.PendingTransmediaHype = Math.Min(25, UniverseSaveManager.Current.PendingTransmediaHype + hypeAdd);
                fansGain = (int)Math.Max(60, Math.Round(company.fans * (isHit ? 0.08 : 0.05)));
                if (focusChar != null) focusChar.Popularity = PopularitySystem.ApplyDelta(focusChar.Popularity, 8 + lawPopBonus);
                summary = $"Bariyer -20, Sıradakine +{hypeAdd} Hype, +{fansGain} Hayran, +{revenue / 1000.0:F1}K Gelir";
            }
            else if (tType == TransmediaType.Novel)
            {
                UniverseSaveManager.AdjustIntegrity(+10);
                double scoreAdd = isHit ? 0.03 : 0.02;
                UniverseSaveManager.Current.PendingTransmediaScoreBonus = Math.Min(0.05, UniverseSaveManager.Current.PendingTransmediaScoreBonus + scoreAdd);
                UniverseSaveManager.Current.PendingTransmediaHype = Math.Min(25, UniverseSaveManager.Current.PendingTransmediaHype + 6);
                fansGain = (int)Math.Max(50, Math.Round(company.fans * (isHit ? 0.07 : 0.04)));
                if (focusChar != null) focusChar.Popularity = PopularitySystem.ApplyDelta(focusChar.Popularity, 10 + lawPopBonus);
                summary = $"Tutarlılık +10, Sıradakine +%{scoreAdd * 100:F0} Skor & +6 Hype, +{fansGain} Hayran, +{revenue / 1000.0:F1}K Gelir";
            }
            else
            {
                int hypeAdd = isHit ? 18 : 14;
                UniverseSaveManager.Current.PendingTransmediaHype = Math.Min(25, UniverseSaveManager.Current.PendingTransmediaHype + hypeAdd);
                fansGain = (int)Math.Max(100, Math.Round(company.fans * (isHit ? 0.10 : 0.06)));
                if (focusChar != null) focusChar.Popularity = PopularitySystem.ApplyDelta(focusChar.Popularity, 7 + lawPopBonus);
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
    // 9B. FAZ 6.2: RAKIPLERE KONUK KARAKTER KIRALAMA (GUEST DLC SISTEMI)
    // =========================================================================
    public static class GuestDlcSystem
    {
        public static bool IsCharacterOnActiveLease(string charId)
        {
            if (string.IsNullOrEmpty(charId) || UniverseSaveManager.Current.GuestLeases == null) return false;
            var list = UniverseSaveManager.Current.GuestLeases;
            for (int i = 0; i < list.Count; i++)
                if (!list[i].IsResolved && list[i].CharacterId == charId) return true;
            return false;
        }

        public static int GetActiveLeaseCount()
        {
            var list = UniverseSaveManager.Current.GuestLeases;
            if (list == null) return 0;
            int cnt = 0;
            for (int i = 0; i < list.Count; i++)
                if (!list[i].IsResolved) cnt++;
            return cnt;
        }

        public static List<CharacterRecord> GetEligibleStars()
        {
            var res = new List<CharacterRecord>();
            var chars = UniverseSaveManager.Current.Characters;
            double nowW = UniverseArchitectMod.ActiveCompany != null ? UniverseArchitectMod.ActiveCompany.currentWeek : 0.0;
            for (int i = 0; i < chars.Count; i++)
            {
                var c = chars[i];
                if (c.IsNpc) continue;
                if (CategoryLabels.ParseFate(c.FateName) != CharacterFate.Alive) continue;
                if (IsCharacterOnActiveLease(c.Id)) continue;
                if (nowW - c.LastGuestLeaseWeek < 18.0) continue; // Ayni yildizi pes pese sonsuza dek kiralama freni!
                if (c.Status == "Yıldız" || c.Status == "Kült" || c.Popularity >= 72)
                    res.Add(c);
            }
            return res;
        }

        public static void Tick(Company company)
        {
            if (company == null) return;
            var u = UniverseSaveManager.Current;
            if (u.GuestLeases == null) u.GuestLeases = new List<GuestLeaseRecord>();

            // 1. Suresi dolan Konuk DLC anlasmalarini sonuclandir
            bool anyChanged = false;
            for (int i = 0; i < u.GuestLeases.Count; i++)
            {
                var lease = u.GuestLeases[i];
                if (lease == null || lease.IsResolved) continue;
                if (company.currentWeek < lease.ResolveWeek) continue;

                ResolveLease(company, lease);
                anyChanged = true;
            }

            // 2. Periyodik otomatik rakip Konuk DLC teklifi (en az 14 hafta arayla)
            if (company.currentWeek - u.LastGuestOfferWeek >= 14.0 &&
                GetActiveLeaseCount() < 2 &&
                !NativePopupQueue.HasPending && !CharacterWorkshopUGUI.IsOpen && !UniverseArchitectMod.IsNativeModalOpen())
            {
                var stars = GetEligibleStars();
                if (stars.Count > 0 && u.Rivals != null && u.Rivals.Count > 0)
                {
                    u.LastGuestOfferWeek = company.currentWeek;
                    EnqueueGuestOffer(company, stars, u.Rivals);
                    anyChanged = true;
                }
            }

            if (anyChanged) UniverseSaveManager.Save();
        }

        public static bool TryRequestManualOffer(Company company, out string feedback)
        {
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }
            var u = UniverseSaveManager.Current;
            if (GetActiveLeaseCount() >= 2)
            {
                feedback = "Aynı anda en fazla 2 aktif Konuk DLC kiralama anlaşması yürütülebilir.";
                return false;
            }
            double waitLeft = 8.0 - (company.currentWeek - u.LastGuestOfferWeek);
            if (waitLeft > 0.1)
            {
                feedback = $"Yeni teklif istemek için ~{waitLeft:F1} hafta daha beklemen gerekiyor.";
                return false;
            }
            var stars = GetEligibleStars();
            if (stars.Count == 0)
            {
                feedback = "Kiralanabilir uygun Yıldız/Kült karakter yok (Popülerlik 75+, hayatta ve boşta olmalı).";
                return false;
            }
            if (u.Rivals == null || u.Rivals.Count == 0)
            {
                feedback = "Henüz aktif rakip stüdyo yok.";
                return false;
            }

            u.LastGuestOfferWeek = company.currentWeek;
            UniverseSaveManager.Save();
            EnqueueGuestOffer(company, stars, u.Rivals);
            feedback = "Rakiplere yıldız kataloğun gönderildi! Pencereyi kapattığında gelen teklif önüne açılacak.";
            return true;
        }

        private static void EnqueueGuestOffer(Company company, List<CharacterRecord> stars, List<RivalStudioRecord> rivals)
        {
            double r1 = 0.5, r2 = 0.5;
            try { r1 = company.getRandom(); r2 = company.getRandom(); } catch { }

            // Kanli Bicakli rakipler Konuk DLC teklifi getirmez!
            var nonHostile = new List<RivalStudioRecord>();
            for (int i = 0; i < rivals.Count; i++)
                if (!rivals[i].IsAcquiredByPlayer && !rivals[i].IsAbsorbedByRival &&
                    CategoryLabels.RelationToStance(rivals[i].Relation) != DiplomaticStance.Hostile)
                    nonHostile.Add(rivals[i]);
            if (nonHostile.Count == 0) return;

            var star = stars[(int)(r1 * stars.Count) % stars.Count];
            var rival = nonHostile[(int)(r2 * nonHostile.Count) % nonHostile.Count];
            var tpl = Phase1Database.FindRivalTemplate(rival.Id);
            var stance = CategoryLabels.RelationToStance(rival.Relation);
            int rep = UniverseSaveManager.Current.StudioReputation;

            int titleIdx = (rival.GamesReleased + UniverseSaveManager.Current.GuestLeases.Count) % tpl.TitlePool.Length;
            string dlcGameTitle = $"{tpl.TitlePool[titleIdx]}: Konuk DLC";

            double rawFee = 15000.0 + star.Popularity * 220.0 + Math.Min(rival.Fans, 4000) * 14.0;
            double bonusMult = 1.0 + UniverseSaveManager.Current.DeptMerchLevel * 0.05;
            if (UniverseSaveManager.GetCulture() == StudioCulture.HypeMachine) bonusMult += 0.15;
            if (stance == DiplomaticStance.Allied) bonusMult += 0.25;
            else if (stance == DiplomaticStance.Friendly) bonusMult += 0.10;
            if (rep >= 75) bonusMult += 0.20;
            else if (rep < 35) bonusMult -= 0.20;

            double fee = Math.Clamp(Math.Round((rawFee * bonusMult) / 1000.0) * 1000.0, 18000.0, 180000.0);
            double highFee = Math.Round((fee * 1.30) / 1000.0) * 1000.0;

            double negChance = 0.60;
            if (rep >= 75) negChance += 0.15;
            else if (rep < 35) negChance -= 0.15;
            if (stance == DiplomaticStance.Allied || stance == DiplomaticStance.Friendly) negChance += 0.08;
            negChance = Math.Clamp(negChance, 0.35, 0.85);
            int negPct = (int)Math.Round(negChance * 100.0);

            NativePopupQueue.Enqueue(
                $"Konuk DLC Teklifi: {rival.Name} [{CategoryLabels.StanceToTurkish(stance)}]",
                $"{rival.Name} ({rival.UniverseName}), yeni oyunu '{dlcGameTitle}' için senin yıldızın {star.Name} (Pop: {star.Popularity}/100) karakterini tek seferlik Konuk DLC olarak kiralamak istiyor!\n\n" +
                $"• Kabul Et (+{fee / 1000.0:F0}K Peşin, +12 İlişki): 6 hafta sonra DLC çıkar. 7.5+ alırsa büyük hayran akışı & +6 Pop; 5.8 altı prestij çiziği!\n" +
                $"• Pazarlık Yap (+{highFee / 1000.0:F0}K İste): %{negPct} şansla yüksek telifle anlaşılır (İtibar & Diplomasi etkili).\n" +
                $"• Reddet: Karakter sadece kendi evreninde kalsın (+2 Tutarlılık, -3 İlişki).",
                new[] { $"Kabul Et (+{fee / 1000.0:F0}K)", $"Pazarlık (+{highFee / 1000.0:F0}K)", "Reddet" },
                opt =>
                {
                    if (opt == 0)
                    {
                        UniverseSaveManager.AdjustRivalRelation(rival, +12);
                        StartLease(company, rival, dlcGameTitle, star, fee);
                    }
                    else if (opt == 1)
                    {
                        double negRoll = 0.5;
                        try { negRoll = company.getRandom(); } catch { }
                        if (negRoll <= negChance)
                        {
                            UniverseSaveManager.AdjustRivalRelation(rival, +8);
                            StartLease(company, rival, dlcGameTitle, star, highFee);
                            NotificationHelper.Show(company, $"Pazarlık Başarılı: {rival.Name}!",
                                $"{rival.Name}, {star.Name} için +{highFee / 1000.0:F0}K peşin telifi kabul etti! (+8 İlişki)");
                        }
                        else
                        {
                            UniverseSaveManager.AdjustRivalRelation(rival, -5);
                            UniverseSaveManager.Save();
                            NotificationHelper.Show(company, $"Pazarlık Bozuldu: {rival.Name}",
                                $"{rival.Name} '{highFee / 1000.0:F0}K bütçemizi aşıyor' diyerek masadan kalktı (-5 İlişki).");
                        }
                    }
                    else
                    {
                        UniverseSaveManager.AdjustIntegrity(+2);
                        UniverseSaveManager.AdjustRivalRelation(rival, -3);
                        UniverseSaveManager.Save();
                        MelonLogger.Msg($"[Faz 7.0 Guest DLC] {rival.Name} teklifi reddedildi ({star.Name}, +2 Tutarlilik, -3 Iliski).");
                    }
                }
            );
        }

        private static void StartLease(Company company, RivalStudioRecord rival, string dlcGameTitle, CharacterRecord star, double fee)
        {
            company.adjustCash(fee, $"Konuk DLC Telifi ({rival.Name}): {star.Name}");
            if (star != null) star.LastGuestLeaseWeek = company.currentWeek;

            var rec = new GuestLeaseRecord
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                RivalId = rival.Id,
                RivalName = rival.Name,
                RivalGameTitle = dlcGameTitle,
                CharacterId = star.Id,
                CharacterName = star.Name,
                UpfrontFee = fee,
                StartWeek = company.currentWeek,
                ResolveWeek = company.currentWeek + 6.0,
                IsResolved = false
            };

            UniverseSaveManager.Current.GuestLeases.Add(rec);
            UniverseSaveManager.AddRivalNews(
                company.currentWeek,
                rival.Name,
                $"KONUK DLC ANLAŞMASI: {star.Name} → {rival.Name}!",
                $"{star.Name}, '{dlcGameTitle}' projesine konuk oluyor (+{fee / 1000.0:F0}K peşin telif). 6 hafta sonra çıkacak!",
                "CROSSOVER"
            );
            UniverseSaveManager.Save();
            MelonLogger.Msg($"[Faz 7.0 Guest DLC] Kiralama basladi: {star.Name} -> {rival.Name} (+{fee:F0}, Bitis: {rec.ResolveWeek:F1})");
        }

        private static void ResolveLease(Company company, GuestLeaseRecord lease)
        {
            lease.IsResolved = true;
            var star = UniverseSaveManager.FindCharacterById(lease.CharacterId);
            var tpl = Phase1Database.FindRivalTemplate(lease.RivalId);

            RivalStudioRecord rival = null;
            var rivals = UniverseSaveManager.Current.Rivals;
            for (int i = 0; i < rivals.Count; i++)
                if (rivals[i].Id == lease.RivalId) { rival = rivals[i]; break; }

            double rnd = 0.5;
            try { rnd = company.getRandom(); } catch { }

            double starBoost = star != null ? (star.Popularity - 60) * 0.008 : 0.15;
            double rawScore = tpl.BaseSkill + starBoost + (rnd - 0.5) * 2.0 * tpl.Variance;
            double score = Math.Round(Math.Clamp(rawScore, 3.8, 9.6), 2);
            lease.RivalScore = score;

            long rivalFans = rival != null ? rival.Fans : 300;

            if (score >= 7.5)
            {
                int fanGain = (int)Math.Max(55, Math.Round(rivalFans * 0.14));
                lease.FansGained = fanGain;
                company.adjustFans(fanGain);
                company.adjustHype(6);

                double royalty = score >= 8.5 ? Math.Round(lease.UpfrontFee * 0.35) : 0.0;
                if (royalty > 0) company.adjustCash(royalty, $"Konuk DLC Ek Telif: {lease.CharacterName}");
                if (star != null) star.Popularity = PopularitySystem.ApplyDelta(star.Popularity, 6);
                UniverseSaveManager.AdjustRivalRelation(rival, +10);
                UniverseSaveManager.AdjustReputation(+3);

                string royStr = royalty > 0 ? $", +{royalty / 1000.0:F1}K Ek Telif" : "";
                lease.ResultSummary = $"[BÜYÜK BAŞARI — {score:F1}/10] +{fanGain} Hayran, +6 Hype, +6 Pop{royStr}";
                UniverseSaveManager.AddRivalNews(company.currentWeek, lease.RivalName,
                    $"{lease.CharacterName} konuk olduğu '{lease.RivalGameTitle}' ile şov yaptı ({score:F1}/10)!",
                    $"{lease.RivalName} kitlesinden +{fanGain} yeni hayran senin evrenine aktı{royStr}.", "HIT");
                NotificationHelper.Show(company, $"Konuk DLC Zaferi: {lease.CharacterName}!",
                    $"{lease.RivalName} oyunu '{lease.RivalGameTitle}' {score:F1}/10 aldı! {lease.CharacterName} rakiplerin kitlesini fethetti (+{fanGain} Hayran, +6 Pop{royStr}).", true);
            }
            else if (score >= 5.8)
            {
                int fanGain = (int)Math.Max(25, Math.Round(rivalFans * 0.06));
                lease.FansGained = fanGain;
                company.adjustFans(fanGain);
                if (star != null) star.Popularity = PopularitySystem.ApplyDelta(star.Popularity, 2);

                lease.ResultSummary = $"[BAŞARILI — {score:F1}/10] +{fanGain} Hayran, +2 Popülerlik";
                UniverseSaveManager.AddRivalNews(company.currentWeek, lease.RivalName,
                    $"'{lease.RivalGameTitle}' ({score:F1}/10) ile {lease.CharacterName} ilgi gördü.",
                    $"+{fanGain} hayran senin evrenini keşfetti.", "HABER");
                NotificationHelper.Show(company, $"Konuk DLC Yayında: {lease.CharacterName}",
                    $"{lease.RivalName} oyunu '{lease.RivalGameTitle}' {score:F1}/10 aldı. {lease.CharacterName} temiz bir reklam yaptı (+{fanGain} Hayran, +2 Pop).");
            }
            else
            {
                lease.FansGained = 0;
                company.adjustHype(-3);
                if (star != null) star.Popularity = PopularitySystem.ApplyDelta(star.Popularity, -7);
                UniverseSaveManager.AdjustRivalRelation(rival, -12);
                UniverseSaveManager.AdjustReputation(-4);

                lease.ResultSummary = $"[PRESTİJ ÇİZİĞİ — {score:F1}/10] Rakip çuvalladı! (-6 Pop, -3 Hype)";
                UniverseSaveManager.AddRivalNews(company.currentWeek, lease.RivalName,
                    $"'{lease.RivalGameTitle}' ({score:F1}/10) fiyasko oldu — {lease.CharacterName} prestij çizigi yedi!",
                    "Hayranlar 'Yıldızımızı yanlış stüdyoya kiralamışsınız' diye sitem ediyor (-6 Pop).", "FIYASKO");
                NotificationHelper.Show(company, $"Konuk DLC Pişmanlığı: {lease.CharacterName}!",
                    $"{lease.RivalName} oyunu '{lease.RivalGameTitle}' sadece {score:F1}/10 alabildi! {lease.CharacterName} bu fiyaskodan ufak bir prestij çizigi yedi (-6 Pop, -3 Hype).");
            }
        }
    }


    // =========================================================================
    // 9C. FAZ 7.0: SIRKET DNA'SI, 3 DEPARTMAN, ITIBAR, DIPLOMASI & MOTOR LISANSI
    // =========================================================================
    public static class CompanyDiplomacySystem
    {
        public static readonly double[] LoreCosts  = { 30000.0, 75000.0, 160000.0 };
        public static readonly double[] PrCosts    = { 25000.0, 65000.0, 140000.0 };
        public static readonly double[] MerchCosts = { 35000.0, 85000.0, 180000.0 };

        public static readonly double[] DeptWeeklyUpkeep = { 1200.0, 1000.0, 800.0 }; // Lore, PR, Merch per level

        public static void LogCompanyTelemetry(Company company, string reason)
        {
            if (company == null) return;
            try
            {
                int lvl = company.currentLevel;
                int staffCount = company.staff != null ? company.staff.Count : 1;
                int engineCount = company.engines != null ? company.engines.Count : 0;
                UniverseSaveManager.Current.LastLoggedOfficeLevel = lvl;
                MelonLogger.Msg($"[Faz 7.0 Telemetri — {reason}] currentLevel={lvl}, staff={staffCount}, engines={engineCount}, fans={company.fans}, cash={company.cash:F0}");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 7.0 Telemetri] Company okuma hatasi: {ex.Message}");
            }
        }

        public static void LogGameSalesTelemetry(Game game, Company company)
        {
            if (game == null) return;
            try
            {
                int engineCount = (company != null && company.engines != null) ? company.engines.Count : 0;
                int lvl = company != null ? company.currentLevel : -1;
                MelonLogger.Msg($"[Faz 7.0 Satis Telemetri] '{game.title ?? "?"}' -> revenue={game.revenue:F0}, totalSalesCash={game.totalSalesCash:F0}, unitsSold={game.unitsSold:F0}, amountSold={game.amountSold}, costs={game.costs:F0} | companyLevel={lvl}, engines={engineCount}");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 7.0 Satis Telemetri] Okuma hatasi: {ex.Message}");
            }
        }

        public static int GetUnlockedDeptCap(Company company)
        {
            if (company == null) return 0;
            int staff = company.staff != null ? company.staff.Count : 1;
            int lvl = 0;
            try { lvl = company.currentLevel; } catch { }
            long fans = company.fans;
            int games = UniverseSaveManager.Current.ReleasedGames.Count;

            if (staff >= 3 || lvl >= 3 || fans >= 5000) return 3;
            if (staff >= 2 || lvl >= 2 || fans >= 1000) return 2;
            if (games >= 2 || staff >= 2 || lvl >= 2 || fans >= 150) return 1;
            return 0;
        }

        public static string GetUnlockHintForLevel(int targetLevel)
        {
            if (targetLevel == 1) return "Kilit: En az 2 oyun çıkar veya 150 Hayran / 2 Çalışan ulaş.";
            if (targetLevel == 2) return "Kilit: 2+ Çalışan veya 1.000 Hayran (Ofis Büyümesi) gerekir.";
            return "Kilit: 3+ Çalışan veya 5.000 Hayran (Büyük Stüdyo) gerekir.";
        }

        public static double GetTotalWeeklyUpkeep()
        {
            var u = UniverseSaveManager.Current;
            return u.DeptLoreLevel * DeptWeeklyUpkeep[0] +
                   u.DeptPrLevel   * DeptWeeklyUpkeep[1] +
                   u.DeptMerchLevel * DeptWeeklyUpkeep[2];
        }

        public static bool TrySetCulture(Company company, StudioCulture newCulture, out string feedback)
        {
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }
            var cur = UniverseSaveManager.GetCulture();
            if (cur == newCulture) { feedback = "Bu kültür zaten aktif!"; return false; }

            if (cur == StudioCulture.None)
            {
                UniverseSaveManager.SetCulture(newCulture);
                UniverseSaveManager.Current.LastCultureChangeWeek = company.currentWeek;
                UniverseSaveManager.Save();
                feedback = $"Şirket DNA'sı belirlendi: {CategoryLabels.CultureToTurkish(newCulture)}!";
                return true;
            }

            double waitLeft = 24.0 - (company.currentWeek - UniverseSaveManager.Current.LastCultureChangeWeek);
            if (waitLeft > 0.1)
            {
                feedback = $"Kültür dönüşümü için ~{waitLeft:F1} hafta daha beklemen gerekiyor.";
                return false;
            }
            if (company.cash < 35000.0)
            {
                feedback = "Şirket kültürünü değiştirmek için 35K nakit gerekiyor!";
                return false;
            }

            company.adjustCash(-35000.0, $"Kültür Dönüşümü: {CategoryLabels.CultureToTurkish(newCulture)}");
            UniverseSaveManager.AdjustReputation(-4);
            UniverseSaveManager.SetCulture(newCulture);
            UniverseSaveManager.Current.LastCultureChangeWeek = company.currentWeek;
            UniverseSaveManager.AddRivalNews(company.currentWeek, company.name ?? "Sen",
                $"ŞİRKET DÖNÜŞÜMÜ: Yeni Kültür '{CategoryLabels.CultureToTurkish(newCulture)}'!",
                "Stüdyo yeni bir vizyon ilan etti; sektör yeni stratejiyi konuşuyor (-4 İtibar).", "SIRKET");
            UniverseSaveManager.Save();
            feedback = $"Kültür güncellendi: {CategoryLabels.CultureToTurkish(newCulture)} (-35K, -4 İtibar)";
            return true;
        }

        public static bool TryUpgradeDepartment(Company company, int deptIdx, out string feedback)
        {
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }

            var u = UniverseSaveManager.Current;
            int curLvl = deptIdx == 0 ? u.DeptLoreLevel : (deptIdx == 1 ? u.DeptPrLevel : u.DeptMerchLevel);
            if (curLvl >= 3) { feedback = "Bu departman zaten maksimum (Sv.3) seviyede!"; return false; }

            int nextLvl = curLvl + 1;
            int cap = GetUnlockedDeptCap(company);
            if (nextLvl > cap)
            {
                feedback = GetUnlockHintForLevel(nextLvl);
                return false;
            }

            double cost = deptIdx == 0 ? LoreCosts[curLvl] : (deptIdx == 1 ? PrCosts[curLvl] : MerchCosts[curLvl]);
            if (company.cash < cost)
            {
                feedback = $"Yetersiz nakit! (Gereken: {cost / 1000.0:F0}K)";
                return false;
            }

            string deptName = deptIdx == 0 ? "Lore & Senaryo Odası" : (deptIdx == 1 ? "PR & Kriz Masası" : "Merch & Lisans Ofisi");
            company.adjustCash(-cost, $"Departman Kurulumu: {deptName} Sv.{nextLvl}");

            if (deptIdx == 0) u.DeptLoreLevel = nextLvl;
            else if (deptIdx == 1) u.DeptPrLevel = nextLvl;
            else u.DeptMerchLevel = nextLvl;

            if (u.LastDeptTickWeek <= 0.1) u.LastDeptTickWeek = company.currentWeek;
            UniverseSaveManager.AdjustReputation(+2);
            UniverseSaveManager.Save();

            NotificationHelper.Show(company, $"Departman Yükseltildi: {deptName} Sv.{nextLvl}!",
                $"{deptName} artık Seviye {nextLvl} olarak stüdyona hizmet veriyor! (+2 İtibar)");
            feedback = $"{deptName} Seviye {nextLvl} oldu! (-{cost / 1000.0:F0}K, +2 İtibar)";
            return true;
        }

        public static int CountMerchEligibleStars(out double avgPop)
        {
            int count = 0;
            double popSum = 0.0;
            var chars = UniverseSaveManager.Current.Characters;
            for (int i = 0; i < chars.Count; i++)
            {
                var c = chars[i];
                if (c.IsNpc) continue;
                if (c.Status == "Yıldız" || c.Status == "Kült" || c.Status == "Ölümsüz Efsane" || c.Status == "Emekli Efsane" || c.Popularity >= 75)
                {
                    count++;
                    popSum += c.Popularity;
                }
            }
            avgPop = count > 0 ? (popSum / count) : 0.0;
            return count;
        }

        public static double EstimateMonthlyMerchPayout()
        {
            var u = UniverseSaveManager.Current;
            if (u.DeptMerchLevel <= 0) return 0.0;
            double avgPop;
            int stars = CountMerchEligibleStars(out avgPop);
            if (stars <= 0) return 0.0;

            double cultureMult = UniverseSaveManager.GetCulture() == StudioCulture.HypeMachine ? 1.15 : 1.0;
            double repMult = 0.80 + (u.StudioReputation / 250.0);
            int effectiveStars = Math.Min(stars, 6 + u.DeptMerchLevel * 3);
            double curW = UniverseArchitectMod.ActiveCompany != null ? UniverseArchitectMod.ActiveCompany.currentWeek : 0.0;
            double goldenMult = (MultiverseEmpireSystem.GetActiveSectorEvent(curW) == SectorEventType.GoldenAge ? 1.15 : 1.0)
                              * (TivitirSystem.IsPlayerTrendingNo1(UniverseArchitectMod.ActiveCompany) ? 1.20 : 1.0);
            double raw = effectiveStars * (800.0 + avgPop * 16.0) * u.DeptMerchLevel * cultureMult * repMult * goldenMult;
            return Math.Round(raw / 100.0) * 100.0;
        }

        public static bool TryGetBestPlayerEngine(Company company, out string engineName, out double techLevel, out int partCount)
        {
            engineName = "";
            techLevel = 0.0;
            partCount = 0;
            if (company == null || company.engines == null || company.engines.Count == 0) return false;

            try
            {
                EngineType best = null;
                double bestScore = -1.0;
                for (int i = 0; i < company.engines.Count; i++)
                {
                    var eng = company.engines[i];
                    if (eng == null) continue;
                    double tl = 1.0;
                    try { tl = eng.techLevel; } catch { }
                    int pc = 0;
                    try { if (eng.partsIds != null) pc = eng.partsIds.Count; } catch { }
                    double sc = tl * 10.0 + pc;
                    if (sc > bestScore)
                    {
                        bestScore = sc;
                        best = eng;
                        techLevel = tl;
                        partCount = pc;
                    }
                }
                if (best == null) return false;

                var nmObj = SafeReflection.GetMember(best, "name", "id");
                engineName = nmObj != null ? nmObj.ToString() : $"Özel Motor #{company.engines.Count}";
                if (string.IsNullOrWhiteSpace(engineName)) engineName = $"Özel Motor #{company.engines.Count}";
                return true;
            }
            catch { return false; }
        }

        public static bool TryDiplomacyGoodwill(Company company, RivalStudioRecord rival, out string feedback)
        {
            feedback = "";
            if (company == null || rival == null) { feedback = "Hedef stüdyo bulunamadı."; return false; }
            double wait = 12.0 - (company.currentWeek - rival.LastDiplomacyWeek);
            if (wait > 0.1) { feedback = $"{rival.Name} ile yeni hamle için ~{wait:F1} hafta bekle."; return false; }
            if (company.cash < 8000.0) { feedback = "İyi Niyet / Ortak Yayın için 8K nakit gerekiyor!"; return false; }

            company.adjustCash(-8000.0, $"Diplomasi: {rival.Name} Ortak Yayın");
            rival.LastDiplomacyWeek = company.currentWeek;
            UniverseSaveManager.AdjustRivalRelation(rival, +18);
            UniverseSaveManager.AdjustReputation(+2);
            company.adjustHype(3);

            UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                $"DOSTANE HAMLE: {rival.Name} ile Ortak Geliştirici Yayını!",
                $"İki stüdyo hayranlarla buluştu (+18 İlişki -> {rival.Relation:+#;-#;0}, +2 İtibar, +3 Hype).", "DIPLOMASI");
            UniverseSaveManager.Save();
            feedback = $"{rival.Name} ile buzlar eridi! (+18 İlişki -> {rival.Relation:+#;-#;0}, +2 İtibar)";
            return true;
        }

        public static bool TryDiplomacyPrDuel(Company company, RivalStudioRecord rival, out string feedback)
        {
            feedback = "";
            if (company == null || rival == null) { feedback = "Hedef stüdyo bulunamadı."; return false; }
            double wait = 12.0 - (company.currentWeek - rival.LastDiplomacyWeek);
            if (wait > 0.1) { feedback = $"{rival.Name} ile yeni hamle için ~{wait:F1} hafta bekle."; return false; }
            if (company.cash < 5000.0) { feedback = "PR Düellosu için 5K nakit gerekiyor!"; return false; }

            company.adjustCash(-5000.0, $"PR Düellosu: {rival.Name}");
            rival.LastDiplomacyWeek = company.currentWeek;
            UniverseSaveManager.AdjustRivalRelation(rival, -22);

            int hypeGain = 10 + UniverseSaveManager.Current.DeptPrLevel * 2;
            company.adjustHype(hypeGain);
            long fanShift = Math.Max(20, (long)Math.Round(rival.Fans * (0.03 + UniverseSaveManager.Current.DeptPrLevel * 0.01)));
            rival.Fans = Math.Max(50, rival.Fans - fanShift);
            company.adjustFans(fanShift);
            if (UniverseSaveManager.Current.DeptPrLevel == 0) UniverseSaveManager.AdjustReputation(-2);

            UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                $"PR SAVAŞI! {rival.Name} hedefte — Sosyal Medya Alev Aldı!",
                $"Sert gönderme kampanyası tuttu (+{hypeGain} Hype, +{fanShift} Hayran, İlişki -22 -> {rival.Relation:+#;-#;0}).", "REKABET");
            UniverseSaveManager.Save();
            feedback = $"{rival.Name} ile PR düellosu patladı! (+{hypeGain} Hype, +{fanShift} Hayran, İlişki -22)";
            return true;
        }

        public static bool TryLicenseEngineToRival(Company company, RivalStudioRecord rival, out string feedback)
        {
            feedback = "";
            if (company == null || rival == null) { feedback = "Hedef stüdyo bulunamadı."; return false; }
            if (rival.HasEngineLicense) { feedback = $"{rival.Name} zaten motor lisansını kullanıyor ('{rival.LicensedEngineName}')!"; return false; }
            if (rival.Relation < 25) { feedback = $"Motor lisanslamak için {rival.Name} ile ilişkin en az +25 (Dostane/Müttefik) olmalı! (Şu an: {rival.Relation:+#;-#;0})"; return false; }

            string engName;
            double tech;
            int parts;
            if (!TryGetBestPlayerEngine(company, out engName, out tech, out parts))
            {
                feedback = "Henüz kendi özel oyun motorunu üretmedin! Önce bir Özel Motor geliştir.";
                return false;
            }

            double upfront = Math.Round((12000.0 + tech * 4500.0 + parts * 1500.0) / 1000.0) * 1000.0;
            upfront = Math.Clamp(upfront, 15000.0, 150000.0);

            company.adjustCash(upfront, $"Motor Lisansı ({rival.Name}): {engName}");
            rival.HasEngineLicense = true;
            rival.LicensedEngineName = engName;
            rival.LicensedEngineTech = Math.Max(1.0, tech);
            rival.NextLicensePayoutWeek = company.currentWeek + 8.0;
            UniverseSaveManager.Current.TotalEngineLicenseIncome += upfront;
            UniverseSaveManager.AdjustRivalRelation(rival, +10);
            UniverseSaveManager.AdjustReputation(+3);

            UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                $"MOTOR LİSANSI: {rival.Name}, '{engName}' motorunu kiraladı!",
                $"+{upfront / 1000.0:F0}K peşin lisans geliri alındı; her 8 haftada bir telif akacak (+10 İlişki, +3 İtibar).", "DIPLOMASI");
            UniverseSaveManager.Save();
            feedback = $"'{engName}' motoru {rival.Name} stüdyosuna lisanslandı! (+{upfront / 1000.0:F0}K peşin + 8 haftada bir telif)";
            return true;
        }

        public static void Tick(Company company)
        {
            if (company == null) return;
            var u = UniverseSaveManager.Current;
            double week = company.currentWeek;
            bool changed = false;

            // 1. Office / Level degisim telemetrisi
            try
            {
                int curLvl = company.currentLevel;
                if (u.LastLoggedOfficeLevel != curLvl)
                {
                    LogCompanyTelemetry(company, "Seviye/Ofis Degisimi");
                    changed = true;
                }
            }
            catch { }

            // 2. Aylik (4 haftada bir) Departman Gideri + Merch & Lisans Ofisi Telif Geliri
            if (u.LastDeptTickWeek <= 0.01) u.LastDeptTickWeek = week;
            if (week - u.LastDeptTickWeek >= 4.0)
            {
                u.LastDeptTickWeek = week;
                double monthlyUpkeep = GetTotalWeeklyUpkeep() * 4.0;
                double merchIncome = EstimateMonthlyMerchPayout();

                if (monthlyUpkeep > 0.0 || merchIncome > 0.0)
                {
                    double net = merchIncome - monthlyUpkeep;
                    if (Math.Abs(net) >= 100.0)
                    {
                        string lbl = net >= 0
                            ? $"Merch & Lisans Net Geliri (+{merchIncome / 1000.0:F1}K / -{monthlyUpkeep / 1000.0:F1}K)"
                            : $"Departman Aylık Gideri (-{monthlyUpkeep / 1000.0:F1}K)";
                        company.adjustCash(net, lbl);
                    }
                    if (merchIncome > 0.0)
                    {
                        u.TotalMerchIncome += merchIncome;
                        MelonLogger.Msg($"[Faz 7.0 Merch] Aylik Figür/Cosplay telifi: +{merchIncome:F0} (Gider: -{monthlyUpkeep:F0}, Net: {net:+0;-0;0})");
                    }
                    changed = true;
                }
            }

            // 3. Aktif Motor Lisansi periyodik telif odemeleri (8 haftada bir)
            if (u.Rivals != null)
            {
                for (int i = 0; i < u.Rivals.Count; i++)
                {
                    var r = u.Rivals[i];
                    if (!r.HasEngineLicense) continue;
                    if (r.Relation <= -15)
                    {
                        r.HasEngineLicense = false;
                        r.LicensedEngineName = "";
                        UniverseSaveManager.AddRivalNews(week, r.Name,
                            $"{r.Name} motor lisans sözleşmesini feshetti!",
                            "İlişkiler gerilince stüdyo kendi motoruna döndü.", "DIPLOMASI");
                        changed = true;
                        continue;
                    }
                    if (week >= r.NextLicensePayoutWeek)
                    {
                        r.NextLicensePayoutWeek = week + 8.0;
                        double royalty = Math.Round((4500.0 + r.LicensedEngineTech * 2200.0) / 100.0) * 100.0;
                        company.adjustCash(royalty, $"Motor Telifi ({r.Name}): {r.LicensedEngineName}");
                        u.TotalEngineLicenseIncome += royalty;
                        MelonLogger.Msg($"[Faz 7.0 Motor Lisansi] {r.Name} telif odedi: +{royalty:F0} ('{r.LicensedEngineName}')");
                        changed = true;
                    }
                }
            }

            if (changed) UniverseSaveManager.Save();
        }
    }


    // =========================================================================
    // 9D. FAZ 7.1: HOLDING CAGI (YAYINCILIK + ORTAK YAPIM + SATIN ALMA / AI
    //     BIRLESME + YATIRIMCI BASKISI + STUDIO DIRECT + MULTIVERSE AWARDS + GISE)
    // =========================================================================
    public static class HoldingEndgameSystem
    {
        public static List<RivalStudioRecord> GetIndependentRivals()
        {
            var list = new List<RivalStudioRecord>();
            var rivals = UniverseSaveManager.Current.Rivals;
            if (rivals == null) return list;
            for (int i = 0; i < rivals.Count; i++)
                if (!rivals[i].IsAcquiredByPlayer && !rivals[i].IsAbsorbedByRival)
                    list.Add(rivals[i]);
            return list;
        }

        public static List<RivalStudioRecord> GetPlayerSubsidiaries()
        {
            var list = new List<RivalStudioRecord>();
            var rivals = UniverseSaveManager.Current.Rivals;
            if (rivals == null) return list;
            for (int i = 0; i < rivals.Count; i++)
                if (rivals[i].IsAcquiredByPlayer) list.Add(rivals[i]);
            return list;
        }

        public static int GetActivePublishDealCount()
        {
            var deals = UniverseSaveManager.Current.PublishingDeals;
            if (deals == null) return 0;
            int cnt = 0;
            for (int i = 0; i < deals.Count; i++)
                if (!deals[i].IsResolved) cnt++;
            return cnt;
        }

        // --- A) RAKIP SATIN ALMA & ALT STUDYO ---
        public static double CalculateAcquisitionPrice(RivalStudioRecord r)
        {
            if (r == null) return 400000.0;
            double curWeek = UniverseArchitectMod.ActiveCompany != null ? UniverseArchitectMod.ActiveCompany.currentWeek : 0.0;
            // Rakipler gelistikce, yillar ilerledikce, hit/crossover ve birlesme yaptikca sirket degerleri de katlanarak artar!
            double eraMult = 1.0 + (curWeek / 75.0);
            double qualityMult = r.AverageScore >= 8.2 ? 1.45 : (r.AverageScore >= 7.5 ? 1.18 : (r.AverageScore > 0.1 && r.AverageScore < 6.0 ? 0.78 : 1.0));
            double baseVal = 220000.0
                           + (r.Fans * 220.0)
                           + (r.CanonIntegrity * 3200.0)
                           + (r.GamesReleased * 38000.0)
                           + (r.CrossoversDone * 140000.0)
                           + (r.AbsorbedCount * 450000.0);
            double val = baseVal * eraMult * qualityMult;
            if (r.CanonIntegrity <= 65) val *= 0.72;   // Bataga saplanan studyo indirimi!
            if (r.Id == "rival_njg") val *= 1.85;      // NJG Games ozel prestij carpani!
            else if (r.Id == "rival_entakos") val *= 1.35; // Entakos Games kult prestiji!
            if (MultiverseEmpireSystem.GetActiveSectorEvent(curWeek) == SectorEventType.Recession) val *= 0.80;
            return Math.Clamp(Math.Round(val / 10000.0) * 10000.0, 280000.0, 80000000.0);
        }

        public static bool TryAcquireRival(Company company, RivalStudioRecord rival, out string feedback)
        {
            feedback = "";
            if (company == null || rival == null) { feedback = "Hedef stüdyo bulunamadı."; return false; }
            if (rival.IsAcquiredByPlayer) { feedback = $"{rival.Name} zaten senin alt stüdyon!"; return false; }
            if (rival.IsAbsorbedByRival) { feedback = $"{rival.Name} başka bir şirket tarafından yutuldu!"; return false; }

            double price = CalculateAcquisitionPrice(rival);
            if (company.cash < price)
            {
                feedback = $"{rival.Name} satın alma bedeli {price / 1000.0:F0}K! (Kasadaki nakit yetersiz)";
                return false;
            }

            company.adjustCash(-price, $"Şirket Satın Alma: {rival.Name}");
            rival.IsAcquiredByPlayer = true;
            rival.HasEngineLicense = false;
            rival.SubsidiaryDirectiveName = SubsidiaryDirective.SideGames.ToString();
            rival.NextSubsidiaryTickWeek = company.currentWeek + 12.0;

            long fanTransfer = Math.Max(40, (long)Math.Round(rival.Fans * 0.20));
            company.adjustFans(fanTransfer);
            UniverseSaveManager.AdjustReputation(+5);
            UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                $"DEV SATIN ALMA: {rival.Name} Holdingine Katıldı!",
                $"{price / 1000.0:F0}K bedelle satın alınan {rival.Name} ({rival.UniverseName}) artık senin alt stüdyon! (+{fanTransfer} Hayran, +5 İtibar)", "HOLDING");
            MultiverseEmpireSystem.ImportAcquiredStudioUniverse(company, rival, false);
            UniverseSaveManager.Save();

            feedback = $"🏛 {rival.Name} satın alındı (-{price / 1000.0:F0}K)! Holding sekmesinden talimat verebilirsin.";
            return true;
        }

        public static string CycleSubsidiaryDirective(RivalStudioRecord sub)
        {
            if (sub == null || !sub.IsAcquiredByPlayer) return "Alt stüdyo bulunamadı.";
            var cur = CategoryLabels.ParseDirective(sub.SubsidiaryDirectiveName);
            SubsidiaryDirective next = cur == SubsidiaryDirective.SideGames
                ? SubsidiaryDirective.Transmedia
                : (cur == SubsidiaryDirective.Transmedia ? SubsidiaryDirective.Profit : SubsidiaryDirective.SideGames);
            sub.SubsidiaryDirectiveName = next.ToString();
            UniverseSaveManager.Save();
            return $"{sub.Name} yeni talimatı: {CategoryLabels.DirectiveToTurkish(next)}!";
        }

        private static bool TickSubsidiaries(Company company, double week)
        {
            var subs = GetPlayerSubsidiaries();
            if (subs.Count == 0) return false;
            bool changed = false;

            for (int i = 0; i < subs.Count; i++)
            {
                var sub = subs[i];
                if (sub.NextSubsidiaryTickWeek <= 0.1) sub.NextSubsidiaryTickWeek = week + 12.0;
                if (week < sub.NextSubsidiaryTickWeek) continue;

                sub.NextSubsidiaryTickWeek = week + 12.0;
                var dir = CategoryLabels.ParseDirective(sub.SubsidiaryDirectiveName);
                var tpl = Phase1Database.FindRivalTemplate(sub.Id);
                double rnd = 0.5;
                try { rnd = company.getRandom(); } catch { }

                double score = Math.Round(Math.Clamp(tpl.BaseSkill + (rnd - 0.45) * 1.4, 5.5, 9.2), 1);
                double baseRev = Math.Round((12000.0 + score * 3000.0 + Math.Min(sub.Fans, 4000) * 5.0) / 1000.0) * 1000.0;
                sub.GamesReleased++;
                sub.LastScore = score;

                if (dir == SubsidiaryDirective.SideGames)
                {
                    int fanGain = (int)Math.Max(35, Math.Round(sub.Fans * 0.08));
                    company.adjustCash(baseRev, $"Alt Stüdyo Yan Evren ({sub.Name})");
                    company.adjustFans(fanGain);
                    UniverseSaveManager.AdjustLoreBarrier(-10);
                    sub.LastGameTitle = $"{sub.UniverseName}: Alt Proje #{sub.GamesReleased}";
                    NotificationHelper.Show(company, $"Alt Stüdyo Yayında: {sub.Name}",
                        $"'{sub.LastGameTitle}' ({score:F1}/10) çıktı! +{baseRev / 1000.0:F0}K temettü, +{fanGain} Hayran, Lore Bariyeri -10.");
                }
                else if (dir == SubsidiaryDirective.Transmedia)
                {
                    double rev = Math.Round((baseRev * 0.75) / 1000.0) * 1000.0;
                    company.adjustCash(rev, $"Alt Stüdyo Transmedya ({sub.Name})");
                    UniverseSaveManager.AdjustIntegrity(+4);
                    UniverseSaveManager.Current.PendingTransmediaHype = Math.Min(25, UniverseSaveManager.Current.PendingTransmediaHype + 6);
                    NotificationHelper.Show(company, $"Alt Stüdyo Transmedya: {sub.Name}",
                        $"{sub.Name} evren yan ürünleri üretti! +{rev / 1000.0:F0}K gelir, +4 Tutarlılık, sıradaki oyuna +6 Hype.");
                }
                else
                {
                    double profitRev = Math.Round((baseRev * 1.65) / 1000.0) * 1000.0;
                    company.adjustCash(profitRev, $"Alt Stüdyo Kâr Temettüsü ({sub.Name})");
                    NotificationHelper.Show(company, $"Alt Stüdyo Temettüsü: {sub.Name}",
                        $"{sub.Name} kâr odaklı çalışarak holding kasasına +{profitRev / 1000.0:F0}K net temettü aktardı!");
                }
                changed = true;
            }
            return changed;
        }

        // --- B) ORTAK YAPIM (STUDYO-ARASI MEGA CROSSOVER) ---
        public static bool TryStartCoProduction(Company company, RivalStudioRecord rival, out string feedback)
        {
            feedback = "";
            if (company == null || rival == null) { feedback = "Hedef stüdyo bulunamadı."; return false; }
            if (rival.IsAcquiredByPlayer || rival.IsAbsorbedByRival) { feedback = "Sadece bağımsız müttefik stüdyolarla ortak yapım kurulabilir."; return false; }

            var u = UniverseSaveManager.Current;
            if (!string.IsNullOrEmpty(u.ActiveCoProdRivalId))
            {
                feedback = $"Zaten {u.ActiveCoProdRivalName} ile aktif bir Ortak Yapım anlaşman var! Sıradaki oyununda tetiklenecek.";
                return false;
            }
            if (rival.Relation < 45)
            {
                feedback = $"Ortak Yapım (Mega Crossover) için {rival.Name} ile ilişkin en az +45 (★ Müttefik) olmalı! (Şu an: {rival.Relation:+#;-#;0})";
                return false;
            }
            if (company.cash < 25000.0)
            {
                feedback = "Ortak Yapım ön hazırlık bütçesi için 25K nakit gerekiyor!";
                return false;
            }

            company.adjustCash(-25000.0, $"Ortak Yapım Protokolü ({rival.Name})");
            u.ActiveCoProdRivalId = rival.Id;
            u.ActiveCoProdRivalName = rival.Name;
            u.ActiveCoProdUniverse = rival.UniverseName;
            UniverseSaveManager.AdjustRivalRelation(rival, +10);
            UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                $"TARİHİ ANLAŞMA: {rival.Name} ile Stüdyolar Arası Mega Crossover!",
                $"Senin evrenin ile {rival.UniverseName} bir sonraki oyununda güçlerini birleştiriyor! Sektör nefesini tuttu.", "CROSSOVER");
            UniverseSaveManager.Save();

            feedback = $"★ {rival.Name} ile Ortak Yapım imzalandı (-25K)! Sıradaki evren oyununda Mega Crossover patlayacak!";
            return true;
        }

        // --- C) YAYINCILIK (PUBLISHER) MASASI ---
        public static bool TryRequestPublishOffer(Company company, out string feedback)
        {
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }
            var u = UniverseSaveManager.Current;
            if (GetActivePublishDealCount() >= 2)
            {
                feedback = "Aynı anda en fazla 2 yayıncılık anlaşması finanse edilebilir.";
                return false;
            }
            double wait = 8.0 - (company.currentWeek - u.LastPublishOfferWeek);
            if (wait > 0.1)
            {
                feedback = $"Yeni yayıncılık başvurusu için ~{wait:F1} hafta daha bekle.";
                return false;
            }
            var candidates = GetIndependentRivals();
            if (candidates.Count == 0)
            {
                feedback = "Yayıncılık teklifi gönderecek bağımsız stüdyo kalmadı.";
                return false;
            }
            if (company.cash < 35000.0)
            {
                feedback = "Yayıncılık fonlamak için kasada en az 35K nakit bulunmalı.";
                return false;
            }

            u.LastPublishOfferWeek = company.currentWeek;
            UniverseSaveManager.Save();
            EnqueuePublishingOffer(company, candidates);
            feedback = "Bağımsız stüdyolara yayıncı çağrısı açıldı! Pencereyi kapattığında başvuru önüne gelecek.";
            return true;
        }

        private static void EnqueuePublishingOffer(Company company, List<RivalStudioRecord> candidates)
        {
            var nonHostile = new List<RivalStudioRecord>();
            for (int i = 0; i < candidates.Count; i++)
                if (CategoryLabels.RelationToStance(candidates[i].Relation) != DiplomaticStance.Hostile)
                    nonHostile.Add(candidates[i]);
            if (nonHostile.Count == 0) return;

            double r1 = 0.5;
            try { r1 = company.getRandom(); } catch { }
            var rival = nonHostile[(int)(r1 * nonHostile.Count) % nonHostile.Count];
            var tpl = Phase1Database.FindRivalTemplate(rival.Id);
            string title = $"{tpl.TitlePool[(rival.GamesReleased + UniverseSaveManager.Current.PublishingDeals.Count) % tpl.TitlePool.Length]} (Yayıncı Projesi)";

            double rawCost = 30000.0 + Math.Min(company.cash * 0.12, 90000.0);
            double invest = Math.Clamp(Math.Round(rawCost / 5000.0) * 5000.0, 30000.0, 120000.0);

            NativePopupQueue.Enqueue(
                $"Yayıncılık Başvurusu: {rival.Name}",
                $"{rival.Name} ({rival.ArchetypeStyle}), yeni oyunu '{title}' için senden yayıncılık desteği istiyor!\n\n" +
                $"• Finanse Et (-{invest / 1000.0:F0}K): 7 hafta sonra oyun çıkar. 8.0+ alırsa 2x civarı kâr payı, +4 İtibar & +16 İlişki; 6.5 altı alırsa yatırımının bir kısmı yanar!\n" +
                $"• Geri Çevir: Nakit kasada kalsın.",
                new[] { $"Finanse Et (-{invest / 1000.0:F0}K)", "Geri Çevir" },
                opt =>
                {
                    if (opt == 0)
                    {
                        if (company.cash < invest)
                        {
                            NotificationHelper.Show(company, "Yetersiz Bütçe", $"'{title}' için gereken {invest / 1000.0:F0}K kasada yok.");
                            return;
                        }
                        company.adjustCash(-invest, $"Yayıncılık Yatırımı ({rival.Name}): {title}");
                        UniverseSaveManager.Current.PublishingDeals.Add(new PublishingDealRecord
                        {
                            Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                            RivalId = rival.Id,
                            RivalName = rival.Name,
                            GameTitle = title,
                            Investment = invest,
                            StartWeek = company.currentWeek,
                            ReleaseWeek = company.currentWeek + 7.0,
                            IsResolved = false
                        });
                        UniverseSaveManager.AdjustRivalRelation(rival, +8);
                        UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                            $"YAYINCILIK ANLAŞMASI: '{title}' senin etiketinle çıkıyor!",
                            $"{invest / 1000.0:F0}K bütçe sağlandı; 7 hafta sonra raflarda (+8 İlişki).", "HOLDING");
                        UniverseSaveManager.Save();
                    }
                }
            );
        }

        private static bool TickPublishingDeals(Company company, double week)
        {
            var u = UniverseSaveManager.Current;
            bool changed = false;

            for (int i = 0; i < u.PublishingDeals.Count; i++)
            {
                var d = u.PublishingDeals[i];
                if (d == null || d.IsResolved || week < d.ReleaseWeek) continue;

                d.IsResolved = true;
                var tpl = Phase1Database.FindRivalTemplate(d.RivalId);
                var rival = UniverseSaveManager.FindRivalById(d.RivalId);
                double rnd = 0.5;
                try { rnd = company.getRandom(); } catch { }

                double score = Math.Round(Math.Clamp(tpl.BaseSkill + (rnd - 0.45) * 2.0 * tpl.Variance, 4.0, 9.6), 2);
                d.FinalScore = score;

                double mult = score >= 8.0 ? (1.85 + (score - 8.0) * 0.30) : (score >= 6.5 ? (1.20 + (score - 6.5) * 0.25) : (0.35 + (score - 4.0) * 0.12));
                double payout = Math.Round((d.Investment * mult) / 1000.0) * 1000.0;
                double netProfit = payout - d.Investment;
                d.Payout = payout;
                company.adjustCash(payout, $"Yayıncılık Kâr Payı: {d.GameTitle}");

                if (score >= 8.0)
                {
                    int fanAdd = (int)Math.Max(50, Math.Round(company.fans * 0.05));
                    company.adjustFans(fanAdd);
                    UniverseSaveManager.AdjustReputation(+4);
                    UniverseSaveManager.AdjustRivalRelation(rival, +16);
                    d.ResultSummary = $"[DEV KÂR — {score:F1}/10] +{payout / 1000.0:F0}K Dönüş (Net +{netProfit / 1000.0:F0}K), +{fanAdd} Hayran";
                    NotificationHelper.Show(company, $"Yayıncılık Zaferi: {d.GameTitle}!",
                        $"{d.RivalName} ile yayınladığın oyun {score:F1}/10 aldı! Kasaya +{payout / 1000.0:F0}K döndü (+4 İtibar, +16 İlişki).", true);
                }
                else if (score >= 6.5)
                {
                    UniverseSaveManager.AdjustReputation(+2);
                    UniverseSaveManager.AdjustRivalRelation(rival, +10);
                    d.ResultSummary = $"[KÂRLI — {score:F1}/10] +{payout / 1000.0:F0}K Dönüş (Net +{netProfit / 1000.0:F0}K)";
                    NotificationHelper.Show(company, $"Yayıncılık Tamamlandı: {d.GameTitle}",
                        $"Oyun {score:F1}/10 aldı. Kasaya +{payout / 1000.0:F0}K kâr payı yattı (+2 İtibar).");
                }
                else
                {
                    UniverseSaveManager.AdjustReputation(-3);
                    UniverseSaveManager.AdjustRivalRelation(rival, -5);
                    d.ResultSummary = $"[ZARAR — {score:F1}/10] Sadece +{payout / 1000.0:F0}K döndü (Net {netProfit / 1000.0:F0}K)";
                    NotificationHelper.Show(company, $"Yayıncılık Zararı: {d.GameTitle}",
                        $"Oyun {score:F1}/10'da kaldı! {d.Investment / 1000.0:F0}K yatırımdan sadece {payout / 1000.0:F0}K kurtarılabildi (-3 İtibar).");
                }
                changed = true;
            }

            // Periyodik otomatik yayincilik teklifi
            if (week - u.LastPublishOfferWeek >= 18.0 && company.cash >= 60000.0 && GetActivePublishDealCount() < 2 &&
                !NativePopupQueue.HasPending && !CharacterWorkshopUGUI.IsOpen && !UniverseArchitectMod.IsNativeModalOpen())
            {
                var cands = GetIndependentRivals();
                if (cands.Count > 0)
                {
                    u.LastPublishOfferWeek = week;
                    EnqueuePublishingOffer(company, cands);
                    changed = true;
                }
            }

            return changed;
        }

        // --- D) YAPAY ZEKA BIRLESMELERI (AI M&A) ---
        private static bool TickAiMergers(Company company, double week)
        {
            var u = UniverseSaveManager.Current;
            if (week < 36.0 || (week - u.LastAiMergerWeek) < 22.0) return false;

            var indep = GetIndependentRivals();
            if (indep.Count < 8) return false;

            RivalStudioRecord strongest = null;
            RivalStudioRecord weakest = null;
            for (int i = 0; i < indep.Count; i++)
            {
                var r = indep[i];
                if (strongest == null || r.Fans > strongest.Fans) strongest = r;
                if (weakest == null || r.CanonIntegrity < weakest.CanonIntegrity || (r.CanonIntegrity == weakest.CanonIntegrity && r.Fans < weakest.Fans))
                    weakest = r;
            }

            if (strongest == null || weakest == null || strongest.Id == weakest.Id) return false;
            if (strongest.Fans < 500 || weakest.CanonIntegrity > 68) return false;

            u.LastAiMergerWeek = week;
            weakest.IsAbsorbedByRival = true;
            weakest.ParentRivalName = strongest.Name;
            weakest.HasEngineLicense = false;

            long absorbedFans = Math.Max(80, (long)Math.Round(weakest.Fans * 0.75));
            strongest.Fans += absorbedFans;
            strongest.AbsorbedCount++;
            strongest.CanonIntegrity = Math.Clamp(strongest.CanonIntegrity + 4, 10, 100);

            UniverseSaveManager.AddRivalNews(week, strongest.Name,
                $"DEV TEKEL BİRLEŞMESİ: {strongest.Name}, {weakest.Name}'i Yuttu!",
                $"Zor günler geçiren {weakest.Name} satın alındı! {strongest.Name} +{absorbedFans:N0} hayranla sektör devine dönüştü.", "HOLDING");
            UniverseSaveManager.AddChronology(week, "HOLDING",
                $"🏛 Tekel Birleşmesi: {strongest.Name} ← {weakest.Name}",
                $"{strongest.Name}, bocalayan {weakest.Name}'i yuttu (+{absorbedFans:N0} Hayran).");
            NotificationHelper.Show(company, $"SEKTÖRDE DEV BİRLEŞME: {strongest.Name}!",
                $"{strongest.Name}, bocalayan {weakest.Name} stüdyosunu satın alıp bünyesine kattı! Liderlik yarışında dişli bir tekel doğdu.", true);
            return true;
        }

        // --- E) YATIRIMCI BASKISI & BAGIMSIZLIK SAVASI (GERCEK SATIS TAKIBI) ---
        public static bool TryTakeInvestorFunding(Company company, int tier, out string feedback)
        {
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }
            var u = UniverseSaveManager.Current;
            if (u.HasInvestor) { feedback = $"Zaten '{u.InvestorName}' ile aktif yatırım anlaşman var! Önce hisseleri geri al."; return false; }

            string invName;
            double cashIn, buyout, targetScore;
            int eqPct;

            if (tier == 0) { invName = "Anatolia Melek Fonu"; cashIn = 120000.0; eqPct = 15; buyout = 150000.0; targetScore = 7.2; }
            else if (tier == 1) { invName = "Apex Risk Sermayesi"; cashIn = 300000.0; eqPct = 20; buyout = 375000.0; targetScore = 7.8; }
            else { invName = "Titan Global Holding"; cashIn = 650000.0; eqPct = 25; buyout = 820000.0; targetScore = 8.2; }

            company.adjustCash(cashIn, $"Yatırımcı Fonu: {invName}");
            u.HasInvestor = true;
            u.InvestorName = invName;
            u.InvestorPrincipal = cashIn;
            u.InvestorEquityPct = eqPct;
            u.InvestorBuyoutCost = buyout;
            u.InvestorTargetScore = targetScore;
            u.InvestorTotalPaid = 0.0;
            UniverseSaveManager.AdjustReputation(-3);

            // Mevcut oyunlarin eski satislarini taban olarak kaydet ki sadece YENI satistan pay kesilsin!
            SnapshotCurrentGameSales(company);

            UniverseSaveManager.AddRivalNews(company.currentWeek, company.name ?? "Sen",
                $"YATIRIM ANLAŞMASI: {invName} (+{cashIn / 1000.0:F0}K) Ortak Oldu!",
                $"Yeni satışlardan %{eqPct} pay alacak ve oyunlarda en az {targetScore:F1} skor bekliyor.", "SIRKET");
            UniverseSaveManager.Save();

            feedback = $"💰 {invName} kasaya +{cashIn / 1000.0:F0}K aktardı! (Yeni satışlardan %{eqPct} pay, Hedef: {targetScore:F1}+)";
            return true;
        }

        public static bool TryBuyoutInvestor(Company company, out string feedback)
        {
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }
            var u = UniverseSaveManager.Current;
            if (!u.HasInvestor) { feedback = "Zaten %100 Bağımsız bir stüdyosun!"; return false; }
            if (company.cash < u.InvestorBuyoutCost)
            {
                feedback = $"Hisseleri geri almak için {u.InvestorBuyoutCost / 1000.0:F0}K nakit gerekiyor!";
                return false;
            }

            string oldName = u.InvestorName;
            double cost = u.InvestorBuyoutCost;
            company.adjustCash(-cost, $"Hisse Geri Alımı: {oldName}");
            u.HasInvestor = false;
            u.InvestorName = "";
            u.InvestorEquityPct = 0;
            UniverseSaveManager.AdjustReputation(+8);

            UniverseSaveManager.AddRivalNews(company.currentWeek, company.name ?? "Sen",
                "★ TAM BAĞIMSIZLIK İLANI! Tüm Hisseler Geri Alındı!",
                $"{oldName} ortaklığı {cost / 1000.0:F0}K ödenerek sonlandırıldı. Hayranlar bağımsız stüdyoyu alkışlıyor (+8 İtibar)!", "HOLDING");
            NotificationHelper.Show(company, "★ TAM BAĞIMSIZ STÜDYO!",
                $"{oldName} hisselerini {cost / 1000.0:F0}K karşılığında geri aldın! Artık tüm satış geliri ve yaratıcı özgürlük %100 senin (+8 İtibar).", true);
            UniverseSaveManager.Save();

            feedback = $"★ {oldName} hisseleri geri alındı! Artık %100 Bağımsız Stüdyosun (+8 İtibar)!";
            return true;
        }

        private static string GetGameSalesKey(Game g, int fallbackIdx)
        {
            if (g == null) return $"g_{fallbackIdx}";
            if (!string.IsNullOrEmpty(g.id)) return g.id;
            return $"{g.title ?? "game"}_{g.releaseWeek}_{fallbackIdx}";
        }

        private static double ReadGameCumulativeSales(Game g)
        {
            if (g == null) return 0.0;
            double s1 = 0.0, s2 = 0.0;
            try { s1 = g.totalSalesCash; } catch { }
            try { s2 = g.revenue; } catch { }
            return Math.Max(s1, s2);
        }

        private static void SnapshotCurrentGameSales(Company company)
        {
            if (company == null || company.gameLog == null) return;
            var tracks = UniverseSaveManager.Current.SalesTrack;
            for (int i = 0; i < company.gameLog.Count; i++)
            {
                var g = company.gameLog[i];
                if (g == null) continue;
                string key = GetGameSalesKey(g, i);
                double curSales = ReadGameCumulativeSales(g);
                UpsertSalesTrack(tracks, key, curSales);
            }
        }

        private static GameSalesTrackEntry FindSalesTrack(List<GameSalesTrackEntry> tracks, string key)
        {
            for (int i = 0; i < tracks.Count; i++)
                if (tracks[i].GameKey == key) return tracks[i];
            return null;
        }

        private static void UpsertSalesTrack(List<GameSalesTrackEntry> tracks, string key, double val)
        {
            var ex = FindSalesTrack(tracks, key);
            if (ex != null) ex.LastSeenSales = val;
            else tracks.Add(new GameSalesTrackEntry { GameKey = key, LastSeenSales = val });
        }

        private static bool TickInvestorSalesCut(Company company)
        {
            var u = UniverseSaveManager.Current;
            if (!u.HasInvestor || u.InvestorEquityPct <= 0 || company == null || company.gameLog == null) return false;

            double totalNewSales = 0.0;
            for (int i = 0; i < company.gameLog.Count; i++)
            {
                var g = company.gameLog[i];
                if (g == null) continue;
                string key = GetGameSalesKey(g, i);
                double curSales = ReadGameCumulativeSales(g);
                var tr = FindSalesTrack(u.SalesTrack, key);
                if (tr == null)
                {
                    u.SalesTrack.Add(new GameSalesTrackEntry { GameKey = key, LastSeenSales = curSales });
                    if (curSales > 0.0) totalNewSales += curSales;
                }
                else if (curSales > tr.LastSeenSales + 50.0)
                {
                    totalNewSales += (curSales - tr.LastSeenSales);
                    tr.LastSeenSales = curSales;
                }
            }

            if (totalNewSales >= 500.0)
            {
                double cut = Math.Round(totalNewSales * (u.InvestorEquityPct / 100.0));
                if (cut >= 100.0)
                {
                    company.adjustCash(-cut, $"Yatırımcı Satış Payı (%{u.InvestorEquityPct} — {u.InvestorName})");
                    u.InvestorTotalPaid += cut;
                    MelonLogger.Msg($"[Faz 7.1 Yatirimci] Yeni satis={totalNewSales:F0} -> %{u.InvestorEquityPct} pay kesildi: -{cut:F0} (Toplam: {u.InvestorTotalPaid:F0})");
                    return true;
                }
            }
            return false;
        }

        public static void OnPlayerGameReviewedInvestorCheck(Company company, string title, double score, List<string> notes)
        {
            var u = UniverseSaveManager.Current;
            if (!u.HasInvestor) return;

            if (score >= u.InvestorTargetScore)
            {
                UniverseSaveManager.AdjustReputation(+3);
                notes.Add($"Yatırımcı Memnun ({score:F1} ≥ {u.InvestorTargetScore:F1}, +3 İtibar)");
            }
            else
            {
                double fine = Math.Min(25000.0, Math.Round(u.InvestorPrincipal * 0.08));
                company.adjustCash(-fine, $"Yatırımcı Hedef Cezası: {u.InvestorName}");
                UniverseSaveManager.AdjustReputation(-6);
                notes.Add($"⚠ Yatırımcı Baskısı! Hedef {u.InvestorTargetScore:F1} tutmadı (-{fine / 1000.0:F0}K, -6 İtibar)");
                NotificationHelper.Show(company, $"YATIRIMCI BASKISI: {u.InvestorName}!",
                    $"'{title}' ({score:F1}/10), yatırımcının {u.InvestorTargetScore:F1} hedefinin altında kaldı! Sözleşme cezası (-{fine / 1000.0:F0}K, -6 İtibar) kesildi.", true);
            }
        }

        // --- F) STUDIO DIRECT (KENDI EVREN SUNUMUN) ---
        public static bool TryHostStudioDirect(Company company, out string feedback)
        {
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }
            var u = UniverseSaveManager.Current;

            double wait = 48.0 - (company.currentWeek - u.LastStudioDirectWeek);
            if (wait > 0.1)
            {
                feedback = $"Yıllık Studio Direct için ~{wait:F1} hafta daha bekle (yılda bir kez).";
                return false;
            }
            if (company.fans < 150)
            {
                feedback = "Studio Direct düzenlemek için en az 150 Hayran gerekiyor!";
                return false;
            }
            if (company.cash < 20000.0)
            {
                feedback = "Studio Direct prodüksiyonu için 20K nakit gerekiyor!";
                return false;
            }

            company.adjustCash(-20000.0, "Studio Direct Dijital Sunumu");
            u.LastStudioDirectWeek = company.currentWeek;
            u.PendingDirectHype = 22;
            u.HasActiveDirectPromise = true;

            long fanWave = Math.Max(60, (long)Math.Round(company.fans * 0.08));
            company.adjustFans(fanWave);
            UniverseSaveManager.AdjustReputation(+3);

            UniverseSaveManager.AddRivalNews(company.currentWeek, company.name ?? "Sen",
                $"📺 STUDIO DIRECT CANLI YAYINI! '{u.UniverseName}' Geleceği Duyuruldu!",
                $"Canlı sunum izlenme rekoru kırdı (+22 Sıradaki Oyun Hype, +{fanWave} Hayran)! Ama beklenti çok yüksek — sıradaki oyun 6.5 altı almamalı!", "HOLDING");
            NotificationHelper.Show(company, "📺 STUDIO DIRECT BÜYÜK SES GETİRDİ!",
                $"Sıradaki oyununa +22 Hype ve +{fanWave} yeni hayran kazandırdın! Dikkat: Sıradaki oyun 6.5 altı alırsa 'Fragmanla alakası yok!' tepkisi doğar.", true);
            UniverseSaveManager.Save();

            feedback = $"📺 Studio Direct tamamlandı! Sıradaki oyuna +22 Hype & +{fanWave} Hayran (6.5+ skor şart!).";
            return true;
        }

        // --- G) YILLIK MULTIVERSE AWARDS (48 HAFTADA BIR) ---
        private static bool TickAnnualAwards(Company company, double week)
        {
            var u = UniverseSaveManager.Current;
            int currentYear = (int)Math.Floor(week / 48.0);
            if (currentYear < 1 || currentYear <= u.LastAwardYear) return false;
            if (NativePopupQueue.HasPending || CharacterWorkshopUGUI.IsOpen) return false;

            u.LastAwardYear = currentYear;
            var board = RivalSimulationSystem.BuildLeaderboard(company);
            var topStudio = board.Count > 0 ? board[0] : null;
            bool playerWonUniverse = topStudio != null && topStudio.IsPlayer;
            string uniWinner = topStudio != null ? $"{topStudio.StudioName} ({topStudio.UniverseName})" : "Bilinmiyor";

            // Yilin Karakteri: Sadece bu yil (son 48 haftada) sahneye cikmis aktif ozel karakterler aday olur!
            CharacterRecord bestChar = null;
            var specials = UniverseSaveManager.GetSpecialCharacters();
            for (int i = 0; i < specials.Count; i++)
            {
                var sc = specials[i];
                if (sc.GamesAppeared <= 0) continue;
                if (week - sc.LastAppearedWeek > 52.0) continue; // Bu yil hic oyuna cikmayanlar odulu otomatik supuremez!
                if (bestChar == null || sc.Popularity > bestChar.Popularity) bestChar = sc;
            }

            var topRivalStar = MultiverseEmpireSystem.GetTopLivingRivalStar();
            int rivalBar = topRivalStar != null ? topRivalStar.Popularity : 76;
            bool playerWonChar = bestChar != null && bestChar.Popularity >= Math.Max(74, rivalBar);
            string charWinner = playerWonChar
                ? $"{bestChar.Name} ({bestChar.ArchetypeName} — Pop {bestChar.Popularity})"
                : (topRivalStar != null
                    ? $"{topRivalStar.Name} [{topRivalStar.RivalName} — Pop {topRivalStar.Popularity}]"
                    : $"{(topStudio != null && !topStudio.IsPlayer ? topStudio.StudioName : "NJG Games")} Yıldızı");

            var rewardNotes = new List<string>();
            if (playerWonUniverse)
            {
                u.TrophyCount++;
                double prize = 40000.0 + currentYear * 10000.0;
                long fanPrize = Math.Max(100, (long)Math.Round(company.fans * 0.10));
                company.adjustCash(prize, $"Multiverse Awards Yıl {currentYear}: Yılın Evreni!");
                company.adjustFans(fanPrize);
                UniverseSaveManager.AdjustReputation(+6);
                rewardNotes.Add($"🏆 Yılın Evreni (+{prize / 1000.0:F0}K, +{fanPrize} Hayran, +6 İtibar)");
            }
            if (playerWonChar)
            {
                u.TrophyCount++;
                company.adjustCash(25000.0, $"Multiverse Awards Yıl {currentYear}: Yılın Karakteri ({bestChar.Name})");
                bestChar.Popularity = PopularitySystem.ApplyDelta(bestChar.Popularity, 9);
                UniverseSaveManager.AdjustReputation(+4);
                rewardNotes.Add($"🌟 Yılın Karakteri: {bestChar.Name} (+25K, +5 Pop, +4 İtibar)");
            }

            string summary = rewardNotes.Count > 0 ? string.Join(" | ", rewardNotes) : "Bu yıl kupalar rakiplere gitti — seneye intikam zamanı!";
            u.AwardsHistory.Add(new AwardEntry
            {
                Year = currentYear,
                UniverseWinner = uniWinner,
                CharacterWinner = charWinner,
                PlayerWonUniverse = playerWonUniverse,
                PlayerWonCharacter = playerWonChar,
                Summary = summary
            });

            UniverseSaveManager.AddRivalNews(week, "Multiverse Awards",
                $"🏆 YIL {currentYear} MULTIVERSE AWARDS SONUÇLARI!",
                $"Yılın Evreni: {uniWinner} | Yılın Karakteri: {charWinner}", "HIT");
            UniverseSaveManager.AddChronology(week, "ODUL",
                $"🏆 Yıl {currentYear} Multiverse Awards",
                $"Evren: {uniWinner} | Karakter: {charWinner}");
            NotificationHelper.Show(company, $"🏆 MULTIVERSE AWARDS — YIL {currentYear}!",
                $"Yılın Evreni: {uniWinner}\nYılın Karakteri: {charWinner}\n\n{summary}", true);
            return true;
        }

        // --- H) GISE CARPISMASI (RELEASE DATE CLASH) ---
        public static void CheckBoxOfficeClash(Company company, string title, double playerScore, ref int modHype, ref int hypePenalty, List<string> notes)
        {
            if (company == null) return;
            double week = company.currentWeek;
            var indep = GetIndependentRivals();

            RivalStudioRecord clashRival = null;
            for (int i = 0; i < indep.Count; i++)
            {
                var r = indep[i];
                var st = CategoryLabels.RelationToStance(r.Relation);
                if (st != DiplomaticStance.Hostile && st != DiplomaticStance.Tense) continue;
                if (Math.Abs(week - r.LastReleaseWeek) <= 5.0 || Math.Abs(week - r.NextReleaseWeek) <= 5.0)
                {
                    clashRival = r;
                    break;
                }
            }
            if (clashRival == null) return;

            double rivalBench = clashRival.LastScore > 0.1 ? clashRival.LastScore : Phase1Database.FindRivalTemplate(clashRival.Id).BaseSkill;
            if (playerScore >= rivalBench)
            {
                modHype += 12;
                long wonFans = Math.Max(50, (long)Math.Round(clashRival.Fans * 0.08));
                clashRival.Fans = Math.Max(50, clashRival.Fans - wonFans);
                company.adjustFans(wonFans);
                UniverseSaveManager.AdjustReputation(+3);
                UniverseSaveManager.AdjustRivalRelation(clashRival, -8);
                notes.Add($"⚔ Gişe Çarpışması Zaferi ({clashRival.Name} ezildi: +12 Hype, +{wonFans} Hayran)");
                UniverseSaveManager.AddRivalNews(week, clashRival.Name,
                    $"⚔ GİŞE ÇARPIŞMASI: '{title}', {clashRival.Name}'i Sandığa Gömdü!",
                    $"Aynı dönemde çıkan iki rakip çarpıştı ve {playerScore:F1} puanla haftanın tüm pastasını sen aldın (+{wonFans} Hayran)!", "REKABET");
            }
            else
            {
                hypePenalty += 8;
                UniverseSaveManager.AdjustReputation(-2);
                notes.Add($"⚔ Gişe Çarpışması Kaybı ({clashRival.Name} haftayı kaptı: -8 Hype)");
                UniverseSaveManager.AddRivalNews(week, clashRival.Name,
                    $"⚔ GİŞE ÇARPIŞMASI: {clashRival.Name} Çıkış Haftasını Çaldı!",
                    $"{clashRival.Name} ({rivalBench:F1}) aynı hafta gündemi ele geçirip '{title}' gölgede bıraktı (-8 Hype).", "REKABET");
            }
        }

        public static void Tick(Company company)
        {
            if (company == null) return;
            double week = company.currentWeek;
            bool changed = false;

            if (TickSubsidiaries(company, week)) changed = true;
            if (TickPublishingDeals(company, week)) changed = true;
            if (TickAiMergers(company, week)) changed = true;
            if (TickInvestorSalesCut(company)) changed = true;
            if (TickAnnualAwards(company, week)) changed = true;

            if (changed) UniverseSaveManager.Save();
        }
    }


    // =========================================================================
    // 9E. FAZ 9.0: COKLU IP, SATIN ALINAN EVRENE TAM ERISIM (18 TENTIFORVERSE +
    //     5 ENTAVERSE + 44 RAKIP YILDIZI), TERSINE KONUK DLC, SAGA FINALI,
    //     SEKTOR DEPREMLERI & ANTI-TEKEL ITTIFAKI
    // =========================================================================
    public static class MultiverseEmpireSystem
    {
        public static readonly Dictionary<string, RivalStarSeed[]> RivalRosterSeeds = new Dictionary<string, RivalStarSeed[]>(StringComparer.OrdinalIgnoreCase)
        {
            // NJG GAMES — TENTIFORVERSE TAM KADRO (22 KARAKTER | FANDOM: TentiTaym)
            ["rival_njg"] = new[]
            {
                // Kutuphane & Ana Cekirdek
                new RivalStarSeed { Name = "Tarı",        ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Kütüphane Muhafızları", StartPop = 76, CustomAxes = new[] { 0.25f, 0.35f, 0.62f, 0.14f, 0.22f, 0.60f, 0.80f, 0.78f } },
                new RivalStarSeed { Name = "Necale",      ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Kütüphane Muhafızları", StartPop = 78, CustomAxes = new[] { 0.52f, 0.42f, 0.68f, 0.18f, 0.30f, 0.62f, 0.58f, 0.82f } },
                new RivalStarSeed { Name = "Feil",        ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "Kütüphane Muhafızları", StartPop = 82, CustomAxes = new[] { 0.22f, 0.28f, 0.34f, 0.20f, 0.42f, 0.94f, 0.88f, 0.66f } },
                new RivalStarSeed { Name = "Afor",        ArchetypeId = "dark_antihero",       Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Evren Gezginleri",      StartPop = 80, CustomAxes = new[] { 0.72f, 0.24f, 0.38f, 0.36f, 0.66f, 0.85f, 0.54f, 0.52f } },
                new RivalStarSeed { Name = "Yejen",       ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Evren Gezginleri",      StartPop = 77, CustomAxes = new[] { 0.70f, 0.58f, 0.62f, 0.40f, 0.80f, 0.78f, 0.34f, 0.68f } },
                new RivalStarSeed { Name = "Saek Luyot",  ArchetypeId = "tragic_survivor",     Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "Evren Gezginleri",      StartPop = 75, CustomAxes = new[] { 0.48f, 0.16f, 0.42f, 0.24f, 0.44f, 0.52f, 0.38f, 0.44f } },
                new RivalStarSeed { Name = "Kyldo Zarın", ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "Evren Gezginleri",      StartPop = 79, CustomAxes = new[] { 0.38f, 0.32f, 0.36f, 0.26f, 0.54f, 0.88f, 0.84f, 0.58f } },
                new RivalStarSeed { Name = "Dost",        ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Evren Gezginleri",      StartPop = 74, CustomAxes = new[] { 0.42f, 0.78f, 0.86f, 0.18f, 0.44f, 0.62f, 0.32f, 0.86f } },
                // Zaman & Kozmik Gucler
                new RivalStarSeed { Name = "Rıpep",       ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Zaman & Kozmos",        StartPop = 79, CustomAxes = new[] { 0.62f, 0.22f, 0.40f, 0.88f, 0.76f, 0.96f, 0.82f, 0.42f } },
                new RivalStarSeed { Name = "Nıraz",       ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Zaman & Kozmos",        StartPop = 74, CustomAxes = new[] { 0.82f, 0.24f, 0.20f, 0.46f, 0.58f, 0.72f, 0.84f, 0.24f } },
                new RivalStarSeed { Name = "L25",         ArchetypeId = "mystery_strategist",  Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Zaman & Kozmos",        StartPop = 76, CustomAxes = new[] { 0.26f, 0.32f, 0.30f, 0.28f, 0.36f, 0.90f, 0.90f, 0.50f } },
                new RivalStarSeed { Name = "Yaşam",       ArchetypeId = "wise_mentor",         Role = CharacterRole.Minor,       Trait = CharacterTrait.CanonAnchor,    TeamName = "Zaman & Kozmos",        StartPop = 78, CustomAxes = new[] { 0.30f, 0.46f, 0.55f, 0.12f, 0.24f, 0.98f, 0.78f, 0.60f } },
                new RivalStarSeed { Name = "Kaelan",      ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Zaman & Kozmos",        StartPop = 73, CustomAxes = new[] { 0.68f, 0.26f, 0.32f, 0.70f, 0.86f, 0.84f, 0.48f, 0.22f } },
                new RivalStarSeed { Name = "Gri",         ArchetypeId = "mystery_strategist",  Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Zaman & Kozmos",        StartPop = 72, CustomAxes = new[] { 0.34f, 0.28f, 0.26f, 0.50f, 0.56f, 0.78f, 0.86f, 0.40f } },
                new RivalStarSeed { Name = "EvrenGezer",  ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "Evren Gezginleri",      StartPop = 76, CustomAxes = new[] { 0.58f, 0.64f, 0.60f, 0.28f, 0.74f, 0.92f, 0.56f, 0.58f } },
                // Gorevliler & Derin Kadro
                new RivalStarSeed { Name = "Zafkı Eşci",  ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Evren Gezginleri",      StartPop = 71, CustomAxes = new[] { 0.52f, 0.72f, 0.64f, 0.56f, 0.84f, 0.68f, 0.42f, 0.54f } },
                new RivalStarSeed { Name = "Egir",        ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Kütüphane Muhafızları", StartPop = 70, CustomAxes = new[] { 0.78f, 0.26f, 0.28f, 0.36f, 0.48f, 0.66f, 0.78f, 0.46f } },
                new RivalStarSeed { Name = "Pileg",       ArchetypeId = "dark_antihero",       Role = CharacterRole.Minor,       Trait = CharacterTrait.MysteriousPast, TeamName = "Zaman & Kozmos",        StartPop = 69, CustomAxes = new[] { 0.60f, 0.22f, 0.30f, 0.58f, 0.70f, 0.74f, 0.62f, 0.34f } },
                new RivalStarSeed { Name = "Zakum",       ArchetypeId = "cold_hunter",         Role = CharacterRole.Minor,       Trait = CharacterTrait.LocationBound,  TeamName = "Kütüphane Muhafızları", StartPop = 66, CustomAxes = new[] { 0.76f, 0.28f, 0.26f, 0.34f, 0.28f, 0.58f, 0.76f, 0.62f } },
                new RivalStarSeed { Name = "Obenas",      ArchetypeId = "classic_hero",        Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Kütüphane Muhafızları", StartPop = 65, CustomAxes = new[] { 0.62f, 0.48f, 0.64f, 0.22f, 0.24f, 0.56f, 0.58f, 0.74f } },
                new RivalStarSeed { Name = "Naleg",       ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Kütüphane Muhafızları", StartPop = 67, CustomAxes = new[] { 0.44f, 0.74f, 0.78f, 0.30f, 0.48f, 0.54f, 0.36f, 0.72f } },
                new RivalStarSeed { Name = "Lebga",       ArchetypeId = "dark_antihero",       Role = CharacterRole.Minor,       Trait = CharacterTrait.MysteriousPast, TeamName = "Zaman & Kozmos",        StartPop = 69, CustomAxes = new[] { 0.64f, 0.20f, 0.24f, 0.54f, 0.68f, 0.80f, 0.66f, 0.30f } }
            },

            // ENTAKOS GAMES — ENTAVERSE TAM KADRO (SADECE ISTENEN 9 KISI | FANDOM: EntaTaym)
            ["rival_entakos"] = new[]
            {
                new RivalStarSeed { Name = "Kuriow",              ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "EntaVerse", StartPop = 81 },
                new RivalStarSeed { Name = "Curio",               ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "EntaVerse", StartPop = 79 },
                new RivalStarSeed { Name = "Çonça",               ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "EntaVerse", StartPop = 76 },
                new RivalStarSeed { Name = "Fenta",               ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "EntaVerse", StartPop = 80 },
                new RivalStarSeed { Name = "Alzhe&İmer Kardeşler",ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "EntaVerse", StartPop = 77 },
                new RivalStarSeed { Name = "LarpTakos",           ArchetypeId = "dark_antihero",       Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "EntaVerse", StartPop = 78 },
                new RivalStarSeed { Name = "maykıl",              ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "EntaVerse", StartPop = 75 },
                new RivalStarSeed { Name = "Küriyo",              ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "EntaVerse", StartPop = 76 },
                new RivalStarSeed { Name = "Negar",               ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "EntaVerse", StartPop = 79 }
            },

            // DIGER 22 RAKIP STUDYONUN TAM KADROLARI (HER BIRINDE 5 OZEL KARAKTER!)
            ["rival_megacorp"] = new[]
            {
                new RivalStarSeed { Name = "Kral Valerius",  ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Hyperion Birliği", StartPop = 74 },
                new RivalStarSeed { Name = "Lord Malakor",   ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Hyperion Birliği", StartPop = 71 },
                new RivalStarSeed { Name = "Leydi Seraphina",ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Hyperion Birliği", StartPop = 70 },
                new RivalStarSeed { Name = "Sör Aldric",     ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Hyperion Birliği", StartPop = 67 },
                new RivalStarSeed { Name = "Kâhin Orlen",    ArchetypeId = "mystery_strategist",  Role = CharacterRole.Minor,       Trait = CharacterTrait.MysteriousPast, TeamName = "Hyperion Birliği", StartPop = 65 }
            },
            ["rival_pixelforge"] = new[]
            {
                new RivalStarSeed { Name = "Fenerci Elara",  ArchetypeId = "tragic_survivor",     Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Kül Gezginleri",   StartPop = 76 },
                new RivalStarSeed { Name = "Pusula Ustası",  ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Kül Gezginleri",   StartPop = 71 },
                new RivalStarSeed { Name = "Kül Çocuğu Milo",ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Kül Gezginleri",   StartPop = 72 },
                new RivalStarSeed { Name = "Sis Kaptanı",    ArchetypeId = "dark_antihero",       Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Kül Gezginleri",   StartPop = 69 },
                new RivalStarSeed { Name = "Bahçıvan Nera",  ArchetypeId = "classic_hero",        Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Kül Gezginleri",   StartPop = 66 }
            },
            ["rival_quantum"] = new[]
            {
                new RivalStarSeed { Name = "Komutan Vektör", ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Protokol-9",       StartPop = 73 },
                new RivalStarSeed { Name = "Ajan Sıfır",     ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Protokol-9",       StartPop = 71 },
                new RivalStarSeed { Name = "Dr. Kuantum",    ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Protokol-9",       StartPop = 68 },
                new RivalStarSeed { Name = "Siber İsyancı K",ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.SceneStealer,   TeamName = "Protokol-9",       StartPop = 70 },
                new RivalStarSeed { Name = "Operatör Vega",  ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Protokol-9",       StartPop = 65 }
            },
            ["rival_kaos"] = new[]
            {
                new RivalStarSeed { Name = "Kaptan Ördek",   ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "Kaos Ekibi",       StartPop = 74 },
                new RivalStarSeed { Name = "Çılgın Kurye",   ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Kaos Ekibi",       StartPop = 70 },
                new RivalStarSeed { Name = "Profesör Çay",   ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Kaos Ekibi",       StartPop = 68 },
                new RivalStarSeed { Name = "Doktor Ters",    ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Kaos Ekibi",       StartPop = 69 },
                new RivalStarSeed { Name = "Tost Robotu",    ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Kaos Ekibi",       StartPop = 67 }
            },
            ["rival_obsidian"] = new[]
            {
                new RivalStarSeed { Name = "Kül Şövalyesi",  ArchetypeId = "dark_antihero",       Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Karanlık Taç",     StartPop = 77 },
                new RivalStarSeed { Name = "Kızıl Rahibe",   ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Karanlık Taç",     StartPop = 73 },
                new RivalStarSeed { Name = "Taçsız Prens",   ArchetypeId = "tragic_survivor",     Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Karanlık Taç",     StartPop = 72 },
                new RivalStarSeed { Name = "Çan Muhafızı",   ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Karanlık Taç",     StartPop = 68 },
                new RivalStarSeed { Name = "Kuzgun Cadısı",  ArchetypeId = "mystery_strategist",  Role = CharacterRole.Minor,       Trait = CharacterTrait.MysteriousPast, TeamName = "Karanlık Taç",     StartPop = 67 }
            },
            ["rival_sakura"] = new[]
            {
                new RivalStarSeed { Name = "Renji Aether",   ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "Aether Akademi",   StartPop = 76 },
                new RivalStarSeed { Name = "Yuki Şimşek",    ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Aether Akademi",   StartPop = 74 },
                new RivalStarSeed { Name = "Sensei Haruto",  ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Aether Akademi",   StartPop = 71 },
                new RivalStarSeed { Name = "Kuro Kılıç",     ArchetypeId = "dark_antihero",       Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Aether Akademi",   StartPop = 73 },
                new RivalStarSeed { Name = "Hana Sakura",    ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Aether Akademi",   StartPop = 69 }
            },
            ["rival_ironclad"] = new[]
            {
                new RivalStarSeed { Name = "Çavuş Demir",    ArchetypeId = "cold_hunter",         Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Çelik Tugay",      StartPop = 72 },
                new RivalStarSeed { Name = "General Kül",    ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.CanonAnchor,    TeamName = "Çelik Tugay",      StartPop = 69 },
                new RivalStarSeed { Name = "Mühendis Kaya",  ArchetypeId = "mystery_strategist",  Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Çelik Tugay",      StartPop = 67 },
                new RivalStarSeed { Name = "Keskin Nişancı", ArchetypeId = "dark_antihero",       Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Çelik Tugay",      StartPop = 68 },
                new RivalStarSeed { Name = "Konvoy Şoförü",  ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Çelik Tugay",      StartPop = 64 }
            },
            ["rival_velvet"] = new[]
            {
                new RivalStarSeed { Name = "Yüzsüz Tanık",   ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Kâbus Arşivi",     StartPop = 75 },
                new RivalStarSeed { Name = "Uyurgezer Mia",  ArchetypeId = "tragic_survivor",     Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Kâbus Arşivi",     StartPop = 72 },
                new RivalStarSeed { Name = "Kasetçi Selim",  ArchetypeId = "mystery_strategist",  Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Kâbus Arşivi",     StartPop = 69 },
                new RivalStarSeed { Name = "Koridor Bekçisi",ArchetypeId = "cold_hunter",         Role = CharacterRole.Villain,     Trait = CharacterTrait.LocationBound,  TeamName = "Kâbus Arşivi",     StartPop = 68 },
                new RivalStarSeed { Name = "Aynadaki Çocuk", ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Kâbus Arşivi",     StartPop = 67 }
            },
            ["rival_nova"] = new[]
            {
                new RivalStarSeed { Name = "Amiral Lyra",    ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Galaksi Konseyi",  StartPop = 75 },
                new RivalStarSeed { Name = "Elçi Orion",     ArchetypeId = "mystery_strategist",  Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Galaksi Konseyi",  StartPop = 71 },
                new RivalStarSeed { Name = "Kaptan Solis",   ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Galaksi Konseyi",  StartPop = 70 },
                new RivalStarSeed { Name = "Lord Nebula",    ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.CanonAnchor,    TeamName = "Galaksi Konseyi",  StartPop = 72 },
                new RivalStarSeed { Name = "Droid V-9",      ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Galaksi Konseyi",  StartPop = 68 }
            },
            ["rival_pebble"] = new[]
            {
                new RivalStarSeed { Name = "Fırıncı Momo",   ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Yosunlu Kasaba",   StartPop = 74 },
                new RivalStarSeed { Name = "Postacı Pipo",   ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Yosunlu Kasaba",   StartPop = 70 },
                new RivalStarSeed { Name = "Çaycı Nine",     ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Yosunlu Kasaba",   StartPop = 69 },
                new RivalStarSeed { Name = "Kurnaz Tilki",   ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Villain,     Trait = CharacterTrait.SceneStealer,   TeamName = "Yosunlu Kasaba",   StartPop = 67 },
                new RivalStarSeed { Name = "Göl Balıkçısı",  ArchetypeId = "classic_hero",        Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Yosunlu Kasaba",   StartPop = 65 }
            },
            ["rival_valkyrie"] = new[]
            {
                new RivalStarSeed { Name = "Freya Mızrak",   ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Kuzey Klanı",      StartPop = 76 },
                new RivalStarSeed { Name = "Buzul Loki",     ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Villain,     Trait = CharacterTrait.SceneStealer,   TeamName = "Kuzey Klanı",      StartPop = 73 },
                new RivalStarSeed { Name = "Savaşçı Bjorn",  ArchetypeId = "dark_antihero",       Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Kuzey Klanı",      StartPop = 71 },
                new RivalStarSeed { Name = "Runik Bilge",    ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Kuzey Klanı",      StartPop = 68 },
                new RivalStarSeed { Name = "Fırtına Kuzgunu",ArchetypeId = "cold_hunter",         Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Kuzey Klanı",      StartPop = 66 }
            },
            ["rival_neonjackal"] = new[]
            {
                new RivalStarSeed { Name = "Krom Çakal",     ArchetypeId = "dark_antihero",       Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Neon 2099",        StartPop = 75 },
                new RivalStarSeed { Name = "Veri Hayaleti",  ArchetypeId = "mystery_strategist",  Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Neon 2099",        StartPop = 72 },
                new RivalStarSeed { Name = "Siber Kraliçe",  ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.SceneStealer,   TeamName = "Neon 2099",        StartPop = 71 },
                new RivalStarSeed { Name = "Lazer Kurye",    ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Neon 2099",        StartPop = 68 },
                new RivalStarSeed { Name = "Tamirci Jin",    ArchetypeId = "wise_mentor",         Role = CharacterRole.Minor,       Trait = CharacterTrait.LocationBound,  TeamName = "Neon 2099",        StartPop = 65 }
            },
            ["rival_clockwork"] = new[]
            {
                new RivalStarSeed { Name = "Usta Zemberek",  ArchetypeId = "wise_mentor",         Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Saatçi Loncası",   StartPop = 72 },
                new RivalStarSeed { Name = "Kaptan Pirinç",  ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Saatçi Loncası",   StartPop = 70 },
                new RivalStarSeed { Name = "Leydi Buhar",    ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Saatçi Loncası",   StartPop = 69 },
                new RivalStarSeed { Name = "Baron Çark",     ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Saatçi Loncası",   StartPop = 68 },
                new RivalStarSeed { Name = "Bakır Otomat",   ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Saatçi Loncası",   StartPop = 66 }
            },
            ["rival_titan"] = new[]
            {
                new RivalStarSeed { Name = "Altın Kalkan",   ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.CanonAnchor,    TeamName = "Titan Birliği",    StartPop = 77 },
                new RivalStarSeed { Name = "Mega Gölge",     ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.SceneStealer,   TeamName = "Titan Birliği",    StartPop = 74 },
                new RivalStarSeed { Name = "Yıldırım Kız",   ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Titan Birliği",    StartPop = 72 },
                new RivalStarSeed { Name = "Demir Yumruk",   ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Titan Birliği",    StartPop = 70 },
                new RivalStarSeed { Name = "Komutan Titan",  ArchetypeId = "wise_mentor",         Role = CharacterRole.Minor,       Trait = CharacterTrait.CanonAnchor,    TeamName = "Titan Birliği",    StartPop = 68 }
            },
            ["rival_lumen"] = new[]
            {
                new RivalStarSeed { Name = "Dedektif Sarp",  ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "Noir Bürosu",      StartPop = 76 },
                new RivalStarSeed { Name = "Siyah Eldiven",  ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Noir Bürosu",      StartPop = 72 },
                new RivalStarSeed { Name = "Gazeteci Vera",  ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Noir Bürosu",      StartPop = 71 },
                new RivalStarSeed { Name = "Savcı Kaan",     ArchetypeId = "classic_hero",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Noir Bürosu",      StartPop = 68 },
                new RivalStarSeed { Name = "Gece Barmeni",   ArchetypeId = "wise_mentor",         Role = CharacterRole.Minor,       Trait = CharacterTrait.LocationBound,  TeamName = "Noir Bürosu",      StartPop = 66 }
            },
            ["rival_bosphorus"] = new[]
            {
                new RivalStarSeed { Name = "Sarnıç Bekçisi", ArchetypeId = "dark_antihero",       Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Yedi Tepe",        StartPop = 74 },
                new RivalStarSeed { Name = "Haritacı Reis",  ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Yedi Tepe",        StartPop = 71 },
                new RivalStarSeed { Name = "Kubbe Mimarı",   ArchetypeId = "mystery_strategist",  Role = CharacterRole.Sidekick,    Trait = CharacterTrait.MysteriousPast, TeamName = "Yedi Tepe",        StartPop = 69 },
                new RivalStarSeed { Name = "Gölge Kervancı", ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Yedi Tepe",        StartPop = 70 },
                new RivalStarSeed { Name = "Fenerci Yusuf",  ArchetypeId = "classic_hero",        Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Yedi Tepe",        StartPop = 66 }
            },
            ["rival_turbodrift"] = new[]
            {
                new RivalStarSeed { Name = "Nitro Kemal",    ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "Asfalt Takımı",    StartPop = 72 },
                new RivalStarSeed { Name = "Kızıl Egzoz",    ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Villain,     Trait = CharacterTrait.SceneStealer,   TeamName = "Asfalt Takımı",    StartPop = 69 },
                new RivalStarSeed { Name = "Viraj Melisa",   ArchetypeId = "classic_hero",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.LocationBound,  TeamName = "Asfalt Takımı",    StartPop = 70 },
                new RivalStarSeed { Name = "Usta Tamirci",   ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Asfalt Takımı",    StartPop = 66 },
                new RivalStarSeed { Name = "Gece Spikeri",   ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Asfalt Takımı",    StartPop = 65 }
            },
            ["rival_gloom"] = new[]
            {
                new RivalStarSeed { Name = "Simyacı Vesper", ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "Sisli Fener",      StartPop = 74 },
                new RivalStarSeed { Name = "Mühürlü Ruh",    ArchetypeId = "tragic_survivor",     Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Sisli Fener",      StartPop = 70 },
                new RivalStarSeed { Name = "Gölge Dekanı",   ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Sisli Fener",      StartPop = 71 },
                new RivalStarSeed { Name = "Fener Öğrencisi",ArchetypeId = "classic_hero",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Sisli Fener",      StartPop = 68 },
                new RivalStarSeed { Name = "Konuşan Kedi",   ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Minor,       Trait = CharacterTrait.SceneStealer,   TeamName = "Sisli Fener",      StartPop = 69 }
            },
            ["rival_starlight"] = new[]
            {
                new RivalStarSeed { Name = "Korsan Vega",    ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "Yıldız Kaçakları", StartPop = 73 },
                new RivalStarSeed { Name = "Asteroit Jax",   ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Yıldız Kaçakları", StartPop = 69 },
                new RivalStarSeed { Name = "Kaptan Zara",    ArchetypeId = "dark_antihero",       Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "Yıldız Kaçakları", StartPop = 71 },
                new RivalStarSeed { Name = "Amiral Gümrük",  ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.CanonAnchor,    TeamName = "Yıldız Kaçakları", StartPop = 68 },
                new RivalStarSeed { Name = "Han İşletmecisi",ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Minor,       Trait = CharacterTrait.LocationBound,  TeamName = "Yıldız Kaçakları", StartPop = 65 }
            },
            ["rival_midas"] = new[]
            {
                new RivalStarSeed { Name = "Baron Midas",    ArchetypeId = "charismatic_villain", Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "Altın Kasa",       StartPop = 71 },
                new RivalStarSeed { Name = "Elmas Ajan",     ArchetypeId = "cold_hunter",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.None,           TeamName = "Altın Kasa",       StartPop = 67 },
                new RivalStarSeed { Name = "VIP Kraliçe",    ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Sidekick,    Trait = CharacterTrait.SceneStealer,   TeamName = "Altın Kasa",       StartPop = 68 },
                new RivalStarSeed { Name = "Kasa Hırsızı",   ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "Altın Kasa",       StartPop = 69 },
                new RivalStarSeed { Name = "Borsa Botu",     ArchetypeId = "mystery_strategist",  Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Altın Kasa",       StartPop = 64 }
            },
            ["rival_karpuz"] = new[]
            {
                new RivalStarSeed { Name = "Bakkal Rex",     ArchetypeId = "cheerful_mascot",     Role = CharacterRole.Protagonist, Trait = CharacterTrait.SceneStealer,   TeamName = "Mahalle Gücü",     StartPop = 73 },
                new RivalStarSeed { Name = "Mamut Kurye",    ArchetypeId = "rebel_outlaw",        Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "Mahalle Gücü",     StartPop = 69 },
                new RivalStarSeed { Name = "Ateşçi Dede",    ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "Mahalle Gücü",     StartPop = 68 },
                new RivalStarSeed { Name = "Kral T-Rex",     ArchetypeId = "charismatic_villain", Role = CharacterRole.Villain,     Trait = CharacterTrait.SceneStealer,   TeamName = "Mahalle Gücü",     StartPop = 70 },
                new RivalStarSeed { Name = "Karpuzcu Can",   ArchetypeId = "classic_hero",        Role = CharacterRole.Minor,       Trait = CharacterTrait.None,           TeamName = "Mahalle Gücü",     StartPop = 66 }
            },
            ["rival_aetherium"] = new[]
            {
                new RivalStarSeed { Name = "Döngü Mimarı",   ArchetypeId = "mystery_strategist",  Role = CharacterRole.Protagonist, Trait = CharacterTrait.MysteriousPast, TeamName = "7. Boyut",         StartPop = 76 },
                new RivalStarSeed { Name = "Paralel Yankı",  ArchetypeId = "chaotic_trickster",   Role = CharacterRole.Villain,     Trait = CharacterTrait.MysteriousPast, TeamName = "7. Boyut",         StartPop = 73 },
                new RivalStarSeed { Name = "Rüya Bekçisi",   ArchetypeId = "wise_mentor",         Role = CharacterRole.Sidekick,    Trait = CharacterTrait.CanonAnchor,    TeamName = "7. Boyut",         StartPop = 71 },
                new RivalStarSeed { Name = "Kırık Saatçi",   ArchetypeId = "tragic_survivor",     Role = CharacterRole.Sidekick,    Trait = CharacterTrait.LocationBound,  TeamName = "7. Boyut",         StartPop = 70 },
                new RivalStarSeed { Name = "Sıfır Gözlemci", ArchetypeId = "cold_hunter",         Role = CharacterRole.Minor,       Trait = CharacterTrait.MysteriousPast, TeamName = "7. Boyut",         StartPop = 68 }
            }
        };

        public static readonly Dictionary<string, RivalBondSeed[]> RivalBondSeeds = new Dictionary<string, RivalBondSeed[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["rival_njg"] = new[]
            {
                new RivalBondSeed { NameA = "Afor",        NameB = "Yejen",  Type = CharacterBondType.Comrades },
                new RivalBondSeed { NameA = "Tarı",        NameB = "Necale", Type = CharacterBondType.MentorStudent },
                new RivalBondSeed { NameA = "Feil",        NameB = "Rıpep",  Type = CharacterBondType.ArchRivalry },
                new RivalBondSeed { NameA = "Afor",        NameB = "Necale", Type = CharacterBondType.Comrades },
                new RivalBondSeed { NameA = "Kaelan",      NameB = "Nıraz",  Type = CharacterBondType.ArchRivalry },
                new RivalBondSeed { NameA = "Kyldo Zarın", NameB = "Dost",   Type = CharacterBondType.Comrades }
            },
            ["rival_entakos"] = new[]
            {
                new RivalBondSeed { NameA = "Kuriow",               NameB = "Curio",  Type = CharacterBondType.ArchRivalry },
                new RivalBondSeed { NameA = "Fenta",                NameB = "Negar",  Type = CharacterBondType.ArchRivalry },
                new RivalBondSeed { NameA = "LarpTakos",            NameB = "Çonça",  Type = CharacterBondType.Comrades },
                new RivalBondSeed { NameA = "Küriyo",               NameB = "maykıl", Type = CharacterBondType.Comrades },
                new RivalBondSeed { NameA = "Alzhe&İmer Kardeşler", NameB = "Kuriow", Type = CharacterBondType.MentorStudent }
            },
            ["rival_megacorp"]   = new[] { new RivalBondSeed { NameA = "Kral Valerius",  NameB = "Lord Malakor",   Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Kral Valerius",  NameB = "Leydi Seraphina", Type = CharacterBondType.Comrades } },
            ["rival_pixelforge"] = new[] { new RivalBondSeed { NameA = "Pusula Ustası",  NameB = "Fenerci Elara",  Type = CharacterBondType.MentorStudent }, new RivalBondSeed { NameA = "Fenerci Elara", NameB = "Kül Çocuğu Milo", Type = CharacterBondType.Comrades } },
            ["rival_quantum"]    = new[] { new RivalBondSeed { NameA = "Komutan Vektör", NameB = "Siber İsyancı K",Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Komutan Vektör", NameB = "Ajan Sıfır",      Type = CharacterBondType.Comrades } },
            ["rival_kaos"]       = new[] { new RivalBondSeed { NameA = "Kaptan Ördek",   NameB = "Doktor Ters",    Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Kaptan Ördek",   NameB = "Çılgın Kurye",    Type = CharacterBondType.Comrades } },
            ["rival_obsidian"]   = new[] { new RivalBondSeed { NameA = "Kül Şövalyesi",  NameB = "Kızıl Rahibe",   Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Kül Şövalyesi",  NameB = "Taçsız Prens",    Type = CharacterBondType.Comrades } },
            ["rival_sakura"]     = new[] { new RivalBondSeed { NameA = "Sensei Haruto",  NameB = "Renji Aether",   Type = CharacterBondType.MentorStudent }, new RivalBondSeed { NameA = "Renji Aether",  NameB = "Kuro Kılıç",      Type = CharacterBondType.ArchRivalry } },
            ["rival_ironclad"]   = new[] { new RivalBondSeed { NameA = "Çavuş Demir",    NameB = "General Kül",    Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Çavuş Demir",    NameB = "Mühendis Kaya",   Type = CharacterBondType.Comrades } },
            ["rival_velvet"]     = new[] { new RivalBondSeed { NameA = "Uyurgezer Mia",  NameB = "Yüzsüz Tanık",   Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Uyurgezer Mia",  NameB = "Kasetçi Selim",   Type = CharacterBondType.Comrades } },
            ["rival_nova"]       = new[] { new RivalBondSeed { NameA = "Amiral Lyra",    NameB = "Lord Nebula",    Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Amiral Lyra",    NameB = "Elçi Orion",      Type = CharacterBondType.Comrades } },
            ["rival_pebble"]     = new[] { new RivalBondSeed { NameA = "Çaycı Nine",     NameB = "Fırıncı Momo",   Type = CharacterBondType.MentorStudent }, new RivalBondSeed { NameA = "Fırıncı Momo",  NameB = "Postacı Pipo",    Type = CharacterBondType.Comrades } },
            ["rival_valkyrie"]   = new[] { new RivalBondSeed { NameA = "Freya Mızrak",   NameB = "Buzul Loki",     Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Freya Mızrak",   NameB = "Savaşçı Bjorn",   Type = CharacterBondType.Comrades } },
            ["rival_neonjackal"] = new[] { new RivalBondSeed { NameA = "Krom Çakal",     NameB = "Siber Kraliçe",  Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Krom Çakal",     NameB = "Veri Hayaleti",   Type = CharacterBondType.Comrades } },
            ["rival_clockwork"]  = new[] { new RivalBondSeed { NameA = "Usta Zemberek",  NameB = "Baron Çark",     Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Usta Zemberek",  NameB = "Kaptan Pirinç",   Type = CharacterBondType.MentorStudent } },
            ["rival_titan"]      = new[] { new RivalBondSeed { NameA = "Altın Kalkan",   NameB = "Mega Gölge",     Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Altın Kalkan",   NameB = "Yıldırım Kız",    Type = CharacterBondType.Comrades } },
            ["rival_lumen"]      = new[] { new RivalBondSeed { NameA = "Dedektif Sarp",  NameB = "Siyah Eldiven",  Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Dedektif Sarp",  NameB = "Gazeteci Vera",   Type = CharacterBondType.Comrades } },
            ["rival_bosphorus"]  = new[] { new RivalBondSeed { NameA = "Sarnıç Bekçisi", NameB = "Gölge Kervancı", Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Haritacı Reis",  NameB = "Sarnıç Bekçisi",  Type = CharacterBondType.MentorStudent } },
            ["rival_turbodrift"] = new[] { new RivalBondSeed { NameA = "Nitro Kemal",    NameB = "Kızıl Egzoz",    Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Nitro Kemal",    NameB = "Viraj Melisa",    Type = CharacterBondType.Comrades } },
            ["rival_gloom"]      = new[] { new RivalBondSeed { NameA = "Simyacı Vesper", NameB = "Gölge Dekanı",   Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Simyacı Vesper", NameB = "Mühürlü Ruh",     Type = CharacterBondType.Comrades } },
            ["rival_starlight"]  = new[] { new RivalBondSeed { NameA = "Korsan Vega",    NameB = "Amiral Gümrük",  Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Korsan Vega",    NameB = "Asteroit Jax",    Type = CharacterBondType.Comrades } },
            ["rival_midas"]      = new[] { new RivalBondSeed { NameA = "Baron Midas",    NameB = "Kasa Hırsızı",   Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Baron Midas",    NameB = "Elmas Ajan",      Type = CharacterBondType.Comrades } },
            ["rival_karpuz"]     = new[] { new RivalBondSeed { NameA = "Bakkal Rex",     NameB = "Kral T-Rex",     Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Bakkal Rex",     NameB = "Mamut Kurye",     Type = CharacterBondType.Comrades } },
            ["rival_aetherium"]  = new[] { new RivalBondSeed { NameA = "Döngü Mimarı",   NameB = "Paralel Yankı",  Type = CharacterBondType.ArchRivalry }, new RivalBondSeed { NameA = "Rüya Bekçisi",   NameB = "Döngü Mimarı",    Type = CharacterBondType.MentorStudent } }
        };

        public static void BuildRivalStarDeltasForSave()
        {
            var u = UniverseSaveManager.Current;
            if (u == null || u.RivalStars == null) return;
            var deltas = new List<RivalStarDelta>();

            for (int i = 0; i < u.RivalStars.Count; i++)
            {
                var rs = u.RivalStars[i];
                if (rs == null) continue;
                int basePop = 68;
                RivalStarSeed[] seeds;
                if (RivalRosterSeeds.TryGetValue(rs.RivalId ?? "", out seeds) && seeds != null)
                {
                    for (int s = 0; s < seeds.Length; s++)
                        if (string.Equals(seeds[s].Name, rs.Name, StringComparison.OrdinalIgnoreCase))
                        { basePop = seeds[s].StartPop; break; }
                }

                if (rs.TransferredToPlayer || rs.Popularity != basePop)
                {
                    deltas.Add(new RivalStarDelta
                    {
                        K = $"{rs.RivalId}|{rs.Name}",
                        P = rs.Popularity,
                        T = rs.TransferredToPlayer
                    });
                }
            }
            u.RivalStarDeltas = deltas;
        }

        public static void EnsureEmpireInitialized(Company company)
        {
            UniverseSaveManager.EnsureOwnedUniversesReady();
            var u = UniverseSaveManager.Current;
            if (u.RivalStarDeltas == null) u.RivalStarDeltas = new List<RivalStarDelta>();

            // Eski v9 save'den tam RivalStars geldiyse farklarini delta haritasina al!
            var deltaMap = new Dictionary<string, RivalStarDelta>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < u.RivalStarDeltas.Count; i++)
                if (!string.IsNullOrEmpty(u.RivalStarDeltas[i].K))
                    deltaMap[u.RivalStarDeltas[i].K] = u.RivalStarDeltas[i];

            bool migratedLegacy = false;
            if (u.RivalStars != null && u.RivalStars.Count > 0)
            {
                for (int i = 0; i < u.RivalStars.Count; i++)
                {
                    var old = u.RivalStars[i];
                    if (old == null || string.IsNullOrEmpty(old.RivalId) || string.IsNullOrEmpty(old.Name)) continue;
                    string key = $"{old.RivalId}|{old.Name}";
                    if (!deltaMap.ContainsKey(key))
                        deltaMap[key] = new RivalStarDelta { K = key, P = old.Popularity, T = old.TransferredToPlayer };
                }
                migratedLegacy = true;
            }

            // 1. Tum 141 rakip yildizini sabit sablonlardan RAM'e kur + sadece delta'lari uygula!
            var freshStars = new List<RivalStarRecord>(144);
            for (int r = 0; r < Phase1Database.RivalTemplates.Count; r++)
            {
                var tpl = Phase1Database.RivalTemplates[r];
                RivalStarSeed[] seeds;
                if (!RivalRosterSeeds.TryGetValue(tpl.Id, out seeds) || seeds == null) continue;

                for (int s = 0; s < seeds.Length; s++)
                {
                    var sd = seeds[s];
                    var arch = Phase1Database.FindArchetype(sd.ArchetypeId);
                    string key = $"{tpl.Id}|{sd.Name}";
                    int pop = sd.StartPop;
                    bool trans = false;
                    RivalStarDelta d;
                    if (deltaMap.TryGetValue(key, out d) && d != null)
                    {
                        pop = Math.Clamp(d.P > 0 ? d.P : sd.StartPop, 10, 100);
                        trans = d.T;
                    }

                    freshStars.Add(new RivalStarRecord
                    {
                        Id = $"rstar_{tpl.Id}_{s}",
                        RivalId = tpl.Id,
                        RivalName = tpl.Name,
                        UniverseName = tpl.UniverseName,
                        Name = sd.Name,
                        ArchetypeId = sd.ArchetypeId,
                        RoleName = sd.Role.ToString(),
                        TraitName = sd.Trait.ToString(),
                        TeamName = sd.TeamName ?? "",
                        Axes = sd.CustomAxes != null ? CharacterAxisSystem.NormalizeAxes(sd.CustomAxes) : (float[])arch.PresetAxes.Clone(),
                        Popularity = pop,
                        TransferredToPlayer = trans
                    });
                }
            }
            u.RivalStars = freshStars;

            // 2. Daha onceki surumde satin alinmis bir alt studyo varsa evrenini & karakterlerini geriye donuk aktar!
            bool changed = migratedLegacy;
            if (u.Rivals != null)
            {
                for (int i = 0; i < u.Rivals.Count; i++)
                {
                    var rv = u.Rivals[i];
                    if (!rv.IsAcquiredByPlayer) continue;
                    string uniId = $"uni_{rv.Id}";
                    if (UniverseSaveManager.FindOwnedUniverseById(uniId) == null)
                    {
                        ImportAcquiredStudioUniverse(company, rv, true);
                        changed = true;
                    }
                }
            }

            if (changed) UniverseSaveManager.Save();
        }

        // --- A) SATIN ALINAN STUDYONUN EVRENINI & TUM KARAKTERLERINI TAM DEVRALMA! ---
        public static void ImportAcquiredStudioUniverse(Company company, RivalStudioRecord rival, bool silent = false)
        {
            if (rival == null) return;
            EnsureEmpireInitializedSafeCheck();
            var u = UniverseSaveManager.Current;
            var tpl = Phase1Database.FindRivalTemplate(rival.Id);
            string uniId = $"uni_{rival.Id}";

            // 1. Evreni OwnedUniverses'a ekle (yoksa)
            if (UniverseSaveManager.FindOwnedUniverseById(uniId) == null)
            {
                float[] presetAx = UniverseAxisSystem.PresetForTone(tpl.Tone);
                UniverseRule presetRule = UniverseRule.None;
                if (rival.Id == "rival_njg")
                {
                    // Tentiforverse'e ozel otantik 6 eksen (Karanlik Gizem, Kozmik, Coklu Evren, Yavas Gizem, Zaman Otesi, Yari Ciddi)
                    presetAx = new[] { 0.24f, 0.90f, 0.96f, 0.28f, 0.96f, 0.34f };
                    presetRule = UniverseRule.MysteriousArchive;
                }
                else if (rival.Id == "rival_entakos")
                {
                    presetAx = new[] { 0.86f, 0.72f, 0.66f, 0.78f, 0.62f, 0.90f };
                    presetRule = UniverseRule.ChaosUnleashed;
                }

                u.OwnedUniverses.Add(new OwnedUniverseRecord
                {
                    Id = uniId,
                    Name = rival.UniverseName,
                    FandomName = !string.IsNullOrEmpty(rival.FandomName) ? rival.FandomName : (tpl.FandomName ?? ""),
                    OwnerStudioId = rival.Id,
                    OwnerStudioName = rival.Name,
                    IsOriginal = false,
                    Configured = true,
                    Axes = presetAx,
                    ToneName = UniverseAxisSystem.DeriveToneFromAxes(presetAx).ToString(),
                    RuleName = presetRule.ToString(),
                    LawName = CosmologyLaw.FlexibleMultiverse.ToString(),
                    CanonIntegrity = Math.Clamp(rival.CanonIntegrity, 40, 100),
                    LoreBarrier = 15,
                    ConsecutiveSideProjects = 0,
                    SagaNumber = 1,
                    SagaGamesCount = Math.Min(2, rival.GamesReleased)
                });
            }

            // 2. O studyonun tum canli karakterlerini oyuncu kadrosuna tam erisimli aktar!
            int importedCount = 0;
            var importedMap = new Dictionary<string, CharacterRecord>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < u.RivalStars.Count; i++)
            {
                var rs = u.RivalStars[i];
                if (rs.RivalId != rival.Id || rs.TransferredToPlayer) continue;
                var rec = TransferRivalStarRecordToPlayer(rs, uniId, rival.UniverseName);
                if (rec != null)
                {
                    importedMap[rec.Name] = rec;
                    importedCount++;
                }
            }

            // 3. Dogal karakter baglarini da kur (Tentiforverse & EntaVerse!)
            RivalBondSeed[] bondSeeds;
            if (RivalBondSeeds.TryGetValue(rival.Id, out bondSeeds) && bondSeeds != null)
            {
                for (int b = 0; b < bondSeeds.Length; b++)
                {
                    CharacterRecord cA, cB;
                    if (importedMap.TryGetValue(bondSeeds[b].NameA, out cA) && importedMap.TryGetValue(bondSeeds[b].NameB, out cB))
                        UniverseSaveManager.UpsertBond(cA.Id, cA.Name, cB.Id, cB.Name, bondSeeds[b].Type);
                }
            }

            double w = company != null ? company.currentWeek : 0.0;
            UniverseSaveManager.AddChronology(w, "HOLDING",
                $"🏛 {rival.Name} & '{rival.UniverseName}' Tam Devralındı!",
                $"{importedCount} ikonik karakter ve tüm evren hakları artık tamamen senin kontrolünde!");

            if (!silent && company != null)
            {
                NotificationHelper.Show(company, $"★ TAM EVREN DEVRALINDI: {rival.UniverseName}!",
                    $"{rival.Name} evrenine ve {importedCount} ikonik karakterine tam erişim açıldı! 'Mod İçeriği > Kozmoloji' veya yeni oyun kurarken aktif evreni değiştirebilirsin.", true);
            }
            MelonLogger.Msg($"[Faz 9.0 Tam Devralma] {rival.Name} -> '{rival.UniverseName}' eklendi ({importedCount} karakter aktarildi).");
        }

        private static void EnsureEmpireInitializedSafeCheck()
        {
            UniverseSaveManager.EnsureOwnedUniversesReady();
            if (UniverseSaveManager.Current.RivalStars == null)
                UniverseSaveManager.Current.RivalStars = new List<RivalStarRecord>();
        }

        private static CharacterRecord TransferRivalStarRecordToPlayer(RivalStarRecord rs, string targetUniId, string targetUniName)
        {
            if (rs == null) return null;
            rs.TransferredToPlayer = true;
            var arch = Phase1Database.FindArchetype(rs.ArchetypeId);
            CharacterRole parsedRole = CharacterRole.Protagonist;
            Enum.TryParse<CharacterRole>(rs.RoleName, out parsedRole);

            string cleanTeam = UniverseSaveManager.RegisterTeam(rs.TeamName);
            var rec = new CharacterRecord
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = rs.Name,
                IsNpc = false,
                ArchetypeId = arch.Id,
                ArchetypeName = arch.Name,
                LastRole = CategoryLabels.RoleToTurkish(parsedRole),
                TraitName = rs.TraitName ?? "None",
                TeamName = cleanTeam,
                DebutGame = $"{rs.RivalName} Arşivi",
                Status = rs.Popularity >= 75 ? "Yıldız" : "Sevilen",
                Popularity = Math.Clamp(rs.Popularity, 35, 88),
                GamesAppeared = 2,
                ConsecutiveUses = 0,
                Axes = rs.Axes != null ? CharacterAxisSystem.NormalizeAxes(rs.Axes) : (float[])arch.PresetAxes.Clone(),
                UniverseId = targetUniId,
                UniverseName = targetUniName
            };
            UniverseSaveManager.Current.Characters.Add(rec);
            return rec;
        }

        // --- B) YENI OZGUN IP (2. / 3. EVREN) ACMA ---
        public static int CountOriginalUniverses()
        {
            UniverseSaveManager.EnsureOwnedUniversesReady();
            int cnt = 0;
            var list = UniverseSaveManager.Current.OwnedUniverses;
            for (int i = 0; i < list.Count; i++)
                if (list[i].IsOriginal) cnt++;
            return cnt;
        }

        public static bool TryCreateNewOriginalUniverse(Company company, out string newUniId, out string feedback)
        {
            newUniId = "";
            feedback = "";
            if (company == null) { feedback = "Şirket hazır değil."; return false; }
            int origCount = CountOriginalUniverses();
            if (origCount >= 3)
            {
                feedback = "En fazla 3 özgün evren (Multi-IP) kurabilirsin; daha fazlası için rakip stüdyoları satın al!";
                return false;
            }
            if (UniverseSaveManager.Current.ReleasedGames.Count < 2)
            {
                feedback = "Yeni bir özgün evren açmak için önce en az 2 oyun çıkarmış olmalısın.";
                return false;
            }
            double cost = origCount == 1 ? 120000.0 : 280000.0;
            if (company.cash < cost)
            {
                feedback = $"Yeni özgün evren (IP #{origCount + 1}) kurulum bedeli {cost / 1000.0:F0}K!";
                return false;
            }

            company.adjustCash(-cost, $"Yeni Özgün Evren Kurulumu (IP #{origCount + 1})");
            UniverseSaveManager.SyncTopLevelToActiveUniverse();
            newUniId = $"uni_orig_{origCount + 1}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
            var rec = new OwnedUniverseRecord
            {
                Id = newUniId,
                Name = $"Yeni Evren #{origCount + 1}",
                OwnerStudioId = "player",
                OwnerStudioName = $"Özgün IP #{origCount + 1}",
                IsOriginal = true,
                Configured = false,
                Axes = new float[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.62f, 0.35f },
                ToneName = "None",
                RuleName = "None",
                LawName = CosmologyLaw.FlexibleMultiverse.ToString(),
                CanonIntegrity = 100,
                LoreBarrier = 0,
                SagaNumber = 1,
                SagaGamesCount = 0
            };
            UniverseSaveManager.Current.OwnedUniverses.Add(rec);
            UniverseSaveManager.LoadUniverseIntoTopLevel(rec);
            UniverseSaveManager.Save();

            feedback = $"★ Yeni Özgün Evren (IP #{origCount + 1}) açıldı (-{cost / 1000.0:F0}K)! Şimdi adını ve 6 eksenini tasarla.";
            return true;
        }

        // --- C) TERSINE KONUK DLC (RAKIPLERDEN YILDIZ KIRALA) & TEKIL IP SATIN ALMA ---
        public static double CalcReverseGuestFee(RivalStarRecord star, RivalStudioRecord rival)
        {
            double relDiscount = rival != null && rival.Relation >= 45 ? 0.80 : 1.0;
            double raw = (18000.0 + star.Popularity * 380.0) * relDiscount;
            return Math.Clamp(Math.Round(raw / 1000.0) * 1000.0, 22000.0, 95000.0);
        }

        public static double CalcSingleStarBuyoutPrice(RivalStarRecord star, RivalStudioRecord rival)
        {
            double distress = (rival != null && rival.CanonIntegrity <= 72) ? 0.72 : 1.0;
            double raw = (65000.0 + star.Popularity * 1800.0) * distress;
            if (star.RivalId == "rival_njg") raw *= 1.45;
            else if (star.RivalId == "rival_entakos") raw *= 1.25;
            return Math.Clamp(Math.Round(raw / 5000.0) * 5000.0, 95000.0, 450000.0);
        }

        public static bool TryHireRivalGuestStar(Company company, RivalStarRecord star, out string feedback)
        {
            feedback = "";
            if (company == null || star == null) { feedback = "Yıldız bulunamadı."; return false; }
            if (star.TransferredToPlayer) { feedback = $"{star.Name} zaten senin kadronun parçası!"; return false; }
            var u = UniverseSaveManager.Current;
            if (u.PendingRivalGuests.Count >= 2) { feedback = "Sıradaki oyun için en fazla 2 konuk rakip yıldız kiralayabilirsin."; return false; }
            for (int i = 0; i < u.PendingRivalGuests.Count; i++)
                if (u.PendingRivalGuests[i].StarId == star.Id) { feedback = $"{star.Name} zaten sıradaki oyunun için kiralandı!"; return false; }

            var rival = UniverseSaveManager.FindRivalById(star.RivalId);
            if (rival == null || rival.IsAbsorbedByRival) { feedback = "Bu stüdyo artık aktif değil."; return false; }
            if (rival.Relation < 15)
            {
                feedback = $"{star.Name} kiralamak için {rival.Name} ile ilişkin en az +15 (Dostane) olmalı! (Şu an: {rival.Relation:+#;-#;0})";
                return false;
            }

            double fee = CalcReverseGuestFee(star, rival);
            if (company.cash < fee) { feedback = $"Tersine Konuk DLC bedeli {fee / 1000.0:F0}K!"; return false; }

            company.adjustCash(-fee, $"Tersine Konuk DLC ({rival.Name}): {star.Name}");
            UniverseSaveManager.AdjustRivalRelation(rival, +10);
            u.PendingRivalGuests.Add(new PendingRivalGuestHire
            {
                StarId = star.Id,
                StarName = star.Name,
                RivalId = rival.Id,
                RivalName = rival.Name,
                ArchetypeId = star.ArchetypeId,
                Popularity = star.Popularity,
                Axes = star.Axes != null ? (float[])star.Axes.Clone() : (float[])Phase1Database.FindArchetype(star.ArchetypeId).PresetAxes.Clone()
            });
            UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                $"🎟 TERSİNE KONUK DLC: {star.Name} ({rival.Name}) Senin Oyununa Geliyor!",
                $"{fee / 1000.0:F0}K telifle anlaşma sağlandı! Sıradaki oyununda konuk yıldız olarak sahne alacak (+10 İlişki).", "CROSSOVER");
            UniverseSaveManager.Save();

            feedback = $"🎟 {star.Name} ({rival.Name}) sıradaki oyunun için kiralandı (-{fee / 1000.0:F0}K, +10 İlişki)!";
            return true;
        }

        public static bool TryBuyRivalStarRights(Company company, RivalStarRecord star, out string feedback)
        {
            feedback = "";
            if (company == null || star == null) { feedback = "Yıldız bulunamadı."; return false; }
            if (star.TransferredToPlayer) { feedback = $"{star.Name} zaten senin kadronun parçası!"; return false; }

            var rival = UniverseSaveManager.FindRivalById(star.RivalId);
            if (rival == null || rival.IsAbsorbedByRival) { feedback = "Bu stüdyo artık aktif değil."; return false; }
            bool canTransfer = rival.Relation >= 20 || rival.CanonIntegrity <= 72;
            if (!canTransfer)
            {
                feedback = $"Tekil IP transferi için {rival.Name} ile ilişkin +20 üstünde veya stüdyonun Tutarlılığı ≤ 72 olmalı!";
                return false;
            }

            double price = CalcSingleStarBuyoutPrice(star, rival);
            if (company.cash < price) { feedback = $"{star.Name} kalıcı IP transfer bedeli {price / 1000.0:F0}K!"; return false; }

            company.adjustCash(-price, $"Karakter IP Transferi ({rival.Name}): {star.Name}");
            var actUni = UniverseSaveManager.GetActiveOwnedUniverse();
            TransferRivalStarRecordToPlayer(star, actUni.Id, actUni.Name);
            UniverseSaveManager.AdjustReputation(+3);
            UniverseSaveManager.AddRivalNews(company.currentWeek, rival.Name,
                $"★ KARAKTER TRANSFERİ: {star.Name} Hakları Satın Alındı!",
                $"{rival.Name} yıldızı {star.Name}, {price / 1000.0:F0}K bedelle '{actUni.Name}' evrenine kalıcı olarak transfer edildi!", "HOLDING");
            UniverseSaveManager.AddChronology(company.currentWeek, "TRANSFER",
                $"★ {star.Name} Kalıcı Transfer Edildi!",
                $"{rival.Name} bünyesinden {price / 1000.0:F0}K bedelle '{actUni.Name}' kadrosuna katıldı.");
            UniverseSaveManager.Save();

            feedback = $"★ {star.Name} kalıcı olarak '{actUni.Name}' kadrona transfer edildi (-{price / 1000.0:F0}K)!";
            return true;
        }

        public static void OnRivalGameReleasedUpdateStars(RivalStudioRecord rival, double score)
        {
            if (rival == null || UniverseSaveManager.Current.RivalStars == null) return;
            double delta = score >= 8.5 ? 6.0 : (score >= 7.5 ? 2.5 : (score < 5.8 ? -6.0 : 0.0));
            if (Math.Abs(delta) < 0.1) return;

            var stars = UniverseSaveManager.Current.RivalStars;
            for (int i = 0; i < stars.Count; i++)
            {
                if (stars[i].RivalId == rival.Id && !stars[i].TransferredToPlayer)
                    stars[i].Popularity = PopularitySystem.ApplyDelta(stars[i].Popularity, delta);
            }
        }

        public static RivalStarRecord GetTopLivingRivalStar()
        {
            var stars = UniverseSaveManager.Current.RivalStars;
            if (stars == null || stars.Count == 0) return null;
            RivalStarRecord best = null;
            for (int i = 0; i < stars.Count; i++)
            {
                var s = stars[i];
                if (s.TransferredToPlayer) continue;
                var rv = UniverseSaveManager.FindRivalById(s.RivalId);
                if (rv != null && (rv.IsAcquiredByPlayer || rv.IsAbsorbedByRival)) continue;
                if (best == null || s.Popularity > best.Popularity) best = s;
            }
            return best;
        }

        // --- D) SEKTOR DEPREMLERI (EKONOMIK DURGUNLUK / ALTIN CAG / MODA) ---
        public static SectorEventType GetActiveSectorEvent(double curWeek)
        {
            var u = UniverseSaveManager.Current;
            if (curWeek > u.SectorEventEndWeek) return SectorEventType.None;
            SectorEventType t;
            if (Enum.TryParse<SectorEventType>(u.SectorEventName, out t)) return t;
            return SectorEventType.None;
        }

        public static string GetSectorEventBanner(double curWeek)
        {
            var ev = GetActiveSectorEvent(curWeek);
            double left = Math.Max(0.0, UniverseSaveManager.Current.SectorEventEndWeek - curWeek);
            if (ev == SectorEventType.Recession)
                return $"📉 EKONOMİK DURGUNLUK (~{left:F0} hf): Stüdyo satın alımları -%20 indirimli, ama Hype biraz zor!";
            if (ev == SectorEventType.GoldenAge)
                return $"✨ ALTIN ÇAĞ PATLAMASI (~{left:F0} hf): Merch & Transmedya +%15 gelir, +%10 ekstra Hayran!";
            if (ev == SectorEventType.TrendWave)
            {
                UniverseTone tr = UniverseTone.EpicAdventure;
                Enum.TryParse<UniverseTone>(UniverseSaveManager.Current.SectorTrendToneName, out tr);
                return $"🔥 SEKTÖR MODASI (~{left:F0} hf): '{CategoryLabels.ToneToTurkish(tr)}' evrenleri +%3 Skor & +8 Hype!";
            }
            return "Sektör Dengeli (Aktif küresel ekonomik dalga yok)";
        }

        private static bool TickSectorEarthquakes(Company company, double week)
        {
            var u = UniverseSaveManager.Current;
            if (u.NextSectorEventWeek <= 0.1) u.NextSectorEventWeek = 24.0;
            if (week < u.NextSectorEventWeek) return false;

            u.NextSectorEventWeek = week + 34.0;
            u.SectorEventEndWeek = week + 22.0;

            double r1 = 0.5, r2 = 0.5;
            try { r1 = company.getRandom(); r2 = company.getRandom(); } catch { }

            if (r1 < 0.30)
            {
                u.SectorEventName = SectorEventType.Recession.ToString();
                UniverseSaveManager.AddRivalNews(week, "Küresel Ekonomi",
                    "📉 SEKTÖR DEPREMİ: Ekonomik Durgunluk Başladı!",
                    "22 hafta boyunca pazar daralacak (-3 Hype baskısı), ama rakip stüdyo satın alma bedelleri -%20 ucuzladı!", "SIRKET");
                UniverseSaveManager.AddChronology(week, "DEPREM", "📉 Ekonomik Durgunluk Başladı", "Rakip şirket değerlemeleri -%20 düştü.");
                NotificationHelper.Show(company, "📉 SEKTÖR DEPREMİ: Ekonomik Durgunluk!",
                    "Piyasa 22 haftalığına soğudu — nakdin varsa rakip satın almak için altın fırsat (-%20 indirim)!", true);
            }
            else if (r1 < 0.62)
            {
                u.SectorEventName = SectorEventType.GoldenAge.ToString();
                UniverseSaveManager.AddRivalNews(week, "Küresel Ekonomi",
                    "✨ ALTIN ÇAĞ PATLAMASI: Oyun Sektörü Rekor Kırıyor!",
                    "22 hafta boyunca Merch, Transmedya gelirleri +%15 ve tüm oyun hayran akışı +%10 arttı!", "HIT");
                UniverseSaveManager.AddChronology(week, "DEPREM", "✨ Oyun Sektöründe Altın Çağ!", "Merch/Transmedya +%15 ve hayran patlaması.");
                NotificationHelper.Show(company, "✨ ALTIN ÇAĞ PATLAMASI!",
                    "Oyun dünyasında altın dönem başladı! 22 hafta boyunca Merch/Transmedya +%15 ve hayran akışı +%10 bonuslu!", true);
            }
            else
            {
                u.SectorEventName = SectorEventType.TrendWave.ToString();
                var trendTone = UniverseAxisSystem.AllPresets[(int)(r2 * UniverseAxisSystem.AllPresets.Length) % UniverseAxisSystem.AllPresets.Length];
                u.SectorTrendToneName = trendTone.ToString();
                string tName = CategoryLabels.ToneToTurkish(trendTone);
                UniverseSaveManager.AddRivalNews(week, "Sektör Trendi",
                    $"🔥 YENİ SEKTÖR MODASI: '{tName}' Rüzgarı Esiyor!",
                    $"22 hafta boyunca '{tName}' ruhundaki evrenler +%3 Skor ve +8 Hype kazanacak!", "HABER");
                UniverseSaveManager.AddChronology(week, "MODA", $"🔥 '{tName}' Modası Başladı", "Sektör 22 hafta boyunca bu evren türünün peşinde.");
                NotificationHelper.Show(company, $"🔥 SEKTÖR MODASI: {tName}!",
                    $"Oyuncular bu dönem '{tName}' türüne akın ediyor! Bu tonda evrenin varsa +%3 Skor ve +8 Hype kaparsın!", true);
            }
            return true;
        }

        // --- E) ANTI-TEKEL ITTIFAKI (SANA KARSI RAKIP MEGA CROSSOVER!) ---
        private static bool TickAntiMonopolyAlliance(Company company, double week)
        {
            var u = UniverseSaveManager.Current;
            if (week < 36.0 || (week - u.LastAntiMonopolyWeek) < 38.0) return false;
            if (NativePopupQueue.HasPending || CharacterWorkshopUGUI.IsOpen || UniverseArchitectMod.IsNativeModalOpen()) return false;

            int subsCount = HoldingEndgameSystem.GetPlayerSubsidiaries().Count;
            var board = RivalSimulationSystem.BuildLeaderboard(company);
            bool playerIsTop = board.Count > 0 && board[0].IsPlayer;
            if (subsCount < 2 && !(playerIsTop && company.fans >= 1500)) return false;

            var indep = HoldingEndgameSystem.GetIndependentRivals();
            if (indep.Count < 3) return false;

            indep.Sort((a, b) => b.Fans.CompareTo(a.Fans));
            var r1 = indep[0];
            var r2 = indep[1];
            var r3 = indep[2];

            u.LastAntiMonopolyWeek = week;
            string allianceTitle = $"{r1.UniverseName} × {r2.UniverseName}: Son İttifak";
            double rnd = 0.5;
            try { rnd = company.getRandom(); } catch { }
            double allianceScore = Math.Round(8.4 + rnd * 1.1, 1);

            r1.Fans += 180; r2.Fans += 160; r3.Fans += 140;
            r1.CrossoversDone++; r2.CrossoversDone++;
            UniverseSaveManager.AdjustRivalRelation(r1, -12);
            UniverseSaveManager.AdjustRivalRelation(r2, -12);

            UniverseSaveManager.AddRivalNews(week, "Anti-Tekel Cephesi",
                $"⚔ ANTİ-TEKEL İTTİFAKI! {r1.Name}, {r2.Name} & {r3.Name} Sana Karşı Birleşti!",
                $"Dev ortak yapım '{allianceTitle}' ({allianceScore:F1}/10) ile piyasadaki hakimiyetine meydan okudular!", "REKABET");
            UniverseSaveManager.AddChronology(week, "SAVAS",
                $"⚔ Anti-Tekel Cephesi: {r1.Name} + {r2.Name} + {r3.Name}",
                $"Sana karşı '{allianceTitle}' ({allianceScore:F1}/10) ortak Crossover'ı ile birleştiler.");

            NativePopupQueue.Enqueue(
                "⚔ ANTİ-TEKEL İTTİFAKI SANA SAVAŞ AÇTI!",
                $"Piyasadaki büyümen rakipleri korkuttu! {r1.Name}, {r2.Name} ve {r3.Name} gizlice anlaşıp '{allianceTitle}' ({allianceScore:F1}/10) adında dev bir Rakip Mega Crossover duyurdu!\n\nNasıl karşılık vereceksin?\n\n" +
                $"• Karşı Dev Duyuru Patlat (-25K): +16 Hype, ittifakın rüzgarını kes & hayranlarını koru.\n" +
                $"• İttifakı Diplomatik Böl (-18K): {r3.Name} ile gizlice anlaş (+25 İlişki, +3 İtibar).\n" +
                $"• Soğukkanlı Kal (Ücretsiz): +5 Tutarlılık, ama kısa vadede -8 Hype.",
                new[] { "Karşı Duyuru (-25K)", "İttifakı Böl (-18K)", "Soğukkanlı Kal" },
                opt =>
                {
                    if (opt == 0)
                    {
                        company.adjustCash(-25000.0, "Anti-Tekel Karşı Duyuru");
                        company.adjustHype(16);
                        UniverseSaveManager.AdjustReputation(+3);
                        NotificationHelper.Show(company, "Karşı Duyuru Gündemi Ele Geçirdi!",
                            "Anti-Tekel İttifakı'nın rüzgarını kestin (+16 Hype, +3 İtibar)!");
                    }
                    else if (opt == 1)
                    {
                        company.adjustCash(-18000.0, $"Gizli Anlaşma: {r3.Name}");
                        UniverseSaveManager.AdjustRivalRelation(r3, +25);
                        UniverseSaveManager.AdjustReputation(+3);
                        NotificationHelper.Show(company, $"İttifak Çatladı: {r3.Name}!",
                            $"{r3.Name} cepheden ayrılıp seninle dostane masaya oturdu (+25 İlişki, +3 İtibar)!");
                    }
                    else
                    {
                        UniverseSaveManager.AdjustIntegrity(+5);
                        company.adjustHype(-8);
                    }
                    UniverseSaveManager.Save();
                }
            );
            return true;
        }

        public static void Tick(Company company)
        {
            if (company == null) return;
            double week = company.currentWeek;
            bool changed = false;

            if (TickSectorEarthquakes(company, week)) changed = true;
            if (TickAntiMonopolyAlliance(company, week)) changed = true;

            if (changed) UniverseSaveManager.Save();
        }
    }


    // =========================================================================
    // 9F. FAZ 10.0: TIVITIR — CANLI SOSYAL MEDYA, @FENOCTAKOS & 6 FENOMEN,
    //     GELISMIS SERBEST TIVIT + SOZ ARSIVI (8), DM (24), YAN HESAP, AMA,
    //     4'LU KARAKTER TURNUVASI, TREND TOPIC, BOYKOT & AYRI KOMPAKT SAVE
    // =========================================================================
    public class TivitReply
    {
        public string H { get; set; }
        public string D { get; set; }
        public string X { get; set; }
        public int L { get; set; }
    }

    public class TivitPost
    {
        public string Id { get; set; }
        public double W { get; set; }
        public string H { get; set; }
        public string D { get; set; }
        public bool V { get; set; }
        public string Tag { get; set; } // PLAYER / RIVAL / FANDOM / INFLUENCER / FANWORK / BURNER / POLL / TOURNEY
        public string Sid { get; set; }
        public string CharN { get; set; }
        public string X { get; set; }
        public string Hash { get; set; }
        public int L { get; set; }
        public int RT { get; set; }
        public bool Del { get; set; }
        public bool CanRatio { get; set; }
        public bool RatioDone { get; set; }
        public bool FanActDone { get; set; }
        public bool Replied { get; set; }
        public List<TivitReply> Replies { get; set; } = new List<TivitReply>();
    }

    public class TivitDm
    {
        public string Id { get; set; }
        public double W { get; set; }
        public string H { get; set; }
        public string D { get; set; }
        public string Kind { get; set; } // CEO_BEEF / MOLE_INTEL / SUPERFAN / FENOCTAKOS_COLLAB
        public string RivalId { get; set; }
        public string Body { get; set; }
        public string Opt0 { get; set; }
        public string Opt1 { get; set; }
        public string Opt2 { get; set; }
        public bool Done { get; set; }
        public string Note { get; set; }
    }

    public class TivitPromise
    {
        public string Id { get; set; }
        public double W { get; set; }
        public string Text { get; set; }
        public string Kind { get; set; } // HIGH_SCORE / SIDE_UNI / ALT_TIME / CROSSOVER
        public string Target { get; set; }
        public bool Resolved { get; set; }
        public bool Kept { get; set; }
    }

    public class TivitTournament
    {
        public bool Active { get; set; }
        public double StartW { get; set; }
        public double EndW { get; set; }
        public string[] Names { get; set; }
        public string[] Studios { get; set; }
        public int[] Votes { get; set; }
        public string Result { get; set; }
    }

    public class TrendItem
    {
        public string Tag;
        public int Score;
        public bool IsPlayerOwned;
    }

    public class SocialSaveData
    {
        public List<TivitPost> Tweets { get; set; } = new List<TivitPost>();
        public List<TivitDm> Dms { get; set; } = new List<TivitDm>();
        public List<TivitPromise> Promises { get; set; } = new List<TivitPromise>();
        public TivitTournament Tourney { get; set; }
        public bool BoycottActive { get; set; }
        public string BoycottReason { get; set; }
        public double BoycottEndW { get; set; }
        public bool SponsorKaosActive { get; set; }
        public bool FakeBeefActive { get; set; }
        public string FakeBeefRivalId { get; set; }
        public int BurnerHeat { get; set; }
        public double LastAmbientW { get; set; }
        public double LastFenoctakosW { get; set; }
        public double LastDmW { get; set; }
        public double LastAmaW { get; set; } = -40.0;
        public double LastRpW { get; set; } = -12.0;
        public double LastPollW { get; set; } = -16.0;
        public List<double> RecentPlayerTweetWeeks { get; set; } = new List<double>();
        public int TotalPosted { get; set; }
    }

    public static class SocialSaveManager
    {
        public const int MaxTweets = 64;
        public const int MaxDms = 24;
        public const int MaxPromises = 8;

        public static SocialSaveData Current { get; private set; } = new SocialSaveData();
        private static string _saveKey = "";
        private static bool _isDirty = false;
        private static float _flushTimer = 0f;

        private static string SocialPath => string.IsNullOrEmpty(_saveKey)
            ? Path.Combine(Application.persistentDataPath, "UniverseArchitect_Social.json")
            : Path.Combine(Application.persistentDataPath, $"UniverseArchitect_Social_{_saveKey}.json");

        public static void MarkDirty(bool saveNow = false)
        {
            _isDirty = true;
            if (saveNow) Flush(true);
        }

        public static void TickFlush(float dt)
        {
            if (!_isDirty) return;
            _flushTimer += dt;
            if (_flushTimer >= 8.0f)
            {
                _flushTimer = 0f;
                Flush(false);
            }
        }

        public static void LoadForCompany(Company company, string saveKey)
        {
            _saveKey = saveKey ?? "";
            _isDirty = false;
            _flushTimer = 0f;
            try
            {
                if (File.Exists(SocialPath))
                {
                    var loaded = JsonSerializer.Deserialize<SocialSaveData>(File.ReadAllText(SocialPath));
                    if (loaded != null)
                    {
                        Current = loaded;
                        EnsureLists();
                        MelonLogger.Msg($"[Faz 10 Tivitir] Sosyal kayit yuklendi [{_saveKey}] ({Current.Tweets.Count} tivit, {Current.Dms.Count} DM, {Current.Promises.Count} soz).");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 10 Tivitir] Sosyal kayit okuma hatasi: {ex.Message}");
            }

            Current = new SocialSaveData();
            EnsureLists();
            TivitirSystem.SeedOpeningTimeline(company);
            Flush(true);
        }

        private static void EnsureLists()
        {
            if (Current.Tweets == null) Current.Tweets = new List<TivitPost>();
            if (Current.Dms == null) Current.Dms = new List<TivitDm>();
            if (Current.Promises == null) Current.Promises = new List<TivitPromise>();
            if (Current.RecentPlayerTweetWeeks == null) Current.RecentPlayerTweetWeeks = new List<double>();
        }

        public static void Flush(bool force)
        {
            if (!force && !_isDirty) return;
            try
            {
                EnsureLists();
                if (Current.Tweets.Count > MaxTweets)
                    Current.Tweets.RemoveRange(0, Current.Tweets.Count - MaxTweets);
                if (Current.Dms.Count > MaxDms)
                    Current.Dms.RemoveRange(0, Current.Dms.Count - MaxDms);
                if (Current.Promises.Count > MaxPromises)
                    Current.Promises.RemoveRange(0, Current.Promises.Count - MaxPromises);

                File.WriteAllText(SocialPath, JsonSerializer.Serialize(Current, UniverseSaveManager.CompactJsonOpts));
                _isDirty = false;
                _flushTimer = 0f;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 10 Tivitir] Sosyal kayit yazma hatasi: {ex.Message}");
            }
        }
    }

    public static class TivitirSystem
    {
        // RAM-ici Anti-Tekrar Hafizasi (Save'e hic yuk bindirmez!)
        private static readonly Queue<string> _recentTemplateKeys = new Queue<string>();
        private static readonly HashSet<string> _recentTemplateSet = new HashSet<string>();
        private static readonly Queue<string> _recentDmKinds = new Queue<string>();
        private static int _seqSeed = 7;

        private static int NextSeed(Company comp, int max)
        {
            if (max <= 1) return 0;
            _seqSeed = (_seqSeed * 31 + 17) & 0x7FFFFFFF;
            double r = 0.0;
            try { if (comp != null) r = comp.getRandom(); } catch { }
            int v = (int)Math.Floor(r * max) + _seqSeed;
            return Math.Abs(v) % max;
        }

        private static string PickNoRepeat(Company comp, string poolTag, string[] pool)
        {
            if (pool == null || pool.Length == 0) return "";
            if (pool.Length == 1) return pool[0];
            int start = NextSeed(comp, pool.Length);
            for (int off = 0; off < pool.Length; off++)
            {
                int idx = (start + off) % pool.Length;
                string key = $"{poolTag}_{idx}";
                if (!_recentTemplateSet.Contains(key))
                {
                    RememberTemplate(key);
                    return pool[idx];
                }
            }
            string fallbackKey = $"{poolTag}_{start}";
            RememberTemplate(fallbackKey);
            return pool[start];
        }

        private static void RememberTemplate(string key)
        {
            if (_recentTemplateSet.Add(key))
            {
                _recentTemplateKeys.Enqueue(key);
                while (_recentTemplateKeys.Count > 120)
                {
                    string old = _recentTemplateKeys.Dequeue();
                    _recentTemplateSet.Remove(old);
                }
            }
        }

        private static string FormatUniPhrase(string uniName)
        {
            if (string.IsNullOrWhiteSpace(uniName)) return "evren";
            string low = uniName.ToLowerInvariant();
            if (low.EndsWith("evreni") || low.EndsWith("verse") || low.EndsWith("saga"))
                return uniName;
            return uniName + " evreni";
        }

        // --- FAZ 10.2 DEV MEGA HAVUZLARI (600+ OZGUN SABLON + 11.000+ KOMBINASYON!) ---
        private static readonly string[] FenoctakosTheories =
        {
            "Beyler yeni teori: Kuriow ile Curio aslında aynı boyutun ikiye bölünmüş yarısı, Küriyo da aradaki glitch! #EntaTaym uyanın!",
            "EntaVerse haritasını ters çevirince Alzhe&İmer Kardeşler'in niye hep kaybolduğu çözülüyor. Çonça gizli boss olabilir mi?!",
            "Son videoda anlattım: LarpTakos aslında Negar'ın eski planını bozmak için Fenta'ya gizlice yardım ediyor! Bunu gizleyemezler!",
            "maykıl'ı hafife alanlar ağlayacak! EntaVerse'ün bütün zaman çizgisini elinde tutan adam bence maykıl, kanıtlarım hazır!",
            "EntaTaym yoklama! Fenta'nın son repliklerini tersten dinleyen oldu mu? Küriyo'nun dönüş tarihi şifrelenmiş bence!",
            "Sabah 4 oldu hâlâ tahtaya çiziyorum: Negar aslında gelecekten gelmiş yaşlı Curio olabilir mi?! #EntaTaym çıldıracak!",
            "Alzhe&İmer Kardeşler'in her oyunda aynı kapıyı unutup tekrar açması tesadüf değil — o kapı Çonça'nın gizli üssü!",
            "Kuriow vs Fenta kapışsa kim alır anketi açtım, yorumlarda maykıl tek atar diyenler yüzünden sunucu çöktü 😂",
            "LarpTakos'un zırhındaki çizikleri tek tek saydım, tam 9 tane var... EntaVerse'ün 9 efsanesine gönderme bu!",
            "Küriyo ile Kuriow aynı karede hiç göründü mü? Hayır! Çünkü biri uyanınca öbürü uyuyor, yazın bunu bir kenara!",
            "Dün gece rüyamda Negar ile Çonça çay içiyordu, uyanır uyanmaz 40 dakikalık EntaVerse kronoloji videosu kaydettim!",
            "EntaTaym'a soruyorum: Fenta olmasa LarpTakos ile Curio 5 dakika anlaşabilir miydi? Bence ilk dakikada kavga çıkar!",
            "maykıl'ın arka planda fısıldadığı cümleyi yavaşlattım: 'Alzhe&İmer'e söyleyin anahtarı unuttular!' Dahi işi lore!",
            "Negar'ı kötü sananlar oyunun gizli sonunu açmamış! Adam aslında Kuriow'un açtığı boyut çatlağını kapatmaya çalışıyor!",
            "Yeni video yayında: 'Çonça'nın 17 Gizli Göndermesi & Küriyo'nun Gerçek Kimliği!' Çayları alın #EntaTaym!",
            "EntaVerse'de en çok hakkı yenen karakter kesinlikle LarpTakos. Adam hem Fenta'yı kolluyor hem Negar'la satranç oynuyor!",
            "Kuriow bir gün ciddileşirse EntaVerse'de taş üstünde taş kalmaz, demedi demeyin! #EntaTaym",
            "Curio'nun defterindeki 3. sayfayı yakınlaştırdım — köşede küçücük bir 'maykıl buradaydı' notu var!! ÇILDIRACAĞIM!",
            "EntaTaym uyanık mıyız?! Çonça'nın gölgesi bazı sahnelerde ikiye ayrılıyor, ikincisi bence gizli Küriyo kopyası!",
            "Fenta'nın ilk oyundaki kırık kılıcını hatırlıyor musunuz? Meğer onu tamir eden LarpTakos'muş, arka plan afişinde yakaladım!",
            "Alzhe&İmer Kardeşler bir gün gerçekten her şeyi hatırlarsa EntaVerse'ün bütün dengesi değişir, Negar bile kaçar!",
            "Kuriow'un kahkahasını ses programında inceledim — içinde Curio'nun fısıldadığı koordinatlar saklı!! #EntaTaym",
            "Bana deli diyeceksiniz ama maykıl aslında tüm EntaVerse hikayesini dışarıdan yazan anlatıcı çıkacak, ekran görüntüsü alın!",
            "Negar'ın pelerinindeki sembolü çevirdim: Eski Enta dilinde 'Çonça'yı uyandırmayın' yazıyormuş! Tüyler diken!",
            "Küriyo'yu yan karakter sananlar sıradaki oyunda şok olacak. Fenta bile onun karşısında geri adım attı!",
            "LarpTakos solo oyunu çıksa 3 gün uyumadan %100 bitiririm. #EntaTaym bu isteği gündeme taşıyalım!",
            "Curio her planı kusursuz yapıyor ama Kuriow odaya girdiği an bütün satranç tahtasını deviriyor — en iyi ikili net bunlar!",
            "Alzhe&İmer Kardeşler'in kayıp 3. kardeşi var mı teorisi attım, 400 yorum geldi hepsi 'varsa bile unutmuşlardır' yazmış 😂",
            "EntaVerse'deki en gizemli sahne: maykıl'ın Negar'a boş bir zarf uzatıp gülerek yürüyüp gitmesi. O zarfta ne vardı?!",
            "Çonça ile Kuriow ittifak kursa Fenta, Curio ve LarpTakos üçlüsü bile onları zor durdurur! #EntaTaym ne diyor?",
            "Fenta'nın 'Ben kahraman değilim, sadece temizlikçiyim' repliği hâlâ kulaklarımda. EntaVerse yazarları alev almış!",
            "Küriyo'nun göz rengi her bölümde hafif değişiyor fark eden var mı? Bence her geçişte başka bir paralel EntaVerse görüyoruz!",
            "Negar aslında Kuriow'un en eski dostuydu diyen teoriye %100 katılıyorum. Finalde sarılıp ağlarsak şaşırmam!",
            "LarpTakos vs Çonça kapışması 2 dakikalık ara sahneye sığdırılamaz, buna ayrı DLC lazım #EntaTaym!",
            "Curio'nun masasında duran saat hep 04:25'te takılı — Alzhe&İmer Kardeşler'in hafızasının sıfırlandığı an bu olabilir mi?!",
            "maykıl hakkında 1 saatlik belgesel tadında video montajlıyorum. 'Sıradan karakter' maskesi düştü artık!",
            "Kuriow, Küriyo ve Curio üçlüsünün isim benzerliği kesinlikle tesadüf değil — Üçlü Ayna Teorisi bu akşam yayında!",
            "EntaTaym anketi: Hangi karakterle aynı evde 1 hafta yaşamak en kaotik olurdu? Benim oyum kesinlikle Alzhe&İmer Kardeşler 😂",
            "Fenta'nın sakladığı kara kutuyu Çonça çaldıysa Negar'ın bir sonraki hamlesi bütün haritayı sıfırlamak olacak!",
            "EntaVerse evrenine yeni başlayanlara rehber çektim: İlk kural — maykıl güldüğünde ciddiye al, Kuriow sustuğunda kaç!"
        };

        private static readonly string[] FenoctakosPlayerOwnedTakes =
        {
            "Yeni patron {playerHandle} EntaVerse'ü devraldı! {playerStudio} eğer Kuriow ve Fenta'ya hakkını verirse #EntaTaym ömür boyu arkanızda!",
            "{playerHandle} duysun: EntaVerse artık sizin elinizde, Alzhe&İmer Kardeşler ile LarpTakos'a dev bir devam oyunu borçlusunuz! #EntaTaym",
            "Entakos dönemini özleriz ama {playerStudio} elinde EntaVerse bambaşka parladı! Curio ve Negar için yeni teoriler yolda!",
            "Eski Entakos günlerinden beri takipçiyim, {playerHandle} EntaVerse'ü kendi evreniyle çarpıştırırsa internet kırılır! #EntaTaym",
            "{playerStudio} çatısı altında Küriyo ve Çonça'nın bütçesi 10 katına çıkmış diyorlar! #EntaTaym çıldırıyor!",
            "Başta '{playerStudio} EntaVerse'ü bozar mı?' diye korkmuştum ama maykıl ve LarpTakos'a dokunmadıkları gibi parlatmışlar!",
            "Hey {playerHandle}! EntaVerse artık sizinse şu Negar'ın geçmişini anlatan karanlık bir Yan Evren patlatsanıza! #EntaTaym",
            "{playerStudio} sayesinde EntaVerse global devlere kafa tutuyor! Kuriow'un posterini bütün duvarlara astık!",
            "Bir gün {playerHandle} kendi başrolüyle Curio'yu aynı sahneye koyduğunda benim kanal 1 milyon aboneyi geçer, net!",
            "{playerStudio} yönetiminden tek ricam: Alzhe&İmer Kardeşler'e asla Final Fedası yazmayın, #EntaTaym bunu kaldıramaz!"
        };

        private static readonly string[] TentiArsivPosts =
        {
            "TentiTaym dikkat! Delilik serisindeki kütüphane kulübesi notları aslında Feil'in zaman yolculuğuna işaret ediyor. Tarı her şeyi biliyordu!",
            "Afor'un beyaz taşı tam 554 yıl sonrasından getirmesi ve Gri ile karşılaşması... NJG Games bu lore'u nasıl kurdu aklım almıyor!",
            "Kyldo Zarın ile Dost'un macerası ne zaman oyun olacak?! Yejen ve Necale ekibiyle kesişseler ortalık yıkılır!",
            "Rıpep mi daha tehlikeli yoksa Kaelan mı? Nıraz ve L25 olmasa Tentiforverse çoktan sıfırlanmıştı, net!",
            "Durak E25'ten inen son yolcunun cebindeki harita aslında Saek Luyot'un ilk gününe bağlanıyor! #TentiTaym uyuyor mu?!",
            "Tömyed Kütüphanesi'nin 6. katındaki 144. kitap hâlâ gizemini koruyor. Tarı o kitabı Necale'den bile saklamış!",
            "Kaelan'ın 63 normal hayat denemesi hikayesini okuyunca tüylerim diken diken oldu... Tentiforverse karanlık başka bir seviye!",
            "EvrenGezer ile Zafkı Eşci aynı sahneye düşse komedi mi çıkar dram mı? Bence Lebga gelip ikisini de susturur!",
            "Egir, Zakum, Obenas ve Naleg nöbetteyken kütüphaneye kimse giremez diyorduk... ta ki Pileg kapıyı çalana kadar!",
            "Yaşam'ın son monoloğunu üç kere baştan okudum. Tentiforverse'de kimse gerçekten tesadüfen karşılaşmıyor!",
            "Yejen'in Afor'a söylediği o tek cümle aslında Delilik 26'nın bütün finalini özetliyormuş! #TentiTaym beyin yandı!",
            "Kyldo alfabesiyle yazılmış duvar yazısını çözdüm: 'Feil zamanı değil, hatıraları büktü.' NJG Games ne yapıyorsun bize?!",
            "Saek Luyot niye her maceraya katılmıyor diyenlere cevap: Adam yaşadığı yere bağlı, boşuna zorlamayın!",
            "Nıraz ile Kaelan'ın o eski hesaplaşması hâlâ kapanmadı. Bir gün tek oyunluk düello çıksa 10/10 alır!",
            "Gri'nin gerçek niyetini hâlâ çözemedik... Afor'a yardım mı etti yoksa Rıpep'in ekmeğine yağ mı sürdü?",
            "L25'in sessizliği hep fırtına öncesi sessizliktir. Durak E25 ışıkları yeniden yanıyorsa #TentiTaym nöbete geçsin!",
            "Delilik 1'deki gece vardiyası atmosferini hâlâ hiçbir korku oyunu veremedi. Necale'nin feneri sönünce kalbim duruyordu!",
            "Dost'un Kyldo Zarın'a sadakati bütün oyun dünyasındaki en iyi arkadaşlık bağı olabilir. #TentiTaym mendilleri çıkarın!",
            "Tarı'nın kütüphane raflarını alfabetik değil kronolojik dizdiğini yeni fark ettim... Bütün ipuçları oradaymış!",
            "Rıpep konuşmaya başladığında insan bir anlığına haklı mı acaba diyor — işte gerçek karizmatik kötü böyle yazılır!",
            "Afor'un Yejen'le atışmaları olmasa Delilik 2'nin karanlık tonunda nefes alamazdık, mükemmel denge!",
            "Zafkı Eşci yine hangi boyuttan kaçak eşya getirdi acaba? Zakum ve Obenas peşinde koşturup duruyor 😂",
            "Naleg'in esprileri komik ama bence o karakterde de büyük bir sır saklı. Tömyed'de sıradan kimse yok!",
            "Pileg ile Lebga'nın karanlık koridordaki 10 saniyelik bakışması bile 30 dakikalık teori videosuna yeter!",
            "Egir'in nöbet defterindeki 26. sayfa yırtılmış... Acaba Feil geleceği korumak için kendi mi yırttı?",
            "EvrenGezer'in farklı boyutlardan topladığı rozetlerin arasında başka stüdyoların evrenlerine gönderme var gibi 👀",
            "Yaşam bir karakterden çok evrenin vicdanı gibi duruyor. Tentiforverse'ün metafizik katmanı çok derin!",
            "Delilik 0'ın o ham tiyatro havasını özleyen kaç eski #TentiTaym üyesi kaldık burada?",
            "Kyldo Zarın'ın el yazması notlarını Kyldo alfabesiyle tercüme eden hayran sitesi çökmüş, yoğunluktan girilmiyor!",
            "Afor ve Necale omuz omuza savaştığında kütüphanenin 6 katı birden titriyor! Efsane ikili! #TentiTaym"
        };

        private static readonly string[] FanHandles =
        {
            "@GeceOyuncusu", "@PikselBeyi", "@LoreAvcisi99", "@KanonDelisi", "@RetroKafe", "@IndieSevdalisi",
            "@OyunBuzdolabi", "@SabahlayanDev", "@TeoriKazani", "@KupaAvcisi", "@SiberCayci", "@BoyutYolcusu",
            "@KlavyeKiran", "@SaveDosyasi", "@BossSavascisi", "@KaranlikMod", "@AltF4Ustasi", "@FpsSayaci",
            "@OyunGurmesi", "@DiyalogOkuyan", "@YanGorevci", "@KanonBekcisi", "@PikselMimar", "@GeceVardiyasi",
            "@BiletliHayran", "@KoleksiyoncuTR", "@SonBolumAglayan", "@HypeTreni", "@CayVeGamepad", "@SpoilerPolisi",
            "@KasetArsivi", "@UcuncuBoyut", "@MuzikDinleyen", "@BugBulucu", "@ModYukleyici", "@IlkGunAlan",
            "@KutuphaneFaresi", "@ZamanBükücü", "@KartonKutuDev", "@EkranGoruntusu", "@CiftZiplama", "@FinalSaveci",
            "@SadeceHikaye", "@ZorModDelisi", "@AfkKalan", "@Ping999", "@KonsolKrali", "@MobilOyuncu42",
            "@PixelArtSevdasi", "@YamaBekleyen", "@IndirimAvcisi", "@SepetDolduran", "@DemoOynayan", "@BetaTestci",
            "@KoltukElestirmeni", "@WikiEditörü", "@FanArtCizeri", "@CosplayTerzisi", "@AltyaziOkuyan", "@SesAcikOyna",
            "@KaranlikOda", "@Gece0317", "@SabahKahvesi", "@KupaTrophy", "@PlatinAvcisi", "@EasterEggci",
            "@GizliSonAvcisi", "@HaritaAcan", "@SandikAcan", "@IksirBiriktiren", "@CanBariKirmizi", "@CheckPointci",
            "@LoadingEkrani", "@AnaMenuMuzigi", "@JenerikBekleyen", "@PostCreditsFan", "@EvrenKurucu", "@CokluBoyutTR",
            "@KaosSevdalisi", "@DramTutkunu"
        };

        private static readonly string[] RivalHitTemplates =
        {
            "🔥 '{title}' yayında ve {score}/10 topladı! {uniPhrase} tarih yazıyor! {tag}",
            "🏆 Eleştirmenler ayakta alkışlıyor: '{title}' tam {score}/10! {uniPhrase} yeni zirvesini gördü! {tag}",
            "⚡ Rekor açılış! '{title}' ({score}/10) ile {studio} yine ustalığını konuşturdu! {tag}",
            "🚀 '{title}' çıktı ve ortalık yıkıldı ({score}/10)! {uniPhrase} durdurulamıyor! {tag}",
            "👑 Başyapıt uyarısı: '{title}' {score}/10 puanla listelerin tepesinde! {tag} gururla sunar!",
            "🎬 Aylar süren bekleyişe değdi! '{title}' ({score}/10) yılın en çok konuşulan yapımı oldu! {tag}",
            "💎 '{title}' ile {score}/10! {studio} ekibi bu gece kutlama yapıyor, {tag} bayram ediyor!",
            "🌟 'Bu sefer çıtayı nereye koydular?' dedirten '{title}' tam {score}/10 aldı! {tag}",
            "🎯 Tam isabet! '{title}' ({score}/10) ile {studio} bütün rakiplerine gözdağı verdi! {tag}",
            "📣 Sunucular kilitlendi! '{title}' ({score}/10) ilk saatinde indirme rekoru kırdı! {tag}",
            "🏅 Yılın Oyunu yarışına bomba gibi girdi: '{title}' tam {score}/10! {uniPhrase} şahlandı! {tag}",
            "✨ '{title}' ({score}/10) sonrası sosyal medya tek ses: '{studio} bunu nasıl başardı?!' {tag}",
            "🎇 Gece yarısı lansmanı olay oldu: '{title}' ({score}/10) oyuncuların kalbini fethetti! {tag}",
            "🥂 Şampanyalar patladı! '{title}' ({score}/10) ile {uniPhrase} altın çağına girdi! {tag}",
            "🔥 '{title}' ({score}/10) oynayan herkes aynı şeyi yazıyor: 'Son 5 yılın en iyi deneyimi!' {tag}",
            "📈 {studio} hisseleri uçtu! '{title}' ({score}/10) eleştirmenlerden tam not aldı! {tag}",
            "💥 Beklentiler yüksekti, '{title}' ({score}/10) hepsinin üstünden atladı! {tag}",
            "🌠 '{title}' ({score}/10) — müzik, hikaye ve oynanış kusursuz birleşmiş! {tag}"
        };

        private static readonly string[] RivalMidTemplates =
        {
            "🎮 Yeni oyunumuz '{title}' ({score}/10) raflarda! {uniPhrase} macerası devam ediyor. {tag}",
            "📢 '{title}' bugün itibarıyla erişimde ({score}/10). Tüm {uni} severlere iyi oyunlar! {tag}",
            "🕹️ '{title}' ({score}/10) çıktı! {studio} ekibi olarak geri bildirimlerinizi bekliyoruz. {tag}",
            "✨ {uniPhrase} yeni bölümü '{title}' ({score}/10) ile genişliyor! İlk yorumlar güzel. {tag}",
            "📦 '{title}' ({score}/10) yayında — sağlam oynanış, tanıdık atmosfer. Hafta sonu planı belli! {tag}",
            "🧭 '{title}' ({score}/10) ile yeni bir sayfa açtık. Bakalım gizli sonu ilk kim bulacak? {tag}",
            "☕ Gece kahvenizi alın: '{title}' ({score}/10) indirmeye açıldı! {tag}",
            "📌 '{title}' ({score}/10) mağazalarda yerini aldı. {uniPhrase} hayranlarını ekran başına alalım! {tag}",
            "🎲 '{title}' ({score}/10) ile temiz, akıcı bir macera! {studio} çizgisini koruyor. {tag}",
            "🚪 '{title}' ({score}/10) kapıları açıldı! Yeni karakterleri ve yan görevleri keşfetmeye hazır mısınız? {tag}",
            "🗓️ Hafta sonu ne oynayacağım derdine son: '{title}' ({score}/10) çıktı! {tag}",
            "🎙️ '{title}' ({score}/10) için ilk incelemeler geldi — 'Keyifli ve tutarlı bir devam!' {tag}",
            "⚡ '{title}' ({score}/10) yayında! Küçük sürprizlerle dolu yeni bir {uni} deneyimi. {tag}",
            "📚 '{title}' ({score}/10) evrenin gizli boşluklarını güzel doldurmuş. {tag}",
            "🎯 '{title}' ({score}/10) — devrim yapmasa da hayranlarını fazlasıyla mutlu ediyor! {tag}",
            "🌙 Bu gece '{title}' ({score}/10) ile sabahlıyoruz, çaylar demlensin! {tag}"
        };

        private static readonly string[] RivalFlopTemplates =
        {
            "🛠️ '{title}' ({score}/10) için acil 1. gün yaması üzerinde çalışıyoruz. Sabrınız için teşekkürler. {tag}",
            "📉 '{title}' ({score}/10) beklediğimiz gibi başlamadı... Topluluğu dinliyor ve düzeltiyoruz. {tag}",
            "😬 '{title}' ({score}/10) sonrası eleştirileri ciddiye alıyoruz. Yol haritamızı yakında paylaşacağız. {tag}",
            "🧯 '{title}' ({score}/10) çıkışında yaşanan teknik aksaklıklar için özür dileriz, yama yolda. {tag}",
            "🌧️ '{title}' ({score}/10)... Bazen risk almak tutmaz. {uniPhrase} daha güçlü dönecek. {tag}",
            "🔧 '{title}' ({score}/10) performans sorunları için gece gündüz mesai yapıyoruz, bizi bırakmayın. {tag}",
            "📋 '{title}' ({score}/10) hakkında haklı tepkilerinizi okuduk — kapsamlı yenileme paketi hazırlanıyor. {tag}",
            "😓 '{title}' ({score}/10) bizim standartlarımızın altında kaldı. Bir sonraki projede telafi edeceğiz. {tag}",
            "🚑 '{title}' ({score}/10) için 3 büyük yama sözü veriyoruz, bu oyunu ayağa kaldıracağız! {tag}",
            "⚠️ '{title}' ({score}/10) çıkışı pürüzlü oldu, iade ve destek kanallarımız 7/24 açık. {tag}"
        };

        private static readonly string[] AmbientFandomTemplates =
        {
            "{studio} yeni oyunda {star} için ayrı hikaye yazmazsa cidden bozuşuruz! {tag} benimle aynı fikirde mi?",
            "Biri bana {uni} içindeki {star} karakterinin neden bu kadar az konuşulduğunu açıklayabilir mi? Aşırı sağlam yazılmış! {tag}",
            "Gece 2'de aklıma düştü: {studio} eskiden '{game}' ile ne atmosfer kurmuştu be... {tag}",
            "Arkadaşla 1 saattir tartışıyoruz: {uni} evreninin gerçek MVP'si kesinlikle {star}, itirazı olan gelsin! {tag}",
            "{studio} lütfen {star} karakterini sıradaki oyunda harcamayın, yoksa Tivitır'ı yakarız! {tag}",
            "'{game}' oyununu 4. kez bitirdim ve {star} repliğindeki gizli detayı yeni fark ettim! {tag}",
            "Popüler olmayan fikir: {studio} '{game}' sonrası biraz frene basıp {star} odaklı ufak bir yan oyun yapmalı. {tag}",
            "Şu an {uni} wiki sayfasında 2 saattir kayboldum, {star} geçmişi roman olurmuş resmen! {tag}",
            "{tag} toplanın! '{game}' içindeki en iyi sahne hangisiydi? Benim oyum tartışmasız {star} finaline!",
            "Bütçemi kenara koydum, {studio} yeni {uni} duyurusu yaptığı an ön siparişi yapıştırıyorum! {tag}",
            "'{game}' müziklerini dinlerken duygulanan tek ben değilimdir herhalde... {star} sahnesi unutulmazdı! {tag}",
            "{studio} senaristleri {star} karakterini yazarken kesin gece 3'te dertleniyordu, o diyaloglar neydi öyle! {tag}",
            "Yeni başlayanlara uyarı: '{game}' oynarken {star} ile olan bütün yan görevleri yapın, final tamamen değişiyor! {tag}",
            "Bence {uni} dizisi çekilse 5 sezon aralıksız izlenir! {star} rolünü kim oynar acaba? {tag}",
            "'{game}' 3. bölümündeki boss'u geçmek için 2 saat uğraştım ama {star} ara sahnesi her saniyesine değdi! {tag}",
            "Her yıl düzenli olarak '{game}' açıp sıfırdan bitirme geleneğim başladı! {tag} kimler benimle?",
            "{studio} sessiz kaldığına göre gizli bir {uni} bombası pişiriyor, demedi demeyin! {tag}",
            "Kütüphanede 80 oyun var ama yine dönüp dolaşıp '{game}' açtım, {star} özleniyor arkadaş! {tag}",
            "{tag} itiraf vakti: '{game}' ilk çıktığında önyargılıydım, oynayınca sabaha kadar kalkamadım!",
            "Şu {star} karakterinin peluşunu veya figürünü çıkarsalar ilk saniyede alırım, {studio} duy sesimizi! {tag}",
            "'{game}' içindeki en iyi plot twist hangisiydi? Ben hâlâ {star} sahnesinin şokundan çıkamadım! {tag}",
            "{uni} lore'u o kadar derin ki 40 dakikalık özet videosu izledim hâlâ yarısındayım 😂 {tag}",
            "Bir gün {studio} çıkıp '{game} Remastered' duyursa bütün hafta izin alırım! {tag}",
            "Arkadaşlarımı zorla '{game}' başlattım, şimdi benden çok {star} teorisi sıkıyorlar! {tag}",
            "{star} karakterinin yürüyüş animasyonu bile ayrı karizma ya, {studio} ince işçilik yapmış! {tag}",
            "Bazen sadece '{game}' ana menüsünde bekleyip müziği dinliyorum, terapi gibi oyun! {tag}",
            "Sıradaki {uni} oyununda {star} kötü tarafa geçerse Tivitır'da 3 gün yas ilan ederiz! {tag}",
            "'{game}' sonundaki jenerik sonrası sahneyi görmeden kapatan arkadaşlarım varmış, hemen geri açtırdım! {tag}",
            "Bu kadar sağlam bir {uni} kurup yıllardır çizgi roman çıkarmamak büyük kayıp {studio}! {tag}",
            "Gece vardiyasında '{game}' lore'u okuyorum, {star} hakkında yazılan teoriler uykumu kaçırdı! {tag}"
        };

        private static readonly string[] PowerscaleTemplates =
        {
            "⚔️ TİVİTIR KAVGASI BAŞLASIN: {starA} ({uniA}) vs {starB} ({uniB})! Tarafsız olun, kim ayakta kalır?",
            "🥊 Hayalî Crossover düellosu: {starA} mı alır yoksa {starB} mi? {tagA} ve {tagB} yorumlarda kapışıyor!",
            "🔥 İki dev karşı karşıya: {uniA} yıldızı {starA} vs {uniB} efsanesi {starB}! Hazırlık süresi varsa sonuç değişir mi?",
            "⚖️ Objektif kıyas: Hangisinin hikayesi daha derin — {starA} ({studioA}) mı, {starB} ({studioB}) mı?",
            "🧠 Zeka savaşı! Satranç masasına {starA} ile {starB} otursa hangisi 10 hamlede mat eder?",
            "💥 Aynı arenada silahlar yok, sadece bilek gücü: {starA} ({uniA}) vs {starB} ({uniB})! Oylar kime?",
            "🎭 Hangisi daha unutulmaz bir giriş sahnesine sahip: {starA} mı yoksa {starB} mi? {tagA} vs {tagB}!",
            "🌪️ İki karakter takım olsa durdurabilecek evren var mı: {starA} + {starB} ittifakı hayal edin!",
            "⚡ Hız ve refleks kapışması: {starA} mı önce vurur yoksa {starB} mi? Tartışma alevlendi!",
            "👑 Evrenler arası turnuva finali olsa: {starA} ({studioA}) vs {starB} ({studioB}) — kim kupayı kaldırır?",
            "🗣️ Sadece karizma ve diyalog kalitesi kapışıyor: {starA} mı alır {starB} mi? Fanatiklik yapmayın 😂",
            "🛡️ Hayatta kalma testi: Issız bir gezegende tek başına hangisi daha uzun yaşar — {starA} mı, {starB} mi?"
        };

        private static readonly string[] PowerscaleReplies =
        {
            "{starA} tek hamlede alır, {starB} hayranları boşa ağlamasın 😂",
            "Hazırlık süresi varsa {starB} 10 kere üst üste kazanır, lore okumamışsınız!",
            "İkisi kapışsa dövüş yarıda kalır, sonra oturup beraber çay içerler bence ☕",
            "{starA} favorim ama dürüst olayım {starB} çok ayrı bir ligde...",
            "İkisinin aynı oyunda karşılaştığını düşünsenize, o oyun 10/10 alır net!",
            "{starB} gözü kapalı alır diyenler {starA}'nın son oyununu oynamamış belli 😂",
            "Bence berabere kalırlar, ikisi de kendi evreninin zirvesi!",
            "Kavgayı izlerken kenardan üçüncü biri gelip ikisini de paketler bence 👀"
        };

        private static readonly string[] MemeTemplates =
        {
            "Oyun stüdyoları: 'Geri bildirimlerinizi dinliyoruz' → Gerçekte yaptıkları: Menü rengini değiştirip 70$ istemek 😂 #OyunMizah",
            "{studio} '{game}' için yama notu yayınlamış: 'Küçük hatalar giderildi.' (Oyunun yarısı uçuyordu kardeşim 😂) #OyunMizah",
            "Geliştirici günlüğü 1. hafta: '100 saatlik açık dünya!' → 12. hafta: 'Kısa ve öz 4 saatlik sanat eseri!' 😂 #OyunMizah",
            "Ben: 'Bu akşam erken yatacağım.' → Saat 04:15: '{game}' oyununda {star} yan görevini bitiriyorum... #OyunMizah",
            "Eleştirmen 6.5 verince oyuncular: 'Para yedirmediniz herhalde' / Eleştirmen 9.5 verince yine oyuncular: 'Kesin para yedirdiniz!' 😂 #OyunMizah",
            "Sektör özeti: {studioA} yeni mekanik icat eder, 3 hafta sonra {studioB} 'Bizim de aklımızdaydı' diye aynısını duyurur! #OyunMizah",
            "Stüdyo hesap yöneticisi yanlışlıkla kendi oyununu yan hesap yerine resmi hesaptan övünce yaşanan o 3 saniyelik kalp krizi 💀 #OyunMizah",
            "Oyunda 99 tane iksir biriktirip final boss'unda bile 'ya sonra lazım olursa' diye içmeyen tek ben olamam 😅 #OyunMizah",
            "{uni} hayranları 2 saniyelik fragmandan 3 saatlik lore videosu çıkarırken geliştiriciler: 'Biz onu süs olsun diye koymuştuk...' 😂 #OyunMizah",
            "Oyuncu: 'Neden ertelendi?!' / Oyun erken çıkıp buglı olunca yine oyuncu: 'Neden ertelemediniz?!' Stüdyoların çilesi bitmez! #OyunMizah",
            "Oyun içi diyalog: 'Acele et dünya 5 dakika içinde yok olacak!' → Ben: 'Dur şu köşedeki 47. yan görevi de yapayım' 😂 #OyunMizah",
            "Yeni oyun indiriyorum: 120 GB. Diskteki boş alan: 119.8 GB. Hangi anımdan vazgeçeyim şimdi?! 😭 #OyunMizah",
            "NPC: 'Bu kadim kılıç 1000 yıldır kimse tarafından çekilemedi.' → Oyuncu 3 dakika sonra tüccara 12 altına satar 😂 #OyunMizah",
            "Boss'a girmeden önce 40 dakika envanter dizip ilk 4 saniyede tek yemek... Her oyuncunun kutsal ritüeli! #OyunMizah",
            "Stüdyo: 'Tamamen sıfırdan yeni oyun!' → Oyuncu: 'Kardeş {game} haritasını aynalamışsınız yemezler 😂' #OyunMizah",
            "Geliştirici oyuna 50 saatlik derin hikaye yazar → Oyuncular: 'Skip tuşu nerede?!' 💀 #OyunMizah",
            "Oyun başında karakter tasarımı: 2 saat. Oyunun tamamında karakterin yüzünü görmek: 0 saniye (kask takıyor) 😂 #OyunMizah",
            "Tutorial: 'Zıplamak için Space'e bas.' → 10 dakika sonra ilk boss: 4 fazlı kozmik tanrı! 😅 #OyunMizah",
            "Save almayı unutup 2 saatlik ilerleme elektrik gidince kaybolduğunda gelen o boşluğa bakma hissi... #OyunMizah",
            "Arkadaşım: 'Kanka sadece 1 el atıp çıkıyoruz.' → Dışarıdan ezan sesi ve kuş cıvıltıları gelmeye başlar 😂 #OyunMizah",
            "Kötü adam 10 dakika boyunca tüm gizli planını anlatırken kahramanın kibar kibar dinlemesi beni bitiriyor 😅 #OyunMizah",
            "Oyun fragmanı: Hollywood sineması gibi 8K CGI. Gerçek oyun içi görüntü: İki piksel birbirine kafa atıyor 😂 #OyunMizah",
            "Stüdyolar 5.8 puan alınca geleneksel sarı arka planlı 'Sizi duyuyoruz' PDF'ini hazırlamaya başlıyor 😂 #OyunMizah",
            "Oyundaki en güçlü düşman final boss değil, merdivenden inerken takılan o 1 piksellik görünmez taş! #OyunMizah",
            "Ben: 'Bu sefer kötü karakter olarak oynayacağım!' → İlk NPC üzülünce hemen save'i geri yüklerim 😅 #OyunMizah",
            "{studio} oyununda kapı kilitli — ahşap kapıyı dev kılıçla kırmak yerine 30 dakika paslı anahtar arıyoruz 😂 #OyunMizah",
            "Indirimde 12 oyun alıp akşam yine 500 saat oynadığım '{game}' oyununu açtım. Tıbben incelenmeliyim! #OyunMizah",
            "Oyun içi ekonomi: Dünyayı kurtardın, ödülün 15 altın. Handaki elma: 20 altın 😂 #OyunMizah",
            "Yama boyutu: 48 GB. Yama notu: 'Bir duvar dokusu düzeltildi ve 1 yazım hatası giderildi.' 💀 #OyunMizah",
            "Yapay zekâ yoldaş NPC: 'Sessizce sızalım' deyip 2 saniye sonra bütün düşman kampının ortasına koşar 😂 #OyunMizah"
        };

        private static readonly string[] TierListTemplates =
        {
            "📊 HAFTALIK KARAKTER TIER LIST: S-Tier → {starA}, A-Tier → {starB}, F-Tier → 'Sadece tutorial'da ölen NPC'. İtirazlar aşağıya! #TierList",
            "📊 EVREN TIER LIST GÜNCEL: {uniA} net S-Tier'a çıktı, {uniB} ise hâlâ potansiyelini harcıyor! Katılmayan RT yapmasın! #TierList",
            "📊 EN İYİ KÖTÜ/ANTİ-KAHRAMAN LİSTESİ: {starA} zirvede tek başına, {starB} hemen ensesinde! {tagA} bugün mutlu uyuyacak! #TierList",
            "📊 SEKTÖR STÜDYO SIRALAMASI: {studioA} son hamleyle A+'a zıpladı, {studioB} ise 'bi yama daha' katmanında kaldı! #TierList",
            "📊 YILIN EN İYİ DİYALOG YAZIMI: {uniA} S-Tier'ı kimseye bırakmıyor! {starA} tek başına bütün listeyi sırtlamış! #TierList",
            "📊 ABARTILANLAR vs HAKKI YENENLER: {starA} kesinlikle hakkı yenen S-Tier, {starB} ise biraz fazla şişirilmiş sanki 👀 #TierList",
            "📊 MÜZİK & ATMOSFER TIER LIST: {studioA} açık ara lider! {uniB} ise bir tık daha iyi besteci bulsa zirveye oynar! #TierList",
            "📊 EN İYİ YAN KARAKTERLER: {starB} başrolü gölgede bıraktığı için S-Tier'a aldım, {starA} kusura bakmasın 😂 #TierList",
            "📊 YENİ BAŞLAYANLAR İÇİN EVREN ZORLUK LİSTESİ: {uniA} tam 'çayını al rahatça gir' seviyesi, {uniB} için 3 saatlik wiki şart! #TierList",
            "📊 SON 5 YILIN FİNAL SAHNELERİ: {studioA} bu alanda doktora yapmış, S+ katmanını sırf onlar için açtım! #TierList"
        };

        private static readonly string[] TierListReplies =
        {
            "Bu listeyi gözün kapalı mı yaptın, hemen sil şunu!! 😂",
            "{starA} S-Tier olduğu sürece bu liste benim için onaylıdır, ellerine sağlık!",
            "{uniB} hakkında bu kadar haksız bir yorum görmedim, takipten çıkıyorum 💀",
            "Sonunda {starB} hak ettiği yere konmuş! Yorumlardaki kavga başlasın 🍿",
            "Kardeş kesin {studioA} sana VIP kupa yolladı itiraf et 😂",
            "F-Tier'daki tutorial NPC'si benim favorimdi ayıp ettin 😅"
        };

        private static readonly string[] SpeedrunTemplates =
        {
            "⏱️ YENİ DÜNYA REKORU! '{game}' ({studio}) oyununu sadece 11 dakika 42 saniyede bitirdim! Kapıdan geri geri zıplayınca boss atlanıyor! #Speedrun",
            "⏱️ '{game}' Any% speedrun'ında {star} diyaloğunu 0.3 saniye kısaltan yeni glitch bulduk! Akşam rekor denemesi var! #Speedrun",
            "⏱️ {studio} geliştiricileri 2 yılda yaptığı '{game}' haritasını 8 dakikada duvarın içinden geçerek bitirmemizi izlerken ağlıyordur 😂 #Speedrun",
            "⏱️ '{game}' için 'Hiç Hasar Almadan' rekoru kırıldı! {star} ile son bölümü gözü kapalı geçen psikopatlar var bu toplulukta! #Speedrun",
            "⏱️ '{game}' 100% kategorisinde tam 47 saniye tıraşladık! Tek şart: {star} ara sahnesinde tam 14. karede zıplamak! #Speedrun",
            "⏱️ İnanılmaz: Bir yayıncı '{game}' oyununu sadece tek tuşla bitirmeyi deniyor ve 3. bölüme ulaştı! #Speedrun",
            "⏱️ {studio} son yamada duvardan geçme bug'ını kapatmış — 200 speedrunner aynı anda 'Eski sürümü geri verin!' diye ağlıyor 😂 #Speedrun",
            "⏱️ '{game}' boss rush rekoru 3 dakika 19 saniyeye indi! {star} kombosu resmen zamanı büküyor! #Speedrun",
            "⏱️ Gece 3'te rastgele bir köşeye takıldım ve oyunun final odasına ışınlandım — yeni '{game}' dünya rekoru geliyor!! #Speedrun",
            "⏱️ '{game}' Pacifist (kimseye vurmadan bitirme) koşusu 2 saatte tamamlandı, {studio} bile bunu tahmin etmemiştir! #Speedrun"
        };

        private static readonly string[] MusicTemplates =
        {
            "🎧 '{game}' ({studio}) ana tema müziği 3 gündür kafamda dönüyor, spor salonunda bile bunu dinliyorum! #OyunMüziği",
            "🎵 Kimse konuşmuyor ama {uni} soundtrack'leri şu an piyasadaki çoğu albümden daha iyi prodüksiyona sahip! {tag} #OyunMüziği",
            "🎹 '{game}' final sahnesinde giren o bas ve yaylı geçişi... {studio} bestecisine zam yapsın acil! #OyunMüziği",
            "🔥 {star} teması için yapılan fan rap remix'i 200K dinlenmeyi geçti! Oyun müzikleri artık ayrı bir kültür! #OyunMüziği",
            "🎻 Bir gün {uni} senfoni konseri yapılsa ilk sıradan bilet alırım, '{game}' albümü şaheser! #OyunMüziği",
            "🎶 Ders çalışırken arka planda '{game}' kasaba müziğini açtım, kendimi RPG tüccarı gibi hissediyorum ☕ #OyunMüziği",
            "🎤 '{game}' tanıtımındaki o rap parçası Spotify listelerinde top 10'a girmiş, {studio} plak şirketi de açsın artık! #OyunMüziği",
            "🥁 {star} boss müziği başladığında koro devreye giriyor ya, işte o an tüylerim diken diken oluyor! #OyunMüziği",
            "🎼 Sadece 8-bit değil, modern synth ve orkestra harmanı! {studio} ses tasarımında çıtayı uzaya koymuş! #OyunMüziği",
            "📻 Gece yürüyüşü + kulaklıkta '{game}' final müziği = anında kendi hayatının başrolü olma hissi! #OyunMüziği"
        };

        private static readonly string[] RivalBeefTemplates =
        {
            "🍿 SEKTÖR KULİSİ: {studioA} ile {studioB} aynı hafta çıkış yapmamak için arka kapıda pazarlık yapıyormuş! #SektörDedikodu",
            "🚨 KULİS BİLGİSİ: {studioA} CEO'su bir yemekte '{uniB} bizim yan görevimiz kadar bile derin değil' demiş! Ortalık fena karışacak! #SektörDedikodu",
            "👀 {studioA} yeni iş ilanında 'Kendi {uniB} rakibimizi kuracak baş yazar arıyoruz' yazmış! Resmen savaş ilanı! #SektörDedikodu",
            "📢 {studioA} ile {studioB} hayranları Tivitır'da sabah 5'e kadar '{starA} vs {starB}' kavgası yaptı, iki etiket de gündemde! #SektörDedikodu",
            "🕵️ SIZINTI: {studioA} stüdyosundan ayrılan iki kıdemli tasarımcı gizlice {studioB} ekibine geçmiş! Transfer savaşı başladı! #SektörDedikodu",
            "💼 Sektör fuarında {studioA} standının tam karşısına {studioB} dev {starB} maketi dikmiş — soğuk savaş resmen ısındı! #SektörDedikodu",
            "🔥 {studioA} yeni fragmanında {uniB} logosuna ince bir taş attı, {studioB} hesabının cevabı merakla bekleniyor! #SektörDedikodu",
            "📊 Analistler konuşuyor: {studioA} bu yıl agresif büyümek için küçük stüdyolardan birine teklif götürebilir! #SektörDedikodu",
            "🎙️ Eski bir çalışan röportajda '{studioA} aslında {starA} için bambaşka bir son yazmıştı ama son gün değiştirdiler' dedi! #SektörDedikodu",
            "⚡ {studioB} stüdyosunun yeni motoru sızdırıldı — {studioA} mühendisleri acil toplantıya çağrılmış! #SektörDedikodu"
        };

        // YENI FENOMEN KATEGORILERI (11..15)
        private static readonly string[] RetroNostaljiTemplates =
        {
            "📼 Eskiden '{game}' gibi oyunları kutulu alır, içindeki kitapçığı otobüste eve dönene kadar ezberlerdik... #RetroNostalji",
            "🕹️ '{game}' ilk çıktığı geceyi hatırlayan var mı? {studio} o gün bütün sektörün kurallarını değiştirmişti! #RetroNostalji",
            "💾 Bugünün gençleri bilmez, biz {star} karakterini ilk gördüğümüzde piksel sayardık ama hayal gücümüz 8K idi! #RetroNostalji",
            "📺 Şu an yeni oyunlar harika ama {uni} ilk döneminin o saf, acemi büyüsü bambaşkaydı be... #RetroNostalji",
            "📻 10 yıl sonra dönüp '{game}' için 'Altın çağın başlangıcıydı' diyeceğiz, bugünün kıymetini bilin! #RetroNostalji",
            "🏆 Eski dergilerin verdiği 9.0 puanlar hâlâ duvarımda asılı. {studio} eskiden de deli işler yapıyordu! #RetroNostalji",
            "🎮 Bir nesil {star} replikleriyle büyüdü, şimdi aynı replikleri yeni nesil Tivitır'da meme yapıyor! #RetroNostalji",
            "🌙 Gece monitör ışığında '{game}' oynarken annenin 'Kapat artık uyu!' diye seslendiği o güzel yıllar... #RetroNostalji"
        };

        private static readonly string[] OyunEkonomiTemplates =
        {
            "💰 SEKTÖR EKONOMİSİ: {studio} son başarısından sonra şirket değerlemesini ikiye katlamış! Yatırımcılar kapıda kuyruk! #OyunEkonomi",
            "🛒 İndirim dönemi geldi — '{game}' %40 düşmüş, almayan varsa bu fiyata kaçırılmaz! #OyunEkonomi",
            "📦 {uni} Koleksiyon Sürümü'ne 250$ istemişler ama kutunun içinden {star} heykeli çıkıyormuş... Cüzdanım titriyor 😅 #OyunEkonomi",
            "📈 Bağımsız stüdyolar dev holdinglerden daha yüksek kâr marjıyla çalışıyor! Büyük şirketler panikte! #OyunEkonomi",
            "💎 Merch pazarı patladı: '{star}' figürleri ve tişörtleri neredeyse oyunun kendisi kadar ciro yapıyor! #OyunEkonomi",
            "🏦 Sektörde satın alma rüzgarı! Küçük ama özgün evren kuran stüdyolar şu an altın değerinde! #OyunEkonomi",
            "🎟️ Konuk DLC telifleri tavan yaptı — tek bir popüler karakterin kiralaması küçük bir oyunun bütçesini çıkarıyor! #OyunEkonomi",
            "📉 Hype'a para döküp boş oyun çıkaran devler yine zarar yazdı. Artık oyuncular yemiyor! #OyunEkonomi"
        };

        private static readonly string[] OyunGundemiTemplates =
        {
            "🗣️ GECE SOHBETİ: Oynarken gerçekten ağladığınız veya donup kaldığınız tek bir oyun sahnesi yazın! #OyunGündemi",
            "❓ TOPLULUK SORUSU: Bir oyunda 10/10 hikaye + orta oynanış mı, yoksa 10/10 oynanış + sıfır hikaye mi? #OyunGündemi",
            "🔥 TARTIŞMA: Sevilen karakterler büyük bir finalle ölmeli mi, yoksa her devam oyununda geri dönmeli mi? #OyunGündemi",
            "🎮 ANKET GİBİ SORU: Tek bir kurgusal evrende 1 yıl yaşama şansınız olsa hangisini seçerdiniz — {uniA} mı, {uniB} mi? #OyunGündemi",
            "☕ PAZAR SOHBETİ: Şu an oynadığınız oyunun adını söylemeden sadece 3 kelimeyle anlatın! #OyunGündemi",
            "🧭 BÜYÜK SORU: Sizce bir oyunun süresi 10 saatlik kusursuz tempo mu olmalı, 100 saatlik devasa dünya mı? #OyunGündemi",
            "🎭 HANGİ TİP KÖTÜ ADAM DAHA İYİ: Saf kaos çıkaran psikopatlar mı, yoksa 'aslında biraz haklı' dedirten trajik kötüler mi? #OyunGündemi",
            "🌟 SEKTÖR NABZI: Bu yılın en büyük sürprizini hangi stüdyo yapacak? Benim gizli favorim {studioA}! #OyunGündemi",
            "🎧 OYUN ZEVKİ: Oyun oynarken oyunun kendi müziğini mi dinlersiniz yoksa arkada kendi çalma listenizi mi açarsınız? #OyunGündemi",
            "📚 LORE SORUSU: Hikayeyi doğrudan ara sahnelerle mi seversiniz yoksa etrafa gizlenmiş notları toplayarak çözmeyi mi? #OyunGündemi"
        };

        private static readonly string[] ModAtolyesiTemplates =
        {
            "🧩 MOD VİTRİNİ: Bir hayran '{game}' içindeki bütün düşmanları dev ördeklere çeviren mod yapmış, 50K indirme aldı 😂 #ModAtölyesi",
            "🛠️ '{game}' için 4K piksel-art yenileme modu çıktı! {studio} bu modcuyu acil işe almalı! #ModAtölyesi",
            "⚙️ Topluluk boş durmuyor: {star} karakterini başka oyunlara taşıyan crossover modu bugün trendlerde! #ModAtölyesi",
            "💻 Resmi mod desteği olan oyunlar 10 yıl boyunca ölmüyor! '{game}' mod topluluğu günde 30 yeni içerik basıyor! #ModAtölyesi",
            "🔥 '{game}' için hayran yapımı 8 saatlik yeni hikaye modu yayınlandı — neredeyse resmi DLC kalitesinde! #ModAtölyesi",
            "😂 Birisi '{game}' boss'larının hepsinin boyunu 3 katına çıkaran 'İmkansız Mod' yapmış, yayıncılar çıldırıyor! #ModAtölyesi",
            "🎨 {uni} haritasına dinamik gece-gündüz ve yağmur ekleyen topluluk modu ekran görüntüsü cenneti oldu! #ModAtölyesi",
            "📦 Mod klasörüm oyunun kendi boyutunu geçti, 84 modla '{game}' açmaya çalışırken telefon/PC dua ediyor 😂 #ModAtölyesi"
        };

        private static readonly string[] CizgiLoreTemplates =
        {
            "📚 KİTAP & ÇİZGİ ROMAN: {uni} için çıkan yan hikayeler ana oyunların lore boşluklarını müthiş kapatıyor! #ÇizgiLore",
            "🎬 UYARLAMA HAYALİ: '{game}' yüksek bütçeli bir animasyon dizisi olsa Netflix rekorlarını kırar! #ÇizgiLore",
            "📖 '{star}' karakterinin gençliğini anlatan 300 sayfalık bir roman çıksa tek gecede bitiririm! #ÇizgiLore",
            "🎨 Çizgi roman sayfalarında {uni} evreninin yan karakterlerini görmek bambaşka bir keyif, transmedya işi büyüyor! #ÇizgiLore",
            "🍿 Oyun + Roman + Konsept Albüm... Artık stüdyolar sadece oyun değil, komple kültür imparatorluğu kuruyor! #ÇizgiLore",
            "✍️ {studio} eğer {uni} için resmi lore ansiklopedisi bassa ciltli baskısını raftan indiririm! #ÇizgiLore"
        };

        // KOMBINATORYAL MIKRO-TIVIT MOTORU (20 x 20 x 16 = 6.400+ BENZERSIZ ORGANIK TIVIT!)
        private static readonly string[] ComboOpenings =
        {
            "Az önce fark ettim:", "Kimse kusura bakmasın ama", "Gece 3 düşüncesi:", "Sektörde herkesin gözden kaçırdığı şey:",
            "Dürüst konuşalım,", "Bunu sadece gerçek fanlar bilir:", "Kahvemi yudumlarken aklıma geldi:", "Popüler olmayan bir görüş:",
            "Yıllardır oyun oynarım,", "Bugün yine emin oldum:", "Arkadaş grubunda kavgası çıktı:", "Bir saniye durup düşünün:",
            "Şaka maka", "İtiraf ediyorum:", "Bütün samimiyetimle söylüyorum:", "Erken tahmin bırakıyorum:",
            "Sabaha kadar tartışırız:", "İlk oynadığımda anlamamıştım ama", "Son günlerde dikkatimi çekti:", "Net konuşayım:"
        };

        private static readonly string[] ComboMiddles =
        {
            "{studio} ekibinin {star} karakterine verdiği özen",
            "{uni} evrenindeki o karanlık ve gizemli detaylar",
            "'{game}' oyunundaki karakterler arası kimya",
            "{star} karakterinin her sahnede rol çalması",
            "{studio} tarafından kurulan bu çoklu hikaye örgüsü",
            "'{game}' finalindeki o beklenmedik ters köşe",
            "{uni} müzikleri ile hikaye temposunun uyumu",
            "{star} için yazılan küçük ama vurucu diyaloglar",
            "{studio} ekibinin topluluğu dinleyip risk alması",
            "'{game}' içindeki yan karakterlerin bile canlı hissettirmesi",
            "{uni} kurallarının baştan sona tutarlı ilerlemesi",
            "{star} geçmişindeki çözülmeyi bekleyen o büyük sır",
            "'{game}' oynarken zamanın nasıl geçtiğini anlamamak",
            "{studio} cephesinden gelen her yeni hamle",
            "{uni} hayranlarının ürettiği çılgın teoriler",
            "{star} karakterinin gelişim arkı",
            "'{game}' atmosferinin insana bıraktığı o garip his",
            "{studio} tarzındaki cesur evren mimarisi",
            "{uni} içindeki ekipler arası gerilim",
            "{star} sahneye girdiği an havanın değişmesi"
        };

        private static readonly string[] ComboClosings =
        {
            "bu sektörün neden hâlâ heyecan verici olduğunun kanıtı! {tag}",
            "gerçekten ders olarak okutulmalı. {tag}",
            "insanı sıradaki oyun için aşırı heyecanlandırıyor! {tag}",
            "benim yılın favorileri listeme direkt girdi! {tag}",
            "rakiplerin uykusunu fena kaçırıyordur 😂 {tag}",
            "uzun zamandır gördüğüm en taze iş! {tag}",
            "hakkında 2 saatlik video çekilse sıkılmadan izlerim! {tag}",
            "bütün topluluğu tek gecede ayağa kaldırdı! {tag}",
            "oyun dünyasında nadir görülen bir kalite! {tag}",
            "bana yeniden oyunlara aşık olma hissi verdi. {tag}",
            "kesinlikle daha fazla konuşulmayı hak ediyor! {tag}",
            "ileride klasik diye anılacak, yazın bunu bir kenara! {tag}",
            "yüzünden bütün hafta sonu planım iptal oldu 😅 {tag}",
            "tam bir 'iyi ki bu evrene girmişim' anı! {tag}",
            "bence ödül töreninde fırtına estirecek! {tag}",
            "diğer stüdyolara resmen meydan okuma gibi! {tag}"
        };

        private static readonly string[] PlayerOrganicFanTemplates =
        {
            "Dürüst olalım: {playerStudio} son oyun '{lastGame}' ile çıtayı fena yükseltti, sıradakini nasıl aşacaklar merakla bekliyorum! {pTag}",
            "{pTag} burada mı?! {charName} karakterinin {playerUni} içindeki gidişatına bayılıyorum, sakın harcamayın! {pTag}",
            "Eskiden ufak bir garaj stüdyosuydu, şimdi {playerStudio} devlerle kafa kafaya çarpışıyor! '{lastGame}' hâlâ favorim! {pTag}",
            "{playerUni} için 20 dakikalık teori flood'u yazdım: {charName} aslında ilk oyundan beri her şeyi planlıyordu! {pTag}",
            "{playerHandle} yeni oyundan tek bir ekran görüntüsü bile atsanız {pTag} olarak 1 hafta onu analiz ederiz, ses verin! 👀",
            "Arkadaşıma '{lastGame}' önerdim, 2 gündür telefonunu açmıyor — {playerUni} evrenine bir kurban daha verdik 😂 {pTag}",
            "Şu {charName} figürü çıksa masamın baş köşesine koyarım, {playerStudio} merch işini büyütün lütfen! {pTag}",
            "{playerUni} evreninin müzikleri, karakterleri, kuralları... {playerStudio} bu işi cidden aşkla yapıyor! {pTag}",
            "Biri {playerStudio} ekibine söylesin: '{lastGame}' yüzünden başka oyunlardan zevk alamıyorum artık! {pTag}",
            "Yıllar sonra 'Ben {playerStudio} henüz 1 kişiyken buradaydım' diye hava atacağım! {pTag}",
            "{charName} karakteri için özel tişört bastırdım, sokakta iki kişi '{pTag}!' diye selam verdi 😂",
            "'{lastGame}' finalinden sonra 15 dakika boş ekrana baktım... {playerUni} yazarları kalbimizi söktü! {pTag}",
            "{playerHandle} ne zaman yeni bir şey paylaşsa bildirim sesinden kalbim hızlanıyor! {pTag}",
            "Eğer {playerStudio} bir gün '{lastGame}' için devam oyunu duyurursa okuldan/işten izin alırım! {pTag}",
            "Bütün arkadaş grubunu {pTag} yaptım, akşamları {charName} replikleriyle konuşuyoruz 😂",
            "{playerUni} wikisinde 40 yeni sayfa açılmış — bu topluluk cidden durdurulamıyor! {pTag}",
            "Diğer dev holdingler milyonlar harcarken {playerStudio} ruhuyla hepsini solluyor! {pTag}",
            "'{lastGame}' içindeki en sevdiğiniz {charName} anı neydi? Yorumlarda nostalji yapalım! {pTag}"
        };

        private static readonly string[] FanWorkTemplates =
        {
            "🎨 {charName} için tam 3 haftada hazırladığım {kind} bitti!! {playerHandle} lütfen görür müsünüz? {pTag}",
            "✨ Gece uyumayıp {charName} temalı {kind} yaptım! {pTag} ailesi nasıl olmuş, puanlayın! {playerHandle}",
            "🖌️ {playerUni} sevgim taştı ve ortaya bu {charName} {kind} çıktı! Beğenirseniz yenisi yolda! {playerHandle} {pTag}",
            "🔥 Haftalardır uğraştığım {charName} {kind} sonunda yayında! {playerHandle} bir RT'nizi alırım! {pTag}",
            "🎭 Topluluk etkinliği için {charName} {kind} hazırladık, bütün ekip bayıldı! {playerHandle} {pTag}",
            "💫 Defter kenarına çizmeye başlamıştım, 20 saat sonra dev bir {charName} {kind} oldu! {playerHandle} {pTag}",
            "🎬 {playerUni} hayran kulübü olarak ortaklaşa {charName} {kind} çıkardık! Desteklerinizi bekliyoruz! {pTag}",
            "🏆 İlk defa bu kadar uğraştım: Karşınızda {charName} için el emeği {kind}! {playerHandle} {pTag}"
        };

        private static readonly string[] FanWorkKinds =
        {
            "Pixel-Art Portresi", "Cosplay Kostümü", "Hayran Modu", "Konsept Çizgi Romanı",
            "3D Diorama Figürü", "Rap Tribute Klibi", "Kısa Animasyon", "Alternatif Afiş Tasarımı",
            "8-Bit Müzik Cover'ı", "El Yapımı Peluş", "Duvar Resmi", "Kısa Hayran Romanı",
            "Özel Kart Destesi", "Stop-Motion Videosu"
        };

        private static readonly string[] RpDayTemplates =
        {
            "Yazarlar çay almaya gitti, hesabı ben devraldım! Ben {name}. Sıradaki oyunda bana iyi davranmazlarsa senaryoyu kendim yazarım! {pTag}",
            "Selam {pTag}, ben {name}. Bugün tüm soruları ben yanıtlıyorum — spoiler yasak dediler ama kahve ısmarlayana küçük ipucu veririm 😉",
            "Resmi hesap artık {name} kontrolünde! Admin şifreyi '123456' koymuş, evreni kurtarmak yine bana kaldı 😂 {pTag}",
            "Ben {name}. Duyduğuma göre bazı hayranlar sıradaki oyunda bana ihanet edeceğimi konuşuyormuş... İsimlerinizi tek tek not aldım! {pTag}",
            "Bugün stüdyo koltuğunda ben {name} oturuyorum! İlk icraatım: Kendime +%200 karizma ve sınırsız ekran süresi! {pTag}",
            "Hey {pTag}! Ben {name}. Çayınızı alın gelin, bugün diğer karakterlerin arkasından dedikodu yapıyoruz — admin uyanmadan yetişin! 🔥",
            "Admin beni tek başıma ofiste bıraktı, büyük hata! Ben {name} — sıradaki oyunun adını değiştirip kendi adımı koyuyorum! {pTag}",
            "Merhaba {pTag}, ben {name}. Tier List'lerde beni A-Tier'a koyan arkadaşla akşam 1v1 atıyoruz, konum at! 😂",
            "Stüdyo hesabından canlı bildiriyorum: Ben {name}. Yazarlar bana hâlâ zam yapmadı, grev başlatıp yan evrene kaçacağım! {pTag}",
            "Ben {name}. Fan-artlarınızın hepsini tek tek beğendim ama şu beni ağlarken çizen arkadaşla özel görüşeceğiz 😅 {pTag}",
            "Dikkat! {name} yayın başında. Bugün tek bir kural var: Bana laf atan rakip karakterlere toplu Ratio atıyoruz! {pTag}",
            "Selamlar {pTag}! Ben {name}. Oyundaki gizli sonun şifresini yazacaktım ama admin klavyemi çekti, yetişemedim 😭"
        };

        private static readonly string[] BurnerSelfTemplates =
        {
            "Herkes uyuyor ama {uni} bence son 10 yılın en derin lore'u! Şu detayları kimse fark etmemiş {pTag}",
            "Tarafsız bir oyuncu olarak söylüyorum: {studio} şu an piyasadaki bütün devlerden daha cesur işler çıkarıyor! {pTag}",
            "Dün gece {uni} evrenine şans verdim ve bağımlısı oldum, bu kadar az konuşulması suç resmen! {pTag}",
            "Beni linçleyeceksiniz ama {studio} bu gidişle 2 yıla tüm rakipleri tek tek cebinden çıkarır, yazın bunu! {pTag}",
            "Hiç reklamını görmedim ama {uni} karakter yazımı acayip iyiymiş, arkadaş grubuma zorla başlattım! {pTag}",
            "Objektif konuşayım: {studio} hem müzik hem hikaye hem mizahı tek potada eritmiş, saygı duydum! {pTag}",
            "Şu {uni} evrenini hâlâ denemeyen arkadaşlarım var, resmen çağın gerisinde kalmışlar 😂 {pTag}",
            "Bir arkadaş önerdi diye {studio} oyununu açtım, 6 saat sonra hâlâ başındayım! Müthiş iş! {pTag}",
            "Sektörde özgünlük kalmadı diyenler gidip bir {uni} oynasın, sonra tekrar konuşalım! {pTag}",
            "Tamamen bağımsız bir vatandaş olarak yazıyorum: {studio} yılın stüdyosu ödülünü gözü kapalı hak ediyor! {pTag}"
        };

        private static readonly string[] BurnerSmearTemplates =
        {
            "Dürüst olalım {target} son zamanlarda fena bozdu, {tUni} artık kendini tekrar ediyor...",
            "Eskiden {target} hayranıydım ama son oyunları bildiğin kopyala-yapıştır olmuş, {tUni} büyüsünü kaybetti.",
            "{target} yine hype pompalıyor ama içi boş çıkacak gibi... {tUni} hayranları bu sefer uyanmalı.",
            "Sadece ben mi fark ettim yoksa {target} son zamanlarda bütün fikirlerini rakiplerden mi aşırıyor? 👀",
            "{target} oyunlarına verilen puanları aklım almıyor, 2 saat oynayıp kapattım. Aşırı şişirilmiş!",
            "Bence {tUni} çoktan miadını doldurdu, {target} sadece eski şöhretinin ekmeğini yiyor...",
            "{target} geliştiricileri oyun yapmak yerine sürekli reklam basıyor gibi geliyor bana, hiç samimi değil.",
            "Arkadaşlar {target} yeni oyununu almadan önce iki kere düşünün, eski tadı gram kalmamış!"
        };

        private static readonly string[] BurnerCaughtTemplates =
        {
            "💀 YILIN İFŞASI! {playerHandle} hesap değiştirmeyi unutup resmi hesaptan kendi oyununa 'Gelmiş geçmiş en iyi stüdyo ağlayın' yazdı!!",
            "📸 YAKALANDINIZ! {playerHandle} yan hesaba geçmeyi unutup resmi profilden 'Bu stüdyo çok yakışıklı ve zeki' yazıp 2 saniyede sildi!",
            "🚨 SKANDAL! {playerHandle} admini gizli hesap yerine kendi mavi tikli hesabından rakip gömmeye kalktı, bütün internet SS aldı!",
            "😂 KOMEDİ! {playerHandle} az önce kendi tivitinin altına resmi hesaptan 'Ben bağımsız bir oyuncuyum ve bu oyuna bayıldım' yazdı!",
            "🔥 İNTERNET YIKILDI! {playerHandle} yan hesap operasyonu elinde patladı — 'Kendi kendini öven stüdyo' etiketi TT oldu!",
            "🤡 TAVSİYE: {playerHandle} ekibi yan hesap açmadan önce tarayıcı sekmelerini ayırsın, az önce canlı canlı yakalandılar!"
        };

        private static string CleanHandle(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "@StudyoResmi";
            var sb = new StringBuilder("@");
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
            }
            return sb.Length > 1 ? sb.ToString() : "@StudyoResmi";
        }

        public static string CleanTag(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "#Evren";
            var sb = new StringBuilder("#");
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
            }
            return sb.Length > 1 ? sb.ToString() : "#Evren";
        }

        private static string GetPlayerStudioHandle(Company comp)
        {
            string nm = (comp != null && !string.IsNullOrWhiteSpace(comp.name)) ? comp.name : "SeninStudyon";
            return CleanHandle(nm);
        }

        private static string GetPlayerPrimaryTag()
        {
            var u = UniverseSaveManager.Current;
            if (!string.IsNullOrWhiteSpace(u.UniverseFandomName)) return CleanTag(u.UniverseFandomName);
            if (!string.IsNullOrWhiteSpace(u.UniverseName)) return CleanTag(u.UniverseName);
            return "#YeniEvren";
        }

        public static string GenerateTweetIdea(Company comp)
        {
            var u = UniverseSaveManager.Current;
            var indep = HoldingEndgameSystem.GetIndependentRivals();
            var specials = UniverseSaveManager.GetSpecialCharacters();
            string rName = indep.Count > 0 ? indep[NextSeed(comp, indep.Count)].Name : "NJG Games";
            string cName = specials.Count > 0 ? specials[NextSeed(comp, specials.Count)].Name : "Başrolümüz";
            string pTag = GetPlayerPrimaryTag();

            string[] ideas =
            {
                $"Yakında {u.UniverseName} için dev bir sürpriz geliyor, teorilerinizi şimdiden hazırlayın! {pTag}",
                $"{rName} sıkıyorsa sıradaki oyunumuzla aynı hafta çıkış yapsın, 9+ puanla geliyoruz! {pTag}",
                $"{cName} karakterinin kaderi sıradaki oyunda bambaşka ve çok karanlık bir finale gidiyor... 👀 {pTag}",
                $"{rName} ekibini son oyunları için tebrik ediyoruz, ileride ortak bir crossover neden olmasın? 🤝",
                $"Sıradaki projemiz {cName} odaklı çılgın bir Yan Evren olacak, hayranlar hazır mı?! {pTag}",
                $"Çayımızı aldık, {u.UniverseName} üzerinde sabahlıyoruz! Kuriow bile gelse bizi durduramaz haha {pTag}",
                $"Alternatif Zaman kapıları aralanıyor... {u.UniverseName} tarihinde hiçbir şey eskisi gibi olmayacak! {pTag}",
                $"Stüdyoda {cName} için gizli bir ihanet sahnesi yazdık, ekip bile şokta! Yakında fragman geliyor! {pTag}",
                $"Entakos ve @Fenoctakos duyun sesimizi: {u.UniverseName} lore'u en az EntaVerse kadar derin, kapışalım mı? 🔥",
                $"NJG Games ve #TentiTaym'a selamlar! Feil ile {cName} aynı kütüphanede karşılaşsa ne olurdu acaba? 👀",
                $"Büyük konuşuyoruz: Sıradaki oyunumuz tam bir başyapıt olacak, 8.5+ aşağısını kabul etmiyoruz! {pTag}",
                $"Bugün ofiste kod yazmak yerine 2 saat '{cName} vs {rName} yıldızı kim alır?' diye tartıştık haha 😂 {pTag}",
                $"Yakında dev bir Crossover bombası patlatacağız, bütün evrenler tek çatıda buluşuyor! {pTag}",
                $"Bütün {pTag} ailesine sonsuz teşekkürler! Sizin teorileriniz olmasa {u.UniverseName} bu kadar büyümezdi ❤️",
                $"Sektördeki bazı balon stüdyolar gibi kopyala-yapıştır yapmıyoruz, {rName} biraz ders alsın! {pTag}",
                $"Satın alma masası ısınıyor... Yakında {rName} cephesiyle sürpriz bir transfer görüşmesi olabilir mi? 👀"
            };
            return PickNoRepeat(comp, "idea", ideas);
        }

        public static int GetUnreadDmCount()
        {
            int c = 0;
            var dms = SocialSaveManager.Current.Dms;
            for (int i = 0; i < dms.Count; i++)
                if (!dms[i].Done) c++;
            return c;
        }

        public static bool IsPlayerTrendingNo1(Company comp)
        {
            var trends = ComputeTopTrends(comp);
            return trends.Count > 0 && trends[0].IsPlayerOwned;
        }

        public static List<TrendItem> ComputeTopTrends(Company comp)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var tweets = SocialSaveManager.Current.Tweets;
            int start = Math.Max(0, tweets.Count - 28);
            for (int i = start; i < tweets.Count; i++)
            {
                var t = tweets[i];
                if (t.Del || string.IsNullOrWhiteSpace(t.Hash)) continue;
                int weight = 10 + Math.Min(60, (t.L + t.RT * 2) / 120) + (i - start);
                int cur = 0;
                counts.TryGetValue(t.Hash, out cur);
                counts[t.Hash] = cur + weight;
            }
            if (SocialSaveManager.Current.BoycottActive)
                counts["#Boykot"] = 999;

            var u = UniverseSaveManager.Current;
            string pTag1 = !string.IsNullOrWhiteSpace(u.UniverseFandomName) ? CleanTag(u.UniverseFandomName) : "";
            string pTag2 = !string.IsNullOrWhiteSpace(u.UniverseName) ? CleanTag(u.UniverseName) : "";
            string pTag3 = (comp != null && !string.IsNullOrWhiteSpace(comp.name)) ? CleanTag(comp.name) : "";

            var list = new List<TrendItem>();
            foreach (var kv in counts)
            {
                bool mine = (!string.IsNullOrEmpty(pTag1) && string.Equals(kv.Key, pTag1, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(pTag2) && string.Equals(kv.Key, pTag2, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(pTag3) && string.Equals(kv.Key, pTag3, StringComparison.OrdinalIgnoreCase));
                list.Add(new TrendItem { Tag = kv.Key, Score = kv.Value, IsPlayerOwned = mine });
            }
            list.Sort((a, b) => b.Score.CompareTo(a.Score));
            if (list.Count > 5) list.RemoveRange(5, list.Count - 5);
            return list;
        }

        private static TivitPost PushPost(TivitPost p, bool saveNow = false)
        {
            if (string.IsNullOrEmpty(p.Id)) p.Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            if (p.Replies == null) p.Replies = new List<TivitReply>();
            if (p.Replies.Count > 3) p.Replies.RemoveRange(3, p.Replies.Count - 3);
            SocialSaveManager.Current.Tweets.Add(p);
            if (SocialSaveManager.Current.Tweets.Count > SocialSaveManager.MaxTweets)
                SocialSaveManager.Current.Tweets.RemoveRange(0, SocialSaveManager.Current.Tweets.Count - SocialSaveManager.MaxTweets);
            SocialSaveManager.MarkDirty(saveNow);
            return p;
        }

        private static void PushDm(TivitDm dm, bool saveNow = false)
        {
            if (string.IsNullOrEmpty(dm.Id)) dm.Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            SocialSaveManager.Current.Dms.Add(dm);
            if (SocialSaveManager.Current.Dms.Count > SocialSaveManager.MaxDms)
                SocialSaveManager.Current.Dms.RemoveRange(0, SocialSaveManager.Current.Dms.Count - SocialSaveManager.MaxDms);
            SocialSaveManager.MarkDirty(saveNow);
        }

        public static void AddPromise(double week, string text, string kind, string target)
        {
            var pr = SocialSaveManager.Current.Promises;
            pr.Add(new TivitPromise
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                W = Math.Round(week, 1),
                Text = text.Length > 110 ? text.Substring(0, 110) + "..." : text,
                Kind = kind,
                Target = target ?? "",
                Resolved = false
            });
            if (pr.Count > SocialSaveManager.MaxPromises)
                pr.RemoveRange(0, pr.Count - SocialSaveManager.MaxPromises);
            SocialSaveManager.MarkDirty(true);
        }

        public static void SeedOpeningTimeline(Company comp)
        {
            double w = comp != null ? comp.currentWeek : 0.0;
            PushPost(new TivitPost
            {
                W = w, H = "@Fenoctakos", D = "Fenoctakos (EntaTaym)", V = true, Tag = "INFLUENCER",
                Sid = "rival_entakos", Hash = "#EntaTaym", L = 1840, RT = 420,
                X = "Tivitır açıldıysa ilk büyük teoriyi bırakıyorum: Kuriow ile Curio aslında tek kişi, Alzhe&İmer Kardeşler bunu unuttuğu için kimse fark etmedi! 🔥 #EntaVerse #EntaTaym",
                Replies = new List<TivitReply>
                {
                    new TivitReply { H = "@EntakosGames", D = "Entakos Games ✓", X = "Kardeş sen bizim senaryo odasına kamera mı koydun sus artık 😂", L = 920 }
                }
            });
            PushPost(new TivitPost
            {
                W = w, H = "@TentiArşiv", D = "TentiTaym Arşivi", V = true, Tag = "FANDOM",
                Sid = "rival_njg", Hash = "#TentiTaym", L = 1420, RT = 310,
                X = "NJG Games'in yeni Tentiforverse projesi için nöbetteyiz! Tarı, Feil ve Kyldo Zarın aynı çizgide buluşursa ortalık karışır. #Tentiforverse #TentiTaym"
            });
            PushPost(new TivitPost
            {
                W = w, H = "@OyunMizah", D = "Oyun Mizah", V = true, Tag = "INFLUENCER",
                Hash = "#OyunMizah", L = 1150, RT = 340,
                X = "24 rakip stüdyo, 141 karakter, bir de yeni kurulan garaj stüdyosu... Bu sezon Tivitır'da mısır stokları tükenecek! 🍿 #OyunSektörü"
            });
            PushPost(new TivitPost
            {
                W = w, H = "@PuanAvcısı", D = "Puan Avcısı", V = true, Tag = "INFLUENCER",
                Hash = "#OyunSektörü", L = 980, RT = 190,
                X = "Bakalım kim gerçekten evren kuracak, kim 5.5 alıp 'vizyonumuzu anlamadınız' diye sarı arka planlı özür metni paylaşacak."
            });
        }

        // --- A) GELISMIS SERBEST TIVIT MOTORU (COKLU VARLIK + NIYET + MINI-THREAD + SOZ ARSIVI) ---
        public static string PostPlayerFreeTweet(Company company, string rawText)
        {
            if (company == null) return "Şirket hazır değil.";
            string text = (rawText ?? "").Trim();
            if (text.Length < 3) return "En az 3 karakterlik bir tivit yaz!";
            if (text.Length > 280) text = text.Substring(0, 280);

            var s = SocialSaveManager.Current;
            var u = UniverseSaveManager.Current;
            double week = company.currentWeek;

            s.RecentPlayerTweetWeeks.RemoveAll(tw => (week - tw) > 4.0);
            bool isSpamming = s.RecentPlayerTweetWeeks.Count >= 3;
            s.RecentPlayerTweetWeeks.Add(week);
            s.TotalPosted++;

            string low = text.ToLowerInvariant();
            double mult = isSpamming ? 0.35 : (1.0 + u.DeptPrLevel * 0.15);

            string detectedHash = GetPlayerPrimaryTag();
            int hashPos = text.IndexOf('#');
            if (hashPos >= 0)
            {
                int end = hashPos + 1;
                while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] == '_')) end++;
                if (end > hashPos + 1) detectedHash = text.Substring(hashPos, end - hashPos);
            }

            RivalStudioRecord mentionedRival = null;
            RivalStarRecord mentionedRivalStar = null;
            CharacterRecord mentionedOwnChar = null;
            bool mentionedEnta = low.Contains("enta") || low.Contains("fenoctakos") || low.Contains("fenoc");
            bool mentionedTenti = low.Contains("tenti") || low.Contains("njg") || low.Contains("delilik");

            for (int i = 0; i < u.Rivals.Count; i++)
            {
                var r = u.Rivals[i];
                if (r.IsAbsorbedByRival) continue;
                string rFirst = r.Name.Split(' ')[0].ToLowerInvariant();
                if (low.Contains(r.Name.ToLowerInvariant()) ||
                    (rFirst.Length >= 4 && low.Contains(rFirst)) ||
                    low.Contains(r.UniverseName.ToLowerInvariant()) ||
                    (!string.IsNullOrEmpty(r.FandomName) && low.Contains(r.FandomName.ToLowerInvariant())))
                {
                    mentionedRival = r;
                    break;
                }
            }

            if (u.RivalStars != null)
            {
                for (int i = 0; i < u.RivalStars.Count; i++)
                {
                    var rs = u.RivalStars[i];
                    if (rs.Name.Length >= 4 && low.Contains(rs.Name.ToLowerInvariant()))
                    {
                        mentionedRivalStar = rs;
                        if (mentionedRival == null) mentionedRival = UniverseSaveManager.FindRivalById(rs.RivalId);
                        break;
                    }
                }
            }

            for (int i = 0; i < u.Characters.Count; i++)
            {
                var c = u.Characters[i];
                if (c.Name.Length >= 3 && low.Contains(c.Name.ToLowerInvariant()))
                {
                    mentionedOwnChar = c;
                    break;
                }
            }

            bool isDiss = ContainsAny(low, "çöp", "cop", "balon", "ezik", "geçeriz", "geceriz", "kopya", "sıkıyorsa", "sikiyorsa", "kapış", "kapis", "rezil", "batık", "batik", "saçma", "sacma", "ağla", "agla");
            bool isPraise = ContainsAny(low, "tebrik", "efsane", "kral", "saygı", "saygi", "dost", "helal", "harika", "başarı", "basari", "seviyoruz", "müthiş", "muthis");
            bool isTeaser = ContainsAny(low, "yakında", "yakinda", "geliyor", "sürpriz", "surpriz", "gizli", "fragman", "bomba", "duyuru", "hazır olun", "hazir olun", "tarih");
            bool isFate = ContainsAny(low, "ölecek", "olecek", "veda", "ihanet", "final", "karanlık", "karanlik", "ölüm", "olum", "diril", "sonu");
            bool isApology = ContainsAny(low, "özür", "ozur", "yama", "düzelttik", "duzelttik", "haklısınız", "haklisiniz", "dinliyoruz", "telafi");
            bool isCrossBuy = ContainsAny(low, "satın al", "satin al", "crossover", " vs ", "transfer", "konuk", "birleş", "birles", "ortak");
            bool isMeme = ContainsAny(low, "haha", "şaka", "saka", "çay", "cay", "absürt", "absurt", "trol", "patates", "lol", "komik", "kanka");

            string promiseNote = "";
            if (ContainsAny(low, "9+", "9 puan", "10/10", "başyapıt", "basyapit", "rekor", "yılın oyunu", "yilin oyunu", "8.5+"))
            {
                AddPromise(week, text, "HIGH_SCORE", "8.5+ Skor");
                promiseNote = " [📌 Söz Arşivi'ne eklendi: 8.5+ Skor!]";
            }
            else if (ContainsAny(low, "yan evren", "spin-off", "spinoff"))
            {
                AddPromise(week, text, "SIDE_UNI", "Yan Evren");
                promiseNote = " [📌 Söz Arşivi'ne eklendi: Yan Evren!]";
            }
            else if (ContainsAny(low, "alternatif", "paralel zaman"))
            {
                AddPromise(week, text, "ALT_TIME", "Alternatif Zaman");
                promiseNote = " [📌 Söz Arşivi'ne eklendi: Alternatif Zaman!]";
            }
            else if (ContainsAny(low, "crossover", "çarpış", "carpis", "buluş", "bulus"))
            {
                AddPromise(week, text, "CROSSOVER", "Crossover");
                promiseNote = " [📌 Söz Arşivi'ne eklendi: Crossover!]";
            }

            int hypeGain = 0;
            int repGain = 0;
            int baseLikes = Math.Max(45, (int)Math.Round((60 + Math.Min(company.fans, 8000) * 0.45) * mult));
            var replies = new List<TivitReply>();
            var effects = new List<string>();

            if (isSpamming)
            {
                string spRep = PickNoRepeat(company, "spam_rep", new[]
                {
                    "Admin telefonu biraz bırak da oyun yap, 4 haftadır Tivitır'dan çıkmadın!",
                    "Stüdyoda kod yazan var mı yoksa bütün ekip Tivitır'da mı nöbet tutuyor 😂",
                    "Yine bildirim geldi sandım ki fragman çıktı, meğer admin yine sohbet ediyor..."
                });
                replies.Add(new TivitReply { H = "@PuanAvcısı", D = "Puan Avcısı ✓", X = spRep, L = baseLikes / 2 });
                effects.Add("Spam Freni (düşük etkileşim)");
            }
            else if (mentionedRival != null)
            {
                string rH = CleanHandle(mentionedRival.Name);
                string rFan = !string.IsNullOrEmpty(mentionedRival.FandomName) ? $"#{mentionedRival.FandomName}" : $"#{mentionedRival.UniverseName}";
                string starRef = mentionedRivalStar != null ? mentionedRivalStar.Name : mentionedRival.UniverseName;

                if (isDiss)
                {
                    UniverseSaveManager.AdjustRivalRelation(mentionedRival, -12);
                    hypeGain += (int)Math.Round(7 * mult);
                    baseLikes = (int)(baseLikes * 1.9);
                    string rReply = PickNoRepeat(company, "diss_rep", new[]
                    {
                        $"Bize laf yetiştireceğine önce kendi evreninin açıklarını kapat! {starRef} tek başına sizin tüm kadroyu sırtlar.",
                        $"Klavyeden sallamak kolay! Çıkış haftası geldiğinde {starRef} ile sahada görüşürüz!",
                        $"Biz işimize bakıyoruz ama {starRef} hakkında böyle konuşanlar genelde yıl sonunda ödül törenini evden izler 😉"
                    });
                    replies.Add(new TivitReply { H = rH, D = mentionedRival.Name + " ✓", X = rReply, L = baseLikes + 180 });
                    if (mentionedRival.Id == "rival_entakos" || mentionedEnta)
                        replies.Add(new TivitReply { H = "@Fenoctakos", D = "Fenoctakos ✓", X = $"HOOOOP {rFan} TOPLANIN! {starRef} hakkında böyle konuşulmaz, akşam 2 saatlik cevap videosu yayında!", L = baseLikes / 2 + 140 });
                    else
                        replies.Add(new TivitReply { H = "@HypeMuhabiri", D = "Hype Muhabiri ✓", X = $"🍿 Savaş baltaları çıktı! {mentionedRival.Name} cephesinde tansiyon tavan yaptı!", L = baseLikes / 3 + 90 });
                    effects.Add($"{mentionedRival.Name} -12 İlişki, +{hypeGain} Hype");
                }
                else if (isPraise)
                {
                    UniverseSaveManager.AdjustRivalRelation(mentionedRival, +10);
                    repGain += 2;
                    hypeGain += 3;
                    string pReply = PickNoRepeat(company, "praise_rep", new[]
                    {
                        $"Eyvallah dostum! {starRef} cephesinden selamlar, ileride ortak bir şeyler neden olmasın? 🤝",
                        $"Büyük saygı! {starRef} ekibi olarak sıradaki oyununuzu merakla bekliyoruz 🔥",
                        $"Sektörde böyle centilmence rekabet görmek şahane, kahveler bizden! ☕"
                    });
                    replies.Add(new TivitReply { H = rH, D = mentionedRival.Name + " ✓", X = pReply, L = baseLikes / 2 + 90 });
                    if (mentionedRival.Id == "rival_entakos" || mentionedEnta)
                        replies.Add(new TivitReply { H = "@Fenoctakos", D = "Fenoctakos ✓", X = "İşte özlediğimiz ortam! #EntaTaym olarak alkışlıyoruz, belki bir Kuriow konuk DLC'si gelir?", L = baseLikes / 3 + 70 });
                    effects.Add($"{mentionedRival.Name} +10 İlişki, +2 İtibar, +3 Hype");
                }
                else if (isCrossBuy)
                {
                    hypeGain += (int)Math.Round(6 * mult);
                    baseLikes = (int)(baseLikes * 1.7);
                    replies.Add(new TivitReply { H = "@SektörFısıltı", D = "Sektör Fısıltı ✓", X = $"🚨 SON DAKİKA: {mentionedRival.Name} ({starRef}) ile gizli görüşme mi var?! Borsa hareketlendi!", L = baseLikes / 2 + 120 });
                    replies.Add(new TivitReply { H = rH, D = mentionedRival.Name + " ✓", X = "Avukatlarımız ve kahve makinemiz şu an fazla mesai yapıyor... Bakalım. 👀", L = baseLikes / 2 });
                    effects.Add($"Dedikodu Patladı (+{hypeGain} Hype)");
                }
                else
                {
                    hypeGain += 4;
                    replies.Add(new TivitReply { H = rH, D = mentionedRival.Name + " ✓", X = $"Adımız geçmiş, {starRef} selam söylüyor! 👀", L = baseLikes / 2 });
                    if (mentionedRival.Id == "rival_entakos")
                        replies.Add(new TivitReply { H = "@Fenoctakos", D = "Fenoctakos ✓", X = "Bu tivitte gizli #EntaVerse göndermesi yakaladım, yarın analizini atıyorum!", L = baseLikes / 3 + 80 });
                    effects.Add($"+4 Hype ({mentionedRival.Name} yanıtladı)");
                }
            }
            else if (mentionedOwnChar != null)
            {
                if (isFate)
                {
                    hypeGain += (int)Math.Round(8 * mult);
                    mentionedOwnChar.Popularity = PopularitySystem.ApplyDelta(mentionedOwnChar.Popularity, 4);
                    baseLikes = (int)(baseLikes * 1.8);
                    string fRep = PickNoRepeat(company, "fate_rep", new[]
                    {
                        $"SAKIN {mentionedOwnChar.Name.ToUpperInvariant()} KARAKTERİNE DOKUNMAYIN!! 3 yıldır profil fotoğrafım o, ağlarım!",
                        $"Eğer {mentionedOwnChar.Name} ölürse bu akşam Tivitır'ı kapatıp dağa çıkıyorum, şaka yapmıyorum!!",
                        $"{mentionedOwnChar.Name} ihanet ederse bütün lore defterimi yakarım, yapmayın etmeyin! 😭"
                    });
                    replies.Add(new TivitReply { H = FanHandles[NextSeed(company, FanHandles.Length)], D = $"{mentionedOwnChar.Name} Fan Club", X = fRep, L = baseLikes / 2 + 110 });
                    replies.Add(new TivitReply { H = "@LoreKazıcısı", D = "Lore Kazıcısı ✓", X = $"Bu cümle kesin bir Final Fedası veya İhanet Arkı sinyali... {mentionedOwnChar.Name} için mendilleri hazırlayın!", L = baseLikes / 3 + 75 });
                    effects.Add($"{mentionedOwnChar.Name} +Pop, +{hypeGain} Hype (Kader Paniği!)");
                }
                else
                {
                    hypeGain += (int)Math.Round(5 * mult);
                    mentionedOwnChar.Popularity = PopularitySystem.ApplyDelta(mentionedOwnChar.Popularity, 3);
                    string cRep = PickNoRepeat(company, "char_rep", new[]
                    {
                        $"{mentionedOwnChar.Name} nihayet gündemde! Yeni oyunda bol ekran süresi istiyoruz!",
                        $"{mentionedOwnChar.Name} resmen evrenin bel kemiği oldu, solo oyun bile yakışır!",
                        $"@TierListçi duysun: {mentionedOwnChar.Name} artık tartışmasız S-Tier!"
                    });
                    replies.Add(new TivitReply { H = FanHandles[NextSeed(company, FanHandles.Length)], D = $"{mentionedOwnChar.Name} Hayranı", X = cRep, L = baseLikes / 2 + 50 });
                    effects.Add($"{mentionedOwnChar.Name} +Pop, +{hypeGain} Hype");
                }
            }
            else if (isApology && s.BoycottActive)
            {
                s.BoycottActive = false;
                repGain += 3;
                replies.Add(new TivitReply { H = "@PuanAvcısı", D = "Puan Avcısı ✓", X = "En azından çıkıp doğrudan yazdınız. Gözümüz üzerinizde ama şimdilik yangın söndü.", L = baseLikes / 2 });
                effects.Add("Boykot Yatıştı (+3 İtibar)");
            }
            else if (isTeaser)
            {
                hypeGain += (int)Math.Round(6 * mult);
                UniverseSaveManager.AdjustLoreBarrier(-3);
                string tRep = PickNoRepeat(company, "teaser_rep", new[]
                {
                    $"Bu tivitteki her harfi inceledim, {u.UniverseName} evreninde dev bir kırılma geliyor! 3 saatlik video yolda!",
                    $"Sadece iki cümleyle bütün topluluğu ayağa kaldırdınız! Fragman tarihi ne zaman?! 🔥",
                    $"Gecenin bu saatinde böyle gizemli tivit atılır mı, uyku düzenimiz gitti! 👀"
                });
                replies.Add(new TivitReply { H = "@LoreKazıcısı", D = "Lore Kazıcısı ✓", X = tRep, L = baseLikes / 2 + 95 });
                effects.Add($"+{hypeGain} Hype, Bariyer -3");
            }
            else if (isMeme)
            {
                hypeGain += (int)Math.Round(4 * mult);
                repGain += 1;
                string mRep = PickNoRepeat(company, "meme_rep", new[]
                {
                    "Ahahaha admin yine formunda, akşam yayında bunu konuşuruz 😂",
                    "Böyle samimi stüdyo hesaplarına can kurban, kahkaha attım!",
                    "Ekran görüntüsü aldım, yarın sabahın en çok RT alan meme'i bu olur!"
                });
                replies.Add(new TivitReply { H = "@OyunMizah", D = "Oyun Mizah ✓", X = mRep, L = baseLikes / 2 + 60 });
                effects.Add($"+{hypeGain} Hype, +1 İtibar (Meme)");
            }
            else
            {
                hypeGain += (int)Math.Round(3 * mult);
                string fLbl = !string.IsNullOrEmpty(u.UniverseFandomName) ? u.UniverseFandomName : "Topluluk";
                string gRep = PickNoRepeat(company, "gen_rep", new[]
                {
                    $"Her paylaşımınızı pusuda bekliyoruz, {u.UniverseName} durmasın!",
                    $"Bildirimleri açtık bekliyoruz, stüdyo ekibine kolay gelsin! 🔥",
                    $"{u.UniverseName} şu an sektörün en heyecanlı projesi, aynen devam!"
                });
                replies.Add(new TivitReply { H = FanHandles[NextSeed(company, FanHandles.Length)], D = $"{fLbl} Üyesi", X = gRep, L = baseLikes / 3 + 25 });
                effects.Add($"+{hypeGain} Hype");
            }

            if (mentionedTenti && (mentionedRival == null || mentionedRival.Id != "rival_njg"))
            {
                replies.Add(new TivitReply { H = "@TentiArşiv", D = "TentiTaym Arşivi ✓", X = "#TentiTaym radarına girdiniz! Tentiforverse kapısı her zaman gizemlidir 👀", L = baseLikes / 3 + 55 });
            }

            var topTrends = ComputeTopTrends(company);
            if (topTrends.Count > 0 && string.Equals(topTrends[0].Tag, detectedHash, StringComparison.OrdinalIgnoreCase))
            {
                hypeGain += 3;
                baseLikes = (int)(baseLikes * 1.4);
                effects.Add($"TT #1 Rüzgarı ({detectedHash} +3 Hype)");
            }

            if (hypeGain != 0) company.adjustHype(hypeGain);
            if (repGain != 0) UniverseSaveManager.AdjustReputation(repGain);

            PushPost(new TivitPost
            {
                W = Math.Round(week, 1),
                H = GetPlayerStudioHandle(company),
                D = (company.name ?? "Senin Stüdyon"),
                V = true,
                Tag = "PLAYER",
                CharN = mentionedOwnChar != null ? mentionedOwnChar.Name : "",
                X = text,
                Hash = detectedHash,
                L = baseLikes,
                RT = Math.Max(8, baseLikes / 4),
                Replies = replies
            }, true);

            UniverseSaveManager.Save();
            return $"𝕏 Tivit gönderildi! {string.Join(" | ", effects)}{promiseNote}";
        }

        // --- B) TIVIT SIL (SS YAKALANMA RISKI!), RATIO AT & FAN-ART MUDAHALE ---
        public static string DeletePlayerTweet(Company company, TivitPost post)
        {
            if (post == null || post.Del || post.Tag != "PLAYER") return "Bu tivit silinemez.";
            post.Del = true;
            double r = 0.5;
            try { r = company != null ? company.getRandom() : 0.5; } catch { }
            if (r < 0.25)
            {
                UniverseSaveManager.AdjustReputation(-4);
                PushPost(new TivitPost
                {
                    W = company != null ? Math.Round(company.currentWeek, 1) : 0,
                    H = "@SektörFısıltı", D = "Sektör Fısıltı ✓", V = true, Tag = "INFLUENCER",
                    Hash = "#Yakalandı", L = 2400, RT = 980,
                    X = $"📸 ÇOK GEÇ KRAL! {GetPlayerStudioHandle(company)} attığı tiviti 3 saniyede sildi ama ekran görüntüsü bizde! \"{post.X}\""
                }, true);
                UniverseSaveManager.Save();
                return "⚠ Tiviti sildin ama @SektörFısıltı ekran görüntüsü almıştı! (-4 İtibar)";
            }
            SocialSaveManager.MarkDirty(true);
            return "Tivit sessizce silindi — kimse fark etmedi.";
        }

        public static string TryRatioRivalPost(Company company, TivitPost post)
        {
            if (company == null || post == null || !post.CanRatio || post.RatioDone) return "Bu tivite zaten hamle yapıldı.";
            post.RatioDone = true;
            var u = UniverseSaveManager.Current;
            var rival = UniverseSaveManager.FindRivalById(post.Sid);

            double myScore = u.ReleasedGames.Count > 0 ? u.ReleasedGames[u.ReleasedGames.Count - 1].FinalScore : 6.8;
            double rivalScore = (rival != null && rival.LastScore > 0.1) ? rival.LastScore : 7.3;
            double prEdge = u.DeptPrLevel * 0.35;
            double rnd = 0.5;
            try { rnd = company.getRandom(); } catch { }

            bool won = (myScore + prEdge + (rnd - 0.45) * 1.0) >= rivalScore;
            if (won)
            {
                company.adjustHype(12);
                long stolen = rival != null ? Math.Max(30, (long)Math.Round(rival.Fans * 0.05)) : 40;
                if (rival != null)
                {
                    rival.Fans = Math.Max(50, rival.Fans - stolen);
                    UniverseSaveManager.AdjustRivalRelation(rival, -14);
                }
                company.adjustFans(stolen);
                UniverseSaveManager.AdjustReputation(+2);
                string rWin = PickNoRepeat(company, "ratio_win", new[]
                {
                    $"Önce {myScore:F1} puanlık bir oyun çıkarın, sonra konuşuruz. Ratio + L! 🔥",
                    $"Sektörde ders vermeye geldik: {myScore:F1} puan ortada, gerisi laf salatası! Ratio!",
                    $"Kusura bakmayın ama {myScore:F1} varken burada söz hakkı bizde 😎 Ratio!"
                });
                post.Replies.Insert(0, new TivitReply
                {
                    H = GetPlayerStudioHandle(company), D = company.name + " ✓",
                    X = rWin, L = post.L + 1450
                });
                if (post.Replies.Count > 3) post.Replies.RemoveAt(3);
                SocialSaveManager.MarkDirty(true);
                UniverseSaveManager.Save();
                return $"⚔ EFSANE RATIO! {post.D} yerle bir oldu (+12 Hype, +{stolen} Hayran, +2 İtibar)!";
            }
            else
            {
                company.adjustHype(-8);
                UniverseSaveManager.AdjustReputation(-5);
                if (rival != null) UniverseSaveManager.AdjustRivalRelation(rival, -10);
                string rLose = PickNoRepeat(company, "ratio_lose", new[]
                {
                    $"Son oyunun {myScore:F1} almışken {post.D}'a Ratio denemek mi? Ters Ratio yedin geçmiş olsun 😂",
                    $"Ah be {GetPlayerStudioHandle(company)}, {myScore:F1} skorla masaya yumruk vurursan elin acır 💀",
                    $"Kendi tivitinden daha az beğeni alan Ratio denemesi tarihe geçti 😂"
                });
                post.Replies.Insert(0, new TivitReply
                {
                    H = "@PuanAvcısı", D = "Puan Avcısı ✓", X = rLose, L = post.L + 1100
                });
                if (post.Replies.Count > 3) post.Replies.RemoveAt(3);
                SocialSaveManager.MarkDirty(true);
                UniverseSaveManager.Save();
                return "💀 TERS RATIO YEDİN! Kalabalık seninle dalga geçiyor (-8 Hype, -5 İtibar).";
            }
        }

        public static string HandleFanWorkPost(Company company, TivitPost post, int action)
        {
            if (company == null || post == null || post.FanActDone) return "Bu hayran işine zaten karar verdin.";
            post.FanActDone = true;
            if (action == 0)
            {
                company.adjustHype(3);
                UniverseSaveManager.AdjustReputation(+1);
                post.RT += 350;
                post.L += 900;
                SocialSaveManager.MarkDirty(true);
                UniverseSaveManager.Save();
                return "❤️ Hayran çalışmasını RT ettin! (+3 Hype, +1 İtibar)";
            }
            else if (action == 1)
            {
                if (company.cash < 5000.0) { post.FanActDone = false; return "Ödül & Kanon tescili için 5K nakit gerekiyor!"; }
                company.adjustCash(-5000.0, "Hayran İşini Kanona Alma Ödülü");
                UniverseSaveManager.AdjustLoreBarrier(-8);
                UniverseSaveManager.AdjustReputation(+4);
                long gain = Math.Max(35, (long)Math.Round(company.fans * 0.04));
                company.adjustFans(gain);
                post.Replies.Insert(0, new TivitReply { H = GetPlayerStudioHandle(company), D = company.name + " ✓", X = "Bu harika çalışma artık resmi kanonun bir parçası! 🏆", L = post.L + 600 });
                if (post.Replies.Count > 3) post.Replies.RemoveAt(3);
                SocialSaveManager.MarkDirty(true);
                UniverseSaveManager.Save();
                return $"⭐ Resmi Kanona alındı (-5K)! Bariyer -8, +4 İtibar, +{gain} Hayran!";
            }
            else
            {
                company.adjustCash(5000.0, "Hayran İşi Telif Tazminatı");
                UniverseSaveManager.AdjustIntegrity(+3);
                UniverseSaveManager.AdjustReputation(-7);
                TriggerBoycott(company, "Kendi hayranına telif davası açtın!");
                SocialSaveManager.MarkDirty(true);
                UniverseSaveManager.Save();
                return "⚖️ Telif attın (+5K, +3 Tut.) AMA #Boykot krizi patladı (-7 İtibar)!";
            }
        }

        // --- C) GIZLI YAN HESAP (BURNER), KARAKTER RP GUNU, YAYINCI SPONSORLUGU, ANKET, AMA, TURNUVA ---
        public static string UseBurnerAccount(Company company, bool smearRival)
        {
            if (company == null) return "Şirket hazır değil.";
            var s = SocialSaveManager.Current;
            double week = company.currentWeek;
            s.BurnerHeat = Math.Clamp(s.BurnerHeat + 22, 0, 100);

            double roll = 1.0;
            try { roll = company.getRandom(); } catch { }
            double catchChance = Math.Clamp((s.BurnerHeat - 12) / 100.0, 0.08, 0.72);

            if (roll < catchChance)
            {
                s.BurnerHeat = 15;
                UniverseSaveManager.AdjustReputation(-10);
                company.adjustHype(-6);
                TriggerBoycott(company, "Gizli yan hesaptan kendi oyununu överken hesap değiştirmeyi unuttun!");
                string caughtX = PickNoRepeat(company, "burner_caught", BurnerCaughtTemplates)
                    .Replace("{playerHandle}", GetPlayerStudioHandle(company));
                string kaosRep = PickNoRepeat(company, "burner_kaos", new[]
                {
                    "BU AKŞAMKİ YAYININ MALZEMESİ ÇIKTI AHAHAHA 😭",
                    "Çayımı püskürttüm ekrana, tarihin en komik PR kazası 😂",
                    "Admin şu an ofiste istifa dilekçesini yazıyor kesin 💀"
                });
                PushPost(new TivitPost
                {
                    W = Math.Round(week, 1), H = "@SektörFısıltı", D = "Sektör Fısıltı ✓", V = true, Tag = "INFLUENCER",
                    Hash = "#Boykot", L = 6200, RT = 2800, X = caughtX,
                    Replies = new List<TivitReply> { new TivitReply { H = "@YayınKaos", D = "Yayın Kaos ✓", X = kaosRep, L = 2100 } }
                }, true);
                UniverseSaveManager.Save();
                return "💀 YAKALANDIN! Hesap değiştirmeyi unuttun — @SektörFısıltı ifşaladı (-10 İtibar, #Boykot)!";
            }

            if (!smearRival)
            {
                company.adjustHype(6);
                string body = PickNoRepeat(company, "burner_self", BurnerSelfTemplates)
                    .Replace("{uni}", UniverseSaveManager.Current.UniverseName)
                    .Replace("{studio}", company.name ?? "Bu stüdyo")
                    .Replace("{pTag}", GetPlayerPrimaryTag());
                PushPost(new TivitPost
                {
                    W = Math.Round(week, 1), H = FanHandles[NextSeed(company, FanHandles.Length)], D = "Gizli Yan Hesap", V = false, Tag = "BURNER",
                    Hash = GetPlayerPrimaryTag(), L = 340 + NextSeed(company, 280), RT = 75, X = body
                }, true);
                return $"🕵️ Gizli yan hesap iş gördü! +6 Hype (Şüphe Isısı: %{s.BurnerHeat})";
            }
            else
            {
                var indep = HoldingEndgameSystem.GetIndependentRivals();
                if (indep.Count == 0) return "Karalanacak bağımsız rakip kalmadı.";
                indep.Sort((a, b) => b.Fans.CompareTo(a.Fans));
                var target = indep[NextSeed(company, Math.Min(3, indep.Count))];
                long drop = Math.Max(25, (long)Math.Round(target.Fans * 0.04));
                target.Fans = Math.Max(50, target.Fans - drop);
                company.adjustHype(4);
                string body = PickNoRepeat(company, "burner_smear", BurnerSmearTemplates)
                    .Replace("{target}", target.Name)
                    .Replace("{tUni}", target.UniverseName);
                PushPost(new TivitPost
                {
                    W = Math.Round(week, 1), H = FanHandles[NextSeed(company, FanHandles.Length)], D = "Gizli Yan Hesap", V = false, Tag = "BURNER",
                    Sid = target.Id, Hash = CleanTag(target.UniverseName), L = 460 + NextSeed(company, 320), RT = 130, X = body
                }, true);
                UniverseSaveManager.Save();
                return $"🕵️ {target.Name} hakkında gizli karalama tuttu (-{drop} rakip hayran, +4 Hype | Isı: %{s.BurnerHeat})";
            }
        }

        public static string TriggerCharacterRpDay(Company company)
        {
            if (company == null) return "Şirket hazır değil.";
            var s = SocialSaveManager.Current;
            double wait = 10.0 - (company.currentWeek - s.LastRpW);
            if (wait > 0.1) return $"Yeni Karakter RP Günü için ~{wait:F1} hafta bekle.";

            var specials = UniverseSaveManager.GetSpecialCharacters();
            var alive = new List<CharacterRecord>();
            for (int i = 0; i < specials.Count; i++)
                if (CategoryLabels.ParseFate(specials[i].FateName) == CharacterFate.Alive) alive.Add(specials[i]);
            if (alive.Count == 0) return "Önce kadrona en az 1 canlı Özel Karakter eklemelisin.";

            // En populer 4 yildiz arasindan sirayla sec ki hep ayni karakter cikmasin!
            alive.Sort((a, b) => b.Popularity.CompareTo(a.Popularity));
            int topPool = Math.Min(4, alive.Count);
            var star = alive[NextSeed(company, topPool)];

            s.LastRpW = company.currentWeek;
            star.Popularity = PopularitySystem.ApplyDelta(star.Popularity, 5);
            company.adjustHype(7);

            string rpBody = PickNoRepeat(company, "rp_day", RpDayTemplates)
                .Replace("{name}", star.Name)
                .Replace("{pTag}", GetPlayerPrimaryTag());
            string rpReply = PickNoRepeat(company, "rp_reply", new[]
            {
                $"EFSANE PAZARLAMA! {star.Name} gerçekmiş gibi cevap veriyor 😂",
                $"Admin sakın hesabı geri alma, {star.Name} çok daha iyi yönetiyor! 🔥",
                $"Bu tiviti çerçeveletip odama asacağım, {star.Name} kraldır!"
            });

            PushPost(new TivitPost
            {
                W = Math.Round(company.currentWeek, 1),
                H = GetPlayerStudioHandle(company), D = $"🎭 {star.Name} (Hesabı Ele Geçirdi!)", V = true, Tag = "PLAYER",
                CharN = star.Name, Hash = CleanTag(star.Name), L = 1950 + star.Popularity * 22, RT = 620,
                X = rpBody,
                Replies = new List<TivitReply> { new TivitReply { H = "@LoreKazıcısı", D = "Lore Kazıcısı ✓", X = rpReply, L = 540 } }
            }, true);
            UniverseSaveManager.Save();
            return $"🎭 {star.Name} bugün Tivitır hesabını ele geçirdi! (+5 Popülerlik, +7 Hype)";
        }

        public static string SponsorYayinKaos(Company company)
        {
            if (company == null) return "Şirket hazır değil.";
            var s = SocialSaveManager.Current;
            if (s.SponsorKaosActive) return "@YayınKaos zaten sıradaki oyunun için sponsorlu bekliyor!";
            if (company.cash < 15000.0) return "@YayınKaos erken erişim sponsorluğu 15K tutar!";

            company.adjustCash(-15000.0, "@YayınKaos Erken Erişim Sponsorluğu");
            s.SponsorKaosActive = true;
            company.adjustHype(8);
            PushPost(new TivitPost
            {
                W = Math.Round(company.currentWeek, 1), H = "@YayınKaos", D = "Yayın Kaos ✓", V = true, Tag = "INFLUENCER",
                Hash = GetPlayerPrimaryTag(), L = 3200, RT = 740,
                X = $"🔥 ERKEN ERİŞİM KODU GELDİ! {GetPlayerStudioHandle(company)} sıradaki oyununu ilk biz canlı yayında kıracağız! Kötü çıkarsa acımam ona göre 😂 {GetPlayerPrimaryTag()}"
            }, true);
            UniverseSaveManager.Save();
            return "🎮 @YayınKaos sponsorlandı (-15K, +8 Hype)! Sıradaki oyun 7.5+ alırsa dev hayran dalgası, 6.0 altı alırsa bug şovu!";
        }

        public static string StartCommunityPoll(Company company)
        {
            if (company == null) return "Şirket hazır değil.";
            var s = SocialSaveManager.Current;
            double wait = 16.0 - (company.currentWeek - s.LastPollW);
            if (wait > 0.1) return $"Yeni Topluluk Anketi için ~{wait:F1} hafta bekle.";

            s.LastPollW = company.currentWeek;
            double r = 0.5;
            try { r = company.getRandom(); } catch { }
            string kind, label;
            if (r < 0.34) { kind = "SIDE_UNI"; label = "Yan Evren (Spin-Off)"; }
            else if (r < 0.67) { kind = "ALT_TIME"; label = "Alternatif Zaman"; }
            else { kind = "CROSSOVER"; label = "Dev Crossover"; }

            AddPromise(company.currentWeek, $"Anket Kazananı: {label}", kind, label);
            company.adjustHype(5);
            PushPost(new TivitPost
            {
                W = Math.Round(company.currentWeek, 1), H = GetPlayerStudioHandle(company), D = company.name + " ✓", V = true, Tag = "POLL",
                Hash = GetPlayerPrimaryTag(), L = 1650, RT = 410,
                X = $"🗳 TOPLULUK ANKETİ SONUÇLANDI! %64 oyla sıradaki oyunda '{label}' istediniz! Söz Arşivi'ne kaydedildi — yerine getirirsek büyük bonus var! {GetPlayerPrimaryTag()}"
            }, true);
            UniverseSaveManager.Save();
            return $"🗳 Anket bitti: Hayranlar '{label}' seçti (+5 Hype, Söz Arşivi'ne eklendi)!";
        }

        public static string StartLiveAma(Company company)
        {
            if (company == null) return "Şirket hazır değil.";
            var s = SocialSaveManager.Current;
            double wait = 36.0 - (company.currentWeek - s.LastAmaW);
            if (wait > 0.1) return $"Yeni Canlı Soru-Cevap (AMA) için ~{wait:F1} hafta bekle.";
            if (NativePopupQueue.HasPending) return "Önce bekleyen bildirim penceresini kapat.";

            s.LastAmaW = company.currentWeek;
            SocialSaveManager.MarkDirty(true);

            NativePopupQueue.Enqueue(
                "🎙 Tivitır Canlı AMA (1/3) — Hayran Sorusu",
                $"@GeceOyuncusu soruyor: '{UniverseSaveManager.Current.UniverseName}' evreninde ölen karakterler gerçekten ölü mü kalacak, yoksa kapı açık mı?\n\n" +
                "• Kesinlikle Kalıcı (+4 Tutarlılık, +3 İtibar)\n• Çoklu Evren Bu, Her Şey Olur (+8 Hype)",
                new[] { "Ölüm Kalıcıdır (+4 Tut.)", "Her Şey Olur (+8 Hype)" },
                opt =>
                {
                    if (opt == 0) { UniverseSaveManager.AdjustIntegrity(+4); UniverseSaveManager.AdjustReputation(+3); }
                    else company.adjustHype(8);

                    NativePopupQueue.Enqueue(
                        "🎙 Tivitır Canlı AMA (2/3) — Eleştirmen Sorusu",
                        "@PuanAvcısı soruyor: 'Sıradaki oyununuz gerçekten 8.5+ seviyesinde bir başyapıt mı olacak, yoksa yine güvenli liman mı?'\n\n" +
                        "• Söz Veriyoruz: 8.5+ Gelecek! (+10 Hype + Söz Arşivi'ne girer!)\n• Önemli Olan Samimi Hikaye (+4 İtibar)",
                        new[] { "8.5+ Sözü Ver (+10 Hype)", "Mütevazı Kal (+4 İtibar)" },
                        opt2 =>
                        {
                            if (opt2 == 0) { company.adjustHype(10); AddPromise(company.currentWeek, "AMA Sözü: 8.5+ Başyapıt", "HIGH_SCORE", "8.5+ Skor"); }
                            else UniverseSaveManager.AdjustReputation(+4);

                            NativePopupQueue.Enqueue(
                                "🎙 Tivitır Canlı AMA (3/3) — @Fenoctakos Baskını!",
                                "@Fenoctakos canlı yayına bağlandı: 'EntaVerse'deki Kuriow sizin evrene gelse tüm kadronuzu tek başına ezer miydi, dürüst cevap verin!'\n\n" +
                                "• Gelip Denesin! (+8 Hype, Entakos -8 İlişki)\n• Kuriow'a Saygımız Sonsuz (+10 Entakos İlişki, +3 İtibar)",
                                new[] { "Gelip Denesin! (+8 Hype)", "Saygımız Sonsuz (+İlişki)" },
                                opt3 =>
                                {
                                    var enta = UniverseSaveManager.FindRivalById("rival_entakos");
                                    if (opt3 == 0) { company.adjustHype(8); UniverseSaveManager.AdjustRivalRelation(enta, -8); }
                                    else { UniverseSaveManager.AdjustRivalRelation(enta, +10); UniverseSaveManager.AdjustReputation(+3); }

                                    PushPost(new TivitPost
                                    {
                                        W = Math.Round(company.currentWeek, 1), H = GetPlayerStudioHandle(company), D = company.name + " ✓", V = true, Tag = "PLAYER",
                                        Hash = GetPlayerPrimaryTag(), L = 2850, RT = 720,
                                        X = $"🎙 Canlı Tivitır AMA bitti! 3 zorlu soruyu da canlı yayında göğüsledik, katılan herkese ve @Fenoctakos'a teşekkürler! {GetPlayerPrimaryTag()}"
                                    }, true);
                                    UniverseSaveManager.Save();
                                }
                            );
                        }
                    );
                }
            );
            return "🎙 Canlı AMA başladı! Pencereyi kapattığında 3 soru art arda ekrana gelecek.";
        }

        public static string StartCharacterTournament(Company company)
        {
            if (company == null) return "Şirket hazır değil.";
            var s = SocialSaveManager.Current;
            if (s.Tourney != null && s.Tourney.Active) return "Zaten devam eden bir 4'lü 'Kim Alır?' turnuvası var!";

            var specials = UniverseSaveManager.GetSpecialCharacters();
            var alive = new List<CharacterRecord>();
            for (int i = 0; i < specials.Count; i++)
                if (CategoryLabels.ParseFate(specials[i].FateName) == CharacterFate.Alive) alive.Add(specials[i]);
            if (alive.Count == 0) return "Turnuvaya sokmak için en az 1 canlı Özel Karakterin olmalı!";
            alive.Sort((a, b) => b.Popularity.CompareTo(a.Popularity));
            var pStar = alive[NextSeed(company, Math.Min(3, alive.Count))];

            var rStars = new List<RivalStarRecord>();
            var allRs = UniverseSaveManager.Current.RivalStars;
            if (allRs != null)
            {
                for (int i = 0; i < allRs.Count; i++)
                    if (!allRs[i].TransferredToPlayer) rStars.Add(allRs[i]);
            }
            rStars.Sort((a, b) => b.Popularity.CompareTo(a.Popularity));
            if (rStars.Count < 3) return "Turnuva için yeterli rakip yıldız yok!";

            var picks = new List<RivalStarRecord>();
            var usedStudios = new HashSet<string>();
            int offset = NextSeed(company, Math.Min(15, rStars.Count));
            for (int k = 0; k < rStars.Count && picks.Count < 3; k++)
            {
                var cand = rStars[(offset + k) % rStars.Count];
                if (usedStudios.Add(cand.RivalId)) picks.Add(cand);
            }
            if (picks.Count < 3) return "Turnuva için yeterli farklı stüdyo yıldızı yok.";

            s.Tourney = new TivitTournament
            {
                Active = true,
                StartW = company.currentWeek,
                EndW = company.currentWeek + 2.0,
                Names = new[] { pStar.Name, picks[0].Name, picks[1].Name, picks[2].Name },
                Studios = new[] { company.name ?? "Sen", picks[0].RivalName, picks[1].RivalName, picks[2].RivalName },
                Votes = new[] { pStar.Popularity * 12, picks[0].Popularity * 12, picks[1].Popularity * 12, picks[2].Popularity * 12 },
                Result = ""
            };
            company.adjustHype(5);
            PushPost(new TivitPost
            {
                W = Math.Round(company.currentWeek, 1), H = "@TivitırAnket", D = "Tivitır Kim Alır? ✓", V = true, Tag = "TOURNEY",
                Hash = "#KimAlır", L = 3400, RT = 1250,
                X = $"🏆 4'LÜ KARAKTER TURNUVASI BAŞLADI! {pStar.Name} vs {picks[0].Name} ({picks[0].RivalName}) vs {picks[1].Name} ({picks[1].RivalName}) vs {picks[2].Name} ({picks[2].RivalName})! 2 hafta sonra şampiyon belli!"
            }, true);
            return $"🏆 4'lü Turnuva başladı! {pStar.Name} vs {picks[0].Name} vs {picks[1].Name} vs {picks[2].Name} (2 hafta sürecek).";
        }

        public static void TriggerBoycott(Company company, string reason)
        {
            var s = SocialSaveManager.Current;
            s.BoycottActive = true;
            s.BoycottReason = reason;
            s.BoycottEndW = (company != null ? company.currentWeek : 0) + 12.0;
            SocialSaveManager.MarkDirty(true);
        }

        public static string ResolveBoycott(Company company, int choice)
        {
            var s = SocialSaveManager.Current;
            if (!s.BoycottActive) return "Aktif bir boykot krizi yok.";
            if (choice == 0)
            {
                if (company.cash < 12000.0) return "Acil yama & telafi paketi 12K tutar!";
                company.adjustCash(-12000.0, "Boykot Çözümü: Ücretsiz Yama");
                s.BoycottActive = false;
                UniverseSaveManager.AdjustReputation(+5);
                PushPost(new TivitPost { W = Math.Round(company.currentWeek, 1), H = GetPlayerStudioHandle(company), D = company.name + " ✓", V = true, Tag = "PLAYER", Hash = GetPlayerPrimaryTag(), L = 2100, RT = 420, X = "Hatalarımızı kabul ediyor ve ücretsiz dev telafi yamasını hemen yayınlıyoruz. Desteğiniz için teşekkürler!" }, true);
                UniverseSaveManager.Save();
                return "✅ Acil yama yayınlandı (-12K), #Boykot anında söndü (+5 İtibar)!";
            }
            else if (choice == 1)
            {
                double chance = Math.Clamp(0.45 + (UniverseSaveManager.Current.StudioReputation - 50) * 0.008 + UniverseSaveManager.Current.DeptPrLevel * 0.12, 0.25, 0.85);
                double roll = 0.5;
                try { roll = company.getRandom(); } catch { }
                s.BoycottActive = false;
                if (roll <= chance)
                {
                    UniverseSaveManager.AdjustReputation(+2);
                    PushPost(new TivitPost { W = Math.Round(company.currentWeek, 1), H = GetPlayerStudioHandle(company), D = company.name + " ✓", V = true, Tag = "PLAYER", Hash = "#ÖzürMetni", L = 1400, RT = 310, X = "🟨 Sizi duyuyoruz, sizi anlıyoruz... Daha iyisini hak ediyorsunuz. Bize bir şans daha verin." }, true);
                    UniverseSaveManager.Save();
                    return "🟨 'Sizi Duyuyoruz' sarı arka planlı özür metni tuttu! Boykot bitti (+2 İtibar).";
                }
                else
                {
                    company.adjustHype(-6);
                    UniverseSaveManager.AdjustReputation(-4);
                    PushPost(new TivitPost { W = Math.Round(company.currentWeek, 1), H = "@PuanAvcısı", D = "Puan Avcısı ✓", V = true, Tag = "INFLUENCER", Hash = "#SarıÖzür", L = 4100, RT = 1650, X = "Yine aynı sarı arka planlı 'Sizi duyuyoruz' şablonunu kopyala-yapıştır yapmışlar, kimse yutmadı 😂" }, true);
                    UniverseSaveManager.Save();
                    return "💀 Özür metni ters tepti! 'Aynı şablonu kopyalamışlar' diye dalga geçiyorlar (-6 Hype, -4 İtibar).";
                }
            }
            else
            {
                s.BoycottActive = false;
                company.adjustHype(-8);
                SocialSaveManager.MarkDirty(true);
                return "Sessizliğe gömüldün. Kriz zamanla unutulacak ama sıradaki oyuna -8 Hype taşındı.";
            }
        }

        // --- E) 12 FARKLI DM SENARYOSU YANITLAYICI ---
        public static string AnswerDm(Company company, TivitDm dm, int opt)
        {
            if (company == null || dm == null || dm.Done) return "Bu DM zaten yanıtlandı.";
            dm.Done = true;
            var rival = UniverseSaveManager.FindRivalById(dm.RivalId);
            double w = Math.Round(company.currentWeek, 1);

            if (dm.Kind == "CEO_BEEF")
            {
                if (opt == 0)
                {
                    company.adjustHype(14);
                    SocialSaveManager.Current.FakeBeefActive = true;
                    SocialSaveManager.Current.FakeBeefRivalId = dm.RivalId;
                    UniverseSaveManager.AdjustRivalRelation(rival, +12);
                    dm.Note = "🎭 Anlaştınız! Sahte kavga kopardınız (+14 Hype, gizlice +12 İlişki)!";
                    PushPost(new TivitPost { W = w, H = dm.H, D = dm.D, V = true, Tag = "RIVAL", Sid = dm.RivalId, CanRatio = true, Hash = "#BüyükKavga", L = 3200, RT = 1100, X = $"Hey {GetPlayerStudioHandle(company)}, sıradaki oyunumuz sizinkini haritadan silecek! (Gizli anlaşmalı şov 😉)" }, true);
                }
                else
                {
                    UniverseSaveManager.AdjustReputation(+3);
                    dm.Note = "Reddettin: 'Biz oyunun kalitesiyle konuşuruz' (+3 İtibar).";
                }
            }
            else if (dm.Kind == "MOLE_INTEL")
            {
                if (opt == 0)
                {
                    if (company.cash < 15000.0) { dm.Done = false; return "Köstebek 15K istiyor!"; }
                    company.adjustCash(-15000.0, "Tivitır DM: Köstebek İstihbaratı");
                    company.adjustHype(12);
                    UniverseSaveManager.AdjustLoreBarrier(-10);
                    dm.Note = "🕵️ İstihbaratı aldın (-15K): +12 Hype & Bariyer -10!";
                }
                else
                {
                    UniverseSaveManager.AdjustReputation(+5);
                    UniverseSaveManager.AdjustRivalRelation(rival, +15);
                    dm.Note = "🛡 Köstebeği ifşa ettin! Dürüstlük alkışlandı (+5 İtibar, +15 İlişki).";
                }
            }
            else if (dm.Kind == "FENOCTAKOS_COLLAB")
            {
                if (opt == 0)
                {
                    if (company.cash < 12000.0) { dm.Done = false; return "@Fenoctakos ortak yayın bütçesi 12K!"; }
                    company.adjustCash(-12000.0, "@Fenoctakos Ortak Lore Yayını");
                    long fGain = Math.Max(70, (long)Math.Round(company.fans * 0.08));
                    company.adjustFans(fGain);
                    company.adjustHype(10);
                    dm.Note = $"🔥 @Fenoctakos senin evrenin için de teori videosu çekti! (+{fGain} Hayran, +10 Hype)";
                    PushPost(new TivitPost { W = w, H = "@Fenoctakos", D = "Fenoctakos ✓", V = true, Tag = "INFLUENCER", Hash = GetPlayerPrimaryTag(), L = 3600, RT = 950, X = $"#EntaTaym bugün misafirliğe gidiyoruz! {UniverseSaveManager.Current.UniverseName} lore'unu masaya yatırdım, acayip detaylar çıktı! {GetPlayerPrimaryTag()}" }, true);
                }
                else
                {
                    UniverseSaveManager.AdjustIntegrity(+3);
                    dm.Note = "Nazikçe reddettin, gizemini korudun (+3 Tutarlılık).";
                }
            }
            else if (dm.Kind == "YAYINKAOS_CHALLENGE")
            {
                if (opt == 0)
                {
                    company.adjustHype(10);
                    UniverseSaveManager.AdjustReputation(+2);
                    dm.Note = "🔥 Meydan okumayı kabul ettin! (+10 Hype, +2 İtibar)";
                    PushPost(new TivitPost { W = w, H = "@YayınKaos", D = "Yayın Kaos ✓", V = true, Tag = "INFLUENCER", Hash = GetPlayerPrimaryTag(), L = 3100, RT = 810, X = $"HELAL OLSUN {GetPlayerStudioHandle(company)}! Zor mod meydan okumamı kabul ettiler, chat hazır olun!! 🔥" }, true);
                }
                else
                {
                    UniverseSaveManager.AdjustIntegrity(+3);
                    dm.Note = "Dengeli tasarım kararını savundun (+3 Tutarlılık).";
                }
            }
            else if (dm.Kind == "MEME_COLLAB")
            {
                if (opt == 0)
                {
                    company.adjustHype(12);
                    long fg = Math.Max(45, (long)Math.Round(company.fans * 0.05));
                    company.adjustFans(fg);
                    UniverseSaveManager.AdjustReputation(-1);
                    dm.Note = $"😂 @OyunMizah seni fena trolledi ama viral oldun! (+12 Hype, +{fg} Hayran)";
                    PushPost(new TivitPost { W = w, H = "@OyunMizah", D = "Oyun Mizah ✓", V = true, Tag = "INFLUENCER", Hash = "#OyunMizah", L = 4400, RT = 1600, X = $"İzin aldık gömüyoruz 😂 {GetPlayerStudioHandle(company)} kendi oyunlarına bile gülüyor, işte özgüven! {GetPlayerPrimaryTag()}" }, true);
                }
                else
                {
                    UniverseSaveManager.AdjustReputation(+2);
                    dm.Note = "Ciddiyetini korudun (+2 İtibar).";
                }
            }
            else if (dm.Kind == "COSPLAY_SPONSOR")
            {
                if (opt == 0)
                {
                    if (company.cash < 8000.0) { dm.Done = false; return "Cosplay bütçesi 8K!"; }
                    company.adjustCash(-8000.0, "Cosplay Sponsorluğu");
                    company.adjustHype(8);
                    long fg = Math.Max(60, (long)Math.Round(company.fans * 0.07));
                    company.adjustFans(fg);
                    dm.Note = $"🎭 Cosplay fuarda olay oldu! (-8K, +8 Hype, +{fg} Hayran)";
                }
                else
                {
                    company.adjustHype(3);
                    dm.Note = "Sadece RT ile destek oldun (+3 Hype).";
                }
            }
            else if (dm.Kind == "OST_REMIX")
            {
                if (opt == 0)
                {
                    company.adjustHype(9);
                    long fg = Math.Max(50, (long)Math.Round(company.fans * 0.06));
                    company.adjustFans(fg);
                    UniverseSaveManager.AdjustReputation(+3);
                    dm.Note = $"🎵 Rap/OST remix'i resmi kanalda patladı! (+9 Hype, +{fg} Hayran, +3 İtibar)";
                    PushPost(new TivitPost { W = w, H = "@MüzikAvcısı", D = "Müzik Avcısı ✓", V = true, Tag = "INFLUENCER", Hash = "#OyunMüziği", L = 2950, RT = 910, X = $"🔥 {GetPlayerStudioHandle(company)} hayran rap remix'ini resmi kanalında paylaştı! Çalma listemden çıkmıyor!" }, true);
                }
                else
                {
                    company.adjustCash(4000.0, "Müzik Telif Payı");
                    UniverseSaveManager.AdjustReputation(-2);
                    dm.Note = "Telif geliri aldın (+4K, -2 İtibar).";
                }
            }
            else if (dm.Kind == "TIERLIST_BRIBE")
            {
                if (opt == 0)
                {
                    if (company.cash < 6000.0) { dm.Done = false; return "VIP koleksiyon kutusu 6K!"; }
                    company.adjustCash(-6000.0, "@TierListçi VIP Kutu");
                    company.adjustHype(9);
                    dm.Note = "📊 Yıldızın 'tesadüfen' S-Tier'ın tepesine yerleşti! (-6K, +9 Hype)";
                    PushPost(new TivitPost { W = w, H = "@TierListçi", D = "Tier Listçi ✓", V = true, Tag = "INFLUENCER", Hash = "#TierList", L = 3200, RT = 980, X = $"📊 GÜNCELLEME: {UniverseSaveManager.Current.UniverseName} tamamen objektif(!) incelemeyle S+ Tier'a yükseldi! {GetPlayerPrimaryTag()}" }, true);
                }
                else
                {
                    UniverseSaveManager.AdjustReputation(+5);
                    dm.Note = "🛡 Rüşvet teklifini ifşa ettin! (+5 İtibar)";
                }
            }
            else if (dm.Kind == "SPEEDRUN_EVENT")
            {
                if (opt == 0)
                {
                    if (company.cash < 10000.0) { dm.Done = false; return "Yardım maratonu sponsorluğu 10K!"; }
                    company.adjustCash(-10000.0, "Speedrun Yardım Maratonu");
                    UniverseSaveManager.AdjustReputation(+6);
                    long fg = Math.Max(70, (long)Math.Round(company.fans * 0.08));
                    company.adjustFans(fg);
                    dm.Note = $"⏱️ Speedrun maratonu rekor kırdı! (-10K, +6 İtibar, +{fg} Hayran)";
                }
                else
                {
                    company.adjustHype(4);
                    dm.Note = "Ücretsiz duyuru desteği verdin (+4 Hype).";
                }
            }
            else if (dm.Kind == "RIVAL_TRUCE")
            {
                if (opt == 0)
                {
                    UniverseSaveManager.AdjustRivalRelation(rival, +20);
                    UniverseSaveManager.AdjustReputation(+3);
                    dm.Note = $"🤝 {(rival != null ? rival.Name : "Rakip")} ile ateşkes imzalandı (+20 İlişki, +3 İtibar)!";
                }
                else
                {
                    UniverseSaveManager.AdjustRivalRelation(rival, -16);
                    company.adjustHype(11);
                    dm.Note = "😈 Mesajı ifşa edip ateşi harladın (+11 Hype, -16 İlişki)!";
                }
            }
            else // SUPERFAN
            {
                if (opt == 0)
                {
                    UniverseSaveManager.AdjustReputation(+3);
                    company.adjustHype(4);
                    dm.Note = "❤️ Süper hayrana söz verdin (+3 İtibar, +4 Hype)!";
                }
                else
                {
                    company.adjustHype(6);
                    dm.Note = "😈 Gizemli emojiler attın — hayran heyecandan uyuyamadı (+6 Hype)!";
                }
            }

            SocialSaveManager.MarkDirty(true);
            UniverseSaveManager.Save();
            return dm.Note;
        }

        // --- F) RAKIP & OYUNCU CIKIS KANCALARI (SKOR SENKRONU + COK CESITLI SABLONLAR!) ---
        public static void OnRivalGameReleased(Company company, RivalStudioRecord r, string title, double score)
        {
            if (r == null) return;
            double w = company != null ? Math.Round(company.currentWeek, 1) : 0.0;
            string fanTag = !string.IsNullOrEmpty(r.FandomName) ? CleanTag(r.FandomName) : CleanTag(r.UniverseName);
            int likes = Math.Max(120, (int)Math.Min(18000, r.Fans * (score >= 8.4 ? 1.8 : 0.9)));

            string tplStr = score >= 8.4
                ? PickNoRepeat(company, "rv_hit", RivalHitTemplates)
                : (score < 5.8 ? PickNoRepeat(company, "rv_flop", RivalFlopTemplates) : PickNoRepeat(company, "rv_mid", RivalMidTemplates));

            string body = tplStr
                .Replace("{title}", title)
                .Replace("{score}", $"{score:F1}")
                .Replace("{uni}", r.UniverseName)
                .Replace("{uniPhrase}", FormatUniPhrase(r.UniverseName))
                .Replace("{studio}", r.Name)
                .Replace("{tag}", fanTag);

            var replies = new List<TivitReply>();
            if (r.Id == "rival_entakos")
            {
                string fRep = score >= 8.0
                    ? PickNoRepeat(company, "fen_hit", new[]
                    {
                        $"BEN DEMİŞTİM!! '{title}' ({score:F1}) ile EntaVerse zirveye oturdu! #EntaTaym kırın şu klavyeleri! 🔥",
                        $"'{title}' ({score:F1}) tam bir Kuriow şovu! Akşam 3 saatlik derin analiz yayını açıyorum! #EntaTaym"
                    })
                    : $"'{title}' ({score:F1}) puanına takılmayın, Fenta ve LarpTakos sahnelerindeki gizli ipuçları yeter! #EntaTaym";
                replies.Add(new TivitReply { H = "@Fenoctakos", D = "Fenoctakos ✓", X = fRep, L = likes / 2 + 240 });
            }
            else if (r.Id == "rival_njg")
            {
                string tRep = PickNoRepeat(company, "tenti_rel", new[]
                {
                    $"'{title}' ({score:F1}/10) çıktı! TentiTaym olarak bütün kütüphane notlarını sabaha kadar çeviriyoruz! #TentiTaym",
                    $"'{title}' ({score:F1}) içinde Feil ve Kyldo Zarın bağlantısı yakaladım, TentiTaym uyumasın! #TentiTaym"
                });
                replies.Add(new TivitReply { H = "@TentiArşiv", D = "TentiTaym Arşivi ✓", X = tRep, L = likes / 2 + 180 });
            }
            else if (score < 5.8)
            {
                string flRep = PickNoRepeat(company, "puan_flop", new[]
                {
                    $"'{title}' ({score:F1}) nedir ya? Buna harcanan elektrik faturasına yazık.",
                    $"'{title}' ({score:F1}) oynarken 4 kere haritanın altına düştüm, elinize sağlık(!) 😂",
                    $"Bu puan ({score:F1}) bile fazla, yama gelene kadar kimse yaklaşmasın."
                });
                replies.Add(new TivitReply { H = "@PuanAvcısı", D = "Puan Avcısı ✓", X = flRep, L = likes + 320 });
            }
            else if (score >= 8.7)
            {
                string hiRep = PickNoRepeat(company, "inf_hit", new[]
                {
                    $"'{title}' ({score:F1}) yılın sürprizi oldu, soundtrack'i bile ayrı başyapıt! 🎧",
                    $"S-Tier listemi acil güncelliyorum, '{title}' ({score:F1}) ortalığı süpürdü! 📊",
                    $"Bu akşam yayında '{title}' kırıyoruz chat, uyku yok! 🔥"
                });
                string hWho = NextSeed(company, 3) == 0 ? "@MüzikAvcısı" : (NextSeed(company, 2) == 0 ? "@TierListçi" : "@YayınKaos");
                string dWho = hWho == "@MüzikAvcısı" ? "Müzik Avcısı ✓" : (hWho == "@TierListçi" ? "Tier Listçi ✓" : "Yayın Kaos ✓");
                replies.Add(new TivitReply { H = hWho, D = dWho, X = hiRep, L = likes / 2 + 160 });
            }

            PushPost(new TivitPost
            {
                W = w, H = CleanHandle(r.Name), D = r.Name, V = true, Tag = "RIVAL",
                Sid = r.Id, CanRatio = true, Hash = fanTag, L = likes, RT = likes / 4,
                X = body, Replies = replies
            }, false);
        }

        // 1. ASAMA (Skor tavani oncesi): TT #1 & Soz Arsivi skor/hype bonuslari
        public static void ApplyPreScoreSocialEffects(Company company, string title, double approxScore, ProjectCategory category, bool wasCrossover, ref double scoreMult, ref int modHype, ref int hypePenalty, List<string> notes)
        {
            if (company == null) return;
            var s = SocialSaveManager.Current;
            string pTag = GetPlayerPrimaryTag();

            if (IsPlayerTrendingNo1(company))
            {
                modHype += 5;
                notes.Add($"🔥 Tivitır TT #1 ({pTag}): +5 Hype!");
            }

            for (int i = 0; i < s.Promises.Count; i++)
            {
                var pr = s.Promises[i];
                if (pr.Resolved) continue;
                bool kept = (pr.Kind == "SIDE_UNI" && category == ProjectCategory.SideUniverse) ||
                            (pr.Kind == "ALT_TIME" && category == ProjectCategory.AltTimeline) ||
                            (pr.Kind == "CROSSOVER" && wasCrossover);
                if (kept)
                {
                    pr.Resolved = true;
                    pr.Kept = true;
                    scoreMult *= 1.03;
                    modHype += 10;
                    UniverseSaveManager.AdjustReputation(+5);
                    notes.Add($"📌 Tivitır Sözü Tutuldu ({pr.Target}): +%3 Skor, +10 Hype, +5 İtibar!");
                }
            }
        }

        // 2. ASAMA (Kesin final skor belliyken!): Gercek skorla Tivitir paylasimi, @YayinKaos & HIGH_SCORE soz kontrolu!
        public static void PostPlayerGameReleaseTweet(Company company, string title, double finalScore, ProjectCategory category, bool wasCrossover, List<string> notes)
        {
            if (company == null) return;
            var s = SocialSaveManager.Current;
            double w = Math.Round(company.currentWeek, 1);
            string pTag = GetPlayerPrimaryTag();

            if (s.SponsorKaosActive)
            {
                s.SponsorKaosActive = false;
                if (finalScore >= 7.5)
                {
                    long wave = Math.Clamp((long)Math.Round(company.fans * 0.14), 85L, 2500L);
                    company.adjustFans(wave);
                    company.adjustHype(10);
                    notes.Add($"🎮 @YayınKaos Yayını Patladı! (+{wave} Hayran, +10 Hype)");
                    PushPost(new TivitPost { W = w, H = "@YayınKaos", D = "Yayın Kaos ✓", V = true, Tag = "INFLUENCER", Hash = pTag, L = 7400, RT = 2100, X = $"OĞLUM '{title}' ({finalScore:F1}/10) NE OLMUŞ BÖYLE!! 8 SAATTİR YAYINDAYIM KALKAMIYORUM!! {pTag}" }, true);
                }
                else if (finalScore < 6.0)
                {
                    company.adjustHype(-7);
                    UniverseSaveManager.AdjustReputation(-4);
                    notes.Add("🎮 @YayınKaos Bug Şovu! (-7 Hype, -4 İtibar)");
                    PushPost(new TivitPost { W = w, H = "@YayınKaos", D = "Yayın Kaos ✓", V = true, Tag = "INFLUENCER", Hash = "#BugŞovu", L = 8900, RT = 3400, X = $"'{title}' ({finalScore:F1}) oynarken karakter uzaya fırladı chat ağlıyor 😂 Sponsor olsalar da gerçekleri söylerim!" }, true);
                }
                else
                {
                    long wave = Math.Clamp((long)Math.Round(company.fans * 0.06), 35L, 800L);
                    company.adjustFans(wave);
                    notes.Add($"🎮 @YayınKaos Yayını (+{wave} Hayran)");
                }
            }

            // HIGH_SCORE & tutmayan sozleri kesin final skorla kontrol et!
            for (int i = 0; i < s.Promises.Count; i++)
            {
                var pr = s.Promises[i];
                if (pr.Resolved) continue;
                bool kept = (pr.Kind == "HIGH_SCORE" && finalScore >= 8.5);
                pr.Resolved = true;
                pr.Kept = kept;

                if (kept)
                {
                    company.adjustHype(10);
                    UniverseSaveManager.AdjustReputation(+5);
                    notes.Add($"📌 Tivitır Sözü Tutuldu ({pr.Target}): +10 Hype, +5 İtibar!");
                    PushPost(new TivitPost { W = w, H = "@LoreKazıcısı", D = "Lore Kazıcısı ✓", V = true, Tag = "INFLUENCER", Hash = pTag, L = 4200, RT = 1100, X = $"👏 SÖZÜNÜ TUTAN STÜDYO! \"{pr.Text}\" demişlerdi ve '{title}' ({finalScore:F2}) ile noktayı koydular! {pTag}" }, true);
                }
                else if (pr.Kind == "HIGH_SCORE" && finalScore < 6.5)
                {
                    UniverseSaveManager.AdjustReputation(-5);
                    company.adjustHype(-5);
                    notes.Add("📌 'Bu Tivit Süt Gibi Kesildi!' (-5 İtibar, -5 Hype)");
                    PushPost(new TivitPost { W = w, H = "@PuanAvcısı", D = "Puan Avcısı ✓", V = true, Tag = "INFLUENCER", Hash = "#Yaşlanmadı", L = 6800, RT = 2600, X = $"🥛 BU TİVİT SÜT GİBİ KESİLDİ! \"{pr.Text}\" deyip '{title}' ile {finalScore:F2}/10 almak nasıl bir özgüven?!" }, true);
                }
            }

            if (finalScore < 5.2 && !s.BoycottActive)
                TriggerBoycott(company, $"'{title}' ({finalScore:F1}/10) fiyaskosu sonrası oyuncular isyanda!");

            string uniName = UniverseSaveManager.Current.UniverseName;
            string pBody;
            if (finalScore >= 8.5)
            {
                pBody = PickNoRepeat(company, "pl_hit", new[]
                {
                    $"🏆 '{title}' çıktı ve tam {finalScore:F2}/10 puan aldı! {FormatUniPhrase(uniName)} tarihinde yeni bir zirve! {pTag}",
                    $"🔥 Tarih yazdık! '{title}' eleştirmenlerden {finalScore:F2}/10 topladı! {pTag} ailesine sonsuz teşekkürler!",
                    $"👑 '{title}' ({finalScore:F2}/10) ile yılın en büyük gecesi! {uniName} daha yeni başlıyor! {pTag}"
                });
            }
            else if (finalScore >= 6.5)
            {
                pBody = PickNoRepeat(company, "pl_mid", new[]
                {
                    $"🎮 Yeni oyunumuz '{title}' ({finalScore:F2}/10) artık herkesle! Tüm destekçilere teşekkürler! {pTag}",
                    $"✨ '{title}' ({finalScore:F2}/10) yayında! {uniName} macerasına katılan herkese iyi oyunlar! {pTag}",
                    $"📢 Aylarca üzerinde çalıştığımız '{title}' ({finalScore:F2}/10) raflarda! En sevdiğiniz sahneyi yazın! {pTag}"
                });
            }
            else
            {
                pBody = PickNoRepeat(company, "pl_low", new[]
                {
                    $"🛠️ '{title}' ({finalScore:F2}/10) beklediğimiz gibi başlamadı. Tüm yorumları okuyor, hızla toparlıyoruz. {pTag}",
                    $"📉 '{title}' ({finalScore:F2}/10) çıkışından önemli dersler çıkardık — sıradaki oyunda telafi sözümüz olsun! {pTag}"
                });
            }

            int pLikes = Math.Max(150, (int)Math.Min(25000, company.fans * (finalScore >= 8.0 ? 2.2 : 1.1)));
            var pReplies = new List<TivitReply>
            {
                new TivitReply
                {
                    H = finalScore >= 8.0 ? "@LoreKazıcısı" : "@PuanAvcısı",
                    D = finalScore >= 8.0 ? "Lore Kazıcısı ✓" : "Puan Avcısı ✓",
                    X = finalScore >= 8.0
                        ? PickNoRepeat(company, "pl_rep_hi", new[] { "Bu oyunun gizli detaylarını haftaya 4 saatlik yayında inceleyeceğim, şahane! 🔥", "Son sahnede tüylerim diken diken oldu, yılın adaylarından biri net!" })
                        : PickNoRepeat(company, "pl_rep_lo", new[] { $"Skor {finalScore:F2}... Bir sonraki oyunda çok daha sert bir geri dönüş lazım.", $"Potansiyel vardı ama {finalScore:F2}'de tıkanmış, biraz daha cila şarttı." }),
                    L = pLikes / 2
                }
            };
            if (UniverseSaveManager.Current.ActiveUniverseId == "uni_rival_entakos")
            {
                pReplies.Add(new TivitReply { H = "@Fenoctakos", D = "Fenoctakos ✓", X = $"Yeni EntaVerse oyunu '{title}' ({finalScore:F2}) çıktı! #EntaTaym sabaha kadar teori zamanı! 🔥", L = pLikes / 3 + 220 });
            }

            PushPost(new TivitPost
            {
                W = w, H = GetPlayerStudioHandle(company), D = company.name ?? "Senin Stüdyon", V = true, Tag = "PLAYER",
                Hash = pTag, L = pLikes, RT = pLikes / 3, X = pBody, Replies = pReplies
            }, true);
        }

        // --- G) ZENGIN 10-KATEGORILI CANLI AKIS + @FENOCTAKOS + 12 DM + BURNER SOGUMA ---
        public static void Tick(Company company)
        {
            if (company == null) return;
            var s = SocialSaveManager.Current;
            double week = company.currentWeek;

            if (s.BoycottActive && week >= s.BoycottEndW)
            {
                s.BoycottActive = false;
                SocialSaveManager.MarkDirty(false);
            }

            // 1) 4'lu Turnuva sonuclandir!
            if (s.Tourney != null && s.Tourney.Active && week >= s.Tourney.EndW)
            {
                s.Tourney.Active = false;
                int winIdx = 0;
                for (int i = 0; i < 4; i++)
                {
                    double rr = 0.5;
                    try { rr = company.getRandom(); } catch { }
                    s.Tourney.Votes[i] += (int)Math.Round(rr * 900.0);
                    if (s.Tourney.Votes[i] > s.Tourney.Votes[winIdx]) winIdx = i;
                }
                string winner = s.Tourney.Names[winIdx];
                string wStudio = s.Tourney.Studios[winIdx];
                s.Tourney.Result = $"🏆 Şampiyon: {winner} ({wStudio}) — {s.Tourney.Votes[winIdx]:N0} Oy!";

                if (winIdx == 0)
                {
                    company.adjustHype(10);
                    long fg = Math.Max(55, (long)Math.Round(company.fans * 0.06));
                    company.adjustFans(fg);
                    var sp = UniverseSaveManager.GetSpecialCharacters();
                    for (int k = 0; k < sp.Count; k++)
                        if (sp[k].Name == winner) { sp[k].Popularity = PopularitySystem.ApplyDelta(sp[k].Popularity, 7); break; }
                    NotificationHelper.Show(company, $"🏆 TİVİTIR TURNUVA ŞAMPİYONU: {winner}!",
                        $"4'lü 'Kim Alır?' anketini {winner} kazandı! (+10 Hype, +{fg} Hayran, +7 Pop | Rakipler 'Bot bastınız!' diye ağlıyor)");
                }
                else
                {
                    NotificationHelper.Show(company, $"🏆 Tivitır Turnuvası Bitti: {winner}",
                        $"{winner} ({wStudio}) turnuvayı kazandı! Bir dahaki sefere rövanş zamanı.");
                }

                PushPost(new TivitPost
                {
                    W = Math.Round(week, 1), H = "@TivitırAnket", D = "Tivitır Kim Alır? ✓", V = true, Tag = "TOURNEY",
                    Hash = "#KimAlır", L = 5100, RT = 1800,
                    X = $"🏆 ANKET KAPANDI! Şampiyon: {winner} ({wStudio}) — {s.Tourney.Votes[winIdx]:N0} oy! Elenen fanlar 'Hile var, bot bastınız!' diye ortalığı yıkıyor!"
                }, true);
            }

            // 2) @FENOCTAKOS (ENTATAYM FENOMENI — 22 BENZERSIZ TEORI + ENTAKOS'A/OYUNCUYA HAYRAN CEKME + BURNER SOGUMA!)
            if (week - s.LastFenoctakosW >= 6.5)
            {
                s.LastFenoctakosW = week;
                if (s.BurnerHeat > 0) s.BurnerHeat = Math.Max(0, s.BurnerHeat - 8);

                var enta = UniverseSaveManager.FindRivalById("rival_entakos");
                if (enta != null && !enta.IsAbsorbedByRival)
                {
                    double r = 0.5;
                    try { r = company.getRandom(); } catch { }
                    long pulledFans = 25 + (long)Math.Round(r * 45.0);
                    if (enta.IsAcquiredByPlayer) company.adjustFans(pulledFans);
                    else enta.Fans += pulledFans;

                    string theoryText;
                    if (enta.IsAcquiredByPlayer && r < 0.35)
                    {
                        theoryText = PickNoRepeat(company, "fen_owned", FenoctakosPlayerOwnedTakes)
                            .Replace("{playerHandle}", GetPlayerStudioHandle(company))
                            .Replace("{playerStudio}", company.name ?? "Yeni Stüdyo");
                    }
                    else
                    {
                        theoryText = PickNoRepeat(company, "fen_th", FenoctakosTheories);
                    }

                    PushPost(new TivitPost
                    {
                        W = Math.Round(week, 1), H = "@Fenoctakos", D = "Fenoctakos (EntaTaym)", V = true, Tag = "INFLUENCER",
                        Sid = "rival_entakos", CanRatio = !enta.IsAcquiredByPlayer, Hash = "#EntaTaym",
                        L = 1300 + (int)Math.Min(12000, enta.Fans * 0.45) + NextSeed(company, 400),
                        RT = 340 + (int)Math.Min(4000, enta.Fans * 0.12),
                        X = theoryText + $" (+{pulledFans} yeni #EntaTaym!)"
                    }, false);
                }
            }

            // 3) 10 FARKLI KATEGORIDE YASAYAN AKIS (her ~3.0 haftada bir!)
            if (week - s.LastAmbientW >= 2.4)
            {
                s.LastAmbientW = week;
                SpawnDiverseAmbientTweet(company, week);
            }

            // 4) 12 FARKLI DM SENARYOSU (her ~8.5 haftada bir, anti-tekrarli!)
            if (week >= 5.0 && (week - s.LastDmW) >= 8.5 && GetUnreadDmCount() < 6)
            {
                s.LastDmW = week;
                SpawnDiverseDm(company, week);
            }
        }

        private static string PickStarNameForRival(Company comp, RivalStudioRecord rv)
        {
            if (rv == null) return "Kahraman";
            RivalStarSeed[] seeds;
            if (MultiverseEmpireSystem.RivalRosterSeeds.TryGetValue(rv.Id, out seeds) && seeds != null && seeds.Length > 0)
                return seeds[NextSeed(comp, seeds.Length)].Name;
            return rv.UniverseName;
        }

        private static void SpawnDiverseAmbientTweet(Company company, double week)
        {
            var u = UniverseSaveManager.Current;
            var specials = UniverseSaveManager.GetSpecialCharacters();
            var activeRivals = new List<RivalStudioRecord>();
            if (u.Rivals != null)
                for (int i = 0; i < u.Rivals.Count; i++)
                    if (!u.Rivals[i].IsAbsorbedByRival) activeRivals.Add(u.Rivals[i]);

            int cat = NextSeed(company, 16);
            double w = Math.Round(week, 1);

            if (cat == 0 && specials.Count > 0)
            {
                // 1. Hayran Isi (Fan-Art / Cosplay / Mod / Rap Klibi / 14 Tur!)
                var ch = specials[NextSeed(company, specials.Count)];
                string kind = FanWorkKinds[NextSeed(company, FanWorkKinds.Length)];
                string body = PickNoRepeat(company, "fanwork", FanWorkTemplates)
                    .Replace("{charName}", ch.Name).Replace("{kind}", kind)
                    .Replace("{playerHandle}", GetPlayerStudioHandle(company))
                    .Replace("{playerUni}", u.UniverseName).Replace("{pTag}", GetPlayerPrimaryTag());
                PushPost(new TivitPost
                {
                    W = w, H = FanHandles[NextSeed(company, FanHandles.Length)], D = "Hayran Sanatçısı",
                    Tag = "FANWORK", CharN = ch.Name, Hash = GetPlayerPrimaryTag(),
                    L = 680 + ch.Popularity * 14 + NextSeed(company, 400), RT = 180 + NextSeed(company, 150), X = body
                }, false);
            }
            else if (cat == 1 && activeRivals.Count >= 2)
            {
                // 2. Powerscaling "Kim Alir?" Kavgasi (12 sablon x 8 farkli yanit!)
                var rA = activeRivals[NextSeed(company, activeRivals.Count)];
                var rB = activeRivals[(activeRivals.IndexOf(rA) + 1 + NextSeed(company, activeRivals.Count - 1)) % activeRivals.Count];
                bool usePlayerChar = specials.Count > 0 && NextSeed(company, 3) == 0;
                string sA = usePlayerChar ? specials[NextSeed(company, specials.Count)].Name : PickStarNameForRival(company, rA);
                string uA = usePlayerChar ? u.UniverseName : rA.UniverseName;
                string stA = usePlayerChar ? (company.name ?? "Sen") : rA.Name;
                string sB = PickStarNameForRival(company, rB);
                string tagA = usePlayerChar ? GetPlayerPrimaryTag() : (!string.IsNullOrEmpty(rA.FandomName) ? CleanTag(rA.FandomName) : CleanTag(rA.UniverseName));
                string tagB = !string.IsNullOrEmpty(rB.FandomName) ? CleanTag(rB.FandomName) : CleanTag(rB.UniverseName);

                string body = PickNoRepeat(company, "pscale", PowerscaleTemplates)
                    .Replace("{starA}", sA).Replace("{uniA}", uA).Replace("{studioA}", stA).Replace("{tagA}", tagA)
                    .Replace("{starB}", sB).Replace("{uniB}", rB.UniverseName).Replace("{studioB}", rB.Name).Replace("{tagB}", tagB);
                string rep = PickNoRepeat(company, "pscale_r", PowerscaleReplies)
                    .Replace("{starA}", sA).Replace("{starB}", sB);
                PushPost(new TivitPost
                {
                    W = w, H = FanHandles[NextSeed(company, FanHandles.Length)], D = "Powerscale Tartışması",
                    Tag = "FANDOM", Hash = "#KimAlır", L = 840 + NextSeed(company, 1400), RT = 240 + NextSeed(company, 200), X = body,
                    Replies = new List<TivitReply>
                    {
                        new TivitReply { H = FanHandles[NextSeed(company, FanHandles.Length)], D = "Tarafsız(?) İzleyici", X = rep, L = 320 + NextSeed(company, 350) }
                    }
                }, false);
            }
            else if (cat == 2 && activeRivals.Count >= 2)
            {
                // 3. @OyunMizah (30 Benzersiz Sektor Meme'i!)
                var rA = activeRivals[NextSeed(company, activeRivals.Count)];
                var rB = activeRivals[(activeRivals.IndexOf(rA) + 1) % activeRivals.Count];
                string star = PickStarNameForRival(company, rA);
                string game = rA.GamesReleased > 0 ? rA.LastGameTitle : Phase1Database.FindRivalTemplate(rA.Id).TitlePool[0];
                string body = PickNoRepeat(company, "meme", MemeTemplates)
                    .Replace("{studio}", rA.Name).Replace("{studioA}", rA.Name).Replace("{studioB}", rB.Name)
                    .Replace("{game}", game).Replace("{star}", star).Replace("{uni}", rA.UniverseName);
                PushPost(new TivitPost
                {
                    W = w, H = "@OyunMizah", D = "Oyun Mizah", V = true, Tag = "INFLUENCER",
                    Hash = "#OyunMizah", L = 1900 + NextSeed(company, 3200), RT = 680 + NextSeed(company, 900), X = body
                }, false);
            }
            else if (cat == 3 && activeRivals.Count >= 2)
            {
                // 4. @TierListci (10 Tartismali Liste x 6 Kizgin Yanit!)
                var rA = activeRivals[NextSeed(company, activeRivals.Count)];
                var rB = activeRivals[(activeRivals.IndexOf(rA) + 2) % activeRivals.Count];
                string sA = PickStarNameForRival(company, rA);
                string sB = PickStarNameForRival(company, rB);
                string tagA = !string.IsNullOrEmpty(rA.FandomName) ? CleanTag(rA.FandomName) : CleanTag(rA.UniverseName);
                string body = PickNoRepeat(company, "tier", TierListTemplates)
                    .Replace("{starA}", sA).Replace("{starB}", sB)
                    .Replace("{uniA}", rA.UniverseName).Replace("{uniB}", rB.UniverseName)
                    .Replace("{studioA}", rA.Name).Replace("{studioB}", rB.Name).Replace("{tagA}", tagA);
                string rep = PickNoRepeat(company, "tier_r", TierListReplies)
                    .Replace("{starA}", sA).Replace("{starB}", sB).Replace("{uniB}", rB.UniverseName).Replace("{studioA}", rA.Name);
                PushPost(new TivitPost
                {
                    W = w, H = "@TierListçi", D = "Tier Listçi", V = true, Tag = "INFLUENCER",
                    Hash = "#TierList", L = 1400 + NextSeed(company, 2100), RT = 420 + NextSeed(company, 300), X = body,
                    Replies = new List<TivitReply>
                    {
                        new TivitReply { H = FanHandles[NextSeed(company, FanHandles.Length)], D = "Sinirli Takipçi", X = rep, L = 410 + NextSeed(company, 380) }
                    }
                }, false);
            }
            else if (cat == 4 && activeRivals.Count > 0)
            {
                // 5. @SpeedrunDelisi (10 Glitch & Rekor!)
                bool aboutPlayer = u.ReleasedGames.Count > 0 && NextSeed(company, 2) == 0;
                string gName, stName, starN, hTag;
                if (aboutPlayer)
                {
                    var lg = u.ReleasedGames[NextSeed(company, u.ReleasedGames.Count)];
                    gName = lg.Title; stName = company.name ?? "Stüdyo";
                    starN = specials.Count > 0 ? specials[NextSeed(company, specials.Count)].Name : "Başrol";
                    hTag = GetPlayerPrimaryTag();
                }
                else
                {
                    var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                    gName = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                    stName = rv.Name; starN = PickStarNameForRival(company, rv); hTag = "#Speedrun";
                }
                string body = PickNoRepeat(company, "speed", SpeedrunTemplates)
                    .Replace("{game}", gName).Replace("{studio}", stName).Replace("{star}", starN);
                PushPost(new TivitPost
                {
                    W = w, H = "@SpeedrunDelisi", D = "Speedrun Delisi", V = true, Tag = "INFLUENCER",
                    Hash = hTag, L = 1250 + NextSeed(company, 1800), RT = 340 + NextSeed(company, 250), X = body
                }, false);
            }
            else if (cat == 5 && activeRivals.Count > 0)
            {
                // 6. @MuzikAvcisi (10 Soundtrack & Rap OST!)
                var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                string game = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                string star = PickStarNameForRival(company, rv);
                string tag = !string.IsNullOrEmpty(rv.FandomName) ? CleanTag(rv.FandomName) : CleanTag(rv.UniverseName);
                string body = PickNoRepeat(company, "music", MusicTemplates)
                    .Replace("{game}", game).Replace("{studio}", rv.Name).Replace("{uni}", rv.UniverseName)
                    .Replace("{star}", star).Replace("{tag}", tag);
                PushPost(new TivitPost
                {
                    W = w, H = "@MüzikAvcısı", D = "Müzik Avcısı", V = true, Tag = "INFLUENCER",
                    Hash = "#OyunMüziği", L = 1100 + NextSeed(company, 1500), RT = 290 + NextSeed(company, 200), X = body
                }, false);
            }
            else if (cat == 6 && activeRivals.Count >= 2)
            {
                // 7. @HypeMuhabiri (10 Kulis Dedikodusu!)
                var rA = activeRivals[NextSeed(company, activeRivals.Count)];
                var rB = activeRivals[(activeRivals.IndexOf(rA) + 3) % activeRivals.Count];
                string body = PickNoRepeat(company, "beef", RivalBeefTemplates)
                    .Replace("{studioA}", rA.Name).Replace("{studioB}", rB.Name)
                    .Replace("{uniB}", rB.UniverseName)
                    .Replace("{starA}", PickStarNameForRival(company, rA))
                    .Replace("{starB}", PickStarNameForRival(company, rB));
                PushPost(new TivitPost
                {
                    W = w, H = "@HypeMuhabiri", D = "Hype Muhabiri", V = true, Tag = "INFLUENCER",
                    Hash = "#SektörDedikodu", L = 1600 + NextSeed(company, 2200), RT = 520 + NextSeed(company, 300), X = body
                }, false);
            }
            else if (cat == 7 && u.ReleasedGames.Count > 0)
            {
                // 8. Oyuncunun Kendi Hayranlarinin Organik Sohbeti (18 Sablon!)
                var lg = u.ReleasedGames[NextSeed(company, u.ReleasedGames.Count)];
                string cName = specials.Count > 0 ? specials[NextSeed(company, specials.Count)].Name : "Başrol";
                string fLbl = !string.IsNullOrEmpty(u.UniverseFandomName) ? u.UniverseFandomName + " Üyesi" : "Sadık Oyuncu";
                string body = PickNoRepeat(company, "pl_org", PlayerOrganicFanTemplates)
                    .Replace("{playerStudio}", company.name ?? "Senin Stüdyon")
                    .Replace("{playerHandle}", GetPlayerStudioHandle(company))
                    .Replace("{playerUni}", u.UniverseName)
                    .Replace("{lastGame}", lg.Title)
                    .Replace("{charName}", cName)
                    .Replace("{pTag}", GetPlayerPrimaryTag());
                PushPost(new TivitPost
                {
                    W = w, H = FanHandles[NextSeed(company, FanHandles.Length)], D = fLbl,
                    Tag = "FANDOM", Hash = GetPlayerPrimaryTag(),
                    L = 420 + (int)Math.Min(6500, company.fans * 0.45) + NextSeed(company, 300), RT = 115 + NextSeed(company, 90), X = body
                }, false);
            }
            else if (cat == 8)
            {
                // 9. @TentiArsiv (30 Benzersiz Tentiforverse Lore Postu!)
                string body = PickNoRepeat(company, "tenti", TentiArsivPosts);
                PushPost(new TivitPost
                {
                    W = w, H = "@TentiArşiv", D = "TentiTaym Arşivi", V = true, Tag = "FANDOM",
                    Sid = "rival_njg", Hash = "#TentiTaym", L = 1280 + NextSeed(company, 1400), RT = 310 + NextSeed(company, 180), X = body
                }, false);
            }
            else if (cat == 9 && activeRivals.Count > 0)
            {
                // 10. @RetroNostalji (Eski Oyunlari Anma!)
                var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                string gTitle = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                string body = PickNoRepeat(company, "retro", RetroNostaljiTemplates)
                    .Replace("{game}", gTitle).Replace("{studio}", rv.Name)
                    .Replace("{star}", PickStarNameForRival(company, rv)).Replace("{uni}", rv.UniverseName);
                PushPost(new TivitPost
                {
                    W = w, H = "@RetroNostalji", D = "Retro Nostalji", V = true, Tag = "INFLUENCER",
                    Hash = "#RetroNostalji", L = 1350 + NextSeed(company, 1500), RT = 380, X = body
                }, false);
            }
            else if (cat == 10 && activeRivals.Count > 0)
            {
                // 11. @OyunEkonomi (Borsa, Indirim, Koleksiyon Kutusu!)
                var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                string gTitle = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                string body = PickNoRepeat(company, "econ", OyunEkonomiTemplates)
                    .Replace("{studio}", rv.Name).Replace("{game}", gTitle)
                    .Replace("{uni}", rv.UniverseName).Replace("{star}", PickStarNameForRival(company, rv));
                PushPost(new TivitPost
                {
                    W = w, H = "@OyunEkonomi", D = "Oyun Ekonomi", V = true, Tag = "INFLUENCER",
                    Hash = "#OyunEkonomi", L = 980 + NextSeed(company, 1200), RT = 260, X = body
                }, false);
            }
            else if (cat == 11 && activeRivals.Count >= 2)
            {
                // 12. @OyunGundemi (Topluluk Tartisma Sorulari!)
                var rA = activeRivals[NextSeed(company, activeRivals.Count)];
                var rB = activeRivals[(activeRivals.IndexOf(rA) + 2) % activeRivals.Count];
                string body = PickNoRepeat(company, "gundem", OyunGundemiTemplates)
                    .Replace("{uniA}", rA.UniverseName).Replace("{uniB}", rB.UniverseName).Replace("{studioA}", rA.Name);
                PushPost(new TivitPost
                {
                    W = w, H = "@OyunGündemi", D = "Oyun Gündemi", V = true, Tag = "INFLUENCER",
                    Hash = "#OyunGündemi", L = 1520 + NextSeed(company, 1800), RT = 410, X = body
                }, false);
            }
            else if (cat == 12 && activeRivals.Count > 0)
            {
                // 13. @ModAtolyesi (Topluluk Modlari!)
                var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                string gTitle = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                string body = PickNoRepeat(company, "modat", ModAtolyesiTemplates)
                    .Replace("{game}", gTitle).Replace("{studio}", rv.Name)
                    .Replace("{star}", PickStarNameForRival(company, rv)).Replace("{uni}", rv.UniverseName);
                PushPost(new TivitPost
                {
                    W = w, H = "@ModAtölyesi", D = "Mod Atölyesi", V = true, Tag = "INFLUENCER",
                    Hash = "#ModAtölyesi", L = 1220 + NextSeed(company, 1600), RT = 340, X = body
                }, false);
            }
            else if (cat == 13 && activeRivals.Count > 0)
            {
                // 14. @CizgiLore (Transmedya / Dizi / Roman Hayalleri!)
                var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                string gTitle = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                string body = PickNoRepeat(company, "cizgi", CizgiLoreTemplates)
                    .Replace("{game}", gTitle).Replace("{studio}", rv.Name)
                    .Replace("{star}", PickStarNameForRival(company, rv)).Replace("{uni}", rv.UniverseName);
                PushPost(new TivitPost
                {
                    W = w, H = "@ÇizgiLore", D = "Çizgi & Lore", V = true, Tag = "INFLUENCER",
                    Hash = "#ÇizgiLore", L = 1040 + NextSeed(company, 1300), RT = 270, X = body
                }, false);
            }
            else if (cat == 14 && activeRivals.Count > 0)
            {
                // 15. KOMBINATORYAL MIKRO-TIVIT MOTORU (6.400+ BENZERSIZ ORGANIK CUMLE!)
                bool usePlayer = u.ReleasedGames.Count > 0 && NextSeed(company, 3) == 0;
                string stName, uniName, gTitle, starName, hTag, dLabel;
                if (usePlayer)
                {
                    stName = company.name ?? "Senin Stüdyon";
                    uniName = u.UniverseName;
                    gTitle = u.ReleasedGames[NextSeed(company, u.ReleasedGames.Count)].Title;
                    starName = specials.Count > 0 ? specials[NextSeed(company, specials.Count)].Name : "Başrol";
                    hTag = GetPlayerPrimaryTag();
                    dLabel = !string.IsNullOrEmpty(u.UniverseFandomName) ? u.UniverseFandomName + " Üyesi" : "Oyun Tutkunu";
                }
                else
                {
                    var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                    stName = rv.Name; uniName = rv.UniverseName;
                    gTitle = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                    starName = PickStarNameForRival(company, rv);
                    hTag = !string.IsNullOrEmpty(rv.FandomName) ? CleanTag(rv.FandomName) : CleanTag(rv.UniverseName);
                    dLabel = !string.IsNullOrEmpty(rv.FandomName) ? rv.FandomName + " Üyesi" : "Oyun Tutkunu";
                }
                string open = ComboOpenings[NextSeed(company, ComboOpenings.Length)];
                string mid = ComboMiddles[NextSeed(company, ComboMiddles.Length)];
                string close = ComboClosings[NextSeed(company, ComboClosings.Length)];
                string body = $"{open} {mid} {close}"
                    .Replace("{studio}", stName).Replace("{uni}", uniName)
                    .Replace("{game}", gTitle).Replace("{star}", starName).Replace("{tag}", hTag);
                PushPost(new TivitPost
                {
                    W = w, H = FanHandles[NextSeed(company, FanHandles.Length)], D = dLabel,
                    Tag = "FANDOM", Hash = hTag, L = 360 + NextSeed(company, 1400), RT = 85 + NextSeed(company, 220), X = body
                }, false);
            }
            else if (activeRivals.Count > 0)
            {
                // 16. 24 Rakip Evrenin 30 Farkli Sablonlu Fandom Sohbeti!
                var rv = activeRivals[NextSeed(company, activeRivals.Count)];
                string starName = PickStarNameForRival(company, rv);
                string gTitle = rv.GamesReleased > 0 ? rv.LastGameTitle : Phase1Database.FindRivalTemplate(rv.Id).TitlePool[0];
                string hTag = !string.IsNullOrEmpty(rv.FandomName) ? CleanTag(rv.FandomName) : CleanTag(rv.UniverseName);
                string body = PickNoRepeat(company, "amb_fan", AmbientFandomTemplates)
                    .Replace("{studio}", rv.Name).Replace("{star}", starName)
                    .Replace("{uni}", rv.UniverseName).Replace("{game}", gTitle).Replace("{tag}", hTag);
                PushPost(new TivitPost
                {
                    W = w, H = FanHandles[NextSeed(company, FanHandles.Length)],
                    D = !string.IsNullOrEmpty(rv.FandomName) ? rv.FandomName + " Üyesi" : "Oyun Tutkunu",
                    Tag = "FANDOM", Sid = rv.Id, Hash = hTag,
                    L = 320 + (int)Math.Min(4500, rv.Fans * 0.5) + NextSeed(company, 350), RT = 95 + NextSeed(company, 120), X = body
                }, false);
            }
        }

        private static void SpawnDiverseDm(Company company, double week)
        {
            var indep = HoldingEndgameSystem.GetIndependentRivals();
            var specials = UniverseSaveManager.GetSpecialCharacters();
            string favChar = specials.Count > 0 ? specials[NextSeed(company, specials.Count)].Name : "favori karakterim";

            string[] allKinds =
            {
                "CEO_BEEF", "MOLE_INTEL", "FENOCTAKOS_COLLAB", "SUPERFAN",
                "YAYINKAOS_CHALLENGE", "MEME_COLLAB", "COSPLAY_SPONSOR",
                "OST_REMIX", "TIERLIST_BRIBE", "SPEEDRUN_EVENT", "RIVAL_TRUCE"
            };

            string chosenKind = allKinds[NextSeed(company, allKinds.Length)];
            for (int tries = 0; tries < allKinds.Length; tries++)
            {
                string cand = allKinds[(NextSeed(company, allKinds.Length) + tries) % allKinds.Length];
                if (_recentDmKinds.Contains(cand)) continue;
                if ((cand == "CEO_BEEF" || cand == "MOLE_INTEL" || cand == "RIVAL_TRUCE") && indep.Count == 0) continue;
                chosenKind = cand;
                break;
            }
            _recentDmKinds.Enqueue(chosenKind);
            while (_recentDmKinds.Count > 5) _recentDmKinds.Dequeue();

            double w = Math.Round(week, 1);
            var rv = indep.Count > 0 ? indep[NextSeed(company, indep.Count)] : null;

            if (chosenKind == "CEO_BEEF" && rv != null)
            {
                PushDm(new TivitDm
                {
                    W = w, H = CleanHandle(rv.Name), D = $"{rv.Name} CEO'su", Kind = "CEO_BEEF", RivalId = rv.Id,
                    Body = "Selam dostum! İkimizin de gündeme ihtiyacı var. Gel Tivitır'da sahte bir kavga çıkaralım, hayranlar çıldırsın (+14 Hype, gizlice +12 İlişki)?",
                    Opt0 = "Anlaştık (+14 Hype)", Opt1 = "Gerek Yok (+3 İtibar)"
                });
            }
            else if (chosenKind == "MOLE_INTEL" && rv != null)
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@AnonimKostebek", D = "Gizli Köstebek", Kind = "MOLE_INTEL", RivalId = rv.Id,
                    Body = $"15K ateşle, {rv.Name} stüdyosunun sıradaki gizli projesinin zayıf karnını vereyim (+12 Hype, Bariyer -10). Ya da beni ifşa edip +İtibar kas!",
                    Opt0 = "İstihbaratı Al (-15K)", Opt1 = "Köstebeği İfşa Et (+5 İtib.)"
                });
            }
            else if (chosenKind == "FENOCTAKOS_COLLAB")
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@Fenoctakos", D = "Fenoctakos ✓", Kind = "FENOCTAKOS_COLLAB", RivalId = "rival_entakos",
                    Body = $"Kral normalde hep #EntaTaym takılıyorum ama '{UniverseSaveManager.Current.UniverseName}' acayip ilgimi çekti! 12K sponsorlukla sizin evrene özel dev teori videosu çekeyim mi?",
                    Opt0 = "Video Çek (-12K)", Opt1 = "Şimdilik Kalsın (+3 Tut.)"
                });
            }
            else if (chosenKind == "YAYINKAOS_CHALLENGE")
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@YayınKaos", D = "Yayın Kaos ✓", Kind = "YAYINKAOS_CHALLENGE",
                    Body = "Kral selam! Sıradaki oyununa 'Tek Can / Kâbus Modu' eklemeye cesaretin varsa ücretsiz dev yayın açıp destekliyorum (+10 Hype)!",
                    Opt0 = "Meydan Okumayı Al (+10 Hype)", Opt1 = "Dengeyi Bozma (+3 Tut.)"
                });
            }
            else if (chosenKind == "MEME_COLLAB")
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@OyunMizah", D = "Oyun Mizah ✓", Kind = "MEME_COLLAB",
                    Body = "Stüdyonuzla tatlı sert dalga geçen dev bir meme serisi hazırladım! İzin ver paylaşıp sizi viral yapayım (+12 Hype & Hayran)?",
                    Opt0 = "Göm Gitsin! (+12 Hype)", Opt1 = "Ciddiyetimizi Koru (+2 İtib.)"
                });
            }
            else if (chosenKind == "COSPLAY_SPONSOR")
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@CosplayAtolye", D = "Ünlü Cosplayer", Kind = "COSPLAY_SPONSOR",
                    Body = $"Büyük fuar için {favChar} kostümü dikeceğim! 8K malzeme desteği verirseniz tüm basında stüdyonuzu etiketlerim (+8 Hype & Hayran)!",
                    Opt0 = "Destek Ol (-8K)", Opt1 = "Sadece RT Et (+3 Hype)"
                });
            }
            else if (chosenKind == "OST_REMIX")
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@MüzikAvcısı", D = "Müzik Avcısı ✓", Kind = "OST_REMIX",
                    Body = $"Oyununuzun müziğine bağımsız bir rapçi efsane remix yapmış! Resmi kanaldan paylaşıp patlatalım mı (+9 Hype), yoksa telif payı mı keselim (+4K)?",
                    Opt0 = "Resmi Paylaş (+9 Hype)", Opt1 = "Telif Kes (+4K)"
                });
            }
            else if (chosenKind == "TIERLIST_BRIBE")
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@TierListçi", D = "Tier Listçi ✓", Kind = "TIERLIST_BRIBE",
                    Body = "Aramızda kalsın... 6K'lık bir 'VIP İnceleme Kutusu' yollarsan yarınki listede sizin evreni direkt S+ Tier yaparım 😉 Ne dersin?",
                    Opt0 = "Kutuyu Yolla (-6K)", Opt1 = "Rüşveti İfşa Et (+5 İtib.)"
                });
            }
            else if (chosenKind == "SPEEDRUN_EVENT")
            {
                PushDm(new TivitDm
                {
                    W = w, H = "@SpeedrunDelisi", D = "Speedrun Delisi ✓", Kind = "SPEEDRUN_EVENT",
                    Body = "Sizin oyunlarınızla 24 saatlik yardım amaçlı Speedrun maratonu yapıyoruz! 10K ödül havuzu koyarsanız dev prestij (+6 İtibar & Hayran)!",
                    Opt0 = "Sponsor Ol (-10K)", Opt1 = "Ücretsiz Duyur (+4 Hype)"
                });
            }
            else if (chosenKind == "RIVAL_TRUCE" && rv != null)
            {
                PushDm(new TivitDm
                {
                    W = w, H = CleanHandle(rv.Name), D = $"{rv.Name} CEO'su", Kind = "RIVAL_TRUCE", RivalId = rv.Id,
                    Body = "İki topluluğun Tivitır'da birbirine girmesi ikimizi de yordu. Gel ortak dostane mesaj atıp buzları eritelim (+20 İlişki)?",
                    Opt0 = "Ateşkes İlan Et (+20 İlişki)", Opt1 = "DM'i İfşa Et (+11 Hype)"
                });
            }
            else
            {
                PushDm(new TivitDm
                {
                    W = w, H = FanHandles[NextSeed(company, FanHandles.Length)], D = "Sadık Hayran", Kind = "SUPERFAN",
                    Body = $"Ne olur sıradaki oyunda {favChar} karakterine kıyma, 4 yıldır duvar kağıdım o!! Bir söz ver rahat uyuyayım 😭",
                    Opt0 = "Güvendesin (+3 İtibar)", Opt1 = "Gizemli Emoji At (+6 Hype)"
                });
            }
        }

        private static bool ContainsAny(string text, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
                if (text.Contains(keys[i])) return true;
            return false;
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
        private static string _draftName = "";
        private static CharacterRole _draftRole = CharacterRole.Protagonist;
        private static CharacterTrait _draftTrait = CharacterTrait.None;
        private static string _draftTeamName = "";
        private static int _draftTeamCycleIdx = -1;
        private static float[] _draftAxes = new float[] { 0.72f, 0.65f, 0.72f, 0.18f, 0.22f, 0.55f, 0.35f, 0.85f };
        // Faz 8.0: Evren Mimari Atolyesi taslak durumu
        private static bool _universeSetupStartsCastAfter = false;
        private static string _uniDraftName = "";
        private static string _uniDraftFandom = "";
        private static float[] _uniDraftAxes = new float[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.62f, 0.35f };
        private static UniverseRule _uniDraftRule = UniverseRule.None;
        private static CosmologyLaw _uniDraftLaw = CosmologyLaw.FlexibleMultiverse;
        private static int _uniNamePoolIdx = 0;
        private static int _fandomPoolIdx = 0;
        private static int _rivalStarStudioFilterIdx = -1; // -1 = Tumu, 0..23 = belirli studyo
        private static UnityEngine.UI.Text[] _activeUniAxisTexts = new UnityEngine.UI.Text[UniverseAxisSystem.AxisCount];
        private static UnityEngine.UI.Text _activeUniToneText;
        private static int _namePoolIdx = 0;

        // Hizli NPC formu
        private static string _npcDraftName = "";
        private static int _npcTypeIdx = 0;
        private static string _npcLinkedCharId = "";
        private static string _npcLinkedCharName = "";
        private static int _npcNamePoolIdx = 0;

        // Eski karakter cagirma & Karakterler alt sekmesi
        private static CharacterRole _returningRole = CharacterRole.Protagonist;
        private static bool _returningShowNpcs = false;
        private static int _rosterSubTab = 0;
        private static int _companySubTab = 0;
        private static int _newsSubTab = 0;
        private static int _tivitSubTab = 0;
        private static int _tivitFeedFilter = 0;
        private static string _tivitDraftText = "";
        private static UnityEngine.UI.InputField _activeTivitInput;
        private static UnityEngine.UI.Text _fallbackTivitText;
        private static UnityEngine.UI.Text _activeTivitCountText;
        private static bool _workingIsSagaFinale = false;
        private static bool _filterByActiveUniverse = false;

        // Mod Icerigi sekme durumlari
        private static int _transmediaFocusCharIdx = -1;
        private static string _statusBannerMsg = "";

        // Canli uGUI referanslari (4 eksen)
        private static UnityEngine.UI.Text[] _activeAxisValueTexts = new UnityEngine.UI.Text[CharacterAxisSystem.AxisCount];
        private static UnityEngine.UI.Text _activeArchetypeMatchText;
        private static UnityEngine.UI.InputField _activeNameInput;
        private static UnityEngine.UI.Text _fallbackNameText;
        private static UnityEngine.UI.InputField _activeTeamInput;
        private static UnityEngine.UI.Text _fallbackTeamText;
        private static UnityEngine.UI.InputField _activeFandomInput;
        private static UnityEngine.UI.Text _fallbackFandomText;

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
                MelonLogger.Msg($"[Faz 10.0] 'Mod Icerigi' uGUI HUD kuruldu! (Canvas: '{hostCanvas.name}', Boyut: {cw:F0}x{ch:F0}, Olcek: {_uiScale:F2}, Font: '{fName}')");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] uGUI kurulum hatasi: {ex}");
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
                int dmCnt = TivitirSystem.GetUnreadDmCount();
                string suffix = SocialSaveManager.Current.BoycottActive ? " [🔥BOYKOT]"
                    : (dmCnt > 0 ? $" ({dmCnt} DM)" : (activeTm > 0 ? $" ({activeTm} TM)" : ""));
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

        public static void OpenUniverseArchitect(IntPtr gamePtr, string gameTitle, bool openCastAfter)
        {
            EnsureHUD();
            if (_modalRoot == null) return;

            try
            {
                _isRosterOnlyMode = !openCastAfter;
                _universeSetupStartsCastAfter = openCastAfter;
                _targetGamePtr = gamePtr;
                _targetGameTitle = string.IsNullOrEmpty(gameTitle) ? "Yeni Oyun" : gameTitle;

                var u = UniverseSaveManager.Current;
                _uniDraftName = u.UniverseConfigured ? (u.UniverseName ?? "") : "";
                _uniDraftFandom = u.UniverseFandomName ?? "";
                _uniDraftAxes = (float[])UniverseSaveManager.GetUniverseAxes().Clone();
                _uniDraftRule = UniverseSaveManager.GetUniverseRule();
                _uniDraftLaw = UniverseSaveManager.GetLaw() == CosmologyLaw.None ? CosmologyLaw.FlexibleMultiverse : UniverseSaveManager.GetLaw();

                IsOpen = true;
                _modalRoot.SetActive(true);
                BringToFront();
                UpdateHudState();
                PauseGameSimulation();

                ShowUniverseArchitectView();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] OpenUniverseArchitect hatasi: {ex}");
                CloseModal();
            }
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
                _workingIsSagaFinale = false;

                // Tersine Konuk DLC ile kiralanan rakip yildizlari otomatik kadroya yerlestir!
                var pendingGuests = UniverseSaveManager.Current.PendingRivalGuests;
                if (pendingGuests != null && pendingGuests.Count > 0)
                {
                    for (int g = 0; g < pendingGuests.Count; g++)
                    {
                        var pg = pendingGuests[g];
                        _workingCast.Add(new ActiveCharacterDraft
                        {
                            IsReturning = false,
                            IsNpc = false,
                            IsRivalGuestHire = true,
                            SourceRivalId = pg.RivalId,
                            SourceRivalName = pg.RivalName,
                            SourceRivalStarId = pg.StarId,
                            Name = pg.StarName,
                            ArchetypeId = pg.ArchetypeId,
                            Role = CharacterRole.Sidekick,
                            Axes = pg.Axes != null ? CharacterAxisSystem.NormalizeAxes(pg.Axes) : CharacterAxisSystem.NormalizeAxes(null)
                        });
                    }
                }

                IsOpen = true;
                _modalRoot.SetActive(true);
                BringToFront();
                UpdateHudState();
                PauseGameSimulation();

                ShowCastOverview();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] OpenCastManager hatasi: {ex}");
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
                MelonLogger.Error($"[Faz 10.0] OpenModContentHub hatasi: {ex}");
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
            int dmCnt = TivitirSystem.GetUnreadDmCount();
            string tivLbl = dmCnt > 0 ? $"𝕏 Tivitır ({dmCnt})" : "𝕏 Tivitır";
            string[] labels =
            {
                $"Karakter ({u.Characters.Count})",
                "Transmedya",
                "Kozmoloji",
                "Şirket & Dip.",
                "Sıralama",
                "Haberler",
                tivLbl
            };

            float tabW = 148f;
            float gap = 8f;
            for (int t = 0; t < 7; t++)
            {
                int capT = t;
                bool isSel = (activeTab == t);
                Color bg = isSel ? ColGreen
                    : (t == 6 ? (SocialSaveManager.Current.BoycottActive ? ColRed : new Color(0.11f, 0.16f, 0.24f, 1f))
                    : (t == 1 ? ColPurple : (t == 3 ? ColTeal : (t == 4 || t == 5 ? ColBlue : ColYellow))));
                Color fg = isSel || t == 1 || t == 3 || t == 4 || t == 5 || t == 6 ? Color.white : ColDarkText;

                var btn = CreateButton(_contentRoot, labels[t], bg, fg, 13, () =>
                {
                    _statusBannerMsg = "";
                    if (capT == 0) ShowFullRosterView();
                    else if (capT == 1) ShowTransmediaStudioView();
                    else if (capT == 2) ShowCosmologyView();
                    else if (capT == 3) ShowCompanyView();
                    else if (capT == 4) ShowLeaderboardView();
                    else if (capT == 5) ShowRivalNewsView();
                    else ShowTivitirView();
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
                var actUni = UniverseSaveManager.GetActiveOwnedUniverse();
                string pairSummary = "", multiSummary = "";
                bool isMultiIp = InteractiveEventSystem.DetectMultiIpCrisis(_workingCast, out multiSummary);
                bool isCrossover = isMultiIp || InteractiveEventSystem.DetectCrossoverCast(_workingCast, out pairSummary);
                string teamPreview = GetTeamSynergyPreview(_workingCast);
                int bondCountInCast = CountActiveBondsInWorkingCast();

                Color bannerBg = isMultiIp ? ColRed : (isCrossover ? ColPurple : (!string.IsNullOrEmpty(teamPreview) ? ColTeal : ColCard));
                Color bannerFg = (isCrossover || !string.IsNullOrEmpty(teamPreview)) ? Color.white : ColDarkText;
                string infoLine;
                if (isMultiIp)
                    infoLine = $"🌌 ÇOKLU IP KRİZİ! ({multiSummary})" + (string.IsNullOrEmpty(teamPreview) ? "" : $" | {teamPreview}");
                else if (isCrossover)
                    infoLine = $"★ CROSSOVER! ({pairSummary})" + (string.IsNullOrEmpty(teamPreview) ? "" : $" | {teamPreview}");
                else if (!string.IsNullOrEmpty(teamPreview))
                    infoLine = $"Evren: {actUni.Name} (Faz {actUni.SagaNumber}) | {teamPreview} | Bağ: {bondCountInCast}";
                else
                    infoLine = $"Evren: {actUni.Name} (Faz {actUni.SagaNumber}) | Tut: {u.CanonIntegrity}/100 | Bariyer: {u.LoreBarrier}/100 | Bağ: {bondCountInCast}";

                bool canSwitchUni = u.OwnedUniverses.Count > 1;
                bool canSagaFinale = actUni.SagaGamesCount >= 3 || u.LoreBarrier >= 45;

                var infoCard = CreatePanel(_contentRoot, bannerBg);
                PlaceRect(infoCard.GetComponent<RectTransform>(), 0f, 0f, 1084f, 52f);
                var infoTxt = CreateText(infoCard.transform, infoLine, 14, bannerFg, TextAnchor.MiddleLeft, true);
                var irt = infoTxt.GetComponent<RectTransform>();
                irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
                irt.offsetMin = new Vector2(14f, 2f); irt.offsetMax = new Vector2(canSwitchUni || canSagaFinale ? -380f : -14f, -2f);

                if (canSwitchUni)
                {
                    var btnSw = CreateButton(infoCard.transform, $"Evren Değiştir ({u.OwnedUniverses.Count}) >", ColYellow, ColDarkText, 13, () =>
                    {
                        int idx = 0;
                        for (int k = 0; k < u.OwnedUniverses.Count; k++)
                            if (u.OwnedUniverses[k].Id == actUni.Id) { idx = k; break; }
                        var nextUni = u.OwnedUniverses[(idx + 1) % u.OwnedUniverses.Count];
                        UniverseSaveManager.SwitchActiveUniverse(nextUni.Id);
                        _workingIsSagaFinale = false;
                        ShowCastOverview();
                    });
                    var swrt = btnSw.GetComponent<RectTransform>();
                    swrt.anchorMin = new Vector2(1f, 0.5f); swrt.anchorMax = new Vector2(1f, 0.5f);
                    swrt.pivot = new Vector2(1f, 0.5f); swrt.anchoredPosition = new Vector2(canSagaFinale ? -194f : -10f, 0f); swrt.sizeDelta = new Vector2(178f, 36f);
                }

                if (canSagaFinale)
                {
                    var btnSaga = CreateButton(infoCard.transform, _workingIsSagaFinale ? "★ SAGA FİNALİ: AÇIK" : "Saga Finali Yap", _workingIsSagaFinale ? ColGreen : ColBlue, Color.white, 13, () =>
                    {
                        _workingIsSagaFinale = !_workingIsSagaFinale;
                        ShowCastOverview();
                    });
                    var sgrt = btnSaga.GetComponent<RectTransform>();
                    sgrt.anchorMin = new Vector2(1f, 0.5f); sgrt.anchorMax = new Vector2(1f, 0.5f);
                    sgrt.pivot = new Vector2(1f, 0.5f); sgrt.anchoredPosition = new Vector2(-10f, 0f); sgrt.sizeDelta = new Vector2(176f, 36f);
                }

                // 5 Ust Aksiyon Butonu (Özel, NPC, Eski Çağır, Bağ Kur, Mod İçeriği)
                var btnNew = CreateButton(_contentRoot, "+ Özel Yaz", ColGreen, Color.white, 16, () =>
                {
                    _promotingNpcId = null;
                    _evolvingCharId = null;
                    _draftName = "";
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
                    _npcDraftName = "";
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
                            string guestTag = d.IsRivalGuestHire ? $" [🎟 KONUK — {d.SourceRivalName}]" : "";
                            line1 = $"[ÖZEL — {CategoryLabels.RoleToTurkish(d.Role)}]  {d.Name}{guestTag} ({matchedArch.Name}, {tag}{evoTag}) | {teamTxt}{traitTxt}{fateTag}";
                            line2 = CharacterAxisSystem.FormatCompactSummary(d.Axes);
                        }

                        bool showRoleBtn = !d.IsNpc;
                        float rightReserve = 122f + (showRoleBtn ? 162f : 0f) + (showFateBtn ? 156f : 0f);
                        var lbl = CreateText(card.transform, $"{line1}\n{line2}", 13, ColDarkText, TextAnchor.MiddleLeft, false);
                        var lrt = lbl.GetComponent<RectTransform>();
                        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                        lrt.offsetMin = new Vector2(14f, 4f); lrt.offsetMax = new Vector2(-rightReserve, -4f);

                        if (showRoleBtn)
                        {
                            Color rCol = d.Role == CharacterRole.Protagonist ? ColGreen
                                : (d.Role == CharacterRole.Villain ? ColRed
                                : (d.Role == CharacterRole.Sidekick ? ColBlue : ColTeal));
                            var roleBtn = CreateButton(card.transform, $"Rol: {CategoryLabels.RoleToTurkish(d.Role)} ↻", rCol, Color.white, 12, () =>
                            {
                                if (d.Role == CharacterRole.Protagonist) d.Role = CharacterRole.Villain;
                                else if (d.Role == CharacterRole.Villain) d.Role = CharacterRole.Sidekick;
                                else if (d.Role == CharacterRole.Sidekick) d.Role = CharacterRole.Minor;
                                else d.Role = CharacterRole.Protagonist;
                                ShowCastOverview();
                            });
                            var rrt = roleBtn.GetComponent<RectTransform>();
                            rrt.anchorMin = new Vector2(1f, 0.5f); rrt.anchorMax = new Vector2(1f, 0.5f);
                            rrt.pivot = new Vector2(1f, 0.5f);
                            rrt.anchoredPosition = new Vector2(showFateBtn ? -280f : -124f, 0f);
                            rrt.sizeDelta = new Vector2(154f, 46f);
                        }

                        if (showFateBtn)
                        {
                            Color fCol = d.PlannedFate == CharacterFate.Sacrificed ? ColRed : (d.PlannedFate == CharacterFate.Retired ? ColPurple : ColYellow);
                            Color fTxt = d.PlannedFate == CharacterFate.Alive ? ColDarkText : Color.white;
                            string fLabel = d.PlannedFate == CharacterFate.Alive ? "Kader: Normal" : CategoryLabels.FateToTurkish(d.PlannedFate);

                            var fateBtn = CreateButton(card.transform, fLabel, fCol, fTxt, 13, () =>
                            {
                                if (d.PlannedFate == CharacterFate.Alive) d.PlannedFate = CharacterFate.Sacrificed;
                                else if (d.PlannedFate == CharacterFate.Sacrificed) d.PlannedFate = CharacterFate.Retired;
                                else d.PlannedFate = CharacterFate.Alive;
                                ShowCastOverview();
                            });
                            var frt = fateBtn.GetComponent<RectTransform>();
                            frt.anchorMin = new Vector2(1f, 0.5f); frt.anchorMax = new Vector2(1f, 0.5f);
                            frt.pivot = new Vector2(1f, 0.5f);
                            frt.anchoredPosition = new Vector2(-124f, 0f);
                            frt.sizeDelta = new Vector2(148f, 46f);
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
                    InteractiveEventSystem.RegisterCastForEvents(_targetGamePtr, _targetGameTitle, finalCast, _workingIsSagaFinale);
                    MelonLogger.Msg($"[Faz 10.0] '{_targetGameTitle}' kadrosu onaylandi ({specialCount} ozel, {npcCount} NPC, {finalBonds.Count} yeni bag, Crossover={isCrossover}).");
                    CloseModal();
                });
                PlaceRect(btnConfirm.GetComponent<RectTransform>(), 0f, 506f, 1084f, 56f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] ShowCastOverview hatasi: {ex}");
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
                MelonLogger.Error($"[Faz 10.0] ShowBondEditorView hatasi: {ex}");
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

                // ===== SAG SUTUN (x=538, w=546): 7 KUTUPLU EKSEN (KAYDIRILABILIR!) =====
                float rx = 538f;
                float rw = 546f;
                _draftAxes = CharacterAxisSystem.NormalizeAxes(_draftAxes);

                var matchCard = CreatePanel(_contentRoot, ColHighlight);
                PlaceRect(matchCard.GetComponent<RectTransform>(), rx, 2f, rw, 50f);
                string subBanner = isEvolve
                    ? "8 ekseni kaydırıp evrimini ayarla (+%1 Evrim + İhanet/Kefaret arkı!)"
                    : "8 Kutuplu Eksen (aşağı kaydır) — evrenin 6 ekseniyle doğrudan uyum kurar!";
                _activeArchetypeMatchText = CreateText(matchCard.transform,
                    $"Yakışan Tarz: {currentClosest.Name}  |  Evren: {UniverseSaveManager.Current.UniverseName}\n{subBanner}",
                    13, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(_activeArchetypeMatchText.GetComponent<RectTransform>(), 12f, 2f);

                var axisScroll = CreateScrollView(_contentRoot, rx, 58f, rw, 440f);
                float cardW = rw - 16f;
                float ay = 0f;

                for (int i = 0; i < CharacterAxisSystem.AxisCount; i++)
                {
                    int axIdx = i;
                    var card = CreatePanel(axisScroll.content, ColCard);
                    PlaceRect(card.GetComponent<RectTransform>(), 0f, ay, cardW, 96f);

                    var valTxt = CreateText(card.transform,
                        $"{axIdx + 1}. {CharacterAxisSystem.AxisNames[axIdx]}:  {CharacterAxisSystem.FormatAxisReading(axIdx, _draftAxes[axIdx])}",
                        16, ColHeader, TextAnchor.MiddleLeft, true);
                    PlaceRect(valTxt.GetComponent<RectTransform>(), 12f, 4f, cardW - 24f, 24f);
                    _activeAxisValueTexts[axIdx] = valTxt;

                    var leftLbl = CreateText(card.transform, $"◄ {CharacterAxisSystem.LeftPoles[axIdx]}", 12, ColDarkText, TextAnchor.MiddleLeft, true);
                    PlaceRect(leftLbl.GetComponent<RectTransform>(), 12f, 30f, (cardW - 24f) * 0.5f, 18f);

                    var rightLbl = CreateText(card.transform, $"{CharacterAxisSystem.RightPoles[axIdx]} ►", 12, ColDarkText, TextAnchor.MiddleRight, true);
                    PlaceRect(rightLbl.GetComponent<RectTransform>(), 12f + (cardW - 24f) * 0.5f, 30f, (cardW - 24f) * 0.5f, 18f);

                    var slider = CreateSlider(card.transform, _draftAxes[axIdx], val =>
                    {
                        float c = Mathf.Clamp01(val);
                        _draftAxes[axIdx] = c;
                        if (_activeAxisValueTexts[axIdx] != null)
                            _activeAxisValueTexts[axIdx].text = $"{axIdx + 1}. {CharacterAxisSystem.AxisNames[axIdx]}:  {CharacterAxisSystem.FormatAxisReading(axIdx, c)}";
                        if (_activeArchetypeMatchText != null)
                        {
                            var m = CharacterAxisSystem.DetectClosestArchetype(_draftAxes);
                            _activeArchetypeMatchText.text = $"Yakışan Tarz: {m.Name}  |  Evren: {UniverseSaveManager.Current.UniverseName}\n{subBanner}";
                        }
                    });
                    PlaceRect(slider.GetComponent<RectTransform>(), 16f, 56f, cardW - 32f, 30f);

                    ay += 104f;
                }
                axisScroll.content.sizeDelta = new Vector2(0f, Mathf.Max(424f, ay));

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
                MelonLogger.Error($"[Faz 10.0] ShowCreateCharacterView hatasi: {ex}");
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
                MelonLogger.Error($"[Faz 10.0] ShowCreateNpcView hatasi: {ex}");
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
                    var roleBanner = CreatePanel(_contentRoot, ColHighlight);
                    PlaceRect(roleBanner.GetComponent<RectTransform>(), 0f, 48f, 1084f, 44f);
                    var rLbl = CreateText(roleBanner.transform, "Eklenecek Rol (Önce Seç):", 15, ColDarkText, TextAnchor.MiddleLeft, true);
                    PlaceRect(rLbl.GetComponent<RectTransform>(), 12f, 0f, 220f, 44f);

                    CharacterRole[] roles = { CharacterRole.Protagonist, CharacterRole.Villain, CharacterRole.Sidekick, CharacterRole.Minor };
                    string[] roleShort = { "★ Başrol", "⚔ Kötü Adam", "🤝 Yardımcı", "👤 Herhangi Biri" };
                    for (int r = 0; r < 4; r++)
                    {
                        CharacterRole capR = roles[r];
                        bool sel = (_returningRole == capR);
                        var rb = CreateButton(roleBanner.transform, sel ? $"✓ {roleShort[r]}" : roleShort[r], sel ? ColPurple : ColYellow, sel ? Color.white : ColDarkText, 14, () =>
                        {
                            _returningRole = capR;
                            ShowReturningPickerView();
                        });
                        PlaceRect(rb.GetComponent<RectTransform>(), 240f + r * 209f, 4f, 200f, 36f);
                    }
                }
                else
                {
                    var tipTxt = CreateText(_contentRoot, "İpucu: NPC'yi bağlı olduğu özel karakter olmadan eklemek hafif bağlam riski taşır.", 14, ColMutedText, TextAnchor.MiddleLeft, true);
                    PlaceRect(tipTxt.GetComponent<RectTransform>(), 496f, 0f, 588f, 42f);
                }

                float scY = _returningShowNpcs ? 54f : 100f;
                float scH = _returningShowNpcs ? 442f : 396f;
                var scroll = CreateScrollView(_contentRoot, 0f, scY, 1084f, scH);
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

                            string uBadge = string.IsNullOrEmpty(c.UniverseName) ? "" : $" [{c.UniverseName}]";
                            line = $"{c.Name}{uBadge}  [{c.Status}]  —  {c.ArchetypeName}  |  Ekip: {teamTxt}{badgeStr}\n" +
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

                        string pickLbl = c.IsNpc ? "+ NPC Ekle" : $"+ {CategoryLabels.RoleToTurkish(_returningRole)}";
                        var pickBtn = CreateButton(card.transform, pickLbl, c.IsNpc ? ColTeal : ColGreen, Color.white, 14, () =>
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
                                    UniverseId = c.UniverseId ?? "",
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
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(scH - 16f, yCursor));

                var btnBack = CreateButton(_contentRoot, "< Kadroya Dön", ColYellow, ColDarkText, 19, () => ShowCastOverview());
                PlaceRect(btnBack.GetComponent<RectTransform>(), 0f, 508f, 260f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] ShowReturningPickerView hatasi: {ex}");
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
                var actUniR = UniverseSaveManager.GetActiveOwnedUniverse();
                var sumTxt = CreateText(summary.transform,
                    $"Aktif Evren: {actUniR.Name} (Faz {actUniR.SagaNumber})  |  Tutarlılık: {u.CanonIntegrity}/100  |  Bariyer: {u.LoreBarrier}/100  |  Ekip: {u.Teams.Count}",
                    14, ColDarkText, TextAnchor.MiddleLeft, true);
                var smrt = sumTxt.GetComponent<RectTransform>();
                smrt.anchorMin = Vector2.zero; smrt.anchorMax = Vector2.one;
                smrt.offsetMin = new Vector2(14f, 0f); smrt.offsetMax = new Vector2(-235f, 0f);

                var btnFilt = CreateButton(summary.transform, _filterByActiveUniverse ? $"Filtre: Sadece {actUniR.Name}" : "Filtre: Tüm Evrenler",
                    _filterByActiveUniverse ? ColPurple : ColBlue, Color.white, 12, () =>
                {
                    _filterByActiveUniverse = !_filterByActiveUniverse;
                    ShowFullRosterView();
                });
                var frt = btnFilt.GetComponent<RectTransform>();
                frt.anchorMin = new Vector2(1f, 0.5f); frt.anchorMax = new Vector2(1f, 0.5f);
                frt.pivot = new Vector2(1f, 0.5f); frt.anchoredPosition = new Vector2(-8f, 0f); frt.sizeDelta = new Vector2(218f, 30f);

                // 4 alt sekme (4*265 + 3*8 = 1084px): Ozel / NPC / Bag-Muze-DLC / Rakip Yildizlar
                int leaseCount = u.GuestLeases != null ? u.GuestLeases.Count : 0;
                int liveRivalStars = 0;
                if (u.RivalStars != null)
                    for (int k = 0; k < u.RivalStars.Count; k++)
                        if (!u.RivalStars[k].TransferredToPlayer) liveRivalStars++;

                var subSpecial = CreateButton(_contentRoot, $"★ Özel ({specials.Count})", _rosterSubTab == 0 ? ColGreen : ColYellow, _rosterSubTab == 0 ? Color.white : ColDarkText, 14, () =>
                {
                    _rosterSubTab = 0; _statusBannerMsg = ""; ShowFullRosterView();
                });
                PlaceRect(subSpecial.GetComponent<RectTransform>(), 0f, 98f, 265f, 38f);

                var subNpc = CreateButton(_contentRoot, $"NPC ({npcs.Count}) — Terfi", _rosterSubTab == 1 ? ColTeal : ColYellow, _rosterSubTab == 1 ? Color.white : ColDarkText, 14, () =>
                {
                    _rosterSubTab = 1; _statusBannerMsg = ""; ShowFullRosterView();
                });
                PlaceRect(subNpc.GetComponent<RectTransform>(), 273f, 98f, 265f, 38f);

                var subBonds = CreateButton(_contentRoot, $"Bağ & Müze ({bondCount + leaseCount})", _rosterSubTab == 2 ? ColPurple : ColYellow, _rosterSubTab == 2 ? Color.white : ColDarkText, 14, () =>
                {
                    _rosterSubTab = 2; _statusBannerMsg = ""; ShowFullRosterView();
                });
                PlaceRect(subBonds.GetComponent<RectTransform>(), 546f, 98f, 265f, 38f);

                var subRivalStars = CreateButton(_contentRoot, $"🌟 Rakip Yıldız ({liveRivalStars})", _rosterSubTab == 3 ? ColBlue : ColYellow, _rosterSubTab == 3 ? Color.white : ColDarkText, 14, () =>
                {
                    _rosterSubTab = 3; _statusBannerMsg = ""; ShowFullRosterView();
                });
                PlaceRect(subRivalStars.GetComponent<RectTransform>(), 819f, 98f, 265f, 38f);

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
                            if (_filterByActiveUniverse && c.UniverseId != u.ActiveUniverseId) continue;
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
                            if (GuestDlcSystem.IsCharacterOnActiveLease(c.Id)) arcStr += " | 🎟 Konuk DLC'de";
                            string spStr = c.SpinOffDemandRemaining > 0 ? $" | 🎬 Spin-Off İsteniyor!" : "";
                            string axLine = CharacterAxisSystem.FormatCompactSummary(UniverseSaveManager.EnsureCharacterAxes(c));

                            var txt = CreateText(card.transform,
                                $"{c.Name} [{(string.IsNullOrEmpty(c.UniverseName) ? u.UniverseName : c.UniverseName)}] [{c.Status}] — {c.ArchetypeName} ({c.LastRole}) | Pop: {c.Popularity}/100 | {fatigue}{arcStr}{spStr}\n" +
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
                                _draftAxes = new[] { 0.50f, 0.55f, 0.60f, 0.40f, 0.50f, 0.50f, 0.50f, 0.50f };
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
                else if (_rosterSubTab == 2) // Baglar, Olumsuz Efsaneler Muzesi & Konuk DLC
                {
                    // Konuk DLC teklif isteme karti (en ustte)
                    var compRef = UniverseArchitectMod.ActiveCompany;
                    int activeLeases = GuestDlcSystem.GetActiveLeaseCount();
                    int eligibleStars = GuestDlcSystem.GetEligibleStars().Count;
                    var dlcHeaderCard = CreatePanel(scroll.content, ColCard);
                    PlaceRect(dlcHeaderCard.GetComponent<RectTransform>(), 0f, yCursor, innerW, 64f);
                    var dlcTxt = CreateText(dlcHeaderCard.transform,
                        $"Konuk DLC Kiralama (Guest DLC)  |  Aktif Anlaşma: {activeLeases}/2  |  Uygun Yıldız: {eligibleStars}\nRakipler otomatik teklif getirir veya buradan kataloğunu gönderip teklif isteyebilirsin.",
                        14, ColDarkText, TextAnchor.MiddleLeft, false);
                    var dtrt = dlcTxt.GetComponent<RectTransform>();
                    dtrt.anchorMin = Vector2.zero; dtrt.anchorMax = Vector2.one;
                    dtrt.offsetMin = new Vector2(14f, 4f); dtrt.offsetMax = new Vector2(-230f, -4f);

                    var pitchBtn = CreateButton(dlcHeaderCard.transform, "Rakiplere Yıldız Öner", eligibleStars > 0 ? ColPurple : ColTrack, Color.white, 14, () =>
                    {
                        string msg;
                        GuestDlcSystem.TryRequestManualOffer(compRef, out msg);
                        _statusBannerMsg = msg;
                        ShowFullRosterView();
                    });
                    var ptrt = pitchBtn.GetComponent<RectTransform>();
                    ptrt.anchorMin = new Vector2(1f, 0.5f); ptrt.anchorMax = new Vector2(1f, 0.5f);
                    ptrt.pivot = new Vector2(1f, 0.5f); ptrt.anchoredPosition = new Vector2(-12f, 0f); ptrt.sizeDelta = new Vector2(204f, 44f);
                    yCursor += 72f;

                    // Aktif & Tamamlanan Konuk DLC kayitlari
                    if (u.GuestLeases != null && u.GuestLeases.Count > 0)
                    {
                        double nowW = compRef != null ? compRef.currentWeek : 0.0;
                        for (int i = u.GuestLeases.Count - 1; i >= 0; i--)
                        {
                            var l = u.GuestLeases[i];
                            var lCard = CreatePanel(scroll.content, l.IsResolved ? ColCard : ColHighlight);
                            PlaceRect(lCard.GetComponent<RectTransform>(), 0f, yCursor, innerW, 64f);
                            string sub = l.IsResolved
                                ? l.ResultSummary
                                : $"KİRALIK YAYIN BEKLENİYOR — Kalan: ~{Math.Max(0.1, l.ResolveWeek - nowW):F1} hafta | Peşin: +{l.UpfrontFee / 1000.0:F0}K";
                            var lTxt = CreateText(lCard.transform,
                                $"[KONUK DLC]  {l.CharacterName} → {l.RivalName} (\'{l.RivalGameTitle}\')\n{sub}",
                                14, ColDarkText, TextAnchor.MiddleLeft, false);
                            StretchWithPad(lTxt.GetComponent<RectTransform>(), 14f, 4f);
                            yCursor += 72f;
                        }
                    }

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

                else // _rosterSubTab == 3: FAZ 9.0 CANLI RAKIP YILDIZLARI + STUDYO FILTRESI + OYUN KATALOGU
                {
                    var compRef = UniverseArchitectMod.ActiveCompany;
                    var indepRivals = HoldingEndgameSystem.GetIndependentRivals();
                    if (_rivalStarStudioFilterIdx >= indepRivals.Count) _rivalStarStudioFilterIdx = -1;

                    RivalStudioRecord selRival = (_rivalStarStudioFilterIdx >= 0 && _rivalStarStudioFilterIdx < indepRivals.Count)
                        ? indepRivals[_rivalStarStudioFilterIdx]
                        : null;

                    var sortedStars = new List<RivalStarRecord>();
                    if (u.RivalStars != null)
                    {
                        for (int i = 0; i < u.RivalStars.Count; i++)
                        {
                            var rs = u.RivalStars[i];
                            if (rs.TransferredToPlayer) continue;
                            var rv = UniverseSaveManager.FindRivalById(rs.RivalId);
                            if (rv != null && (rv.IsAcquiredByPlayer || rv.IsAbsorbedByRival)) continue;
                            if (selRival != null && rs.RivalId != selRival.Id) continue;
                            sortedStars.Add(rs);
                        }
                    }
                    sortedStars.Sort((a, b) => b.Popularity.CompareTo(a.Popularity));

                    // Ustte Studyo Filtresi + Fandom & Oyun Katalogu Ozeti!
                    var filtCard = CreatePanel(scroll.content, selRival != null ? ColHighlight : ColCard);
                    PlaceRect(filtCard.GetComponent<RectTransform>(), 0f, yCursor, innerW, selRival != null ? 86f : 56f);

                    string filtHeader;
                    if (selRival != null)
                    {
                        var tplF = Phase1Database.FindRivalTemplate(selRival.Id);
                        string fBadge = !string.IsNullOrEmpty(selRival.FandomName) ? $"  |  ♥ Fan Topluluğu: {selRival.FandomName}" : "";
                        int showGames = Math.Min(tplF.TitlePool.Length, 5);
                        var gSample = new List<string>();
                        for (int g = 0; g < showGames; g++) gSample.Add(tplF.TitlePool[g]);
                        filtHeader = $"🏛 {selRival.Name} — {selRival.UniverseName} ({sortedStars.Count} Karakter){fBadge}\n" +
                                     $"Oyunlar ({tplF.TitlePool.Length}): {string.Join(", ", gSample)}...";
                    }
                    else
                    {
                        filtHeader = $"Tüm Bağımsız Stüdyoların Yıldızları ({sortedStars.Count} Canlı Karakter — İlk 50 Gösterilir)\nBelirli bir evrenin (NJG / Entakos vb.) tüm kadrosunu & oyunlarını görmek için sağdan stüdyo seç!";
                    }

                    var filtTxt = CreateText(filtCard.transform, filtHeader, 13, ColDarkText, TextAnchor.MiddleLeft, true);
                    var ftrt = filtTxt.GetComponent<RectTransform>();
                    ftrt.anchorMin = Vector2.zero; ftrt.anchorMax = Vector2.one;
                    ftrt.offsetMin = new Vector2(14f, 4f); ftrt.offsetMax = new Vector2(-240f, -4f);

                    var btnCycleStudio = CreateButton(filtCard.transform, selRival == null ? "Stüdyo Seç >" : "Sonraki Stüdyo >", ColBlue, Color.white, 13, () =>
                    {
                        _rivalStarStudioFilterIdx++;
                        if (_rivalStarStudioFilterIdx >= indepRivals.Count) _rivalStarStudioFilterIdx = -1;
                        ShowFullRosterView();
                    });
                    var csrt = btnCycleStudio.GetComponent<RectTransform>();
                    csrt.anchorMin = new Vector2(1f, 0.5f); csrt.anchorMax = new Vector2(1f, 0.5f);
                    csrt.pivot = new Vector2(1f, 0.5f); csrt.anchoredPosition = new Vector2(-12f, 0f); csrt.sizeDelta = new Vector2(214f, 40f);
                    yCursor += (selRival != null ? 94f : 64f);

                    if (sortedStars.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColHighlight);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, yCursor, innerW, 84f);
                        var et = CreateText(empty.transform, "Bu filtrede dışarıda kalan rakip yıldız yok!", 16, ColDarkText, TextAnchor.MiddleCenter, true);
                        StretchWithPad(et.GetComponent<RectTransform>(), 16f, 8f);
                        yCursor += 92f;
                    }
                    else
                    {
                        int showMax = selRival != null ? sortedStars.Count : Math.Min(sortedStars.Count, 50);
                        for (int i = 0; i < showMax; i++)
                        {
                            var rs = sortedStars[i];
                            var rv = UniverseSaveManager.FindRivalById(rs.RivalId);
                            int rel = rv != null ? rv.Relation : 0;
                            double hireFee = MultiverseEmpireSystem.CalcReverseGuestFee(rs, rv);
                            double buyFee = MultiverseEmpireSystem.CalcSingleStarBuyoutPrice(rs, rv);

                            var card = CreatePanel(scroll.content, rel >= 15 ? ColHighlight : ColCard);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 74f);

                            var arch = Phase1Database.FindArchetype(rs.ArchetypeId);
                            var txt = CreateText(card.transform,
                                $"🌟 {rs.Name} ({rs.RivalName} — {rs.UniverseName})  |  Pop: {rs.Popularity}/100  |  İlişki: {rel:+#;-#;0}\n" +
                                $"Tarz: {arch.Name}  |  Ekip: {(string.IsNullOrEmpty(rs.TeamName) ? "Bağımsız" : rs.TeamName)}",
                                14, ColDarkText, TextAnchor.MiddleLeft, false);
                            var trt = txt.GetComponent<RectTransform>();
                            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                            trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(-365f, -4f);

                            bool canHire = rel >= 15 && compRef != null && compRef.cash >= hireFee;
                            var btnHire = CreateButton(card.transform, $"🎟 Konuk Kirala\n(-{hireFee / 1000.0:F0}K)", canHire ? ColTeal : ColTrack, Color.white, 12, () =>
                            {
                                string msg; MultiverseEmpireSystem.TryHireRivalGuestStar(compRef, rs, out msg);
                                _statusBannerMsg = msg; ShowFullRosterView();
                            });
                            var hrt = btnHire.GetComponent<RectTransform>();
                            hrt.anchorMin = new Vector2(1f, 0.5f); hrt.anchorMax = new Vector2(1f, 0.5f);
                            hrt.pivot = new Vector2(1f, 0.5f); hrt.anchoredPosition = new Vector2(-186f, 0f); hrt.sizeDelta = new Vector2(166f, 52f);

                            bool canBuyIp = (rel >= 20 || (rv != null && rv.CanonIntegrity <= 72)) && compRef != null && compRef.cash >= buyFee;
                            var btnBuyIp = CreateButton(card.transform, $"★ IP Satın Al\n(-{buyFee / 1000.0:F0}K)", canBuyIp ? ColPurple : ColTrack, Color.white, 12, () =>
                            {
                                string msg; MultiverseEmpireSystem.TryBuyRivalStarRights(compRef, rs, out msg);
                                _statusBannerMsg = msg; ShowFullRosterView();
                            });
                            var brt = btnBuyIp.GetComponent<RectTransform>();
                            brt.anchorMin = new Vector2(1f, 0.5f); brt.anchorMax = new Vector2(1f, 0.5f);
                            brt.pivot = new Vector2(1f, 0.5f); brt.anchoredPosition = new Vector2(-12f, 0f); brt.sizeDelta = new Vector2(166f, 52f);

                            yCursor += 82f;
                        }
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
                MelonLogger.Error($"[Faz 10.0] ShowFullRosterView hatasi: {ex}");
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
                MelonLogger.Error($"[Faz 10.0] ShowTransmediaStudioView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // GORUNUM 2C (FAZ 8.0): DETAYLI EVREN MIMARI ATOLYESI (4 EKSEN + KURAL + YASA)
        // ---------------------------------------------------------------------
        private static void ShowUniverseArchitectView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                bool isFirstTime = !u.UniverseConfigured;
                _headerTitle.text = isFirstTime
                    ? $"★ Evren Mimarı Atölyesi — İlk Evrenini Tasarla ({_targetGameTitle})"
                    : $"★ Evren Mimarı Atölyesi — {u.UniverseName} Düzenle";

                _activeNameInput = null;
                _fallbackNameText = null;
                _activeFandomInput = null;
                _fallbackFandomText = null;
                _uniDraftAxes = UniverseAxisSystem.NormalizeAxes(_uniDraftAxes);

                // ===== SOL SUTUN (x=0..515): EVREN ADI + FAN TOPLULUGU + 15 TUR + KURAL + KOZMOLOJI =====
                var lblName = CreateText(_contentRoot, "Evren Adı:", 15, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblName.GetComponent<RectTransform>(), 0f, 2f, 88f, 34f);

                _activeNameInput = CreateInputFieldSafe(_contentRoot, _uniDraftName, "Evrenine isim ver...", val => _uniDraftName = val, out _fallbackNameText);
                if (_activeNameInput != null)
                    PlaceRect(_activeNameInput.GetComponent<RectTransform>(), 92f, 2f, 300f, 34f);
                else if (_fallbackNameText != null)
                    PlaceRect(_fallbackNameText.transform.parent.GetComponent<RectTransform>(), 92f, 2f, 300f, 34f);

                var btnRandUni = CreateButton(_contentRoot, "Rastgele", ColYellow, ColDarkText, 14, () =>
                {
                    _uniDraftName = UniverseAxisSystem.UniverseNamePool[_uniNamePoolIdx % UniverseAxisSystem.UniverseNamePool.Length];
                    _uniNamePoolIdx++;
                    if (_activeNameInput != null) _activeNameInput.text = _uniDraftName;
                    if (_fallbackNameText != null) _fallbackNameText.text = _uniDraftName;
                });
                PlaceRect(btnRandUni.GetComponent<RectTransform>(), 398f, 2f, 117f, 34f);

                // Fan Toplulugu Ozel Ismi (Opsiyonel — orn. TentiTaym, EntaTaym)
                var lblFan = CreateText(_contentRoot, "Fan Adı:", 15, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblFan.GetComponent<RectTransform>(), 0f, 40f, 88f, 34f);

                _activeFandomInput = CreateInputFieldSafe(_contentRoot, _uniDraftFandom, "Fan topluluğu adı (opsiyonel, örn. TentiTaym)", val => _uniDraftFandom = val, out _fallbackFandomText);
                if (_activeFandomInput != null)
                    PlaceRect(_activeFandomInput.GetComponent<RectTransform>(), 92f, 40f, 300f, 34f);
                else if (_fallbackFandomText != null)
                    PlaceRect(_fallbackFandomText.transform.parent.GetComponent<RectTransform>(), 92f, 40f, 300f, 34f);

                var btnRandFan = CreateButton(_contentRoot, "Öner", ColBlue, Color.white, 14, () =>
                {
                    _uniDraftFandom = UniverseAxisSystem.FandomNamePool[_fandomPoolIdx % UniverseAxisSystem.FandomNamePool.Length];
                    _fandomPoolIdx++;
                    if (_activeFandomInput != null) _activeFandomInput.text = _uniDraftFandom;
                    if (_fallbackFandomText != null) _fallbackFandomText.text = _uniDraftFandom;
                });
                PlaceRect(btnRandFan.GetComponent<RectTransform>(), 398f, 40f, 117f, 34f);

                // 15 Hazir Evren Turu (Kaydirilabilir Izgara!)
                var lblPre = CreateText(_contentRoot, "15 Evren Türü Şablonu (Aşağı Kaydır — Opsiyonel Hızlı Doldurma):", 13, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblPre.GetComponent<RectTransform>(), 0f, 78f, 515f, 18f);

                var preScroll = CreateScrollView(_contentRoot, 0f, 98f, 515f, 116f);
                var curDerivedTone = UniverseAxisSystem.DeriveToneFromAxes(_uniDraftAxes);
                float pW = 244f, pH = 38f, pGx = 10f, pGy = 6f;
                for (int i = 0; i < UniverseAxisSystem.AllPresets.Length; i++)
                {
                    UniverseTone capT = UniverseAxisSystem.AllPresets[i];
                    int col = i % 2;
                    int row = i / 2;
                    bool isMatch = (curDerivedTone == capT);
                    var pb = CreateButton(preScroll.content, CategoryLabels.ToneToTurkish(capT), isMatch ? ColGreen : ColCard, isMatch ? Color.white : ColDarkText, 13, () =>
                    {
                        _uniDraftAxes = UniverseAxisSystem.PresetForTone(capT);
                        ShowUniverseArchitectView();
                    });
                    PlaceRect(pb.GetComponent<RectTransform>(), col * (pW + pGx), row * (pH + pGy), pW, pH);
                }
                int preRows = (UniverseAxisSystem.AllPresets.Length + 1) / 2;
                preScroll.content.sizeDelta = new Vector2(0f, Mathf.Max(130f, preRows * (pH + pGy)));

                // Evren Temel Kurali (5 secenek)
                var lblRule = CreateText(_contentRoot, "Evren Temel Kuralı (Özel Pasif Etki):", 14, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblRule.GetComponent<RectTransform>(), 0f, 218f, 515f, 20f);

                UniverseRule[] rules =
                {
                    UniverseRule.None, UniverseRule.MysteriousArchive, UniverseRule.LethalWorld,
                    UniverseRule.TeamSpirit, UniverseRule.ChaosUnleashed
                };
                for (int i = 0; i < rules.Length; i++)
                {
                    UniverseRule capR = rules[i];
                    bool sel = (_uniDraftRule == capR);
                    int row = i < 3 ? 0 : 1;
                    int col = i < 3 ? i : (i - 3);
                    float bw = row == 0 ? 166f : 252f;
                    var rb = CreateButton(_contentRoot, CategoryLabels.UniverseRuleToTurkish(capR), sel ? ColPurple : ColYellow, sel ? Color.white : ColDarkText, 13, () =>
                    {
                        _uniDraftRule = capR;
                        ShowUniverseArchitectView();
                    });
                    PlaceRect(rb.GetComponent<RectTransform>(), col * (bw + 8f), 240f + row * 38f, bw, 33f);
                }

                var ruleDesc = CreateText(_contentRoot, CategoryLabels.UniverseRuleDesc(_uniDraftRule), 12, ColHeader, TextAnchor.MiddleLeft, true);
                PlaceRect(ruleDesc.GetComponent<RectTransform>(), 2f, 316f, 512f, 32f);

                // Kozmoloji Yasasi Secimi
                var lblLaw = CreateText(_contentRoot, "Kozmoloji Anayasası:", 14, ColDarkText, TextAnchor.MiddleLeft, true);
                PlaceRect(lblLaw.GetComponent<RectTransform>(), 0f, 352f, 515f, 20f);

                CosmologyLaw[] laws = { CosmologyLaw.StrictCanon, CosmologyLaw.FlexibleMultiverse, CosmologyLaw.CharacterLegendEra };
                for (int i = 0; i < 3; i++)
                {
                    CosmologyLaw capL = laws[i];
                    bool sel = (_uniDraftLaw == capL);
                    var lb = CreateButton(_contentRoot, CategoryLabels.LawToTurkish(capL), sel ? ColTeal : ColYellow, sel ? Color.white : ColDarkText, 13, () =>
                    {
                        _uniDraftLaw = capL;
                        ShowUniverseArchitectView();
                    });
                    PlaceRect(lb.GetComponent<RectTransform>(), i * 174f, 374f, 166f, 36f);
                }

                var lawDescCard = CreatePanel(_contentRoot, ColCard);
                PlaceRect(lawDescCard.GetComponent<RectTransform>(), 0f, 416f, 515f, 82f);
                var lawDescTxt = CreateText(lawDescCard.transform,
                    $"{CategoryLabels.LawToTurkish(_uniDraftLaw)}: {CategoryLabels.LawShortDesc(_uniDraftLaw)}",
                    12, ColDarkText, TextAnchor.MiddleLeft, false);
                StretchWithPad(lawDescTxt.GetComponent<RectTransform>(), 10f, 4f);

                // ===== SAG SUTUN (x=538, w=546): 6 KUTUPLU EVREN EKSENI (KAYDIRILABILIR + 12 CAG!) =====
                float rx = 538f;
                float rw = 546f;

                var toneCard = CreatePanel(_contentRoot, ColHighlight);
                PlaceRect(toneCard.GetComponent<RectTransform>(), rx, 2f, rw, 50f);
                _activeUniToneText = CreateText(toneCard.transform,
                    $"Baskın Tür: {CategoryLabels.ToneToTurkish(curDerivedTone)}  |  Çağ: {UniverseAxisSystem.GetEraName(_uniDraftAxes[4])}\n6 Evren Ekseni (aşağı kaydır) — karakterlerin 8 ekseniyle doğrudan uyum kurar!",
                    13, ColDarkText, TextAnchor.MiddleLeft, true);
                StretchWithPad(_activeUniToneText.GetComponent<RectTransform>(), 12f, 2f);

                var uniAxisScroll = CreateScrollView(_contentRoot, rx, 58f, rw, 440f);
                float cardW = rw - 16f;
                float uy = 0f;

                for (int i = 0; i < UniverseAxisSystem.AxisCount; i++)
                {
                    int axIdx = i;
                    var card = CreatePanel(uniAxisScroll.content, axIdx == 4 ? ColHighlight : ColCard);
                    PlaceRect(card.GetComponent<RectTransform>(), 0f, uy, cardW, 96f);

                    var valTxt = CreateText(card.transform,
                        $"{axIdx + 1}. {UniverseAxisSystem.AxisNames[axIdx]}:  {UniverseAxisSystem.FormatAxisReading(axIdx, _uniDraftAxes[axIdx])}",
                        16, ColHeader, TextAnchor.MiddleLeft, true);
                    PlaceRect(valTxt.GetComponent<RectTransform>(), 12f, 4f, cardW - 24f, 24f);
                    _activeUniAxisTexts[axIdx] = valTxt;

                    var leftLbl = CreateText(card.transform, $"◄ {UniverseAxisSystem.LeftPoles[axIdx]}", 12, ColDarkText, TextAnchor.MiddleLeft, true);
                    PlaceRect(leftLbl.GetComponent<RectTransform>(), 12f, 30f, (cardW - 24f) * 0.5f, 18f);

                    var rightLbl = CreateText(card.transform, $"{UniverseAxisSystem.RightPoles[axIdx]} ►", 12, ColDarkText, TextAnchor.MiddleRight, true);
                    PlaceRect(rightLbl.GetComponent<RectTransform>(), 12f + (cardW - 24f) * 0.5f, 30f, (cardW - 24f) * 0.5f, 18f);

                    var slider = CreateSlider(card.transform, _uniDraftAxes[axIdx], val =>
                    {
                        float c = Mathf.Clamp01(val);
                        _uniDraftAxes[axIdx] = c;
                        if (_activeUniAxisTexts[axIdx] != null)
                            _activeUniAxisTexts[axIdx].text = $"{axIdx + 1}. {UniverseAxisSystem.AxisNames[axIdx]}:  {UniverseAxisSystem.FormatAxisReading(axIdx, c)}";
                        if (_activeUniToneText != null)
                        {
                            var dt = UniverseAxisSystem.DeriveToneFromAxes(_uniDraftAxes);
                            _activeUniToneText.text = $"Baskın Tür: {CategoryLabels.ToneToTurkish(dt)}  |  Çağ: {UniverseAxisSystem.GetEraName(_uniDraftAxes[4])}\n6 Evren Ekseni (aşağı kaydır) — karakterlerin 8 ekseniyle doğrudan uyum kurar!";
                        }
                    });
                    PlaceRect(slider.GetComponent<RectTransform>(), 16f, 56f, cardW - 32f, 30f);
                    uy += 104f;
                }
                uniAxisScroll.content.sizeDelta = new Vector2(0f, Mathf.Max(424f, uy));

                // ALT BUTONLAR
                if (!_universeSetupStartsCastAfter)
                {
                    var btnCancel = CreateButton(_contentRoot, "< İptal (Kozmolojiye Dön)", ColYellow, ColDarkText, 17, () => ShowCosmologyView());
                    PlaceRect(btnCancel.GetComponent<RectTransform>(), 0f, 508f, 280f, 52f);
                }

                string confirmText = _universeSetupStartsCastAfter
                    ? "★ Evreni Yarat ve Karakter Kadrosuna Geç"
                    : "★ Evren Değişikliklerini Kaydet";
                var btnConfirm = CreateButton(_contentRoot, confirmText, ColGreen, Color.white, 20, () =>
                {
                    string finalUniName = (_activeNameInput != null && !string.IsNullOrWhiteSpace(_activeNameInput.text))
                        ? _activeNameInput.text.Trim()
                        : (string.IsNullOrWhiteSpace(_uniDraftName) ? "Ana Evren" : _uniDraftName.Trim());

                    CosmologyLaw oldLaw = UniverseSaveManager.GetLaw();
                    if (!isFirstTime && oldLaw != CosmologyLaw.None && oldLaw != _uniDraftLaw)
                    {
                        var comp = UniverseArchitectMod.ActiveCompany;
                        if (comp != null && comp.cash >= BalanceCaps.RetconCashCost)
                        {
                            comp.adjustCash(-BalanceCaps.RetconCashCost, "Kozmoloji: Büyük Retcon");
                            UniverseSaveManager.AdjustIntegrity(-BalanceCaps.RetconIntegrityCost);
                            UniverseSaveManager.AdjustReputation(-5);
                        }
                        else
                        {
                            _uniDraftLaw = oldLaw;
                        }
                    }

                    string finalFandom = (_activeFandomInput != null) ? _activeFandomInput.text.Trim() : (_uniDraftFandom ?? "").Trim();
                    UniverseSaveManager.ApplyUniverseConfig(finalUniName, _uniDraftAxes, _uniDraftRule, _uniDraftLaw, finalFandom);
                    MelonLogger.Msg($"[Faz 8.1 Evren Atolyesi] '{finalUniName}' kaydedildi! ({UniverseAxisSystem.FormatCompactSummary(_uniDraftAxes)} | Tur: {CategoryLabels.ToneToTurkish(UniverseSaveManager.GetTone())} | Kural: {CategoryLabels.UniverseRuleToTurkish(_uniDraftRule)})");

                    if (_universeSetupStartsCastAfter)
                    {
                        _universeSetupStartsCastAfter = false;
                        _workingCast = new List<ActiveCharacterDraft>();
                        _workingBonds = new List<PendingBondDraft>();
                        ShowCastOverview();
                    }
                    else
                    {
                        _statusBannerMsg = $"★ '{finalUniName}' evren ayarları güncellendi!";
                        ShowCosmologyView();
                    }
                });
                PlaceRect(btnConfirm.GetComponent<RectTransform>(), _universeSetupStartsCastAfter ? 0f : 538f, 508f, _universeSetupStartsCastAfter ? 1084f : 546f, 52f);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] ShowUniverseArchitectView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // MOD ICERIGI SEKME 2: EVREN, KOZMOLOJI, BEKLENTI BASKISI & 10X TURBO ANAHTARI
        // ---------------------------------------------------------------------
        private static void ShowCosmologyView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                var comp = UniverseArchitectMod.ActiveCompany;
                var currentLaw = UniverseSaveManager.GetLaw();
                var currentRule = UniverseSaveManager.GetUniverseRule();
                double expPen = BalancePressureSystem.ComputeExpectationPenalty(comp);

                _headerTitle.text = $"Mod İçeriği — Evren ({u.UniverseName}), Kozmoloji & Ayarlar";
                DrawModContentTabs(2);

                // FAZ 9.0: COKLU IP EVREN SECICI + EVREN ATOLYESI + SEKTOR DEPREMI
                var actUni = UniverseSaveManager.GetActiveOwnedUniverse();
                double nowW = comp != null ? comp.currentWeek : 0.0;
                string eqBanner = MultiverseEmpireSystem.GetSectorEventBanner(nowW);

                var curCard = CreatePanel(_contentRoot, ColHighlight);
                PlaceRect(curCard.GetComponent<RectTransform>(), 0f, 52f, 1084f, 80f);
                var curTxt = CreateText(curCard.transform,
                    $"Aktif Evren ({u.OwnedUniverses.Count} IP): {actUni.Name}{(!string.IsNullOrEmpty(actUni.FandomName) ? $" [{actUni.FandomName}]" : "")} — {actUni.OwnerStudioName} (Faz {actUni.SagaNumber})\n" +
                    $"Eksenler: {UniverseAxisSystem.FormatCompactSummary(UniverseSaveManager.GetUniverseAxes())}  |  Beklenti Baskısı: -%{expPen * 100.0:F1}\n" +
                    $"{eqBanner}",
                    12, ColDarkText, TextAnchor.MiddleLeft, true);
                var ctrt = curTxt.GetComponent<RectTransform>();
                ctrt.anchorMin = Vector2.zero; ctrt.anchorMax = Vector2.one;
                ctrt.offsetMin = new Vector2(12f, 4f); ctrt.offsetMax = new Vector2(-435f, -4f);

                if (u.OwnedUniverses.Count > 1)
                {
                    var btnNextUni = CreateButton(curCard.transform, $"Evren Değiştir\n({u.OwnedUniverses.Count} Sahip IP) >", ColBlue, Color.white, 12, () =>
                    {
                        int idx = 0;
                        for (int k = 0; k < u.OwnedUniverses.Count; k++)
                            if (u.OwnedUniverses[k].Id == actUni.Id) { idx = k; break; }
                        UniverseSaveManager.SwitchActiveUniverse(u.OwnedUniverses[(idx + 1) % u.OwnedUniverses.Count].Id);
                        _statusBannerMsg = $"Aktif evren değiştirildi: {UniverseSaveManager.Current.UniverseName}!";
                        ShowCosmologyView();
                    });
                    var nrt = btnNextUni.GetComponent<RectTransform>();
                    nrt.anchorMin = new Vector2(1f, 0.5f); nrt.anchorMax = new Vector2(1f, 0.5f);
                    nrt.pivot = new Vector2(1f, 0.5f); nrt.anchoredPosition = new Vector2(-296f, 0f); nrt.sizeDelta = new Vector2(132f, 60f);
                }

                var btnNewIp = CreateButton(curCard.transform, "+ Yeni IP\nEvren Aç", MultiverseEmpireSystem.CountOriginalUniverses() < 3 ? ColTeal : ColTrack, Color.white, 12, () =>
                {
                    string newId, msg;
                    if (MultiverseEmpireSystem.TryCreateNewOriginalUniverse(comp, out newId, out msg))
                    {
                        _universeSetupStartsCastAfter = false;
                        _uniDraftName = "";
                        _uniDraftFandom = "";
                        _uniDraftAxes = new float[] { 0.50f, 0.50f, 0.50f, 0.50f, 0.62f, 0.35f };
                        _uniDraftRule = UniverseRule.None;
                        _uniDraftLaw = CosmologyLaw.FlexibleMultiverse;
                        ShowUniverseArchitectView();
                    }
                    else
                    {
                        _statusBannerMsg = msg;
                        ShowCosmologyView();
                    }
                });
                var nirt = btnNewIp.GetComponent<RectTransform>();
                nirt.anchorMin = new Vector2(1f, 0.5f); nirt.anchorMax = new Vector2(1f, 0.5f);
                nirt.pivot = new Vector2(1f, 0.5f); nirt.anchoredPosition = new Vector2(-156f, 0f); nirt.sizeDelta = new Vector2(132f, 60f);

                var btnEditUni = CreateButton(curCard.transform, "★ Evren Atölyesi\n(15 Tür / 6 Eksen)", ColPurple, Color.white, 12, () =>
                {
                    _universeSetupStartsCastAfter = false;
                    _uniDraftName = u.UniverseName ?? "";
                    _uniDraftFandom = u.UniverseFandomName ?? "";
                    _uniDraftAxes = (float[])UniverseSaveManager.GetUniverseAxes().Clone();
                    _uniDraftRule = currentRule;
                    _uniDraftLaw = currentLaw == CosmologyLaw.None ? CosmologyLaw.FlexibleMultiverse : currentLaw;
                    ShowUniverseArchitectView();
                });
                var ert = btnEditUni.GetComponent<RectTransform>();
                ert.anchorMin = new Vector2(1f, 0.5f); ert.anchorMax = new Vector2(1f, 0.5f);
                ert.pivot = new Vector2(1f, 0.5f); ert.anchoredPosition = new Vector2(-10f, 0f); ert.sizeDelta = new Vector2(138f, 60f);

                // 3 Kozmoloji Yasasi Karti
                CosmologyLaw[] laws = { CosmologyLaw.StrictCanon, CosmologyLaw.FlexibleMultiverse, CosmologyLaw.CharacterLegendEra };
                float cardW = 352f;
                for (int i = 0; i < 3; i++)
                {
                    CosmologyLaw capLaw = laws[i];
                    bool isActive = (currentLaw == capLaw);
                    float cx = i * (cardW + 14f);

                    var lCard = CreatePanel(_contentRoot, ColCard);
                    PlaceRect(lCard.GetComponent<RectTransform>(), cx, 136f, cardW, 186f);

                    var lTitle = CreateText(lCard.transform, CategoryLabels.LawToTurkish(capLaw), 17, ColHeader, TextAnchor.UpperLeft, true);
                    PlaceRect(lTitle.GetComponent<RectTransform>(), 12f, 8f, cardW - 24f, 26f);

                    var lDesc = CreateText(lCard.transform, CategoryLabels.LawShortDesc(capLaw), 13, ColDarkText, TextAnchor.UpperLeft, false);
                    PlaceRect(lDesc.GetComponent<RectTransform>(), 12f, 38f, cardW - 24f, 94f);

                    string btnLabel;
                    Color btnColor;
                    if (isActive) { btnLabel = "Aktif Yasa"; btnColor = ColGreen; }
                    else if (currentLaw == CosmologyLaw.None) { btnLabel = "Yasayı İlan Et (Ücretsiz)"; btnColor = ColPurple; }
                    else { btnLabel = $"Büyük Retcon (-{BalanceCaps.RetconCashCost / 1000.0:F0}K, -{BalanceCaps.RetconIntegrityCost} Tut.)"; btnColor = ColRed; }

                    var lBtn = CreateButton(lCard.transform, btnLabel, btnColor, Color.white, 13, () =>
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
                                UniverseSaveManager.AdjustReputation(-5);
                                UniverseSaveManager.SetLaw(capLaw);
                                UniverseSaveManager.AddRivalNews(comp.currentWeek, comp.name ?? "Sen",
                                    "BÜYÜK RETCON! Evrenin kuralları baştan yazıldı!",
                                    $"Yeni Kozmoloji Yasası: {CategoryLabels.LawToTurkish(capLaw)}. Hayranlar şaşkın (-{BalanceCaps.RetconIntegrityCost} Tutarlılık).", "HABER");
                                _statusBannerMsg = $"Büyük Retcon tamamlandı! Yeni yasa: {CategoryLabels.LawToTurkish(capLaw)}";
                            }
                        }
                        ShowCosmologyView();
                    });
                    PlaceRect(lBtn.GetComponent<RectTransform>(), 12f, 136f, cardW - 24f, 38f);
                }

                // Alt Sol: Solo-Dev Tukenmislik (w=660) | Alt Sag: 10x Ar-Ge Turbo Anahtari (w=408)
                bool isSolo = SoloBurnoutSystem.IsSoloDev(comp);
                int burnout = u.SoloBurnout;
                string burnoutState = !isSolo
                    ? "Ekip Modu (Düşük Risk)"
                    : (burnout >= 70 ? "KRİTİK TÜKENMİŞLİK!" : (burnout >= 40 ? "Yorgunluk Birikiyor" : "Dinç & Odaklı"));

                var bCard = CreatePanel(_contentRoot, burnout >= 70 && isSolo ? new Color(0.96f, 0.84f, 0.82f, 1f) : ColCard);
                PlaceRect(bCard.GetComponent<RectTransform>(), 0f, 332f, 660f, 126f);

                var bTxt = CreateText(bCard.transform,
                    $"Solo-Dev Tükenmişlik: %{burnout} — {burnoutState}\nTek başına aralıksız üretim verimliliği düşürür. Mola vererek veya ekip kurarak sıfırla.",
                    14, ColDarkText, TextAnchor.MiddleLeft, false);
                var brt = bTxt.GetComponent<RectTransform>();
                brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
                brt.offsetMin = new Vector2(14f, 8f); brt.offsetMax = new Vector2(-190f, -8f);

                var restBtn = CreateButton(bCard.transform, "Çay Molası Ver\n(-%45 Yorgunluk)", ColGreen, Color.white, 14, () =>
                {
                    _statusBannerMsg = SoloBurnoutSystem.TakeSoloBreak(comp);
                    ShowCosmologyView();
                });
                var rrt = restBtn.GetComponent<RectTransform>();
                rrt.anchorMin = new Vector2(1f, 0.5f); rrt.anchorMax = new Vector2(1f, 0.5f);
                rrt.pivot = new Vector2(1f, 0.5f); rrt.anchoredPosition = new Vector2(-12f, 0f); rrt.sizeDelta = new Vector2(166f, 66f);

                // 10x Ar-Ge Turbo Anahtari Karti!
                bool turboOn = ModSettings.TestMode10xResearch;
                var tCard = CreatePanel(_contentRoot, turboOn ? ColHighlight : ColCard);
                PlaceRect(tCard.GetComponent<RectTransform>(), 676f, 332f, 408f, 126f);

                var tTxt = CreateText(tCard.transform,
                    $"⚡ 10x Ar-Ge Turbo Modu: {(turboOn ? "AÇIK (Test)" : "KAPALI (Dengeli)")}\nYouTube serisinde gerçekçi tempo için kapatabilir, test ederken tek tıkla açabilirsin.",
                    13, ColDarkText, TextAnchor.UpperLeft, true);
                PlaceRect(tTxt.GetComponent<RectTransform>(), 12f, 10f, 384f, 54f);

                var btnTurbo = CreateButton(tCard.transform, turboOn ? "Turbo'yu KAPAT (Normal Tempo)" : "⚡ Turbo'yu AÇ (10x Hız)", turboOn ? ColRed : ColGreen, Color.white, 15, () =>
                {
                    UniverseSaveManager.SetTurboEnabled(!ModSettings.TestMode10xResearch);
                    _statusBannerMsg = ModSettings.TestMode10xResearch
                        ? "⚡ 10x Ar-Ge Turbo AÇILDI!"
                        : "✓ 10x Ar-Ge Turbo KAPATILDI — Dengeli kariyer temposu aktif!";
                    ShowCosmologyView();
                });
                PlaceRect(btnTurbo.GetComponent<RectTransform>(), 12f, 70f, 384f, 44f);

                if (!string.IsNullOrEmpty(_statusBannerMsg))
                {
                    var msgTxt = CreateText(_contentRoot, _statusBannerMsg, 15, ColPurple, TextAnchor.MiddleLeft, true);
                    PlaceRect(msgTxt.GetComponent<RectTransform>(), 4f, 466f, 1076f, 30f);
                }

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] ShowCosmologyView hatasi: {ex}");
                CloseModal();
            }
        }

        // ---------------------------------------------------------------------
        // MOD ICERIGI SEKME 3 (FAZ 7.1): SIRKET DNA, DEPARTMANLAR, DIPLOMASI & HOLDING
        // ---------------------------------------------------------------------
        private static void ShowCompanyView()
        {
            try
            {
                ClearContent();
                var u = UniverseSaveManager.Current;
                var comp = UniverseArchitectMod.ActiveCompany;
                var curCulture = UniverseSaveManager.GetCulture();
                double cashK = comp != null ? comp.cash / 1000.0 : 0.0;

                _headerTitle.text = $"Mod İçeriği — Şirket, Diplomasi & Holding (Kasa: {cashK:F1}K | İtibar: {u.StudioReputation}/100 | 🏆 {u.TrophyCount})";
                DrawModContentTabs(3);

                // 4 Alt Sekme (4 * 265 + 3 * 8 = 1084px)
                int totalDeptLvls = u.DeptLoreLevel + u.DeptPrLevel + u.DeptMerchLevel;
                int indepCount = HoldingEndgameSystem.GetIndependentRivals().Count;
                int subCount = HoldingEndgameSystem.GetPlayerSubsidiaries().Count;
                string[] subLabels =
                {
                    $"★ Kültür & İtibar ({CategoryLabels.ReputationTier(u.StudioReputation)})",
                    $"Departmanlar ({totalDeptLvls}/9)",
                    $"Diplomasi ({indepCount} Rakip)",
                    $"🏛 Holding & Ödül ({subCount} Alt)"
                };
                Color[] subColors = { ColGreen, ColTeal, ColPurple, ColBlue };
                for (int s = 0; s < 4; s++)
                {
                    int capS = s;
                    bool isSel = (_companySubTab == s);
                    var sb = CreateButton(_contentRoot, subLabels[s], isSel ? subColors[s] : ColYellow, isSel ? Color.white : ColDarkText, 14, () =>
                    {
                        _companySubTab = capS; _statusBannerMsg = ""; ShowCompanyView();
                    });
                    PlaceRect(sb.GetComponent<RectTransform>(), s * 273f, 52f, 265f, 38f);
                }

                if (_companySubTab == 0)
                {
                    double estMerch = CompanyDiplomacySystem.EstimateMonthlyMerchPayout();
                    double weeklyUpkeep = CompanyDiplomacySystem.GetTotalWeeklyUpkeep();
                    string invBadge = u.HasInvestor ? $"Yatırımcı: {u.InvestorName} (%{u.InvestorEquityPct} pay)" : "★ %100 Bağımsız Stüdyo";

                    var repCard = CreatePanel(_contentRoot, ColHighlight);
                    PlaceRect(repCard.GetComponent<RectTransform>(), 0f, 98f, 1084f, 76f);
                    var repTxt = CreateText(repCard.transform,
                        $"Stüdyo İtibarı: {u.StudioReputation}/100 [{CategoryLabels.ReputationTier(u.StudioReputation)}]  |  {invBadge}  |  🏆 Kupa: {u.TrophyCount}\n" +
                        $"Aylık Merch: +{estMerch / 1000.0:F1}K  |  Haftalık Gider: -{weeklyUpkeep / 1000.0:F1}K  |  Toplam Merch: +{u.TotalMerchIncome / 1000.0:F1}K  |  Motor Telifi: +{u.TotalEngineLicenseIncome / 1000.0:F1}K",
                        14, ColDarkText, TextAnchor.MiddleLeft, true);
                    StretchWithPad(repTxt.GetComponent<RectTransform>(), 14f, 6f);

                    StudioCulture[] cultures = { StudioCulture.ArtisanIndie, StudioCulture.HypeMachine, StudioCulture.CanonGuardian, StudioCulture.CommunityFirst };
                    float cW = 534f, cH = 136f;
                    for (int i = 0; i < 4; i++)
                    {
                        StudioCulture capC = cultures[i];
                        bool isActive = (curCulture == capC);
                        int col = i % 2;
                        int row = i / 2;
                        float cx = col * (cW + 16f);
                        float cy = 184f + row * (cH + 12f);

                        var card = CreatePanel(_contentRoot, isActive ? ColHighlight : ColCard);
                        PlaceRect(card.GetComponent<RectTransform>(), cx, cy, cW, cH);

                        var titleTxt = CreateText(card.transform, CategoryLabels.CultureToTurkish(capC), 17, ColHeader, TextAnchor.UpperLeft, true);
                        PlaceRect(titleTxt.GetComponent<RectTransform>(), 12f, 8f, cW - 24f, 24f);

                        var descTxt = CreateText(card.transform, CategoryLabels.CultureShortDesc(capC), 13, ColDarkText, TextAnchor.UpperLeft, false);
                        PlaceRect(descTxt.GetComponent<RectTransform>(), 12f, 36f, cW - 24f, 52f);

                        string bLbl = isActive ? "✓ Aktif Kültür" : (curCulture == StudioCulture.None ? "Kültürü Seç (Ücretsiz)" : "Dönüştür (-35K, -4 İtibar)");
                        Color bCol = isActive ? ColGreen : (curCulture == StudioCulture.None ? ColPurple : ColBlue);
                        var btn = CreateButton(card.transform, bLbl, bCol, Color.white, 14, () =>
                        {
                            string msg;
                            CompanyDiplomacySystem.TrySetCulture(comp, capC, out msg);
                            _statusBannerMsg = msg;
                            ShowCompanyView();
                        });
                        PlaceRect(btn.GetComponent<RectTransform>(), 12f, 90f, cW - 24f, 36f);
                    }
                }
                else if (_companySubTab == 1)
                {
                    int cap = CompanyDiplomacySystem.GetUnlockedDeptCap(comp);
                    int staffCount = (comp != null && comp.staff != null) ? comp.staff.Count : 1;
                    double avgPop;
                    int starCount = CompanyDiplomacySystem.CountMerchEligibleStars(out avgPop);

                    var capCard = CreatePanel(_contentRoot, ColCard);
                    PlaceRect(capCard.GetComponent<RectTransform>(), 0f, 98f, 1084f, 52f);
                    var capTxt = CreateText(capCard.transform,
                        $"Şirket Büyüme Kapasitesi: Maks. Departman Sv.{cap}/3  (Çalışan: {staffCount} | Yıldız/Kült Karakter: {starCount} | Haftalık Gider: -{CompanyDiplomacySystem.GetTotalWeeklyUpkeep() / 1000.0:F1}K)",
                        15, ColDarkText, TextAnchor.MiddleLeft, true);
                    StretchWithPad(capTxt.GetComponent<RectTransform>(), 14f, 0f);

                    string[] names = { "Lore & Senaryo Odası", "PR & Kriz Masası", "Merch & Lisans Ofisi" };
                    int[] lvls = { u.DeptLoreLevel, u.DeptPrLevel, u.DeptMerchLevel };
                    double[][] costs = { CompanyDiplomacySystem.LoreCosts, CompanyDiplomacySystem.PrCosts, CompanyDiplomacySystem.MerchCosts };
                    string[] descs =
                    {
                        "• Karakter kimyasına pasif cila (+%2 / Sv.)\n• Ana Evren Tutarlılık kayıplarını azaltır (-%15 / Sv.)\n• Ana Evren Lore Bariyerini frenler (-2 / Sv.)\n• Haftalık gider: 1.2K × Seviye",
                        "• Hype cezalarını emer (-%20 / Sv.)\n• Kötü skor & krizlerde İtibar hasarını azaltır (-%18 / Sv.)\n• PR Düellosu ve karşı tanıtım Hype'ını güçlendirir (+2 / Sv.)\n• Haftalık gider: 1.0K × Seviye",
                        $"• Her 4 haftada bir {starCount} Yıldız/Kült karakterden pasif figür & cosplay telifi!\n• Tahmini Aylık Gelir: +{CompanyDiplomacySystem.EstimateMonthlyMerchPayout() / 1000.0:F1}K\n• Transmedya & Konuk DLC gelirine +%5 / Sv.\n• Haftalık gider: 0.8K × Seviye"
                    };

                    float dW = 352f;
                    for (int i = 0; i < 3; i++)
                    {
                        int capIdx = i;
                        int curL = lvls[i];
                        float dx = i * (dW + 14f);

                        var card = CreatePanel(_contentRoot, curL > 0 ? ColHighlight : ColCard);
                        PlaceRect(card.GetComponent<RectTransform>(), dx, 160f, dW, 304f);

                        var tTxt = CreateText(card.transform, $"{names[i]}\nSeviye: {curL} / 3", 17, ColHeader, TextAnchor.UpperLeft, true);
                        PlaceRect(tTxt.GetComponent<RectTransform>(), 12f, 10f, dW - 24f, 46f);

                        var dTxt = CreateText(card.transform, descs[i], 13, ColDarkText, TextAnchor.UpperLeft, false);
                        PlaceRect(dTxt.GetComponent<RectTransform>(), 12f, 62f, dW - 24f, 152f);

                        string bLabel;
                        Color bColor;
                        if (curL >= 3) { bLabel = "✓ Maksimum Seviye (Sv.3)"; bColor = ColGreen; }
                        else if (curL + 1 > cap) { bLabel = $"Kilitli (Sv.{curL + 1} Kapasite Gerek)"; bColor = ColTrack; }
                        else
                        {
                            double cost = costs[i][curL];
                            bLabel = curL == 0 ? $"Kur (-{cost / 1000.0:F0}K)" : $"Sv.{curL + 1} Yükselt (-{cost / 1000.0:F0}K)";
                            bColor = (comp != null && comp.cash >= cost) ? ColGreen : ColTrack;
                        }

                        var btn = CreateButton(card.transform, bLabel, bColor, Color.white, 15, () =>
                        {
                            string msg;
                            CompanyDiplomacySystem.TryUpgradeDepartment(comp, capIdx, out msg);
                            _statusBannerMsg = msg;
                            ShowCompanyView();
                        });
                        PlaceRect(btn.GetComponent<RectTransform>(), 12f, 248f, dW - 24f, 44f);
                    }
                }
                else if (_companySubTab == 2)
                {
                    // STUDYO DIPLOMASISI + MOTOR LISANSI + ORTAK YAPIM + SATIN ALMA!
                    string bestEng;
                    double bestTech;
                    int bestParts;
                    bool hasEng = CompanyDiplomacySystem.TryGetBestPlayerEngine(comp, out bestEng, out bestTech, out bestParts);

                    var topBar = CreatePanel(_contentRoot, string.IsNullOrEmpty(u.ActiveCoProdRivalId) ? ColCard : ColPurple);
                    PlaceRect(topBar.GetComponent<RectTransform>(), 0f, 98f, 1084f, 42f);
                    string topStr = !string.IsNullOrEmpty(u.ActiveCoProdRivalId)
                        ? $"★ AKTİF ORTAK YAPIM: {u.ActiveCoProdRivalName} ({u.ActiveCoProdUniverse}) — Sıradaki evren oyununda Mega Crossover patlayacak!"
                        : (hasEng
                            ? $"En İyi Motorun: '{bestEng}' (Tech {bestTech:F1})  |  Motor Lisansı: +25 İlişki  |  Ortak Yapım: +45 Müttefik"
                            : "İyi Niyetle Müttefik (+45) olup Ortak Yapım kurabilir veya kasayı doldurup rakibi Satın Alabilirsin!");
                    var topTxt = CreateText(topBar.transform, topStr, 14, string.IsNullOrEmpty(u.ActiveCoProdRivalId) ? ColDarkText : Color.white, TextAnchor.MiddleLeft, true);
                    StretchWithPad(topTxt.GetComponent<RectTransform>(), 14f, 0f);

                    float listH = string.IsNullOrEmpty(_statusBannerMsg) ? 350f : 322f;
                    var scroll = CreateScrollView(_contentRoot, 0f, 146f, 1084f, listH);
                    float innerW = 1068f;
                    float yCursor = 0f;

                    var indepRivals = HoldingEndgameSystem.GetIndependentRivals();
                    if (indepRivals.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColHighlight);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 100f);
                        var et = CreateText(empty.transform, "Sektörde bağımsız rakip kalmadı — bütün piyasayı tekeline aldın!", 18, ColDarkText, TextAnchor.MiddleCenter, true);
                        StretchWithPad(et.GetComponent<RectTransform>(), 16f, 8f);
                        yCursor = 108f;
                    }
                    else
                    {
                        for (int i = 0; i < indepRivals.Count; i++)
                        {
                            var r = indepRivals[i];
                            var st = CategoryLabels.RelationToStance(r.Relation);
                            Color rowBg = ColCard;
                            if (st == DiplomaticStance.Allied) rowBg = new Color(0.84f, 0.94f, 0.85f, 1f);
                            else if (st == DiplomaticStance.Friendly) rowBg = ColHighlight;
                            else if (st == DiplomaticStance.Hostile) rowBg = new Color(0.96f, 0.82f, 0.82f, 1f);
                            else if (st == DiplomaticStance.Tense) rowBg = new Color(0.96f, 0.89f, 0.84f, 1f);

                            var card = CreatePanel(scroll.content, rowBg);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 102f);

                            double buyPrice = HoldingEndgameSystem.CalculateAcquisitionPrice(r);
                            string licTxt = r.HasEngineLicense ? $" | ⚙ Lisans: {r.LicensedEngineName}" : "";
                            string discTxt = r.CanonIntegrity <= 65 ? " [İNDİRİM!]" : "";
                            var txt = CreateText(card.transform,
                                $"{r.Name} ({r.UniverseName}) — {CategoryLabels.StanceToTurkish(st)} ({r.Relation:+#;-#;0}){licTxt}\n" +
                                $"Hayran: {r.Fans:N0} | Ort: {r.AverageScore:F1} | Tut: {r.CanonIntegrity}/100 | Satın Alma: {buyPrice / 1000.0:F0}K{discTxt}",
                                14, ColDarkText, TextAnchor.MiddleLeft, false);
                            var trt = txt.GetComponent<RectTransform>();
                            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                            trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(-430f, -4f);

                            // Sagda 2 satirli kompakt diplomasi paneli (Ust: Iyi Niyet, PR, Motor | Alt: Ortak Yapim, Satin Al)
                            var btnGood = CreateButton(card.transform, "İyi Niyet (-8K)", ColGreen, Color.white, 12, () =>
                            {
                                string msg; CompanyDiplomacySystem.TryDiplomacyGoodwill(comp, r, out msg);
                                _statusBannerMsg = msg; ShowCompanyView();
                            });
                            PlaceRect(btnGood.GetComponent<RectTransform>(), innerW - 418f, 8f, 132f, 40f);

                            var btnDuel = CreateButton(card.transform, "PR Düello (-5K)", ColRed, Color.white, 12, () =>
                            {
                                string msg; CompanyDiplomacySystem.TryDiplomacyPrDuel(comp, r, out msg);
                                _statusBannerMsg = msg; ShowCompanyView();
                            });
                            PlaceRect(btnDuel.GetComponent<RectTransform>(), innerW - 280f, 8f, 132f, 40f);

                            bool canLic = !r.HasEngineLicense && r.Relation >= 25 && hasEng;
                            var btnLic = CreateButton(card.transform, r.HasEngineLicense ? "⚙ Lisans Aktif" : "Motor Lisansla", r.HasEngineLicense ? ColTeal : (canLic ? ColBlue : ColTrack), Color.white, 12, () =>
                            {
                                string msg; CompanyDiplomacySystem.TryLicenseEngineToRival(comp, r, out msg);
                                _statusBannerMsg = msg; ShowCompanyView();
                            });
                            PlaceRect(btnLic.GetComponent<RectTransform>(), innerW - 142f, 8f, 132f, 40f);

                            bool canCoProd = r.Relation >= 45 && string.IsNullOrEmpty(u.ActiveCoProdRivalId);
                            var btnCoProd = CreateButton(card.transform, "★ Ortak Yapım (-25K)", canCoProd ? ColPurple : ColTrack, Color.white, 12, () =>
                            {
                                string msg; HoldingEndgameSystem.TryStartCoProduction(comp, r, out msg);
                                _statusBannerMsg = msg; ShowCompanyView();
                            });
                            PlaceRect(btnCoProd.GetComponent<RectTransform>(), innerW - 418f, 54f, 200f, 40f);

                            bool canBuy = comp != null && comp.cash >= buyPrice;
                            var btnBuy = CreateButton(card.transform, $"🏛 Satın Al (-{buyPrice / 1000.0:F0}K)", canBuy ? ColHeader : ColTrack, Color.white, 12, () =>
                            {
                                string msg; HoldingEndgameSystem.TryAcquireRival(comp, r, out msg);
                                _statusBannerMsg = msg; ShowCompanyView();
                            });
                            PlaceRect(btnBuy.GetComponent<RectTransform>(), innerW - 210f, 54f, 200f, 40f);

                            yCursor += 110f;
                        }
                    }
                    scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(listH - 16f, yCursor));
                }
                else
                {
                    // ALT SEKME 3: HOLDING, YATIRIMCI, STUDIO DIRECT, YAYINCILIK & ODULLER
                    var topActions = CreatePanel(_contentRoot, ColHighlight);
                    PlaceRect(topActions.GetComponent<RectTransform>(), 0f, 98f, 1084f, 104f);

                    // Sol yari: Yatirimci Durumu / Fon Cek / Hisse Geri Al
                    string invLine = u.HasInvestor
                        ? $"Yatırımcı: {u.InvestorName} (%{u.InvestorEquityPct} Pay | Hedef: {u.InvestorTargetScore:F1}+ | Ödenen: {u.InvestorTotalPaid / 1000.0:F1}K)"
                        : "★ %100 Bağımsız Stüdyo — Büyük bütçe gerekirse aşağıdan Yatırımcı Fonu çekebilirsin:";
                    var invTxt = CreateText(topActions.transform, invLine, 14, ColDarkText, TextAnchor.UpperLeft, true);
                    PlaceRect(invTxt.GetComponent<RectTransform>(), 12f, 8f, 650f, 34f);

                    if (u.HasInvestor)
                    {
                        bool canBuyout = comp != null && comp.cash >= u.InvestorBuyoutCost;
                        var btnBuyout = CreateButton(topActions.transform, $"★ Hisseleri Geri Al & Bağımsız Ol (-{u.InvestorBuyoutCost / 1000.0:F0}K)", canBuyout ? ColGreen : ColTrack, Color.white, 14, () =>
                        {
                            string msg; HoldingEndgameSystem.TryBuyoutInvestor(comp, out msg);
                            _statusBannerMsg = msg; ShowCompanyView();
                        });
                        PlaceRect(btnBuyout.GetComponent<RectTransform>(), 12f, 48f, 640f, 44f);
                    }
                    else
                    {
                        var bInv0 = CreateButton(topActions.transform, "Melek +120K\n(%15, 7.2+)", ColBlue, Color.white, 12, () =>
                        {
                            string msg; HoldingEndgameSystem.TryTakeInvestorFunding(comp, 0, out msg);
                            _statusBannerMsg = msg; ShowCompanyView();
                        });
                        PlaceRect(bInv0.GetComponent<RectTransform>(), 12f, 48f, 206f, 46f);

                        var bInv1 = CreateButton(topActions.transform, "Risk VC +300K\n(%20, 7.8+)", ColPurple, Color.white, 12, () =>
                        {
                            string msg; HoldingEndgameSystem.TryTakeInvestorFunding(comp, 1, out msg);
                            _statusBannerMsg = msg; ShowCompanyView();
                        });
                        PlaceRect(bInv1.GetComponent<RectTransform>(), 226f, 48f, 206f, 46f);

                        var bInv2 = CreateButton(topActions.transform, "Mega Fon +650K\n(%25, 8.2+)", ColRed, Color.white, 12, () =>
                        {
                            string msg; HoldingEndgameSystem.TryTakeInvestorFunding(comp, 2, out msg);
                            _statusBannerMsg = msg; ShowCompanyView();
                        });
                        PlaceRect(bInv2.GetComponent<RectTransform>(), 440f, 48f, 206f, 46f);
                    }

                    // Sag yari: Studio Direct & Yayincilik Teklifi
                    var btnDirect = CreateButton(topActions.transform, "📺 Studio Direct (-20K)\nSıradakine +22 Hype!", ColPurple, Color.white, 13, () =>
                    {
                        string msg; HoldingEndgameSystem.TryHostStudioDirect(comp, out msg);
                        _statusBannerMsg = msg; ShowCompanyView();
                    });
                    PlaceRect(btnDirect.GetComponent<RectTransform>(), 666f, 10f, 200f, 84f);

                    var btnPub = CreateButton(topActions.transform, $"📦 Yayıncılık ({HoldingEndgameSystem.GetActivePublishDealCount()}/2)\nTeklif İste", ColTeal, Color.white, 13, () =>
                    {
                        string msg; HoldingEndgameSystem.TryRequestPublishOffer(comp, out msg);
                        _statusBannerMsg = msg; ShowCompanyView();
                    });
                    PlaceRect(btnPub.GetComponent<RectTransform>(), 874f, 10f, 198f, 84f);

                    // Asagi kaydirilabilir alan: Alt Studyolar + Yayincilik + Multiverse Awards Gecmisi
                    float listH = string.IsNullOrEmpty(_statusBannerMsg) ? 286f : 258f;
                    var scroll = CreateScrollView(_contentRoot, 0f, 210f, 1084f, listH);
                    float innerW = 1068f;
                    float yCursor = 0f;
                    double nowW = comp != null ? comp.currentWeek : 0.0;

                    // 1) Alt Studyolar
                    var subs = HoldingEndgameSystem.GetPlayerSubsidiaries();
                    for (int i = 0; i < subs.Count; i++)
                    {
                        var sub = subs[i];
                        var dir = CategoryLabels.ParseDirective(sub.SubsidiaryDirectiveName);
                        var row = CreatePanel(scroll.content, new Color(0.84f, 0.94f, 0.86f, 1f));
                        PlaceRect(row.GetComponent<RectTransform>(), 0f, yCursor, innerW, 66f);

                        var txt = CreateText(row.transform,
                            $"[🏛 ALT STÜDYO]  {sub.Name} ({sub.UniverseName})  |  Talimat: {CategoryLabels.DirectiveToTurkish(dir)}\n" +
                            $"Sonraki Rapor: ~{Math.Max(0.1, sub.NextSubsidiaryTickWeek - nowW):F1} hafta  |  Hayran: {sub.Fans:N0}  |  Son İş: {sub.LastGameTitle}",
                            14, ColDarkText, TextAnchor.MiddleLeft, false);
                        var trt = txt.GetComponent<RectTransform>();
                        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                        trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(-215f, -4f);

                        var dirBtn = CreateButton(row.transform, "Talimat Değiştir >", ColBlue, Color.white, 14, () =>
                        {
                            _statusBannerMsg = HoldingEndgameSystem.CycleSubsidiaryDirective(sub);
                            ShowCompanyView();
                        });
                        var drt = dirBtn.GetComponent<RectTransform>();
                        drt.anchorMin = new Vector2(1f, 0.5f); drt.anchorMax = new Vector2(1f, 0.5f);
                        drt.pivot = new Vector2(1f, 0.5f); drt.anchoredPosition = new Vector2(-12f, 0f); drt.sizeDelta = new Vector2(190f, 44f);
                        yCursor += 74f;
                    }

                    // 2) Yayincilik Anlasmalari
                    if (u.PublishingDeals != null)
                    {
                        for (int i = u.PublishingDeals.Count - 1; i >= 0; i--)
                        {
                            var d = u.PublishingDeals[i];
                            var row = CreatePanel(scroll.content, d.IsResolved ? ColCard : ColHighlight);
                            PlaceRect(row.GetComponent<RectTransform>(), 0f, yCursor, innerW, 62f);
                            string subLine = d.IsResolved
                                ? d.ResultSummary
                                : $"GELİŞTİRİLİYOR — Kalan: ~{Math.Max(0.1, d.ReleaseWeek - nowW):F1} hafta | Yatırım: {d.Investment / 1000.0:F0}K";
                            var txt = CreateText(row.transform, $"[📦 YAYINCILIK]  {d.GameTitle} ({d.RivalName})\n{subLine}", 14, ColDarkText, TextAnchor.MiddleLeft, false);
                            StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 4f);
                            yCursor += 70f;
                        }
                    }

                    // 3) Yillik Multiverse Awards Muzesi
                    if (u.AwardsHistory != null)
                    {
                        for (int i = u.AwardsHistory.Count - 1; i >= 0; i--)
                        {
                            var aw = u.AwardsHistory[i];
                            var row = CreatePanel(scroll.content, (aw.PlayerWonUniverse || aw.PlayerWonCharacter) ? ColHighlight : ColCard);
                            PlaceRect(row.GetComponent<RectTransform>(), 0f, yCursor, innerW, 64f);
                            var txt = CreateText(row.transform,
                                $"[🏆 MULTIVERSE AWARDS — YIL {aw.Year}]  Evren: {aw.UniverseWinner}  |  Karakter: {aw.CharacterWinner}\n{aw.Summary}",
                                13, ColDarkText, TextAnchor.MiddleLeft, false);
                            StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 4f);
                            yCursor += 72f;
                        }
                    }

                    if (yCursor == 0f)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 100f);
                        var et = CreateText(empty.transform,
                            "Holding defterin henüz temiz!\nDiplomasi sekmesinden rakip satın alıp Alt Stüdyo kurabilir, yukarıdan Yayıncılık veya Studio Direct başlatabilirsin. Her 48 haftada bir Multiverse Awards burada birikir!",
                            15, ColMutedText, TextAnchor.MiddleCenter, false);
                        StretchWithPad(et.GetComponent<RectTransform>(), 16f, 8f);
                        yCursor = 108f;
                    }
                    scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(listH - 16f, yCursor));
                }

                if (!string.IsNullOrEmpty(_statusBannerMsg))
                {
                    var msgTxt = CreateText(_contentRoot, _statusBannerMsg, 15, ColPurple, TextAnchor.MiddleLeft, true);
                    PlaceRect(msgTxt.GetComponent<RectTransform>(), 4f, 472f, 1076f, 28f);
                }

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] ShowCompanyView hatasi: {ex}");
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
                DrawModContentTabs(4);

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
                    string fanBadge = !string.IsNullOrEmpty(e.FandomName) ? $"  ♥ {e.FandomName}" : "";
                    string line =
                        $"{rankBadge}   {e.StudioName}  —  {e.UniverseName}{fanBadge}  [{e.StyleOrLaw}]  |  {e.DiplomacyOrRep}\n" +
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
                MelonLogger.Error($"[Faz 10.0] ShowLeaderboardView hatasi: {ex}");
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
                int chCount = u.Chronology != null ? u.Chronology.Count : 0;
                _headerTitle.text = _newsSubTab == 0
                    ? $"Mod İçeriği — Rakip Haberleri & Sektör Bülteni ({u.RivalNews.Count})"
                    : $"Mod İçeriği — Evren Kronolojisi & Tarih Ansiklopedisi ({chCount})";

                DrawModContentTabs(5);

                // 2 Alt Sekme: Sektor Haberleri | Evren Kronolojisi
                var subN0 = CreateButton(_contentRoot, $"📰 Sektör Haberleri ({u.RivalNews.Count})", _newsSubTab == 0 ? ColBlue : ColYellow, _newsSubTab == 0 ? Color.white : ColDarkText, 15, () =>
                {
                    _newsSubTab = 0; ShowRivalNewsView();
                });
                PlaceRect(subN0.GetComponent<RectTransform>(), 0f, 52f, 536f, 38f);

                var subN1 = CreateButton(_contentRoot, $"📜 Evren Kronolojisi — Tarih Şeridi ({chCount})", _newsSubTab == 1 ? ColPurple : ColYellow, _newsSubTab == 1 ? Color.white : ColDarkText, 15, () =>
                {
                    _newsSubTab = 1; ShowRivalNewsView();
                });
                PlaceRect(subN1.GetComponent<RectTransform>(), 548f, 52f, 536f, 38f);

                var scroll = CreateScrollView(_contentRoot, 0f, 98f, 1084f, 398f);
                float innerW = 1068f;
                float yCursor = 0f;

                if (_newsSubTab == 0)
                {
                    if (u.RivalNews.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 110f);
                        var t = CreateText(empty.transform,
                            "Sektör şimdilik sakin. Haftalar ilerledikçe 24 rakip stüdyonun oyunları, hitleri ve rekabet hamleleri burada akacak.",
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
                            else if (n.Tag == "DIPLOMASI" || n.Tag == "SIRKET" || n.Tag == "HOLDING") cardBg = new Color(0.84f, 0.93f, 0.92f, 1f);
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
                }
                else
                {
                    if (u.Chronology == null || u.Chronology.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 110f);
                        var t = CreateText(empty.transform,
                            "Tarih defteri henüz açıldı. Evren kurulumları, başyapıtlar, Saga finalleri, ihanetler, şirket satın almaları ve ödüller burada kronolojik birikecek!",
                            16, ColMutedText, TextAnchor.MiddleCenter, false);
                        StretchWithPad(t.GetComponent<RectTransform>(), 18f, 8f);
                        yCursor = 118f;
                    }
                    else
                    {
                        for (int i = u.Chronology.Count - 1; i >= 0; i--)
                        {
                            var ev = u.Chronology[i];
                            Color bg = ColCard;
                            if (ev.Category == "SAGA" || ev.Category == "KRIZ" || ev.Category == "ODUL") bg = ColHighlight;
                            else if (ev.Category == "HOLDING" || ev.Category == "TRANSFER") bg = new Color(0.84f, 0.94f, 0.86f, 1f);
                            else if (ev.Category == "SAVAS" || ev.Category == "KADER") bg = new Color(0.96f, 0.85f, 0.86f, 1f);

                            var card = CreatePanel(scroll.content, bg);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, yCursor, innerW, 68f);

                            var txt = CreateText(card.transform,
                                $"[YIL {ev.Year} — Hf.{ev.Week:F0} | {ev.Category}]  {ev.Title}\n{ev.Detail}",
                                14, ColDarkText, TextAnchor.MiddleLeft, false);
                            StretchWithPad(txt.GetComponent<RectTransform>(), 14f, 4f);
                            yCursor += 76f;
                        }
                    }
                }
                scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(382f, yCursor));

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] ShowRivalNewsView hatasi: {ex}");
                CloseModal();
            }
        }


        // ---------------------------------------------------------------------
        // GORUNUM 8 (FAZ 10.0): 𝕏 TIVITIR — CANLI AKIS, SERBEST TIVIT, DM, YAN HESAP & TURNUVA
        // ---------------------------------------------------------------------
        private static void ShowTivitirView()
        {
            try
            {
                ClearContent();
                var comp = UniverseArchitectMod.ActiveCompany;
                var s = SocialSaveManager.Current;
                int dmUnread = TivitirSystem.GetUnreadDmCount();

                _headerTitle.text = $"𝕏 Tivitır — 25 Evrenin Canlı Akışı ({s.Tweets.Count}/64 Tivit | {dmUnread} Yeni DM | {s.Promises.Count}/8 Söz)";
                DrawModContentTabs(6);

                var trends = TivitirSystem.ComputeTopTrends(comp);
                var ttCard = CreatePanel(_contentRoot, s.BoycottActive ? ColRed : new Color(0.13f, 0.19f, 0.28f, 1f));
                PlaceRect(ttCard.GetComponent<RectTransform>(), 0f, 50f, 1084f, 34f);
                var ttParts = new List<string>();
                for (int i = 0; i < trends.Count; i++)
                    ttParts.Add($"{i + 1}.{trends[i].Tag}{(trends[i].IsPlayerOwned ? "★" : "")}");
                string ttStr = s.BoycottActive
                    ? $"🔥 AKTİF #BOYKOT KRİZİ: {s.BoycottReason}  |  Gündem: {string.Join("  ", ttParts)}"
                    : $"🔥 Canlı Gündem (TT):  {(ttParts.Count > 0 ? string.Join("   •   ", ttParts) : "Sektör ısınıyor...")}  {(trends.Count > 0 && trends[0].IsPlayerOwned ? " [★ TT #1 BONUSU AKTİF: +%20 Merch/TM!]" : "")}";
                var ttTxt = CreateText(ttCard.transform, ttStr, 13, Color.white, TextAnchor.MiddleLeft, true);
                StretchWithPad(ttTxt.GetComponent<RectTransform>(), 12f, 0f);

                string[] subLabels =
                {
                    $"🏠 Akış ({s.Tweets.Count}/64)",
                    $"📩 DM ({dmUnread} Yeni / {s.Dms.Count})",
                    "🎯 Aksiyon & Yan Hesap",
                    $"🏆 Turnuva & Sözler ({s.Promises.Count}/8)"
                };
                for (int st = 0; st < 4; st++)
                {
                    int capSt = st;
                    bool sel = (_tivitSubTab == st);
                    Color bg = sel ? ColGreen : (st == 1 && dmUnread > 0 ? ColRed : ColYellow);
                    Color fg = sel || (st == 1 && dmUnread > 0) ? Color.white : ColDarkText;
                    var sb = CreateButton(_contentRoot, subLabels[st], bg, fg, 13, () =>
                    {
                        _tivitSubTab = capSt; _statusBannerMsg = ""; ShowTivitirView();
                    });
                    PlaceRect(sb.GetComponent<RectTransform>(), st * 273f, 90f, 265f, 36f);
                }

                float bodyTop = 132f;
                float bodyH = string.IsNullOrEmpty(_statusBannerMsg) ? 366f : 338f;

                if (_tivitSubTab == 0)
                {
                    _activeTivitInput = null;
                    _fallbackTivitText = null;
                    _activeTivitCountText = null;

                    _activeTivitInput = CreateInputFieldSafe(_contentRoot, _tivitDraftText,
                        "280 karaktere kadar uzun tivit yaz... (rakip, karakter, #etiket veya söz geçir — @Entakos, Kuriow, 9+ gibi!)",
                        val =>
                        {
                            _tivitDraftText = val ?? "";
                            if (_activeTivitCountText != null)
                                _activeTivitCountText.text = $"{_tivitDraftText.Length}/280";
                        },
                        out _fallbackTivitText, 280, true);
                    if (_activeTivitInput != null)
                        PlaceRect(_activeTivitInput.GetComponent<RectTransform>(), 0f, bodyTop, 742f, 58f);
                    else if (_fallbackTivitText != null)
                        PlaceRect(_fallbackTivitText.transform.parent.GetComponent<RectTransform>(), 0f, bodyTop, 742f, 58f);

                    var btnIdea = CreateButton(_contentRoot, "💡 Fikir", ColYellow, ColDarkText, 13, () =>
                    {
                        _tivitDraftText = TivitirSystem.GenerateTweetIdea(comp);
                        if (_activeTivitInput != null) _activeTivitInput.text = _tivitDraftText;
                        if (_fallbackTivitText != null) _fallbackTivitText.text = _tivitDraftText;
                        if (_activeTivitCountText != null) _activeTivitCountText.text = $"{_tivitDraftText.Length}/280";
                    });
                    PlaceRect(btnIdea.GetComponent<RectTransform>(), 750f, bodyTop, 84f, 36f);

                    _activeTivitCountText = CreateText(_contentRoot, $"{(_tivitDraftText ?? "").Length}/280", 12, ColMutedText, TextAnchor.MiddleCenter, true);
                    PlaceRect(_activeTivitCountText.GetComponent<RectTransform>(), 750f, bodyTop + 38f, 84f, 20f);

                    var btnSend = CreateButton(_contentRoot, "𝕏 Tivit At!", ColBlue, Color.white, 15, () =>
                    {
                        string txt = _activeTivitInput != null ? _activeTivitInput.text : _tivitDraftText;
                        _statusBannerMsg = TivitirSystem.PostPlayerFreeTweet(comp, txt);
                        _tivitDraftText = "";
                        ShowTivitirView();
                    });
                    PlaceRect(btnSend.GetComponent<RectTransform>(), 842f, bodyTop, 130f, 58f);

                    string[] fNames = { "Tümü", "Biz", "Rakipler", "Fenomen" };
                    var btnFilt = CreateButton(_contentRoot, $"Filtre:\n{fNames[_tivitFeedFilter]}", ColPurple, Color.white, 12, () =>
                    {
                        _tivitFeedFilter = (_tivitFeedFilter + 1) % 4;
                        ShowTivitirView();
                    });
                    PlaceRect(btnFilt.GetComponent<RectTransform>(), 980f, bodyTop, 104f, 58f);

                    var scroll = CreateScrollView(_contentRoot, 0f, bodyTop + 64f, 1084f, bodyH - 64f);
                    float innerW = 1068f;
                    float y = 0f;

                    for (int i = s.Tweets.Count - 1; i >= 0; i--)
                    {
                        var p = s.Tweets[i];
                        if (p.Del) continue;
                        if (_tivitFeedFilter == 1 && p.Tag != "PLAYER" && p.Tag != "FANWORK" && p.Tag != "POLL") continue;
                        if (_tivitFeedFilter == 2 && p.Tag != "RIVAL" && p.Tag != "FANDOM") continue;
                        if (_tivitFeedFilter == 3 && p.Tag != "INFLUENCER" && p.Tag != "TOURNEY") continue;

                        int replyCount = p.Replies != null ? Math.Min(p.Replies.Count, 3) : 0;
                        bool hasActionRow = (p.Tag == "PLAYER") || (p.CanRatio && !p.RatioDone) || (p.Tag == "FANWORK" && !p.FanActDone);
                        int bodyExtraLines = Math.Max(0, ((p.X != null ? p.X.Length : 0) - 92) / 92);
                        float cardH = 74f + bodyExtraLines * 19f + replyCount * 26f + (hasActionRow ? 34f : 0f);

                        Color bg = ColCard;
                        if (p.Tag == "PLAYER" || p.Tag == "POLL") bg = ColHighlight;
                        else if (p.Tag == "INFLUENCER") bg = new Color(0.86f, 0.92f, 0.98f, 1f);
                        else if (p.Tag == "FANWORK") bg = new Color(0.87f, 0.96f, 0.89f, 1f);
                        else if (p.Tag == "BURNER") bg = new Color(0.92f, 0.87f, 0.96f, 1f);

                        var card = CreatePanel(scroll.content, bg);
                        PlaceRect(card.GetComponent<RectTransform>(), 0f, y, innerW, cardH);

                        var sbBody = new StringBuilder();
                        sbBody.Append($"{p.D} {(p.V ? "✓ " : "")}({p.H})  •  Hf.{p.W:F0}  •  {p.Hash}   |   ❤️ {p.L:N0}   🔁 {p.RT:N0}\n");
                        sbBody.Append(p.X);
                        for (int r = 0; r < replyCount; r++)
                        {
                            var rp = p.Replies[r];
                            sbBody.Append($"\n   ↳ {rp.D} ({rp.H}): {rp.X}  [❤️ {rp.L:N0}]");
                        }

                        var txt = CreateText(card.transform, sbBody.ToString(), 13, ColDarkText, TextAnchor.UpperLeft, false);
                        var trt = txt.GetComponent<RectTransform>();
                        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                        trt.offsetMin = new Vector2(12f, hasActionRow ? 36f : 6f);
                        trt.offsetMax = new Vector2(-12f, -6f);

                        if (hasActionRow)
                        {
                            float btnY = cardH - 34f;
                            if (p.Tag == "PLAYER")
                            {
                                var btnDel = CreateButton(card.transform, "🗑 Tiviti Sil (SS Riski)", ColRed, Color.white, 11, () =>
                                {
                                    _statusBannerMsg = TivitirSystem.DeletePlayerTweet(comp, p);
                                    ShowTivitirView();
                                });
                                PlaceRect(btnDel.GetComponent<RectTransform>(), 12f, btnY, 175f, 28f);
                            }
                            else if (p.CanRatio && !p.RatioDone)
                            {
                                var btnRat = CreateButton(card.transform, "⚔ Ratio At! (Skor Kumarı)", ColPurple, Color.white, 12, () =>
                                {
                                    _statusBannerMsg = TivitirSystem.TryRatioRivalPost(comp, p);
                                    ShowTivitirView();
                                });
                                PlaceRect(btnRat.GetComponent<RectTransform>(), 12f, btnY, 210f, 28f);
                            }
                            else if (p.Tag == "FANWORK" && !p.FanActDone)
                            {
                                var bRt = CreateButton(card.transform, "❤️ RT (+3 Hype)", ColGreen, Color.white, 11, () =>
                                {
                                    _statusBannerMsg = TivitirSystem.HandleFanWorkPost(comp, p, 0); ShowTivitirView();
                                });
                                PlaceRect(bRt.GetComponent<RectTransform>(), 12f, btnY, 150f, 28f);

                                var bCan = CreateButton(card.transform, "⭐ Kanona Al (-5K)", ColBlue, Color.white, 11, () =>
                                {
                                    _statusBannerMsg = TivitirSystem.HandleFanWorkPost(comp, p, 1); ShowTivitirView();
                                });
                                PlaceRect(bCan.GetComponent<RectTransform>(), 170f, btnY, 165f, 28f);

                                var bDmca = CreateButton(card.transform, "⚖️ Telif At (+5K / Risk)", ColRed, Color.white, 11, () =>
                                {
                                    _statusBannerMsg = TivitirSystem.HandleFanWorkPost(comp, p, 2); ShowTivitirView();
                                });
                                PlaceRect(bDmca.GetComponent<RectTransform>(), 343f, btnY, 185f, 28f);
                            }
                        }

                        y += cardH + 8f;
                    }
                    scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(bodyH - 60f, y));
                }
                else if (_tivitSubTab == 1)
                {
                    var scroll = CreateScrollView(_contentRoot, 0f, bodyTop, 1084f, bodyH);
                    float innerW = 1068f;
                    float y = 0f;

                    if (s.Dms.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, 0f, innerW, 100f);
                        var et = CreateText(empty.transform, "DM kutusu şimdilik boş! Haftalar ilerledikçe rakip CEO'lar, @Fenoctakos, köstebekler ve süper hayranlar sana gizli mesaj atacak.", 16, ColMutedText, TextAnchor.MiddleCenter, false);
                        StretchWithPad(et.GetComponent<RectTransform>(), 16f, 8f);
                        y = 108f;
                    }
                    else
                    {
                        for (int i = s.Dms.Count - 1; i >= 0; i--)
                        {
                            var dm = s.Dms[i];
                            var card = CreatePanel(scroll.content, dm.Done ? ColCard : ColHighlight);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, y, innerW, dm.Done ? 72f : 96f);

                            string body = dm.Done
                                ? $"[YANITLANDI] {dm.D} ({dm.H}) — Hf.{dm.W:F0}\n{dm.Note}"
                                : $"📩 {dm.D} ({dm.H}) — Hf.{dm.W:F0}\n\"{dm.Body}\"";
                            var txt = CreateText(card.transform, body, 13, ColDarkText, TextAnchor.MiddleLeft, !dm.Done);
                            var trt = txt.GetComponent<RectTransform>();
                            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                            trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(dm.Done ? -14f : -365f, -4f);

                            if (!dm.Done)
                            {
                                var b0 = CreateButton(card.transform, dm.Opt0 ?? "Kabul Et", ColGreen, Color.white, 12, () =>
                                {
                                    _statusBannerMsg = TivitirSystem.AnswerDm(comp, dm, 0); ShowTivitirView();
                                });
                                var r0 = b0.GetComponent<RectTransform>();
                                r0.anchorMin = new Vector2(1f, 0.5f); r0.anchorMax = new Vector2(1f, 0.5f);
                                r0.pivot = new Vector2(1f, 0.5f); r0.anchoredPosition = new Vector2(-186f, 0f); r0.sizeDelta = new Vector2(166f, 52f);

                                var b1 = CreateButton(card.transform, dm.Opt1 ?? "Reddet", ColPurple, Color.white, 12, () =>
                                {
                                    _statusBannerMsg = TivitirSystem.AnswerDm(comp, dm, 1); ShowTivitirView();
                                });
                                var r1 = b1.GetComponent<RectTransform>();
                                r1.anchorMin = new Vector2(1f, 0.5f); r1.anchorMax = new Vector2(1f, 0.5f);
                                r1.pivot = new Vector2(1f, 0.5f); r1.anchoredPosition = new Vector2(-12f, 0f); r1.sizeDelta = new Vector2(166f, 52f);
                            }
                            y += (dm.Done ? 80f : 104f);
                        }
                    }
                    scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(bodyH - 16f, y));
                }
                else if (_tivitSubTab == 2)
                {
                    var scroll = CreateScrollView(_contentRoot, 0f, bodyTop, 1084f, bodyH);
                    float innerW = 1068f;
                    float y = 0f;

                    if (s.BoycottActive)
                    {
                        var bCard = CreatePanel(scroll.content, new Color(0.97f, 0.84f, 0.84f, 1f));
                        PlaceRect(bCard.GetComponent<RectTransform>(), 0f, y, innerW, 92f);
                        var bt = CreateText(bCard.transform, $"🔥 ACİL #BOYKOT KRİZİ: {s.BoycottReason}\nTivitır'da linç büyüyor — nasıl söndüreceksin?", 14, ColRed, TextAnchor.UpperLeft, true);
                        PlaceRect(bt.GetComponent<RectTransform>(), 14f, 6f, innerW - 28f, 44f);

                        var bFix = CreateButton(bCard.transform, "Ücretsiz Yama (-12K)", ColGreen, Color.white, 12, () => { _statusBannerMsg = TivitirSystem.ResolveBoycott(comp, 0); ShowTivitirView(); });
                        PlaceRect(bFix.GetComponent<RectTransform>(), 14f, 52f, 230f, 32f);

                        var bYel = CreateButton(bCard.transform, "🟨 'Sizi Duyuyoruz' Özür (Riskli)", ColYellow, ColDarkText, 12, () => { _statusBannerMsg = TivitirSystem.ResolveBoycott(comp, 1); ShowTivitirView(); });
                        PlaceRect(bYel.GetComponent<RectTransform>(), 254f, 52f, 290f, 32f);

                        var bSil = CreateButton(bCard.transform, "Sessiz Kal (-8 Hype)", ColRed, Color.white, 12, () => { _statusBannerMsg = TivitirSystem.ResolveBoycott(comp, 2); ShowTivitirView(); });
                        PlaceRect(bSil.GetComponent<RectTransform>(), 554f, 52f, 210f, 32f);
                        y += 100f;
                    }

                    Action<string, string, Color, Action> addActCard = (title, btnTxt, btnCol, onClick) =>
                    {
                        var c = CreatePanel(scroll.content, ColCard);
                        PlaceRect(c.GetComponent<RectTransform>(), 0f, y, innerW, 68f);
                        var tx = CreateText(c.transform, title, 14, ColDarkText, TextAnchor.MiddleLeft, false);
                        var trt = tx.GetComponent<RectTransform>();
                        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                        trt.offsetMin = new Vector2(14f, 4f); trt.offsetMax = new Vector2(-245f, -4f);

                        var b = CreateButton(c.transform, btnTxt, btnCol, Color.white, 13, () => { onClick(); ShowTivitirView(); });
                        var brt = b.GetComponent<RectTransform>();
                        brt.anchorMin = new Vector2(1f, 0.5f); brt.anchorMax = new Vector2(1f, 0.5f);
                        brt.pivot = new Vector2(1f, 0.5f); brt.anchoredPosition = new Vector2(-12f, 0f); brt.sizeDelta = new Vector2(220f, 46f);
                        y += 76f;
                    };

                    addActCard("🎭 Karakter RP Günü (Ücretsiz — 10 hf bekleme)\nEn popüler karakterin 1 günlüğüne Tivitır hesabını ele geçirir (+5 Popülerlik, +7 Hype).",
                        "🎭 RP Günü Başlat", ColPurple, () => _statusBannerMsg = TivitirSystem.TriggerCharacterRpDay(comp));

                    addActCard($"🎮 @YayınKaos Erken Erişim Sponsorluğu (-15K)  {(s.SponsorKaosActive ? "[AKTİF!]" : "")}\nSıradaki oyun 7.5+ alırsa canlı yayında dev hayran patlaması; 6.0 altında bug şovu riski!",
                        s.SponsorKaosActive ? "Sponsorlu ✓" : "Sponsor Ol (-15K)", s.SponsorKaosActive ? ColTrack : ColBlue, () => _statusBannerMsg = TivitirSystem.SponsorYayinKaos(comp));

                    addActCard("🗳 Topluluk Anketi Aç (Ücretsiz — 16 hf bekleme)\nHayranlara sıradaki oyunda ne istediklerini sor (+5 Hype). Sözünü tutarsan +%3 Skor & +10 Hype!",
                        "🗳 Anket Başlat", ColTeal, () => _statusBannerMsg = TivitirSystem.StartCommunityPoll(comp));

                    addActCard("🎙 Canlı Soru–Cevap / AMA Yayını (Ücretsiz — 36 hf bekleme)\nHayran, @PuanAvcısı ve @Fenoctakos'tan art arda 3 canlı soru gelir!",
                        "🎙 Canlı AMA Aç", ColGreen, () => _statusBannerMsg = TivitirSystem.StartLiveAma(comp));

                    var burnCard = CreatePanel(scroll.content, new Color(0.92f, 0.86f, 0.95f, 1f));
                    PlaceRect(burnCard.GetComponent<RectTransform>(), 0f, y, innerW, 82f);
                    var btx = CreateText(burnCard.transform,
                        $"🕵️ GİZLİ YAN HESAP (BURNER ACCOUNT)  |  Şüphe Isısı: %{s.BurnerHeat}\nAnonim hesaptan kendi oyununu öv (+6 Hype) veya lider rakibi karala! Dikkat: Hesap değiştirmeyi unutursan ifşa!",
                        13, ColDarkText, TextAnchor.MiddleLeft, true);
                    var btrt = btx.GetComponent<RectTransform>();
                    btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one;
                    btrt.offsetMin = new Vector2(14f, 4f); btrt.offsetMax = new Vector2(-365f, -4f);

                    var bSelf = CreateButton(burnCard.transform, "🕵️ Kendini Öv\n(+6 Hype)", ColPurple, Color.white, 12, () => { _statusBannerMsg = TivitirSystem.UseBurnerAccount(comp, false); ShowTivitirView(); });
                    var srt1 = bSelf.GetComponent<RectTransform>();
                    srt1.anchorMin = new Vector2(1f, 0.5f); srt1.anchorMax = new Vector2(1f, 0.5f);
                    srt1.pivot = new Vector2(1f, 0.5f); srt1.anchoredPosition = new Vector2(-186f, 0f); srt1.sizeDelta = new Vector2(166f, 52f);

                    var bSmear = CreateButton(burnCard.transform, "🗡 Rakibi Karala\n(-Rakip Hayran)", ColRed, Color.white, 12, () => { _statusBannerMsg = TivitirSystem.UseBurnerAccount(comp, true); ShowTivitirView(); });
                    var srt2 = bSmear.GetComponent<RectTransform>();
                    srt2.anchorMin = new Vector2(1f, 0.5f); srt2.anchorMax = new Vector2(1f, 0.5f);
                    srt2.pivot = new Vector2(1f, 0.5f); srt2.anchoredPosition = new Vector2(-12f, 0f); srt2.sizeDelta = new Vector2(166f, 52f);
                    y += 90f;

                    scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(bodyH - 16f, y));
                }
                else
                {
                    var scroll = CreateScrollView(_contentRoot, 0f, bodyTop, 1084f, bodyH);
                    float innerW = 1068f;
                    float y = 0f;

                    var tCard = CreatePanel(scroll.content, ColHighlight);
                    PlaceRect(tCard.GetComponent<RectTransform>(), 0f, y, innerW, 92f);
                    string tDesc;
                    if (s.Tourney != null && s.Tourney.Active)
                        tDesc = $"🏆 AKTİF 4'LÜ 'KİM ALIR?' TURNUVASI (Bitiş: Hf.{s.Tourney.EndW:F0})\n{s.Tourney.Names[0]} ({s.Tourney.Votes[0]})  vs  {s.Tourney.Names[1]} ({s.Tourney.Votes[1]})  vs  {s.Tourney.Names[2]} ({s.Tourney.Votes[2]})  vs  {s.Tourney.Names[3]} ({s.Tourney.Votes[3]})";
                    else if (s.Tourney != null && !string.IsNullOrEmpty(s.Tourney.Result))
                        tDesc = $"🏆 Son Turnuva: {s.Tourney.Result}\nYeni 4'lü 'Kim Alır?' turnuvası açıp yıldızını rakip yıldızlarla kapıştırabilirsin!";
                    else
                        tDesc = "🏆 4'LÜ 'KİM ALIR?' KARAKTER TURNUVASI\nEn popüler karakterini 3 rakip evren yıldızıyla (Feil, Kuriow vb.) 2 haftalık Tivitır anketinde kapıştır!";

                    var tTxt = CreateText(tCard.transform, tDesc, 13, ColDarkText, TextAnchor.MiddleLeft, true);
                    var ttrt = tTxt.GetComponent<RectTransform>();
                    ttrt.anchorMin = Vector2.zero; ttrt.anchorMax = Vector2.one;
                    ttrt.offsetMin = new Vector2(14f, 4f); ttrt.offsetMax = new Vector2(-235f, -4f);

                    bool tActive = s.Tourney != null && s.Tourney.Active;
                    var bTour = CreateButton(tCard.transform, tActive ? "Oylama Sürüyor..." : "🏆 Turnuva Başlat", tActive ? ColTrack : ColPurple, Color.white, 13, () =>
                    {
                        _statusBannerMsg = TivitirSystem.StartCharacterTournament(comp); ShowTivitirView();
                    });
                    var btrt = bTour.GetComponent<RectTransform>();
                    btrt.anchorMin = new Vector2(1f, 0.5f); btrt.anchorMax = new Vector2(1f, 0.5f);
                    btrt.pivot = new Vector2(1f, 0.5f); btrt.anchoredPosition = new Vector2(-12f, 0f); btrt.sizeDelta = new Vector2(210f, 52f);
                    y += 100f;

                    var pHead = CreateText(scroll.content, $"📌 Tivitır Söz Arşivi ('Bu Tivit Yaşlanmadı!' — Son {s.Promises.Count}/8 Taahhüt):", 15, ColHeader, TextAnchor.MiddleLeft, true);
                    PlaceRect(pHead.GetComponent<RectTransform>(), 4f, y, innerW, 28f);
                    y += 32f;

                    if (s.Promises.Count == 0)
                    {
                        var empty = CreatePanel(scroll.content, ColCard);
                        PlaceRect(empty.GetComponent<RectTransform>(), 0f, y, innerW, 74f);
                        var et = CreateText(empty.transform, "Henüz verilmiş büyük bir Tivitır sözü yok. '9+ gelecek', 'Yan Evren geliyor' gibi tivitler atarsan veya Anket/AMA yaparsan burada takip edilir!", 14, ColMutedText, TextAnchor.MiddleCenter, false);
                        StretchWithPad(et.GetComponent<RectTransform>(), 14f, 4f);
                        y += 82f;
                    }
                    else
                    {
                        for (int i = s.Promises.Count - 1; i >= 0; i--)
                        {
                            var pr = s.Promises[i];
                            Color bg = !pr.Resolved ? ColCard : (pr.Kept ? new Color(0.84f, 0.94f, 0.86f, 1f) : new Color(0.96f, 0.85f, 0.86f, 1f));
                            string stStr = !pr.Resolved ? "[BEKLİYOR]" : (pr.Kept ? "[✓ SÖZ TUTULDU!]" : "[✗ SÜT GİBİ KESİLDİ!]");
                            var card = CreatePanel(scroll.content, bg);
                            PlaceRect(card.GetComponent<RectTransform>(), 0f, y, innerW, 56f);
                            var tx = CreateText(card.transform, $"{stStr}  Hf.{pr.W:F0}  —  Hedef: {pr.Target}\n\"{pr.Text}\"", 13, ColDarkText, TextAnchor.MiddleLeft, !pr.Resolved);
                            StretchWithPad(tx.GetComponent<RectTransform>(), 12f, 4f);
                            y += 64f;
                        }
                    }
                    scroll.content.sizeDelta = new Vector2(0f, Mathf.Max(bodyH - 16f, y));
                }

                if (!string.IsNullOrEmpty(_statusBannerMsg))
                {
                    var msgTxt = CreateText(_contentRoot, _statusBannerMsg, 14, ColPurple, TextAnchor.MiddleLeft, true);
                    PlaceRect(msgTxt.GetComponent<RectTransform>(), 4f, 474f, 1076f, 26f);
                }

                DrawHubCloseButton();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Faz 10.0] ShowTivitirView hatasi: {ex}");
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

        private static UnityEngine.UI.InputField CreateInputFieldSafe(Transform parent, string initial, string placeholderText, Action<string> onChanged, out UnityEngine.UI.Text fallbackText, int maxChars = 28, bool multiLine = false)
        {
            fallbackText = null;
            try
            {
                var go = CreatePanel(parent, Color.white);
                var input = go.AddComponent<UnityEngine.UI.InputField>();
                input.targetGraphic = go.GetComponent<UnityEngine.UI.Image>();

                TextAnchor align = multiLine ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
                int phSize = multiLine ? 13 : 16;
                int txSize = multiLine ? 14 : 17;

                var ph = CreateText(go.transform, placeholderText, phSize, ColMutedText, align, false);
                StretchWithPad(ph.GetComponent<RectTransform>(), 10f, multiLine ? 5f : 2f);

                var tx = CreateText(go.transform, initial ?? "", txSize, ColDarkText, align, !multiLine);
                try { tx.supportRichText = false; } catch { }
                try
                {
                    if (multiLine)
                    {
                        tx.horizontalOverflow = HorizontalWrapMode.Wrap;
                        tx.verticalOverflow = VerticalWrapMode.Truncate;
                        ph.horizontalOverflow = HorizontalWrapMode.Wrap;
                    }
                }
                catch { }
                StretchWithPad(tx.GetComponent<RectTransform>(), 10f, multiLine ? 5f : 2f);

                input.textComponent = tx;
                input.placeholder = ph;
                try { input.characterLimit = maxChars; } catch { }
                try { if (multiLine) input.lineType = UnityEngine.UI.InputField.LineType.MultiLineSubmit; } catch { }
                input.text = initial ?? "";

                if (onChanged != null)
                    input.onValueChanged.AddListener(DelegateSupport.ConvertDelegate<UnityAction<string>>(onChanged));
                return input;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Faz 10.2] InputField kullanilamadi, salt-metin moduna gecildi: {ex.Message}");
                var box = CreatePanel(parent, Color.white);
                fallbackText = CreateText(box.transform, initial ?? "", multiLine ? 14 : 17, ColDarkText, multiLine ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft, true);
                StretchWithPad(fallbackText.GetComponent<RectTransform>(), 10f, 4f);
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

                    if (chosen == ProjectCategory.MainUniverse && !UniverseSaveManager.Current.UniverseConfigured)
                    {
                        CharacterWorkshopUGUI.OpenUniverseArchitect(capturedGamePtr, title, true);
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
                CompanyDiplomacySystem.LogCompanyTelemetry(company, "Ilk Baglanti");
            }

            TopicInjectorHelper.EnsureTopicsInjected(true);
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
                    "Universe Architect v10.2.0 — 280 Karakter & Dev Tivitır Havuzu Hazır!",
                    "Faz 10C yayında! Tivit yazma sınırı 24 karakterden 280 karaktere (çok satırlı + canlı sayaç!) çıktı; Tivitır havuzu 5 katın üzerinde büyüyüp 600+ özgün şablon, 16 fenomen, 80 hayran hesabı ve 11.000+ dinamik kombinasyona ulaştı!"
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
            LoggerInstance.Msg("Universe Architect v10.2.0 (Faz 10C — 280 Karakter Uzun Tivit + 600+ Benzersiz Sablon + 11.000+ Kombinasyon Motoru + 16 Fenomen + 80 Hayran Hesabi + 120 Anti-Tekrar) — Tivitir Canli Sosyal Medya + @Fenoctakos & 6 Fenomen + Serbest Tivit & Soz Arsivi [8] + DM [24] + Gizli Yan Hesap + AMA + Karakter Turnuvasi + Akilli Save Diyeti [Delta + Kompakt JSON + Ayri Sosyal Kayit]) baslatiliyor...");

            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.v1020");
                harmony.PatchAll();

                int patchedCount = 0;
                foreach (var _ in harmony.GetPatchedMethods()) patchedCount++;
                LoggerInstance.Msg($"[Universe Architect] Tum v10.2.0 yamalari uygulandi! (Yamalanan metot sayisi: {patchedCount})");
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
            SocialSaveManager.TickFlush(dt);

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
                try { GuestDlcSystem.Tick(ActiveCompany); } catch { }
                try { CompanyDiplomacySystem.Tick(ActiveCompany); } catch { }
                try { HoldingEndgameSystem.Tick(ActiveCompany); } catch { }
                try { MultiverseEmpireSystem.Tick(ActiveCompany); } catch { }
                try { TivitirSystem.Tick(ActiveCompany); } catch { }
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
        private static int _lastLoggedTotal = -1;

        public static void TryInjectTopics()
        {
            EnsureTopicsInjected(false);
        }

        public static void EnsureTopicsInjected(bool forceLog)
        {
            try
            {
                var allTopicsAssets = UnityEngine.Resources.FindObjectsOfTypeAll<Topics>();
                if (allTopicsAssets == null || allTopicsAssets.Length == 0) return;

                int totalAdded = 0;
                int totalAssetCount = 0;
                int finalTopicListCount = 0;

                for (int a = 0; a < allTopicsAssets.Length; a++)
                {
                    var topicsAsset = allTopicsAssets[a];
                    if (topicsAsset == null || topicsAsset.topics == null || topicsAsset.topics.Count == 0) continue;
                    totalAssetCount++;

                    var templateTopic = topicsAsset.topics[0];
                    for (int t = 0; t < Phase1Database.Topics.Count; t++)
                    {
                        var def = Phase1Database.Topics[t];
                        bool exists = false;
                        for (int i = 0; i < topicsAsset.topics.Count; i++)
                            if (topicsAsset.topics[i] != null && topicsAsset.topics[i].id == def.Id) { exists = true; break; }
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
                        totalAdded++;
                    }
                    finalTopicListCount = topicsAsset.topics.Count;
                }

                if (totalAdded > 0 || (forceLog && _lastLoggedTotal != finalTopicListCount))
                {
                    _lastLoggedTotal = finalTopicListCount;
                    MelonLogger.Msg($"[Universe Architect — Konu Zirhi] {Phase1Database.Topics.Count} ozel konu dogrulandi (+{totalAdded} yeni eklendi, Asset: {totalAssetCount}, Toplam Konu: {finalTopicListCount}).");
                    TopicIconInjector.IconsInjected = false;
                    TopicIconInjector.TryInjectIcons();
                }
                if (totalAssetCount > 0) UniverseArchitectMod.TopicsInjected = true;
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

                // A) Post-Credits, Transmedya & Studio Direct Miraslari
                bool hadDirectPromise = false;
                if (UniverseSaveManager.Current.PendingDirectHype > 0)
                {
                    int dHype = UniverseSaveManager.Current.PendingDirectHype;
                    totalModHype += dHype;
                    UniverseSaveManager.Current.PendingDirectHype = 0;
                    hadDirectPromise = UniverseSaveManager.Current.HasActiveDirectPromise;
                    UniverseSaveManager.Current.HasActiveDirectPromise = false;
                    activeBonusNotes.Add($"📺 Studio Direct Rüzgarı (+{dHype} Hype)");
                }
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

                // B) 36 Ozel Konu Bonuslari + Evren Zaman Cizgisi (Cag) Uyumu
                var customTopic = Phase1Database.FindTopicById(topicId);
                if (customTopic != null)
                {
                    if (customTopic.ScoreBonusMultiplier > 1.0) totalScoreMult *= customTopic.ScoreBonusMultiplier;
                    if (customTopic.BonusHype > 0) totalModHype += customTopic.BonusHype;
                    activeBonusNotes.Add($"{customTopic.Name}: {customTopic.BonusLabel}");
                }
                if (category != ProjectCategory.NonUniverse)
                {
                    var uAx = UniverseSaveManager.GetUniverseAxes();
                    string eraLabel;
                    if (UniverseAxisSystem.CheckEraTopicSynergy(uAx[4], topicId, out eraLabel))
                    {
                        totalScoreMult *= 1.02;
                        totalModHype += 3;
                        activeBonusNotes.Add($"{eraLabel} (+%2 Skor, +3 Hype)");
                    }
                    if (uAx[5] >= 0.70f && (category == ProjectCategory.AltTimeline || wasCrossover))
                    {
                        totalModHype += 4;
                        activeBonusNotes.Add("Absürt Çoklu Evren Ruhu (+4 Hype)");
                    }
                    else if (uAx[5] <= 0.30f && category == ProjectCategory.MainUniverse)
                    {
                        totalScoreMult *= 1.01;
                        activeBonusNotes.Add("Ciddi Kanon Disiplini (+%1 Skor)");
                    }
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
                    int modHypeAdd = 8 + (UniverseSaveManager.GetCulture() == StudioCulture.CommunityFirst ? 6 : 0);
                    totalScoreMult *= 1.04; totalModHype += modHypeAdd;
                    double bonusFans = Math.Max(50.0, Math.Round(company.fans * 0.05));
                    company.adjustFans(bonusFans);
                    activeBonusNotes.Add($"Resmi Mod Desteği (+%4 Skor, +{modHypeAdd} Hype, +{bonusFans:F0} Hayran)");
                }

                // C.5) FAZ 9.0: SEKTOR DEPREMLERI (DURGUNLUK / ALTIN CAG / MODA)
                var secEv = MultiverseEmpireSystem.GetActiveSectorEvent(company.currentWeek);
                if (secEv == SectorEventType.Recession)
                {
                    hypePenalty += 3;
                    activeBonusNotes.Add("📉 Ekonomik Durgunluk (-3 Hype)");
                }
                else if (secEv == SectorEventType.GoldenAge)
                {
                    double gFans = Math.Clamp(Math.Round(company.fans * 0.10), 25.0, 1200.0);
                    company.adjustFans(gFans);
                    activeBonusNotes.Add($"✨ Altın Çağ (+{gFans:F0} Hayran)");
                }
                else if (secEv == SectorEventType.TrendWave && category != ProjectCategory.NonUniverse)
                {
                    if (string.Equals(UniverseSaveManager.Current.UniverseToneName, UniverseSaveManager.Current.SectorTrendToneName, StringComparison.OrdinalIgnoreCase))
                    {
                        totalScoreMult *= 1.03;
                        totalModHype += 8;
                        activeBonusNotes.Add("🔥 Sektör Modası Eşleşti! (+%3 Skor, +8 Hype)");
                    }
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
                        if (UniverseSaveManager.GetCulture() == StudioCulture.CanonGuardian) seqBonus += 0.02;
                        totalScoreMult *= (1.0 + seqBonus);
                        activeBonusNotes.Add($"Ana Evren Devam Oyunu (+%{seqBonus * 100:F0} Skor)");
                    }

                    if (UniverseSaveManager.Current.LoreBarrier >= 60)
                    {
                        double barrierLoss = Math.Min(company.fans * 0.03, 300.0);
                        if (barrierLoss >= 10.0) company.adjustFans(-barrierLoss);
                        activeBonusNotes.Add($"Lore Bariyeri Yüksek ({UniverseSaveManager.Current.LoreBarrier}/100)!");
                    }
                    int barrierInc = UniverseSaveManager.GetCulture() == StudioCulture.CanonGuardian ? 10 : 15;
                    if (UniverseSaveManager.GetUniverseRule() == UniverseRule.MysteriousArchive) barrierInc -= 4;
                    barrierInc = Math.Max(4, barrierInc - UniverseSaveManager.Current.DeptLoreLevel * 2);
                    UniverseSaveManager.AdjustLoreBarrier(barrierInc);
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
                        int altH = UniverseSaveManager.GetUniverseRule() == UniverseRule.ChaosUnleashed ? 14 : 10;
                        totalModHype += altH;
                        activeBonusNotes.Add($"Alternatif Zaman (+{altH} Hype, Bariyer {barrierDrop})");
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
                double rawBaseScore = game.score;
                if (cast != null && cast.Count > 0 && category != ProjectCategory.NonUniverse)
                {
                    EvaluateCast(company, title, topicId, genreKey, secondGenreKey, displayGenre, category, law, rawBaseScore, cast, pendingBonds, castSummary,
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
                        else if (projectedScore < (UniverseSaveManager.GetUniverseRule() == UniverseRule.ChaosUnleashed ? 6.0 : 6.5))
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

                // G.2) FAZ 7.1: MUTTEFIK ORTAK YAPIM (STUDYO-ARASI MEGA CROSSOVER!)
                if (category != ProjectCategory.NonUniverse && !string.IsNullOrEmpty(UniverseSaveManager.Current.ActiveCoProdRivalId))
                {
                    var coRival = UniverseSaveManager.FindRivalById(UniverseSaveManager.Current.ActiveCoProdRivalId);
                    string coName = UniverseSaveManager.Current.ActiveCoProdRivalName;
                    string coUni = UniverseSaveManager.Current.ActiveCoProdUniverse;
                    UniverseSaveManager.Current.ActiveCoProdRivalId = "";
                    UniverseSaveManager.Current.ActiveCoProdRivalName = "";
                    UniverseSaveManager.Current.ActiveCoProdUniverse = "";

                    wasCrossover = true;
                    UniverseSaveManager.Current.CompletedCrossovers++;
                    totalScoreMult *= 1.06;
                    totalModHype += 20;

                    long coFans = Math.Max(120, (long)Math.Round(((coRival != null ? coRival.Fans : 400) * 0.15) + company.fans * 0.10));
                    company.adjustFans(coFans);
                    if (coRival != null)
                    {
                        coRival.Fans += Math.Max(60, (long)Math.Round(company.fans * 0.08));
                        UniverseSaveManager.AdjustRivalRelation(coRival, +15);
                    }
                    UniverseSaveManager.AdjustReputation(+5);
                    activeBonusNotes.Add($"★ MEGA CROSSOVER ({coName}): +%6 Skor, +20 Hype, +{coFans} Hayran!");
                    UniverseSaveManager.AddRivalNews(company.currentWeek, coName,
                        $"★ STÜDYOLAR ARASI MEGA CROSSOVER: '{title}' Piyasayı Salladı!",
                        $"Senin evrenin ile {coUni} birleşti! Karşılıklı dev hayran akışı (+{coFans} Hayran, +15 İlişki, +5 İtibar).", "CROSSOVER");
                    NotificationHelper.Show(company, $"★ MEGA CROSSOVER YAYINDA: {title}!",
                        $"{coName} ({coUni}) ile ortak yapım dev ses getirdi! (+%6 Skor, +20 Hype, +{coFans} Hayran, +5 İtibar)", true);
                }

                // G.5) FAZ 7.0: SIRKET KULTURU & PR MASASI ETKILERI
                var sCulture = UniverseSaveManager.GetCulture();
                if (sCulture == StudioCulture.ArtisanIndie)
                {
                    totalScoreMult *= 1.04;
                    if (company.cash >= 12000.0) company.adjustCash(-8000.0, "Sanatçı Indie Cila Gideri");
                    activeBonusNotes.Add("Kültür: Sanatçı Indie (+%4 Skor)");
                }
                else if (sCulture == StudioCulture.HypeMachine)
                {
                    totalModHype += 10;
                    activeBonusNotes.Add("Kültür: Hype Makinesi (+10 Hype)");
                }
                else if (sCulture == StudioCulture.CommunityFirst)
                {
                    double commFans = Math.Clamp(Math.Round(company.fans * 0.08), 25.0, 1500.0);
                    company.adjustFans(commFans);
                    activeBonusNotes.Add($"Kültür: Topluluk Dostu (+{commFans:F0} Hayran)");
                }

                if (hypePenalty > 0 && UniverseSaveManager.Current.DeptPrLevel > 0)
                {
                    double prKeep = Math.Max(0.40, 1.0 - UniverseSaveManager.Current.DeptPrLevel * 0.20);
                    int reduced = (int)Math.Round(hypePenalty * prKeep);
                    if (reduced < hypePenalty)
                    {
                        activeBonusNotes.Add($"PR Masası Sv.{UniverseSaveManager.Current.DeptPrLevel} (-{hypePenalty - reduced} Hype hasarı emildi)");
                        hypePenalty = reduced;
                    }
                }

                // G.8) FAZ 9.0: COKLU IP KRIZI, SAGA FINALI & TERS KOSE KUMARI!
                if (eventOutcome != null && eventOutcome.IsMultiIpCrisis && category != ProjectCategory.NonUniverse)
                {
                    totalScoreMult *= 1.07;
                    totalModHype += 22;
                    double mFans = Math.Clamp(Math.Round(company.fans * 0.12), 120.0, 2200.0);
                    company.adjustFans(mFans);
                    UniverseSaveManager.AdjustReputation(+5);
                    activeBonusNotes.Add($"🌌 ÇOKLU IP KRİZİ ({eventOutcome.MultiIpSummary}): +%7 Skor, +22 Hype, +{mFans:F0} Hayran!");
                    UniverseSaveManager.AddChronology(company.currentWeek, "KRIZ",
                        $"🌌 Çoklu IP Krizi: '{title}'",
                        $"{eventOutcome.MultiIpSummary} evrenleri tek oyunda çarpıştı!");
                }

                if (eventOutcome != null && eventOutcome.HasTwistGamble)
                {
                    double projTwist = Math.Clamp(game.score * totalScoreMult, 1.0, 10.0);
                    if (projTwist >= 7.5)
                    {
                        totalScoreMult *= 1.05;
                        totalModHype += 12;
                        activeBonusNotes.Add("🔮 Efsane Ters Köşe! (+%5 Skor, +12 Hype)");
                    }
                    else if (projTwist < 6.5)
                    {
                        hypePenalty += 6;
                        UniverseSaveManager.AdjustReputation(-3);
                        activeBonusNotes.Add("🔮 Ters Köşe Tepkisi (-6 Hype, -3 İtibar)");
                    }
                }

                // G.9) FAZ 10.1: TIVITIR ON-SKOR ETKILERI (TT #1 & KATEGORI SOZLERI)
                double projSocialScore = Math.Clamp(game.score * totalScoreMult, 1.0, 10.0);
                TivitirSystem.ApplyPreScoreSocialEffects(company, title, projSocialScore, category, wasCrossover, ref totalScoreMult, ref totalModHype, ref hypePenalty, activeBonusNotes);

                // H) Global Denge Tavani + Buyume Beklenti Baskisi + Gercek ReviewType Ekran Senkronu!
                double maxScoreCap = wasCrossover ? BalanceCaps.MaxCrossoverScoreMult : BalanceCaps.MaxModScoreMult;
                double cappedMult = Math.Clamp(totalScoreMult, BalanceCaps.MinModScoreMult, maxScoreCap);
                double expectationPenalty = BalancePressureSystem.ComputeExpectationPenalty(company);
                if (expectationPenalty >= 0.01)
                    activeBonusNotes.Add($"Beklenti Baskısı (-%{expectationPenalty * 100.0:F0} Skor)");

                double preModScore = game.score;
                double finalMult = Math.Clamp(cappedMult * (1.0 - expectationPenalty), 0.72, maxScoreCap);
                double computedFinalScore = Math.Clamp(preModScore * finalMult, 1.0, 10.0);
                game.score = computedFinalScore;
                BalancePressureSystem.SynchronizeReviewCards(game, preModScore, computedFinalScore);
                TivitirSystem.PostPlayerGameReleaseTweet(company, title, game.score, category, wasCrossover, activeBonusNotes);

                if (sCulture == StudioCulture.HypeMachine && game.score < 6.0)
                {
                    hypePenalty += 10;
                    UniverseSaveManager.AdjustReputation(-8);
                    activeBonusNotes.Add("Hype Makinesi Hayal Kırıklığı! (-10 Hype, -8 İtibar)");
                }

                int netHype = UniverseArchitectMod.ApplyCappedModHype(totalModHype, wasCrossover) - hypePenalty;
                if (netHype != 0) company.adjustHype(netHype);

                // H.4) FAZ 7.1: STUDIO DIRECT SOZ KONTROLU, YATIRIMCI HEDEF & GISE CARPISMASI
                if (hadDirectPromise)
                {
                    if (game.score >= 8.0)
                    {
                        long dFans = Math.Max(80, (long)Math.Round(company.fans * 0.07));
                        company.adjustFans(dFans);
                        UniverseSaveManager.AdjustReputation(+6);
                        activeBonusNotes.Add($"📺 Efsane Direct Teslimatı (+{dFans} Hayran, +6 İtibar)");
                    }
                    else if (game.score < 6.5)
                    {
                        company.adjustHype(-12);
                        UniverseSaveManager.AdjustReputation(-10);
                        activeBonusNotes.Add("⚠ Direct Hayal Kırıklığı! 'Fragmanla alakası yok!' (-12 Hype, -10 İtibar)");
                        NotificationHelper.Show(company, $"STUDIO DIRECT TEPKİSİ: {title}!",
                            $"Büyük sunumla duyurduğun '{title}' ({game.score:F1}/10) beklentiyi karşılayamadı! (-12 Hype, -10 İtibar)", true);
                    }
                }

                HoldingEndgameSystem.OnPlayerGameReviewedInvestorCheck(company, title, game.score, activeBonusNotes);
                int clashBonusHype = 0, clashPenHype = 0;
                HoldingEndgameSystem.CheckBoxOfficeClash(company, title, game.score, ref clashBonusHype, ref clashPenHype, activeBonusNotes);
                if (clashBonusHype - clashPenHype != 0) company.adjustHype(clashBonusHype - clashPenHype);

                // H.45) FAZ 9.0: SAGA FINALI & KRONOLOJI KAYDI
                var actUniPost = UniverseSaveManager.GetActiveOwnedUniverse();
                if (category != ProjectCategory.NonUniverse) actUniPost.SagaGamesCount++;
                if (eventOutcome != null && eventOutcome.IsSagaFinale && category != ProjectCategory.NonUniverse)
                {
                    if (game.score >= 7.5)
                    {
                        int finishedSaga = actUniPost.SagaNumber;
                        actUniPost.SagaNumber++;
                        actUniPost.SagaGamesCount = 0;
                        UniverseSaveManager.Current.LoreBarrier = 0;
                        UniverseSaveManager.AdjustIntegrity(+10);
                        UniverseSaveManager.AdjustReputation(+6);
                        company.adjustHype(14);
                        activeBonusNotes.Add($"★ SAGA FİNALİ ZAFERİ! {actUniPost.Name} Faz {actUniPost.SagaNumber} Başladı (Bariyer Sıfırlandı!)");
                        UniverseSaveManager.AddChronology(company.currentWeek, "SAGA",
                            $"★ {actUniPost.Name} Faz {finishedSaga} Finali: '{title}' ({game.score:F1}/10)",
                            $"Lore Bariyeri sıfırlandı ve Faz {actUniPost.SagaNumber} temiz sayfayla açıldı!");
                        NotificationHelper.Show(company, $"★ SAGA FİNALİ EFSANESİ: {title}!",
                            $"{actUniPost.Name} Faz {finishedSaga} görkemle kapandı! Lore Bariyeri 0'a sıfırlandı, Faz {actUniPost.SagaNumber} başladı (+10 Tut., +6 İtibar).", true);
                    }
                    else
                    {
                        UniverseSaveManager.AdjustIntegrity(-6);
                        activeBonusNotes.Add("⚠ Saga Finali Beklentiyi Karşılamadı (-6 Tut.)");
                    }
                }
                else if (game.score >= 8.5)
                {
                    UniverseSaveManager.AddChronology(company.currentWeek, "HIT",
                        $"★ Başyapıt: '{title}' ({game.score:F2}/10)",
                        $"Evren: {actUniPost.Name} | Kategori: {CategoryLabels.ToTurkish(category)}");
                }

                // H.5) FAZ 7.0: STUDYO ITIBARI GUNCELLEMESI
                if (game.score >= 9.0) UniverseSaveManager.AdjustReputation(+7);
                else if (game.score >= 8.0) UniverseSaveManager.AdjustReputation(+4);
                else if (game.score < 5.5) UniverseSaveManager.AdjustReputation(-4);

                // I) Ana Evren Tutarlilik Guncellemesi (Kanon Capasi + Kanon Muhafizi + Lore Odasi kalkani!)
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
                        if (sCulture == StudioCulture.CanonGuardian)
                            damage = (int)Math.Round(damage * 0.65);
                        if (UniverseSaveManager.Current.DeptLoreLevel > 0)
                            damage = (int)Math.Round(damage * Math.Max(0.55, 1.0 - UniverseSaveManager.Current.DeptLoreLevel * 0.15));

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

                // J) FAZ 5 & 7.0: SOLO-DEV TUKENMISLIK, RAKIP TEPKILERI & TELEMETRI
                SoloBurnoutSystem.OnWorkCompleted(company, category == ProjectCategory.NonUniverse ? 12 : 20);
                RivalSimulationSystem.OnPlayerGameReleased(company, title, game.score, category, wasCrossover);
                CompanyDiplomacySystem.LogGameSalesTelemetry(game, company);

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
            ProjectCategory category, CosmologyLaw law, double rawBaseScore, List<ActiveCharacterDraft> cast, List<PendingBondDraft> pendingBonds,
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
            int protagonistCount = 0;
            for (int pIdx = 0; pIdx < cast.Count; pIdx++)
                if (!cast[pIdx].IsNpc && cast[pIdx].Role == CharacterRole.Protagonist) protagonistCount++;

            // 1. GECIS: OZEL KARAKTERLER (4 EKSEN + EVRIM + ROL KIRILMASI + OZLEM + SPIN-OFF + KADER)
            for (int cIdx = 0; cIdx < cast.Count; cIdx++)
            {
                var draft = cast[cIdx];
                if (draft.IsNpc) continue;

                // FAZ 9.0: TERSINE KONUK DLC (RAKIPLERDEN KIRALANAN KONUK YILDIZ)
                if (draft.IsRivalGuestHire)
                {
                    ensembleScoreDelta += 0.03;
                    modHype += 10;
                    var srcRival = UniverseSaveManager.FindRivalById(draft.SourceRivalId);
                    long guestFans = Math.Clamp((long)Math.Round((srcRival != null ? srcRival.Fans : 350) * 0.12), 45L, 900L);
                    company.adjustFans(guestFans);
                    notes.Add($"🎟 Konuk Yıldız: {draft.Name} ({draft.SourceRivalName}): +%3 Skor, +10 Hype, +{guestFans} Hayran!");
                    castSummary.Add($"{draft.Name} [Konuk: {draft.SourceRivalName}]");

                    // PendingRivalGuests listesinden tuket
                    var pgList = UniverseSaveManager.Current.PendingRivalGuests;
                    for (int pg = pgList.Count - 1; pg >= 0; pg--)
                        if (pgList[pg].StarId == draft.SourceRivalStarId) pgList.RemoveAt(pg);
                    continue;
                }

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
                double deptChemBoost = UniverseSaveManager.Current.DeptLoreLevel * 0.02 + (UniverseSaveManager.GetCulture() == StudioCulture.ArtisanIndie ? 0.03 : 0.0);

                // 8. Eksen (Ekip Ruhu): Yalniz Kurt (<=0.35) vs Ekip Lideri (>=0.65)
                bool isSoloCastChar = string.IsNullOrWhiteSpace(draft.TeamName);
                if (axes[7] <= 0.35f && isSoloCastChar)
                {
                    deptChemBoost += 0.04;
                    notes.Add($"Yalnız Kurt Karizması: {draft.Name}");
                }
                else if (axes[7] >= 0.65f && !isSoloCastChar)
                {
                    deptChemBoost += 0.03;
                }

                if (deptChemBoost > 0.0) chemistry = Math.Clamp(chemistry + deptChemBoost, 0.20, 0.99);

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
                        Popularity = PopularitySystem.ComputeDebutPopularity(chemistry, rawBaseScore),
                        UniverseId = UniverseSaveManager.GetActiveOwnedUniverse().Id,
                        UniverseName = UniverseSaveManager.GetActiveOwnedUniverse().Name
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
                    double missFans = Math.Clamp(Math.Round(company.fans * 0.035), 40.0, 900.0);
                    company.adjustFans(missFans);
                    notes.Add($"🔥 Özlem Bonusu: {rec.Name} geri döndü! (+%4 Skor, +14 Hype)");
                    NotificationHelper.Show(company, $"BÜYÜK GERİ DÖNÜŞ: {rec.Name}!",
                        $"{rec.GamesMissed} oyundur gözükmeyen {rec.Name} sahneye geri dönünce hayranlar çıldırdı! (+14 Hype, +{missFans:F0} Hayran)", true);
                }
                rec.GamesMissed = 0;

                // C) Kult Solo Spin-Off Baskisi karsilandi mi?
                if (!isBrandNew && rec.SpinOffDemandRemaining > 0 && category == ProjectCategory.SideUniverse && draft.Role == CharacterRole.Protagonist && protagonistCount == 1)
                {
                    rec.SpinOffDemandRemaining = 0;
                    ensembleScoreDelta += 0.06;
                    modHype += 15;
                    double spFans = Math.Clamp(Math.Round(company.fans * 0.05), 60.0, 1200.0);
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
                        UniverseSaveManager.AddChronology(company.currentWeek, "ARK", $"⚡ İhanet Arkı: {rec.Name}", $"'{gameTitle}' oyununda karanlık tarafa geçti.");
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
                        UniverseSaveManager.AddChronology(company.currentWeek, "ARK", $"★ Kefaret Arkı: {rec.Name}", $"'{gameTitle}' oyununda iyiliğe döndü.");
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
                        if (UniverseSaveManager.GetUniverseRule() == UniverseRule.LethalWorld) tutPenalty -= 6;
                        UniverseSaveManager.AdjustIntegrity(tutPenalty);
                        UniverseSaveManager.AdjustReputation(-8);
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
                rec.LastAppearedWeek = company.currentWeek;
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
                    double fatMult = UniverseSaveManager.GetCulture() == StudioCulture.CommunityFirst ? 0.5 : 1.0;
                    ensembleScoreDelta -= 0.04 * roleWeight * fatMult;
                    hypePenalty += (int)Math.Round(5 * roleWeight * fatMult);
                    rec.Popularity = PopularitySystem.ApplyDelta(rec.Popularity, -8);
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
                        double fanBoost = Math.Clamp(Math.Round(company.fans * 0.03 * roleWeight), 20.0, 750.0);
                        company.adjustFans(fanBoost);

                        bool becameCultSideRole = (draft.Role == CharacterRole.Minor || draft.Role == CharacterRole.Sidekick) && rec.Status != "Kült";
                        double rawDelta = PopularitySystem.ComputeGamePerformanceRawDelta(chemistry, rawBaseScore, roleWeight);
                        if (!isBrandNew) rec.Popularity = PopularitySystem.ApplyDelta(rec.Popularity, Math.Max(3.0, rawDelta));
                        else rec.Popularity = PopularitySystem.ApplyDelta(rec.Popularity, Math.Max(2.0, rawDelta * 0.45));
                        rec.Status = draft.Role == CharacterRole.Minor ? "Kült" : (rec.Popularity >= 72 || chemistry >= 0.85 ? "Yıldız" : "Sevilen");
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
                        double midRaw = PopularitySystem.ComputeGamePerformanceRawDelta(chemistry, rawBaseScore, roleWeight);
                        rec.Popularity = PopularitySystem.ApplyDelta(rec.Popularity, midRaw);
                        notes.Add($"Sevilen {rec.LastRole}: {rec.Name} [%{chemPct}]");
                    }
                    else if (chemistry < 0.45)
                    {
                        ensembleScoreDelta -= 0.07 * roleWeight;
                        hypePenalty += (int)Math.Round(6 * roleWeight);
                        rec.Status = "Fiyasko";
                        double flopRaw = Math.Min(-6.0, PopularitySystem.ComputeGamePerformanceRawDelta(chemistry, rawBaseScore, roleWeight));
                        rec.Popularity = PopularitySystem.ApplyDelta(rec.Popularity, flopRaw);
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
                        bool isLethal = UniverseSaveManager.GetUniverseRule() == UniverseRule.LethalWorld;
                        ensembleScoreDelta += isLethal ? 0.08 : 0.06;
                        modHype += 16;
                        UniverseSaveManager.AdjustIntegrity(+6);
                        UniverseSaveManager.AdjustReputation(isLethal ? +8 : +5);
                        rec.Popularity = PopularitySystem.ApplyDelta(rec.Popularity, 12);
                        double sacFans = Math.Clamp(Math.Round(company.fans * 0.07), 80.0, 1500.0);
                        company.adjustFans(sacFans);
                        notes.Add($"☠ FİNAL FEDASI: {rec.Name} ölümsüzleşti! (+%6 Skor, +16 Hype, +6 Tut.)");
                        UniverseSaveManager.AddChronology(company.currentWeek, "KADER", $"☠ Final Fedası: {rec.Name}", $"'{gameTitle}' finalinde kendini feda ederek ölümsüzleşti.");
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
                    double bScore = 0.01 + bond.Level * 0.01 + (UniverseSaveManager.GetUniverseRule() == UniverseRule.TeamSpirit ? 0.01 : 0.0);
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
                            int inheritedRaw = Math.Clamp(mentor.Popularity / 6, 8, 14);
                            int beforeP = rookie.Popularity;
                            rookie.Popularity = PopularitySystem.ApplyDelta(rookie.Popularity, inheritedRaw);
                            int inherited = Math.Max(1, rookie.Popularity - beforeP);
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
                ensembleScoreDelta += UniverseSaveManager.GetUniverseRule() == UniverseRule.TeamSpirit ? 0.05 : 0.04;
                modHype += 8;
                double teamFans = Math.Clamp(Math.Round(company.fans * 0.035), 35.0, 900.0);
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
                    npcRec.Popularity = PopularitySystem.ApplyDelta(npcRec.Popularity, 18);
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
                    npcRec.Popularity = PopularitySystem.ApplyDelta(npcRec.Popularity, -16);
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
