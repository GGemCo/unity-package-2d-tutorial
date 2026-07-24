using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 가이드 페이지의 Sprite, 한글 원문과 자동 생성 Localization 키를 표시합니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(TutorialAuthoringGuidePage))]
    internal sealed class TutorialAuthoringGuidePageDrawer : PropertyDrawer
    {
        private const float VerticalSpacing = 2f;

        /// <summary>
        /// 가이드 페이지의 자식 필드를 모두 표시하는 데 필요한 높이를 계산합니다.
        /// </summary>
        /// <param name="property">가이드 페이지 SerializedProperty입니다.</param>
        /// <param name="label">배열 요소에 표시할 라벨입니다.</param>
        /// <returns>픽셀 단위의 전체 필드 높이입니다.</returns>
        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty sourceText =
                property.FindPropertyRelative("descriptionSourceKo");
            float sourceHeight = sourceText != null
                ? EditorGUI.GetPropertyHeight(sourceText, true)
                : EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight * 3f +
                   sourceHeight +
                   VerticalSpacing * 3f;
        }

        /// <summary>
        /// 한글 원문은 편집 가능하게, 자동 생성 키는 읽기 전용으로 그립니다.
        /// </summary>
        /// <param name="position">PropertyDrawer에 할당된 영역입니다.</param>
        /// <param name="property">가이드 페이지 SerializedProperty입니다.</param>
        /// <param name="label">배열 요소에 표시할 라벨입니다.</param>
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect row = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(row, label, EditorStyles.boldLabel);

            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = previousIndent + 1;

            MoveToNextRow(ref row);
            EditorGUI.PropertyField(
                row,
                property.FindPropertyRelative("guideSprite"),
                new GUIContent("가이드 Sprite"));

            SerializedProperty sourceText =
                property.FindPropertyRelative("descriptionSourceKo");
            float sourceHeight = sourceText != null
                ? EditorGUI.GetPropertyHeight(sourceText, true)
                : EditorGUIUtility.singleLineHeight;
            MoveToNextRow(ref row);
            row.height = sourceHeight;
            if (sourceText != null)
            {
                EditorGUI.PropertyField(
                    row,
                    sourceText,
                    new GUIContent("한글 설명"),
                    true);
            }

            row.y += row.height + VerticalSpacing;
            row.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty localizationKey =
                property.FindPropertyRelative("descriptionLocalizationKey");
            string keyText =
                string.IsNullOrWhiteSpace(localizationKey?.stringValue)
                    ? "JSON Export 시 자동 생성"
                    : localizationKey.stringValue;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.TextField(row, "Localization Key", keyText);
            }

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 현재 행을 한 줄 아래로 이동합니다.
        /// </summary>
        /// <param name="row">이동할 GUI 영역입니다.</param>
        private static void MoveToNextRow(ref Rect row)
        {
            row.y += EditorGUIUtility.singleLineHeight + VerticalSpacing;
            row.height = EditorGUIUtility.singleLineHeight;
        }
    }
}
