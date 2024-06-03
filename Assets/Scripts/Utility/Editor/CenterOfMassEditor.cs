using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CenterOfMassDebug))]
public class CenterOfMassEditor : Editor
{
    private CenterOfMassDebug m_CenterOfMassDebug;

    private SerializedProperty positionCM;
    private SerializedProperty modifyCM;

    private void OnEnable()
    {
        positionCM = serializedObject.FindProperty("positionCM");
        modifyCM = serializedObject.FindProperty("modifyCM");

        m_CenterOfMassDebug  = (CenterOfMassDebug)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var oldPos = positionCM.vector3Value;
        var oldMod = modifyCM.boolValue;

        positionCM.vector3Value = EditorGUILayout.Vector3Field("CoM position", positionCM.vector3Value);
        modifyCM.boolValue = EditorGUILayout.Toggle("Change Default CoM?", modifyCM.boolValue);

        serializedObject.ApplyModifiedProperties();

        if (oldPos != positionCM.vector3Value || oldMod != modifyCM.boolValue)
        {
            m_CenterOfMassDebug.UpdateCM();
        }
    }
}
