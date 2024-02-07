using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VfxManager : MonoBehaviour
{

    [SerializeField] private List<ColorShapeVfx> colorShapeVfx;
    [SerializeField] private List<SpecialShapeVfx> specialShapeVfx;

    private Dictionary<string, VfxInfo> vfxDict;
    private Dictionary<string, List<PoolItem>> pools;

    private void Awake()
    {
        vfxDict = new Dictionary<string, VfxInfo>();
        pools = new Dictionary<string, List<PoolItem>>();

        foreach (ColorShapeVfx colorShape in colorShapeVfx)
        {
            vfxDict[colorShape.Type.ToString()] = colorShape.Vfx;
            pools[colorShape.Type.ToString()] = new List<PoolItem>
            {
                new PoolItem(Instantiate(colorShape.Vfx.Prefab, transform), colorShape.Vfx.DisplayDuration, colorShape.Vfx.DisplayColor, colorShape.Vfx.ApplyColor)
            };
        }

        foreach (SpecialShapeVfx specialShape in specialShapeVfx)
        {
            vfxDict[specialShape.Type.ToString()] = specialShape.Vfx;
            pools[specialShape.Type.ToString()] = new List<PoolItem>
            {
                new PoolItem(Instantiate(specialShape.Vfx.Prefab, transform), specialShape.Vfx.DisplayDuration, specialShape.Vfx.DisplayColor, specialShape.Vfx.ApplyColor)
            };
        }
    }

    public void OnShapeDestroyed(string shapeName, Vector3 position)
    {
        if (!pools.ContainsKey(shapeName))
        {
            Debug.Log(shapeName + " Vfx Does Not Exists");
            return;
        }

        var item = pools[shapeName].Find(i => !i.GameObject.activeInHierarchy);

        if (item == null)
        {
            var vfx = vfxDict[shapeName];
            item = new PoolItem(Instantiate(vfx.Prefab, transform), vfx.DisplayDuration, vfx.DisplayColor, vfx.ApplyColor);
            pools[shapeName].Add(item);
        }

        item.SetUp(position);
        StartCoroutine(ReturnToPool(item));
    }

    private IEnumerator ReturnToPool(PoolItem item)
    {
        yield return new WaitForSeconds(item.TimeToReturn); 
        item.Reset();
    }
}

public class PoolItem
{
    public GameObject GameObject;
    public float TimeToReturn;
    public ParticleSystem[] ParticleSystems;

    public PoolItem(GameObject gameObject, float timeToReturn, Color color, bool applyColor)
    {
        GameObject = gameObject;
        TimeToReturn = timeToReturn;

        if (applyColor)
        {
            ParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in ParticleSystems)
                ps.startColor = color;
        }

        GameObject.SetActive(false);
    }

    public void Reset()
    {
        GameObject.SetActive(false);
        if (ParticleSystems != null)
        {
            foreach (var ps in ParticleSystems)
            {
                ps.Stop();
                ps.Clear();
            }
        }
    }

    public void SetUp(Vector3 position)
    {
        GameObject.SetActive(true);
        GameObject.transform.position = position;

        if (ParticleSystems != null)
        {
            foreach (var ps in ParticleSystems)
                ps.Play();
        }
    }

}

[System.Serializable]
public struct ColorShapeVfx
{
    public ShapeType Type;
    public VfxInfo Vfx;
}

[System.Serializable]
public struct SpecialShapeVfx
{
    public SpecialGridPiece Type;
    public VfxInfo Vfx;
}

[System.Serializable]
public class VfxInfo
{
    public float DisplayDuration;
    public GameObject Prefab;
    public Color DisplayColor;
    public bool ApplyColor;
}