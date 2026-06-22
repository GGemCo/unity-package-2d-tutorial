using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 도메인에서 사용하는 Addressables Key 규칙을 정의한다.
    /// </summary>
    /// <remarks>
    /// - Addressables 로드 시 사용하는 Key 문자열을 중앙에서 관리한다.
    /// - 문자열 하드코딩을 방지하고, SDK 공통 접두사(<see cref="ConfigDefine.NameSDK"/>)를 일관되게 적용한다.
    /// - Group 이름(<see cref="ConfigAddressableGroupNameTutorial"/>)과는 역할이 다르므로 혼용하지 않도록 한다.
    /// </remarks>
    public static class ConfigAddressableKeyTutorial
    {
        private const string JsonExtension = ".json";

        /// <summary>
        /// Tutorial JSON 에셋 키 접두사입니다.
        /// </summary>
        public const string Tutorial = ConfigDefine.NameSDK + "_Tutorial";

        /// <summary>
        /// 지정한 Tutorial UID에 대응되는 Tutorial JSON Addressables 주소를 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 Addressables 주소입니다. UID가 유효하지 않으면 null입니다.</returns>
        public static string GetDefinitionAddressableKey(int uid)
        {
            return uid > 0 ? $"{Tutorial}_{uid}" : null;
        }

        /// <summary>
        /// 지정한 Tutorial UID에 대응되는 Tutorial JSON 파일명을 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 Tutorial JSON 파일명입니다.</returns>
        public static string GetDefinitionFileName(int uid)
        {
            return uid > 0 ? $"tutorial_{uid}{JsonExtension}" : null;
        }
    }
}
