using System;
using System.Linq;
using System.Buffers.Binary;
using System.Numerics;

namespace MaxGodotFrame.Random;

/// <summary>Versioned, independent xoshiro256** stream. No engine or global state.</summary>
public sealed class RandomStream
{
    public const string Algorithm = "xoshiro256starstar-splitmix64-v1";
    private ulong a, b, c, d;

    public RandomStream(ulong seed) => Reseed(seed);

    public void Reseed(ulong seed)
    {
        a = SplitMix(ref seed); b = SplitMix(ref seed);
        c = SplitMix(ref seed); d = SplitMix(ref seed);
    }

    public ulong NextUInt64()
    {
        unchecked
        {
            ulong result = BitOperations.RotateLeft(b * 5, 7) * 9;
            ulong shift = b << 17;
            c ^= a; d ^= b; b ^= c; a ^= d;
            c ^= shift; d = BitOperations.RotateLeft(d, 45);
            return result;
        }
    }

    /// <summary>Uniform integer in [0, exclusiveMaximum). Rejection removes modulo bias.</summary>
    public int NextInt(int exclusiveMaximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveMaximum);
        ulong bound = (ulong)exclusiveMaximum;
        ulong threshold = unchecked(0UL - bound) % bound;
        ulong sample;
        do { sample = NextUInt64(); } while (sample < threshold);
        return (int)(sample % bound);
    }

    /// <summary>Four little-endian words. The algorithm identifier must travel with this payload.</summary>
    public byte[] ExportState()
    {
        byte[] result = new byte[32];
        ulong[] words = [a, b, c, d];
        for (int i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteUInt64LittleEndian(result.AsSpan(i * 8, 8), words[i]);
        return result;
    }

    public void ImportState(string algorithm, ReadOnlySpan<byte> state)
    {
        if (algorithm != Algorithm || state.Length != 32)
            throw new ArgumentException("Unsupported random state format.");
        ulong[] words = new ulong[4];
        for (int i = 0; i < words.Length; i++)
            words[i] = BinaryPrimitives.ReadUInt64LittleEndian(state.Slice(i * 8, 8));
        if (words.All(x => x == 0)) throw new ArgumentException("All-zero random state is invalid.");
        (a, b, c, d) = (words[0], words[1], words[2], words[3]);
    }

    private static ulong SplitMix(ref ulong seed)
    {
        unchecked
        {
            ulong z = seed += 0x9e3779b97f4a7c15UL;
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
            z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
            return z ^ (z >> 31);
        }
    }
}
