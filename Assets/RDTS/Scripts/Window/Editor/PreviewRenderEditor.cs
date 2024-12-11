using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace RDTS.Window
{
    /// <summary>
    ///模型的三维预览窗口
    /// </summary>
    public class PreviewRenderEditor : Editor
    {
        private static PreviewRenderUtility _previewRenderUtility;
        private GameObject _previewInstance;
        private GameObject _targetObj;
        private static bool _loaded = true;
        private Vector2 _drag = new Vector2(250f, -30f);
        //private Vector3 _translate = new Vector3(-3.26f, 2.60f, 1.18f);
        private DragAndTranslate dragAndTranslate = new DragAndTranslate() {
            scrollPosition = new Vector2(250f, -30f),
            translatePosition = new Vector3(-3.26f, 2.60f, 1.18f)
        };
        private Vector2 _lightRot = new Vector2(180f, 0);
       

        ///static Transform _position;

        public void RefreshLightRot(Vector2 rot)
        {
            _lightRot = rot;
        }

        public void RefreshPreviewInstance(GameObject obj)
        {
            _targetObj = obj;
            if (_previewInstance)
                UnityEngine.Object.DestroyImmediate(_previewInstance);

            _previewInstance = null;
            _loaded = true;
        }


        private void OnEnable()
        {
            if (_previewRenderUtility == null)
            {
                _previewRenderUtility = new PreviewRenderUtility();
            }
           
        }


        private void OnDisable()
        {
            Reset();
        }


        public void Reset()
        {
            if (_previewRenderUtility != null)
            {
                // 必须进行清理，否则会存在残留对象
                _previewInstance = null;
                _previewRenderUtility.Cleanup();
                _previewRenderUtility = null;
            }
        }


        //设置缩放值
        protected static float _zoomRate = 1f;
        public void SetZoomRateValue(float value = 1f)
        {
            _zoomRate = value;
        }

        //获取缩放值
        public float GetZoomRateValue()
        {
            return 1/_zoomRate;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            // _loaded	确保只加载一次物体
            if (_loaded && _targetObj)
            {
                _previewInstance = Instantiate(_targetObj as GameObject, Vector3.zero, Quaternion.identity);
                // AddSingleGO 添加单个物体
                _previewRenderUtility.AddSingleGO(_previewInstance);
                _loaded = false;
            }

            // 获取拖拽向量
            //_translate = Translate2D(_translate, r);
            //_drag = Drag2D(_drag, r);
            dragAndTranslate = DragAndTranslate2D(dragAndTranslate, r);


            // 事件为绘制时，才进行绘制
            if (Event.current.type == EventType.Repaint)
            {
                _previewRenderUtility.BeginPreview(r, background);

                //调整相机位置与角度
                Camera camera = _previewRenderUtility.camera;
                var cameraTran = camera.transform;
                cameraTran.position = Vector2.zero;
                cameraTran.rotation = Quaternion.Euler(new Vector3(-dragAndTranslate.scrollPosition.y, -dragAndTranslate.scrollPosition.x, 0));
                
                cameraTran.position = cameraTran.forward * -4f * _zoomRate;
                var pos = cameraTran.position;
                cameraTran.position = new Vector3(pos.x, pos.y + 0.6f, pos.z);
                ///_previewInstance.transform.position = dragAndTranslate.translatePosition;//调整相机位置效果不行，如何改进？=> 直接在模型文件中处理好坐标


                EditorUtility.SetCameraAnimateMaterials(camera, true);//?

                camera.cameraType = CameraType.Preview;//用于指示在 Editor 中渲染预览的摄像机
                camera.enabled = false;
                //camera.clearFlags = CameraClearFlags.Skybox;//天空盒
                camera.clearFlags = CameraClearFlags.Nothing;
                camera.fieldOfView = 40;
                camera.farClipPlane = 500.0f;
                camera.nearClipPlane = 0.1f;
                //camera.backgroundColor = new Color(49.0f / 255.0f, 77.0f / 255.0f, 121.0f / 255.0f, 0f);//淡蓝色背景
                camera.backgroundColor = new Color(20.0f / 255.0f, 20.0f / 255.0f, 20.0f / 255.0f, 0f);//浅灰色背景

                // 设置光源数据
                _previewRenderUtility.lights[0].intensity = 0.7f;
                _previewRenderUtility.lights[0].transform.rotation = Quaternion.Euler(_lightRot.x, _lightRot.y, 0f);
                _previewRenderUtility.lights[1].intensity = 0.7f;
                _previewRenderUtility.lights[1].transform.rotation = Quaternion.Euler(_lightRot.x, _lightRot.y, 0f);
                _previewRenderUtility.ambientColor = new Color(0.3f, 0.3f, 0.3f, 0f);

                //camera.transform.LookAt(_previewInstance.transform);
                // 相机渲染
                camera.Render();
                // 结束并绘制
                _previewRenderUtility.EndAndDrawPreview(r);
            }
        }

        // Drag2D 来自源码
        private static int sliderHash = "Slider".GetHashCode();

        private static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            // 每次获得独一无二的 controlID
            int controlID = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
            Event current = Event.current;
            // 获取对应 controlID 的事件
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    {
                        bool flag = position.Contains(current.mousePosition) && position.width > 50f;
                        if (flag)
                        { 
                            // 鼠标摁住拖出预览窗口外，预览物体任然能够旋转
                            GUIUtility.hotControl = controlID;
                            // 采用事件
                            current.Use();
                            // 让鼠标可以拖动到屏幕外后，从另一边出来
                            EditorGUIUtility.SetWantsMouseJumping(1);
                        }

                        break;
                    }
                case EventType.MouseUp:
                    {
                        bool flag2 = GUIUtility.hotControl == controlID;
                        if (flag2)
                        {
                            GUIUtility.hotControl = 0;
                        }

                        EditorGUIUtility.SetWantsMouseJumping(0);
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        bool flag3 = GUIUtility.hotControl == controlID;
                        if (flag3)
                        {
                            if(current.button == 0 || current.button == 1)
                            {
                                // shift 加速
                                scrollPosition -= current.delta * (float)(current.shift ? 3 : 1) /
                                                  Mathf.Min(position.width, position.height) * 140f;
                                // 以下两条缺少任意一个，会导致延迟更新,拖动过程中无法实时更新
                                // 直到 repaint事件触发才重新绘制
                                current.Use();
                                GUI.changed = true;
                            }


                        }

                        break;
                    }
                case EventType.ScrollWheel:///鼠标滚轮调整相机距离 —> 实现缩放效果
                    {
                        bool flag4 = position.Contains(current.mousePosition);
                        if (flag4)
                        {
                            //Debug.Log("e.delta：" + current.delta);
                            var delta = current.delta.y;
                            _zoomRate += delta * Time.deltaTime * 0.1f;

                            //限幅
                            if (_zoomRate >= 10f)
                                _zoomRate = 10f;
                            if (_zoomRate <= 0.2f)
                                _zoomRate = 0.2f;

                            current.Use();
                            GUI.changed = true;
                        }

                        ///Debug.Log("ScrollWheel：" + _zoomRate);
                        break;
                    }
            }

            return scrollPosition;
        }



        private static Vector3 Translate2D(Vector3 translatePosition, Rect position)
        {
            // 每次获得独一无二的 controlID
            int controlID = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
            Event current = Event.current;

            //var currentTransform = _previewRenderUtility.camera.transform;
            //var currentPos = translatePosition;
            //Vector3 targetPos = currentPos;
            // 获取对应 controlID 的事件
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    {
                        bool flag = position.Contains(current.mousePosition) && position.width > 50f;
                        if (flag)
                        {
                            // 鼠标摁住拖出预览窗口外，预览物体任然能够旋转
                            GUIUtility.hotControl = controlID;
                            // 采用事件
                            current.Use();
                            // 让鼠标可以拖动到屏幕外后，从另一边出来
                            EditorGUIUtility.SetWantsMouseJumping(1);
                        }

                        break;
                    }
                case EventType.MouseUp:
                    {
                        bool flag2 = GUIUtility.hotControl == controlID;
                        if (flag2)
                        {
                            GUIUtility.hotControl = 0;
                        }

                        EditorGUIUtility.SetWantsMouseJumping(0);
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        bool flag3 = GUIUtility.hotControl == controlID;
                        if (flag3)
                        {
                          
                            if (current.button == 0)
                            {
                                var currentTransform = _previewRenderUtility.camera.transform;
                                var currentPos = translatePosition;
                                Vector3 targetPos = currentPos;
                                var dxdy = currentTransform.right * current.delta.x * Time.deltaTime * 0.4f + currentTransform.up * -current.delta.y * Time.deltaTime * 0.4f;
                                targetPos = currentPos - dxdy * 1f;
                                translatePosition = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 0.1f);
                                //Debug.Log("MouseDrag");
                                //Debug.Log($"Mouse X：{current.delta.x},Mouse Y：{current.delta.y}");
                                //Debug.Log($"translatePosition X：{translatePosition.x},translatePosition Y：{translatePosition.y}");

                                // translatePosition = targetPos;
                            }

                        }

                        break;
                    }
            }

           // translatePosition = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 0.05f);
            return translatePosition;



        }


        
        /// <summary>
        /// 用此方法替代Drag2D和Translate2D方法，同时获取鼠标事件信息。【但是平移Translate效果不好，而且没必要，应当在模型文件中实现处理好模型坐标位置】
        /// </summary>
        /// <param name="dragAndTranslate"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private DragAndTranslate DragAndTranslate2D(DragAndTranslate dragAndTranslate, Rect r)
        {
            // 每次获得独一无二的 controlID
            int controlID = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
            Event current = Event.current;
            // 获取对应 controlID 的事件
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    {
                        bool flag = r.Contains(current.mousePosition) && r.width > 50f;
                        if (flag)
                        {
                           
                            // 鼠标摁住拖出预览窗口外，预览物体任然能够旋转
                            GUIUtility.hotControl = controlID;
                            // 采用事件
                            current.Use();
                            // 让鼠标可以拖动到屏幕外后，从另一边出来
                            EditorGUIUtility.SetWantsMouseJumping(1);
                        }

                        break;
                    }
                case EventType.MouseUp:
                    {
                        bool flag2 = GUIUtility.hotControl == controlID;
                        if (flag2)
                        {
                            GUIUtility.hotControl = 0;
                        }

                        EditorGUIUtility.SetWantsMouseJumping(0);
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        bool flag3 = GUIUtility.hotControl == controlID;
                        if (flag3)
                        {
                            //左/右键旋转
                            if (current.button == 0 || current.button == 1)
                            {
                                // shift 加速
                                dragAndTranslate.scrollPosition -= current.delta * (float)(current.shift ? 3 : 1) /
                                                  Mathf.Min(r.width, r.height) * 140f;      
                            }

                            //中建平移
                            /*
                            if (current.button == 2)
                            {
                                var currentTransform = _previewRenderUtility.camera.transform;
                                var currentPos = dragAndTranslate.translatePosition;
                                Vector3 targetPos = currentPos;
                                var dxdy = currentTransform.right * current.delta.x * Time.deltaTime * 0.4f + currentTransform.up * -current.delta.y * Time.deltaTime * 0.4f;
                                targetPos = currentPos - dxdy * 1f;
                                dragAndTranslate.translatePosition = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 0.1f);
                                //Debug.Log("MouseDrag");
                            }
                            */

                            // 以下两条缺少任意一个，会导致延迟更新,拖动过程中无法实时更新
                            // 直到 repaint事件触发才重新绘制
                            current.Use();
                            GUI.changed = true;
                        }

                        break;
                    }
                case EventType.ScrollWheel:///鼠标滚轮调整相机距离 —> 实现缩放效果
                    {
                        bool flag4 = r.Contains(current.mousePosition);
                        if (flag4)
                        {
                            //Debug.Log("e.delta：" + current.delta);
                            var delta = current.delta.y;
                            _zoomRate += delta * Time.deltaTime * Mathf.Abs(_zoomRate) * 0.1f;

                            //限幅
                            if (_zoomRate >= 10f)
                                _zoomRate = 10f;
                            if (_zoomRate <= 0.2f)
                                _zoomRate = 0.2f;

                            current.Use();
                            GUI.changed = true;
                        }

                        ///Debug.Log("ScrollWheel：" + _zoomRate);
                        break;
                    }
            }

            return dragAndTranslate;
        }


        public class DragAndTranslate
        {
            public Vector2 scrollPosition;
            public Vector3 translatePosition;
        }

    }


}