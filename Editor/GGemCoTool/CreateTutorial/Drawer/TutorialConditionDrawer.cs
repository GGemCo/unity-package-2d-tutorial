using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 조건 종류에 맞는 UID·enum 필드를 그립니다.
    /// </summary>
    internal sealed class TutorialConditionDrawer
    {
        /// <summary>
        /// 조건 SerializedProperty를 현재 이벤트 종류에 맞게 편집합니다.
        /// </summary>
        public void Draw(SerializedProperty conditionProperty)
        {
            if (conditionProperty == null)
            {
                EditorGUILayout.HelpBox("조건 데이터를 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            SerializedProperty typeProperty = conditionProperty.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProperty, new GUIContent("조건 타입"));
            TutorialEventType type = (TutorialEventType)typeProperty.intValue;

            switch (type)
            {
                case TutorialEventType.EnterMap:
                case TutorialEventType.KillMonster:
                case TutorialEventType.OpenWindow:
                case TutorialEventType.UiClicked:
                case TutorialEventType.QuestStarted:
                case TutorialEventType.QuestCompleted:
                case TutorialEventType.GuideClicked:
                    DrawTargetUid(conditionProperty, GetTargetLabel(type));
                    break;
                case TutorialEventType.InputAction:
                    EditorGUILayout.PropertyField(
                        conditionProperty.FindPropertyRelative("inputAction"),
                        new GUIContent("입력 액션"));
                    break;
                case TutorialEventType.AutoMoveDistanceReached:
                    DrawTargetUid(conditionProperty, "Map UID");
                    EditorGUILayout.PropertyField(
                        conditionProperty.FindPropertyRelative("floatValue"),
                        new GUIContent("이동 거리"));
                    break;
                case TutorialEventType.None:
                    EditorGUILayout.HelpBox("조건 타입을 선택하십시오.", MessageType.Info);
                    break;
            }

            SerializedProperty requiredCount =
                conditionProperty.FindPropertyRelative("requiredCount");
            EditorGUILayout.PropertyField(requiredCount, new GUIContent("필요 횟수"));
            requiredCount.intValue = Mathf.Max(1, requiredCount.intValue);
            EditorGUILayout.PropertyField(
                conditionProperty.FindPropertyRelative("memo"),
                new GUIContent("제작 메모"));
        }

        private static void DrawTargetUid(SerializedProperty property, string label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative("targetUid"),
                new GUIContent(label));
        }

        private static string GetTargetLabel(TutorialEventType type)
        {
            return type switch
            {
                TutorialEventType.EnterMap => "Map UID",
                TutorialEventType.KillMonster => "Monster UID",
                TutorialEventType.OpenWindow => "Window UID",
                TutorialEventType.UiClicked => "UI Target UID",
                TutorialEventType.QuestStarted => "Quest UID",
                TutorialEventType.QuestCompleted => "Quest UID",
                TutorialEventType.GuideClicked => "Guide UID",
                _ => "Target UID",
            };
        }
    }
}
