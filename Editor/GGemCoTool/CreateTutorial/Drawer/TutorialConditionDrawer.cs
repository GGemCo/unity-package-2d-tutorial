using GGemCo2DTutorial;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 조건 항목을 타입별로 편집하는 Drawer의 공통 기반입니다.
    /// </summary>
    internal abstract class TutorialConditionDrawer
    {
        /// <summary>
        /// 조건 항목의 상세 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        public void Draw(SerializedProperty conditionProperty)
        {
            if (conditionProperty == null)
            {
                EditorGUILayout.HelpBox("조건 SerializedProperty가 비어 있습니다.", MessageType.Error);
                return;
            }

            DrawType(conditionProperty);
            DrawTypeSpecificFields(conditionProperty);
            DrawRequiredCount(conditionProperty);
            DrawMemo(conditionProperty);
        }

        /// <summary>
        /// 조건 타입에 맞는 전용 입력 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        protected abstract void DrawTypeSpecificFields(SerializedProperty conditionProperty);

        /// <summary>
        /// 조건 타입 enum 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        protected static void DrawType(SerializedProperty conditionProperty)
        {
            SerializedProperty typeProperty = conditionProperty.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProperty, new GUIContent("조건 타입"));
        }

        /// <summary>
        /// 문자열 비교 키 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        /// <param name="label">표시 라벨입니다.</param>
        /// <param name="tooltip">도움말입니다.</param>
        protected static void DrawKey(SerializedProperty conditionProperty, string label, string tooltip)
        {
            SerializedProperty keyProperty = conditionProperty.FindPropertyRelative("key");
            EditorGUILayout.PropertyField(keyProperty, new GUIContent(label, tooltip));
        }

        /// <summary>
        /// 정수 비교 값 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        /// <param name="label">표시 라벨입니다.</param>
        /// <param name="tooltip">도움말입니다.</param>
        protected static void DrawIntValue(SerializedProperty conditionProperty, string label, string tooltip)
        {
            SerializedProperty intValueProperty = conditionProperty.FindPropertyRelative("intValue");
            EditorGUILayout.PropertyField(intValueProperty, new GUIContent(label, tooltip));
        }

        /// <summary>
        /// 필요 누적 횟수 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        protected static void DrawRequiredCount(SerializedProperty conditionProperty)
        {
            SerializedProperty requiredCountProperty = conditionProperty.FindPropertyRelative("requiredCount");
            if (requiredCountProperty == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(requiredCountProperty, new GUIContent("필요 횟수", "조건이 완료되기 위해 필요한 누적 횟수입니다."));
            if (requiredCountProperty.intValue <= 0)
            {
                requiredCountProperty.intValue = 1;
            }
        }

        /// <summary>
        /// 제작 메모 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        protected static void DrawMemo(SerializedProperty conditionProperty)
        {
            SerializedProperty memoProperty = conditionProperty.FindPropertyRelative("memo");
            EditorGUILayout.PropertyField(memoProperty, new GUIContent("제작 메모"));
        }

        /// <summary>
        /// 조건 매칭 규칙에 대한 공통 도움말을 그립니다.
        /// </summary>
        protected static void DrawMatchHelpBox()
        {
            EditorGUILayout.HelpBox(
                "Key가 비어 있으면 문자열 비교를 생략하고, 정수 값이 0 이하이면 정수 비교를 생략합니다.",
                MessageType.Info);
        }
    }

    /// <summary>
    /// 선택된 조건 타입이 없을 때 사용하는 Drawer입니다.
    /// </summary>
    internal sealed class TutorialConditionNoneDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            EditorGUILayout.HelpBox("조건 타입을 선택하십시오.", MessageType.Info);
        }
    }

    /// <summary>
    /// 맵 입장 조건을 편집합니다.
    /// </summary>
    internal sealed class TutorialConditionEnterMapDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawIntValue(conditionProperty, "Map UID", "입장해야 하는 맵 UID입니다. 0이면 모든 맵 입장을 허용합니다.");
        }
    }

    /// <summary>
    /// 몬스터 처치 조건을 편집합니다.
    /// </summary>
    internal sealed class TutorialConditionKillMonsterDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawIntValue(conditionProperty, "Monster UID", "처치해야 하는 몬스터 UID입니다. 0이면 모든 몬스터 처치를 허용합니다.");
        }
    }

    /// <summary>
    /// 입력 액션 조건을 편집합니다.
    /// </summary>
    internal sealed class TutorialConditionInputActionDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawKey(conditionProperty, "Input Action ID", "TutorialEventBus.PublishInputAction에 전달되는 입력 액션 식별자입니다.");
        }
    }

    /// <summary>
    /// UI 창 열림 조건을 편집합니다.
    /// </summary>
    internal sealed class TutorialConditionOpenWindowDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawKey(conditionProperty, "Window Key", "열려야 하는 UI 창 식별자입니다.");
            DrawIntValue(conditionProperty, "Window UID", "창을 정수 UID로 비교할 때 사용합니다. 사용하지 않으면 0으로 둡니다.");
            DrawMatchHelpBox();
        }
    }

    /// <summary>
    /// UI 클릭 조건을 편집합니다.
    /// </summary>
    internal sealed class TutorialConditionUiClickedDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawKey(conditionProperty, "UI Target Key", "클릭해야 하는 UI 대상 식별자입니다. 예: hud.attack-button");
        }
    }

    /// <summary>
    /// Quest 상태 조건을 편집합니다.
    /// </summary>
    internal sealed class TutorialConditionQuestDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawIntValue(conditionProperty, "Quest UID", "시작 또는 완료 상태를 감지할 Quest UID입니다.");
        }
    }

    /// <summary>
    /// 사용자 정의 조건을 편집합니다.
    /// </summary>
    internal sealed class TutorialConditionCustomDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawKey(conditionProperty, "Custom Key", "게임 상위 계층 Adapter가 발행하는 사용자 정의 이벤트 키입니다.");
            DrawIntValue(conditionProperty, "정수 값", "사용자 정의 이벤트의 정수 비교 값입니다. 사용하지 않으면 0으로 둡니다.");
            DrawMatchHelpBox();
        }
    }

    /// <summary>
    /// 알 수 없는 조건 타입을 안전하게 편집하기 위한 Drawer입니다.
    /// </summary>
    internal sealed class TutorialConditionFallbackDrawer : TutorialConditionDrawer
    {
        /// <inheritdoc />
        protected override void DrawTypeSpecificFields(SerializedProperty conditionProperty)
        {
            DrawKey(conditionProperty, "Key", "조건 비교에 사용할 문자열 키입니다.");
            DrawIntValue(conditionProperty, "정수 값", "조건 비교에 사용할 정수 값입니다.");
            DrawMatchHelpBox();
        }
    }
}
