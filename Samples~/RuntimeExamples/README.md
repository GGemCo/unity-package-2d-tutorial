# Tutorial Runtime 코드 예제

이 샘플은 게임 코드와 Tutorial Runtime 사이의 연결 지점을 보여 줍니다.

## 구성

- `TutorialExampleEventPublisher`
  - 입력, UI, Quest, 사용자 정의 이벤트를 발행합니다.
  - Tutorial UID를 지정해 수동 시작할 수 있습니다.
  - 현재 입력 제한 정책을 조회한 뒤 허용된 입력만 발행합니다.
- `TutorialExampleActionHandler`
  - 가이드 표시, UI 강조, 기능 해금, Quest 시작, 사용자 정의 액션을 UnityEvent로 전달합니다.
  - 활성화/비활성화 시 액션 처리기를 안전하게 등록/해제합니다.

## 빠른 확인

1. 빈 GameObject에 두 컴포넌트를 추가합니다.
2. `TutorialExampleActionHandler`의 UnityEvent를 프로젝트 UI 또는 로그 처리 메서드에 연결합니다.
3. Unity UI Button의 `OnClick`에 `TutorialExampleEventPublisher` 메서드를 연결합니다.
4. `Tutorial Data` 샘플을 적용한 후 다음 순서로 이벤트를 발행합니다.

| 예제 | 이벤트 순서 |
|---|---|
| 기본 조작 | `PublishExampleStart` → `PublishMove` → `PublishJump` |
| UI 상호작용 | `PublishInventoryOpened` → `PublishInventoryItemClicked` → `PublishEquipButtonClicked` |
| 전투 누적 | 맵 100 입장 후 UID 200 몬스터를 3회 처치 |
| 사용자 정의 | `PublishOnboardingReady` → `PublishTutorialTokenCollected` 3회 |

실제 프로젝트에서는 버튼 연결용 예제 메서드 대신 Input, UI, Quest, 게임 전용 시스템의 성공 시점에서 같은 EventBus API를 호출하십시오.
