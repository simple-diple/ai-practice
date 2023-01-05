using System;
using UnityEngine;

public class ObjectsPool<T> where T : ObjectsPool<T>, new()
{
    private class Destroyer : IDestroyable
    {
        public Destroyer()
        {
        }

        public void Destroy()
        {
            pool = null;
            objCount = 0;
            _destroyer = null;
        }

    }

    private static Destroyer _destroyer = new Destroyer();
    private const int PoolSize = 1024;
    private static ObjectsPool<T>[] pool;
    private static int objCount;
    public int id;
    private static int nextId;
    //private static int newObjCount;

    protected ObjectsPool()
    {
        if (pool == null)
        {
            pool = new ObjectsPool<T>[PoolSize];
            if (_destroyer == null)
            {
                _destroyer = new Destroyer();
            }
            ObjectsPoolManager.Add(_destroyer);
        }
    }

    public static void Destroy()
    {
        Debug.Log("Cleaning pool: " + typeof(T) + " / " + objCount);
        if (_destroyer != null)
        {
            ObjectsPoolManager.Remove(_destroyer);
        }
    }

    protected static T Allocate()
    {
        if (objCount == 0)
        {
            //newObjCount++;
            //Debug.Log(typeof(T) + ". New " + newObjCount);
            var newObj = new T();
            newObj.id = nextId++;
            return newObj;
        }
        //Debug.Log(typeof(T) + ". Reused " + objCount + " / " + newObjCount);
        var result = (T) pool[--objCount];
        result.id = nextId++;
        return result;
    }

    public virtual void Release()
    {
        if (objCount >= pool.Length)
        {
            var newPool = new ObjectsPool<T>[(int)(pool.Length * 1.61f)];
            Array.Copy(pool, newPool, pool.Length);
            pool = newPool;
            Debug.Log(typeof(T) + ". Doubled " + objCount);
        }
        for (var i = 0; i < objCount; ++i)
        {
            if (pool[i] == this)
            {
                Debug.LogError(typeof(T) + ". Already released " + objCount + " / " + id);
            }
        }
        pool[objCount++] = this;
    }
}