using System;
using System.Reflection;
using GGemCo2DTutorial;
using UnityEditor;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// CreateTutorialWindow에서 플레이 모드 Tutorial Runtime을 테스트하기 위한 Editor 전용 유틸리티입니다.
    /// </summary>
    internal static class TutorialPlayModeTestUtility
    {
        private const BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>
        /// 현재 플레이 모드와 Tutorial Runtime 상태를 요약합니다.
        /// </summary>
        /// <param name="status">조회된 Runtime 상태입니다.</param>
        /// <returns>상태 조회에 성공하면 true입니다.</returns>
        public static bool TryGetStatus(out TutorialPlayModeRuntimeStatus status)
        {
            status = TutorialPlayModeRuntimeStatus.Empty;
            if (!EditorApplication.isPlaying)
            {
                return false;
            }

            TutorialManager manager = GetTutorialManager();
            if (manager == null)
            {
                return false;
            }

            TutorialStepRunner runner = GetStepRunner(manager);
            bool inputBlockActive = manager.InputBlockPolicy != null && manager.InputBlockPolicy.IsActive;
            status = new TutorialPlayModeRuntimeStatus(
                true,
                runner != null && runner.IsRunning,
                runner?.TutorialUid ?? manager.ActiveTutorialUid,
                runner?.StepIndex ?? -1,
                inputBlockActive);
            return true;
        }

        /// <summary>
        /// 현재 제작 데이터를 Addressables Catalog 없이 즉시 StepRunner에 주입하여 시작합니다.
        /// </summary>
        /// <param name="asset">테스트할 제작 데이터입니다.</param>
        /// <param name="startStepIndex">시작할 Step 인덱스입니다.</param>
        /// <returns>테스트 시작 결과입니다.</returns>
        public static TutorialPlayModeTestResult StartAuthoringAsset(
            TutorialAuthoringAsset asset,
            int startStepIndex)
        {
            if (!EditorApplication.isPlaying)
            {
                return TutorialPlayModeTestResult.Failure("플레이 모드에서만 Authoring Asset 직접 시작 테스트를 실행할 수 있습니다.");
            }

            if (asset == null)
            {
                return TutorialPlayModeTestResult.Failure("테스트할 TutorialAuthoringAsset을 먼저 선택하십시오.");
            }

            TutorialManager manager = GetTutorialManager();
            if (manager == null)
            {
                return TutorialPlayModeTestResult.Failure("TutorialPackageManager 또는 TutorialManager가 아직 준비되지 않았습니다.");
            }

            TutorialStepRunner runner = GetStepRunner(manager);
            if (runner == null)
            {
                return TutorialPlayModeTestResult.Failure("TutorialManager 내부 StepRunner를 찾을 수 없습니다.");
            }

            TutorialDefinition definition = asset.ToRuntimeDefinition();
            if (!TutorialDefinitionValidator.ValidateRuntime(definition.uid, definition, out string error))
            {
                return TutorialPlayModeTestResult.Failure($"테스트 시작 전 Runtime 검증에 실패했습니다. {error}");
            }

            int safeStepIndex = startStepIndex >= 0 && startStepIndex < definition.steps.Count
                ? startStepIndex
                : 0;
            bool started = runner.Start(definition, safeStepIndex);
            return started
                ? TutorialPlayModeTestResult.Success($"Authoring Asset 직접 시작: uid={definition.uid}, stepIndex={safeStepIndex}")
                : TutorialPlayModeTestResult.Failure("StepRunner.Start가 false를 반환했습니다.");
        }

        /// <summary>
        /// Runtime Catalog에 등록된 튜토리얼 UID를 수동 시작합니다.
        /// </summary>
        /// <param name="tutorialUid">시작할 튜토리얼 UID입니다.</param>
        /// <param name="restart">완료 상태와 진행 상태를 무시하고 재시작할지 여부입니다.</param>
        /// <param name="onCompleted">비동기 시작 완료 시 호출할 콜백입니다.</param>
        public static async void StartCatalogTutorialAsync(
            int tutorialUid,
            bool restart,
            Action<TutorialPlayModeTestResult> onCompleted)
        {
            if (!EditorApplication.isPlaying)
            {
                onCompleted?.Invoke(TutorialPlayModeTestResult.Failure("플레이 모드에서만 Catalog 수동 시작을 실행할 수 있습니다."));
                return;
            }

            if (tutorialUid <= 0)
            {
                onCompleted?.Invoke(TutorialPlayModeTestResult.Failure("시작할 Tutorial UID가 유효하지 않습니다."));
                return;
            }

            TutorialManager manager = GetTutorialManager();
            if (manager == null)
            {
                onCompleted?.Invoke(TutorialPlayModeTestResult.Failure("TutorialPackageManager 또는 TutorialManager가 아직 준비되지 않았습니다."));
                return;
            }

            try
            {
                bool started = await manager.StartTutorialAsync(tutorialUid, restart);
                onCompleted?.Invoke(started
                    ? TutorialPlayModeTestResult.Success($"Catalog 튜토리얼 시작 요청 성공: uid={tutorialUid}, restart={restart}")
                    : TutorialPlayModeTestResult.Failure($"Catalog 튜토리얼을 시작하지 못했습니다. uid={tutorialUid}"));
            }
            catch (Exception exception)
            {
                onCompleted?.Invoke(TutorialPlayModeTestResult.Failure($"Catalog 튜토리얼 시작 중 예외가 발생했습니다. {exception.Message}"));
            }
        }

        /// <summary>
        /// 튜토리얼 조건 판정 이벤트를 Runtime EventBus로 발행합니다.
        /// </summary>
        /// <param name="eventType">발행할 이벤트 종류입니다.</param>
        /// <param name="key">문자열 식별자입니다.</param>
        /// <param name="intValue">정수 식별자입니다.</param>
        /// <param name="amount">조건 진행 수량입니다.</param>
        /// <returns>이벤트 발행 결과입니다.</returns>
        public static TutorialPlayModeTestResult PublishEvent(
            TutorialEventType eventType,
            string key,
            int intValue,
            int amount)
        {
            if (!EditorApplication.isPlaying)
            {
                return TutorialPlayModeTestResult.Failure("플레이 모드에서만 Tutorial 이벤트를 발행할 수 있습니다.");
            }

            if (eventType == TutorialEventType.None)
            {
                return TutorialPlayModeTestResult.Failure("발행할 이벤트 타입을 선택하십시오.");
            }

            TutorialEventBus.Publish(new TutorialGameEvent(eventType, key, intValue, amount));
            return TutorialPlayModeTestResult.Success(
                $"Tutorial 이벤트 발행: type={eventType}, key={key}, intValue={intValue}, amount={Math.Max(1, amount)}");
        }

        /// <summary>
        /// 현재 입력 차단 정책에서 특정 입력 액션이 차단되는지 확인합니다.
        /// </summary>
        /// <param name="actionId">검사할 입력 액션 ID입니다.</param>
        /// <returns>입력 차단 검사 결과입니다.</returns>
        public static TutorialPlayModeTestResult CheckInputBlocked(string actionId)
        {
            if (!EditorApplication.isPlaying)
            {
                return TutorialPlayModeTestResult.Failure("플레이 모드에서만 입력 차단 상태를 확인할 수 있습니다.");
            }

            TutorialManager manager = GetTutorialManager();
            if (manager?.InputBlockPolicy == null)
            {
                return TutorialPlayModeTestResult.Failure("TutorialInputBlockPolicy가 준비되지 않았습니다.");
            }

            bool blocked = manager.InputBlockPolicy.IsBlocked(actionId);
            return TutorialPlayModeTestResult.Success(
                $"입력 액션 '{actionId}' 차단 여부: {(blocked ? "차단" : "허용")}, policyActive={manager.InputBlockPolicy.IsActive}");
        }

        /// <summary>
        /// 현재 TutorialManager를 반환합니다.
        /// </summary>
        /// <returns>준비된 TutorialManager입니다. 없으면 null입니다.</returns>
        private static TutorialManager GetTutorialManager()
        {
            return TutorialPackageManager.Instance != null
                ? TutorialPackageManager.Instance.TutorialManager
                : null;
        }

        /// <summary>
        /// TutorialManager 내부 StepRunner를 Editor 테스트 목적으로 조회합니다.
        /// </summary>
        /// <param name="manager">조회할 TutorialManager입니다.</param>
        /// <returns>내부 StepRunner입니다. 찾을 수 없으면 null입니다.</returns>
        private static TutorialStepRunner GetStepRunner(TutorialManager manager)
        {
            if (manager == null)
            {
                return null;
            }

            FieldInfo field = typeof(TutorialManager).GetField("_stepRunner", InstanceBindingFlags);
            return field?.GetValue(manager) as TutorialStepRunner;
        }
    }

    /// <summary>
    /// 플레이 모드 Tutorial Runtime 상태 요약입니다.
    /// </summary>
    internal readonly struct TutorialPlayModeRuntimeStatus
    {
        /// <summary>
        /// 빈 Runtime 상태입니다.
        /// </summary>
        public static readonly TutorialPlayModeRuntimeStatus Empty = new TutorialPlayModeRuntimeStatus(false, false, 0, -1, false);

        /// <summary>
        /// TutorialManager가 준비되었는지 여부입니다.
        /// </summary>
        public bool IsReady { get; }

        /// <summary>
        /// 현재 실행 중인 튜토리얼이 있는지 여부입니다.
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// 현재 실행 중인 튜토리얼 UID입니다.
        /// </summary>
        public int ActiveTutorialUid { get; }

        /// <summary>
        /// 현재 실행 중인 Step 인덱스입니다.
        /// </summary>
        public int StepIndex { get; }

        /// <summary>
        /// 현재 입력 제한 정책이 활성화되어 있는지 여부입니다.
        /// </summary>
        public bool IsInputBlockActive { get; }

        /// <summary>
        /// 플레이 모드 Runtime 상태를 생성합니다.
        /// </summary>
        /// <param name="isReady">TutorialManager 준비 여부입니다.</param>
        /// <param name="isRunning">실행 중인 튜토리얼 존재 여부입니다.</param>
        /// <param name="activeTutorialUid">현재 실행 중인 튜토리얼 UID입니다.</param>
        /// <param name="stepIndex">현재 Step 인덱스입니다.</param>
        /// <param name="isInputBlockActive">입력 제한 활성 여부입니다.</param>
        public TutorialPlayModeRuntimeStatus(
            bool isReady,
            bool isRunning,
            int activeTutorialUid,
            int stepIndex,
            bool isInputBlockActive)
        {
            IsReady = isReady;
            IsRunning = isRunning;
            ActiveTutorialUid = activeTutorialUid;
            StepIndex = stepIndex;
            IsInputBlockActive = isInputBlockActive;
        }
    }
}
