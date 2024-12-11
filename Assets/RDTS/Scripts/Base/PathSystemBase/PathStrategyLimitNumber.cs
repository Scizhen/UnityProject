using System.Collections.Generic;
using RDTS;
using NaughtyAttributes;
using UnityEngine;

//路径系统中与寻路有关的脚本

public class PathStrategyLimitNumber : PathStrategy
{
    [Header("Settings")]
    public int MaxNumber;

    [ReorderableList] public List<SimulationPath> AreaEnterPathes;
    [ReorderableList] public List<SimulationPath> AreaExitPathes;

    [Header("Status")]
    [RDTS.Utility.ReadOnly] public int CurrentNumber;

    public override void SelectNextPath(PathMover pathMover, ref List<SimulationPath> pathes)
    {
        if (CurrentNumber >= MaxNumber)
        {
            AddBlocked(pathMover);
            pathes.Clear();
        }
    }

    public void OnPathEntered(SimulationPath path, PathMover pathMover)
    {
        CurrentNumber = CurrentNumber + 1;
    }

    public void OnPathExit(SimulationPath path, PathMover pathMover)
    {
        CurrentNumber = CurrentNumber - 1;
        if (CurrentNumber < MaxNumber)
            AwakeBlocked();
    }

    void Start()
    {
        CurrentNumber = 0;
        foreach (var enterpath in AreaEnterPathes)
        {
            enterpath.OnPathEntered.AddListener(OnPathEntered);
            foreach (var pred in enterpath.Predecessors)
            {
                pred.PathStrategy = this;
            }

        }
        foreach (var exitpath in AreaEnterPathes)
        {
            exitpath.OnPathExit.AddListener(OnPathExit);
        }
    }

    void Update()
    {

    }
}
