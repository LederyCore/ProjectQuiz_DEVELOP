using System;

[Serializable]
public class UpdatePlayerDataRequest
{
    public string user_id;
    public string player_money;
    public int maximum_money;
    public int current_day;
    public int day_progress;
    public int total_correct_quizzes;
    public int score;
    public int is_first_run;
    public int is_guide_finished;
    public int current_guide_index;
    public string visited_places;
    public string visited_countries;
    public string travel_log;
    public string my_artifacts;
    public string status_tracker;
}
