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
            else if (asset.Uid > (int.MaxValue - 999) / 1000)
            {
                result.AddError(
                    "Uid",
                    "Tutorial UID가 자동 시작 조건 행 UID 생성 범위를 초과했습니다.");
            }

            if (asset.StartConditions.Count > 999)
            {
                result.AddError(
                    "StartConditions",
                    "자동 시작 조건은 Tutorial 하나당 최대 999개까지 등록할 수 있습니다.");
            }

            for (int i = 0; i < asset.StartConditions.Count; i++)
            {
                ValidateStartCondition(
                    result,
                    $"StartConditions[{i}]",
                    asset.StartConditions[i]);
            }
            HashSet<int> stepUids = new HashSet<int>();
            HashSet<int> localizationUids = new HashSet<int>();
            HashSet<string> localizationKeys =
                new HashSet<string>(System.StringComparer.Ordinal);
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

                ValidateActions(
                    result,
                    path,
                    step.ActionsOnEnter,
                    localizationUids,
                    localizationKeys);
                ValidateActions(
                    result,
                    path,
                    step.ActionsOnExit,
                    localizationUids,
                    localizationKeys);
            }

            return result;
        }

        /// <summary>
        /// 자동 시작 조건의 Source별 필수 타입과 대상 UID를 검증합니다.
        /// </summary>
        /// <param name="result">검증 문제를 누적할 결과입니다.</param>
        /// <param name="path">제작 에셋 내 조건 경로입니다.</param>
        /// <param name="condition">검증할 자동 시작 조건입니다.</param>
        private static void ValidateStartCondition(
            TutorialAuthoringValidationResult result,
            string path,
            TutorialAuthoringCondition condition)
        {
            if (condition == null)
            {
                result.AddError(path, "자동 시작 조건 데이터가 없습니다.");
                return;
            }

            if (condition.StartSource == TutorialStartConditionSource.Event)
            {
                ValidateCondition(result, path, condition, allowNone: false);
                return;
            }

            if (condition.StartStateType == TutorialStartStateType.None)
            {
                result.AddError(path, "자동 시작 상태 타입을 선택해야 합니다.");
                return;
            }

            if ((condition.StartStateType ==
                    TutorialStartStateType.WindowVisible ||
                 condition.StartStateType ==
                    TutorialStartStateType.MapCleared) &&
                condition.TargetUid <= 0)
            {
                result.AddError(
                    path,
                    "WindowVisible과 MapCleared 상태 조건에는 대상 UID가 필요합니다.");
            }
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

            if (condition.Type == TutorialEventType.ItemPurchased &&
                condition.TargetUid <= 0)
            {
                result.AddError(
                    path,
                    "아이템 구매 조건에는 Item UID가 필요합니다.");
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

        /// <summary>
        /// 액션별 필수 값과 전체 튜토리얼 범위의 가이드 Localization 식별자 중복을 검사합니다.
        /// </summary>
        /// <param name="result">검증 문제를 누적할 결과입니다.</param>
        /// <param name="stepPath">오류 위치에 표시할 현재 단계 경로입니다.</param>
        /// <param name="actions">검사할 진입 또는 종료 액션 목록입니다.</param>
        /// <param name="localizationUids">이미 사용된 페이지 Localization UID 집합입니다.</param>
        /// <param name="localizationKeys">이미 사용된 페이지 Localization 키 집합입니다.</param>
        private static void ValidateActions(
            TutorialAuthoringValidationResult result,
            string stepPath,
            IReadOnlyList<TutorialAuthoringAction> actions,
            HashSet<int> localizationUids,
            HashSet<string> localizationKeys)
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
                    action.GuidePages.Count == 0)
                {
                    result.AddError(path, "ShowGuide에는 가이드 페이지를 하나 이상 연결해야 합니다.");
                    continue;
                }

                if (action.Type != TutorialActionType.ShowGuide)
                {
                    continue;
                }

                for (int pageIndex = 0; pageIndex < action.GuidePages.Count; pageIndex++)
                {
                    TutorialAuthoringGuidePage page = action.GuidePages[pageIndex];
                    string pagePath = $"{path}.GuidePages[{pageIndex}]";
                    if (page == null)
                    {
                        result.AddError(pagePath, "가이드 페이지 데이터가 없습니다.");
                        continue;
                    }

                    if (page.GuideSprite == null)
                    {
                        result.AddError(
                            pagePath,
                            "가이드 페이지에는 Project 창의 Sprite를 연결해야 합니다.");
                    }

                    if (string.IsNullOrWhiteSpace(page.DescriptionSourceKo))
                    {
                        if (string.IsNullOrWhiteSpace(
                                page.DescriptionLocalizationKey) ||
                            TutorialGuideLocalizationKeyUtility.IsGeneratedKey(
                                page.DescriptionLocalizationKey))
                        {
                            result.AddError(
                                pagePath,
                                "가이드 페이지에 한글 설명 원문을 입력해야 합니다.");
                        }
                        else
                        {
                            // 기존 수동 키 데이터는 JSON 호환성을 유지하고 자동 동기화만 건너뜁니다.
                            result.AddWarning(
                                pagePath,
                                "기존 수동 Localization 키에 한글 원문이 없어 문자열 테이블 동기화에서 제외됩니다.");
                        }
                    }

                    if (page.LocalizationUid > 0 &&
                        !localizationUids.Add(page.LocalizationUid))
                    {
                        result.AddError(
                            pagePath,
                            "가이드 페이지 Localization UID가 중복되었습니다.");
                    }

                    if (string.IsNullOrWhiteSpace(page.DescriptionLocalizationKey))
                    {
                        result.AddInfo(
                            pagePath,
                            "Localization 키는 JSON Export 시 자동 생성됩니다.");
                    }
                    else if (!localizationKeys.Add(page.DescriptionLocalizationKey))
                    {
                        result.AddError(
                            pagePath,
                            "가이드 페이지 설명 Localization 키가 중복되었습니다.");
                    }
                }
            }
        }
    }
}
