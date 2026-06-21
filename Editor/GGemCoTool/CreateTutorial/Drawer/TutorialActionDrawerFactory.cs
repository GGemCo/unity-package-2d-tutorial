using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 액션 타입에 맞는 TutorialActionDrawer를 선택합니다.
    /// </summary>
    internal static class TutorialActionDrawerFactory
    {
        private static readonly TutorialActionNoneDrawer NoneDrawer = new TutorialActionNoneDrawer();
        private static readonly TutorialActionShowGuideDrawer ShowGuideDrawer = new TutorialActionShowGuideDrawer();
        private static readonly TutorialActionHideGuideDrawer HideGuideDrawer = new TutorialActionHideGuideDrawer();
        private static readonly TutorialActionHighlightUiDrawer HighlightUiDrawer = new TutorialActionHighlightUiDrawer();
        private static readonly TutorialActionClearHighlightDrawer ClearHighlightDrawer = new TutorialActionClearHighlightDrawer();
        private static readonly TutorialActionBlockInputExceptDrawer BlockInputExceptDrawer = new TutorialActionBlockInputExceptDrawer();
        private static readonly TutorialActionClearInputBlockDrawer ClearInputBlockDrawer = new TutorialActionClearInputBlockDrawer();
        private static readonly TutorialActionUnlockFeatureDrawer UnlockFeatureDrawer = new TutorialActionUnlockFeatureDrawer();
        private static readonly TutorialActionStartQuestDrawer StartQuestDrawer = new TutorialActionStartQuestDrawer();
        private static readonly TutorialActionCustomDrawer CustomDrawer = new TutorialActionCustomDrawer();
        private static readonly TutorialActionFallbackDrawer FallbackDrawer = new TutorialActionFallbackDrawer();

        /// <summary>
        /// 액션 타입에 맞는 상세 UI를 그립니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        public static void Draw(SerializedProperty actionProperty)
        {
            TutorialActionDrawer drawer = GetDrawer(GetActionType(actionProperty));
            drawer.Draw(actionProperty);
        }

        /// <summary>
        /// 액션 목록에 표시할 제목을 생성합니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        /// <param name="index">액션 인덱스입니다.</param>
        /// <returns>접이식 헤더에 표시할 제목입니다.</returns>
        public static string BuildTitle(SerializedProperty actionProperty, int index)
        {
            TutorialActionType type = GetActionType(actionProperty);
            string suffix = type == TutorialActionType.None ? "타입 미지정" : type.ToString();
            return $"액션 {index + 1} - {suffix}";
        }

        /// <summary>
        /// SerializedProperty에서 액션 타입을 읽습니다.
        /// </summary>
        /// <param name="actionProperty">액션 SerializedProperty입니다.</param>
        /// <returns>액션 타입입니다.</returns>
        private static TutorialActionType GetActionType(SerializedProperty actionProperty)
        {
            SerializedProperty typeProperty = actionProperty?.FindPropertyRelative("type");
            if (typeProperty == null)
            {
                return TutorialActionType.None;
            }

            int value = typeProperty.intValue;
            return System.Enum.IsDefined(typeof(TutorialActionType), value)
                ? (TutorialActionType)value
                : TutorialActionType.None;
        }

        /// <summary>
        /// 액션 타입에 맞는 Drawer 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="type">액션 타입입니다.</param>
        /// <returns>액션 Drawer입니다.</returns>
        private static TutorialActionDrawer GetDrawer(TutorialActionType type)
        {
            switch (type)
            {
                case TutorialActionType.None:
                    return NoneDrawer;
                case TutorialActionType.ShowGuide:
                    return ShowGuideDrawer;
                case TutorialActionType.HideGuide:
                    return HideGuideDrawer;
                case TutorialActionType.HighlightUi:
                    return HighlightUiDrawer;
                case TutorialActionType.ClearHighlight:
                    return ClearHighlightDrawer;
                case TutorialActionType.BlockInputExcept:
                    return BlockInputExceptDrawer;
                case TutorialActionType.ClearInputBlock:
                    return ClearInputBlockDrawer;
                case TutorialActionType.UnlockFeature:
                    return UnlockFeatureDrawer;
                case TutorialActionType.StartQuest:
                    return StartQuestDrawer;
                case TutorialActionType.Custom:
                    return CustomDrawer;
                default:
                    return FallbackDrawer;
            }
        }
    }
}
