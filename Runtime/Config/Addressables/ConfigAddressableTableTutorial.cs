using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 패키지에서 사용하는 Addressables 테이블 리소스 정의를 관리합니다.
    /// </summary>
    public static class ConfigAddressableTableTutorial
    {
        /// <summary>
        /// Tutorial 런타임 테이블 팩 식별자입니다.
        /// </summary>
        public const string PackageId = "tutorial";

        /// <summary>
        /// Tutorial Catalog 역할을 수행하는 테이블 이름입니다.
        /// </summary>
        public const string Tutorial = "tutorial";

        /// <summary>
        /// Tutorial 테이블 Addressables 자산 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTutorial =
            ConfigAddressableTable.Make(Tutorial);

        /// <summary>
        /// Tutorial 패키지 런타임 테이블 팩 Addressables 자산 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TablePackTutorial =
            ConfigAddressableTablePack.Make(PackageId);

        /// <summary>
        /// Tutorial 패키지에서 로드해야 하는 개별 테이블 목록입니다.
        /// </summary>
        public static readonly List<AddressableAssetInfo> All = new List<AddressableAssetInfo>
        {
            TableTutorial,
        };
    }
}
