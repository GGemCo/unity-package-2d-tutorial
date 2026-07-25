using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// UID와 enum 기반 튜토리얼 조건의 공통 일치 규칙을 제공합니다.
    /// </summary>
    internal static class TutorialConditionMatcher
    {
        /// <summary>
        /// 조건과 게임 이벤트가 같은 대상을 나타내는지 확인합니다.
        /// </summary>
        public static bool Matches(
            TutorialConditionDefinition condition,
            in TutorialGameEvent tutorialEvent)
        {
            if (condition == null || !MatchesEventType(condition.type, tutorialEvent.Type))
            {
                return false;
            }

            if (condition.targetUid > 0 && condition.targetUid != tutorialEvent.TargetUid)
            {
                return false;
            }

            if (condition.inputAction != TutorialInputActionType.None &&
                condition.inputAction != tutorialEvent.InputAction)
            {
                return false;
            }

            if (condition.intValue > 0 && condition.intValue != tutorialEvent.IntValue)
            {
                return false;
            }

            // 자동 이동 거리는 목표 거리 이상에 도달하면 완료되도록 임계값 비교를 적용합니다.
            if (condition.type == TutorialEventType.AutoMoveDistanceReached)
            {
                return tutorialEvent.FloatValue + Mathf.Epsilon >= condition.floatValue;
            }

            return condition.floatValue <= 0f ||
                   Mathf.Approximately(condition.floatValue, tutorialEvent.FloatValue);
        }

        /// <summary>
        /// 자동 시작 이벤트 조건과 발행된 게임 이벤트가 일치하는지 확인합니다.
        /// </summary>
        /// <param name="condition">평가할 자동 시작 이벤트 조건입니다.</param>
        /// <param name="tutorialEvent">발행된 Tutorial 게임 이벤트입니다.</param>
        /// <returns>모든 이벤트 인자가 조건과 일치하면 <see langword="true"/>입니다.</returns>
        public static bool Matches(
            TutorialStartConditionDefinition condition,
            in TutorialGameEvent tutorialEvent)
        {
            if (condition == null ||
                condition.source != TutorialStartConditionSource.Event ||
                !MatchesEventType(condition.eventType, tutorialEvent.Type))
            {
                return false;
            }

            if (condition.targetUid > 0 &&
                condition.targetUid != tutorialEvent.TargetUid)
            {
                return false;
            }

            if (condition.inputAction != TutorialInputActionType.None &&
                condition.inputAction != tutorialEvent.InputAction)
            {
                return false;
            }

            if (condition.intValue > 0 &&
                condition.intValue != tutorialEvent.IntValue)
            {
                return false;
            }

            if (condition.eventType ==
                TutorialEventType.AutoMoveDistanceReached)
            {
                return tutorialEvent.FloatValue + Mathf.Epsilon >=
                       condition.floatValue;
            }

            return condition.floatValue <= 0f ||
                   Mathf.Approximately(
                       condition.floatValue,
                       tutorialEvent.FloatValue);
        }

        /// <summary>
        /// 현재 이벤트 타입과 조건 타입의 일치 여부를 하위 호환 규칙까지 포함하여 확인합니다.
        /// </summary>
        /// <param name="conditionType">튜토리얼 단계에 저장된 조건 타입입니다.</param>
        /// <param name="eventType">런타임에서 발행된 이벤트 타입입니다.</param>
        /// <returns>현재 이벤트로 조건을 진행할 수 있으면 true입니다.</returns>
        private static bool MatchesEventType(
            TutorialEventType conditionType,
            TutorialEventType eventType)
        {
            if (conditionType == eventType)
            {
                return true;
            }

            // 기존 단일 가이드 JSON은 GuideClicked 조건을 사용하므로 닫기 완료 이벤트도 수용합니다.
            return conditionType == TutorialEventType.GuideClicked &&
                   eventType == TutorialEventType.GuideClosed;
        }
    }
}
