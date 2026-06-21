using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 활성 튜토리얼의 현재 단계 조건 진행과 진입/종료 액션을 관리합니다.
    /// </summary>
    public sealed class TutorialStepRunner
    {
        private readonly TutorialInputBlockPolicy _inputBlockPolicy;
        private readonly List<int> _conditionProgress = new List<int>();
        private TutorialDefinition _definition;
        private int _stepIndex;

        /// <summary>
        /// 단계가 변경될 때 호출됩니다.
        /// </summary>
        public event Action<int, int> StepChanged;

        /// <summary>
        /// 마지막 단계가 완료될 때 호출됩니다.
        /// </summary>
        public event Action<int> TutorialCompleted;

        /// <summary>
        /// 현재 실행 중인 튜토리얼 UID입니다.
        /// </summary>
        public int TutorialUid => _definition?.uid ?? 0;

        /// <summary>
        /// 현재 단계 인덱스입니다.
        /// </summary>
        public int StepIndex => _stepIndex;

        /// <summary>
        /// 활성 단계가 있는지 여부입니다.
        /// </summary>
        public bool IsRunning => _definition != null;

        /// <summary>
        /// 단계 실행기를 생성합니다.
        /// </summary>
        public TutorialStepRunner(TutorialInputBlockPolicy inputBlockPolicy)
        {
            _inputBlockPolicy = inputBlockPolicy;
        }

        /// <summary>
        /// 지정한 튜토리얼을 저장된 단계부터 시작합니다.
        /// </summary>
        public bool Start(TutorialDefinition definition, int startStepIndex)
        {
            if (definition?.steps == null || definition.steps.Count <= 0)
            {
                return false;
            }

            Stop(executeExitActions: false);
            _definition = definition;
            _stepIndex = startStepIndex >= 0 && startStepIndex < definition.steps.Count
                ? startStepIndex
                : 0;
            EnterCurrentStep();
            return true;
        }

        /// <summary>
        /// 현재 단계에 이벤트를 적용하고 모든 조건이 충족되면 다음 단계로 전환합니다.
        /// </summary>
        public void HandleEvent(in TutorialGameEvent tutorialEvent)
        {
            TutorialStepDefinition step = GetCurrentStep();
            if (step?.conditions == null)
            {
                return;
            }

            bool changed = false;
            for (int i = 0; i < step.conditions.Count; i++)
            {
                TutorialConditionDefinition condition = step.conditions[i];
                if (!Matches(condition, tutorialEvent))
                {
                    continue;
                }

                int requiredCount = condition.requiredCount > 0 ? condition.requiredCount : 1;
                int next = _conditionProgress[i] + tutorialEvent.Amount;
                _conditionProgress[i] = next < requiredCount ? next : requiredCount;
                changed = true;
            }

            if (changed && AreAllConditionsCompleted(step))
            {
                CompleteCurrentStep();
            }
        }

        /// <summary>
        /// 현재 단계의 종료 액션과 입력 제한을 정리하고 실행을 중단합니다.
        /// </summary>
        public void Stop(bool executeExitActions = true)
        {
            if (_definition != null && executeExitActions)
            {
                ExecuteActions(GetCurrentStep()?.actionsOnExit);
            }

            _definition = null;
            _stepIndex = 0;
            _conditionProgress.Clear();
            _inputBlockPolicy?.Clear();
        }

        private void EnterCurrentStep()
        {
            TutorialStepDefinition step = GetCurrentStep();
            if (step == null)
            {
                return;
            }

            _conditionProgress.Clear();
            int conditionCount = step.conditions?.Count ?? 0;
            for (int i = 0; i < conditionCount; i++)
            {
                _conditionProgress.Add(0);
            }

            ExecuteActions(step.actionsOnEnter);
            StepChanged?.Invoke(_definition.uid, _stepIndex);
        }

        private void CompleteCurrentStep()
        {
            TutorialStepDefinition step = GetCurrentStep();
            ExecuteActions(step?.actionsOnExit);

            _stepIndex++;
            if (_definition == null || _stepIndex >= _definition.steps.Count)
            {
                int completedTutorialUid = TutorialUid;
                Stop(executeExitActions: false);
                TutorialCompleted?.Invoke(completedTutorialUid);
                return;
            }

            EnterCurrentStep();
        }

        private void ExecuteActions(IReadOnlyList<TutorialActionDefinition> actions)
        {
            if (actions == null || _definition == null)
            {
                return;
            }

            for (int i = 0; i < actions.Count; i++)
            {
                TutorialActionDefinition action = actions[i];
                if (action == null || action.type == TutorialActionType.None)
                {
                    continue;
                }

                ApplyInternalAction(action);
                var context = new TutorialActionContext(_definition.uid, _stepIndex, action);
                bool handled = TutorialActionHandlerRegistry.Execute(context);
                if (!handled && RequiresExternalHandler(action.type))
                {
                    GcLogger.LogWarning(
                        $"Tutorial 액션 처리기가 등록되지 않았습니다. tutorialUid: {_definition.uid}, stepIndex: {_stepIndex}, action: {action.type}");
                }
            }
        }

        private void ApplyInternalAction(TutorialActionDefinition action)
        {
            switch (action.type)
            {
                case TutorialActionType.BlockInputExcept:
                    _inputBlockPolicy?.BlockExcept(action.stringValues);
                    break;
                case TutorialActionType.ClearInputBlock:
                    _inputBlockPolicy?.Clear();
                    break;
            }
        }

        private static bool RequiresExternalHandler(TutorialActionType type)
        {
            return type != TutorialActionType.BlockInputExcept &&
                   type != TutorialActionType.ClearInputBlock;
        }

        private bool AreAllConditionsCompleted(TutorialStepDefinition step)
        {
            for (int i = 0; i < step.conditions.Count; i++)
            {
                int requiredCount = step.conditions[i].requiredCount > 0
                    ? step.conditions[i].requiredCount
                    : 1;
                if (_conditionProgress[i] < requiredCount)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Matches(
            TutorialConditionDefinition condition,
            in TutorialGameEvent tutorialEvent)
        {
            if (condition == null || condition.type != tutorialEvent.Type)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(condition.key) &&
                !string.Equals(condition.key, tutorialEvent.Key, StringComparison.Ordinal))
            {
                return false;
            }

            return condition.intValue <= 0 || condition.intValue == tutorialEvent.IntValue;
        }

        private TutorialStepDefinition GetCurrentStep()
        {
            return _definition?.steps != null &&
                   _stepIndex >= 0 &&
                   _stepIndex < _definition.steps.Count
                ? _definition.steps[_stepIndex]
                : null;
        }
    }
}
