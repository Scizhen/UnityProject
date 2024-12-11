
namespace RDTS
{
    public interface IWorkstationProcess
    {
        //�ӿ�������Workstation����ķ���
        bool AllowEntry(WorkStation station, PathMover pathMover);

        bool AllowStart(WorkStation station, PathMover pathMover);

        void OnFailureStart(WorkStation station);

        void OnFailureEnd(WorkStation station);

        void OnStart(WorkStation station, float time);

        void OnEnd(WorkStation station, PathMover pathMover);

        void OnExit(WorkStation station, PathMover pathMover);
    }
}

