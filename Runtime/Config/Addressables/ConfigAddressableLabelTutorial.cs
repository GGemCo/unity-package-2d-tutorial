using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 도메인에서 사용하는 Addressables Label 네이밍 규칙을 정의한다.
    /// </summary>
    /// <remarks>
    /// - Label은 여러 에셋을 논리적으로 묶어 조회하거나 일괄 로딩하기 위한 식별자이다.
    /// - 문자열 하드코딩을 방지하고, SDK 공통 접두사(<see cref="ConfigDefine.NameSDK"/>)를 일관되게 적용한다.
    /// - Group(<see cref="ConfigAddressableGroupNameTutorial"/>) 및 Key(<see cref="ConfigAddressableKeyTutorial"/>)와
    ///   역할이 다르므로 혼용하지 않도록 한다.
    /// </remarks>
    public static class ConfigAddressableLabelTutorial
    {
        /// <summary>
        /// Tutorial JSON 에셋 라벨입니다.
        /// </summary>
        public const string Tutorial = ConfigDefine.NameSDK + "_Tutorial";

        /// <summary>
        /// 생성 도구가 자동 등록한 튜토리얼 가이드 Sprite 라벨입니다.
        /// </summary>
        public const string GuideSprite = Tutorial + "_GuideSprite";
    }
}
