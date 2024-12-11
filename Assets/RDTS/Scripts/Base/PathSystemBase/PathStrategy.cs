using System.Collections;
using System.Collections.Generic;
using RDTS;
using UnityEngine;


//路径系统中与寻路有关的基类脚本
public abstract class PathStrategy : RDTSBehavior, ISelectNextPath
{
    public abstract void SelectNextPath(PathMover pathMover, ref List<SimulationPath> Pathes);

    private List<PathMover> Blocked = new List<PathMover>();

    protected void AddBlocked(PathMover pathMover)
    {
        if (!Blocked.Contains(pathMover))
            Blocked.Add(pathMover);
    }

    protected new void Awake()
    {
        foreach (var transportable in Blocked.ToArray())
        {
            if (transportable.TryMoveNext())
            {
                Blocked.Remove(transportable);
                return;
            }
        }
        base.Awake();
    }
    protected void AwakeBlocked()
    {
        Invoke("Awake", 0.01f);
    }
}
