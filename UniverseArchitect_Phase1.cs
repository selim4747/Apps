using System;
using System.Collections.Generic;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2CppRarebyte.GDT;

[assembly: MelonInfo(typeof(UniverseArchitect.UniverseArchitectMod), "Universe Architect (NJG Edition)", "1.0.1", "NJG Games")]
[assembly: MelonGame("Greenheart Games", "Game Dev Tycoon")]

namespace UniverseArchitect
{
    public class CustomTopicDef
    {
        public string Id;
        public string Name;
        public Dictionary<string, double> GenreWeights;
        public double[] AudienceWeights; // [0] = Young, [1] = Everyone, [2] = Mature
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
                AudienceWeights = new[] { 0.9, 0.7, 1.0 }
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
                AudienceWeights = new[] { 0.6, 0.7, 1.0 }
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
                AudienceWeights = new[] { 0.8, 1.0, 0.8 }
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
                AudienceWeights = new[] { 1.0, 1.0, 0.7 }
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
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }
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
                AudienceWeights = new[] { 0.9, 1.0, 0.8 }
            }
        };
    }

    public class UniverseArchitectMod : MelonMod
    {
        public static bool TopicsInjected = false;
        private float _injectTimer = 0f;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Universe Architect v1.0.1 (Sadece Offline + Guvenli Konular) baslatiliyor...");
            try
            {
                var harmony = new HarmonyLib.Harmony("com.njggames.universearchitect.safetest");
                harmony.PatchAll();
                LoggerInstance.Msg("[Universe Architect] Offline yamalari basariyla uygulandi!");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"[Universe Architect] Harmony yama hatasi: {ex}");
            }
        }

        public override void OnUpdate()
        {
            if (TopicsInjected) return;

            _injectTimer += Time.unscaledDeltaTime;
            if (_injectTimer < 2.0f) return;
            _injectTimer = 0f;

            TopicInjectorHelper.TryInjectTopics();
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
                if (allTopicsAssets == null || allTopicsAssets.Length == 0)
                    return;

                var topicsAsset = allTopicsAssets[0];
                if (topicsAsset == null || topicsAsset.topics == null || topicsAsset.topics.Count == 0)
                    return;

                // Mevcut ilk konuyu sablon al (missionOverrides null kalmasin diye)
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

                    // Varsayilan sira: Action, Adventure, RPG, Simulation, Strategy, Casual
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
                    MelonLogger.Msg($"[Universe Architect] Guvenli konu eklendi: {def.Name}");
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
    // GOOGLE PLAY GAMES OFFLINE YAMALARI
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
