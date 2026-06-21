using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 테이블을 Addressables에 등록하는 에디터 창입니다.
    /// </summary>
    public sealed class AddressableEditorTutorial : DefaultEditorWindow
    {
        private const string Title = "Addressable 셋팅하기";

        private SettingTableTutorial _settingTableTutorial;
        private SettingTutorial _settingTutorial;
        private Vector2 _scrollPosition;

        /// <summary>
        /// 2열 레이아웃에서 사용하는 버튼 폭입니다.
        /// </summary>
        public float ButtonWidth { get; private set; }

        /// <summary>
        /// 설정 실행 버튼의 기본 높이입니다.
        /// </summary>
        public float ButtonHeight { get; private set; }

        /// <summary>
        /// Tutorial Addressables 설정 창을 엽니다.
        /// </summary>
        [MenuItem(ConfigEditorTutorial.NameToolSettingAddressable, false, (int)ConfigEditorTutorial.ToolOrdering.SettingAddressable)]
        public static void ShowWindow()
        {
            GetWindow<AddressableEditorTutorial>(Title);
        }

        /// <summary>
        /// 창이 활성화될 때 Addressables 설정 모듈을 준비합니다.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            ButtonHeight = 40f;
            _settingTableTutorial = new SettingTableTutorial(this);
            _settingTutorial = new SettingTutorial(this);
        }

        /// <summary>
        /// Tutorial Addressables 설정 UI를 그립니다.
        /// </summary>
        private void OnGUI()
        {
            ButtonWidth = position.width / 2f - 10f;

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;

                using (new EditorGUILayout.HorizontalScope())
                {
                    _settingTableTutorial?.OnGUI();
                    _settingTutorial?.OnGUI();
                }

                EditorGUILayout.Space(20f);
            }
        }
    }
}
