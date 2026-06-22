using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// 튜토리얼 UID와 가이드 UID에 대응하는 Sprite 참조입니다.
    /// </summary>
    [Serializable]
    public sealed class TutorialGuideCatalogEntry
    {
        public int tutorialUid;
        public int guideUid;
        public Sprite sprite;
    }

    /// <summary>
    /// 생성툴에서 연결한 모든 가이드 Sprite를 Addressables로 제공하는 카탈로그입니다.
    /// </summary>
    public sealed class TutorialGuideCatalog : ScriptableObject
    {
        [SerializeField] private List<TutorialGuideCatalogEntry> entries =
            new List<TutorialGuideCatalogEntry>();

        public List<TutorialGuideCatalogEntry> Entries => entries;

        /// <summary>
        /// 튜토리얼 UID와 가이드 UID로 Sprite를 조회합니다.
        /// </summary>
        public Sprite Find(int tutorialUid, int guideUid)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                TutorialGuideCatalogEntry entry = entries[i];
                if (entry != null &&
                    entry.tutorialUid == tutorialUid &&
                    entry.guideUid == guideUid)
                {
                    return entry.sprite;
                }
            }

            return null;
        }
    }
}
