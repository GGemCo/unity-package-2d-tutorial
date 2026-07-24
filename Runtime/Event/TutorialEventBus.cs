using System;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼 조건 판정에 사용하는 값 타입 게임 이벤트입니다.
    /// </summary>
    public readonly struct TutorialGameEvent
    {
        public readonly TutorialEventType Type;
        public readonly int TargetUid;
        public readonly TutorialInputActionType InputAction;
        public readonly int IntValue;
        public readonly float FloatValue;
        public readonly int Amount;

        /// <summary>
        /// UID와 enum 기반 튜토리얼 게임 이벤트를 생성합니다.
        /// </summary>
        public TutorialGameEvent(
            TutorialEventType type,
            int targetUid = 0,
            TutorialInputActionType inputAction = TutorialInputActionType.None,
            int intValue = 0,
            float floatValue = 0f,
            int amount = 1)
        {
            Type = type;
            TargetUid = targetUid;
            InputAction = inputAction;
            IntValue = intValue;
            FloatValue = floatValue;
            Amount = amount > 0 ? amount : 1;
        }
    }

    /// <summary>
    /// 상위 패키지의 상태 변화를 튜토리얼 런타임에 전달하는 이벤트 버스입니다.
    /// </summary>
    public static class TutorialEventBus
    {
        public static event Action<TutorialGameEvent> Published;

        /// <summary>
        /// 튜토리얼 게임 이벤트를 발행합니다.
        /// </summary>
        public static void Publish(in TutorialGameEvent tutorialEvent)
        {
            Published?.Invoke(tutorialEvent);
        }

        /// <summary>
        /// 표준 입력 액션 수행 이벤트를 발행합니다.
        /// </summary>
        public static void PublishInputAction(TutorialInputActionType inputAction)
        {
            if (inputAction == TutorialInputActionType.None)
            {
                return;
            }

            Publish(new TutorialGameEvent(
                TutorialEventType.InputAction,
                inputAction: inputAction));
        }

        /// <summary>
        /// UI 창 열림 이벤트를 Window 테이블 UID로 발행합니다.
        /// </summary>
        public static void PublishWindowOpened(int windowUid)
        {
            if (windowUid > 0)
            {
                Publish(new TutorialGameEvent(TutorialEventType.OpenWindow, windowUid));
            }
        }

        /// <summary>
        /// 퀘스트 시작 또는 완료 이벤트를 Quest UID로 발행합니다.
        /// </summary>
        public static void PublishQuestState(int questUid, bool completed)
        {
            if (questUid > 0)
            {
                Publish(new TutorialGameEvent(
                    completed ? TutorialEventType.QuestCompleted : TutorialEventType.QuestStarted,
                    questUid));
            }
        }

        /// <summary>
        /// 플레이어가 비전투 상태에서 전투 상태로 진입했음을 발행합니다.
        /// </summary>
        public static void PublishCombatStarted()
        {
            Publish(new TutorialGameEvent(TutorialEventType.CombatStarted));
        }

        /// <summary>
        /// 현재 가이드의 모든 페이지 확인과 닫기 입력이 완료되었음을 발행합니다.
        /// </summary>
        public static void PublishGuideClosed()
        {
            Publish(new TutorialGameEvent(TutorialEventType.GuideClosed));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Published = null;
        }
    }
}
