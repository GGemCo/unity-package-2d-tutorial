using System.Collections.Generic;
using GGemCo2DCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GGemCo2DTutorial
{
    /// <summary>
    /// Tutorial 전용 저장 파일에 기록되는 데이터 컨테이너입니다.
    /// </summary>
    public sealed class SaveDataContainerTutorial
    {
        /// <summary>
        /// 튜토리얼 UID별 진행 상태를 보관하는 Tutorial 데이터입니다.
        /// </summary>
        public TutorialData TutorialData;

        /// <summary>
        /// 향후 Tutorial 하위 기능이 추가할 수 있는 확장 저장 섹션입니다.
        /// </summary>
        public Dictionary<string, JToken> Extensions;
    }

    /// <summary>
    /// Tutorial 진행 데이터의 초기화, 복원과 Tutorial 전용 파일 저장을 담당합니다.
    /// </summary>
    public sealed class SaveDataManagerTutorial : SaveDataManagerBase
    {
        /// <summary>
        /// 현재 게임에서 사용하는 Tutorial 진행 데이터입니다.
        /// </summary>
        public TutorialData Tutorial { get; private set; }

        /// <summary>
        /// 로딩 씬에서 읽은 Tutorial 전용 저장 파일과 Core 확장 섹션을 이용해 진행 데이터를 복원합니다.
        /// 전용 저장 파일이 있으면 해당 데이터를 우선 적용하고, 없으면 기존 Core의
        /// tutorial.progress 섹션을 하위 호환 폴백으로 사용합니다.
        /// </summary>
        protected override void InitializeData()
        {
            SaveDataContainerTutorial saveDataContainer =
                SaveDataLoaderTutorial.Instance?.GetSaveDataContainer();

            Tutorial = new TutorialData();

            // Core 저장 파일에서 이미 읽은 tutorial.progress 보류 데이터를 먼저 적용합니다.
            // 전용 파일의 Extensions도 먼저 복원한 뒤 Tutorial 본 데이터를 마지막에 적용하여
            // SaveDataTutorial.json의 진행 상태가 항상 최종 우선순위를 갖게 합니다.
            Tutorial.Register();

            if (saveDataContainer?.Extensions != null)
            {
                var envelope = new SaveEnvelope();
                foreach (KeyValuePair<string, JToken> extension in
                         saveDataContainer.Extensions)
                {
                    envelope.Sections[extension.Key] = extension.Value;
                }

                SaveRegistry.ApplyRestore(envelope);
            }

            Tutorial.Initialize(saveDataContainer);
        }

        /// <summary>
        /// 현재 Tutorial 진행 데이터를 선택된 슬롯의 Tutorial 전용 저장 파일에 기록합니다.
        /// </summary>
        /// <returns>Tutorial 저장 파일 기록에 성공하면 true를 반환합니다.</returns>
        public override bool SaveData()
        {
            if (!base.SaveData())
            {
                return false;
            }

            string filePath = saveFileController.GetSaveFilePath(
                currentSaveSlot,
                SaveDataConstantsTutorial.SaveDataFileName);

            var saveData = new SaveDataContainerTutorial
            {
                TutorialData = Tutorial,
            };

            string json = JsonConvert.SerializeObject(saveData);
            SaveDataFileService.WriteAllText(
                filePath,
                json,
                SaveDataConstantsTutorial.CreateIdentity(currentSaveSlot));
            return true;
        }

        /// <summary>
        /// 매니저가 제거될 때 Tutorial 저장 기여자 등록을 해제합니다.
        /// </summary>
        protected override void OnDestroy()
        {
            Tutorial?.Unregister();
            Tutorial = null;
        }
    }
}
