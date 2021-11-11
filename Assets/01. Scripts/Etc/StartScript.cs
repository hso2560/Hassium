using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

public class StartScript : MonoBehaviour
{
    public Material[] skyMaterials;
    public Light _light;
    public Camera mainCam;

    public int maxIndex = 3;

    private readonly string weatherInfoURL = "http://www.kma.go.kr/wid/queryDFSRSS.jsp?zone=4215061500";
    //private readonly string cityName = "����";
    private string filePath;

    private Dictionary<string, int> weatherInfoDict;
    private List<float> temperatureAvaList = new List<float>();
    private string wtInfo = "NONE";
    private string currentTime;

    private async void Awake()
    {
        Init();
        ReflectCurrentTime();
        await ReflectCurrentWeather();
        WriteRealWeather();
    }

    private void Init()
    {
        filePath = string.Concat(Application.persistentDataPath, "/", "wt01");

        weatherInfoDict = new Dictionary<string, int>();

        /*weatherInfoDict.Add("����", 0);
        weatherInfoDict.Add("��������", 0);
        weatherInfoDict.Add("��������", 0);
        weatherInfoDict.Add("�������� ��", 0);
        weatherInfoDict.Add("�������� ��/��", 0);
        weatherInfoDict.Add("�������� ��/��", 0);
        weatherInfoDict.Add("�������� ��", 0);
        weatherInfoDict.Add("�帲", 0);
        weatherInfoDict.Add("�帮�� ��", 0);
        weatherInfoDict.Add("�帮�� ��/��", 0);
        weatherInfoDict.Add("�帮�� ��/��", 0);
        weatherInfoDict.Add("�帮�� ��", 0);*/
    }

    private void ReflectCurrentTime()  //�� �ð� ���� �ð� �����´�
    {
        currentTime = DateTime.Now.ToString("HH");
    }

    private async Task ReflectCurrentWeather()  //������ �µ��� �����´�
    {
        try
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(weatherInfoURL);

            if (!response.IsSuccessStatusCode) return;

            string content = await response.Content.ReadAsStringAsync();
            XmlDocument document = new XmlDocument();
            document.LoadXml(content);

            XmlNodeList nodes = document.DocumentElement.SelectNodes("descendant::data");

            int index = 0;
            foreach (XmlNode item in nodes)
            {
                //weatherInfoDict[item.SelectSingleNode("wfKor").InnerText.Trim()]++;
                string s = item.SelectSingleNode("wfKor").InnerText;
                if (weatherInfoDict.ContainsKey(s)) weatherInfoDict[s]++;
                else weatherInfoDict.Add(s, 1);    //weatherInfoDict[s] = 0;

                ++index;
                if (index == maxIndex) break;
            }

            foreach(XmlNode item in nodes)
            {
                if(item.SelectSingleNode("day").InnerText=="1")  //day�� 0�� ���� �� �µ��� -999�İ� �������� (�� ���ðŴ� �ȳ����� ��)
                {
                    float tmpMin = float.Parse(item.SelectSingleNode("tmn").InnerText);
                    float tmpMax = float.Parse(item.SelectSingleNode("tmx").InnerText);
                    temperatureAvaList.Add((tmpMin + tmpMax) / 2f);
                    break;
                }
            }

            wtInfo = "";
        }
        catch(Exception e) { Debug.Log(e); }
    }

    private void WriteRealWeather()  //���Ͽ� ���� ���� ����
    {
        if(wtInfo=="")  //���� ������ ����� ������
        {
            string best = "";
            foreach(string key in weatherInfoDict.Keys)
            {
                if(best=="")
                {
                    best = key;
                }
                else
                {
                    if (weatherInfoDict[key] > weatherInfoDict[best])
                        best = key;
                }
            }

            float avg = 0.0f;
            for (int i = 0; i < temperatureAvaList.Count; i++) avg += temperatureAvaList[i];
            avg /= temperatureAvaList.Count;

            wtInfo = string.Concat(best, "#", avg.ToString());
        }

        File.WriteAllText(filePath,wtInfo + "$" + currentTime);
    }

    private void Update()
    {
        
    }
}





/*XmlNodeList nodes = document.DocumentElement.SelectNodes("descendant::location");

            foreach (XmlNode node in nodes)
            {
                XmlNode cityNode = node.SelectSingleNode("city");

                if (cityNode == null) continue;
                
                if (cityNode.InnerText != cityName) continue;
                
                XmlNodeList nodeList = node.SelectNodes("descendant::data");

                int index = 0;
                foreach (XmlNode item in nodeList)
                {
                    int tmpMin = int.Parse(item.SelectSingleNode("tmn").InnerText);
                    int tmpMax = int.Parse(item.SelectSingleNode("tmx").InnerText);

                    temperatureAvaList.Add((tmpMax + tmpMin) / 2);
                    weatherInfoDict[item.SelectSingleNode("wf").InnerText]++;
                    ++index;
                    if (index == maxIndex) break;
                    //Debug.Log($"����: {weatherNode.InnerText}");
                }

                //Debug.Log("���� ����:" + weatherInfoDict["��������"]);
                //temperatureAvaList.ForEach(x => Debug.Log(x));
                wtInfo = "";     //string.Empty;
                //������ ���� ��� ����
                break;
            }*/
