using System;
using System.Collections.Generic;
using System.Globalization;
using GGemCo2DTutorial;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 가이드 페이지의 영구 UID와 Localization 키 생성 규칙을 관리합니다.
    /// </summary>
    internal static class TutorialGuideLocalizationKeyUtility
    {
        private const string KeyPrefix = "tutorial";
        private const string GuideToken = "guide";
        private const string DescriptionToken = "description";

        /// <summary>
        /// 모든 ShowGuide 페이지에 튜토리얼 내부 고유 UID와 안정적인 키를 할당합니다.
        /// 복제로 인해 UID 또는 키가 중복되면 뒤쪽 페이지에 새 UID를 발급합니다.
        /// </summary>
        /// <param name="asset">Localization 식별자를 준비할 제작 에셋입니다.</param>
        /// <returns>제작 데이터가 변경되었으면 true입니다.</returns>
        public static bool PrepareKeys(TutorialAuthoringAsset asset)
        {
            if (asset == null || asset.Uid <= 0)
            {
                return false;
            }

            asset.EnsureDefaults();
            List<TutorialAuthoringGuidePage> pages = CollectGuidePages(asset);
            int nextUid = FindMaxLocalizationUid(pages);
            HashSet<int> usedUids = new HashSet<int>();
            HashSet<string> usedKeys =
                new HashSet<string>(StringComparer.Ordinal);
            bool changed = false;

            for (int i = 0; i < pages.Count; i++)
            {
                TutorialAuthoringGuidePage page = pages[i];
                if (page == null)
                {
                    continue;
                }

                int pageUid = page.LocalizationUid;
                if (pageUid <= 0 || !usedUids.Add(pageUid))
                {
                    pageUid = AllocateNextUid(usedUids, ref nextUid);
                    changed |= page.SetLocalizationUid(pageUid);
                }

                string currentKey = page.DescriptionLocalizationKey;
                bool hasUniqueManualKey =
                    !string.IsNullOrWhiteSpace(currentKey) &&
                    !IsGeneratedKey(currentKey) &&
                    usedKeys.Add(currentKey);
                if (hasUniqueManualKey)
                {
                    continue;
                }

                string generatedKey = BuildKey(asset.Uid, pageUid);
                while (usedKeys.Contains(generatedKey))
                {
                    pageUid = AllocateNextUid(usedUids, ref nextUid);
                    changed |= page.SetLocalizationUid(pageUid);
                    generatedKey = BuildKey(asset.Uid, pageUid);
                }

                usedKeys.Add(generatedKey);
                changed |= page.SetDescriptionLocalizationKey(generatedKey);
            }

            return changed;
        }

        /// <summary>
        /// Tutorial UID와 페이지 Localization UID로 문자열 테이블 키를 생성합니다.
        /// </summary>
        /// <param name="tutorialUid">TutorialAuthoringAsset의 UID입니다.</param>
        /// <param name="localizationUid">튜토리얼 내부에서 영구 유지되는 페이지 UID입니다.</param>
        /// <returns>소문자 snake_case 형식의 Localization 키입니다.</returns>
        public static string BuildKey(int tutorialUid, int localizationUid)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}_{2}_{3:0000}_{4}",
                KeyPrefix,
                Math.Max(0, tutorialUid),
                GuideToken,
                Math.Max(0, localizationUid),
                DescriptionToken);
        }

        /// <summary>
        /// 지정한 키가 이 도구의 자동 생성 규칙을 따르는지 검사합니다.
        /// </summary>
        /// <param name="key">검사할 Localization 키입니다.</param>
        /// <returns>자동 생성 키 형식이면 true입니다.</returns>
        public static bool IsGeneratedKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            string[] tokens = key.Split('_');
            return tokens.Length == 5 &&
                   string.Equals(tokens[0], KeyPrefix, StringComparison.Ordinal) &&
                   int.TryParse(
                       tokens[1],
                       NumberStyles.None,
                       CultureInfo.InvariantCulture,
                       out _) &&
                   string.Equals(tokens[2], GuideToken, StringComparison.Ordinal) &&
                   int.TryParse(
                       tokens[3],
                       NumberStyles.None,
                       CultureInfo.InvariantCulture,
                       out _) &&
                   string.Equals(
                       tokens[4],
                       DescriptionToken,
                       StringComparison.Ordinal);
        }

        /// <summary>
        /// 제작 에셋에서 ShowGuide 액션의 모든 페이지를 실행 순서대로 수집합니다.
        /// </summary>
        /// <param name="asset">페이지를 수집할 제작 에셋입니다.</param>
        /// <returns>가이드 페이지 참조 목록입니다.</returns>
        private static List<TutorialAuthoringGuidePage> CollectGuidePages(
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
                    if (guidePages[pageIndex] != null)
                    {
                        pages.Add(guidePages[pageIndex]);
                    }
                }
            }

            return pages;
        }

        /// <summary>
        /// 현재 페이지 목록에서 가장 큰 Localization UID를 찾습니다.
        /// </summary>
        /// <param name="pages">검사할 페이지 목록입니다.</param>
        /// <returns>발견된 최대 양수 UID이며 없으면 0입니다.</returns>
        private static int FindMaxLocalizationUid(
            IReadOnlyList<TutorialAuthoringGuidePage> pages)
        {
            int maxUid = 0;
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i] != null)
                {
                    maxUid = Math.Max(maxUid, pages[i].LocalizationUid);
                }
            }

            return maxUid;
        }

        /// <summary>
        /// 이미 사용 중인 값을 건너뛰고 다음 양수 UID를 발급합니다.
        /// </summary>
        /// <param name="usedUids">현재 사용 중인 UID 집합입니다.</param>
        /// <param name="nextUid">마지막으로 확인한 UID입니다.</param>
        /// <returns>집합에 등록된 새 UID입니다.</returns>
        private static int AllocateNextUid(
            HashSet<int> usedUids,
            ref int nextUid)
        {
            do
            {
                nextUid++;
            }
            while (nextUid <= 0 || !usedUids.Add(nextUid));

            return nextUid;
        }
    }
}
