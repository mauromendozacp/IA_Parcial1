using System;
using System.Collections.Generic;

public class State
{
    public List<Action> behaviours = null;
    public Action onAbruptExit = null;
}

public class FSM
{
    #region PRIVATE_FIELDS
    private int currentState = 0;
    private int[,] relations = null;
    private Dictionary<int, State> behaviours = null;
    #endregion

    #region CONSTRUCTORS
    public FSM(int states, int flags)
    {
        currentState = -1;

        relations = new int[states, flags];
        for (int i = 0; i < states; i++)
        {
            for (int j = 0; j < flags; j++)
            {
                relations[i, j] = -1;
            }
        }

        behaviours = new Dictionary<int, State>();
    }
    #endregion

    #region PUBLIC_METHODS
    public void ForceCurretState(int state)
    {
        currentState = state;
    }

    public void SetRelation(int sourceState, int flag, int destinationState)
    {
        relations[sourceState, flag] = destinationState;
    }

    public void SetFlag(int flag)
    {
        if (relations[currentState, flag] != -1)
            currentState = relations[currentState, flag];
    }

    public int GetCurrentState()
    {
        return currentState;
    }

    public void SetBehaviour(int state, Action behaviour, Action onExitBehaviour = null)
    {
        State newState = new State();
        newState.behaviours = new List<Action>();
        newState.behaviours.Add(behaviour);
        newState.onAbruptExit = onExitBehaviour;

        if (behaviours.ContainsKey(state))
            behaviours[state] = newState;
        else
            behaviours.Add(state, newState);
    }

    public void AddBehaviour(int state, Action behaviour, Action onExitBehaviour = null)
    {
        if (behaviours.ContainsKey(state))
            behaviours[state].behaviours.Add(behaviour);
        else
        {
            State newState = new State();
            newState.behaviours = new List<Action>();
            newState.behaviours.Add(behaviour);
            newState.onAbruptExit = onExitBehaviour;

            behaviours.Add(state, newState);
        }
    }

    public void Exit()
    {
        if (behaviours.ContainsKey(currentState))
        {
            Action onExit = behaviours[currentState].onAbruptExit;
            onExit?.Invoke();
        }
    }

    public void Update()
    {
        if (behaviours.ContainsKey(currentState))
        {
            List<Action> actions = behaviours[currentState].behaviours;
            if (actions != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] != null)
                    {
                        actions[i].Invoke();
                    }
                }
            }
        }
    }
    #endregion
}