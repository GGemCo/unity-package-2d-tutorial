using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 액션 항목을 타입별로 편집하는 Drawer의 공통 기반입니다.
    /// </summary>
    internal abstract class TutorialActionDrawer
    {
        /// <summary>
        /// 액션 항목의 상세 필드를 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        public void Draw(SerializedProperty actionProperty)
        {
            if (actionProperty == null)
            {
                EditorGUILayout.HelpBox("액션 SerializedProperty가 비어 있습니다.", MessageType.Error);
                return;
            }

            DrawType(actionProperty);
            DrawTypeSpecificFields(actionProperty);
            DrawMemo(actionProperty);
        }

        /// <summary>
        /// 액션 타입에 맞는 전용 입력 필드를 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        protected abstract void DrawTypeSpecificFields(SerializedProperty actionProperty);

        /// <summary>
        /// 액션 타입 enum 필드를 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        protected static void DrawType(SerializedProperty actionProperty)
        {
            SerializedProperty typeProperty = actionProperty.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProperty, new GUIContent("액션 타입"));
        }

        /// <summary>
        /// 문자열 키 인자 필드를 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        /// <param name="label">표시 라벨입니다.</param>
        /// <param name="tooltip">도움말입니다.</param>
        protected static void DrawKey(SerializedProperty actionProperty, string label, string tooltip)
        {
            SerializedProperty keyProperty = actionProperty.FindPropertyRelative("key");
            EditorGUILayout.PropertyField(keyProperty, new GUIContent(label, tooltip));
        }

        /// <summary>
        /// 정수 인자 필드를 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        /// <param name="label">표시 라벨입니다.</param>
        /// <param name="tooltip">도움말입니다.</param>
        protected static void DrawIntValue(SerializedProperty actionProperty, string label, string tooltip)
        {
            SerializedProperty intValueProperty = actionProperty.FindPropertyRelative("intValue");
            EditorGUILayout.PropertyField(intValueProperty, new GUIContent(label, tooltip));
        }

        /// <summary>
        /// 문자열 배열 인자 목록을 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        /// <param name="label">표시 라벨입니다.</param>
        /// <param name="tooltip">도움말입니다.</param>
        protected static void DrawStringValues(SerializedProperty actionProperty, string label, string tooltip)
        {
            SerializedProperty stringValuesProperty = actionProperty.FindPropertyRelative("stringValues");
            if (stringValuesProperty == null)
            {
                return;
            }

            EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < stringValuesProperty.arraySize; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    SerializedProperty valueProperty = stringValuesProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(valueProperty, new GUIContent($"값 {i + 1}"));
                    if (GUILayout.Button("삭제", GUILayout.Width(50f)))
                    {
                        stringValuesProperty.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 12f);
                if (GUILayout.Button("문자열 값 추가", GUILayout.Width(120f)))
                {
                    int index = stringValuesProperty.arraySize;
                    stringValuesProperty.InsertArrayElementAtIndex(index);
                    SerializedProperty addedProperty = stringValuesProperty.GetArrayElementAtIndex(index);
                    if (addedProperty != null)
                    {
                        addedProperty.stringValue = string.Empty;
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 제작 메모 필드를 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        protected static void DrawMemo(SerializedProperty actionProperty)
        {
            SerializedProperty memoProperty = actionProperty.FindPropertyRelative("memo");
            EditorGUILayout.PropertyField(memoProperty, new GUIContent("제작 메모"));
        }
    }

    /// <summary>
    /// 선택된 액션 타입이 없을 때 사용하는 Drawer입니다.
    /// </summary>
    internal sealed class TutorialActionNoneDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            EditorGUILayout.HelpBox("액션 타입을 선택하십시오.", MessageType.Info);
        }
    }

    /// <summary>
    /// 안내 UI 표시 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionShowGuideDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawKey(actionProperty, "Guide Key / Message Key", "표시할 안내 UI 또는 로컬라이즈 메시지 키입니다.");
            DrawStringValues(actionProperty, "추가 문자열 인자", "위치, 앵커, 스타일 같은 외부 처리기용 선택 인자입니다.");
        }
    }

    /// <summary>
    /// 안내 UI 숨김 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionHideGuideDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawKey(actionProperty, "Guide Key", "숨길 안내 UI 식별자입니다. 비워두면 외부 처리기에서 전체 숨김으로 해석할 수 있습니다.");
        }
    }

    /// <summary>
    /// UI 하이라이트 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionHighlightUiDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawKey(actionProperty, "UI Target Key", "강조할 UI 대상 식별자입니다. 예: hud.attack-button");
            DrawStringValues(actionProperty, "하이라이트 옵션", "모양, 여백, 손가락 방향 등 외부 처리기용 선택 인자입니다.");
        }
    }

    /// <summary>
    /// UI 하이라이트 해제 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionClearHighlightDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawKey(actionProperty, "UI Target Key", "해제할 UI 대상 식별자입니다. 비워두면 전체 해제로 해석할 수 있습니다.");
        }
    }

    /// <summary>
    /// 입력 제한 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionBlockInputExceptDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawStringValues(actionProperty, "허용 입력 Action ID", "튜토리얼 중 허용할 입력 액션 식별자 목록입니다.");
            EditorGUILayout.HelpBox("입력 제한은 TutorialInputBlockPolicy에 저장되며, Control 입력 앞단의 Adapter가 이 정책을 조회해야 실제 차단됩니다.", MessageType.Info);
        }
    }

    /// <summary>
    /// 입력 제한 해제 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionClearInputBlockDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            EditorGUILayout.HelpBox("별도 인자 없이 현재 입력 제한을 모두 해제합니다.", MessageType.Info);
        }
    }

    /// <summary>
    /// 기능 해금 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionUnlockFeatureDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawKey(actionProperty, "Feature Key", "해금할 기능 식별자입니다.");
            DrawIntValue(actionProperty, "Feature UID", "기능을 정수 UID로 구분할 때 사용합니다. 사용하지 않으면 0으로 둡니다.");
        }
    }

    /// <summary>
    /// Quest 시작 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionStartQuestDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawIntValue(actionProperty, "Quest UID", "시작할 Quest UID입니다.");
        }
    }

    /// <summary>
    /// 사용자 정의 액션을 편집합니다.
    /// </summary>
    internal sealed class TutorialActionCustomDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawKey(actionProperty, "Custom Key", "외부 처리기가 해석할 사용자 정의 액션 키입니다.");
            DrawIntValue(actionProperty, "정수 인자", "외부 처리기에 전달할 정수 인자입니다.");
            DrawStringValues(actionProperty, "문자열 인자", "외부 처리기에 전달할 문자열 인자 목록입니다.");
        }
    }

    /// <summary>
    /// 알 수 없는 액션 타입을 안전하게 편집하기 위한 Drawer입니다.
    /// </summary>
    internal sealed class TutorialActionFallbackDrawer : TutorialActionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty actionProperty)
        {
            DrawKey(actionProperty, "Key", "외부 처리기에 전달할 문자열 키입니다.");
            DrawIntValue(actionProperty, "정수 인자", "외부 처리기에 전달할 정수 인자입니다.");
            DrawStringValues(actionProperty, "문자열 인자", "외부 처리기에 전달할 문자열 인자 목록입니다.");
        }
    }
}
