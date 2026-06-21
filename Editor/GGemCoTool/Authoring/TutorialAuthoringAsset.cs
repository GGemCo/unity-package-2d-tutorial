using System.Collections.Generic;
using GGemCo2DTutorial;
using UnityEngine;

namespace GGemCo2DTutorialEditor
{
    /// <summary>
    /// 튜토리얼 제작 툴에서 편집할 ScriptableObject 원본 데이터입니다.
    /// Export 단계에서 런타임 Tutorial Catalog/Definition JSON으로 변환합니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialAuthoringAsset",
        menuName = "GGemCo/Tutorial/Tutorial Authoring Asset",
        order = 2100)]
    public sealed class TutorialAuthoringAsset : ScriptableObject
    {
        [SerializeField]
        private int uid;

        [SerializeField]
        private string title;

        [SerializeField]
        private string category;

        [SerializeField]
        [TextArea(2, 5)]
        private string memo;

        [SerializeField]
        private bool repeatable;

        [SerializeField]
        [HideInInspector]
        private string exportFileName;

        [SerializeField]
        private TutorialAuthoringCondition startCondition = TutorialAuthoringCondition.CreateDefault();

        [SerializeField]
        private List<TutorialAuthoringStep> steps = new List<TutorialAuthoringStep>();

        /// <summary>
        /// 튜토리얼 UID입니다.
        /// </summary>
        public int Uid
        {
            get => uid;
            set => uid = value;
        }

        /// <summary>
        /// 튜토리얼 제목입니다.
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }

        /// <summary>
        /// 제작 툴에서 분류와 검색에 사용할 카테고리입니다.
        /// 런타임 JSON으로는 내보내지 않습니다.
        /// </summary>
        public string Category
        {
            get => category;
            set => category = value;
        }

        /// <summary>
        /// 제작자가 남기는 튜토리얼 설명 메모입니다.
        /// 런타임 JSON으로는 내보내지 않습니다.
        /// </summary>
        public string Memo
        {
            get => memo;
            set => memo = value;
        }

        /// <summary>
        /// 완료 후에도 자동 시작 조건이 다시 충족되면 재실행할지 여부입니다.
        /// </summary>
        public bool Repeatable
        {
            get => repeatable;
            set => repeatable = value;
        }

        /// <summary>
        /// UID 규칙으로 계산한 Tutorial JSON Addressables 키입니다.
        /// </summary>
        public string AddressableKey
        {
            get => TutorialAddressableKeyUtility.GetDefinitionAddressableKey(uid);
            set => exportFileName = TutorialAddressableKeyUtility.GetDefinitionFileName(uid);
        }

        /// <summary>
        /// UID 규칙으로 계산한 Tutorial JSON 파일명입니다.
        /// </summary>
        public string ExportFileName
        {
            get => TutorialAddressableKeyUtility.GetDefinitionFileName(uid);
            set => exportFileName = TutorialAddressableKeyUtility.GetDefinitionFileName(uid);
        }

        /// <summary>
        /// Catalog에서 자동 시작 여부를 판단할 시작 조건입니다.
        /// </summary>
        public TutorialAuthoringCondition StartCondition => startCondition;

        /// <summary>
        /// 순차 실행할 튜토리얼 Step 목록입니다.
        /// </summary>
        public List<TutorialAuthoringStep> Steps => steps;

        /// <summary>
        /// 현재 제작 데이터를 런타임 Tutorial Definition DTO로 변환합니다.
        /// </summary>
        /// <returns>런타임 JSON에 저장할 튜토리얼 정의입니다.</returns>
        public TutorialDefinition ToRuntimeDefinition()
        {
            EnsureDefaults();
            TutorialDefinition definition = new TutorialDefinition
            {
                uid = uid,
                title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
                steps = new List<TutorialStepDefinition>(),
            };

            for (int i = 0; i < steps.Count; i++)
            {
                TutorialAuthoringStep step = steps[i];
                if (step == null)
                {
                    continue;
                }

                TutorialStepDefinition runtimeStep = step.ToRuntimeDefinition();
                if (runtimeStep != null)
                {
                    definition.steps.Add(runtimeStep);
                }
            }

            return definition;
        }

        /// <summary>
        /// 현재 제작 데이터를 런타임 Tutorial Catalog Entry DTO로 변환합니다.
        /// </summary>
        /// <returns>Catalog JSON에 저장할 튜토리얼 항목입니다.</returns>
        public TutorialCatalogEntry ToCatalogEntry()
        {
            EnsureDefaults();
            return new TutorialCatalogEntry
            {
                uid = uid,
                addressableKey = TutorialAddressableKeyUtility.GetDefinitionAddressableKey(uid),
                repeatable = repeatable,
                startCondition = startCondition != null && startCondition.Type != TutorialEventType.None
                    ? startCondition.ToRuntimeDefinition()
                    : null,
            };
        }

        /// <summary>
        /// 새 Step을 추가하고 기본 UID를 자동으로 부여합니다.
        /// </summary>
        /// <returns>추가된 제작용 Step 데이터입니다.</returns>
        public TutorialAuthoringStep AddStep()
        {
            EnsureDefaults();
            TutorialAuthoringStep step = TutorialAuthoringStep.CreateDefault(GetNextStepUid());
            steps.Add(step);
            return step;
        }

        /// <summary>
        /// 제작 중 잘못 입력될 수 있는 null 목록과 비어 있는 기본값을 보정합니다.
        /// </summary>
        public void EnsureDefaults()
        {
            if (startCondition == null)
            {
                startCondition = TutorialAuthoringCondition.CreateDefault();
            }
            else
            {
                startCondition.EnsureDefaults();
            }

            if (steps == null)
            {
                steps = new List<TutorialAuthoringStep>();
            }

            for (int i = steps.Count - 1; i >= 0; i--)
            {
                TutorialAuthoringStep step = steps[i];
                if (step == null)
                {
                    steps.RemoveAt(i);
                    continue;
                }

                step.EnsureDefaults(i + 1);
            }

            if (steps.Count <= 0)
            {
                steps.Add(TutorialAuthoringStep.CreateDefault(1));
            }

            if (uid > 0)
            {
                exportFileName = TutorialAddressableKeyUtility.GetDefinitionFileName(uid);
            }
        }

        /// <summary>
        /// Inspector 값이 변경될 때 기본값을 즉시 보정합니다.
        /// </summary>
        private void OnValidate()
        {
            EnsureDefaults();
        }

        /// <summary>
        /// 현재 Step 목록에서 사용하지 않는 다음 UID를 계산합니다.
        /// </summary>
        /// <returns>새 Step에 사용할 UID입니다.</returns>
        private int GetNextStepUid()
        {
            int maxUid = 0;
            if (steps != null)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    TutorialAuthoringStep step = steps[i];
                    if (step != null && step.Uid > maxUid)
                    {
                        maxUid = step.Uid;
                    }
                }
            }

            return maxUid + 1;
        }
    }
}
