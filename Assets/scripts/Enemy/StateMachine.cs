using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activeState;
    public void Initialise()
    {
        Changstate(new PatrolState());
    }
    void Update()
    {
        if (activeState != null)
        {
            activeState.Preform();
        }
    }

    public void Changstate(BaseState neWStrate)
    {
        if (activeState != null)
        {
            activeState.Exit();
        }

        activeState = neWStrate;

        if (activeState != null)
        {
            activeState.stateMachine = this;
            activeState.enemy = GetComponent<Enemy>();
            activeState.Enter();
        }
    }
}
