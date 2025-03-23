

namespace RDTS
{
    //指示2个可序列化的事件
    [System.Serializable]
    
    public class AreaEnterEvent : UnityEngine.Events.UnityEvent<Area, PathMover> {}
    public class AreaExitEvent : UnityEngine.Events.UnityEvent<Area, PathMover> {}
}

