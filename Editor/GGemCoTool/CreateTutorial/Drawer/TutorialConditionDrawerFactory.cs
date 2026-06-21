using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 조건 타입에 맞는 TutorialConditionDrawer를 선택합니다.
    /// </summary>
    internal static class TutorialConditionDrawerFactory
    {
        private static readonly TutorialConditionNoneDrawer NoneDrawer = new TutorialConditionNoneDrawer();
        private static readonly TutorialConditionEnterMapDrawer EnterMapDrawer = new TutorialConditionEnterMapDrawer();
        private static readonly TutorialConditionKillMonsterDrawer KillMonsterDrawer = new TutorialConditionKillMonsterDrawer();
        private static readonly TutorialConditionInputActionDrawer InputActionDrawer = new TutorialConditionInputActionDrawer();
        private static readonly TutorialConditionOpenWindowDrawer OpenWindowDrawer = new TutorialConditionOpenWindowDrawer();
        private static readonly TutorialConditionUiClickedDrawer UiClickedDrawer = new TutorialConditionUiClickedDrawer();
        private static readonly TutorialConditionQuestDrawer QuestDrawer = new TutorialConditionQuestDrawer();
        private static readonly TutorialConditionCustomDrawer CustomDrawer = new TutorialConditionCustomDrawer();
        private static readonly TutorialConditionFallbackDrawer FallbackDrawer = new TutorialConditionFallbackDrawer();

        /// <summary>
        /// 조건 타입에 맞는 상세 UI를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        public static void Draw(SerializedProperty conditionProperty)
        {
            TutorialConditionDrawer drawer = GetDrawer(GetConditionType(conditionProperty));
            drawer.Draw(conditionProperty);
        }

        /// <summary>
        /// 조건 목록에 표시할 제목을 생성합니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        /// <param name="index">조건 인덱스입니다.</param>
        /// <returns>접이식 헤더에 표시할 제목입니다.</returns>
        public static string BuildTitle(SerializedProperty conditionProperty, int index)
        {
            TutorialEventType type = GetConditionType(conditionProperty);
            string suffix = type == TutorialEventType.None ? "타입 미지정" : type.ToString();
            return $"조건 {index + 1} - {suffix}";
        }

        /// <summary>
        /// SerializedProperty에서 조건 타입을 읽습니다.
        /// </summary>
        /// <param name="conditionProperty">조건 SerializedProperty입니다.</param>
        /// <returns>조건 이벤트 타입입니다.</returns>
        private static TutorialEventType GetConditionType(SerializedProperty conditionProperty)
        {
            SerializedProperty typeProperty = conditionProperty?.FindPropertyRelative("type");
            if (typeProperty == null)
            {
                return TutorialEventType.None;
            }

            int value = typeProperty.intValue;
            return System.Enum.IsDefined(typeof(TutorialEventType), value)
                ? (TutorialEventType)value
                : TutorialEventType.None;
        }

        /// <summary>
        /// 조건 타입에 맞는 Drawer 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="type">조건 이벤트 타입입니다.</param>
        /// <returns>조건 Drawer입니다.</returns>
        private static TutorialConditionDrawer GetDrawer(TutorialEventType type)
        {
            switch (type)
            {
                case TutorialEventType.None:
                    return NoneDrawer;
                case TutorialEventType.EnterMap:
                    return EnterMapDrawer;
                case TutorialEventType.KillMonster:
                    return KillMonsterDrawer;
                case TutorialEventType.InputAction:
                    return InputActionDrawer;
                case TutorialEventType.OpenWindow:
                    return OpenWindowDrawer;
                case TutorialEventType.UiClicked:
                    return UiClickedDrawer;
                case TutorialEventType.QuestStarted:
                case TutorialEventType.QuestCompleted:
                    return QuestDrawer;
                case TutorialEventType.Custom:
                    return CustomDrawer;
                default:
                    return FallbackDrawer;
            }
        }
    }
}
