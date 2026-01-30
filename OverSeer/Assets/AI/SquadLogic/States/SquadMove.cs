using UnityEngine;

public class SquadMove : SquadState
{
    private int leaderStateDepth;
    private bool holdingCorner = false;
    private float cornerTimer = 0f;
    private float cornerHoldTime = 5f;
    private float cornerHoldActionTime = 2f;
    private bool cornerActionTrigger = false;

    private bool init = false;
    public SquadMove(Squad squad) : base(squad) { }

    public override void Enter()
    {
        if (squad == null || squad.leader == null)
            return;

        if (!NavGridGen.ready) return;
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
        init = true;
    }

    public override void Update()
    {
        if (!init) Enter();
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
            new MovingAndClimb(squad.path[squad.path.Count - 1])
        ) ;

        leaderStateDepth = squad.leader.GetStateDepth();

        for (int i = 1; i < squad.soldiers.Count; i++)
        {
        }
    }

    void MoveLeaderToNext()
    {
        squad.PushSquadState(
            squad.leader,
            new MovingAndClimb(squad.path[squad.path.Count - 1])
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
            squad.PushSquadState(squad.leader, new TacticalMove(squad.path[squad.path.Count - 1], squad.coverFrom, true,false)) ;
            var cover = squad.FindCoverPoints(squad.leader.transform.position, squad.coverFrom, 30, squad.soldiers.Count);
            Debug.Log("covers = " + cover.Count + ", soldier = " + squad.soldiers.Count);
            for (int i = 1; i < squad.soldiers.Count; i++)
            {
                squad.PushSquadState(
                    squad.soldiers[i],
                    new TacticalMove(cover[i], squad.coverFrom, false,false)
                );
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
        squad.ReassignFormation();
    }

   
}


