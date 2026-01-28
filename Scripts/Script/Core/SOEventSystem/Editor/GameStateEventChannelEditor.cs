using UnityEditor;

[CustomEditor(typeof(GameStateEventChannelSO))]
public class GameStateEventChannelEditor : GenericEventChannelEditor<GameState, GameStateEventChannelSO>
{
    protected override GameState DrawTypeSpecificField(GameState value)
    {
        return (GameState)EditorGUILayout.EnumPopup("Debug State to Raise", value);
    }
}