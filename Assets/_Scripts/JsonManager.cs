﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class JsonManager : MonoBehaviour
{
    public StateManager stateManager;
    public GameObject ObjectToSave;
    private Text TextFieldToPrint, TextFieldToPrint2;
    public GameObject bed, bookshelf, desk, deskChair, gasrange, lightStand, oven, refrigerator, sink, sofa, table, tableChair, television, wardrobe;
    
    private void Start()
    {
        // AR화면 현재상태 매니저
        stateManager = TrackerManager.Instance.GetStateManager();

        // 출력할 Text 찾기
        TextFieldToPrint = GameObject.Find("TextForDebug").GetComponent<Text>();
        TextFieldToPrint2 = GameObject.Find("LoadText").GetComponent<Text>();

        // 버튼에 이벤트 달기
        Button PrintButton = GameObject.Find("Button").GetComponent<Button>();
        PrintButton.onClick.AddListener(SaveObjectInJson);
        Button PrintButton2 = GameObject.Find("Button (2)").GetComponent<Button>();
        PrintButton2.onClick.AddListener(LoadObjectFromJson);
    }

    // 저장파일 경로
    public string pathForDocumentsFile(string filename)
    {
        // 아이폰(미테스트)
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5) + "/Resources";
            //path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }
        // 안드로이드
        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath + "/Resources";
            //path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        // pc
        else
        {
            string path = Application.dataPath + "/Resources";
            return Path.Combine(path, filename);
        }
    }

    // json을 통한 오브젝트 저장
    public void SaveObjectInJson()
    {
        // trackable한(인식되는) 가구들
        var trackable = stateManager.GetActiveTrackableBehaviours();
        // 인식되는 가구들을 SerializedData list에 저장
        List<SerializedData> list = new List<SerializedData>();

        foreach(var t in trackable)
        {            
            ObjectToSave = t.gameObject.GetComponentInChildren<SerializedObject>().gameObject;
            Debug.Log(ObjectToSave.transform.position);
            if (ObjectToSave.GetComponent<SerializedObject>().sd == null)
            {
                Debug.LogError("Object can't save");
                return;
            }
            // list에 넣기 전 현상태 불러오기
            ObjectToSave.GetComponent<SerializedObject>().SetCurrentState();
            list.Add(ObjectToSave.GetComponent<SerializedObject>().sd);
        }
        // 배열의 내용을 json형식 string으로 저장
        string json = JsonHelper.ToJson<SerializedData>(list.ToArray());

        // 저장 디렉토리 존재여부
        if (!Directory.Exists(Application.persistentDataPath + "/Resources"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Resources");
        }
        // 전체 파일 경로, ("")안의 이름으로 파일 저장
        // TODO : 파일 이름을 사용자가 설정해서 저장시켜야 함
        string path = pathForDocumentsFile("data.json");
        FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(file);
        sw.WriteLine(json);
        sw.Close();
        file.Close();
    }

    public void LoadObjectFromJson()
    {
        // TODO : 파일 이름을 저장소 내부의 파일 이름으로 가져와야 함
        string filename = "data.json";
        string path = pathForDocumentsFile(filename);

        if (File.Exists(path))
        {
            // 파일로 부터 string으로 된 json을 가져온다.
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            string json = null;
            json = sr.ReadLine();

            sr.Close();
            file.Close();
            
            TextFieldToPrint2.text = json; // 구현이 끝나면 삭제
            // 데이터 배열로 deserialize 하여 그 정보로 오브젝트 생성
            SerializedData[] list = JsonHelper.FromJson<SerializedData>(json);
            foreach (var ObjectToLoad in list)
            {
                string name = ObjectToLoad.mFurniture.Trim();

                if (name.IndexOf("(") == 1)
                {
                    name = name.Substring(0, name.IndexOf("(") - 1);
                }
                Debug.Log(name);

                GameObject madeObject;
                switch (name)
                {
                    case "Wall":
                        madeObject = new GameObject("Wall");
                        MeshFilter mf = madeObject.AddComponent<MeshFilter>();
                        MeshRenderer mr = madeObject.AddComponent<MeshRenderer>();
                        madeObject.AddComponent<SerializedObject>();
                        mr.material = new Material(Shader.Find("Standard"));
                        break;
                    case "bed":
                        madeObject = GameObject.Instantiate(bed);
                        break;
                    case "bookshelf":
                        madeObject = GameObject.Instantiate(bookshelf);
                        break;
                    case "desk":
                        madeObject = GameObject.Instantiate(desk);
                        break;
                    case "deskChair":
                        madeObject = GameObject.Instantiate(deskChair);
                        break;
                    case "gasrange":
                        madeObject = GameObject.Instantiate(gasrange);
                        break;
                    case "lightStand":
                        madeObject = GameObject.Instantiate(lightStand);
                        break;
                    case "oven":
                        madeObject = GameObject.Instantiate(oven);
                        break;
                    case "refrigerator":
                        madeObject = GameObject.Instantiate(refrigerator);
                        break;
                    case "sink":
                        madeObject = GameObject.Instantiate(sink);
                        break;
                    case "sofa":
                        madeObject = GameObject.Instantiate(sofa);
                        break;
                    case "table":
                        madeObject = GameObject.Instantiate(table);
                        break;
                    case "tableChair":
                        madeObject = GameObject.Instantiate(tableChair);
                        break;
                    case "television":
                        madeObject = GameObject.Instantiate(television);
                        break;
                    case "wardrobe":
                        madeObject = GameObject.Instantiate(wardrobe);
                        break;
                    default:
                        madeObject = new GameObject("error");
                        Debug.LogError("error");
                        break;
                }

                madeObject.GetComponent<SerializedObject>().sd = ObjectToLoad;
                madeObject.GetComponent<SerializedObject>().SetSdToObject();
            }
        }
        else
        {
            Debug.LogError("없는 가구, 가구 추가 요망");
        }
    }
}

// JsonUtility 배열 지원용 Helper 클래스
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}