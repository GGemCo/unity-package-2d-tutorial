using GGemCo2DCoreEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 에디터 확장(툴 메뉴)에서 사용하는 메뉴 경로 문자열과 정렬 순서를 정의합니다.
    /// </summary>
    /// <remarks>
    /// UnityEditor.MenuItem의 경로/우선순위(order) 값으로 사용되며,
    /// GGemCoToolMenu의 패키지/카테고리 경로를 기반으로 메뉴 트리를 구성합니다.
    /// </remarks>
    public static class ConfigEditorTutorial
    {
        /// <summary>
        /// Tutorial 패키지 Unity 메뉴 항목의 정렬 순서(order) 값을 정의합니다.
        /// </summary>
        /// <remarks>
        /// 공통 메뉴 우선순위 기준값에 Tutorial 내부 로컬 순서를 더해 메뉴 배치 순서를 결정합니다.
        /// </remarks>
        public enum ToolOrdering
        {
            /// <summary>자동 셋팅 메뉴 섹션의 시작 위치입니다.</summary>
            AutoSetting = GGemCoToolMenuPriority.TutorialSettings + 1,

            /// <summary>기본 셋팅 메뉴 섹션의 시작 위치입니다.</summary>
            DefaultSetting,

            /// <summary>Addressables 셋팅 메뉴의 위치입니다.</summary>
            SettingAddressable,

            /// <summary>개발 도구 메뉴 섹션의 시작 위치입니다.</summary>
            Development = GGemCoToolMenuPriority.TutorialDevelopment,
            CreateTutorial,

            /// <summary>테스트 도구 메뉴 섹션의 시작 위치입니다.</summary>
            Test = GGemCoToolMenuPriority.TutorialTest,
            Debug = GGemCoToolMenuPriority.TutorialDebug,

            /// <summary>기타 도구 메뉴 섹션의 시작 위치입니다.</summary>
            Etc = GGemCoToolMenuPriority.TutorialEtc
        }

        /// <summary>
        /// Tutorial 툴 메뉴의 최상위 경로 접두사입니다.
        /// </summary>
        private const string NameToolGGemCoTutorial = GGemCoToolMenu.Tutorial;

        // 기본 셋팅하기

        /// <summary>
        /// 기본 셋팅 메뉴(설정하기)의 경로 접두사입니다.
        /// </summary>
        private const string NameToolSettings = NameToolGGemCoTutorial + GGemCoToolMenu.Settings;

        /// <summary>
        /// "자동 셋팅하기" 메뉴 경로입니다.
        /// </summary>
        public const string NameToolSettingAuto = NameToolSettings + "자동 셋팅하기";

        /// <summary>
        /// "기본 셋팅하기" 메뉴 경로입니다.
        /// </summary>
        public const string NameToolSettingDefault = NameToolSettings + "기본 셋팅하기";

        /// <summary>
        /// "Addressable 셋팅하기" 메뉴 경로입니다.
        /// </summary>
        public const string NameToolSettingAddressable = NameToolSettings + "Addressable 셋팅하기";

        /// <summary>
        /// 개발툴 메뉴의 경로 접두사입니다.
        /// </summary>
        private const string NameToolDevelopment = NameToolGGemCoTutorial + GGemCoToolMenu.Development;

        public const string NameToolTutorial = NameToolDevelopment + "튜토리얼 생성툴";

        /// <summary>
        /// 테스트툴 메뉴의 경로 접두사입니다.
        /// </summary>
        private const string NameToolTest = NameToolGGemCoTutorial + GGemCoToolMenu.Test;
        
        // 디버그
        private const string NameToolDebug = NameToolGGemCoTutorial + GGemCoToolMenu.Debug;

        /// <summary>
        /// 기타 메뉴의 경로 접두사입니다.
        /// </summary>
        private const string NameToolEtc = NameToolGGemCoTutorial + GGemCoToolMenu.Etc;

        /// <summary>
        /// 패키지 내 Tutorial 에디터에서 참조하는 기본 경로(패키지 루트)입니다.
        /// </summary>
        public const string PathPackageCore = "Packages/com.ggemco.2d.tutorial";
    }
}
