using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace VisualSpline
{
    [Serializable]
    public class LineDetail
    {
        //public int currentLine;//当前所在的线条
        public bool isExist;//此线条是否存在于运动轨迹中
        public int index;//线条在运动轨迹中的索引
        [Range(0, 1)] public float percentage;//占当前所在的线条的百分比（小数形式）
        public bool isArrived = false;//是否走完该线段
        public SplinePoint startPoint;//线条的起始点
        public SplinePoint endPoint;//线条的结束点

        public LineDetail(int currentLine, float currentPercentage, SplinePoint startPoint, SplinePoint endPoint)
        {
            //this.currentLine = currentLine;
            this.percentage = currentPercentage;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        public void SetValue(int currentLine, float currentPercentage, SplinePoint startPoint, SplinePoint endPoint)
        {
            //this.currentLine = currentLine;
            this.percentage = currentPercentage;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }





        public void SetIndex(int index)
        {
            this.index = index;
        }


        public void SetPercentage(float percentage)
        {
            this.percentage = percentage;
        }

        public void SetPoints(SplinePoint startPoint, SplinePoint endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }



    }


    /// <summary>
    /// 线条重新参数化：记录线条的[ 长度，所占总长度的百分比 ] 
    /// </summary>
    public class SplineReparam
    {
        public float length;
        public float percentage;

        public SplineReparam(float length, float percentage)
        {
            this.length = length;
            this.percentage = percentage;
        }
    }


    /// <summary>
    /// 贝塞尔曲线
    /// </summary>
    public static class BezierCurves
    {
        //Quadratic Bezier: 二次贝塞尔曲线
        public static Vector3 GetPoint(Vector3 startPosition, Vector3 controlPoint, Vector3 endPosition, float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            float oneMinusT = 1f - percentage;
            return oneMinusT * oneMinusT * startPosition + 2f * oneMinusT * percentage * controlPoint + percentage * percentage * endPosition;
        }

        /// <summary>
        /// 获取一阶导数
        /// </summary>
        public static Vector3 GetFirstDerivative(Vector3 startPoint, Vector3 controlPoint, Vector3 endPosition, float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            return 2f * (1f - percentage) * (controlPoint - startPoint) + 2f * percentage * (endPosition - controlPoint);
        }

        //Cubic Bezier: 三次贝塞尔曲线
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="startTangent"></param>
        /// <param name="endTangent"></param>
        /// <param name="percentage"></param>
        /// <param name="evenDistribution">均匀分布</param>
        /// <param name="distributionSteps">分配步骤</param>
        /// <returns></returns>
        public static Vector3 GetPoint(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, float percentage, bool evenDistribution, int distributionSteps)
        {
            if (evenDistribution)
            {
                int maxPoint = distributionSteps + 1;
                float[] arcLengths = new float[maxPoint];
                Vector3 previousPoint = Locate(startPosition, endPosition, startTangent, endTangent, 0);
                float sum = 0;

                //store arc lengths: 存储弧长：
                for (int i = 1; i < maxPoint; i++)
                {
                    Vector3 p = Locate(startPosition, endPosition, startTangent, endTangent, i / (float)maxPoint);
                    sum += Vector3.Distance(previousPoint, p);
                    arcLengths[i] = sum;
                    previousPoint = p;
                }

                float targetLength = percentage * arcLengths[distributionSteps];

                //找到percentage对应的索引点（index的值）
                //search:
                int low = 0;
                int high = distributionSteps;
                int index = 0;
                while (low < high)
                {
                    index = low + (((high - low) / 2) | 0);
                    if (arcLengths[index] < targetLength)
                    {
                        low = index + 1;

                    }
                    else
                    {
                        high = index;
                    }
                }

                //adjust:
                if (arcLengths[index] > targetLength)
                {
                    index--;
                }

                float lengthBefore = arcLengths[index];//索引点前的长度

                //interpolate or use as is: 插值或按原样使用：
                if (lengthBefore == targetLength)
                {
                    return Locate(startPosition, endPosition, startTangent, endTangent, index / distributionSteps);
                }
                else
                {
                    return Locate(startPosition, endPosition, startTangent, endTangent, (index + (targetLength - lengthBefore) / (arcLengths[index + 1] - lengthBefore)) / distributionSteps);
                }
            }

            return Locate(startPosition, endPosition, startTangent, endTangent, percentage);
        }

        public static Vector3 GetFirstDerivative(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            float oneMinusT = 1f - percentage;
            return 3f * oneMinusT * oneMinusT * (startTangent - startPosition) + 6f * oneMinusT * percentage * (endTangent - startTangent) + 3f * percentage * percentage * (endPosition - endTangent);
        }

        private static Vector3 Locate(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            float oneMinusT = 1f - percentage;
            return oneMinusT * oneMinusT * oneMinusT * startPosition + 3f * oneMinusT * oneMinusT * percentage * startTangent + 3f * oneMinusT * percentage * percentage * endTangent + percentage * percentage * percentage * endPosition;
        }
    }


    /// <summary>
    /// 直线
    /// </summary>
    public static class StraightLines
    {
        public static Vector3 GetDirection(Vector3 startPosition, Vector3 endPosition)
        {
            return endPosition - startPosition;
        }

        public static Vector3 GetPoint(Vector3 startPosition, Vector3 endPosition, float percentage)
        {
            return startPosition + (endPosition - startPosition) * percentage;
        }


    }

}
