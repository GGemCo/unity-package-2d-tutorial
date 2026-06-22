using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 액션 Drawer 진입점을 제공합니다.
    /// </summary>
    internal static class TutorialActionDrawerFactory
    {
        private static readonly TutorialActionDrawer Drawer = new TutorialActionDrawer();

        public static void Draw(SerializedProperty actionProperty)
        {
            Drawer.Draw(actionProperty);
        }

        public static string BuildTitle(SerializedProperty actionProperty, int index)
        {
            SerializedProperty typeProperty = actionProperty?.FindPropertyRelative("type");
            TutorialActionType type = typeProperty != null
                ? (TutorialActionType)typeProperty.intValue
                : TutorialActionType.None;
            return $"액션 {index + 1} - {type}";
        }
    }
}
