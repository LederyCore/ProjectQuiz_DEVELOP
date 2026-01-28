using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

// 1. CSV 파일 변경 감지
public class DataAutoImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool csvChanged = false;
        foreach (string str in importedAssets)
        {
            if (str.EndsWith("PlaceQuizData.csv") ||
                str.EndsWith("CountryQuizData.csv") ||
                str.EndsWith("GlobalQuizData.csv") ||
                str.EndsWith("EventData.csv") ||
                str.EndsWith("ArtifactData.csv") ||
                str.EndsWith("MissionData.csv"))
            {
                csvChanged = true;
                break;
            }
        }

        if (csvChanged)
        {
            Debug.Log("📄 CSV 변경 감지! 데이터를 폴더별로 갱신합니다...");
            Importer.ImportAllData();
        }
    }
}

public class Importer 
{
    // CSV 파일 경로
    private static string placeQuizPath = Config.PlaceQuizPath;
    private static string countryQuizPath = Config.CountryQuizPath;
    private static string globalQuizPath = Config.GlobalQuizPath;
    private static string eventDataPath = Config.EventDataPath;
    private static string artifactDataPath = Config.ArtifactDataPath;
    private static string missionDataPath = Config.MissionDataPath;

    // 생성될 루트 폴더 경로들
    private static string ROOT_PLACE = Config.ROOT_PLACE;
    private static string ROOT_COUNTRY = Config.ROOT_COUNTRY;
    private static string ROOT_GLOBAL = Config.ROOT_GLOBAL;
    private static string ROOT_ARTIFACT = Config.ROOT_ARTIFACT;
    private static string ROOT_EVENT = Config.ROOT_EVENT;
    private static string ROOT_MISSION = Config.ROOT_MISSION;

