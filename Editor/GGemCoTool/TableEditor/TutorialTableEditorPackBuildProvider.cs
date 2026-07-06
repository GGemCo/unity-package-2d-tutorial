using System.Collections.Generic;
using GGemCo2DTutorial;
using GGemCo2DCore;
using GGemCo2DCoreEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 패키지 테이블 저장 후 Tutorial 런타임 테이블 pack을 재생성하는 Provider입니다.
    /// </summary>
    internal sealed class TutorialTableEditorPackBuildProvider : TableEditorPackBuildProviderBase
    {
        /// <summary>
        /// Provider가 담당하는 패키지 표시 이름입니다.
        /// </summary>
        public override string PackageName => "Tutorial";

        /// <summary>
        /// Tutorial 런타임 pack 내부에 기록할 패키지 식별자입니다.
        /// </summary>
        protected override string PackageId => ConfigAddressableTableTutorial.PackageId;

        /// <summary>
        /// Tutorial 런타임 테이블 pack의 Addressables 정보입니다.
        /// </summary>
        protected override AddressableAssetInfo PackInfo => ConfigAddressableTableTutorial.TablePackTutorial;

        /// <summary>
        /// Tutorial 런타임 pack에 포함할 개별 테이블 목록입니다.
        /// </summary>
        protected override IReadOnlyList<AddressableAssetInfo> Tables => ConfigAddressableTableTutorial.All;
    }
}
