using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    public Material[] skyMaterials;
    public GameObject rain, snow;
    public Light _light;
    public Camera mainCam;
    public Color[] lightColors;
    public GameObject touchTrailEffect;
    public ParticleSystem touchParticle;

    #region �ð�, ���� ����
    public int maxIndex = 3;

    private readonly string weatherInfoURL = "http://www.kma.go.kr/wid/queryDFSRSS.jsp?zone=4215061500";
    //private readonly string cityName = "����";
    private string filePath;

    private Dictionary<string, int> weatherInfoDict;
    private List<float> temperatureAvaList = new List<float>();
    private string wtInfo = "NONE";
    private string currentTime;
    #endregion

    private WaitForSeconds ws1, ws2, ws3;

    #region UI
    public CanvasGroup loadingCvsg;
    public RectTransform startBtnRect;
    public Image quitImage;
    public Image quitPanel;
    private Text startText;
    #endregion

    private async void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
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

        startText = startBtnRect.GetChild(0).GetComponent<Text>();
        ws1 = new WaitForSeconds(1);
        ws2 = new WaitForSeconds(0.35f);
        ws3 = new WaitForSeconds(0.1f);
        mainCam.transform.DORotate(new Vector3(0, 0, -5), 5).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        StartCoroutine(LightEffectCo());
    }

    private void ReflectCurrentTime()  //�� �ð� ���� �ð� �����´�
    {
        currentTime = DateTime.Now.ToString("HH");

        int time = int.Parse(currentTime);

        if (!File.Exists(string.Concat(Application.persistentDataPath, "/", "ten")))
        {
            if (time > 18 || time < 5) RenderSettings.skybox = skyMaterials[0];
        }
        else
        {
            RenderSettings.skybox = skyMaterials[2];
        }
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
                if(item.SelectSingleNode("day").InnerText=="1")  //day�� 0�� ���� ������ �µ��� -999�� (�� ���ðŴ� �ȳ����� ��)
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

            if (best.Contains("��"))
            {
                snow.SetActive(true);
            }
            if (best.Contains("��"))
            {
                rain.SetActive(true);
            }
        }

        File.WriteAllText(filePath,wtInfo + "$" + currentTime);

        //���� ȭ�� ��Ÿ��
        DOTween.To(() => loadingCvsg.alpha, x => loadingCvsg.alpha = x, 0, 1).OnComplete(() => {
            loadingCvsg.gameObject.SetActive(false);
            startBtnRect.DOAnchorPos(new Vector2(-15.4f, -452.7f), 1.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                startText.DOColor(Color.white, 0.4f).OnComplete(() =>
                {
                    Color c = startText.color;
                    c.a = 0.25f;

                    startText.DOColor(c, 2).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
                    quitImage.DOFillAmount(1, 1);
                });
            });
        });       
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickQuitBtn();
        }
    }

    private void LateUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100000))
            {
                touchParticle.transform.position = hit.point;
                touchParticle.Play();
                touchTrailEffect.SetActive(true);
            }
        }
        if(Input.GetMouseButton(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100000))
            {
                touchTrailEffect.transform.position = hit.point;
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            touchTrailEffect.SetActive(false);
        }
    }

    public void OnClickStart()
    {
        loadingCvsg.gameObject.SetActive(true);
        loadingCvsg.DOFade(1, 1).OnComplete(() => SceneManager.LoadScene("Main"));
    }

    public void OnClickQuitBtn()
    {
        quitPanel.gameObject.SetActive(!quitPanel.gameObject.activeSelf);
    }

    public void Quit() => Application.Quit();

    private IEnumerator LightEffectCo() //Ÿ��Ʋ ȭ�� �� ȿ��
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(7f, 12f));

            _light.cullingMask = 1<<0 | 1<<7;
            yield return ws1;
            for(int i=0; i<3; i++)
            {
                _light.cullingMask = 1 << 7;
                yield return ws3;
                _light.cullingMask = 1 << 0 | 1 << 7;
                yield return ws3;
            }

            _light.cullingMask = ~-1;
            yield return ws2;

            Color c;
            do
            {
                c = lightColors[UnityEngine.Random.Range(0, lightColors.Length)];
            } while (c == _light.color);
            _light.color = c;

            /*_light.cullingMask = 1 << 0 | 1 << 7;
            yield return ws2;
            _light.cullingMask = 1 << 7;
            yield return ws2;
            _light.cullingMask = 1 << 0 | 1 << 7;
            yield return ws3;*/
            _light.cullingMask = 1 << 7;
        }
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
