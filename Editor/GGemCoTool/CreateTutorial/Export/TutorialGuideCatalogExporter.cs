using System;
using System.IO;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// Authoring Asset의 가이드 Sprite를 공용 Addressables 카탈로그에 동기화합니다.
    /// </summary>
    internal static class TutorialGuideCatalogExporter
    {
        /// <summary>
        /// 지정한 튜토리얼의 기존 항목을 교체하고 카탈로그를 Addressables에 등록합니다.
        /// </summary>
        public static TutorialExportResult Export(TutorialAuthoringAsset authoringAsset)
        {
            if (authoringAsset == null || authoringAsset.Uid <= 0)
            {
                return TutorialExportResult.Failure("가이드 카탈로그를 내보낼 Tutorial UID가 없습니다.");
            }

            try
            {
                string assetPath = ConfigAddressablePathTutorial.Tutorial.GuideCatalog;
                EnsureDirectory(assetPath);
                TutorialGuideCatalog catalog =
                    AssetDatabase.LoadAssetAtPath<TutorialGuideCatalog>(assetPath);
                if (catalog == null)
                {
                    catalog = UnityEngine.ScriptableObject.CreateInstance<TutorialGuideCatalog>();
                    AssetDatabase.CreateAsset(catalog, assetPath);
                }

                Undo.RecordObject(catalog, "Export Tutorial Guide Catalog");
                for (int i = catalog.Entries.Count - 1; i >= 0; i--)
                {
                    if (catalog.Entries[i] == null ||
                        catalog.Entries[i].tutorialUid == authoringAsset.Uid)
                    {
                        catalog.Entries.RemoveAt(i);
                    }
                }

                for (int i = 0; i < authoringAsset.Guides.Count; i++)
                {
                    TutorialAuthoringGuide guide = authoringAsset.Guides[i];
                    if (guide == null || guide.Uid <= 0 || guide.Sprite == null)
                    {
                        continue;
                    }

                    catalog.Entries.Add(new TutorialGuideCatalogEntry
                    {
                        tutorialUid = authoringAsset.Uid,
                        guideUid = guide.Uid,
                        sprite = guide.Sprite,
                    });
                }

                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
                RegisterAddressable(assetPath);
                return TutorialExportResult.Success(
                    $"가이드 Sprite 카탈로그를 갱신했습니다. uid: {authoringAsset.Uid}",
                    assetPath,
                    catalog);
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure(
                    $"가이드 Sprite 카탈로그 Export에 실패했습니다. {exception.Message}");
            }
        }

        private static void RegisterAddressable(string assetPath)
        {
            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                throw new InvalidOperationException("Addressables 설정을 찾을 수 없습니다.");
            }

            AddressableAssetGroup group =
                settings.FindGroup(ConfigAddressableGroupNameTutorial.Tutorial) ??
                settings.CreateGroup(
                    ConfigAddressableGroupNameTutorial.Tutorial,
                    false,
                    false,
                    true,
                    settings.DefaultGroup.Schemas);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = ConfigAddressableKeyTutorial.GuideCatalog;
            entry.SetLabel(ConfigAddressableLabelTutorial.Tutorial, true, true);
            settings.SetDirty(
                AddressableAssetSettings.ModificationEvent.EntryMoved,
                entry,
                true);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureDirectory(string assetPath)
        {
            string absolutePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                assetPath);
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.Refresh();
        }
    }
}
