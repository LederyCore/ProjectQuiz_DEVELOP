using System;

[Serializable]
public class CheckLatestVersionResponse
{
    public string latestVersion;           // ex 0.0.1
    public string updateUrlAnd;
    public string updateUrlIos;
}