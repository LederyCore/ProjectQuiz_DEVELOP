using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity JsonUtility 확장 헬퍼 클래스
/// List, Dictionary, Array 등의 직렬화/역직렬화 지원
/// </summary>
public static class JsonHelper
{
    // List 직렬화를 위한 Wrapper 클래스
    [Serializable]
    private class ListWrapper<T>
    {
        public List<T> list;

        public ListWrapper(List<T> list)
        {
            this.list = list;
        }
    }

    // Dictionary 직렬화를 위한 Wrapper 클래스
    [Serializable]
    private class DictionaryWrapper<TKey, TValue>
    {
        public List<TKey> keys;
        public List<TValue> values;

        public DictionaryWrapper(Dictionary<TKey, TValue> dictionary)
        {
            keys = new List<TKey>(dictionary.Keys);
            values = new List<TValue>(dictionary.Values);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }
    }

    // Array 직렬화를 위한 Wrapper 클래스
    [Serializable]
    private class ArrayWrapper<T>
    {
        public T[] array;

        public ArrayWrapper(T[] array)
        {
            this.array = array;
        }
    }

    #region JSON Array 직렬화/역직렬화 (서버 통신용)

    /// <summary>
    /// List를 JSON 배열 문자열로 변환 (서버 전송용)
    /// 예: [{"ID":"ACH_01",...},{"ID":"ACH_02",...}]
    /// </summary>
    public static string ToJsonArray<T>(List<T> list, bool prettyPrint = false)
    {
        if (list == null || list.Count == 0)
            return "[]";

        var wrapper = new ListWrapper<T>(list);
        string json = JsonUtility.ToJson(wrapper, prettyPrint);

        // Wrapper 형식에서 순수 배열 형식으로 변환
        // {"list":[...]} -> [...]
        return ExtractArrayFromWrapper(json);
    }

