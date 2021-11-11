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
    //private readonly string cityName = "서울";
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

        /*weatherInfoDict.Add("맑음", 0);
        weatherInfoDict.Add("구름조금", 0);
        weatherInfoDict.Add("구름많음", 0);
        weatherInfoDict.Add("구름많고 비", 0);
        weatherInfoDict.Add("구름많고 비/눈", 0);
        weatherInfoDict.Add("구름많고 눈/비", 0);
        weatherInfoDict.Add("구름많고 눈", 0);
        weatherInfoDict.Add("흐림", 0);
        weatherInfoDict.Add("흐리고 비", 0);
        weatherInfoDict.Add("흐리고 비/눈", 0);
        weatherInfoDict.Add("흐리고 눈/비", 0);
        weatherInfoDict.Add("흐리고 눈", 0);*/
    }

    private void ReflectCurrentTime()  //폰 시간 기준 시간 가져온다
    {
        currentTime = DateTime.Now.ToString("HH");
    }

    private async Task ReflectCurrentWeather()  //날씨와 온도를 가져온다
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
                if(item.SelectSingleNode("day").InnerText=="1")  //day가 0인 것은 왜 온도가 -999냐고 ㅋㅋㅋㅋ (걍 오늘거는 안나오는 듯)
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

    private void WriteRealWeather()  //파일에 날씨 정보 써줌
    {
        if(wtInfo=="")  //날씨 정보를 제대로 가져옴
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
                    //Debug.Log($"날씨: {weatherNode.InnerText}");
                }

                //Debug.Log("구름 많음:" + weatherInfoDict["구름많음"]);
                //temperatureAvaList.ForEach(x => Debug.Log(x));
                wtInfo = "";     //string.Empty;
                //날씨에 따른 배경 조절
                break;
            }*/
