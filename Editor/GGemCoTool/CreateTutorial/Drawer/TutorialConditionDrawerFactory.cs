using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 조건 Drawer 진입점을 제공합니다.
    /// </summary>
    internal static class TutorialConditionDrawerFactory
    {
        private static readonly TutorialConditionDrawer Drawer = new TutorialConditionDrawer();

        public static void Draw(SerializedProperty conditionProperty)
        {
            Drawer.Draw(conditionProperty);
        }

        /// <summary>
        /// 이벤트와 현재 상태를 구분하는 자동 시작 조건 필드를 그립니다.
        /// </summary>
        /// <param name="conditionProperty">자동 시작 조건 SerializedProperty입니다.</param>
        public static void DrawStart(SerializedProperty conditionProperty)
        {
            Drawer.DrawStart(conditionProperty);
        }

        public static string BuildTitle(SerializedProperty conditionProperty, int index)
        {
            SerializedProperty typeProperty = conditionProperty?.FindPropertyRelative("type");
            TutorialEventType type = typeProperty != null
                ? (TutorialEventType)typeProperty.intValue
                : TutorialEventType.None;
            return $"조건 {index + 1} - {type}";
        }
    }
}
