using UnityEngine;
using UnityEditor;

namespace RDTS
{
    [CustomEditor(typeof(SimulationPath), true)]
    public class LineHandle : NaughtyAttributes.Editor.NaughtyInspector
    { 
        private GameObject draggedObj;
        private LibraryObject behaviour;
        private bool mouseHold;
        protected virtual  void OnSceneGUI()
        {
            
            SimulationPath path = (SimulationPath)target;
           
            if (Event.current.type == EventType.DragExited)
            {
                draggedObj = Selection.activeGameObject;
            /*    if (draggedObj != null)
                {
                    behaviour.IsEditable = false;
                    behaviour.ShowInspector(draggedObj,behaviour.IsEditable);
                    behaviour.ShowChildren(behaviour.IsEditable);
                    //draggedObj.SetLocalPositionY(behaviour.GetHeight());
                    behaviour.Modify();
                }

                // Util.RefreshHierarchy(); */
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
                path.CheckSnapping();
                /*  foreach (Transform trans in Selection.transforms)
                  {
                      foreach (SnapPoint snap in trans.GetComponentsInChildren<SnapPoint>())
                      {
                          if (!snap.snapped)
                          {
                              snap.CheckSnap();
                          }
                      }
                  }*/
            }
        }
    }

}

