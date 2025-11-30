using ResoniteModLoader;
using HarmonyLib;
using FrooxEngine;

namespace FastSync;

public partial class FastSync : ResoniteMod
{
    public override string Name => "FastSync";
    public override string Author => "Raidriar796";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/Raidriar796/FastSync";
    private static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Harmony harmony = new Harmony("net.raidriar796.FastSync");
        Config = GetConfiguration();
        Config?.Save(true);
        harmony.PatchAll();
    }
}
