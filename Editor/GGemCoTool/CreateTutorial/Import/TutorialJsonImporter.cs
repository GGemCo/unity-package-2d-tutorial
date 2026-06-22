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
    /// Runtime Tutorial JSON을 읽고 TutorialAuthoringAsset으로 변환합니다.
    /// </summary>
    internal static class TutorialJsonImporter
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        /// <summary>
        /// 지정한 JSON 파일을 Runtime Tutorial 정의로 역직렬화하고 기본 구조를 검증합니다.
        /// </summary>
        /// <param name="filePath">읽을 JSON 파일의 절대 경로입니다.</param>
        /// <returns>로드된 정의 또는 오류가 포함된 결과입니다.</returns>
        public static TutorialImportResult LoadDefinition(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return TutorialImportResult.Failure("가져올 Tutorial JSON 파일을 찾을 수 없습니다.");
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string json = StrictUtf8.GetString(RemoveUtf8Bom(bytes));
                TutorialDefinition definition = JsonConvert.DeserializeObject<TutorialDefinition>(json);
                int expectedUid = definition?.uid ?? 0;
                if (!TutorialDefinitionValidator.ValidateRuntime(expectedUid, definition, out string error))
                {
                    return TutorialImportResult.Failure(error);
                }

                return TutorialImportResult.Loaded(definition);
            }
            catch (DecoderFallbackException)
            {
                return TutorialImportResult.Failure("Tutorial JSON 파일이 올바른 UTF-8 인코딩이 아닙니다.");
            }
            catch (JsonException exception)
            {
                return TutorialImportResult.Failure($"Tutorial JSON 형식이 올바르지 않습니다. {exception.Message}");
            }
            catch (Exception exception)
            {
                return TutorialImportResult.Failure($"Tutorial JSON을 읽는 중 오류가 발생했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// Runtime Tutorial 정의를 검증한 뒤 새 Authoring Asset을 만들거나 기존 에셋을 갱신합니다.
        /// </summary>
        /// <param name="definition">가져올 Runtime Tutorial 정의입니다.</param>
        /// <param name="tableRow">자동 시작 조건과 반복 정책을 제공할 Tutorial 테이블 행입니다.</param>
        /// <param name="existingAsset">덮어쓸 기존 에셋입니다. 없으면 새 에셋을 생성합니다.</param>
        /// <returns>생성되거나 갱신된 에셋이 포함된 Import 결과입니다.</returns>
        public static TutorialImportResult Import(
            TutorialDefinition definition,
            StruckTableTutorial tableRow,
            TutorialAuthoringAsset existingAsset)
        {
            if (definition == null || tableRow == null || definition.uid != tableRow.Uid)
            {
                return TutorialImportResult.Failure("Tutorial JSON과 테이블 행의 UID가 일치하지 않습니다.");
            }

            TutorialAuthoringAsset candidate = ScriptableObject.CreateInstance<TutorialAuthoringAsset>();
            try
            {
                PopulateCandidate(candidate, definition, tableRow, existingAsset);
                TutorialAuthoringValidationResult validationResult =
                    TutorialAuthoringValidator.Validate(candidate);
                if (!validationResult.IsValid)
                {
                    return TutorialImportResult.Failure(validationResult.BuildErrorSummary());
                }

                if (existingAsset != null)
                {
                    string existingPath = AssetDatabase.GetAssetPath(existingAsset);
                    Undo.RecordObject(existingAsset, "Import Tutorial JSON");
                    EditorUtility.CopySerialized(candidate, existingAsset);
                    existingAsset.EnsureDefaults();
                    EditorUtility.SetDirty(existingAsset);
                    AssetDatabase.SaveAssets();
                    return TutorialImportResult.Success(
                        $"Tutorial JSON을 기존 제작 데이터에 반영했습니다. uid: {definition.uid}, path: {existingPath}",
                        definition,
                        existingAsset);
                }

                string assetPath =
                    TutorialAuthoringNamingUtility.BuildUniqueAuthoringAssetPath(definition.uid);
                AssetDatabase.CreateAsset(candidate, assetPath);
                Undo.RegisterCreatedObjectUndo(candidate, "Import Tutorial JSON");
                EditorUtility.SetDirty(candidate);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                TutorialAuthoringAsset createdAsset = candidate;
                candidate = null;
                return TutorialImportResult.Success(
                    $"Tutorial JSON으로 제작 데이터를 생성했습니다. uid: {definition.uid}, path: {assetPath}",
                    definition,
                    createdAsset);
            }
            catch (Exception exception)
            {
                return TutorialImportResult.Failure(
                    $"Tutorial JSON Import 중 오류가 발생했습니다. {exception.Message}");
            }
            finally
            {
                if (candidate != null)
                {
                    UnityEngine.Object.DestroyImmediate(candidate);
                }
            }
        }

        /// <summary>
        /// Runtime 정의와 테이블 정책을 임시 Authoring Asset에 깊은 복사합니다.
        /// 기존 에셋이 있으면 JSON에 없는 제작 전용 정보와 메모를 보존합니다.
        /// </summary>
        /// <param name="target">값을 채울 임시 Authoring Asset입니다.</param>
        /// <param name="definition">Runtime Tutorial 정의입니다.</param>
        /// <param name="tableRow">Tutorial 테이블 행입니다.</param>
        /// <param name="existingAsset">제작 전용 정보를 가져올 기존 에셋입니다.</param>
        private static void PopulateCandidate(
            TutorialAuthoringAsset target,
            TutorialDefinition definition,
            StruckTableTutorial tableRow,
            TutorialAuthoringAsset existingAsset)
        {
            target.Uid = definition.uid;
            target.Title = !string.IsNullOrWhiteSpace(definition.title)
                ? definition.title
                : !string.IsNullOrWhiteSpace(existingAsset?.Title)
                    ? existingAsset.Title
                    : tableRow.Name;
            target.Category = existingAsset?.Category;
            target.Memo = existingAsset != null ? existingAsset.Memo : tableRow.Memo;
            target.Repeatable = tableRow.Repeatable;

            CopyCondition(
                target.StartCondition,
                new TutorialConditionDefinition
                {
                    type = tableRow.StartEventType,
                    key = tableRow.StartKey,
                    intValue = tableRow.StartIntValue,
                    requiredCount = tableRow.StartRequiredCount,
                });
            target.StartCondition.Memo = existingAsset?.StartCondition?.Memo;

            Dictionary<int, TutorialAuthoringStep> existingSteps =
                BuildExistingStepLookup(existingAsset);
            target.Steps.Clear();
            for (int i = 0; i < definition.steps.Count; i++)
            {
                TutorialStepDefinition sourceStep = definition.steps[i];
                existingSteps.TryGetValue(sourceStep.uid, out TutorialAuthoringStep existingStep);
                target.Steps.Add(ConvertStep(sourceStep, existingStep));
            }

            target.EnsureDefaults();
        }

        /// <summary>
        /// 기존 제작 데이터의 Step을 UID 기준으로 조회할 수 있는 캐시로 변환합니다.
        /// </summary>
        /// <param name="asset">기존 제작 데이터입니다.</param>
        /// <returns>Step UID별 제작 데이터 사전입니다.</returns>
        private static Dictionary<int, TutorialAuthoringStep> BuildExistingStepLookup(
            TutorialAuthoringAsset asset)
        {
            Dictionary<int, TutorialAuthoringStep> result =
                new Dictionary<int, TutorialAuthoringStep>();
            if (asset?.Steps == null)
            {
                return result;
            }

            for (int i = 0; i < asset.Steps.Count; i++)
            {
                TutorialAuthoringStep step = asset.Steps[i];
                if (step != null && step.Uid > 0 && !result.ContainsKey(step.Uid))
                {
                    result.Add(step.Uid, step);
                }
            }

            return result;
        }

        /// <summary>
        /// Runtime Step을 제작용 Step으로 변환하고 기존 표시 정보와 메모를 보존합니다.
        /// </summary>
        /// <param name="source">변환할 Runtime Step입니다.</param>
        /// <param name="existing">같은 UID의 기존 제작용 Step입니다.</param>
        /// <returns>변환된 제작용 Step입니다.</returns>
        private static TutorialAuthoringStep ConvertStep(
            TutorialStepDefinition source,
            TutorialAuthoringStep existing)
        {
            TutorialAuthoringStep target = TutorialAuthoringStep.CreateDefault(source.uid);
            target.Uid = source.uid;
            target.DisplayName = !string.IsNullOrWhiteSpace(existing?.DisplayName)
                ? existing.DisplayName
                : $"Step {source.uid}";
            target.MessageKey = source.messageKey;
            target.GuideTextPreview = existing?.GuideTextPreview;

            ConvertConditions(source.conditions, existing?.Conditions, target.Conditions);
            ConvertActions(source.actionsOnEnter, existing?.ActionsOnEnter, target.ActionsOnEnter);
            ConvertActions(source.actionsOnExit, existing?.ActionsOnExit, target.ActionsOnExit);
            target.EnsureDefaults(source.uid);
            return target;
        }

        /// <summary>
        /// Runtime 조건 목록을 제작용 조건 목록으로 변환합니다.
        /// 같은 인덱스의 기존 조건 메모는 유지합니다.
        /// </summary>
        /// <param name="source">변환할 Runtime 조건 목록입니다.</param>
        /// <param name="existing">메모를 보존할 기존 제작용 조건 목록입니다.</param>
        /// <param name="target">변환 결과를 저장할 제작용 조건 목록입니다.</param>
        private static void ConvertConditions(
            IReadOnlyList<TutorialConditionDefinition> source,
            IReadOnlyList<TutorialAuthoringCondition> existing,
            List<TutorialAuthoringCondition> target)
        {
            target.Clear();
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                TutorialAuthoringCondition condition =
                    TutorialAuthoringCondition.CreateDefault();
                CopyCondition(condition, source[i]);
                if (existing != null && i < existing.Count && existing[i] != null)
                {
                    condition.Memo = existing[i].Memo;
                }

                target.Add(condition);
            }
        }

        /// <summary>
        /// Runtime 액션 목록을 제작용 액션 목록으로 변환합니다.
        /// 같은 인덱스의 기존 액션 메모는 유지합니다.
        /// </summary>
        /// <param name="source">변환할 Runtime 액션 목록입니다.</param>
        /// <param name="existing">메모를 보존할 기존 제작용 액션 목록입니다.</param>
        /// <param name="target">변환 결과를 저장할 제작용 액션 목록입니다.</param>
        private static void ConvertActions(
            IReadOnlyList<TutorialActionDefinition> source,
            IReadOnlyList<TutorialAuthoringAction> existing,
            List<TutorialAuthoringAction> target)
        {
            target.Clear();
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                TutorialActionDefinition sourceAction = source[i];
                if (sourceAction == null)
                {
                    continue;
                }

                TutorialAuthoringAction action =
                    TutorialAuthoringAction.CreateDefault(sourceAction.type);
                action.Key = sourceAction.key;
                action.IntValue = sourceAction.intValue;
                if (sourceAction.stringValues != null)
                {
                    for (int valueIndex = 0;
                         valueIndex < sourceAction.stringValues.Length;
                         valueIndex++)
                    {
                        action.StringValues.Add(sourceAction.stringValues[valueIndex]);
                    }
                }

                if (existing != null && i < existing.Count && existing[i] != null)
                {
                    action.Memo = existing[i].Memo;
                }

                target.Add(action);
            }
        }

        /// <summary>
        /// Runtime 조건 값을 제작용 조건에 복사합니다.
        /// </summary>
        /// <param name="target">값을 저장할 제작용 조건입니다.</param>
        /// <param name="source">복사할 Runtime 조건입니다.</param>
        private static void CopyCondition(
            TutorialAuthoringCondition target,
            TutorialConditionDefinition source)
        {
            target.Type = source?.type ?? TutorialEventType.None;
            target.Key = source?.key;
            target.IntValue = source?.intValue ?? 0;
            target.RequiredCount = source != null ? source.requiredCount : 1;
        }

        /// <summary>
        /// UTF-8 BOM이 있으면 제외한 새 바이트 배열을 반환합니다.
        /// </summary>
        /// <param name="bytes">JSON 파일 원본 바이트입니다.</param>
        /// <returns>BOM이 제거된 바이트 배열입니다.</returns>
        private static byte[] RemoveUtf8Bom(byte[] bytes)
        {
            if (bytes == null ||
                bytes.Length < 3 ||
                bytes[0] != 0xEF ||
                bytes[1] != 0xBB ||
                bytes[2] != 0xBF)
            {
                return bytes ?? Array.Empty<byte>();
            }

            byte[] result = new byte[bytes.Length - 3];
            Buffer.BlockCopy(bytes, 3, result, 0, result.Length);
            return result;
        }
    }
}
