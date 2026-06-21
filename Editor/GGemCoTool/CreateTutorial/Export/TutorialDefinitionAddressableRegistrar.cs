using System;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Export된 개별 Tutorial JSON을 Addressables에 자동 등록합니다.
    /// </summary>
    internal static class TutorialDefinitionAddressableRegistrar
    {
        /// <summary>
        /// 지정한 Tutorial JSON을 UID 기반 주소로 Addressables에 등록합니다.
        /// 기존 엔트리가 있으면 Tutorial 그룹으로 이동하고 주소와 라벨을 갱신합니다.
        /// </summary>
        /// <param name="assetPath">등록할 JSON의 Unity 프로젝트 상대 경로입니다.</param>
        /// <param name="tutorialUid">Addressables 주소를 계산할 Tutorial UID입니다.</param>
        /// <returns>Addressables 등록 결과입니다.</returns>
        public static TutorialExportResult Register(string assetPath, int tutorialUid)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return TutorialExportResult.Failure("Addressables에 등록할 Tutorial JSON 경로가 비어 있습니다.");
            }

            string address = ConfigAddressableKeyTutorial.GetDefinitionAddressableKey(tutorialUid);
            if (string.IsNullOrWhiteSpace(address))
            {
                return TutorialExportResult.Failure(
                    $"Tutorial JSON Addressables 주소를 계산할 수 없습니다. uid: {tutorialUid}");
            }

            try
            {
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    settings = AddressableAssetSettings.Create(
                        "Assets/AddressableAssetsData",
                        "AddressableAssetSettings",
                        true,
                        true);
                    AddressableAssetSettingsDefaultObject.Settings = settings;
                }

                AddressableAssetGroup group = settings.FindGroup(ConfigAddressableGroupNameTutorial.Tutorial);
                if (group == null)
                {
                    if (settings.DefaultGroup == null)
                    {
                        return TutorialExportResult.Failure(
                            "Addressables 기본 그룹이 없어 Tutorial 그룹을 생성할 수 없습니다.");
                    }

                    group = settings.CreateGroup(
                        ConfigAddressableGroupNameTutorial.Tutorial,
                        false,
                        false,
                        true,
                        settings.DefaultGroup.Schemas);
                }

                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrWhiteSpace(guid))
                {
                    return TutorialExportResult.Failure(
                        $"Export된 Tutorial JSON의 GUID를 찾을 수 없습니다. path: {assetPath}");
                }

                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                if (entry == null)
                {
                    return TutorialExportResult.Failure(
                        $"Tutorial JSON Addressables 엔트리를 생성하지 못했습니다. path: {assetPath}");
                }

                entry.address = address;
                entry.SetLabel(ConfigAddressableLabelTutorial.Tutorial, true, true);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();

                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                return TutorialExportResult.Success(
                    $"Tutorial JSON을 저장하고 Addressables에 등록했습니다. address: {address}",
                    assetPath,
                    textAsset);
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure(
                    $"Tutorial JSON은 저장했지만 Addressables 등록에 실패했습니다. {exception.Message}");
            }
        }
    }
}
