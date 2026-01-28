public static class API
{
    // 기본 서버 URL
    public const string URL = "http://x2rtestserver.iptime.org:8832";

    // 엔드 포인트

    // 초기화 관련
    public const string CHECK_SERVER_STATUS = "/server/status";
    public const string CHECK_LATEST_VERSION = "/server/checklatestversion";

    // 유저 인증 관련
    public const string TRY_LOGIN = "/users/trylogin";

    // 플레이어 데이터 관련
    public const string LOAD_PLAYER_DATA = "/player_data/getdata";
    public const string SAVE_PLAYER_DATA = "/player_data/updatedata";

    // 게임 콘텐츠 관련
    public const string LOAD_RANKING = "/game/ranking/";        // 뒤에 {top_n} 추가, 붙이지 않으면 전체 조회
    public const string LOAD_RANKING_WHO = "/game/ranking/who/";   // 뒤에 {user_id} 추가
}