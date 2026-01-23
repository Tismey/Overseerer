


public abstract class SquadState
{
    protected Squad squad;

    public SquadState(Squad squad)
    {
        this.squad = squad;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
