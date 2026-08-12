using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼 시작 시 외부 시스템에 전달하는 수명주기 이벤트 데이터입니다.
    /// </summary>
    public readonly struct TutorialStartedEventData
    {
        /// <summary>
        /// 시작한 튜토리얼 UID입니다.
        /// </summary>
        public readonly int TutorialUid;

        /// <summary>
        /// 시작한 단계 인덱스입니다. 저장 진행도를 복원한 경우 0보다 클 수 있습니다.
        /// </summary>
        public readonly int StartStepIndex;

        /// <summary>
        /// 명시적인 재시작 요청으로 시작했는지 여부입니다.
        /// </summary>
        public readonly bool IsRestart;

        /// <summary>
        /// 튜토리얼 시작 이벤트 데이터를 생성합니다.
        /// </summary>
        /// <param name="tutorialUid">시작한 튜토리얼 UID입니다.</param>
        /// <param name="startStepIndex">시작한 단계 인덱스입니다.</param>
        /// <param name="isRestart">명시적인 재시작 여부입니다.</param>
        public TutorialStartedEventData(int tutorialUid, int startStepIndex, bool isRestart)
        {
            TutorialUid = tutorialUid;
            StartStepIndex = startStepIndex;
            IsRestart = isRestart;
        }
    }

    /// <summary>
    /// 튜토리얼 완료 시 외부 시스템에 전달하는 수명주기 이벤트 데이터입니다.
    /// </summary>
    public readonly struct TutorialCompletedEventData
    {
        /// <summary>
        /// 완료한 튜토리얼 UID입니다.
        /// </summary>
        public readonly int TutorialUid;

        /// <summary>
        /// 튜토리얼 완료 이벤트 데이터를 생성합니다.
        /// </summary>
        /// <param name="tutorialUid">완료한 튜토리얼 UID입니다.</param>
        public TutorialCompletedEventData(int tutorialUid)
        {
            TutorialUid = tutorialUid;
        }
    }

    /// <summary>
    /// Tutorial 패키지의 성공이 확정된 수명주기 변경을 외부 시스템에 전달합니다.
    /// Analytics 같은 선택 기능은 이 이벤트를 구독하며 Tutorial 런타임은 외부 구현을 직접 참조하지 않습니다.
    /// </summary>
    public static class TutorialLifecycleEvents
    {
        /// <summary>
        /// 튜토리얼 실행기가 유효한 정의로 시작된 후 발생합니다.
        /// </summary>
        public static event Action<TutorialStartedEventData> Started;

        /// <summary>
        /// 튜토리얼의 마지막 단계가 완료되고 저장 상태가 갱신된 후 발생합니다.
        /// </summary>
        public static event Action<TutorialCompletedEventData> Completed;

        /// <summary>
        /// 성공한 튜토리얼 시작을 구독자에게 전달합니다.
        /// </summary>
        /// <param name="eventData">튜토리얼 시작 데이터입니다.</param>
        internal static void NotifyStarted(in TutorialStartedEventData eventData)
        {
            Started?.Invoke(eventData);
        }

        /// <summary>
        /// 성공한 튜토리얼 완료를 구독자에게 전달합니다.
        /// </summary>
        /// <param name="eventData">튜토리얼 완료 데이터입니다.</param>
        internal static void NotifyCompleted(in TutorialCompletedEventData eventData)
        {
            Completed?.Invoke(eventData);
        }

        /// <summary>
        /// Domain Reload 비활성 환경에서도 이전 플레이 세션의 구독자가 남지 않도록 정적 이벤트를 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Started = null;
            Completed = null;
        }
    }

    /// <summary>
    /// Tutorial Catalog, 자동 시작 조건, 활성 단계와 저장 상태를 조정합니다.
    /// </summary>
    public sealed class TutorialManager : IDisposable
    {
        private readonly HashSet<int> _startingTutorialUids = new HashSet<int>();
        private readonly TutorialAddressableRepository _repository;
        private readonly TutorialData _data;
        private readonly TutorialStepRunner _stepRunner;
        private readonly TutorialStartConditionEvaluator _startConditionEvaluator;
        private TutorialCatalog _catalog;
        private int _lifecycleVersion;
        private bool _isStartingTutorial;
        private bool _isDisposed;

        /// <summary>
        /// 현재 입력 차단 정책입니다.
        /// Control 또는 게임 상위 계층 Adapter가 조회할 수 있습니다.
        /// </summary>
        public TutorialInputBlockPolicy InputBlockPolicy { get; }

        /// <summary>
        /// 현재 실행 중인 튜토리얼 UID입니다.
        /// </summary>
        public int ActiveTutorialUid => _stepRunner.TutorialUid;

        /// <summary>
        /// Tutorial Manager를 생성합니다.
        /// </summary>
        public TutorialManager(TutorialData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _repository = new TutorialAddressableRepository();
            InputBlockPolicy = new TutorialInputBlockPolicy();
            _stepRunner = new TutorialStepRunner(InputBlockPolicy);
            _startConditionEvaluator = new TutorialStartConditionEvaluator();
            _stepRunner.StepChanged += HandleStepChanged;
            _stepRunner.TutorialCompleted += HandleTutorialCompleted;
        }

        /// <summary>
        /// 지정한 Catalog 공급자에서 Catalog를 로드하고 게임 이벤트 구독을 시작합니다.
        /// </summary>
        /// <param name="catalogProvider">Catalog를 공급할 Provider입니다.</param>
        /// <returns>초기화에 성공하면 true입니다.</returns>
        public async Task<bool> InitializeAsync(ITutorialCatalogProvider catalogProvider)
        {
            if (_isDisposed || catalogProvider == null)
            {
                return false;
            }

            int lifecycleVersion = ++_lifecycleVersion;
            TutorialCatalog catalog = await catalogProvider.LoadCatalogAsync();
            if (_isDisposed || lifecycleVersion != _lifecycleVersion || catalog?.tutorials == null)
            {
                return false;
            }

            _catalog = catalog;
            _startConditionEvaluator.Initialize(catalog);
            TutorialEventBus.Published -= HandlePublishedEvent;
            TutorialEventBus.Published += HandlePublishedEvent;
            TutorialStartStateRegistry.Changed -= HandleStartStateChanged;
            TutorialStartStateRegistry.Changed += HandleStartStateChanged;
            TryStartSatisfiedTutorial();
            return true;
        }

        /// <summary>
        /// UID로 튜토리얼을 수동 시작합니다.
        /// 이미 다른 튜토리얼이 실행 중이면 시작하지 않습니다.
        /// </summary>
        public Task<bool> StartTutorialAsync(int tutorialUid, bool restart = false)
        {
            TutorialCatalogEntry entry = FindEntry(tutorialUid);
            return StartTutorialAsync(entry, restart);
        }

        /// <summary>
        /// 이벤트 구독, 단계 실행기와 Addressables 정의 캐시를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _lifecycleVersion++;
            TutorialEventBus.Published -= HandlePublishedEvent;
            TutorialStartStateRegistry.Changed -= HandleStartStateChanged;
            _stepRunner.StepChanged -= HandleStepChanged;
            _stepRunner.TutorialCompleted -= HandleTutorialCompleted;
            _stepRunner.Stop();
            _repository.Dispose();
            _startingTutorialUids.Clear();
            _isStartingTutorial = false;
            _catalog = null;
        }

        private void HandlePublishedEvent(TutorialGameEvent tutorialEvent)
        {
            if (_isDisposed)
            {
                return;
            }

            _startConditionEvaluator.HandleEvent(tutorialEvent);
            _stepRunner.HandleEvent(tutorialEvent);
            TryStartSatisfiedTutorial();
        }

        /// <summary>
        /// Window 표시 또는 맵 진행 상태가 변경되면 복합 자동 시작 조건을 다시 평가합니다.
        /// </summary>
        /// <param name="eventData">변경된 상태 종류와 대상 UID입니다.</param>
        private void HandleStartStateChanged(
            TutorialStartStateChangedEventData eventData)
        {
            if (_isDisposed)
            {
                return;
            }

            TryStartSatisfiedTutorial();
        }

        /// <summary>
        /// 현재 Catalog 순서대로 복합 조건을 평가하고 시작 가능한 첫 Tutorial을 요청합니다.
        /// 실행 중인 Tutorial이 있더라도 거짓으로 돌아온 조건은 다시 준비 상태로 갱신합니다.
        /// </summary>
        private void TryStartSatisfiedTutorial()
        {
            if (_isDisposed || _catalog?.tutorials == null)
            {
                return;
            }

            bool canStart = !_stepRunner.IsRunning && !_isStartingTutorial;
            for (int i = 0; i < _catalog.tutorials.Count; i++)
            {
                TutorialCatalogEntry entry = _catalog.tutorials[i];
                if (entry == null ||
                    (!entry.repeatable && _data.IsCompleted(entry.uid)) ||
                    !_startConditionEvaluator.IsReady(entry) ||
                    !canStart)
                {
                    continue;
                }

                _startConditionEvaluator.MarkAttempted(entry.uid);
                _ = StartAutomaticallyAsync(entry);
                break;
            }
        }

        /// <summary>
        /// 복합 조건을 충족한 Tutorial을 비동기로 시작하고 성공 여부에 따라 조건 진행도를 정리합니다.
        /// </summary>
        /// <param name="entry">자동 시작할 Catalog 항목입니다.</param>
        private async Task StartAutomaticallyAsync(TutorialCatalogEntry entry)
        {
            try
            {
                bool started =
                    await StartTutorialAsync(entry, restart: entry.repeatable);
                if (started)
                {
                    _startConditionEvaluator.ResetEventProgress(entry.uid);
                }
                else
                {
                    _startConditionEvaluator.MarkAttemptFailed(entry.uid);
                }
            }
            catch (Exception exception)
            {
                _startConditionEvaluator.MarkAttemptFailed(entry.uid);
                GcLogger.LogException(exception);
            }
        }

        private async Task<bool> StartTutorialAsync(TutorialCatalogEntry entry, bool restart)
        {
            if (_isDisposed ||
                entry == null ||
                _stepRunner.IsRunning ||
                _isStartingTutorial ||
                _startingTutorialUids.Contains(entry.uid) ||
                (!restart && !entry.repeatable && _data.IsCompleted(entry.uid)))
            {
                return false;
            }

            _isStartingTutorial = true;
            _startingTutorialUids.Add(entry.uid);
            int lifecycleVersion = _lifecycleVersion;
            try
            {
                TutorialDefinition definition = await _repository.GetAsync(entry);
                if (_isDisposed ||
                    lifecycleVersion != _lifecycleVersion ||
                    definition == null ||
                    _stepRunner.IsRunning)
                {
                    return false;
                }

                int startStepIndex = 0;
                if (!restart &&
                    _data.Progress.TryGetValue(entry.uid, out TutorialProgressData progress) &&
                    progress != null &&
                    !progress.IsCompleted)
                {
                    startStepIndex = progress.StepIndex;
                }

                bool started = _stepRunner.Start(definition, startStepIndex);
                if (started)
                {
                    var eventData = new TutorialStartedEventData(
                        entry.uid,
                        startStepIndex,
                        restart);
                    TutorialLifecycleEvents.NotifyStarted(eventData);
                }

                return started;
            }
            finally
            {
                _startingTutorialUids.Remove(entry.uid);
                _isStartingTutorial = false;
            }
        }

        private void HandleStepChanged(int tutorialUid, int stepIndex)
        {
            _data.SetProgress(tutorialUid, stepIndex, isCompleted: false);
        }

        private void HandleTutorialCompleted(int tutorialUid)
        {
            _data.SetProgress(tutorialUid, 0, isCompleted: true);
            TutorialLifecycleEvents.NotifyCompleted(
                new TutorialCompletedEventData(tutorialUid));
            TryStartSatisfiedTutorial();
        }

        private TutorialCatalogEntry FindEntry(int tutorialUid)
        {
            if (_catalog?.tutorials == null || tutorialUid <= 0)
            {
                return null;
            }

            for (int i = 0; i < _catalog.tutorials.Count; i++)
            {
                TutorialCatalogEntry entry = _catalog.tutorials[i];
                if (entry != null && entry.uid == tutorialUid)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
