using UnityEngine;

public class SquadCombat : SquadState
{
    private bool initialized = false;

    public SquadCombat(Squad squad) : base(squad) { }

    public override void Enter()
    {
        initialized = false;
    }

    public override void Update()
    {
        if (squad == null || squad.soldiers.Count == 0)
            return;

        // 🔴 Sortie combat : plus personne en alert
        if (!AnySoldierAlert())
        {
            squad.SetState(new SquadMove(squad));
            return;
        }

        // 🟢 Init combat (une seule fois)
        if (!initialized)
        {
            InitializeCombat();
            initialized = true;
        }
    }

    public override void Exit()
    {
    }

    // ----------------------------------------------------
    // INIT COMBAT
    // ----------------------------------------------------
    private void InitializeCombat()
    {
        Vector3 danger = GetGlobalDanger();

        var covers = squad.FindCoverPoints(squad.leader.transform.position, squad.coverFrom, 10, squad.soldiers.Count);
        int i = 0;
        foreach (var soldier in squad.soldiers)
        {
            if (soldier == null)
                continue;



            soldier.AddState(new TacticalMove(covers[i], danger, false,false));
            i++;
        }
    }

    // ----------------------------------------------------
    // UTILS
    // ----------------------------------------------------
    private bool AnySoldierAlert()
    {
        foreach (var s in squad.soldiers)
        {
            if (s != null &&  s.alert)
                return true;
        }
        return false;
    }

    private Vector3 GetGlobalDanger()
    {
       

        return squad.leader.transform.forward; // fallback safe
    }
}
