using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Converters;
using Quepland_2.Bosses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

public class BattleManager
{
    private static readonly BattleManager instance = new BattleManager();
    private BattleManager() { }
    static BattleManager() { }
    public static BattleManager Instance { get { return instance; } }
    public List<Monster> Monsters = new List<Monster>();
    public List<Monster> CurrentOpponents { get; set; } = new List<Monster>();
    public IBoss CurrentBoss { get; set; }
    public Monster Target { get; set; }
    public Area CurrentArea;
    public Dojo CurrentDojo { get; set; }
    public bool BattleHasEnded = true;
    public bool WonLastBattle = false;
    public bool WaitedAutoBattleGameTick { get; set; }
    public bool AutoBattle { get; set; }
    public Monster SelectedOpponent { get; set; }
    private static readonly Random random = new Random();
    public async Task LoadMonsters(HttpClient Http)
    {
        Monsters.AddRange(await Http.GetJsonAsync<Monster[]>("data/Monsters/Overworld.json"));
        Monsters.AddRange(await Http.GetJsonAsync<Monster[]>("data/Monsters/Bosses.json"));
        Monsters.AddRange(await Http.GetJsonAsync<Monster[]>("data/Monsters/DojoOpponents.json"));
        foreach(Monster m in Monsters)
        {
            m.LoadStatusEffects();
        }
    }
    public void StartBattle()
    {
        if (CurrentOpponents == null || CurrentOpponents.Count == 0)
        {
            Console.WriteLine("Opponents were null or nonexistent.");
            return;
        }
        else
        {
            foreach(Monster monster in CurrentOpponents)
            {
                ResetOpponent(monster);
            }
            BattleHasEnded = false;
        }
        WaitedAutoBattleGameTick = false;
        Target = GetNextTarget();
    }
    public void StartBattle(Area area)
    {
        CurrentArea = area;
        int r = random.Next(0, CurrentArea.Monsters.Count);
        CurrentOpponents.Clear();
        CurrentOpponents.Add(Monsters.FirstOrDefault(x => x.Name == CurrentArea.Monsters[r]));
        SelectedOpponent = null;
        if(CurrentOpponents[0] == null)
        {
            Console.WriteLine("No monsters found for area.");
        }
        else
        {
            foreach(Monster m in CurrentOpponents)
            {
                m.IsDefeated = false;
            }
           
        }
        StartBattle();
        
    }
    public void StartBattle(Monster opponent)
    {
        CurrentOpponents.Clear();
        CurrentOpponents.Add(opponent);
        if (AutoBattle)
        {
            SelectedOpponent = opponent;
        }
        StartBattle();
    }
    public void StartBattle(List<Monster> opponents)
    {
        CurrentOpponents.Clear();
        CurrentOpponents.AddRange(opponents);
        SelectedOpponent = null;
        StartBattle();
    }
    public void DoBattle()
    {
        if(BattleHasEnded == false)
        {
            foreach(Monster opponent in CurrentOpponents)
            {
                if(opponent.IsDefeated)
                {
                    opponent.TicksToNextAttack = opponent.AttackSpeed;
                }
                else
                {
                    opponent.TicksToNextAttack--;
                }
            }
            Player.Instance.TicksToNextAttack--;
            if (Player.Instance.TicksToNextAttack < 0)
            {
                Attack();
                if (CurrentBoss != null)
                {
                    CurrentBoss.OnBeAttacked(Target);
                }
                Player.Instance.TicksToNextAttack = Player.Instance.GetWeaponAttackSpeed();
            }
            foreach(Monster opponent in CurrentOpponents)
            {
                if (opponent.CurrentHP <= 0 && opponent.IsDefeated == false)
                {
                    opponent.CurrentHP = 0;                   
                    Drop drop = opponent.DropTable.GetDrop();

                    if (LootTracker.Instance.TrackLoot)
                    {
                        foreach(Drop d in opponent.DropTable.AlwaysDrops)
                        {
                            LootTracker.Instance.Inventory.AddDrop(d);
                        }
                        LootTracker.Instance.Inventory.AddDrop(drop);
                    }
                    else
                    {
                        MessageManager.AddMessage("You defeated the " + opponent.Name + ".");
                        foreach (Drop d in opponent.DropTable.AlwaysDrops)
                        {
                            Player.Instance.Inventory.AddDrop(d);
                        }
                        Player.Instance.Inventory.AddDrop(drop);
                    }
                    
                    opponent.IsDefeated = true;
                    if(CurrentBoss != null)
                    {
                        CurrentBoss.OnDie(opponent);
                    }
                }
                else if (opponent.TicksToNextAttack < 0 && opponent.IsDefeated == false)
                {
                    if (CurrentBoss != null && CurrentBoss.CustomAttacks)
                    {
                        CurrentBoss.OnAttack();
                    }
                    else if(CurrentBoss != null)
                    {
                        CurrentBoss.OnAttack();
                        BeAttacked(opponent);
                    }
                    else
                    {
                        BeAttacked(opponent);
                        
                    }
                    opponent.TicksToNextAttack = opponent.AttackSpeed;
                }
            }
            if (AllOpponentsDefeated())
            {
                if(CurrentDojo != null)
                {
                    CurrentDojo.CurrentOpponent++;
                    if(CurrentDojo.CurrentOpponent >= CurrentDojo.Opponents.Count)
                    {
                        CurrentDojo.LastWinTime = DateTime.Now;
                    }
                }
                WonLastBattle = true;
                EndBattle();
            }
            if (CurrentBoss != null)
            {
                CurrentBoss.TicksToNextSpecialAttack--;
                if(CurrentBoss.TicksToNextSpecialAttack <= 0)
                {
                    CurrentBoss.OnSpecialAttack();
                }
                
            }

            if (Player.Instance.CurrentHP <= 0)
            {
                Player.Instance.Die();
                WonLastBattle = false;
            }

        }

    }

