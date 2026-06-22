using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 JSON, 카탈로그 테이블, 가이드 이미지를 한곳에서 제작하는 원본 에셋입니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialAuthoringAsset",
        menuName = "GGemCo/Tutorial/Tutorial Authoring Asset",
        order = 2100)]
    public sealed class TutorialAuthoringAsset : ScriptableObject
    {
        [SerializeField] private int uid;
        [SerializeField] private string title;
        [SerializeField] private string category;
        [SerializeField, TextArea(2, 5)] private string memo;
        [SerializeField] private bool enabled = true;
        [SerializeField] private bool repeatable;
        [SerializeField] private int priority;
        [SerializeField] private TutorialPreloadPolicy preloadPolicy;
        [SerializeField, HideInInspector] private string exportFileName;
        [SerializeField] private TutorialAuthoringCondition startCondition =
            TutorialAuthoringCondition.CreateDefault();
        [SerializeField] private List<TutorialAuthoringGuide> guides =
            new List<TutorialAuthoringGuide>();
        [SerializeField] private List<TutorialAuthoringStep> steps =
            new List<TutorialAuthoringStep>();

        public int Uid { get => uid; set => uid = value; }
        public string Title { get => title; set => title = value; }
        public string Category { get => category; set => category = value; }
        public string Memo { get => memo; set => memo = value; }
        public bool Enabled { get => enabled; set => enabled = value; }
        public bool Repeatable { get => repeatable; set => repeatable = value; }
        public int Priority { get => priority; set => priority = value; }
        public TutorialPreloadPolicy PreloadPolicy { get => preloadPolicy; set => preloadPolicy = value; }
        public string ExportFileName
        {
            get => ConfigAddressableKeyTutorial.GetDefinitionFileName(uid);
            set => exportFileName = ConfigAddressableKeyTutorial.GetDefinitionFileName(uid);
        }
        public TutorialAuthoringCondition StartCondition => startCondition;
        public List<TutorialAuthoringGuide> Guides => guides;
        public List<TutorialAuthoringStep> Steps => steps;

        /// <summary>
        /// 현재 제작 데이터를 런타임 튜토리얼 정의로 변환합니다.
        /// </summary>
        public TutorialDefinition ToRuntimeDefinition()
        {
            EnsureDefaults();
            TutorialDefinition definition = new TutorialDefinition
            {
                uid = uid,
                title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            };

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i] != null)
                {
                    definition.steps.Add(steps[i].ToRuntimeDefinition());
                }
            }

            return definition;
        }

        /// <summary>
        /// 다음 사용 가능한 UID로 새 단계를 추가합니다.
        /// </summary>
        public TutorialAuthoringStep AddStep()
        {
            EnsureDefaults();
            TutorialAuthoringStep step = TutorialAuthoringStep.CreateDefault(GetNextStepUid());
            steps.Add(step);
            return step;
        }

        /// <summary>
        /// null 목록과 잘못된 기본값을 안전한 제작 상태로 보정합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            startCondition ??= TutorialAuthoringCondition.CreateDefault();
            startCondition.EnsureDefaults();
            guides ??= new List<TutorialAuthoringGuide>();
            steps ??= new List<TutorialAuthoringStep>();

            for (int i = steps.Count - 1; i >= 0; i--)
            {
                if (steps[i] == null)
                {
                    steps.RemoveAt(i);
                }
                else
                {
                    steps[i].EnsureDefaults(i + 1);
                }
            }

            if (steps.Count == 0)
            {
                steps.Add(TutorialAuthoringStep.CreateDefault(1));
            }

            if (uid > 0)
            {
                exportFileName = ConfigAddressableKeyTutorial.GetDefinitionFileName(uid);
            }
        }

        private void OnValidate()
        {
            EnsureDefaults();
        }

        private int GetNextStepUid()
        {
            int maxUid = 0;
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i] != null)
                {
                    maxUid = Mathf.Max(maxUid, steps[i].Uid);
                }
            }

            return maxUid + 1;
        }
    }
}
