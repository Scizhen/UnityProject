using UnityEngine;
using UnityEditor;
using NaughtyAttributes.Editor;

namespace RDTS
{
    //路径系统中的编辑器类脚本
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LibraryObject), true)]
    public class LibraryObjectEditor : NaughtyInspector
    {
        private GameObject draggedObj;
        private LibraryObject behaviour;
        private bool mouseHold;


        private void OnSceneGUI()
        {
            behaviour = (LibraryObject) target;

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                {
                    behaviour.OnKeyPressed(Event.current.keyCode);
                    break;
                }
            }

            
            if (Event.current.type == EventType.DragExited)
            {
                draggedObj = Selection.activeGameObject;
                if (draggedObj != null)
                {
                    behaviour.IsEditable = false;
                    behaviour.ShowInspector(draggedObj,behaviour.IsEditable);
                    behaviour.ShowChildren(behaviour.IsEditable);
                    //draggedObj.SetLocalPositionY(behaviour.GetHeight());
                    behaviour.Modify();
                }

                // Util.RefreshHierarchy();
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && !Event.current.alt)
            {
                mouseHold = true;
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                mouseHold = false;
            }

            if (mouseHold)
            {
                foreach (Transform trans in Selection.transforms)
                {
                    foreach (SnapPoint snap in trans.GetComponentsInChildren<SnapPoint>())
                    {
                        if (!snap.snapped)
                        {
                            snap.CheckSnap();
                        }
                    }
                }
            }
        }
    }
}