    public void Attack()
    {
        if(Target == null || Target.IsDefeated)
        {
            if (CurrentOpponents == null || CurrentOpponents.Count == 0)
            {
                return;
            }
            Target = GetNextTarget();
        }
        RollForPlayerAttackEffects();
        int total = (int)Math.Max(1, Math.Min(Player.Instance.GetTotalDamage().ToRandomDamage() * CalculateTypeBonus(Target) * Extensions.CalculateArmorDamageReduction(Target), Target.CurrentHP));
        Target.CurrentHP -= total;
        
        if(Player.Instance.GetWeapon() == null)
        {
            Player.Instance.GainExperience("Strength", total);
            MessageManager.AddMessage("You punch the " + Target.Name + " for " + total + " damage!");
        }
        else
        {
            if(Player.Instance.GetWeapon().EnabledActions == "Archery" && Player.Instance.Inventory.HasArrows() == false)
            {
                Player.Instance.GainExperience("Strength", total);
                MessageManager.AddMessage("You whack the " + Target.Name + " with your bow for " + total + " damage!");
            }
            else
            {
                Player.Instance.GainExperienceFromWeapon(Player.Instance.GetWeapon(), total);
                MessageManager.AddMessage("You hit the " + Target.Name + " for " + total + " damage!");
                Player.Instance.Inventory.RemoveItems(Player.Instance.Inventory.GetStrongestArrow(), 1);
            }
            
        }
        
        
        if (Target.IsDefeated)
        {
            Target = GetNextTarget();
        }
    }
    public void BeAttacked(Monster opponent)
    {
        int total = (int)Math.Max(1, (opponent.Damage.ToRandomDamage() * Extensions.CalculateArmorDamageReduction()));
        RollForMonsterAttackEffects(opponent);
        Player.Instance.CurrentHP -= total;
        Player.Instance.GainExperience("HP", total);
        MessageManager.AddMessage("The " + opponent.Name + " hit you for " + total + " damage!");
    }
    public double CalculateTypeBonus(Monster m)
    {
        double bonus = 1;
        if(Player.Instance.GetWeapon() != null)
        {
            foreach(string s in Player.Instance.GetWeapon().GetRequiredSkills())
            {
                if (m.Weaknesses.Contains(s))
                {
                    MessageManager.AddMessage("Your opponent seems weak to your weapon!");
                    bonus += 0.4;
                }
                else if (m.Strengths.Contains(s))
                {
                    MessageManager.AddMessage("Your opponent seems to be resistant to your weapon...");
                    bonus -= 0.4;
                }
            }
        }

        return Math.Min(Math.Max(bonus, 0.1), 10);
    }
    public bool AllOpponentsDefeated()
    {
        if(CurrentOpponents == null || CurrentOpponents.Count == 0)
        {
            return true;
        }
        foreach(Monster opponent in CurrentOpponents)
        {
            if(opponent.IsDefeated == false)
            {
                return false;
            }
        }
        return true;
    }
    public void ResetOpponent(Monster monster)
    {
        monster.CurrentHP = monster.HP;
        monster.TicksToNextAttack = monster.AttackSpeed;
        monster.IsDefeated = false;
        monster.CurrentStatusEffects.Clear();
    }
    public void SetBoss(Quepland_2.Bosses.IBoss boss)
    {
        CurrentBoss = boss;
        CurrentBoss.Monsters = CurrentOpponents;
    }
    public void EndBattle()
    {
        BattleHasEnded = true;
        CurrentBoss = null;
        if(Player.Instance.JustDied == false &&
            AutoBattle &&
            CurrentArea != null)
        {
            WaitedAutoBattleGameTick = true;           
        }
    }
    private Monster GetNextTarget()
    {
        if(CurrentOpponents == null || CurrentOpponents.Count == 0)
        {
            return null;
        }
        foreach(Monster m in CurrentOpponents)
        {
            if(m.IsDefeated == false)
            {
                return m;
            }
        }
        return null;
    }

