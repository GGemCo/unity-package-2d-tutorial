using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 전용 저장 파일명과 논리 저장 식별자를 정의합니다.
    /// </summary>
    public static class SaveDataConstantsTutorial
    {
        /// <summary>
        /// 확장자를 제외한 Tutorial 저장 파일명입니다.
        /// </summary>
        public const string SaveDataFileName = "SaveDataTutorial";

        /// <summary>
        /// 확장자를 제외한 Tutorial 백업 저장 파일명입니다.
        /// </summary>
        public const string BackupFileNameWithoutExtension =
            SaveDataFileName + ".backup";

        /// <summary>
        /// Tutorial 저장 데이터의 암호화 AAD에 사용할 영역 ID입니다.
        /// </summary>
        public const string SaveDataScope = "tutorial";

        /// <summary>
        /// Tutorial 저장 파일 확장자입니다.
        /// </summary>
        public const string SaveDataFileExt = ".json";

        /// <summary>
        /// 확장자를 포함한 Tutorial 기본 저장 파일명을 반환합니다.
        /// </summary>
        public static string DefaultFileName =>
            $"{SaveDataFileName}{SaveDataFileExt}";

        /// <summary>
        /// 지정한 슬롯의 Tutorial 전용 논리 저장 식별자를 생성합니다.
        /// </summary>
        /// <param name="slotIndex">저장 슬롯 번호입니다.</param>
        /// <returns>Tutorial 저장 데이터 암호화에 사용할 논리 식별자입니다.</returns>
        public static SaveDataIdentity CreateIdentity(int slotIndex)
        {
            return new SaveDataIdentity(slotIndex, SaveDataScope);
        }
    }
}
