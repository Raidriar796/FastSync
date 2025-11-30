using ResoniteModLoader;
using HarmonyLib;
using FrooxEngine;
using Elements.Core;
using LZ4;

namespace FastSync;

public partial class FastSync : ResoniteMod
{
    [AutoRegisterConfigKey]
    public static readonly ModConfigurationKey<bool> enable =
        new("enable", "Enable FastSync", () => true);

    public override string Name => "FastSync";
    public override string Author => "Raidriar796";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/Raidriar796/FastSync";
    private static ModConfiguration? Config;

    public override void OnEngineInit()
    {
        Harmony harmony = new("net.raidriar796.FastSync");
        Config = GetConfiguration();
        Config?.Save(true);
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(SyncMessage), "Encode")]
    private class EncodePatch
    {
        private static bool Prefix(ref RawOutMessage __result, SyncMessage __instance)
        {
            // Run the original method if the mod is not enabled
            if (!Config!.GetValue(enable)) return true;

            // Reconstruction of the original method
            RawOutMessage raw = new();
	        raw.UseReliable = __instance.Reliable;
	        raw.Background = __instance.Background;
	        MemoryStream stream = SyncMessage.StreamManager.GetStream();
	        BinaryWriterX writer = Pool.BorrowBinaryWriter(stream);
	        byte header = (byte)__instance.SyncMessageType;
	        if (__instance.ShouldCompress)
	        {
		        header |= 128;
	        }
	        writer.Write(header);
	        writer.Write7BitEncoded(__instance.SenderStateVersion);
	        writer.Write7BitEncoded(__instance.SenderSyncTick);
	        LZ4Stream lz4stream = null!;
	        if (__instance.ShouldCompress)
	        {
	            // This is where the magic happens
		        lz4stream = new LZ4Stream(stream, LZ4StreamMode.Compress, LZ4StreamFlags.IsolateInnerStream, 1048576, SyncMessage._lz4BufferAllocator);
		        writer.TargetStream = lz4stream;
	        }
	        __instance.EncodeData(writer);
	        Pool.Return(ref writer);
	        if (lz4stream != null)
	        {
		        lz4stream.Dispose();
	        }
	        raw.Data = stream;
	        __instance.EncodedSize = (int)stream.Length;
	        foreach (IConnection t in __instance.Targets)
	        {
		        raw.Targets.Add(t);
	        }
	        // Change result of the original method
	        __result = raw;
        
            // Skip original method
	        return false;
        }
    }
}
