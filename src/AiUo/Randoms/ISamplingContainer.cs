using System;

namespace AiUo.Randoms;

public interface ISamplingContainer : IDisposable
{
    void AddRandom(int min, int max, params byte[] rnds);
    void AddRandom(int min, int max, params int[] rnds);
    void AddNotRepeat(int range, int size, params int[] rnds);
}