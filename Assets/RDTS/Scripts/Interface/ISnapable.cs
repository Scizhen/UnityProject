using UnityEngine;

namespace RDTS
{
    //! Interface to define the needed methods for snap points
    //�ӿ�������snap points����ķ���
    public interface ISnapable
    {
        public void Connect(SnapPoint ownSnapPoint, SnapPoint snapPointMate, ISnapable mateObject, bool ismoved);

        public void Disconnect(SnapPoint snapPoint, SnapPoint snapPointMate, ISnapable Mateobj, bool ismoved);
    }
}