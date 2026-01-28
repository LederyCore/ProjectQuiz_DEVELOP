using System;
[Serializable]
public class ServerCheckResponse
{
    public int status; // 0: 성공, 1: 오프라인, 2: 점검중, 기타: 알수없는 이유
    public string message;
}
