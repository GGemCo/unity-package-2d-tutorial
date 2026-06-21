using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 패키지의 설정 ScriptableObject Addressables 정보를 정의합니다.
    /// </summary>
    public static class ConfigAddressableSettingTutorial
    {
        /// <summary>
        /// Tutorial 런타임 설정 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TutorialSettings =
            ConfigAddressableSetting.Make(nameof(TutorialSettings));

        /// <summary>
        /// Tutorial 로딩 과정에서 사용할 설정 에셋 목록입니다.
        /// </summary>
        public static readonly List<AddressableAssetInfo> NeedLoadInLoadingScene = new()
        {
            TutorialSettings,
        };
    }
}