    /// <summary>
    /// JSON 배열 문자열을 List로 변환 (서버 수신용)
    /// 예: [{"ID":"ACH_01",...},{"ID":"ACH_02",...}] -> List<T>
    /// </summary>
    public static List<T> FromJsonArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "[]")
            return new List<T>();

        // 순수 배열을 Wrapper 형식으로 변환
        // [...] -> {"list":[...]}
        string wrappedJson = WrapArrayToWrapper(json);

        try
        {
            var wrapper = JsonUtility.FromJson<ListWrapper<T>>(wrappedJson);
            return wrapper?.list ?? new List<T>();
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 배열 파싱 실패: {e.Message}\n원본 JSON: {json}");
            return new List<T>();
        }
    }

    /// <summary>
    /// Wrapper JSON에서 순수 배열 부분만 추출
    /// {"list":[...]} -> [...]
    /// </summary>
    private static string ExtractArrayFromWrapper(string wrapperJson)
    {
        int startIndex = wrapperJson.IndexOf('[');
        int endIndex = wrapperJson.LastIndexOf(']');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return wrapperJson.Substring(startIndex, endIndex - startIndex + 1);
        }

        return "[]";
    }

    /// <summary>
    /// 순수 배열을 Wrapper 형식으로 감싸기
    /// [...] -> {"list":[...]}
    /// </summary>
    private static string WrapArrayToWrapper(string arrayJson)
    {
        // 이미 Wrapper 형식인지 확인
        if (arrayJson.TrimStart().StartsWith("{\"list\""))
            return arrayJson;

        return $"{{\"list\":{arrayJson}}}";
    }

    #endregion

    #region List 직렬화/역직렬화 (내부 저장용)

    /// <summary>
    /// List를 JSON 문자열로 변환 (Wrapper 포함)
    /// </summary>
    public static string ToJson<T>(List<T> list, bool prettyPrint = false)
    {
        var wrapper = new ListWrapper<T>(list);
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    /// <summary>
    /// JSON 문자열을 List로 변환 (Wrapper 포함)
    /// </summary>
    public static List<T> FromJsonToList<T>(string json)
    {
        var wrapper = JsonUtility.FromJson<ListWrapper<T>>(json);
        return wrapper?.list ?? new List<T>();
    }

    #endregion

    #region Array 직렬화/역직렬화

    /// <summary>
    /// Array를 JSON 문자열로 변환
    /// </summary>
    public static string ToJson<T>(T[] array, bool prettyPrint = false)
    {
        var wrapper = new ArrayWrapper<T>(array);
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    /// <summary>
    /// JSON 문자열을 Array로 변환
    /// </summary>
    public static T[] FromJsonToArray<T>(string json)
    {
        var wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(json);
        return wrapper?.array ?? new T[0];
    }

    #endregion

    #region Dictionary 직렬화/역직렬화

    /// <summary>
    /// Dictionary를 JSON 문자열로 변환
    /// </summary>
    public static string ToJson<TKey, TValue>(Dictionary<TKey, TValue> dictionary, bool prettyPrint = false)
    {
        var wrapper = new DictionaryWrapper<TKey, TValue>(dictionary);
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    /// <summary>
    /// JSON 문자열을 Dictionary로 변환
    /// </summary>
    public static Dictionary<TKey, TValue> FromJsonToDictionary<TKey, TValue>(string json)
    {
        var wrapper = JsonUtility.FromJson<DictionaryWrapper<TKey, TValue>>(json);
        return wrapper?.ToDictionary() ?? new Dictionary<TKey, TValue>();
    }

    #endregion

    #region 일반 객체 직렬화/역직렬화 (JsonUtility 래핑)

    /// <summary>
    /// 일반 객체를 JSON 문자열로 변환
    /// </summary>
    public static string ToJson<T>(T obj, bool prettyPrint = false)
    {
        return JsonUtility.ToJson(obj, prettyPrint);
    }

    /// <summary>
    /// JSON 문자열을 객체로 변환
    /// </summary>
    public static T FromJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    /// <summary>
    /// JSON 문자열로 기존 객체 덮어쓰기
    /// </summary>
    public static void FromJsonOverwrite<T>(string json, T objectToOverwrite)
    {
        JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
    }

    #endregion

    #region 파일 저장/로드 헬퍼

    /// <summary>
    /// List를 파일로 저장
    /// </summary>
    public static void SaveListToFile<T>(List<T> list, string filePath, bool prettyPrint = false)
    {
        string json = ToJson(list, prettyPrint);
        System.IO.File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 파일에서 List 로드
    /// </summary>
    public static List<T> LoadListFromFile<T>(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning($"파일을 찾을 수 없습니다: {filePath}");
            return new List<T>();
        }

        string json = System.IO.File.ReadAllText(filePath);
        return FromJsonToList<T>(json);
    }

    /// <summary>
    /// Dictionary를 파일로 저장
    /// </summary>
    public static void SaveDictionaryToFile<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string filePath, bool prettyPrint = false)
    {
        string json = ToJson(dictionary, prettyPrint);
        System.IO.File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 파일에서 Dictionary 로드
    /// </summary>
    public static Dictionary<TKey, TValue> LoadDictionaryFromFile<TKey, TValue>(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning($"파일을 찾을 수 없습니다: {filePath}");
            return new Dictionary<TKey, TValue>();
        }

        string json = System.IO.File.ReadAllText(filePath);
        return FromJsonToDictionary<TKey, TValue>(json);
    }

    /// <summary>
    /// 일반 객체를 파일로 저장
    /// </summary>
    public static void SaveToFile<T>(T obj, string filePath, bool prettyPrint = false)
    {
        string json = ToJson(obj, prettyPrint);
        System.IO.File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 파일에서 객체 로드
    /// </summary>
    public static T LoadFromFile<T>(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning($"파일을 찾을 수 없습니다: {filePath}");
            return default(T);
        }

        string json = System.IO.File.ReadAllText(filePath);
        return FromJson<T>(json);
    }

    #endregion
}