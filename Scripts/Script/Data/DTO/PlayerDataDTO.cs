using System;

[Serializable]
public class PlayerDataDTO
{
    public int player_money;
    public int maximum_money;
    public int current_day;
    public int day_progress;
    public int total_correct_quizzes;
    public int is_first_run; // 1/0
    public int score;
    public string visited_places;    // "a,b,c"
    public string visited_countries; // "kr,jp"
    public string travel_log;        // "place1,place2"
    public string my_artifacts;      // "artifactA,artifactB"
    public int is_guide_finished;    // 1/0
    public int current_guide_index;
    public string status_tracker;    // JSON string
}