using System;
using System.Collections.Generic;
using System.IO;
using GGemCo2DCoreEditor;
using GGemCo2DTutorial;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 가이드 페이지의 한글 원문을 Unity Localization String Table Collection에 동기화합니다.
    /// </summary>
    internal static class TutorialGuideLocalizationExportService
    {
        /// <summary>
        /// 제작 에셋의 ShowGuide 페이지를 지정된 가이드 문자열 테이블에 추가하거나 갱신합니다.
        /// 한글 원문만 덮어쓰고 이미 존재하는 다른 Locale 번역은 보존합니다.
        /// </summary>
        /// <param name="asset">Localization으로 내보낼 TutorialAuthoringAsset입니다.</param>
        /// <returns>컬렉션 동기화 결과입니다.</returns>
        public static TutorialExportResult Export(TutorialAuthoringAsset asset)
        {
            if (asset == null)
            {
                return TutorialExportResult.Failure(
                    "Localization에 동기화할 TutorialAuthoringAsset이 없습니다.");
            }

            try
            {
                List<TutorialAuthoringGuidePage> pages =
                    CollectLocalizedGuidePages(asset);
                if (pages.Count == 0)
                {
                    return TutorialExportResult.Success(
                        "동기화할 튜토리얼 가이드 한글 원문이 없습니다.",
                        null,
                        null);
                }

                List<Locale> locales = CollectLocales();
                Locale sourceLocale = FindSourceLocale(locales);
                EnsureOutputDirectory();

                StringTableCollection collection =
                    HelperLocalization.EnsureStringTableCollection(
                        ConfigEditorTutorial.LocalizationGuideCollectionName,
                        ConfigEditorTutorial.LocalizationGuideOutputPath);

                int createdCount = 0;
                int updatedCount = 0;
                for (int i = 0; i < pages.Count; i++)
                {
                    LocalizationEntryChange change = UpsertEntry(
                        collection,
                        locales,
                        sourceLocale,
                        pages[i].DescriptionLocalizationKey,
                        pages[i].DescriptionSourceKo);
                    if (change == LocalizationEntryChange.Created)
                    {
                        createdCount++;
                    }
                    else if (change == LocalizationEntryChange.Updated)
                    {
                        updatedCount++;
                    }
                }

                EditorUtility.SetDirty(collection.SharedData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                string collectionPath = AssetDatabase.GetAssetPath(collection);
                return TutorialExportResult.Success(
                    $"튜토리얼 가이드 Localization을 동기화했습니다. 신규 {createdCount}개, 갱신 {updatedCount}개, 전체 {pages.Count}개",
                    collectionPath,
                    collection);
            }
            catch (Exception exception)
            {
                return TutorialExportResult.Failure(
                    $"튜토리얼 가이드 Localization 동기화에 실패했습니다. {exception.Message}");
            }
        }

        /// <summary>
        /// 한글 원문과 Localization 키가 모두 있는 ShowGuide 페이지를 수집합니다.
        /// </summary>
        /// <param name="asset">페이지를 수집할 제작 에셋입니다.</param>
        /// <returns>Localization Export 대상 페이지 목록입니다.</returns>
        private static List<TutorialAuthoringGuidePage> CollectLocalizedGuidePages(
            TutorialAuthoringAsset asset)
        {
            List<TutorialAuthoringAction> actions =
                new List<TutorialAuthoringAction>();
            List<TutorialAuthoringGuidePage> pages =
                new List<TutorialAuthoringGuidePage>();
            asset.CollectActions(actions);

            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                TutorialAuthoringAction action = actions[actionIndex];
                if (action == null || action.Type != TutorialActionType.ShowGuide)
                {
                    continue;
                }

                IReadOnlyList<TutorialAuthoringGuidePage> guidePages =
                    action.GuidePages;
                for (int pageIndex = 0; pageIndex < guidePages.Count; pageIndex++)
                {
                    TutorialAuthoringGuidePage page = guidePages[pageIndex];
                    if (page == null ||
                        string.IsNullOrWhiteSpace(page.DescriptionSourceKo) ||
                        string.IsNullOrWhiteSpace(page.DescriptionLocalizationKey))
                    {
                        continue;
                    }

                    pages.Add(page);
                }
            }

            return pages;
        }

        /// <summary>
        /// 프로젝트에 등록된 유효한 Locale을 수집합니다.
        /// </summary>
        /// <returns>현재 Localization 설정의 Locale 목록입니다.</returns>
        private static List<Locale> CollectLocales()
        {
            List<Locale> locales = new List<Locale>();
            foreach (Locale locale in LocalizationEditorSettings.GetLocales())
            {
                if (locale != null)
                {
                    locales.Add(locale);
                }
            }

            if (locales.Count == 0)
            {
                throw new InvalidOperationException(
                    "Localization Locale이 설정되어 있지 않습니다.");
            }

            return locales;
        }

        /// <summary>
        /// 설정된 Locale 목록에서 한글 원문 Locale을 찾습니다.
        /// </summary>
        /// <param name="locales">검사할 Locale 목록입니다.</param>
        /// <returns>설정된 원문 Locale 코드와 일치하는 Locale입니다.</returns>
        private static Locale FindSourceLocale(IReadOnlyList<Locale> locales)
        {
            for (int i = 0; i < locales.Count; i++)
            {
                Locale locale = locales[i];
                if (string.Equals(
                        locale.Identifier.Code,
                        ConfigEditorTutorial.LocalizationGuideSourceLocaleCode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return locale;
                }
            }

            throw new InvalidOperationException(
                $"튜토리얼 원문 Locale을 찾을 수 없습니다. locale: {ConfigEditorTutorial.LocalizationGuideSourceLocaleCode}");
        }

        /// <summary>
        /// 컬렉션 자동 생성에 사용할 프로젝트 출력 폴더가 존재하도록 보장합니다.
        /// </summary>
        private static void EnsureOutputDirectory()
        {
            if (Directory.Exists(
                    ConfigEditorTutorial.LocalizationGuideOutputPath))
            {
                return;
            }

            Directory.CreateDirectory(
                ConfigEditorTutorial.LocalizationGuideOutputPath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Shared Key와 Locale별 StringTableEntry를 생성하거나 갱신합니다.
        /// 새 번역 엔트리는 빈 문자열 노출을 피하기 위해 한글 원문으로 초기화합니다.
        /// 기존 비원문 Locale 엔트리는 번역 보호를 위해 변경하지 않습니다.
        /// </summary>
        /// <param name="collection">대상 String Table Collection입니다.</param>
        /// <param name="locales">프로젝트 Locale 목록입니다.</param>
        /// <param name="sourceLocale">한글 원문 Locale입니다.</param>
        /// <param name="key">가이드 페이지 Localization 키입니다.</param>
        /// <param name="sourceText">개발툴에서 입력한 한글 원문입니다.</param>
        /// <returns>엔트리 생성 또는 갱신 상태입니다.</returns>
        private static LocalizationEntryChange UpsertEntry(
            StringTableCollection collection,
            IReadOnlyList<Locale> locales,
            Locale sourceLocale,
            string key,
            string sourceText)
        {
            if (collection == null || string.IsNullOrWhiteSpace(key))
            {
                return LocalizationEntryChange.None;
            }

            SharedTableData sharedData = collection.SharedData;
            if (sharedData == null)
            {
                throw new InvalidOperationException(
                    $"SharedTableData를 찾을 수 없습니다. collection: {collection.TableCollectionName}");
            }

            string normalizedSource = sourceText?.Trim() ?? string.Empty;
            SharedTableData.SharedTableEntry sharedEntry =
                sharedData.GetEntry(key);
            bool created = sharedEntry == null;
            if (created)
            {
                sharedEntry = sharedData.AddKey(key);
                EditorUtility.SetDirty(sharedData);
            }

            bool updated = false;
            for (int i = 0; i < locales.Count; i++)
            {
                Locale locale = locales[i];
                StringTable table =
                    HelperLocalization.EnsureLocaleTable(collection, locale);
                StringTableEntry entry = table.GetEntry(sharedEntry.Id);
                bool isSourceLocale = IsSameLocale(locale, sourceLocale);

                if (entry == null)
                {
                    table.AddEntry(sharedEntry.Id, normalizedSource);
                    entry = table.GetEntry(sharedEntry.Id);
                    if (entry != null)
                    {
                        entry.IsSmart = false;
                    }

                    EditorUtility.SetDirty(table);
                    updated |= !created;
                    continue;
                }

                if (!isSourceLocale ||
                    (string.Equals(
                         entry.Value,
                         normalizedSource,
                         StringComparison.Ordinal) &&
                     !entry.IsSmart))
                {
                    continue;
                }

                table.AddEntry(sharedEntry.Id, normalizedSource);
                entry = table.GetEntry(sharedEntry.Id);
                if (entry != null)
                {
                    entry.IsSmart = false;
                }

                EditorUtility.SetDirty(table);
                updated = true;
            }

            if (created)
            {
                return LocalizationEntryChange.Created;
            }

            return updated
                ? LocalizationEntryChange.Updated
                : LocalizationEntryChange.None;
        }

        /// <summary>
        /// 두 Locale이 Locale 코드 기준으로 같은지 비교합니다.
        /// </summary>
        /// <param name="left">첫 번째 Locale입니다.</param>
        /// <param name="right">두 번째 Locale입니다.</param>
        /// <returns>Locale 코드가 같으면 true입니다.</returns>
        private static bool IsSameLocale(Locale left, Locale right)
        {
            return left != null &&
                   right != null &&
                   string.Equals(
                       left.Identifier.Code,
                       right.Identifier.Code,
                       StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Localization 엔트리 단위 변경 상태입니다.
        /// </summary>
        private enum LocalizationEntryChange
        {
            None = 0,
            Created = 1,
            Updated = 2,
        }
    }
}
