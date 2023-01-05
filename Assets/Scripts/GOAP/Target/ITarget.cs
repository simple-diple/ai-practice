using UnityEngine;

namespace GOAP.Target
{
    public interface ITarget
    {
        Vector2 Advance(ref float deltaDist, ref Vector3 dir);
        float Distance { get; }
        float StartDistance { get; }
        void Release();
        Vector3 StartDirection { get; }
        void UpdateUnitPath();
    }
}