    [MenuItem("Tools/Quiz Data Import (All)")]
    public static void ImportAllData()
    {
        // 1. 기본 데이터 생성
        ProcessFile(placeQuizPath, "도시");
        ProcessFile(countryQuizPath, "국가");
        ProcessFile(globalQuizPath, "전역");

        // 2. 이벤트 데이터 생성 (EventDataSO)
        ProcessEventFile(eventDataPath);

        // 3. 유물 생성
        ProcessArtifactFile(artifactDataPath);

        // 4. 미션 데이터 생성 (MissionDataSO)
        ProcessMissionfile(missionDataPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ 모든 데이터 동기화 완료! (Global 웰컴 메시지 수정 포함)");
    }

    private static void ProcessMissionfile(string missionDataPath)
    {
        List<Dictionary<string, object>> data = CSVReader.Read(missionDataPath);
        if (data == null || data.Count == 0) return;

        // 폴더가 없으면 생성
        if (!Directory.Exists(ROOT_MISSION)) Directory.CreateDirectory(ROOT_MISSION);

        foreach (var row in data)
        {
            string id = GetString(row, "ID");
            if (string.IsNullOrEmpty(id) || id == "ID") 
                continue;

            // 파일 경로: Assets/Resources/Data/MissionData/Mission_01.asset
            string assetPath = $"{ROOT_MISSION}/{ValidateFileName(id)}.asset";

            // MissionDataSO 로드 또는 생성
            MissionDataSO mission = AssetDatabase.LoadAssetAtPath<MissionDataSO>(assetPath);
            if (mission == null)
            {
                mission = ScriptableObject.CreateInstance<MissionDataSO>();
                AssetDatabase.CreateAsset(mission, assetPath);
            }

            // --- 필드 매핑 ---
            mission.Id = id;
            mission.Title = GetString(row, "Title");
            mission.Description = GetString(row, "Description");
            switch (GetString(row, "Type"))
            {
                case "DayCount":
                    mission.Type = MissionType.DayCount;
                    break;
                case "CityCount":
                    mission.Type = MissionType.CityCount;
                    break;
                case "CountryCount":
                    mission.Type = MissionType.CountryCount;
                    break;
                case "FlightCount":
                    mission.Type = MissionType.FlightCount;
                    break;
                case "TotalQuiz":
                    mission.Type = MissionType.TotalQuiz;
                    break;
                case "ArtifactCount":
                    mission.Type = MissionType.ArtifactCount;
                    break;
                case "MoneyPossessed":
                    mission.Type = MissionType.MoneyPossessed;
                    break;
            }
            ;

            string targetStr = GetString(row, "Target");
            mission.Target = int.TryParse(targetStr, out int target) ? target : 0;
            string rewardMoneyStr = GetString(row, "RewardMoney");
            mission.RewardMoney = int.TryParse(rewardMoneyStr, out int rewardMoney) ? rewardMoney : 0;
            mission.RewardArtifact = GetString(row, "RewardArtifact");
            EditorUtility.SetDirty(mission);
        }
    }

    // -----------------------------------------------------------------------
    // EventData.csv -> EventDataSO 변환
    // -----------------------------------------------------------------------
    static void ProcessEventFile(string csvPath)
    {
        List<Dictionary<string, object>> data = CSVReader.Read(csvPath);
        if (data == null || data.Count == 0) return;

        // 폴더가 없으면 생성
        if (!Directory.Exists(ROOT_EVENT)) Directory.CreateDirectory(ROOT_EVENT);

        foreach (var row in data)
        {
            string id = GetString(row, "ID");
            if (string.IsNullOrEmpty(id)) continue;

            // 파일 경로: Assets/AssetResources/Data/EventData/Danger_01.asset
            string assetPath = $"{ROOT_EVENT}/{ValidateFileName(id)}.asset";

            // EventDataSO 로드 또는 생성
            EventDataSO evt = AssetDatabase.LoadAssetAtPath<EventDataSO>(assetPath);
            if (evt == null)
            {
                evt = ScriptableObject.CreateInstance<EventDataSO>();
                AssetDatabase.CreateAsset(evt, assetPath);
            }

            // --- 필드 매핑 ---
            evt.eventID = id;

            // 1. 타입 설정 (Danger / Luck / Event / Artifact / Fail)
            string typeStr = GetString(row, "Type", "Event");
            if (typeStr == "Danger") evt.eventType = EventActionType.Danger;
            else if (typeStr == "Luck") evt.eventType = EventActionType.Luck;
            else if (typeStr == "Fail") evt.eventType = EventActionType.Fail;
            else if (typeStr == "Artifact") evt.eventType = EventActionType.Artifact;
            else evt.eventType = EventActionType.Event;

            // 2. 제목
            evt.title = GetString(row, "Title");
            if (string.IsNullOrEmpty(evt.title)) evt.title = GetString(row, "Question");

            // 3. 설명
            evt.description = GetString(row, "Description");

            // 4. 금액
            string moneyStr = GetString(row, "Money");
            if (string.IsNullOrEmpty(moneyStr)) moneyStr = GetString(row, "Correct");
            evt.moneyAmount = int.TryParse(moneyStr, out int amount) ? amount : 0;

            // 5. 이미지
            string imgName = GetString(row, "Image");
            if (!string.IsNullOrEmpty(imgName)) evt.eventImage = FindSpriteExact(imgName);

            // 6. 특수 효과
            evt.specialEffectID = GetString(row, "SpecialEffect");

            EditorUtility.SetDirty(evt);
        }
    }

    // -----------------------------------------------------------------------
    // 유물 데이터 처리
    // -----------------------------------------------------------------------
    static void ProcessArtifactFile(string csvPath)
    {
        List<Dictionary<string, object>> data = CSVReader.Read(csvPath);
        if (data == null || data.Count == 0) return;
        foreach (var row in data)
        {
            string ownerID = GetString(row, "OwnerID");
            string artifactID = GetString(row, "ID");
            if (string.IsNullOrEmpty(artifactID)) continue;
            ArtifactDataSO artifact = CreateArtifactAsset(ownerID, artifactID, row);
            if (!string.IsNullOrEmpty(ownerID)) LinkArtifactToOwner(ownerID, artifact);
        }
    }

    static ArtifactDataSO CreateArtifactAsset(string ownerID, string artifactID, Dictionary<string, object> row)
    {
        string safeOwner = ValidateFileName(string.IsNullOrEmpty(ownerID) ? "Common" : ownerID);
        string folderPath = $"{ROOT_ARTIFACT}/{safeOwner}";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string fileName = ValidateFileName(artifactID);
        string assetPath = $"{folderPath}/{fileName}.asset";
        ArtifactDataSO artifact = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(assetPath);
        if (artifact == null) { artifact = ScriptableObject.CreateInstance<ArtifactDataSO>(); AssetDatabase.CreateAsset(artifact, assetPath); }

        artifact.artifactID = artifactID;
        artifact.artifactName = GetString(row, "Name");
        artifact.description = GetString(row, "Description");
        artifact.rarity = GetString(row, "Rarity");
        string valStr = GetString(row, "Value");
        artifact.value = int.TryParse(valStr, out int v) ? v : 0;
        string imgName = GetString(row, "Image");
        if (!string.IsNullOrEmpty(imgName)) artifact.artifactImage = FindSpriteExact(imgName);

        EditorUtility.SetDirty(artifact);
        return artifact;
    }

    // -----------------------------------------------------------------------
    // 퀴즈 데이터 처리
    // -----------------------------------------------------------------------
    static void ProcessFile(string csvPath, string label)
    {
        List<Dictionary<string, object>> data = CSVReader.Read(csvPath);
        if (data == null || data.Count == 0) return;
        Dictionary<string, List<Dictionary<string, object>>> groupedData = new Dictionary<string, List<Dictionary<string, object>>>();
        foreach (var row in data)
        {
            string id = GetString(row, "PlaceID");
            if (string.IsNullOrEmpty(id)) id = GetString(row, "CountryID");
            if (string.IsNullOrEmpty(id)) id = GetString(row, "ID");
            if (string.IsNullOrEmpty(id)) continue;
            if (!groupedData.ContainsKey(id)) groupedData[id] = new List<Dictionary<string, object>>();
            groupedData[id].Add(row);
        }
        foreach (var group in groupedData) UpdateTargetDataSO(group.Key, group.Value, label);
    }

    static void UpdateTargetDataSO(string targetID, List<Dictionary<string, object>> rows, string label)
    {
        // 1. 폴더 및 경로 설정
        string rootFolder = "";
        if (label == "도시") rootFolder = ROOT_PLACE;
        else if (label == "국가") rootFolder = ROOT_COUNTRY;
        else if (label == "전역") rootFolder = ROOT_GLOBAL;

        string safeID = ValidateFileName(targetID);
        string targetFolder = $"{rootFolder}/{safeID}";
        if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

        string assetPath = $"{targetFolder}/{safeID}.asset";

        // =================================================================
        // [구현 부분] FindRowValue 로컬 함수 정의
        // 역할: rows 리스트를 순회하며 해당 key(컬럼)에 값이 있는 첫 번째 행의 데이터를 반환
        // =================================================================
        string FindRowValue(string key)
        {
            foreach (var row in rows)
            {
                // 기존에 만들어둔 GetString 함수를 재사용
                string val = GetString(row, key);

                // 값이 비어있지 않다면 그 값을 반환하고 종료
                if (!string.IsNullOrEmpty(val))
                {
                    return val;
                }
            }
            // 모든 행을 뒤져도 값이 없으면 빈 문자열 반환
            return "";
        }
        // =================================================================

        // 2. 데이터 처리 시작
        if (label == "도시" || label == "전역")
        {
            PlaceDataSO placeData = AssetDatabase.LoadAssetAtPath<PlaceDataSO>(assetPath);
            if (placeData == null)
            {
                placeData = ScriptableObject.CreateInstance<PlaceDataSO>();
                AssetDatabase.CreateAsset(placeData, assetPath);
            }

            placeData.placeID = targetID;

            if (label == "전역")
            {
                placeData.isGlobal = true;
                // ★ 여기서 FindRowValue 사용
                string region = FindRowValue("Region");
                placeData.placeName_KR = string.IsNullOrEmpty(region) ? "Global" : region;
                placeData.regionName_KR = "Global";
                placeData.countryID = "";
            }
            else
            {
                placeData.isGlobal = false;
                // ★ 여기서 FindRowValue 사용
                placeData.placeName_KR = FindRowValue("City_KR");
                placeData.regionName_KR = FindRowValue("Region");
                placeData.countryID = FindRowValue("CountryID");
            }

            // ★ 공통 데이터도 FindRowValue로 안전하게 찾기
            placeData.welcomeMessage = FindRowValue("WelcomeMsg");
            string wImg = FindRowValue("WelcomeImg");
            placeData.welcomeImage = !string.IsNullOrEmpty(wImg) ? FindSpriteExact(wImg) : null;

            // 덱 초기화 및 생성
            if (placeData.placeQuizDeck == null) placeData.placeQuizDeck = new List<CardDataSO>();
            if (placeData.placeArtifactDeck == null) placeData.placeArtifactDeck = new List<ArtifactDataSO>();
            placeData.placeQuizDeck.Clear();
            placeData.placeArtifactDeck.Clear();

            int qIdx = 0;
            foreach (var row in rows)
            {
                string type = GetString(row, "Type", "Quiz");
                if (type != "Artifact")
                {
                    string quizFolder = $"{targetFolder}/Quizzes";
                    placeData.placeQuizDeck.Add(CreateCard(targetID, quizFolder, qIdx++, type, row));
                }
            }
            EditorUtility.SetDirty(placeData);
        }
        else if (label == "국가")
        {
            CountryDataSO countryData = AssetDatabase.LoadAssetAtPath<CountryDataSO>(assetPath);
            if (countryData == null)
            {
                countryData = ScriptableObject.CreateInstance<CountryDataSO>();
                AssetDatabase.CreateAsset(countryData, assetPath);
            }

            countryData.countryID = targetID;

            // ★ [국가 데이터] FindRowValue 적용
            // 여러 행 중 하나라도 CountryName_KR 값이 있으면 가져옴
            string cName = FindRowValue("CountryName_KR");
            countryData.countryName_KR = string.IsNullOrEmpty(cName) ? targetID : cName;

            // 여러 행 중 하나라도 StampImage 값이 있으면 가져옴
            string stampName = FindRowValue("StampImage");
            countryData.stampImage = !string.IsNullOrEmpty(stampName) ? FindSpriteExact(stampName) : null;

            // 덱 초기화 및 생성
            if (countryData.countryQuizDeck == null) countryData.countryQuizDeck = new List<CardDataSO>();
            countryData.countryQuizDeck.Clear();

            int qIdx = 0;
            foreach (var row in rows)
            {
                string type = GetString(row, "Type", "Quiz");
                if (type != "Artifact")
                {
                    string quizFolder = $"{targetFolder}/Quizzes";
                    countryData.countryQuizDeck.Add(CreateCard(targetID, quizFolder, qIdx++, type, row));
                }
            }
            EditorUtility.SetDirty(countryData);
        }
    }

    static CardDataSO CreateCard(string ownerID, string folderPath, int index, string type, Dictionary<string, object> row)
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string fileName = $"Quiz_{ValidateFileName(ownerID)}_{index}";
        string assetPath = $"{folderPath}/{fileName}.asset";

        CardDataSO card = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
        if (card == null) { card = ScriptableObject.CreateInstance<CardDataSO>(); AssetDatabase.CreateAsset(card, assetPath); }

        card.cardType = CardType.Quiz;
        card.CompositeID = GetString(row, "CompositeID");
        card.cardName = fileName;
        card.question = GetString(row, "Question");
        card.explanation = GetString(row, "Explanation");

        string imgName = GetString(row, "Image");
        if (!string.IsNullOrEmpty(imgName)) card.cardImage = FindSpriteExact(imgName);

        card.correctAnswer = GetString(row, "Correct");
        card.wrongAnswer1 = GetString(row, "Wrong1");
        card.wrongAnswer2 = GetString(row, "Wrong2");
        card.wrongAnswer3 = GetString(row, "Wrong3");

        EditorUtility.SetDirty(card);
        return card;
    }

