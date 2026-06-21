using GGemCo2DCoreEditor;
using GGemCo2DTutorial;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Tutorial 패키지 Editor 도구에서 사용하는 테이블 로더입니다.
    /// </summary>
    public static class TableLoaderManagerTutorialEditor
    {
        /// <summary>
        /// Tutorial 테이블을 Editor 파일 경로에서 로드합니다.
        /// </summary>
        /// <param name="forceReload">기존 캐시를 무시하고 다시 로드할지 여부입니다.</param>
        /// <returns>로드된 Tutorial 테이블입니다.</returns>
        public static TableTutorial LoadTutorialTable(bool forceReload = true)
        {
            return TableLoaderManagerBase.LoadTable<TableTutorial>(
                ConfigAddressableTableTutorial.TableTutorial.Path,
                forceReload);
        }
    }
}
