using UnityEditor;
using ProjectColombo.UI;

#if UNITY_EDITOR
[CustomEditor(typeof(UIInputSwitcher))]
public class UIInputSwitcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UIInputSwitcher inputSwitcher = (UIInputSwitcher)target;
        
        EditorGUILayout.HelpBox("This component switches between mouse and controller input for UI navigation.\n\nWhen using a controller after clicking outside a button, it will automatically select the first button.\n\nAssign the first selectable button below.", MessageType.Info);
        
        DrawDefaultInspector();
    }
}
#endif
