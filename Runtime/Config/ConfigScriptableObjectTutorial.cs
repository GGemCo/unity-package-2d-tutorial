using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 패키지 ScriptableObject 생성 메뉴 규칙을 정의합니다.
    /// </summary>
    public static class ConfigScriptableObjectTutorial
    {
        /// <summary>
        /// ScriptableObject 생성 메뉴의 기본 경로입니다.
        /// </summary>
        public const string BasePath = ConfigDefine.NameSDK + "/Settings/";

        /// <summary>
        /// ScriptableObject 파일명의 공통 접두사입니다.
        /// </summary>
        public const string BaseName = ConfigDefine.NameSDK;

        /// <summary>
        /// Tutorial 설정 에셋 생성 규칙입니다.
        /// </summary>
        public static class Tutorial
        {
            /// <summary>
            /// Tutorial 설정 에셋의 기본 파일명입니다.
            /// </summary>
            public const string FileName = BaseName + "TutorialSettings";

            /// <summary>
            /// Tutorial 설정 에셋 생성 메뉴 경로입니다.
            /// </summary>
            public const string MenuName = BasePath + FileName;

            /// <summary>
            /// Tutorial 설정 에셋 생성 메뉴 정렬 순서입니다.
            /// </summary>
            public const int Ordering = 6000;
        }
    }
}
