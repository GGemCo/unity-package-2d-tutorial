using System;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼 조건 판정에 사용되는 값 타입 이벤트입니다.
    /// </summary>
    public readonly struct TutorialGameEvent
    {
        public readonly TutorialEventType Type;
        public readonly string Key;
        public readonly int IntValue;
        public readonly int Amount;

        /// <summary>
        /// 튜토리얼 게임 이벤트를 생성합니다.
        /// </summary>
        /// <param name="type">이벤트 종류입니다.</param>
        /// <param name="key">입력 액션, 창, 사용자 정의 이벤트 식별자입니다.</param>
        /// <param name="intValue">맵, 몬스터, Quest 등 정수 식별자입니다.</param>
        /// <param name="amount">조건 진행에 더할 수량입니다.</param>
        public TutorialGameEvent(
            TutorialEventType type,
            string key = null,
            int intValue = 0,
            int amount = 1)
        {
            Type = type;
            Key = key;
            IntValue = intValue;
            Amount = amount > 0 ? amount : 1;
        }
    }

    /// <summary>
    /// 하위 패키지 참조 없이 외부 시스템의 상태 변화를 Tutorial Runtime에 전달하는 이벤트 버스입니다.
    /// </summary>
    public static class TutorialEventBus
    {
        /// <summary>
        /// 튜토리얼 런타임이 구독하는 게임 이벤트입니다.
        /// </summary>
        public static event Action<TutorialGameEvent> Published;

        /// <summary>
        /// 튜토리얼 게임 이벤트를 발행합니다.
        /// </summary>
        /// <param name="tutorialEvent">발행할 이벤트입니다.</param>
        public static void Publish(in TutorialGameEvent tutorialEvent)
        {
            Published?.Invoke(tutorialEvent);
        }

        /// <summary>
        /// 입력 액션 수행 이벤트를 발행합니다.
        /// </summary>
        /// <param name="actionId">Input Action 또는 프로젝트 입력 식별자입니다.</param>
        public static void PublishInputAction(string actionId)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                return;
            }

            Publish(new TutorialGameEvent(TutorialEventType.InputAction, actionId));
        }

        /// <summary>
        /// UI 창 열림 이벤트를 발행합니다.
        /// </summary>
        /// <param name="windowKey">창 UID 문자열 또는 안정적인 창 식별자입니다.</param>
        public static void PublishWindowOpened(string windowKey)
        {
            if (string.IsNullOrWhiteSpace(windowKey))
            {
                return;
            }

            Publish(new TutorialGameEvent(TutorialEventType.OpenWindow, windowKey));
        }

        /// <summary>
        /// Quest 상태 변경 이벤트를 발행합니다.
        /// Quest 패키지 또는 게임 상위 계층의 Adapter에서 호출합니다.
        /// </summary>
        /// <param name="questUid">상태가 변경된 Quest UID입니다.</param>
        /// <param name="completed">완료 상태이면 true, 시작 상태이면 false입니다.</param>
        public static void PublishQuestState(int questUid, bool completed)
        {
            if (questUid <= 0)
            {
                return;
            }

            Publish(new TutorialGameEvent(
                completed ? TutorialEventType.QuestCompleted : TutorialEventType.QuestStarted,
                intValue: questUid));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Published = null;
        }
    }
}
