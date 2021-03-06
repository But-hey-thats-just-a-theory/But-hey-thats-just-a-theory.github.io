using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PoisonEffect : IStatusEffect
{
    public string Name { get; set; } = "Poison";
    public int Duration { get; set; }
	public int Speed { get; set; }
    public int Power { get; set; }
	public int RemainingTime { get; set; }

	public double ProcOdds { get; set; }
    public bool SelfInflicted { get; set; }

    public string Message { get; set; }
    public PoisonEffect(StatusEffectData data)
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
        if(RemainingTime % Speed == 0 && RemainingTime > 0)
        {
            m.CurrentHP -= Power;
        }
    }
	public void DoEffect(Player p)
    {
        if (RemainingTime % Speed == 0 && RemainingTime > 0)
        {
            MessageManager.AddMessage("You took " + Power + " damage from the poison.");
            p.CurrentHP -= Power;
        }
    }
}

