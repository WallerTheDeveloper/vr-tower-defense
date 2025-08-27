# VR Tower Defense

A Virtual Reality tower defense game built with Unity, featuring immersive spatial gameplay where players defend their headquarters using strategic tower placement in a fully 3D environment.

## Game Overview

Players must defend their headquarters from enemy attacks by strategically placing and managing different types of towers. The VR implementation allows for intuitive hand-based interactions and immersive spatial awareness that traditional tower defense games can't provide.

### Tower Types
- **Rocket Launcher** - High damage explosive projectiles
- **Machine Gun** - Rapid-fire ballistic attacks  
- **Laser Tower** - Continuous beam damage with visual effects

## Architecture & Design Patterns

This project emphasizes clean code architecture and scalable design patterns from the ground up:

### Core Design Patterns
- **Factory Pattern** - Modular tower and projectile creation system
- **State Pattern** - Complex tower and enemy behavior management
- **Command Pattern** - Action management with undo capability
- **Singleton Pattern** - Core game managers and systems

### Performance Optimizations  
- **Object Pooling** - Efficient memory management for projectiles and effects
- **Component-based Architecture** - Modular, reusable game systems

## Features Implemented

### Core Systems
- **Health System** - Modular damage and health management
- **Tower Behavior State Machine** - Complex tower AI with multiple states:
  - Auto-placement and ground alignment
  - Target acquisition and prioritization
  - Attack preparation and execution
  - Idle animations
- **Shooting Mechanics** - Projectile-based and raycast-based attacks
- **Particle Effects** - Visual feedback for combat and interactions
- **Object Pooling** - Performance-optimized projectile management

### VR Integration
- **Hand Tower Menu** - Wrist-mounted tower selection interface
- **Hand Pitch Interactions** - Natural grabbing and throwing mechanics
- **Spatial UI** - 3D interactive elements with hand tracking
- **Auto-placement System** - Smart tower positioning on surfaces

### UI & Interaction
- **Health Bar Visualization** - Real-time health display for all units
- **Hand Hover Detection** - Precise hand tracking for UI interactions
- **Pinch Gestures** - Natural selection and activation methods
- **Visual Feedback** - Emission-based material effects for interactive elements

## Technical Implementation

### Project Structure
```
Assets/Scripts/
├── Core/
│   ├── Commands/          # Command pattern implementation
│   ├── Factories/         # Factory pattern for unit creation
│   ├── HealthSystem/      # Modular health management
│   ├── Pooling/          # Object pooling system
│   ├── StateMachine/     # State pattern for unit behaviors
│   └── TowerUnits/       # Specialized tower implementations
├── Data/
│   ├── UI/               # UI data structures
│   └── Units/            # Unit configuration ScriptableObjects
├── Hands/                # Hand tracking and interaction
└── UI/
    └── Inventory/        # VR menu and interaction systems
```

### Key Components

#### State Machine System
Behavior management using the State Pattern:
```csharp
// Tower states: AutoPlacement → Idle → PrepareToAttack → Attack
// Enemy states: FlyTowardsTarget → ProjectileAttack → SelfExplode
```

#### Factory System
Flexible unit creation with concrete factories:
- `TowerFactory` - Tower unit instantiation with pooling
- `EnemyFactory` - Enemy unit creation
- `BulletFactory` / `RocketFactory` - Projectile creation

#### Command System
Action management with undo capability:
```csharp
// Spawn towers with command pattern for potential undo functionality
var command = new SpawnTowerCommand(unitFactory, position, rotation);
CommandManager.Instance.ExecuteCommand(command);
```

## VR-Specific Features

### Hand Tracking Integration
- **XR Hand Subsystem** integration for precise hand detection
- **Pinch Gestures** for natural selection mechanics
- **Wrist Menu Controller** - Palm orientation-based menu activation
- **Spatial Button Interactions** - 3D buttons with hover and selection states

### Performance Considerations
- **Object Pooling** prevents garbage collection spikes during intense gameplay
- **Efficient State Management** reduces computational overhead
- **Optimized Particle Systems** maintain frame rate stability
- **Smart Target Acquisition** with layered priority system

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or newer
- XR Interaction Toolkit
- XR Hands package
- VR headset with hand tracking support

## 🎮 How to Play

1. **Activate Wrist Menu**: Orient your palm towards your face
2. **Select Tower**: Use pinch gestures on 3D tower models
3. **Place Towers**: Grab and position/throw towers strategically
4. **Auto-placement**: Towers automatically align to surfaces
5. **Defend**: Towers automatically engage enemies within range

## 💻 Code Examples

### State Pattern Implementation
Tower behavior management using clean state transitions:

```csharp
public class ProjectileTowerUnit : Unit
{
    private TowerAutoPlacement _autoPlacementState;
    private TowerProjectileAttack _attackState;
    private TowerIdle _idleState;
    private TowerPrepareToAttack _towerPrepareState;

    protected override void Initialize()
    {
        // Get state components and set up event handlers
        _autoPlacementState = statesLayer.GetComponent<TowerAutoPlacement>();
        _attackState = statesLayer.GetComponent<TowerProjectileAttack>();
        
        base.ChangeState(_autoPlacementState);
        
        // Event-driven state transitions
        _autoPlacementState.OnStateFinished += OnAutoPlacementStateFinished;
        _towerPrepareState.OnStateFinished += OnTowerPrepareStateFinished;
    }

    protected override void Tick()
    {
        // Smart state management based on target availability
        if (currentTarget == null && !_autoPlacementState.IsStateActive && !_idleState.IsStateActive)
        {
            base.ChangeState(_idleState);
        }
        if (currentTarget != null && !_towerPrepareState.IsStateActive && !_attackState.IsStateActive)
        {
            base.ChangeState(_towerPrepareState, base.currentTarget);
        }
    }
}
```

