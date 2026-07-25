using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GGemCo2DTutorial;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 새 UID·enum 스키마의 Tutorial JSON을 Authoring Asset으로 가져옵니다.
    /// </summary>
    internal static class TutorialJsonImporter
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// UTF-8 JSON 파일을 읽고 런타임 정의를 검증합니다.
        /// </summary>
        public static TutorialImportResult LoadDefinition(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return TutorialImportResult.Failure("가져올 Tutorial JSON 파일을 찾을 수 없습니다.");
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                int offset = HasUtf8Bom(bytes) ? 3 : 0;
                string json = StrictUtf8.GetString(bytes, offset, bytes.Length - offset);
                TutorialDefinition definition =
                    JsonConvert.DeserializeObject<TutorialDefinition>(json);
                int expectedUid = definition?.uid ?? 0;
                if (!TutorialDefinitionValidator.ValidateRuntime(
                        expectedUid,
                        definition,
                        out string error))
                {
                    return TutorialImportResult.Failure(error);
                }

                return TutorialImportResult.Loaded(definition);
            }
            catch (DecoderFallbackException)
            {
                return TutorialImportResult.Failure("Tutorial JSON 파일이 올바른 UTF-8 인코딩이 아닙니다.");
            }
            catch (Exception exception)
            {
                return TutorialImportResult.Failure(
                    $"Tutorial JSON을 읽는 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// 런타임 정의를 기존 Authoring Asset에 반영하거나 새 에셋으로 생성합니다.
        /// </summary>
        public static TutorialImportResult Import(
            TutorialDefinition definition,
            StruckTableTutorial tableRow,
            TutorialAuthoringAsset existingAsset)
        {
            if (definition == null || tableRow == null || definition.uid != tableRow.Uid)
            {
                return TutorialImportResult.Failure("Tutorial JSON과 테이블 행의 UID가 일치하지 않습니다.");
            }

            TutorialAuthoringAsset candidate =
                ScriptableObject.CreateInstance<TutorialAuthoringAsset>();
            try
            {
                Populate(candidate, definition, tableRow, existingAsset);
                if (existingAsset != null)
                {
                    Undo.RecordObject(existingAsset, "Import Tutorial JSON");
                    EditorUtility.CopySerialized(candidate, existingAsset);
                    existingAsset.EnsureDefaults();
                    EditorUtility.SetDirty(existingAsset);
                    AssetDatabase.SaveAssets();
                    return TutorialImportResult.Success(
                        $"Tutorial JSON을 기존 제작 에셋에 반영했습니다. uid: {definition.uid}",
                        definition,
                        existingAsset);
                }

                string assetPath =
                    TutorialAuthoringNamingUtility.BuildUniqueAuthoringAssetPath(definition.uid);
                AssetDatabase.CreateAsset(candidate, assetPath);
                Undo.RegisterCreatedObjectUndo(candidate, "Import Tutorial JSON");
                AssetDatabase.SaveAssets();
                TutorialAuthoringAsset created = candidate;
                candidate = null;
                return TutorialImportResult.Success(
                    $"Tutorial JSON으로 제작 에셋을 생성했습니다. path: {assetPath}",
                    definition,
                    created);
            }
            finally
            {
                if (candidate != null)
                {
                    UnityEngine.Object.DestroyImmediate(candidate);
                }
            }
        }

        private static void Populate(
            TutorialAuthoringAsset target,
            TutorialDefinition definition,
            StruckTableTutorial row,
            TutorialAuthoringAsset existing)
        {
            target.Uid = definition.uid;
            target.Title = string.IsNullOrWhiteSpace(definition.title)
                ? row.Name
                : definition.title;
            target.Category = existing?.Category;
            target.Memo = existing != null ? existing.Memo : row.Memo;
            target.Enabled = row.Enabled;
            target.Repeatable = row.Repeatable;
            target.Priority = row.Priority;
            target.PreloadPolicy = row.PreloadPolicy;
            target.StartMatchMode =
                existing != null
                    ? existing.StartMatchMode
                    : row.StartMatchMode;

            CopyCondition(target.StartCondition, new TutorialConditionDefinition
            {
                type = row.StartEventType,
                targetUid = row.StartTargetUid,
                inputAction = row.StartInputAction,
                intValue = row.StartIntValue,
                floatValue = row.StartFloatValue,
                requiredCount = row.StartRequiredCount,
            });
            target.StartConditions.Clear();
            if (existing?.StartConditions != null)
            {
                for (int i = 0; i < existing.StartConditions.Count; i++)
                {
                    TutorialAuthoringCondition source =
                        existing.StartConditions[i];
                    if (source == null)
                    {
                        continue;
                    }

                    TutorialAuthoringCondition copied =
                        TutorialAuthoringCondition.CreateDefault(source.Type);
                    copied.StartSource = source.StartSource;
                    copied.StartStateType = source.StartStateType;
                    copied.TargetUid = source.TargetUid;
                    copied.InputAction = source.InputAction;
                    copied.IntValue = source.IntValue;
                    copied.FloatValue = source.FloatValue;
                    copied.RequiredCount = source.RequiredCount;
                    copied.Memo = source.Memo;
                    target.StartConditions.Add(copied);
                }
            }

            if (target.StartConditions.Count == 0)
            {
                TableTutorialStartCondition startConditionTable =
                    TableLoaderManagerTutorialEditor
                        .LoadTutorialStartConditionTable();
                IReadOnlyList<StruckTableTutorialStartCondition> rows =
                    startConditionTable?.GetRowsByTutorialUid(row.Uid);
                if (rows != null && rows.Count > 0)
                {
                    target.StartMatchMode = row.StartMatchMode;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        StruckTableTutorialStartCondition source = rows[i];
                        TutorialAuthoringCondition copied =
                            TutorialAuthoringCondition.CreateDefault(
                                source.EventType);
                        copied.StartSource = source.Source;
                        copied.StartStateType = source.StateType;
                        copied.TargetUid = source.TargetUid;
                        copied.InputAction = source.InputAction;
                        copied.IntValue = source.IntValue;
                        copied.FloatValue = source.FloatValue;
                        copied.RequiredCount = source.RequiredCount;
                        copied.Memo = source.Memo;
                        target.StartConditions.Add(copied);
                    }
                }
            }

            target.Steps.Clear();
            for (int i = 0; i < definition.steps.Count; i++)
            {
                TutorialStepDefinition sourceStep = definition.steps[i];
                target.Steps.Add(ConvertStep(
                    sourceStep,
                    FindExistingStep(existing, sourceStep.uid)));
            }

            target.EnsureDefaults();
        }

        private static TutorialAuthoringStep ConvertStep(
            TutorialStepDefinition source,
            TutorialAuthoringStep existing)
        {
            TutorialAuthoringStep target =
                TutorialAuthoringStep.CreateDefault(source.uid);
            target.MessageUid = source.messageUid;
            target.Conditions.Clear();
            CopyConditions(source.conditions, target.Conditions);
            CopyActions(
                source.actionsOnEnter,
                target.ActionsOnEnter,
                existing?.ActionsOnEnter);
            CopyActions(
                source.actionsOnExit,
                target.ActionsOnExit,
                existing?.ActionsOnExit);
            return target;
        }

        private static void CopyConditions(
            IReadOnlyList<TutorialConditionDefinition> source,
            List<TutorialAuthoringCondition> target)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                TutorialAuthoringCondition condition =
                    TutorialAuthoringCondition.CreateDefault();
                CopyCondition(condition, source[i]);
                target.Add(condition);
            }
        }

        private static void CopyCondition(
            TutorialAuthoringCondition target,
            TutorialConditionDefinition source)
        {
            target.Type = source?.type ?? TutorialEventType.None;
            target.TargetUid = source?.targetUid ?? 0;
            target.InputAction = source?.inputAction ?? TutorialInputActionType.None;
            target.IntValue = source?.intValue ?? 0;
            target.FloatValue = source?.floatValue ?? 0f;
            target.RequiredCount = source?.requiredCount ?? 1;
        }

        /// <summary>
        /// 런타임 액션을 제작 액션으로 변환하고 기존 Editor 전용 Sprite와 Localization 원문을 보존합니다.
        /// </summary>
        /// <param name="source">JSON에서 읽은 런타임 액션 목록입니다.</param>
        /// <param name="target">변환 결과를 저장할 제작 액션 목록입니다.</param>
        /// <param name="existing">동일 단계에 있던 기존 제작 액션 목록입니다.</param>
        private static void CopyActions(
            IReadOnlyList<TutorialActionDefinition> source,
            List<TutorialAuthoringAction> target,
            IReadOnlyList<TutorialAuthoringAction> existing)
        {
            target.Clear();
            if (source == null)
            {
                return;
            }

            List<TutorialGuidePageDefinition> runtimePages =
                new List<TutorialGuidePageDefinition>();
            for (int i = 0; i < source.Count; i++)
            {
                TutorialActionDefinition sourceAction = source[i];
                if (sourceAction == null)
                {
                    continue;
                }

                TutorialAuthoringAction action =
                    TutorialAuthoringAction.CreateDefault(sourceAction.type);
                action.TargetUid = sourceAction.targetUid;
                action.IntValue = sourceAction.intValue;
                action.InputMask = sourceAction.inputMask;
                action.GameplayState = sourceAction.gameplayState;
                sourceAction.CollectGuidePages(runtimePages);
                TutorialAuthoringAction existingAction =
                    existing != null && i < existing.Count
                        ? existing[i]
                        : null;

                for (int pageIndex = 0;
                     pageIndex < runtimePages.Count;
                     pageIndex++)
                {
                    TutorialGuidePageDefinition runtimePage =
                        runtimePages[pageIndex];
                    TutorialAuthoringGuidePage existingPage =
                        sourceAction.type == TutorialActionType.ShowGuide &&
                        existingAction != null &&
                        pageIndex < existingAction.GuidePages.Count
                            ? existingAction.GuidePages[pageIndex]
                            : null;
                    Sprite resolvedSprite =
                        TutorialGuideSpriteAddressableSynchronizer.ResolveSprite(
                            runtimePage.spriteAddress);
                    if (resolvedSprite == null &&
                        existingPage != null)
                    {
                        resolvedSprite = existingPage.GuideSprite;
                    }

                    TutorialAuthoringGuidePage authoringPage =
                        TutorialAuthoringGuidePage.Create(
                            resolvedSprite,
                            runtimePage.spriteAddress,
                            runtimePage.descriptionLocalizationKey);
                    authoringPage.RestoreLocalizationAuthoring(existingPage);
                    action.GuidePages.Add(authoringPage);
                }

                target.Add(action);
            }
        }

        /// <summary>
        /// 기존 제작 에셋에서 동일 UID의 단계를 찾아 JSON Import 시 Editor 전용 참조를 보존합니다.
        /// </summary>
        /// <param name="asset">기존 제작 에셋입니다.</param>
        /// <param name="stepUid">검색할 Step UID입니다.</param>
        /// <returns>동일 UID의 기존 단계이며 찾지 못하면 null입니다.</returns>
        private static TutorialAuthoringStep FindExistingStep(
            TutorialAuthoringAsset asset,
            int stepUid)
        {
            if (asset == null)
            {
                return null;
            }

            for (int i = 0; i < asset.Steps.Count; i++)
            {
                TutorialAuthoringStep step = asset.Steps[i];
                if (step != null && step.Uid == stepUid)
                {
                    return step;
                }
            }

            return null;
        }

        private static bool HasUtf8Bom(byte[] bytes)
        {
            return bytes != null &&
                   bytes.Length >= 3 &&
                   bytes[0] == 0xEF &&
                   bytes[1] == 0xBB &&
                   bytes[2] == 0xBF;
        }
    }
}
