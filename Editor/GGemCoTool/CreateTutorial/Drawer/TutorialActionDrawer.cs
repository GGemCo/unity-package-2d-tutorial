using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 액션 종류에 맞는 UID·enum 필드를 그립니다.
    /// </summary>
    internal sealed class TutorialActionDrawer
    {
        /// <summary>
        /// 액션 SerializedProperty를 현재 액션 종류에 맞게 편집합니다.
        /// </summary>
        public void Draw(SerializedProperty actionProperty)
        {
            if (actionProperty == null)
            {
                EditorGUILayout.HelpBox("액션 데이터를 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            SerializedProperty typeProperty = actionProperty.FindPropertyRelative("type");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProperty, new GUIContent("액션 타입"));
            if (EditorGUI.EndChangeCheck())
            {
                TutorialGuideSpriteAddressableSynchronizer.ScheduleSynchronize();
            }

            TutorialActionType type = (TutorialActionType)typeProperty.intValue;

            switch (type)
            {
                case TutorialActionType.ShowGuide:
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(
                        actionProperty.FindPropertyRelative("guidePages"),
                        new GUIContent("가이드 페이지"),
                        true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        TutorialGuideSpriteAddressableSynchronizer.ScheduleSynchronize();
                    }

                    EditorGUILayout.HelpBox(
                        "각 페이지에 Sprite와 한글 설명을 입력합니다. Localization 키는 JSON Export 시 자동 생성되며, 위에서 아래 순서로 페이지를 표시합니다.",
                        MessageType.Info);
                    break;
                case TutorialActionType.HighlightUi:
                case TutorialActionType.ClearHighlight:
                    DrawTargetUid(actionProperty, "UI Target UID");
                    break;
                case TutorialActionType.UnlockFeature:
                    DrawTargetUid(actionProperty, "Feature UID");
                    break;
                case TutorialActionType.StartQuest:
                    DrawTargetUid(actionProperty, "Quest UID");
                    break;
                case TutorialActionType.BlockInputExcept:
                    EditorGUILayout.PropertyField(
                        actionProperty.FindPropertyRelative("inputMask"),
                        new GUIContent("허용 입력"));
                    break;
                case TutorialActionType.SetGameplayState:
                    EditorGUILayout.PropertyField(
                        actionProperty.FindPropertyRelative("gameplayState"),
                        new GUIContent("게임 상태"));
                    break;
                case TutorialActionType.None:
                    EditorGUILayout.HelpBox("액션 타입을 선택하십시오.", MessageType.Info);
                    break;
            }

            EditorGUILayout.PropertyField(
                actionProperty.FindPropertyRelative("memo"),
                new GUIContent("제작 메모"));
        }

        private static void DrawTargetUid(SerializedProperty property, string label)
        {
            EditorGUILayout.PropertyField(
                property.FindPropertyRelative("targetUid"),
                new GUIContent(label));
        }
    }
}
