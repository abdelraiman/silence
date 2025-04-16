using Unity.VisualScripting;

public abstract class BaseState
{
    public StateMachine stateMachine;
    public Enemy enemy;
    public abstract void Enter();
    public abstract void Preform();
    public abstract void Exit();
}