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
            }

            error = null;
            return true;
        }
    }
}