    public Monster GetMonsterByName(string name)
    {
        return Monsters.FirstOrDefault(x => x.Name == name);
    }
    public void RollForMonsterAttackEffects(Monster m)
    {
        foreach(IStatusEffect e in m.StatusEffects)
        {
            double roll = random.NextDouble();
            if(roll <= e.ProcOdds)
            {
                if (e.SelfInflicted)
                {
                    m.AddStatusEffect(e);
                }
                else
                {
                    Player.Instance.AddStatusEffect(e);                  
                }
                MessageManager.AddMessage(e.Message);

            }
        }
    }
    public void RollForPlayerAttackEffects()
    {
        foreach (GameItem item in Player.Instance.GetEquippedItems())
        {
            double roll = random.NextDouble();
            if (item.WeaponInfo != null)
            {
                foreach(IStatusEffect e in item.WeaponInfo.StatusEffects)
                {
                    if (roll <= e.ProcOdds)
                    {
                        if (e.SelfInflicted)
                        {
                            Player.Instance.AddStatusEffect(e);
                        }
                        else
                        {
                            Target.AddStatusEffect(e);

                        }
                        MessageManager.AddMessage(e.Message);
                    }
                }
            }
            if(item.ArmorInfo != null)
            {
                foreach (IStatusEffect e in item.ArmorInfo.StatusEffects)
                {
                    if (roll <= e.ProcOdds)
                    {
                        if (e.SelfInflicted)
                        {
                            Player.Instance.AddStatusEffect(e);
                        }
                        else
                        {
                            Target.AddStatusEffect(e);
                        }                        
                        MessageManager.AddMessage(e.Message);
                    }
                }
            }           
        }
    }
    public IStatusEffect GenerateStatusEffect(StatusEffectData data)
    {
        if(data.Name == "Poison")
        {
            return new PoisonEffect(data);
        }
        else if (data.Name == "Burn")
        {
            return new BurnEffect(data);
        }
        else if(data.Name == "Chicken")
        {
            return new SummonChickenEffect(data);
        }
        else if(data.Name == "Stun")
        {
            return new StunEffect(data);
        }
        else if(data.Name == "Hypnotize")
        {
            return new HypnotizeEffect(data);
        }
        else if(data.Name == "SelfHeal")
        {
            return new SelfHealEffect(data);
        }
        else
        {
            Console.WriteLine("Warning:" + data.Name + " not in list of status effects in Battle Manager.");
        }
        return null;
    }
}

