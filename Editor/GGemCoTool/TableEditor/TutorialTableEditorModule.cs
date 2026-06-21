using System.Collections.Generic;
using GGemCo2DCoreEditor;
using GGemCo2DTutorial;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Core 테이블 에디터에 Tutorial 테이블 정의를 제공합니다.
    /// </summary>
    internal sealed class TutorialTableEditorModule : ITableEditorModule
    {
        /// <summary>
        /// 테이블 에디터 모듈 이름입니다.
        /// </summary>
        public string ModuleName => "Tutorial";

        /// <summary>
        /// 테이블 에디터 패키지 분류 이름입니다.
        /// </summary>
        public string PackageName => "Tutorial";

        /// <summary>
        /// Tutorial 패키지에서 편집할 테이블 정의를 생성합니다.
        /// </summary>
        /// <returns>Tutorial 테이블 정의 목록입니다.</returns>
        public IEnumerable<TableEditorTableDefinition> BuildDefinitions()
        {
            yield return TableEditorDefinitionFactory.Create(
                ModuleName,
                PackageName,
                ConfigAddressableTableTutorial.Tutorial,
                ConfigAddressableTableTutorial.TableTutorial.Path,
                ConfigAddressableTableTutorial.Tutorial,
                typeof(TableTutorial),
                typeof(StruckTableTutorial),
                TableEditorDefinitionFactory.CreateDefaultReloadAction(
                    ConfigAddressableTableTutorial.TableTutorial.Path),
                TableEditorRegistry.FindReferenceTable);
        }
    }
}
