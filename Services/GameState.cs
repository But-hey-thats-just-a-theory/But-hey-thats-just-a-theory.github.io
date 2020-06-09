﻿using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Quepland_2.Components;
using Quepland_2.Pages;
using Quepland_2.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;


    public class GameState
    {
    public event EventHandler StateChanged;
    public IJSRuntime JSRuntime;

    public static string Version { get; set; } = "0.0.1";
    public static string Location { get; set; } = "";
    public static bool InitCompleted { get; set; } = false;
    public static bool ShowStartMenu { get; set; } = true;
    private bool stopActions = false;
    private bool stopNoncombatActions = false;
    public bool IsStoppingNextTick;
    private Timer GameTimer { get; set; }
    public int testInt = 0;
    private static Guid _guid;
    public static Guid Guid
    {
        get
        {
            if (_guid == Guid.Empty)
            {
                _guid = Guid.NewGuid();
                return _guid;
            }
            else
            {
                return _guid;
            }
        }
        set
        {
            _guid = value;
        }
    }

    public GameItem NewGatherItem;
    public Recipe NewSmeltingRecipe;
    public Recipe NewSmithingRecipe;

    public GameItem CurrentGatherItem;
    public Recipe CurrentSmeltingRecipe;
    public Recipe CurrentSmithingRecipe;
    
    public GameItem CurrentFood;
    public Recipe CurrentRecipe;
    public Land CurrentLand;
    public ItemViewerComponent itemViewer;
    public SmithyComponent SmithingComponent;
    public NavMenu NavMenu;
    public static int TicksToNextAction;
    public static readonly int GameSpeed = 200;
    public int TicksToNextHeal;
    public int HealingTicks;
    public int CurrentTick;

    public static int GameWindowWidth;
    public int SmithingStage;

    private QuestTester QuestTester = new QuestTester();
    private RecipeTester RecipeTester = new RecipeTester();

    public void Start()
    {
        if(GameTimer != null)
        {
            return;
        }
        //QuestTester.TestQuests();
        //RecipeTester.TestRecipes();
        GameTimer = new Timer(new TimerCallback(_ =>
        {
            if (stopActions)
            {
                ClearActions();
            }
            else if (stopNoncombatActions)
            {
                ClearNonCombatActions();
            }
            if(TicksToNextAction <= 0 && CurrentGatherItem != null)
            {                
                GatherItem();
            }
            else if(TicksToNextAction <= 0 && CurrentRecipe != null)
            {
                CraftItem();
            }
            else if(TicksToNextAction <= 0 && CurrentSmithingRecipe != null && CurrentSmeltingRecipe != null)
            {
                SmithItem();
            }
            else if(BattleManager.Instance.CurrentOpponents != null && BattleManager.Instance.CurrentOpponents.Count > 0)
            {
                BattleManager.Instance.DoBattle();
            }
            if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking)
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
                if(Player.Instance.CurrentFollower.TicksToNextAction <= 0)
                {
                    Player.Instance.CurrentFollower.BankItems();
                }
            }
            if(CurrentFood != null && CurrentTick % CurrentFood.FoodInfo.HealSpeed == 0)
            {
                Player.Instance.CurrentHP += CurrentFood.FoodInfo.HealAmount;
                if(Player.Instance.CurrentHP <= 0)
                {
                    Player.Instance.Die();
                }
                HealingTicks--;
                if(HealingTicks <= 0)
                {
                    if(CurrentFood.FoodInfo.BuffedSkill != null)
                    {
                        Player.Instance.Skills.Find(x => x.Name == CurrentFood.FoodInfo.BuffedSkill).Boost = 0;
                    }
                    CurrentFood = null;
                }
            }
            GetDimensions();
            TicksToNextAction--;
            CurrentTick++;
            StateHasChanged();
        }), null, GameSpeed, GameSpeed);
        StateHasChanged();
    }
    /// <summary>
    /// Pauses actions at the beginning of the next game tick.
    /// </summary>
    public void StopActions()
    {
        stopActions = true;
        IsStoppingNextTick = true;
    }
    public void StopNonCombatActions()
    {
        stopNoncombatActions = true;
        IsStoppingNextTick = true;
    }
    private void ClearActions()
    {
        CurrentGatherItem = null;
        CurrentRecipe = null;
        BattleManager.Instance.CurrentOpponents.Clear();
        CurrentSmithingRecipe = null;
        CurrentSmeltingRecipe = null;
        stopActions = false;
        IsStoppingNextTick = false;
        if(NewGatherItem != null)
        {
            CurrentGatherItem = NewGatherItem;
            NewGatherItem = null;
            Console.WriteLine("Current Gather Item is null:" + (CurrentGatherItem == null));
        }
        if(NewSmithingRecipe != null)
        {
            CurrentSmithingRecipe = NewSmithingRecipe;
            NewSmithingRecipe = null; 
            Console.WriteLine("Current Smithing Item is null:" + (CurrentSmithingRecipe == null));

        }
        if (NewSmeltingRecipe != null)
        {
            CurrentSmeltingRecipe = NewSmeltingRecipe;
            NewSmeltingRecipe = null;
            Console.WriteLine("Current smelting Item is null:" + (CurrentSmeltingRecipe == null));

        }
    }
    private void ClearNonCombatActions()
    {
        CurrentGatherItem = null;
        CurrentRecipe = null;
        CurrentSmithingRecipe = null;
        CurrentSmeltingRecipe = null;
        stopNoncombatActions = false;
        IsStoppingNextTick = false;
    }

    public void Pause()
    {
        GameTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void GatherItem()
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking == false)
        {
            if (Player.Instance.CurrentFollower.Inventory.GetAvailableSpaces() <= 0)
            {
                Player.Instance.CurrentFollower.IsBanking = true;
                Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                PlayerGatherItem();
                MessageManager.AddMessage(Player.Instance.CurrentFollower.AutoCollectMessage.Replace("$", CurrentGatherItem.Name));
            }
            else
            {
                Player.Instance.CurrentFollower.Inventory.AddItem(CurrentGatherItem);
                TicksToNextAction = GetTicksToNextGather();
                Player.Instance.GainExperience(CurrentGatherItem.ExperienceGained);
                MessageManager.AddMessage(CurrentGatherItem.GatherString + " and hand it over to " + Player.Instance.CurrentFollower.Name + ".");
            }
            
        }
        else {
            PlayerGatherItem();
        }
        
        



    }
    private bool PlayerGatherItem()
    {
        if (Player.Instance.Inventory.AddItem(CurrentGatherItem) == false)
        {
            if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking)
            {
                MessageManager.AddMessage("Your inventory is full. You wait for your follower to return from banking.");
            }
            else
            {
                MessageManager.AddMessage("Your inventory is full.");
                CurrentGatherItem = null;
            }       
            return false;
        }
        else
        {
            Player.Instance.GainExperience(CurrentGatherItem.ExperienceGained);
            TicksToNextAction = GetTicksToNextGather();
            MessageManager.AddMessage(CurrentGatherItem.GatherString);
        }
        return true;
    }
    private void CraftItem()
    {
        if (CurrentRecipe == null)
        {
            return;
        }
        else if(BattleManager.Instance.CurrentOpponents.Count > 0)
        {
            MessageManager.AddMessage("You can't make that while fighting!");
            return;
        }
        if (CurrentRecipe.Create(out int created))
        {
            TicksToNextAction = CurrentRecipe.CraftingSpeed;
            MessageManager.AddMessage(CurrentRecipe.Output.GatherString.Replace("$", created.ToString()));
        }
        else
        {
            if(CurrentRecipe.Ingredients.Count == 1 && CurrentRecipe.Ingredients[0].Item == itemViewer.Item.Name)
            {
                itemViewer.ClearItem();
            }
            MessageManager.AddMessage("You have run out of materials.");
            CurrentRecipe = null;
        }

        
    }
    public void Eat(GameItem item)
    {
        if (item.FoodInfo != null)
        {
            CurrentFood = item;
            HealingTicks = CurrentFood.FoodInfo.HealDuration;
            Player.Instance.Inventory.RemoveItems(item, 1);
            if (CurrentFood.FoodInfo.BuffedSkill != null)
            {
                Player.Instance.Skills.Find(x => x.Name == item.FoodInfo.BuffedSkill).Boost = CurrentFood.FoodInfo.BuffAmount;
                MessageManager.AddMessage("You eat a " + CurrentFood + "." + " It somehow makes you feel better at " + CurrentFood.FoodInfo.BuffedSkill + ".");

            }
            else
            {
                MessageManager.AddMessage("You eat a " + CurrentFood + ".");

            }
        }
        else
        {
            Console.WriteLine("Item has no food info:" + item.Name);
        }


    }

    private void SmithItem()
    {
        if(CurrentSmeltingRecipe == null || CurrentSmithingRecipe == null)
        {
            Console.WriteLine("Failed to start smithing, Smelting Item is null:" + (CurrentSmeltingRecipe == null) + " and Smithing Item is null:" + (CurrentSmithingRecipe == null));
            return;
        }
        if(SmithingStage == 0)
        {
            if (Player.Instance.Inventory.RemoveRecipeItems(CurrentSmeltingRecipe))
            {
                MessageManager.AddMessage("You smelt the " + CurrentSmeltingRecipe.GetShortIngredientsString() + " into a " + CurrentSmeltingRecipe.OutputItemName);
                Player.Instance.Inventory.AddItem(CurrentSmeltingRecipe.Output);
                //Player.Instance.GainExperience("Smithing", CurrentSmeltingItem.SmithingInfo.SmeltingExperience);
                TicksToNextAction = CurrentSmeltingRecipe.CraftingSpeed;
                SmithingStage = 1;
            }
            else
            {
                if(SmithingComponent != null)
                {
                    SmithingComponent.UpdateSmeltables();
                }
                MessageManager.AddMessage("You have run out of ores.");
                StopActions();
            }
        }
        else if(SmithingStage == 1)
        {
            if (Player.Instance.Inventory.RemoveRecipeItems(CurrentSmithingRecipe))
            {
                MessageManager.AddMessage("You hammer the " + CurrentSmeltingRecipe.OutputItemName + " into a " + CurrentSmithingRecipe.OutputItemName + " and place it in water to cool.");              
                //Player.Instance.GainExperience("Smithing", CurrentSmeltingItem.SmithingInfo.SmeltingExperience);
                TicksToNextAction = CurrentSmithingRecipe.CraftingSpeed;
                SmithingStage = 2;
            }
            else
            {
                if (SmithingComponent != null)
                {
                    SmithingComponent.UpdateSmeltables();
                }
                StopActions();
            }
        }
        else if(SmithingStage == 2)
        {
            if (Player.Instance.Inventory.AddItem(CurrentSmithingRecipe.Output))
            {
                MessageManager.AddMessage("You withdraw the " + CurrentSmithingRecipe.Output.Name);
                TicksToNextAction = 12;
                SmithingStage = 0;
            }
        }

    }
    public void SetCraftingItem(Recipe recipe)
    {
        CurrentRecipe = recipe;
        TicksToNextAction = recipe.CraftingSpeed;
        UpdateState();
    }
    private int GetTicksToNextGather()
    {
        if(CurrentGatherItem != null)
        {
            int baseValue = CurrentGatherItem.GatherSpeed.ToGaussianRandom();
            baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetGearMultiplier(CurrentGatherItem));
            return baseValue;
        }
        Console.WriteLine("Current Gather Item Was null.");
        return 100000;
       
    }
    public void ShowTooltip(MouseEventArgs args, string tipName, bool alignRight)
    {
        if (alignRight)
        {
            TooltipManager.ShowTip(args, tipName, alignRight);
            UpdateState();
        }
        else
        {
            ShowTooltip(args, tipName);
        }
    }
    public void ShowTooltip(MouseEventArgs args, string tipName, string tipData)
    {
        TooltipManager.ShowTip(args, tipName, tipData);
        UpdateState();
    }
    public void ShowTooltip(MouseEventArgs args, string tipName)
    {
        TooltipManager.ShowTip(args, tipName);
        UpdateState();
    }
    public void ShowTooltip(MouseEventArgs args, Tooltip tip)
    {
        TooltipManager.ShowTip(args, tip);
        UpdateState();
    }
    public async Task GetDimensions()
    {
        GameWindowWidth = await JSRuntime.InvokeAsync<int>("getWidth");
    }
    public void HideTooltip()
    {
        TooltipManager.HideTip();
        UpdateState();
    }
    private void StateHasChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
    public void UpdateState()
    {
        StateHasChanged();
    }
}

