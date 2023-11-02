WIP : 

1. Deferred Stylized Shading
WarpMap 기반 툰 셰이딩을 디퍼드 패스에서 구현

2. Non-physically based punctual light range
멀어지면 너무 어둡고, 가까우면 하얗게 타는 스팟, 포인트 라이트의 밝기를 Warp맵을 이용하여 어느 거리에서든 부드럽게 방출하는 선택적 파이프라인 기능

3. Main Character Focused Directional Light Shadow
메인캐릭터만 들어가는 크기의 직선광 그림자 절두체로 특정 캐릭터에게 그림자 해상도를 집중하는 선택적 파이프라인 기능

4. Cached punctual light shadow
고정된 스팟, 포인트 라이트는 고정된 스태틱 오브젝트를 항상 렌더할 필요가 없다는 것에 착안, 캐시 섀도우맵을 구워 다이나믹 오브젝트만 그리는 것으로 드로우콜을 아끼는 선택적 파이프라인 기능

planned in future:

1. Cached Directional light shadow
캐시 라이트 섀도우의 직선광 버전

2. Main Character Focused point light shadow
포인트 라이트 한면의 절두체를 메인 캐릭터가 가득 차도록 fov를 조절하여 포인트라이트의 6면 렌더를 절약하여 퍼포먼스를 높이는 선택적 기능
