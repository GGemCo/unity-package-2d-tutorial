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
        [SerializeField]
        [Tooltip("Tutorial Catalog TextAsset의 Addressables 키입니다.")]
        private string catalogAddressableKey = TutorialConstants.DefaultCatalogKey;

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

        private readonly TutorialCoreEventSubscriber _coreEventSubscriber =
            new TutorialCoreEventSubscriber();
        private Coroutine _initializeCoroutine;
        private SceneGame _sceneGame;

        /// <summary>
        /// 씬에 Tutorial 패키지 매니저가 없으면 기본 설정으로 자동 생성합니다.
        /// 씬에 직접 배치한 인스턴스가 있으면 해당 인스턴스의 Catalog 키를 우선 사용합니다.
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

        private IEnumerator InitializeWhenReady()
        {
            while (SceneGame.Instance == null || SceneGame.Instance.saveDataManager == null)
            {
                yield return null;
            }

            _sceneGame = SceneGame.Instance;
            TutorialData = new TutorialData();
            TutorialData.Register();

            TutorialManager = new TutorialManager(TutorialData);
            var initializeTask = TutorialManager.InitializeAsync(catalogAddressableKey);
            while (!initializeTask.IsCompleted)
            {
                yield return null;
            }

            if (initializeTask.IsCanceled ||
                initializeTask.IsFaulted ||
                !initializeTask.Result)
            {
                GcLogger.LogError(
                    $"Tutorial Runtime 초기화에 실패했습니다. catalogKey: {catalogAddressableKey}");
                Destroy(gameObject);
                yield break;
            }

            _coreEventSubscriber.Subscribe();
            _sceneGame.OnSceneGameDestroyed += HandleSceneGameDestroyed;
            _initializeCoroutine = null;
        }

        private void HandleSceneGameDestroyed()
        {
            Destroy(gameObject);
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
            TutorialData?.Unregister();
            TutorialManager = null;
            TutorialData = null;
            _sceneGame = null;

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
