using System.Collections;
using System.Threading.Tasks;
using GGemCo2DTutorial;
using UnityEngine;

namespace GGemCo2DTutorial.Samples
{
    /// <summary>
    /// UnityEvent 또는 게임 로직에서 호출할 수 있는 Tutorial 이벤트 발행 예제입니다.
    /// </summary>
    public sealed class TutorialExampleEventPublisher : MonoBehaviour
    {
        private Coroutine _startTutorialCoroutine;

        /// <summary>
        /// 기본 조작 예제의 자동 시작 이벤트를 발행합니다.
        /// </summary>
        public void PublishExampleStart()
        {
            TutorialEventBus.PublishInputAction("TutorialExampleStart");
        }

        /// <summary>
        /// 이동 입력이 실제로 수행된 시점의 이벤트를 발행합니다.
        /// </summary>
        public void PublishMove()
        {
            PublishAllowedInput("Move");
        }

        /// <summary>
        /// 점프 입력이 실제로 수행된 시점의 이벤트를 발행합니다.
        /// </summary>
        public void PublishJump()
        {
            PublishAllowedInput("Jump");
        }

        /// <summary>
        /// 인벤토리 창이 열린 시점의 이벤트를 발행합니다.
        /// </summary>
        public void PublishInventoryOpened()
        {
            TutorialEventBus.PublishWindowOpened("Inventory");
        }

        /// <summary>
        /// 인벤토리 아이템 UI가 클릭된 시점의 이벤트를 발행합니다.
        /// </summary>
        public void PublishInventoryItemClicked()
        {
            PublishUiClicked("InventoryItem");
        }

        /// <summary>
        /// 장착 버튼이 클릭된 시점의 이벤트를 발행합니다.
        /// </summary>
        public void PublishEquipButtonClicked()
        {
            PublishUiClicked("EquipButton");
        }

        /// <summary>
        /// 사용자 정의 예제의 자동 시작 이벤트를 발행합니다.
        /// </summary>
        public void PublishOnboardingReady()
        {
            PublishCustom("OnboardingReady");
        }

        /// <summary>
        /// 튜토리얼 토큰을 획득한 시점의 사용자 정의 이벤트를 발행합니다.
        /// </summary>
        public void PublishTutorialTokenCollected()
        {
            PublishCustom("CollectTutorialToken");
        }

        /// <summary>
        /// Quest 시작 이벤트를 Tutorial Runtime에 전달합니다.
        /// </summary>
        /// <param name="questUid">시작된 Quest UID입니다.</param>
        public void PublishQuestStarted(int questUid)
        {
            TutorialEventBus.PublishQuestState(questUid, completed: false);
        }

        /// <summary>
        /// Quest 완료 이벤트를 Tutorial Runtime에 전달합니다.
        /// </summary>
        /// <param name="questUid">완료된 Quest UID입니다.</param>
        public void PublishQuestCompleted(int questUid)
        {
            TutorialEventBus.PublishQuestState(questUid, completed: true);
        }

        /// <summary>
        /// 지정한 Tutorial을 저장 진행도와 무관하게 처음부터 수동 시작합니다.
        /// </summary>
        /// <param name="tutorialUid">시작할 Tutorial UID입니다.</param>
        public void RestartTutorial(int tutorialUid)
        {
            if (_startTutorialCoroutine != null)
            {
                StopCoroutine(_startTutorialCoroutine);
            }

            _startTutorialCoroutine = StartCoroutine(StartTutorialRoutine(tutorialUid));
        }

        /// <summary>
        /// 현재 Tutorial 입력 제한 정책에서 지정 입력이 차단되는지 확인합니다.
        /// </summary>
        /// <param name="actionId">확인할 입력 액션 식별자입니다.</param>
        /// <returns>입력이 차단되면 true입니다.</returns>
        public bool IsInputBlocked(string actionId)
        {
            TutorialManager manager = TutorialPackageManager.Instance?.TutorialManager;
            return manager?.InputBlockPolicy != null &&
                   manager.InputBlockPolicy.IsBlocked(actionId);
        }

        /// <summary>
        /// 입력 제한 정책을 먼저 확인한 뒤 허용된 입력 이벤트만 발행합니다.
        /// </summary>
        /// <param name="actionId">발행할 입력 액션 식별자입니다.</param>
        private void PublishAllowedInput(string actionId)
        {
            if (!IsInputBlocked(actionId))
            {
                TutorialEventBus.PublishInputAction(actionId);
            }
        }

        /// <summary>
        /// UI 클릭 이벤트를 발행합니다.
        /// </summary>
        /// <param name="uiKey">클릭된 UI의 안정적인 식별자입니다.</param>
        private static void PublishUiClicked(string uiKey)
        {
            TutorialEventBus.Publish(new TutorialGameEvent(
                TutorialEventType.UiClicked,
                uiKey));
        }

        /// <summary>
        /// 게임 전용 사용자 정의 이벤트를 발행합니다.
        /// </summary>
        /// <param name="eventKey">사용자 정의 이벤트 식별자입니다.</param>
        private static void PublishCustom(string eventKey)
        {
            TutorialEventBus.Publish(new TutorialGameEvent(
                TutorialEventType.Custom,
                eventKey));
        }

        /// <summary>
        /// Tutorial Runtime 준비 여부와 비동기 시작 결과를 안전하게 확인합니다.
        /// </summary>
        /// <param name="tutorialUid">시작할 Tutorial UID입니다.</param>
        private IEnumerator StartTutorialRoutine(int tutorialUid)
        {
            TutorialManager manager = TutorialPackageManager.Instance?.TutorialManager;
            if (manager == null)
            {
                Debug.LogWarning("Tutorial Runtime이 아직 준비되지 않았습니다.", this);
                _startTutorialCoroutine = null;
                yield break;
            }

            Task<bool> startTask = manager.StartTutorialAsync(tutorialUid, restart: true);
            while (!startTask.IsCompleted)
            {
                yield return null;
            }

            if (startTask.IsFaulted)
            {
                Debug.LogException(startTask.Exception, this);
            }
            else if (startTask.IsCanceled || !startTask.Result)
            {
                Debug.LogWarning($"Tutorial을 시작하지 못했습니다. uid: {tutorialUid}", this);
            }

            _startTutorialCoroutine = null;
        }

        /// <summary>
        /// 컴포넌트가 비활성화될 때 진행 중인 수동 시작 대기를 정리합니다.
        /// </summary>
        private void OnDisable()
        {
            if (_startTutorialCoroutine == null)
            {
                return;
            }

            StopCoroutine(_startTutorialCoroutine);
            _startTutorialCoroutine = null;
        }
    }
}
