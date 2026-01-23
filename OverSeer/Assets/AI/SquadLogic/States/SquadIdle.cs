public class SquadIdle : SquadState
{
    public SquadIdle(Squad squad) : base(squad) { }

    public override void Enter()
    {
        foreach (var s in squad.soldiers)
            s.AddState(new Idle());
    }

    public override void Update()
    {
        // plus tard : détection joueur
    }

    public override void Exit()
    {
    }
}
