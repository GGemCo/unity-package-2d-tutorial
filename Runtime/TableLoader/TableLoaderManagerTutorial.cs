using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 패키지 전용 테이블을 등록하고 조회합니다.
    /// </summary>
    public sealed class TableLoaderManagerTutorial : TableLoaderBase
    {
        /// <summary>
        /// 현재 Tutorial 테이블 로더 인스턴스입니다.
        /// </summary>
        public static TableLoaderManagerTutorial Instance { get; private set; }

        /// <summary>
        /// Tutorial Catalog 역할을 수행하는 기본 테이블입니다.
        /// </summary>
        public TableTutorial TableTutorial { get; } = new TableTutorial();

        /// <summary>
        /// Tutorial 테이블 레지스트리를 초기화합니다.
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
            registry = new TableRegistry();
            registry.Register(TableTutorial);
        }

        /// <summary>
        /// 인스턴스가 제거될 때 정적 참조를 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
