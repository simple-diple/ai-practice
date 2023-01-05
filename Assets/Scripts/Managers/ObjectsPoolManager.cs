using System.Collections.Generic;

public static class ObjectsPoolManager
{

    private static readonly List<IDestroyable> pools = new List<IDestroyable>();

    public static void Add(IDestroyable pool)
    {
        pools.Add(pool);
    }

    public static void Remove(IDestroyable pool)
    {
        pool.Destroy();
        pools.Remove(pool);
    }

    public static void Destroy()
    {
        var cnt = pools.Count;
        for (var i = 0; i < cnt; ++i)
        {
            pools[i].Destroy();
        }
        pools.Clear();
    }
}