    static T FindAssetByID<T>(string id) where T : ScriptableObject { string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}"); foreach (string guid in guids) { T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)); if (asset is PlaceDataSO p && p.placeID == id) return asset; if (asset is CountryDataSO c && c.countryID == id) return asset; } return null; }
    static void LinkArtifactToOwner(string ownerID, ArtifactDataSO artifact) { PlaceDataSO place = FindAssetByID<PlaceDataSO>(ownerID); if (place != null) { if (place.placeArtifactDeck == null) place.placeArtifactDeck = new List<ArtifactDataSO>(); if (!place.placeArtifactDeck.Contains(artifact)) { place.placeArtifactDeck.Add(artifact); EditorUtility.SetDirty(place); } } }
    static Sprite FindSpriteExact(string imageName) { if (string.IsNullOrEmpty(imageName)) return null; string[] guids = AssetDatabase.FindAssets($"{imageName} t:Sprite"); foreach (string guid in guids) { string path = AssetDatabase.GUIDToAssetPath(guid); if (Path.GetFileNameWithoutExtension(path).Equals(imageName, System.StringComparison.OrdinalIgnoreCase)) return AssetDatabase.LoadAssetAtPath<Sprite>(path); } return null; }
    static string GetString(Dictionary<string, object> row, string key, string defaultValue = "") { if (row.ContainsKey(key) && row[key] != null) return row[key].ToString().Trim(); return defaultValue; }
    static string ValidateFileName(string name) { if (string.IsNullOrEmpty(name)) return "Unknown"; string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()); foreach (char c in invalidChars) name = name.Replace(c, '_'); return name; }
}