# Tutorial 데이터 예제

튜토리얼 시스템의 대표 조건과 액션을 확인할 수 있는 데이터 예제입니다.

## 포함 예제

| UID | 예제 | 자동 시작 조건 | 확인할 기능 |
|---:|---|---|---|
| 10001 | 기본 조작 | `InputAction / TutorialExampleStart` | 입력 허용 목록, 이동·점프 단계 전환 |
| 10002 | UI 상호작용 | `OpenWindow / Inventory` | UI 강조, 클릭 조건, 강조 해제 |
| 10003 | 전투 누적 | `EnterMap / 100` | 동일 몬스터 3회 처치 누적 |
| 10004 | 사용자 정의 이벤트 | `Custom / OnboardingReady` | 사용자 정의 조건, 기능 해금 액션 |

## 적용 순서

1. Package Manager에서 `Tutorial Data` 샘플을 가져옵니다.
2. `Tables/tutorial.txt`를 프로젝트의 Tutorial 테이블 경로에 복사하거나 기존 테이블에 행을 병합합니다.
3. `Tutorials` 폴더의 JSON을 프로젝트 Tutorial Addressables 경로에 복사합니다.
4. GGemCo Tutorial 설정 도구로 테이블과 JSON Addressables를 등록합니다.
5. `Tutorial Runtime Examples` 샘플의 컴포넌트 또는 게임 코드에서 시작 이벤트를 발행합니다.

기존 프로젝트에 같은 UID가 있으면 덮어쓰지 말고 예제 UID를 변경하십시오. UID를 변경할 때는 테이블 행과 JSON의 `uid`, 파일명, Addressables 키를 함께 변경해야 합니다.
