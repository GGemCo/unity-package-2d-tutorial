namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 정의 JSON 파일명과 Addressables 주소 규칙을 중앙에서 계산합니다.
    /// </summary>
    public static class TutorialAddressableKeyUtility
    {
        private const string DefinitionAddressRoot = "Tutorial";
        private const string DefinitionFilePrefix = "tutorial_";
        private const string JsonExtension = ".json";

        /// <summary>
        /// 지정한 Tutorial UID에 대응되는 Tutorial JSON 파일명을 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 Tutorial JSON 파일명입니다.</returns>
        public static string GetDefinitionFileName(int uid)
        {
            return uid > 0 ? $"{DefinitionFilePrefix}{uid}{JsonExtension}" : $"{DefinitionFilePrefix}0{JsonExtension}";
        }

        /// <summary>
        /// 지정한 Tutorial UID에 대응되는 Tutorial JSON Addressables 주소를 반환합니다.
        /// </summary>
        /// <param name="uid">Tutorial UID입니다.</param>
        /// <returns>규칙 기반 Addressables 주소입니다. UID가 유효하지 않으면 null입니다.</returns>
        public static string GetDefinitionAddressableKey(int uid)
        {
            return uid > 0 ? $"{DefinitionAddressRoot}/{DefinitionFilePrefix}{uid}" : null;
        }
    }
}
