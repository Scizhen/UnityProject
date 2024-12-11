using UnityEngine;

namespace RDTS
{
    //! Interface to define the needed methods for snap points
    //接口来定义snap points所需的方法
    public interface ISnapable
    {
        public void Connect(SnapPoint ownSnapPoint, SnapPoint snapPointMate, ISnapable mateObject, bool ismoved);

        public void Disconnect(SnapPoint snapPoint, SnapPoint snapPointMate, ISnapable Mateobj, bool ismoved);
    }
}