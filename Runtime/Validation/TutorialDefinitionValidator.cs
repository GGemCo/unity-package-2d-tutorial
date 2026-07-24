namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial JSON 정의의 런타임 필수 조건을 검사합니다.
    /// Editor 검증 도구에서도 같은 규칙을 사용합니다.
    /// </summary>
    public static class TutorialDefinitionValidator
    {
        /// <summary>
        /// 개별 Tutorial 정의를 검증합니다.
        /// </summary>
        public static bool ValidateRuntime(
            int expectedUid,
            TutorialDefinition definition,
            out string error)
        {
            if (definition == null)
            {
                error = $"Tutorial JSON 정의가 없습니다. uid: {expectedUid}";
                return false;
            }

            if (definition.uid <= 0 || definition.uid != expectedUid)
            {
                error =
                    $"Tutorial JSON UID가 요청 UID와 다릅니다. expected: {expectedUid}, actual: {definition.uid}";
                return false;
            }

            if (definition.steps == null || definition.steps.Count <= 0)
            {
                error = $"Tutorial JSON에 단계가 없습니다. uid: {expectedUid}";
                return false;
            }

            for (int i = 0; i < definition.steps.Count; i++)
            {
                TutorialStepDefinition step = definition.steps[i];
                if (step == null || step.uid <= 0)
                {
                    error = $"Tutorial 단계가 유효하지 않습니다. uid: {expectedUid}, stepIndex: {i}";
                    return false;
                }

                if (step.conditions == null || step.conditions.Count <= 0)
                {
                    error = $"Tutorial 단계 완료 조건이 없습니다. uid: {expectedUid}, stepIndex: {i}";
                    return false;
                }

                for (int conditionIndex = 0;
                     conditionIndex < step.conditions.Count;
                     conditionIndex++)
                {
                    TutorialConditionDefinition condition = step.conditions[conditionIndex];
                    if (condition == null || condition.type == TutorialEventType.None)
                    {
                        error =
                            $"Tutorial 조건이 유효하지 않습니다. uid: {expectedUid}, stepIndex: {i}, conditionIndex: {conditionIndex}";
                        return false;
                    }
                }

                if (!ValidateActions(
                        expectedUid,
                        i,
                        step.actionsOnEnter,
                        "enter",
                        out error) ||
                    !ValidateActions(
                        expectedUid,
                        i,
                        step.actionsOnExit,
                        "exit",
                        out error))
                {
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// 단계 액션 목록과 ShowGuide Sprite 주소를 검증합니다.
        /// </summary>
        /// <param name="tutorialUid">검증 중인 Tutorial UID입니다.</param>
        /// <param name="stepIndex">검증 중인 단계 인덱스입니다.</param>
        /// <param name="actions">검증할 액션 목록입니다.</param>
        /// <param name="phase">진입 또는 종료 액션 구분 문자열입니다.</param>
        /// <param name="error">검증 실패 메시지입니다.</param>
        /// <returns>모든 액션이 유효하면 true입니다.</returns>
        private static bool ValidateActions(
            int tutorialUid,
            int stepIndex,
            System.Collections.Generic.IReadOnlyList<TutorialActionDefinition> actions,
            string phase,
            out string error)
        {
            if (actions == null)
            {
                error = null;
                return true;
            }

            for (int i = 0; i < actions.Count; i++)
            {
                TutorialActionDefinition action = actions[i];
                if (action == null || action.type == TutorialActionType.None)
                {
                    error =
                        $"Tutorial 액션이 유효하지 않습니다. uid: {tutorialUid}, stepIndex: {stepIndex}, phase: {phase}, actionIndex: {i}";
                    return false;
                }

                if (action.type == TutorialActionType.ShowGuide &&
                    !HasValidGuidePage(action))
                {
                    error =
                        $"ShowGuide 가이드 페이지 주소가 없습니다. uid: {tutorialUid}, stepIndex: {stepIndex}, phase: {phase}, actionIndex: {i}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// ShowGuide 액션에 다중 페이지 또는 기존 단일 페이지 주소가 있는지 확인합니다.
        /// </summary>
        /// <param name="action">검증할 튜토리얼 액션입니다.</param>
        /// <returns>로드 가능한 주소가 하나 이상 있으면 true입니다.</returns>
        private static bool HasValidGuidePage(TutorialActionDefinition action)
        {
            if (!string.IsNullOrWhiteSpace(action.guideSpriteAddress))
            {
                return true;
            }

            if (action.guideSpriteAddresses == null)
            {
                return false;
            }

            for (int i = 0; i < action.guideSpriteAddresses.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(action.guideSpriteAddresses[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
