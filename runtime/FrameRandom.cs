using Godot;
using MaxGodotFrame.Random;

namespace MaxGodotFrame;

/// <summary>Variant-safe API for C# and GDScript. State uses PackedByteArray, not unsigned Variants.</summary>
[GlobalClass]
public partial class FrameRandom : RefCounted
{
    private readonly RandomStream stream = new(0);

    public void Reseed(long seed) => stream.Reseed(unchecked((ulong)seed));
    public int NextInt(int exclusiveMaximum) => stream.NextInt(exclusiveMaximum);
    public byte[] ExportState() => stream.ExportState();
    public string GetAlgorithm() => RandomStream.Algorithm;
    public bool RestoreState(string algorithm, byte[] state)
    {
        try { stream.ImportState(algorithm, state); return true; }
        catch (System.ArgumentException error) { GD.PushError(error.Message); return false; }
    }
}
