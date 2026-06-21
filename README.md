# GGemCo 2D Tutorial

`com.ggemco.2d.core` 위에서 동작하는 단계 기반 튜토리얼 오케스트레이션 패키지입니다.

## 주요 기능

- Tutorial 테이블 기반 실행 목록과 개별 Tutorial JSON Addressables 지연 로딩
- 시작 조건, 단계 완료 조건, 진입/종료 액션 실행
- Core 맵 입장 및 몬스터 처치 이벤트 기본 연동
- 입력 액션, UI 창, Quest 상태 등 외부 패키지 이벤트의 느슨한 연동
- 입력 허용 목록 기반 차단 정책
- Core `SaveRegistry`를 이용한 `tutorial.progress` 저장 확장
- Tutorial JSON 검증 EditorWindow와 샘플 데이터

## 의존성

```text
Core
  ↑
Tutorial
```

Tutorial Runtime은 Quest, Control, Skill, Affect, AI BT와 TimingBattle을 직접 참조하지 않습니다.
외부 패키지 또는 게임 스크립트는 `TutorialEventBus`에 이벤트를 발행하거나
`TutorialActionHandlerRegistry`에 액션 처리기를 등록하여 연결합니다.

자세한 구조와 연동 예시는 `Documentation~/ARCHITECTURE.md`를 참고하십시오.
