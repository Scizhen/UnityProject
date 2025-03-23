using ExcelDataReader;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

namespace RDTS
{
    public class ExcelReader : MonoBehaviour
    {
        public string path;
        public string sheet;


        [Button("Read Excel")]
        public void ButtonReadExcel()
        {
            var excelRowData = ReadExcelRows(path, sheet);
            for (int i = 0; i < excelRowData.Count; i++)
            {
                Debug.Log(excelRowData[i][0]);
            }

        }

        /// <summary>
        /// ��ȡ����·���µ�Excel�ļ�
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static DataSet ReadExcel(string excelPath)
        {
            using (FileStream fileStream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);

                var result = excelReader.AsDataSet();

                return result;
            }

        }

        /// <summary>
        /// ��ȡ����·���µ�Excel�ļ��ı���Ϣ
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static DataTableCollection ReadExcelTables(string excelPath)
        {
            using (FileStream fileStream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);

                var result = excelReader.AsDataSet();

                return result.Tables;
            }

        }

        /// <summary>
        /// ��ȡ����·�������µ�Excel�ļ���ÿһ�е�����
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="excelSheet"></param>
        /// <returns></returns>
        public static DataRowCollection ReadExcelRows(string excelPath, string excelSheet)
        {
            using (FileStream fileStream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);

                var result = excelReader.AsDataSet();

                return result.Tables[excelSheet].Rows;
            }

        }
    }
}