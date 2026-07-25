using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField] private TutorialStartConditionMatchMode startMatchMode =
            TutorialStartConditionMatchMode.All;
        [SerializeField, HideInInspector] private string exportFileName;
        [SerializeField, HideInInspector] private TutorialAuthoringCondition startCondition =
            TutorialAuthoringCondition.CreateDefault();
        [SerializeField] private List<TutorialAuthoringCondition> startConditions =
            new List<TutorialAuthoringCondition>();
        [SerializeField, HideInInspector] private bool startConditionsMigrated;
        [FormerlySerializedAs("guides")]
        [SerializeField, HideInInspector] private List<TutorialAuthoringGuide> legacyGuides =
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
        public TutorialStartConditionMatchMode StartMatchMode
        {
            get => startMatchMode;
            set => startMatchMode = value;
        }
        public string ExportFileName
        {
            get => ConfigAddressableKeyTutorial.GetDefinitionFileName(uid);
            set => exportFileName = ConfigAddressableKeyTutorial.GetDefinitionFileName(uid);
        }
        public TutorialAuthoringCondition StartCondition => startCondition;
        public List<TutorialAuthoringCondition> StartConditions => startConditions;
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
            startConditions ??= new List<TutorialAuthoringCondition>();
            // 기존 단일 제작 에셋은 최초 보정 시 조건 목록 한 건으로 승격하여 데이터 손실 없이 유지합니다.
            if (!startConditionsMigrated)
            {
                if (startConditions.Count == 0 &&
                    startCondition.Type != TutorialEventType.None)
                {
                    startConditions.Add(CloneStartCondition(startCondition));
                }

                startConditionsMigrated = true;
            }

            for (int i = startConditions.Count - 1; i >= 0; i--)
            {
                if (startConditions[i] == null)
                {
                    startConditions.RemoveAt(i);
                    continue;
                }

                startConditions[i].EnsureDefaults();
            }
            legacyGuides ??= new List<TutorialAuthoringGuide>();
            steps ??= new List<TutorialAuthoringStep>();

            bool legacyGuidesMigrated = true;
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                if (steps[i] == null)
                {
                    steps.RemoveAt(i);
                }
                else
                {
                    legacyGuidesMigrated &=
                        MigrateLegacyGuideActions(steps[i], legacyGuides);
                    steps[i].EnsureDefaults(i + 1);
                }
            }

            if (legacyGuidesMigrated && legacyGuides.Count > 0)
            {
                legacyGuides.Clear();
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

        /// <summary>
        /// 기존 단일 자동 시작 조건을 신규 조건 목록에 사용할 독립 객체로 복사합니다.
        /// </summary>
        /// <param name="source">복사할 기존 자동 시작 조건입니다.</param>
        /// <returns>이벤트 기반으로 복사된 신규 제작 조건입니다.</returns>
        private static TutorialAuthoringCondition CloneStartCondition(
            TutorialAuthoringCondition source)
        {
            TutorialAuthoringCondition clone =
                TutorialAuthoringCondition.CreateDefault(source.Type);
            clone.StartSource = TutorialStartConditionSource.Event;
            clone.TargetUid = source.TargetUid;
            clone.InputAction = source.InputAction;
            clone.IntValue = source.IntValue;
            clone.FloatValue = source.FloatValue;
            clone.RequiredCount = source.RequiredCount;
            return clone;
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

        /// <summary>
        /// 모든 단계의 액션을 지정 목록에 추가합니다.
        /// </summary>
        /// <param name="result">액션을 누적할 목록입니다.</param>
        public void CollectActions(List<TutorialAuthoringAction> result)
        {
            if (result == null)
            {
                return;
            }

            for (int i = 0; i < steps.Count; i++)
            {
                steps[i]?.CollectActions(result);
            }
        }

        private static bool MigrateLegacyGuideActions(
            TutorialAuthoringStep step,
            IReadOnlyList<TutorialAuthoringGuide> guides)
        {
            bool migrated = true;
            for (int i = 0; i < step.ActionsOnEnter.Count; i++)
            {
                migrated &= step.ActionsOnEnter[i] == null ||
                            step.ActionsOnEnter[i].TryMigrateLegacyGuide(guides);
            }

            for (int i = 0; i < step.ActionsOnExit.Count; i++)
            {
                migrated &= step.ActionsOnExit[i] == null ||
                            step.ActionsOnExit[i].TryMigrateLegacyGuide(guides);
            }

            return migrated;
        }
    }
}
