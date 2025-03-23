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

        public string xmlDocPath;//����ͼ���·��
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
        //��ȡxml�ļ�
        public void LoadXmlAndDrawMap()
        {
            //RemoveAllChildren(parentSpline);
            xmlDoc.Load(xmlDocPath);//����xml�ļ�

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
            {//���������ӽڵ�
                if (xe.Name == "point")
                {
                    string name = xe.GetAttribute("name");
                    XmlNode pointLayout = xe.SelectSingleNode("pointLayout");
                    float xPosition = float.Parse(pointLayout.Attributes["positionX"].Value)/scale;//opentcs���°��λ����ϢΪpositionX������xPosition
                    float yPosition = float.Parse(pointLayout.Attributes["positionY"].Value) /scale;
                    SplinePoint splinePoint = new SplinePoint();
                    splinePoint.AddSplinePointAsXml(id,name,xPosition,yPosition,parentSpline);
                    id = id + 1;
                }       
            }

            //��ӹ�������Ϣ
            foreach (XmlElement xe in nodeList)
            {//���������ӽڵ�
                if (xe.Name == "point")
                {
                    XmlNodeList pointNodeList = xe.SelectNodes("outgoingPath");
                    foreach (XmlElement xp in pointNodeList)
                    {
                        string outgoingPath = xp.GetAttribute("name");//��ù�����
                        string[] sArry = outgoingPath.Split(" --- ");
                        string point1Name = sArry[0];//outgoingPath.Substring(0,3);//��õ�һ����
                        string point2Name = sArry[1];//outgoingPath.Substring(7, 9);//��õڶ�����
                        SplinePoint point1 =  parentSpline.transform.Find(point1Name).GetComponent<SplinePoint>();//ͨ�������ҵ�parentSpline�µĵ�һ����
                        SplinePoint point2 = parentSpline.transform.Find(point2Name).GetComponent<SplinePoint>();//ͨ�������ҵ�parentSpline�µĵ�һ����
                        point1.AddConnectedPoint(point2);
                        point2.AddConnectedPoint(point1);

                    }
                }
            }

            Invoke("SetPathType", 0.5f);

        }

        //����������
        void SetPathType()
        {
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("model").ChildNodes;
            //����ÿ���ߵ����ͺͱ��������߹ؼ���
            foreach (XmlElement xe in nodeList)
            {//���������ӽڵ�
                if (xe.Name == "path")
                {
                    string sourcePointName = xe.GetAttribute("sourcePoint");
                    string destinationPointName = xe.GetAttribute("destinationPoint");
                    SplinePoint sourcePoint = parentSpline.transform.Find(sourcePointName).GetComponent<SplinePoint>();//ͨ�������ҵ�parentSpline�µĵ�һ����
                    SplinePoint destinationPoint = parentSpline.transform.Find(destinationPointName).GetComponent<SplinePoint>();//ͨ�������ҵ�parentSpline�µĵ�һ����
                    Debug.Log(parentSpline.lines.Count);
                    Line line = parentSpline.GetLineByPoints(sourcePoint, destinationPoint, parentSpline.lines);//ͨ���������ҵ�����

                    line.point1 = sourcePoint;
                    line.point2 = destinationPoint;
                    XmlNode pathLayout = xe.SelectSingleNode("pathLayout");//�ҵ�pathLayout���
                    string connectionType = pathLayout.Attributes["connectionType"].Value;//���connectionType��ֵ
                    if (line is not null)
                    {
                        if (connectionType == "BEZIER")
                        {
                            line.lineType = LineType.Bezier;
                            XmlNodeList controlPoint = pathLayout.SelectNodes("controlPoint");
                            int controlPointCount = 0;
                            foreach (XmlElement xp in controlPoint)
                            {
                                GameObject tangentPointTemplate = Resources.Load("TangentPoint") as GameObject;//��resource�ļ����м���TangentPoint
                                if (tangentPointTemplate == null) return;


                                float controlPointx = 50 * (float.Parse(xp.GetAttribute("x")) / scale);//��ñ��������Ƶ��x��yֵ
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

