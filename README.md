
# Task Streamer

![Image](https://github.com/user-attachments/assets/6f65121c-04bb-4344-9344-0b598b452d04)  
**TaskStreamer**는 Unity 에디터 환경에서 AI 행동 시스템을 시각적으로 설계하고 관리할 수 있도록 지원하는 노드 기반 그래프 툴킷입니다.  
주요 목적은 **Behavior Tree(행동 트리)**, **Finite State Machine(유한 상태 머신, FSM)** 기반의 AI 로직을 직관적으로 생성·편집·디버깅할 수 있게 하며, BT와 FSM, 두 그래프를 결합하여 페이징 기법 등, 다양한 방법을 가능하게 합니다.

---

# Feature

## Behavior Tree & FSM 지원
![Image](https://github.com/user-attachments/assets/c7de68d6-340b-498a-b8ce-656f50adbc46)
- 계층적 의사결정(Behavior Tree)과 상태 기반 제어(FSM) 모두 지원  
- 각 그래프 타입별로 노드, 전이, 조건, 서비스 등 다양한 요소를 시각적으로 배치 및 연결 가능
- Sub Graph를 통한 BT와 FSM 결합을 지원.

## Blackboard 시스템
- 변수(BlackboardVariable)를 통한 데이터 공유 및 상태 관리  
- 런타임/에디터 블랙보드 동기화 및 자동 변수 관리

## Unity 에디터 완전 통합
- 커스텀 에디터 창, Inspector, UIElements 기반의 패널 및 레이아웃  
- 우클릭 메뉴, 드래그 앤 드롭, 템플릿 기반 노드/스크립트 생성 등 생산성 강화

## 런타임 컴포넌트
- `TaskStreamer` MonoBehaviour를 통해 그래프 실행 및 변수 접근  
- 다양한 Tick 모드(수동, Fixed, Late, External) 지원  
- 런타임 중 그래프 상태 및 변수 실시간 조회/수정 가능

## 샘플 및 템플릿 제공
 ![Image](https://github.com/user-attachments/assets/128e9ae4-23f2-485e-95a8-ae3363280ec4)  
- Unity 공식 Starter Package 기반 샘플 에셋 포함  
- BT/FSM/Blackboard 등 각종 노드/상태/서비스 템플릿 제공

---

# Installation

## Requirements

- Unity 6.0 or later


![Image](https://github.com/user-attachments/assets/5cfd3805-d676-4800-88a1-c1704ef39549)

```https
https://github.com/Stellar-F0X/Task-Streamer.git?path=taskStreamer/Assets/TaskStreamer
```