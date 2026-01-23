using UnityEngine;

public class SquadMove : SquadState
{
    private int leaderStateDepth;
    private bool holdingCorner = false;
    private float cornerTimer = 0f;
    private float cornerHoldTime = 5f;
    private float cornerHoldActionTime = 2f;
    private bool cornerActionTrigger = false;

    public SquadMove(Squad squad) : base(squad) { }

    public override void Enter()
    {
        if (squad == null || squad.leader == null)
            return;

        Vector3 dir = squad.coverFrom - squad.leader.transform.position;
        int coverDir = NavGridGen.GetClosestDirectionIndex(dir);

        squad.path = AStarPathFinder.FindPathWithCover(
            squad.leader.transform.position,
            squad.destination,
            coverDir
        );

        squad.currentPathIndex = 0;

        if (squad.path == null || squad.path.Count == 0)
        {
            Debug.LogWarning("Squad path invalid");
            return;
        }

        AssignFileIndienne();
    }

    public override void Update()
    {
        if (squad == null || squad.leader == null)
            return;

        // ---- Corner check ----
        int i, j, h;
        if (!holdingCorner &&
            NavGridGen.WorldToGrid(squad.leader.transform.position, out i, out j, out h))
        {
            NodeGrid n = NavGridGen.grid[i, j];
            if (n.isCorner[h])
            {
                EnterCornerHold();
                return;
            }
        }

        // ---- Corner hold ----
        if (holdingCorner)
        {
            CornerHoldRoutine();
            cornerTimer += Time.deltaTime;
            if (cornerTimer >= cornerHoldTime)
            {
                ExitCornerHold();
            }
            return;
        }

        // ---- Leader finished movement ----
        if (squad.leader.GetStateDepth() < leaderStateDepth)
        {
            squad.currentPathIndex++;

            if (squad.currentPathIndex >= squad.path.Count)
            {
                squad.SetState(new SquadIdle(squad));
                return;
            }

            MoveLeaderToNext();
        }
    }

    public override void Exit()
    {
        squad.FinishSquadStates();
    }

    // =======================
    // Movement
    // =======================

    void AssignFileIndienne()
    {
        squad.FinishSquadStates();

        squad.PushSquadState(
            squad.leader,
            new MovingAndClimb(squad.path[0])
        );

        leaderStateDepth = squad.leader.GetStateDepth();

        for (int i = 1; i < squad.soldiers.Count; i++)
        {
            squad.PushSquadState(
                squad.soldiers[i],
                new Follow(squad)
            );
        }
    }

    void MoveLeaderToNext()
    {
        squad.PushSquadState(
            squad.leader,
            new MovingAndClimb(squad.path[squad.currentPathIndex])
        );

        leaderStateDepth = squad.leader.GetStateDepth();
    }

    // =======================
    // Corner Hold
    // =======================

    void EnterCornerHold()
    {
        holdingCorner = true;
        cornerTimer = 0f;

        squad.FinishSquadStates();

        // 👉 ici plus tard :
        // - soldats en appui
        // - lancer grenade
        // - peek / suppress
    }

    void CornerHoldRoutine()
    {
        if (cornerTimer < cornerHoldActionTime) return;
        if (!cornerActionTrigger)
        {
            squad.PushSquadState(squad.leader, new TacticalMove(squad.path[squad.currentPathIndex], squad.coverFrom, true));
            var cover = squad.FindCoverPoints(squad.leader.transform.position, squad.coverFrom, 10, squad.soldiers.Count - 1);
            for (int i = 1; i < squad.soldiers.Count; i++)
            {
                squad.PushSquadState(
                    squad.soldiers[i],
                    new TacticalMove(cover[i], squad.coverFrom, false)
                ) ;
            }
            cornerActionTrigger = true;

        }
        
    }

    void ExitCornerHold()
    {
        holdingCorner = false;
        cornerActionTrigger = false;

        MoveLeaderToNext();
        RebuildFormation();
    }

    void RebuildFormation()
    {
        // Leader : ne rien faire, il est piloté par SquadMove
        for (int i = 1; i < squad.soldiers.Count; i++)
        {
            AIbase s = squad.soldiers[i];
            if (s == null) continue;

            s.AddState(new Follow(squad));
        }
    }

}


