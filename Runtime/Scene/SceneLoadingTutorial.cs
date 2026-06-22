using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 패키지의 테이블 로딩 단계를 Core 로딩 씬에 등록하는 씬 컴포넌트입니다.
    /// </summary>
    public sealed class SceneLoadingTutorial : DefaultScene
    {
        /// <summary>
        /// Addressables 로더 설정이 없으면 PreIntro 씬으로 되돌립니다.
        /// </summary>
        private void Awake()
        {
            if (!AddressableLoaderSettings.Instance)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(ConfigDefine.SceneNamePreIntro);
            }
        }

        /// <summary>
        /// 로딩 시작 직전 이벤트 훅을 구독합니다.
        /// </summary>
        private void OnEnable()
        {
            GameLoaderManager.BeforeLoadStartInLoadingScene += OnBeforeLoadStartInLoadingScene;
        }

        /// <summary>
        /// 로딩 시작 직전 이벤트 훅 구독을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            GameLoaderManager.BeforeLoadStartInLoadingScene -= OnBeforeLoadStartInLoadingScene;
        }

        /// <summary>
        /// 로딩 씬에서 실제 로딩이 시작되기 직전에 Tutorial 테이블 로딩 스텝을 등록합니다.
        /// </summary>
        /// <param name="sender">로딩 스텝을 등록할 GameLoaderManager입니다.</param>
        /// <param name="e">로딩 시작 직전 이벤트 인자입니다.</param>
        private static void OnBeforeLoadStartInLoadingScene(
            GameLoaderManager sender,
            GameLoaderManager.EventArgsBeforeLoadStart e)
        {
            TableLoaderManagerTutorial tableLoader =
                CompatObjectFind.FindFirst<TableLoaderManagerTutorial>() ??
                new GameObject(nameof(TableLoaderManagerTutorial))
                    .AddComponent<TableLoaderManagerTutorial>();

            TablePackLoadStep stepTable = new TablePackLoadStep(
                id: "core.table.tutorial",
                order: 247,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeTables(),
                tableLoader: tableLoader,
                tablePack: ConfigAddressableTableTutorial.TablePackTutorial,
                fallbackTables: ConfigAddressableTableTutorial.All);
            sender.Register(stepTable);

            // Tutorial 전용 저장 파일은 Core 저장 파일 로드 이후에 읽어
            // 기존 tutorial.progress 확장 섹션을 하위 호환 폴백으로 사용할 수 있게 합니다.
            SaveDataLoaderTutorial saveDataLoader =
                CompatObjectFind.FindFirst<SaveDataLoaderTutorial>() ??
                new GameObject(nameof(SaveDataLoaderTutorial))
                    .AddComponent<SaveDataLoaderTutorial>();
            var saveDataStep = new SaveDataLoadStep(
                id: "core.savedata.tutorial",
                order: 383,
                localizedKey:
                    LocalizationConstants.Keys.Loading.TextTypeSaveData(),
                saveDataLoader: saveDataLoader);
            sender.Register(saveDataStep);
        }
    }
}
