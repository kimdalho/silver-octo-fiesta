using System;
using UnityEngine;

public enum GameStep
{
    Local,
    SelectBattleField,
    EnterBattleField,
    Battle,
    Return
}

public class GameLoopManager : MonoBehaviour
{
    public GameStep CurrentStep;

    public void ChangeStep(GameStep step)
    {
        CurrentStep = step;

        switch (step)
        {
            case GameStep.Local:
                EnterLocal();
                break;

            case GameStep.SelectBattleField:
                EnterSelect();
                break;

            case GameStep.EnterBattleField:
                EnterBattleField();
                break;

            case GameStep.Battle:
                StartBattle();
                break;

            case GameStep.Return:
                ReturnToLocal();
                break;
        }
    }

    private void ReturnToLocal()
    {
        throw new NotImplementedException();
    }

    private void StartBattle()
    {
        throw new NotImplementedException();
    }

    private void EnterBattleField()
    {
        throw new NotImplementedException();
    }

    private void EnterSelect()
    {
        throw new NotImplementedException();
    }

    private void EnterLocal()
    {
        throw new NotImplementedException();
    }
}