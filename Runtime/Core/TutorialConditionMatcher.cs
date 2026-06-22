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
            if (condition == null || condition.type != tutorialEvent.Type)
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
    }
}
