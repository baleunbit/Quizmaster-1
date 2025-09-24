# 오디오 리소스 폴더

이 폴더는 게임의 배경음악과 효과음을 저장합니다.

## 폴더 구조
```
4_오디오/
├── BGM/           # 배경음악 파일들
│   ├── StartScreen.wav      # 시작화면 BGM
│   ├── SubjectSelection.wav # 과목선택 BGM
│   ├── Quiz.wav            # 퀴즈 진행 BGM
│   ├── EndScreen.wav       # 결과화면 BGM
│   └── Loading.wav         # 로딩 BGM
└── SFX/           # 효과음 파일들
    ├── ButtonClick.wav     # 버튼 클릭음
    ├── Correct.wav         # 정답 효과음
    ├── Wrong.wav           # 오답 효과음
    └── Timer.wav           # 타이머 효과음
```

## 오디오 파일 요구사항
- **포맷**: WAV, MP3, OGG
- **비트레이트**: 128kbps 이상 권장
- **길이**: BGM은 30초~2분 권장 (반복재생)
- **볼륨**: -3dB ~ -6dB 권장 (클리핑 방지)

## Unity에서 설정 방법
1. 오디오 파일을 해당 폴더에 복사
2. Unity에서 Import Settings 확인
3. AudioManager 스크립트의 SerializeField에 연결
4. 게임 실행하여 테스트

## 권장 BGM 스타일
- **StartScreen**: 밝고 친근한 메인 테마
- **SubjectSelection**: 차분한 선택 화면 음악
- **Quiz**: 긴장감 있는 퀴즈 음악
- **EndScreen**: 성취감을 주는 결과 음악
- **Loading**: 부드러운 로딩 음악
