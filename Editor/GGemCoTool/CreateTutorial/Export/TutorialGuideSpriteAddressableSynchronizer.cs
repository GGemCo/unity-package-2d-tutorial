using System;
using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 모든 TutorialAuthoringAsset의 ShowGuide Sprite 참조와 Addressables 항목을 동기화합니다.
    /// </summary>
    internal static class TutorialGuideSpriteAddressableSynchronizer
    {
        private const string AutoAddressPrefix = "GGemCo_Tutorial_GuideSprite_";
        private const string LegacyGuideCatalogAddress = "GGemCo_Tutorial_GuideCatalog";
        private static bool _isSynchronizationScheduled;

        /// <summary>
        /// SerializedProperty 변경 적용이 끝난 다음 Editor 지연 호출에서 Addressables 동기화를 실행합니다.
        /// 여러 변경이 같은 프레임에 발생해도 동기화는 한 번만 수행합니다.
        /// </summary>
        public static void ScheduleSynchronize()
        {
            if (_isSynchronizationScheduled)
            {
                return;
            }

            _isSynchronizationScheduled = true;
            EditorApplication.delayCall += ExecuteScheduledSynchronization;
        }

        /// <summary>
        /// 현재 제작 에셋에서 참조하는 가이드 Sprite를 자동 등록하고 미사용 자동 항목을 제거합니다.
        /// </summary>
        /// <returns>동기화 성공 여부와 결과 메시지입니다.</returns>
        public static TutorialExportResult Synchronize()
        {
            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return TutorialExportResult.Failure(
                    "Addressables 설정을 찾을 수 없어 가이드 Sprite를 동기화하지 못했습니다.");
            }

            AddressableAssetGroup group = GetOrCreateTutorialGroup(settings);
            if (group == null)
            {
                return TutorialExportResult.Failure(
                    "Tutorial Addressables 그룹을 생성하거나 찾지 못했습니다.");
            }

            try
            {
                HashSet<string> referencedAutoGuids =
                    new HashSet<string>(StringComparer.Ordinal);
                int registeredCount = SynchronizeAuthoringAssets(
                    settings,
                    group,
                    referencedAutoGuids);
                int removedCount = RemoveUnusedAutoEntries(
                    settings,
                    referencedAutoGuids);
                RemoveLegacyCatalogEntry(settings);

                settings.SetDirty(
                    AddressableAssetSettings.ModificationEvent.EntryMoved,
                    null,
                    true);
                EditorUtility.SetDirty(settings);
                EditorUtility.SetDirty(group);
                AssetDatabase.SaveAssets();

                return TutorialExportResult.Success(
                    $"가이드 Sprite Addressables를 동기화했습니다. 사용: {registeredCount}, 제거: {removedCount}",
                    null,
                    null);
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure(
                    $"가이드 Sprite Addressables 동기화에 실패했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// JSON에 저장된 Addressables 주소를 Editor의 Sprite 참조로 복원합니다.
        /// </summary>
        /// <param name="runtimeAddress">Sprite 하위 에셋 표기를 포함할 수 있는 런타임 주소입니다.</param>
        /// <returns>주소에 대응하는 Sprite이며 찾지 못하면 null입니다.</returns>
        public static Sprite ResolveSprite(string runtimeAddress)
        {
            if (string.IsNullOrWhiteSpace(runtimeAddress))
            {
                return null;
            }

            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return null;
            }

            SplitRuntimeAddress(runtimeAddress, out string baseAddress, out string subObjectName);
            AddressableAssetEntry entry = FindEntryByAddress(settings, baseAddress);
            if (entry == null)
            {
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(subObjectName))
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite &&
                    string.Equals(sprite.name, subObjectName, StringComparison.Ordinal))
                {
                    return sprite;
                }
            }

            return null;
        }

        /// <summary>
        /// 프로젝트의 모든 TutorialAuthoringAsset을 순회하여 ShowGuide 주소를 갱신합니다.
        /// </summary>
        /// <param name="settings">Addressables 설정입니다.</param>
        /// <param name="group">자동 생성 항목을 배치할 Tutorial 그룹입니다.</param>
        /// <param name="referencedAutoGuids">현재 참조 중인 자동 등록 GUID 집합입니다.</param>
        /// <returns>유효한 ShowGuide Sprite 참조 수입니다.</returns>
        private static int SynchronizeAuthoringAssets(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            HashSet<string> referencedAutoGuids)
        {
            int registeredCount = 0;
            string[] authoringGuids =
                AssetDatabase.FindAssets("t:TutorialAuthoringAsset");
            List<TutorialAuthoringAction> actions =
                new List<TutorialAuthoringAction>();

            for (int assetIndex = 0; assetIndex < authoringGuids.Length; assetIndex++)
            {
                string authoringPath =
                    AssetDatabase.GUIDToAssetPath(authoringGuids[assetIndex]);
                TutorialAuthoringAsset authoringAsset =
                    AssetDatabase.LoadAssetAtPath<TutorialAuthoringAsset>(authoringPath);
                if (authoringAsset == null)
                {
                    continue;
                }

                authoringAsset.EnsureDefaults();
                actions.Clear();
                authoringAsset.CollectActions(actions);
                bool changed = false;

                for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
                {
                    TutorialAuthoringAction action = actions[actionIndex];
                    if (action.Type != TutorialActionType.ShowGuide)
                    {
                        continue;
                    }

                    string runtimeAddress = RegisterOrReuseSprite(
                        settings,
                        group,
                        action.GuideSprite,
                        referencedAutoGuids);
                    if (!string.Equals(
                            action.GuideSpriteAddress,
                            runtimeAddress,
                            StringComparison.Ordinal))
                    {
                        action.SetGuideSpriteAddress(runtimeAddress);
                        changed = true;
                    }

                    if (!string.IsNullOrWhiteSpace(runtimeAddress))
                    {
                        registeredCount++;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(authoringAsset);
                }
            }

            return registeredCount;
        }

        /// <summary>
        /// Sprite의 기존 Addressables 항목을 재사용하거나 Tutorial 소유 항목을 생성합니다.
        /// </summary>
        /// <param name="settings">Addressables 설정입니다.</param>
        /// <param name="group">자동 등록 대상 그룹입니다.</param>
        /// <param name="sprite">등록할 Sprite입니다.</param>
        /// <param name="referencedAutoGuids">현재 사용 중인 자동 등록 GUID 집합입니다.</param>
        /// <returns>Sprite를 직접 로드할 런타임 주소이며 Sprite가 없으면 null입니다.</returns>
        private static string RegisterOrReuseSprite(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            Sprite sprite,
            HashSet<string> referencedAutoGuids)
        {
            if (sprite == null)
            {
                return null;
            }

            string assetPath = AssetDatabase.GetAssetPath(sprite);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrWhiteSpace(assetPath) ||
                string.IsNullOrWhiteSpace(guid))
            {
                throw new InvalidOperationException(
                    $"가이드 Sprite의 에셋 경로 또는 GUID를 찾을 수 없습니다. sprite: {sprite.name}");
            }

            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            bool isAutoEntry = entry != null &&
                               IsAutoAddress(entry.address);
            if (entry != null && string.IsNullOrWhiteSpace(entry.address))
            {
                entry.address = BuildAutoAddress(guid);
                isAutoEntry = true;
            }

            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);
                if (entry == null)
                {
                    throw new InvalidOperationException(
                        $"가이드 Sprite를 Addressables에 등록하지 못했습니다. path: {assetPath}");
                }

                entry.address = BuildAutoAddress(guid);
                entry.SetLabel(
                    ConfigAddressableLabelTutorial.GuideSprite,
                    true,
                    true);
                isAutoEntry = true;
            }
            else if (isAutoEntry)
            {
                if (entry.parentGroup != group)
                {
                    entry = settings.CreateOrMoveEntry(guid, group);
                }

                entry.address = BuildAutoAddress(guid);
                entry.SetLabel(
                    ConfigAddressableLabelTutorial.GuideSprite,
                    true,
                    true);
            }

            if (isAutoEntry)
            {
                referencedAutoGuids.Add(guid);
            }

            return BuildRuntimeAddress(entry.address, sprite);
        }

        /// <summary>
        /// 모든 Addressables 그룹에서 더 이상 참조되지 않는 Tutorial 자동 등록 항목을 제거합니다.
        /// </summary>
        /// <param name="settings">Addressables 설정입니다.</param>
        /// <param name="referencedAutoGuids">현재 사용 중인 자동 등록 GUID 집합입니다.</param>
        /// <returns>제거한 항목 수입니다.</returns>
        private static int RemoveUnusedAutoEntries(
            AddressableAssetSettings settings,
            HashSet<string> referencedAutoGuids)
        {
            int removedCount = 0;
            List<AddressableAssetEntry> entries =
                new List<AddressableAssetEntry>();
            for (int groupIndex = 0;
                 groupIndex < settings.groups.Count;
                 groupIndex++)
            {
                AddressableAssetGroup group = settings.groups[groupIndex];
                if (group == null)
                {
                    continue;
                }

                entries.Clear();
                entries.AddRange(group.entries);
                for (int entryIndex = 0;
                     entryIndex < entries.Count;
                     entryIndex++)
                {
                    AddressableAssetEntry entry = entries[entryIndex];
                    if (entry == null ||
                        !IsAutoAddress(entry.address) ||
                        referencedAutoGuids.Contains(entry.guid))
                    {
                        continue;
                    }

                    if (settings.RemoveAssetEntry(entry.guid))
                    {
                        removedCount++;
                    }
                }
            }

            return removedCount;
        }

        /// <summary>
        /// 이전 TutorialGuideCatalog Addressables 항목이 남아 있으면 제거합니다.
        /// </summary>
        /// <param name="settings">Addressables 설정입니다.</param>
        private static void RemoveLegacyCatalogEntry(
            AddressableAssetSettings settings)
        {
            for (int groupIndex = 0;
                 groupIndex < settings.groups.Count;
                 groupIndex++)
            {
                AddressableAssetGroup group = settings.groups[groupIndex];
                if (group == null)
                {
                    continue;
                }

                List<AddressableAssetEntry> entries =
                    new List<AddressableAssetEntry>(group.entries);
                for (int entryIndex = 0;
                     entryIndex < entries.Count;
                     entryIndex++)
                {
                    AddressableAssetEntry entry = entries[entryIndex];
                    if (entry != null &&
                        string.Equals(
                            entry.address,
                            LegacyGuideCatalogAddress,
                            StringComparison.Ordinal))
                    {
                        settings.RemoveAssetEntry(entry.guid);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Tutorial Addressables 그룹을 조회하고 없으면 기본 스키마로 생성합니다.
        /// </summary>
        /// <param name="settings">Addressables 설정입니다.</param>
        /// <returns>Tutorial Addressables 그룹입니다.</returns>
        private static AddressableAssetGroup GetOrCreateTutorialGroup(
            AddressableAssetSettings settings)
        {
            AddressableAssetGroup group =
                settings.FindGroup(ConfigAddressableGroupNameTutorial.Tutorial);
            if (group != null)
            {
                return group;
            }

            return settings.CreateGroup(
                ConfigAddressableGroupNameTutorial.Tutorial,
                false,
                false,
                true,
                settings.DefaultGroup.Schemas);
        }

        /// <summary>
        /// 전체 Addressables 그룹에서 지정 주소와 일치하는 항목을 찾습니다.
        /// </summary>
        /// <param name="settings">Addressables 설정입니다.</param>
        /// <param name="address">검색할 기본 주소입니다.</param>
        /// <returns>일치하는 항목이며 찾지 못하면 null입니다.</returns>
        private static AddressableAssetEntry FindEntryByAddress(
            AddressableAssetSettings settings,
            string address)
        {
            for (int groupIndex = 0; groupIndex < settings.groups.Count; groupIndex++)
            {
                AddressableAssetGroup group = settings.groups[groupIndex];
                if (group == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry != null &&
                        string.Equals(
                            entry.address,
                            address,
                            StringComparison.Ordinal))
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sprite가 하위 에셋이면 Addressables 하위 오브젝트 표기를 포함한 주소를 생성합니다.
        /// </summary>
        /// <param name="baseAddress">Addressables 기본 주소입니다.</param>
        /// <param name="sprite">주소를 생성할 Sprite입니다.</param>
        /// <returns>런타임 Sprite 로드 주소입니다.</returns>
        private static string BuildRuntimeAddress(
            string baseAddress,
            Sprite sprite)
        {
            return AssetDatabase.IsSubAsset(sprite)
                ? $"{baseAddress}[{sprite.name}]"
                : baseAddress;
        }

        /// <summary>
        /// 에셋 GUID를 기반으로 Tutorial 소유의 안정적인 자동 주소를 생성합니다.
        /// </summary>
        /// <param name="guid">Sprite 에셋 GUID입니다.</param>
        /// <returns>자동 Addressables 주소입니다.</returns>
        private static string BuildAutoAddress(string guid)
        {
            return AutoAddressPrefix + guid;
        }

        /// <summary>
        /// 지정 주소가 Tutorial 생성 도구가 소유한 자동 주소인지 확인합니다.
        /// </summary>
        /// <param name="address">검사할 Addressables 주소입니다.</param>
        /// <returns>Tutorial 자동 주소이면 true입니다.</returns>
        private static bool IsAutoAddress(string address)
        {
            return !string.IsNullOrWhiteSpace(address) &&
                   address.StartsWith(
                       AutoAddressPrefix,
                       StringComparison.Ordinal);
        }

        /// <summary>
        /// Addressables 런타임 주소를 기본 주소와 선택적 하위 오브젝트 이름으로 분리합니다.
        /// </summary>
        /// <param name="runtimeAddress">분리할 런타임 주소입니다.</param>
        /// <param name="baseAddress">분리된 기본 주소입니다.</param>
        /// <param name="subObjectName">분리된 하위 오브젝트 이름입니다.</param>
        private static void SplitRuntimeAddress(
            string runtimeAddress,
            out string baseAddress,
            out string subObjectName)
        {
            string trimmed = runtimeAddress.Trim();
            int openingBracket = trimmed.LastIndexOf('[');
            if (openingBracket <= 0 ||
                trimmed[trimmed.Length - 1] != ']')
            {
                baseAddress = trimmed;
                subObjectName = null;
                return;
            }

            baseAddress = trimmed.Substring(0, openingBracket);
            subObjectName = trimmed.Substring(
                openingBracket + 1,
                trimmed.Length - openingBracket - 2);
        }

        /// <summary>
        /// Editor 지연 호출로 예약된 Addressables 동기화를 실행합니다.
        /// </summary>
        private static void ExecuteScheduledSynchronization()
        {
            EditorApplication.delayCall -= ExecuteScheduledSynchronization;
            _isSynchronizationScheduled = false;
            TutorialExportResult result = Synchronize();
            if (!result.Succeeded)
            {
                Debug.LogWarning(result.Message);
            }
        }
    }

    /// <summary>
    /// Project 창에서 에셋이 삭제된 뒤 끊어진 가이드 Sprite 자동 등록 항목을 정리하도록 요청합니다.
    /// </summary>
    internal sealed class TutorialGuideSpriteAssetPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// 에셋 삭제가 감지되면 Authoring 참조 상태가 반영된 다음 Addressables 동기화를 예약합니다.
        /// </summary>
        /// <param name="importedAssets">이번 처리에서 가져온 에셋 경로입니다.</param>
        /// <param name="deletedAssets">이번 처리에서 삭제된 에셋 경로입니다.</param>
        /// <param name="movedAssets">이번 처리에서 이동된 에셋의 새 경로입니다.</param>
        /// <param name="movedFromAssetPaths">이번 처리에서 이동된 에셋의 이전 경로입니다.</param>
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (deletedAssets != null && deletedAssets.Length > 0)
            {
                TutorialGuideSpriteAddressableSynchronizer.ScheduleSynchronize();
            }
        }
    }
}
