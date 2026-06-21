using System.Collections.Generic;
using System.IO;
using GGemCo2DTutorial;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// TutorialAuthoringAsset 제작 데이터에 대해 Runtime 최소 검증보다 강한 Editor 검증을 수행합니다.
    /// </summary>
    internal static class TutorialAuthoringValidator
    {
        /// <summary>
        /// 개별 Tutorial 제작 데이터의 Export 가능 여부와 제작 흐름 위험 요소를 검증합니다.
        /// </summary>
        /// <param name="asset">검증할 제작 데이터입니다.</param>
        /// <param name="requireCatalogFields">Catalog Export에 필요한 필드를 오류로 검사할지 여부입니다.</param>
        /// <returns>검증 결과입니다.</returns>
        public static TutorialAuthoringValidationResult Validate(
            TutorialAuthoringAsset asset,
            bool requireCatalogFields = false)
        {
            TutorialAuthoringValidationResult result = new TutorialAuthoringValidationResult();
            if (asset == null)
            {
                result.AddError("Tutorial", "검증할 TutorialAuthoringAsset이 없습니다.");
                return result;
            }

            asset.EnsureDefaults();
            ValidateRoot(asset, result, requireCatalogFields);
            ValidateStartCondition(asset.StartCondition, result);
            ValidateSteps(asset.Steps, result);
            ValidateRuntimeDefinition(asset, result);
            ValidateFlow(asset.Steps, result);
            return result;
        }

        /// <summary>
        /// 여러 제작 데이터를 Catalog 기준으로 검증합니다.
        /// </summary>
        /// <param name="assets">검증할 제작 데이터 목록입니다.</param>
        /// <returns>Catalog Export용 검증 결과입니다.</returns>
        public static TutorialAuthoringValidationResult ValidateCatalog(IReadOnlyList<TutorialAuthoringAsset> assets)
        {
            TutorialAuthoringValidationResult result = new TutorialAuthoringValidationResult();
            if (assets == null || assets.Count <= 0)
            {
                result.AddError("Catalog", "Catalog에 포함할 TutorialAuthoringAsset이 없습니다.");
                return result;
            }

            HashSet<int> usedUids = new HashSet<int>();
            HashSet<string> usedAddressableKeys = new HashSet<string>();
            int validAssetCount = 0;

            for (int i = 0; i < assets.Count; i++)
            {
                TutorialAuthoringAsset asset = assets[i];
                string path = $"Catalog[{i}]";
                if (asset == null)
                {
                    result.AddWarning(path, "비어 있는 제작 데이터 항목은 Catalog에서 제외됩니다.");
                    continue;
                }

                asset.EnsureDefaults();
                validAssetCount++;
                AppendNestedResult(result, Validate(asset, true), path);

                if (asset.Uid > 0 && !usedUids.Add(asset.Uid))
                {
                    result.AddError(path, $"중복 Tutorial UID입니다. uid: {asset.Uid}");
                }

                string addressableKey = TrimOrNull(asset.AddressableKey);
                if (!string.IsNullOrEmpty(addressableKey) && !usedAddressableKeys.Add(addressableKey))
                {
                    result.AddError(path, $"중복 Addressables Key입니다. key: {addressableKey}");
                }
            }

            if (validAssetCount <= 0)
            {
                result.AddError("Catalog", "Catalog에 저장할 유효한 제작 데이터가 없습니다.");
            }

            return result;
        }

        /// <summary>
        /// 루트 제작 데이터 필드를 검증합니다.
        /// </summary>
        /// <param name="asset">검증할 제작 데이터입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        /// <param name="requireCatalogFields">Catalog 필드를 오류로 검사할지 여부입니다.</param>
        private static void ValidateRoot(
            TutorialAuthoringAsset asset,
            TutorialAuthoringValidationResult result,
            bool requireCatalogFields)
        {
            if (asset.Uid <= 0)
            {
                result.AddError("Tutorial.uid", "Tutorial UID는 1 이상이어야 합니다.");
            }

            if (string.IsNullOrWhiteSpace(asset.Title))
            {
                result.AddWarning("Tutorial.title", "튜토리얼 제목이 비어 있습니다. 제작/QA 구분을 위해 제목 입력을 권장합니다.");
            }

            if (string.IsNullOrWhiteSpace(asset.ExportFileName))
            {
                result.AddWarning("Tutorial.exportFileName", "Export 파일명이 비어 있습니다. UID 기반 기본 파일명이 사용됩니다.");
            }
            else if (ContainsInvalidFileNameCharacters(asset.ExportFileName))
            {
                result.AddError("Tutorial.exportFileName", "Export 파일명에 사용할 수 없는 문자가 포함되어 있습니다.");
            }

            if (string.IsNullOrWhiteSpace(asset.AddressableKey))
            {
                if (requireCatalogFields)
                {
                    result.AddError("Tutorial.addressableKey", "Catalog Export에는 Addressables Key가 필요합니다.");
                }
                else
                {
                    result.AddWarning("Tutorial.addressableKey", "Addressables Key가 비어 있습니다. Catalog Export 전에는 반드시 입력해야 합니다.");
                }
            }
        }

        /// <summary>
        /// Catalog 자동 시작 조건을 검증합니다.
        /// </summary>
        /// <param name="condition">검증할 시작 조건입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        private static void ValidateStartCondition(
            TutorialAuthoringCondition condition,
            TutorialAuthoringValidationResult result)
        {
            if (condition == null || condition.Type == TutorialEventType.None)
            {
                result.AddInfo("StartCondition", "자동 시작 조건이 없습니다. 이 튜토리얼은 수동 시작 전용으로 사용할 수 있습니다.");
                return;
            }

            ValidateCondition(condition, "StartCondition", result, false);
        }

        /// <summary>
        /// Step 목록과 Step 내부 조건/액션을 검증합니다.
        /// </summary>
        /// <param name="steps">검증할 Step 목록입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        private static void ValidateSteps(
            IReadOnlyList<TutorialAuthoringStep> steps,
            TutorialAuthoringValidationResult result)
        {
            if (steps == null || steps.Count <= 0)
            {
                result.AddError("Steps", "튜토리얼 Step이 1개 이상 필요합니다.");
                return;
            }

            HashSet<int> usedStepUids = new HashSet<int>();
            for (int stepIndex = 0; stepIndex < steps.Count; stepIndex++)
            {
                TutorialAuthoringStep step = steps[stepIndex];
                string stepPath = $"Step[{stepIndex}]";
                if (step == null)
                {
                    result.AddError(stepPath, "Step 데이터가 비어 있습니다.");
                    continue;
                }

                if (step.Uid <= 0)
                {
                    result.AddError($"{stepPath}.uid", "Step UID는 1 이상이어야 합니다.");
                }
                else if (!usedStepUids.Add(step.Uid))
                {
                    result.AddError($"{stepPath}.uid", $"중복 Step UID입니다. uid: {step.Uid}");
                }

                if (string.IsNullOrWhiteSpace(step.DisplayName))
                {
                    result.AddWarning($"{stepPath}.displayName", "제작 목록에서 구분할 표시 이름이 비어 있습니다.");
                }

                ValidateStepConditions(step.Conditions, result, stepPath);
                ValidateActions(step.ActionsOnEnter, result, $"{stepPath}.actionsOnEnter", step);
                ValidateActions(step.ActionsOnExit, result, $"{stepPath}.actionsOnExit", step);
            }
        }

        /// <summary>
        /// Step 완료 조건 목록을 검증합니다.
        /// </summary>
        /// <param name="conditions">검증할 조건 목록입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        /// <param name="stepPath">현재 Step 경로입니다.</param>
        private static void ValidateStepConditions(
            IReadOnlyList<TutorialAuthoringCondition> conditions,
            TutorialAuthoringValidationResult result,
            string stepPath)
        {
            if (conditions == null || conditions.Count <= 0)
            {
                result.AddError($"{stepPath}.conditions", "Step 완료 조건이 1개 이상 필요합니다.");
                return;
            }

            for (int conditionIndex = 0; conditionIndex < conditions.Count; conditionIndex++)
            {
                ValidateCondition(
                    conditions[conditionIndex],
                    $"{stepPath}.conditions[{conditionIndex}]",
                    result,
                    true);
            }
        }

        /// <summary>
        /// 단일 조건의 타입별 필수 값을 검증합니다.
        /// </summary>
        /// <param name="condition">검증할 조건입니다.</param>
        /// <param name="path">검증 경로입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        /// <param name="requireNonNone">None 조건을 오류로 처리할지 여부입니다.</param>
        private static void ValidateCondition(
            TutorialAuthoringCondition condition,
            string path,
            TutorialAuthoringValidationResult result,
            bool requireNonNone)
        {
            if (condition == null)
            {
                result.AddError(path, "조건 데이터가 비어 있습니다.");
                return;
            }

            if (condition.Type == TutorialEventType.None)
            {
                if (requireNonNone)
                {
                    result.AddError(path, "조건 타입을 선택해야 합니다.");
                }

                return;
            }

            if (condition.RequiredCount <= 0)
            {
                result.AddError($"{path}.requiredCount", "필요 횟수는 1 이상이어야 합니다.");
            }

            switch (condition.Type)
            {
                case TutorialEventType.EnterMap:
                    WarnWhenWildcardInt(result, path, "Map UID", condition.IntValue);
                    break;
                case TutorialEventType.KillMonster:
                    WarnWhenWildcardInt(result, path, "Monster UID", condition.IntValue);
                    break;
                case TutorialEventType.InputAction:
                    RequireKey(result, path, condition.Key, "Input Action ID가 필요합니다.");
                    break;
                case TutorialEventType.OpenWindow:
                    RequireKeyOrInt(result, path, condition.Key, condition.IntValue, "Window Key 또는 Window UID가 필요합니다.");
                    break;
                case TutorialEventType.UiClicked:
                    RequireKey(result, path, condition.Key, "UI Target Key가 필요합니다.");
                    break;
                case TutorialEventType.QuestStarted:
                case TutorialEventType.QuestCompleted:
                    RequirePositiveInt(result, path, condition.IntValue, "Quest UID가 필요합니다.");
                    break;
                case TutorialEventType.Custom:
                    RequireKey(result, path, condition.Key, "Custom Key가 필요합니다.");
                    break;
            }
        }

        /// <summary>
        /// 액션 목록의 타입별 필수 값을 검증합니다.
        /// </summary>
        /// <param name="actions">검증할 액션 목록입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        /// <param name="path">검증 경로입니다.</param>
        /// <param name="step">현재 Step 데이터입니다.</param>
        private static void ValidateActions(
            IReadOnlyList<TutorialAuthoringAction> actions,
            TutorialAuthoringValidationResult result,
            string path,
            TutorialAuthoringStep step)
        {
            if (actions == null || actions.Count <= 0)
            {
                return;
            }

            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                ValidateAction(actions[actionIndex], result, $"{path}[{actionIndex}]", step);
            }
        }

        /// <summary>
        /// 단일 액션의 타입별 필수 값을 검증합니다.
        /// </summary>
        /// <param name="action">검증할 액션입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        /// <param name="path">검증 경로입니다.</param>
        /// <param name="step">현재 Step 데이터입니다.</param>
        private static void ValidateAction(
            TutorialAuthoringAction action,
            TutorialAuthoringValidationResult result,
            string path,
            TutorialAuthoringStep step)
        {
            if (action == null)
            {
                result.AddError(path, "액션 데이터가 비어 있습니다.");
                return;
            }

            switch (action.Type)
            {
                case TutorialActionType.None:
                    result.AddWarning(path, "액션 타입이 None입니다. Export 시 제외됩니다.");
                    break;
                case TutorialActionType.ShowGuide:
                    if (string.IsNullOrWhiteSpace(action.Key) && (step == null || string.IsNullOrWhiteSpace(step.MessageKey)))
                    {
                        result.AddWarning(path, "ShowGuide 액션에 Guide Key가 없고 Step 안내 문구 Key도 비어 있습니다.");
                    }
                    break;
                case TutorialActionType.HighlightUi:
                    RequireKey(result, path, action.Key, "UI Target Key가 필요합니다.");
                    break;
                case TutorialActionType.BlockInputExcept:
                    if (!HasAnyStringValue(action.StringValues))
                    {
                        result.AddError(path, "허용 입력 Action ID가 1개 이상 필요합니다.");
                    }
                    break;
                case TutorialActionType.UnlockFeature:
                    RequireKeyOrInt(result, path, action.Key, action.IntValue, "Feature Key 또는 Feature UID가 필요합니다.");
                    break;
                case TutorialActionType.StartQuest:
                    RequirePositiveInt(result, path, action.IntValue, "Quest UID가 필요합니다.");
                    break;
                case TutorialActionType.Custom:
                    RequireKey(result, path, action.Key, "Custom Key가 필요합니다.");
                    break;
            }
        }

        /// <summary>
        /// Runtime Validator로 변환 결과가 실제 런타임 최소 조건을 만족하는지 검사합니다.
        /// </summary>
        /// <param name="asset">검증할 제작 데이터입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        private static void ValidateRuntimeDefinition(
            TutorialAuthoringAsset asset,
            TutorialAuthoringValidationResult result)
        {
            TutorialDefinition definition = asset.ToRuntimeDefinition();
            if (!TutorialDefinitionValidator.ValidateRuntime(asset.Uid, definition, out string error))
            {
                result.AddError("RuntimeDefinition", error);
            }
        }

        /// <summary>
        /// 모바일 튜토리얼에서 자주 문제가 되는 가이드/하이라이트/입력 제한 해제 누락을 검사합니다.
        /// </summary>
        /// <param name="steps">검증할 Step 목록입니다.</param>
        /// <param name="result">검증 결과입니다.</param>
        private static void ValidateFlow(
            IReadOnlyList<TutorialAuthoringStep> steps,
            TutorialAuthoringValidationResult result)
        {
            if (steps == null || steps.Count <= 0)
            {
                return;
            }

            bool hasShowGuide = false;
            bool hasHideGuide = false;
            bool hasHighlight = false;
            bool hasClearHighlight = false;
            bool hasInputBlock = false;
            bool hasClearInputBlock = false;

            for (int i = 0; i < steps.Count; i++)
            {
                TutorialAuthoringStep step = steps[i];
                if (step == null)
                {
                    continue;
                }

                AccumulateActionFlags(step.ActionsOnEnter, ref hasShowGuide, ref hasHideGuide, ref hasHighlight, ref hasClearHighlight, ref hasInputBlock, ref hasClearInputBlock);
                AccumulateActionFlags(step.ActionsOnExit, ref hasShowGuide, ref hasHideGuide, ref hasHighlight, ref hasClearHighlight, ref hasInputBlock, ref hasClearInputBlock);
            }

            if (hasShowGuide && !hasHideGuide)
            {
                result.AddWarning("Flow.ShowGuide", "ShowGuide 액션이 있지만 HideGuide 액션이 없습니다. 튜토리얼 종료 후 안내 UI가 남지 않는지 확인하십시오.");
            }

            if (hasHighlight && !hasClearHighlight)
            {
                result.AddWarning("Flow.HighlightUi", "HighlightUi 액션이 있지만 ClearHighlight 액션이 없습니다. 튜토리얼 종료 후 하이라이트가 남지 않는지 확인하십시오.");
            }

            if (hasInputBlock && !hasClearInputBlock)
            {
                result.AddWarning("Flow.BlockInputExcept", "BlockInputExcept 액션이 있지만 ClearInputBlock 액션이 없습니다. 입력 차단이 해제되는지 확인하십시오.");
            }
        }

        /// <summary>
        /// 액션 목록에서 흐름 검증용 플래그를 누적합니다.
        /// </summary>
        private static void AccumulateActionFlags(
            IReadOnlyList<TutorialAuthoringAction> actions,
            ref bool hasShowGuide,
            ref bool hasHideGuide,
            ref bool hasHighlight,
            ref bool hasClearHighlight,
            ref bool hasInputBlock,
            ref bool hasClearInputBlock)
        {
            if (actions == null)
            {
                return;
            }

            for (int i = 0; i < actions.Count; i++)
            {
                TutorialAuthoringAction action = actions[i];
                if (action == null)
                {
                    continue;
                }

                switch (action.Type)
                {
                    case TutorialActionType.ShowGuide:
                        hasShowGuide = true;
                        break;
                    case TutorialActionType.HideGuide:
                        hasHideGuide = true;
                        break;
                    case TutorialActionType.HighlightUi:
                        hasHighlight = true;
                        break;
                    case TutorialActionType.ClearHighlight:
                        hasClearHighlight = true;
                        break;
                    case TutorialActionType.BlockInputExcept:
                        hasInputBlock = true;
                        break;
                    case TutorialActionType.ClearInputBlock:
                        hasClearInputBlock = true;
                        break;
                }
            }
        }

        /// <summary>
        /// 중첩 검증 결과를 상위 검증 결과에 병합합니다.
        /// </summary>
        /// <param name="target">검증 항목을 추가할 결과입니다.</param>
        /// <param name="source">병합할 검증 결과입니다.</param>
        /// <param name="prefix">검증 경로 앞에 붙일 접두사입니다.</param>
        private static void AppendNestedResult(
            TutorialAuthoringValidationResult target,
            TutorialAuthoringValidationResult source,
            string prefix)
        {
            if (target == null || source == null || source.Issues == null)
            {
                return;
            }

            for (int i = 0; i < source.Issues.Count; i++)
            {
                TutorialAuthoringValidationIssue issue = source.Issues[i];
                if (issue == null)
                {
                    continue;
                }

                string path = $"{prefix}.{issue.Path}";
                switch (issue.Severity)
                {
                    case TutorialAuthoringValidationSeverity.Error:
                        target.AddError(path, issue.Message);
                        break;
                    case TutorialAuthoringValidationSeverity.Warning:
                        target.AddWarning(path, issue.Message);
                        break;
                    default:
                        target.AddInfo(path, issue.Message);
                        break;
                }
            }
        }

        /// <summary>
        /// 정수 식별자가 0 이하일 때 모든 대상을 허용하는 와일드카드 조건임을 경고합니다.
        /// </summary>
        private static void WarnWhenWildcardInt(
            TutorialAuthoringValidationResult result,
            string path,
            string label,
            int value)
        {
            if (value <= 0)
            {
                result.AddWarning(path, $"{label}가 0 이하입니다. 모든 대상을 허용하는 조건으로 동작합니다.");
            }
        }

        /// <summary>
        /// 문자열 키가 비어 있으면 오류를 추가합니다.
        /// </summary>
        private static void RequireKey(
            TutorialAuthoringValidationResult result,
            string path,
            string key,
            string message)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                result.AddError(path, message);
            }
        }

        /// <summary>
        /// 문자열 키와 정수 값 중 하나 이상이 유효한지 검사합니다.
        /// </summary>
        private static void RequireKeyOrInt(
            TutorialAuthoringValidationResult result,
            string path,
            string key,
            int intValue,
            string message)
        {
            if (string.IsNullOrWhiteSpace(key) && intValue <= 0)
            {
                result.AddError(path, message);
            }
        }

        /// <summary>
        /// 정수 값이 양수인지 검사합니다.
        /// </summary>
        private static void RequirePositiveInt(
            TutorialAuthoringValidationResult result,
            string path,
            int value,
            string message)
        {
            if (value <= 0)
            {
                result.AddError(path, message);
            }
        }

        /// <summary>
        /// 문자열 목록에 유효한 값이 하나 이상 있는지 검사합니다.
        /// </summary>
        private static bool HasAnyStringValue(IReadOnlyList<string> values)
        {
            if (values == null || values.Count <= 0)
            {
                return false;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 파일명에 운영체제에서 허용하지 않는 문자가 있는지 검사합니다.
        /// </summary>
        private static bool ContainsInvalidFileNameCharacters(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if (fileName.IndexOf('/') >= 0 || fileName.IndexOf('\\') >= 0)
            {
                return true;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++)
            {
                if (fileName.IndexOf(invalidChars[i]) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 앞뒤 공백을 제거하고 빈 문자열이면 null을 반환합니다.
        /// </summary>
        private static string TrimOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
