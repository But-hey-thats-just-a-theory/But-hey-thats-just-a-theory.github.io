using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class HypnotizeEffect : IStatusEffect
{
    public string Name { get; set; } = "Hypnotize";
    public int Duration { get; set; }
    public int Speed { get; set; }
    public int Power { get; set; }
    public int RemainingTime { get; set; }

    public double ProcOdds { get; set; }

    public string Message { get; set; }
    public bool SelfInflicted { get; set; }
    public HypnotizeEffect(StatusEffectData data)
    {
        Name = data.Name;
        Duration = data.Duration;
        Speed = data.Speed;
        ProcOdds = data.ProcOdds;
        Power = data.Power;
        Message = data.Message;
        RemainingTime = data.Duration;
        SelfInflicted = data.SelfInflicted;
    }
    public void DoEffect(Monster m)
    {
        if (RemainingTime % Speed == 0 && RemainingTime > 0)
        {
            m.TicksToNextAttack = m.AttackSpeed;
        }
    }
    public void DoEffect(Player p)
    {
        if (RemainingTime % Speed == 0 && RemainingTime > 0)
        {
            p.TicksToNextAttack = p.GetWeaponAttackSpeed();
        }
    }
}

