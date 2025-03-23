using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using System.Threading.Tasks; 

namespace VisualSpline
{
    public class Xml2AGVMap : MonoBehaviour
    {

        public string xmlDocPath;//拓扑图存放路径
        public Spline parentSpline;
        public float scale = 1;
        public int Id = 1;

        private XmlDocument xmlDoc = new XmlDocument();

        // Start is called before the first frame update
        void Start()
        {
            //LoadXmlAndDrawMap();
        }
        private void Update()
        {

            //Debug.Log(name);
        }
        //读取xml文件
        public void LoadXmlAndDrawMap()
        {
            //RemoveAllChildren(parentSpline);
            xmlDoc.Load(xmlDocPath);//加载xml文件

            if (parentSpline == null)
            {
                GameObject parentSplineObj = new GameObject();
                parentSplineObj.name = xmlDoc.Name;
                parentSplineObj.AddComponent<Spline>();
                parentSpline = parentSplineObj.GetComponent<Spline>();
            }

            int id = Id;
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("model").ChildNodes;
            foreach (XmlElement xe in nodeList)
            {//遍历所有子节点
                if (xe.Name == "point")
                {
                    string name = xe.GetAttribute("name");
                    XmlNode pointLayout = xe.SelectSingleNode("pointLayout");
                    float xPosition = float.Parse(pointLayout.Attributes["positionX"].Value)/scale;//opentcs最新版的位置信息为positionX而不是xPosition
                    float yPosition = float.Parse(pointLayout.Attributes["positionY"].Value) /scale;
                    SplinePoint splinePoint = new SplinePoint();
                    splinePoint.AddSplinePointAsXml(id,name,xPosition,yPosition,parentSpline);
                    id = id + 1;
                }       
            }

            //添加关联点信息
            foreach (XmlElement xe in nodeList)
            {//遍历所有子节点
                if (xe.Name == "point")
                {
                    XmlNodeList pointNodeList = xe.SelectNodes("outgoingPath");
                    foreach (XmlElement xp in pointNodeList)
                    {
                        string outgoingPath = xp.GetAttribute("name");//获得关联点
                        string[] sArry = outgoingPath.Split(" --- ");
                        string point1Name = sArry[0];//outgoingPath.Substring(0,3);//获得第一个点
                        string point2Name = sArry[1];//outgoingPath.Substring(7, 9);//获得第二个点
                        SplinePoint point1 =  parentSpline.transform.Find(point1Name).GetComponent<SplinePoint>();//通过名称找到parentSpline下的第一个点
                        SplinePoint point2 = parentSpline.transform.Find(point2Name).GetComponent<SplinePoint>();//通过名称找到parentSpline下的第一个点
                        point1.AddConnectedPoint(point2);
                        point2.AddConnectedPoint(point1);

                    }
                }
            }

            Invoke("SetPathType", 0.5f);

        }

        //设置线类型
        void SetPathType()
        {
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("model").ChildNodes;
            //设置每条线的类型和贝塞尔曲线关键点
            foreach (XmlElement xe in nodeList)
            {//遍历所有子节点
                if (xe.Name == "path")
                {
                    string sourcePointName = xe.GetAttribute("sourcePoint");
                    string destinationPointName = xe.GetAttribute("destinationPoint");
                    SplinePoint sourcePoint = parentSpline.transform.Find(sourcePointName).GetComponent<SplinePoint>();//通过名称找到parentSpline下的第一个点
                    SplinePoint destinationPoint = parentSpline.transform.Find(destinationPointName).GetComponent<SplinePoint>();//通过名称找到parentSpline下的第一个点
                    Debug.Log(parentSpline.lines.Count);
                    Line line = parentSpline.GetLineByPoints(sourcePoint, destinationPoint, parentSpline.lines);//通过点名称找到该线

                    line.point1 = sourcePoint;
                    line.point2 = destinationPoint;
                    XmlNode pathLayout = xe.SelectSingleNode("pathLayout");//找到pathLayout这层
                    string connectionType = pathLayout.Attributes["connectionType"].Value;//获得connectionType的值
                    if (line is not null)
                    {
                        if (connectionType == "BEZIER")
                        {
                            line.lineType = LineType.Bezier;
                            XmlNodeList controlPoint = pathLayout.SelectNodes("controlPoint");
                            int controlPointCount = 0;
                            foreach (XmlElement xp in controlPoint)
                            {
                                GameObject tangentPointTemplate = Resources.Load("TangentPoint") as GameObject;//从resource文件夹中加载TangentPoint
                                if (tangentPointTemplate == null) return;


                                float controlPointx = 50 * (float.Parse(xp.GetAttribute("x")) / scale);//获得贝塞尔控制点的x和y值
                                float controlPointy = -50 * (float.Parse(xp.GetAttribute("y")) / scale);

                                if (controlPointCount == 0)
                                {
                                    GameObject controlPoint1 = Instantiate<GameObject>(tangentPointTemplate,sourcePoint.transform);
                                    controlPoint1.name = sourcePointName.Substring(0,3) +"-"+ destinationPointName.Substring(0,3) + "_SourcePoint";
                                    controlPoint1.transform.position = new Vector3(controlPointx, 0f, controlPointy);
                                    line.sourceControl = controlPoint1.GetComponent<SplineTangent>();
                                    sourcePoint.OutTangent.position = new Vector3(controlPointx,0f,controlPointy);
                                }
                                if (controlPointCount == 1)
                                {
                                    GameObject controlPoint2 = Instantiate<GameObject>(tangentPointTemplate, destinationPoint.transform);
                                    controlPoint2.name = sourcePointName.Substring(0, 3) + "-" + destinationPointName.Substring(0, 3) + "_DestinationPoint";
                                    controlPoint2.transform.position = new Vector3(controlPointx, 0f, controlPointy);
                                    line.destinationControl = controlPoint2.GetComponent<SplineTangent>();
                                    destinationPoint.InTangent.position = new Vector3(controlPointx, 0f, controlPointy);
                                }
                                controlPointCount ++ ;
                            }
                        }
                        if (connectionType == "DIRECT")
                        {
                            line.lineType = LineType.Straight;
                        }
                    }

                }
            }
        }


        public void RemoveAllPoint()
        {
            parentSpline.ClearLines();
            Transform transform;
            int pointCount = parentSpline.transform.childCount;
            //List<GameObject> childPoint = new List<GameObject>();
            for (int i = 0 ; i < pointCount; i++)
            {
                transform = parentSpline.transform.GetChild(0);
                GameObject.DestroyImmediate(transform.gameObject);
            }
        }

    }
}

