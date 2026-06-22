# Tutorial 패키지 아키텍처

## 의존성 방향

```text
GGemCo2DCore
      ↑
GGemCo2DTutorial
      ↑
게임 전용 Adapter 또는 선택적 상위 패키지 연동
```

Tutorial Runtime은 Core만 참조합니다. Quest, Control, Skill, Affect, AI BT와
TimingBattle 타입을 직접 참조하지 않습니다.

## 주요 구성

- `TutorialPackageManager`
  - Core 게임 씬 준비를 기다린 뒤 저장 데이터와 실행 매니저를 생성합니다.
  - Core 이벤트 Adapter의 구독과 해제를 관리합니다.
  - 씬 로드 후 인스턴스가 없으면 자동 생성됩니다.
- `TutorialManager`
  - Catalog 로딩, 자동 시작 조건, 활성 Tutorial과 저장 상태를 조정합니다.
- `TutorialStepRunner`
  - 현재 단계 조건 진행, 진입 액션, 종료 액션과 단계 전환을 처리합니다.
- `TutorialAddressableRepository`
  - 개별 Tutorial JSON을 지연 로드하고 중복 요청을 합칩니다.
  - TextAsset 파싱 후 Addressables 핸들을 즉시 해제합니다.
- `TutorialData`
  - Core `SaveRegistry`의 `tutorial.progress` 확장 섹션을 사용합니다.
- `TutorialEventBus`
  - 외부 패키지의 입력, UI, Quest 상태와 사용자 정의 이벤트를 전달합니다.
- `TutorialActionHandlerRegistry`
  - UI 가이드, 강조, 기능 해금, Quest 시작 같은 상위 기능을 Adapter로 실행합니다.
- `TutorialInputBlockPolicy`
  - 현재 단계에서 허용한 입력 액션 목록과 차단 여부를 제공합니다.

## 외부 이벤트 연동

Control 또는 게임 입력 계층:

```csharp
TutorialEventBus.PublishInputAction(TutorialInputActionType.Jump);
```

UI 계층:

```csharp
TutorialEventBus.PublishWindowOpened(1001);
```

Quest Adapter:

```csharp
TutorialEventBus.PublishQuestState(10001, completed: true);
```

## 액션 처리기 연동

게임 상위 계층에서 `ITutorialActionHandler`를 구현하고 활성화 시 등록합니다.

```csharp
TutorialActionHandlerRegistry.Register(handler);
TutorialActionHandlerRegistry.Unregister(handler);
```

처리기는 `ShowGuide`, `HighlightUi`, `UnlockFeature`, `StartQuest`, `SetGameplayState` 액션을
프로젝트 정책에 맞게 실행합니다. Tutorial 패키지는 해당 시스템의 구체 타입을 알지 않습니다.

## 입력 제한 연동

Control을 직접 수정하지 않고 게임 상위 입력 규칙에서 다음 정책을 조회할 수 있습니다.

```csharp
TutorialPackageManager.Instance?.TutorialManager?.InputBlockPolicy.IsBlocked(actionId)
```

입력 제한이 프로젝트 공통 기능으로 확정되면 추후 Core의 일반 입력 정책 포트로 승격할 수 있습니다.
현재 단계에서는 Tutorial 전용 개념이 Core에 유입되지 않도록 패키지 내부에 유지합니다.

## Addressables 키

- Tutorial 목록과 자동 시작 조건은 `tutorial.txt` 테이블에서 로드합니다.
- 개별 Tutorial JSON 키는 UID 규칙에 따라 `Tutorial/tutorial_{uid}` 형식으로 계산합니다.

샘플 JSON은 `Samples~/DataAddressable/Tutorials`에 있습니다. 프로젝트로 가져온 뒤
각 TextAsset을 위 키로 Addressables에 등록해야 합니다.

## 저장 호환성

- 저장 섹션 키: `tutorial.progress`
- 기존 Core 저장 구조는 변경하지 않습니다.
- 신규 패키지이므로 과거 Tutorial 저장 데이터 마이그레이션은 없습니다.
- 향후 필드를 변경할 때는 `TutorialProgressSnapshot`의 하위 호환성을 유지해야 합니다.
