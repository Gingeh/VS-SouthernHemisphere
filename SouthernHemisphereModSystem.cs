using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace SouthernHemisphere;

[HarmonyPatch(
    typeof(NoiseClimateRealistic),
    MethodType.Constructor,
    typeof(long), typeof(double), typeof(int), typeof(int), typeof(int)
)]
public static class NoiseClimateRealisticPatch
{
    static void Postfix(NoiseClimateRealistic __instance, double mapsizeZ, double ___halfRange)
    {
        var flippedZOffset = 2 * ___halfRange - __instance.ZOffset - mapsizeZ;
        Traverse.Create(__instance)
            .Property(nameof(NoiseClimateRealistic.ZOffset))
            .SetValue(flippedZOffset);
    }
}

public class SouthernHemisphereModSystem : ModSystem
{
    private Harmony? _harmony;

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Server;
    }

    public override void StartPre(ICoreAPI api)
    {
        _harmony = new Harmony(Mod.Info.ModID);
        _harmony.PatchAll();
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Event.SaveGameCreated += () =>
        {
            var sixMonths = 60 * 60 * (double)api.World.Calendar.HoursPerDay * api.World.Calendar.DaysPerMonth * 6;
            var savegame = (SaveGame)api.WorldManager.SaveGame;
            savegame.TotalGameSecondsStart += (long)sixMonths;
            savegame.TotalGameSeconds += (long)sixMonths;
        };
    }

    public override void Dispose()
    {
        _harmony?.UnpatchAll(Mod.Info.ModID);
    }
}

