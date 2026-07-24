using System.Collections.Generic;
using GGemCo2DTutorial;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 데이터의 UID, 조건, 액션, 가이드 참조를 검증합니다.
    /// </summary>
    internal static class TutorialAuthoringValidator
    {
        /// <summary>
        /// 지정한 제작 에셋을 Export 가능한 상태인지 검증합니다.
        /// </summary>
        public static TutorialAuthoringValidationResult Validate(TutorialAuthoringAsset asset)
        {
            TutorialAuthoringValidationResult result = new TutorialAuthoringValidationResult();
            if (asset == null)
            {
                result.AddError("Asset", "TutorialAuthoringAsset이 없습니다.");
                return result;
            }

            asset.EnsureDefaults();
            if (asset.Uid <= 0)
            {
                result.AddError("Uid", "Tutorial UID는 1 이상이어야 합니다.");
            }

            ValidateCondition(result, "StartCondition", asset.StartCondition, true);
            HashSet<int> stepUids = new HashSet<int>();
            for (int i = 0; i < asset.Steps.Count; i++)
            {
                TutorialAuthoringStep step = asset.Steps[i];
                string path = $"Steps[{i}]";
                if (step == null)
                {
                    result.AddError(path, "Step 데이터가 없습니다.");
                    continue;
                }

                if (step.Uid <= 0 || !stepUids.Add(step.Uid))
                {
                    result.AddError(path, "Step UID는 1 이상이며 중복될 수 없습니다.");
                }

                if (step.Conditions.Count == 0)
                {
                    result.AddError(path, "Step 완료 조건이 필요합니다.");
                }

                for (int conditionIndex = 0;
                     conditionIndex < step.Conditions.Count;
                     conditionIndex++)
                {
                    ValidateCondition(
                        result,
                        $"{path}.Conditions[{conditionIndex}]",
                        step.Conditions[conditionIndex],
                        false);
                }

                ValidateActions(result, path, step.ActionsOnEnter);
                ValidateActions(result, path, step.ActionsOnExit);
            }

            return result;
        }

        private static void ValidateCondition(
            TutorialAuthoringValidationResult result,
            string path,
            TutorialAuthoringCondition condition,
            bool allowNone)
        {
            if (condition == null)
            {
                result.AddError(path, "조건 데이터가 없습니다.");
                return;
            }

            if (condition.Type == TutorialEventType.None)
            {
                if (!allowNone)
                {
                    result.AddError(path, "조건 타입을 선택해야 합니다.");
                }
                return;
            }

            if (condition.Type == TutorialEventType.InputAction &&
                condition.InputAction == TutorialInputActionType.None)
            {
                result.AddError(path, "입력 액션을 선택해야 합니다.");
            }

            if (condition.Type == TutorialEventType.AutoMoveDistanceReached)
            {
                if (condition.TargetUid <= 0)
                {
                    result.AddError(path, "자동 이동 시작 조건에는 Map UID가 필요합니다.");
                }

                if (condition.FloatValue <= 0f)
                {
                    result.AddError(path, "자동 이동 거리는 0보다 커야 합니다.");
                }
            }
        }

        private static void ValidateActions(
            TutorialAuthoringValidationResult result,
            string stepPath,
            IReadOnlyList<TutorialAuthoringAction> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                TutorialAuthoringAction action = actions[i];
                string path = $"{stepPath}.Actions[{i}]";
                if (action == null || action.Type == TutorialActionType.None)
                {
                    result.AddError(path, "액션 타입을 선택해야 합니다.");
                    continue;
                }

                if (action.Type == TutorialActionType.ShowGuide &&
                    action.GuideSprites.Count == 0)
                {
                    result.AddError(path, "ShowGuide에는 가이드 페이지를 하나 이상 연결해야 합니다.");
                    continue;
                }

                if (action.Type != TutorialActionType.ShowGuide)
                {
                    continue;
                }

                for (int pageIndex = 0; pageIndex < action.GuideSprites.Count; pageIndex++)
                {
                    if (action.GuideSprites[pageIndex] == null)
                    {
                        result.AddError(
                            $"{path}.GuidePages[{pageIndex}]",
                            "가이드 페이지에는 Project 창의 Sprite를 연결해야 합니다.");
                    }
                }
            }
        }
    }
}
