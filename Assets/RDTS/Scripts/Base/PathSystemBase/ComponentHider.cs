
using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

namespace RDTS
{
    //用于对组件的隐藏
    public class ComponentHider : MonoBehaviour
    {
        [OnValueChanged("Changed")] public bool Hide = true;

        [HideIf("Hide")] public bool HideAll;
        [HideIf("Hide")] [ReorderableList] public List<Component> Components = new List<Component>();

        private void Changed()
        {
#if UNITY_EDITOR
            HideFlags flag = HideFlags.None;
            
            if (Hide)
                flag = HideFlags.HideInInspector;


            if (HideAll)
            {
                var comps = GetComponents<Component>();
                foreach (var component in comps)
                {
                    component.hideFlags = flag;
                    EditorUtility.SetDirty(component);
                }

                return;
            }
            foreach (var component in Components)
            {
                component.hideFlags = flag;
                EditorUtility.SetDirty(component);
            }
            
 #endif     
        }

        public void HideComponents(bool flag)
        {
            Hide = flag;
            Changed();
        }

        //public void OnDrawGizmosSelected(bool hasFocus)
        //{
        //    if (hasFocus)
        //    {
        //        Changed();
        //    }
        //}
    }
}

