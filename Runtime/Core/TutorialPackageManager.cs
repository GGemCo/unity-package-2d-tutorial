using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 저장 데이터, 실행 매니저와 Core 이벤트 연결의 수명주기를 관리합니다.
    /// </summary>
    public sealed class TutorialPackageManager : MonoBehaviour
    {
        /// <summary>
        /// 현재 게임에서 사용하는 Tutorial 패키지 매니저입니다.
        /// </summary>
        public static TutorialPackageManager Instance { get; private set; }

        /// <summary>
        /// 현재 Tutorial 실행 매니저입니다.
        /// </summary>
        public TutorialManager TutorialManager { get; private set; }

        /// <summary>
        /// 현재 Tutorial 저장 데이터입니다.
        /// </summary>
        public TutorialData TutorialData { get; private set; }

        /// <summary>
        /// Tutorial 전용 저장 파일의 복원과 저장을 담당하는 매니저입니다.
        /// </summary>
        public SaveDataManagerTutorial SaveDataManagerTutorial { get; private set; }

        private readonly TutorialCoreEventSubscriber _coreEventSubscriber =
            new TutorialCoreEventSubscriber();
        private Coroutine _initializeCoroutine;
        private SceneGame _sceneGame;

        /// <summary>
        /// 씬에 Tutorial 패키지 매니저가 없으면 자동 생성합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null ||
                CompatObjectFind.FindFirst<TutorialPackageManager>() != null)
            {
                return;
            }

            new GameObject(nameof(TutorialPackageManager))
                .AddComponent<TutorialPackageManager>();
        }

        /// <summary>
        /// 중복 인스턴스를 제거하고 씬 전환에도 패키지 매니저를 유지합니다.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Core 게임 씬과 저장 매니저가 준비된 후 Tutorial Runtime을 초기화합니다.
        /// </summary>
        private void Start()
        {
            if (Instance == this)
            {
                _initializeCoroutine = StartCoroutine(InitializeWhenReady());
            }
        }

        /// <summary>
        /// Core 게임 씬과 저장 매니저가 준비될 때까지 대기한 뒤 Tutorial Runtime을 초기화합니다.
        /// </summary>
        private IEnumerator InitializeWhenReady()
        {
            while (SceneGame.Instance == null || SceneGame.Instance.saveDataManager == null)
            {
                yield return null;
            }

            _sceneGame = SceneGame.Instance;
            GameObject managerContainer = GameObject.Find("Managers");
            if (managerContainer == null)
            {
                GcLogger.LogError(
                    "Tutorial 저장 매니저를 생성할 Managers 오브젝트가 없습니다.");
                _initializeCoroutine = null;
                Destroy(gameObject);
                yield break;
            }

            SaveDataManagerTutorial =
                _sceneGame.CreateManager<SaveDataManagerTutorial>(
                    managerContainer);
            SaveDataManagerTutorial.Initialize(new GameInitContext(
                _sceneGame,
                TableLoaderManager.Instance,
                AddressableLoaderSettings.Instance));
            if (!SaveDataManagerTutorial.IsInitialized)
            {
                GcLogger.LogError(
                    "Tutorial 저장 매니저를 초기화하지 못했습니다.");
                Destroy(SaveDataManagerTutorial.gameObject);
                SaveDataManagerTutorial = null;
                _initializeCoroutine = null;
                Destroy(gameObject);
                yield break;
            }

            TutorialData = SaveDataManagerTutorial.Tutorial;
            if (TutorialData == null)
            {
                GcLogger.LogError(
                    "Tutorial 저장 데이터를 초기화하지 못했습니다.");
                Destroy(SaveDataManagerTutorial.gameObject);
                SaveDataManagerTutorial = null;
                _initializeCoroutine = null;
                Destroy(gameObject);
                yield break;
            }

            TutorialManager = new TutorialManager(TutorialData);
            ITutorialCatalogProvider tableCatalogProvider = CreateTableCatalogProvider();
            if (tableCatalogProvider == null)
            {
                GcLogger.LogError(
                    "Tutorial Runtime 초기화에 실패했습니다. 로드된 tutorial 테이블 데이터가 없습니다.");
                Destroy(gameObject);
                yield break;
            }

            var initializeTask = TutorialManager.InitializeAsync(tableCatalogProvider);
            while (!initializeTask.IsCompleted)
            {
                yield return null;
            }

            if (initializeTask.IsCanceled ||
                initializeTask.IsFaulted ||
                !initializeTask.Result)
            {
                GcLogger.LogError("Tutorial Runtime 초기화에 실패했습니다. tutorial 테이블을 확인하십시오.");
                Destroy(gameObject);
                yield break;
            }

            _coreEventSubscriber.Subscribe();
            _sceneGame.OnSceneGameDestroyed += HandleSceneGameDestroyed;
            _initializeCoroutine = null;
        }

        /// <summary>
        /// Game 씬 종료 이벤트를 받으면 Tutorial 패키지 매니저를 제거합니다.
        /// </summary>
        private void HandleSceneGameDestroyed()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// 로드된 TableTutorial이 있으면 테이블 기반 Catalog 공급자를 생성합니다.
        /// </summary>
        /// <returns>테이블 기반 Catalog 공급자입니다. 사용할 테이블이 없으면 null입니다.</returns>
        private static ITutorialCatalogProvider CreateTableCatalogProvider()
        {
            TableLoaderManagerTutorial tableLoader = TableLoaderManagerTutorial.Instance;
            if (tableLoader == null || tableLoader.TableTutorial.GetCount() <= 0)
            {
                return null;
            }

            return new TableTutorialCatalogProvider(
                tableLoader.TableTutorial,
                tableLoader.TableTutorialStartCondition);
        }

        /// <summary>
        /// 패키지 매니저가 제거될 때 이벤트, 저장 기여자와 비동기 저장소를 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            if (_initializeCoroutine != null)
            {
                StopCoroutine(_initializeCoroutine);
                _initializeCoroutine = null;
            }

            if (_sceneGame != null)
            {
                _sceneGame.OnSceneGameDestroyed -= HandleSceneGameDestroyed;
            }

            _coreEventSubscriber.Unsubscribe();
            TutorialManager?.Dispose();

            if (SaveDataManagerTutorial != null)
            {
                Destroy(SaveDataManagerTutorial.gameObject);
            }

            TutorialManager = null;
            TutorialData = null;
            SaveDataManagerTutorial = null;
            _sceneGame = null;

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
