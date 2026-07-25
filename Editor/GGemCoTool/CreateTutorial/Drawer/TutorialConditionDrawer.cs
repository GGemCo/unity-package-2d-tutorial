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
        /// 자동 시작 조건의 이벤트/상태 Source를 먼저 표시하고 Source에 맞는 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">자동 시작 조건 SerializedProperty입니다.</param>
        public void DrawStart(SerializedProperty conditionProperty)
        {
            if (conditionProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "자동 시작 조건 데이터를 찾을 수 없습니다.",
                    MessageType.Error);
                return;
            }

            SerializedProperty sourceProperty =
                conditionProperty.FindPropertyRelative("startSource");
            EditorGUILayout.PropertyField(
                sourceProperty,
                new GUIContent("판정 방식"));
            TutorialStartConditionSource source =
                (TutorialStartConditionSource)sourceProperty.intValue;
            if (source == TutorialStartConditionSource.Event)
            {
                Draw(conditionProperty);
                return;
            }

            SerializedProperty stateTypeProperty =
                conditionProperty.FindPropertyRelative("startStateType");
            EditorGUILayout.PropertyField(
                stateTypeProperty,
                new GUIContent("상태 타입"));
            TutorialStartStateType stateType =
                (TutorialStartStateType)stateTypeProperty.intValue;
            switch (stateType)
            {
                case TutorialStartStateType.WindowVisible:
                    DrawTargetUid(conditionProperty, "Window UID");
                    EditorGUILayout.HelpBox(
                        "지정한 UI Window가 현재 표시 중일 때 충족됩니다.",
                        MessageType.Info);
                    break;
                case TutorialStartStateType.MapCleared:
                    DrawTargetUid(conditionProperty, "Map UID");
                    EditorGUILayout.HelpBox(
                        "지정한 맵의 클리어 기록이 저장되어 있을 때 충족됩니다.",
                        MessageType.Info);
                    break;
                case TutorialStartStateType.None:
                    EditorGUILayout.HelpBox(
                        "상태 타입을 선택하십시오.",
                        MessageType.Info);
                    break;
            }

            EditorGUILayout.PropertyField(
                conditionProperty.FindPropertyRelative("memo"),
                new GUIContent("제작 메모"));
        }

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
                    DrawTargetUid(conditionProperty, GetTargetLabel(type));
                    break;
                case TutorialEventType.GuideClicked:
                    EditorGUILayout.HelpBox(
                        "기존 단일 가이드 클릭 조건입니다. 새 가이드 닫기 이벤트도 호환하여 처리합니다.",
                        MessageType.Info);
                    break;
                case TutorialEventType.GuideClosed:
                    EditorGUILayout.HelpBox(
                        "모든 가이드 페이지를 확인한 뒤 닫기 버튼을 누를 때 완료됩니다.",
                        MessageType.Info);
                    break;
                case TutorialEventType.CombatStarted:
                    EditorGUILayout.HelpBox(
                        "플레이어가 비전투 상태에서 전투 상태로 진입할 때 완료됩니다.",
                        MessageType.Info);
                    break;
                case TutorialEventType.PlayerDied:
                    EditorGUILayout.HelpBox(
                        "플레이어의 사망 상태 전환이 확정될 때 완료됩니다.",
                        MessageType.Info);
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
                _ => "Target UID",
            };
        }
    }
}