### Object Pooling System
High-performance memory management with generic pool implementation:

```csharp
public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation) where T : Component
{
    if (!_objectPool.ContainsKey(typePrefab.gameObject))
    {
        CreatePool(typePrefab.gameObject, spawnPos, spawnRotation);
    }
    
    GameObject obj = _objectPool[typePrefab.gameObject].Get();
    
    if (!_cloneToPrefabMap.ContainsKey(obj))
    {
        _cloneToPrefabMap.Add(obj, typePrefab.gameObject);
    }
    
    obj.transform.position = spawnPos;
    obj.transform.rotation = spawnRotation;
    obj.SetActive(true);
    
    return obj.GetComponent<T>();
}
```

### VR Hand Interaction System
Precise hand tracking with pinch gesture recognition:

```csharp
private void CheckPinchGesture()
{
    XRHand hand = useLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
    
    if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbTipPose) &&
        hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTipPose))
    {
        float pinchDistance = Vector3.Distance(thumbTipPose.position, indexTipPose.position);
        
        if (!isPinching && pinchDistance <= pinchThreshold && 
            !PieMenuHoverManager.Instance.HasAnyButtonTriggeredPinchAction())
        {
            bool canStartPinch = requireHoverToPinch ? isHovering : 
                IsClosestButtonToPinch(thumbTipPose.position, indexTipPose.position);
                
            if (canStartPinch)
            {
                isPinching = true;
                OnPinchSelectAction(); // Trigger tower spawning
            }
        }
    }
}
```

### Command Pattern with Factory Integration
Clean action management with undo capability:

```csharp
[Serializable]
public class SpawnTowerCommand : ICommand
{
    private UnitFactory _unitFactory;
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    
    public SpawnTowerCommand(UnitFactory unitFactory, Vector3 position, Quaternion rotation)
    {
        _unitFactory = unitFactory;
        _spawnPosition = position;
        _spawnRotation = rotation;
    }

    public void Execute()
    {
        _unitFactory.CreateTower(_spawnPosition, _spawnRotation);
    }

    public void Undo()
    {
        // Implementation for tower removal
    }
}

// Usage in VR interaction
public void OnButtonClick()
{
    Vector3 spawnPosition = GetSpawnPosition();
    Quaternion spawnRotation = GetSpawnRotation();
    
    var command = new SpawnTowerCommand(unitFactory, spawnPosition, spawnRotation);
    CommandManager.Instance.ExecuteCommand(command);
}
```

### Health System with MVC Pattern
Modular damage system with clean separation of concerns:

```csharp
public class HealthController : MonoBehaviour
{
    [SerializeField] private HealthBarView view;
    private HealthModel _model;

    private void OnEnable()
    {
        _model = new HealthModel(baseUnitSettings, baseUnitSettings.MaxHealth);
        _model.OnDeath += OnDeath;
        view.Initialize(baseUnitSettings.MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        _model.TakeDamage(damage);
        view.UpdateHealthBar(_model.HealthPercentage);
    }

    private void OnDeath()
    {
        // Particle effects, cleanup, and pooling
        ParticleSystem deathEffect = Instantiate(baseUnitSettings.DeathParticleSystem, 
            transform.position, Quaternion.identity);
        
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}
```

### Smart Target Acquisition System
Priority-based enemy targeting with layered detection:

```csharp
[System.Serializable]
public class LayerGroup
{
    public string name;
    public LayerMask layerMask;
    public int priority; // Lower number = higher priority
}

private TargetInfo FindNewTargetWithPriority()
{
    foreach (var layerGroup in layerGroups)
    {
        Transform target = FindClosestTarget(layerGroup.layerMask);
        if (target != null)
        {
            return new TargetInfo(target, layerGroup.priority);
        }
    }
    return new TargetInfo(null, int.MaxValue);
}

private bool ShouldSwitchTarget(TargetInfo newTargetInfo)
{
    if (newTargetInfo.priority < currentTargetPriority)
    {
        Debug.Log($"Switching to higher priority target. Old: {currentTargetPriority}, New: {newTargetInfo.priority}");
        return true;
    }
    return false;
}
```

## Development Status

This project is in active development with a focus on:
- Clean, maintainable code architecture
- Performance optimization for VR
- Immersive spatial interactions
- Scalable design patterns

## Code Quality Focus

This project demonstrates:
- **SOLID Principles** applied throughout the codebase
- **Design Pattern Implementation** for maintainable architecture
- **Performance-First Approach** with object pooling and optimized rendering
- **Modular Component Design** for easy feature extension

## Contributing

This project serves as a demonstration of clean VR game architecture. Feel free to explore the codebase to see practical implementations of:
- State machines in game development
- Factory pattern for flexible object creation
- Command pattern for action management
- VR-specific interaction patterns
- Performance optimization techniques

---
