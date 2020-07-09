﻿using System;
using System.Collections.Generic;

public class WorldTester
{
    public List<GameItem> IncludedItems = new List<GameItem>();
    public List<GameItem> MissingItems = new List<GameItem>();
    public void TestWorld()
    {

            foreach(Area area in AreaManager.Instance.Areas)
            {
                foreach(string a in area.Actions)
                {
                    if (a == null || a.Contains(':') == false)
                    {

                    }
                    else
                    {
                        foreach (string i in a.Split(':')[1].Split(','))
                        {
                            GameItem it = ItemManager.Instance.GetItemByName(i);
                            if (it != null && IncludedItems.Contains(it) == false)
                            {
                                IncludedItems.Add(it);
                            }
                            else
                            {
                                Console.WriteLine("Item not found:" + i);
                            }
                        }
                    }
                }
            }
        
        foreach(Recipe r in ItemManager.Instance.Recipes)
        {
            bool IngredientsIncluded = true;
            foreach(Ingredient i in r.Ingredients)
            {
                GameItem item = ItemManager.Instance.GetItemByName(i.Item);
                if (IncludedItems.Contains(item) == false)
                {
                    IngredientsIncluded = false;
                }
            }
            if (IngredientsIncluded && IncludedItems.Contains(r.Output) == false)
            {
                IncludedItems.Add(r.Output);
            }
        }
        foreach (Recipe r in ItemManager.Instance.SmithingRecipes)
        {
            bool IngredientsIncluded = true;
            foreach (Ingredient i in r.Ingredients)
            {
                GameItem item = ItemManager.Instance.GetItemByName(i.Item);
                if (IncludedItems.Contains(item) == false)
                {
                    IngredientsIncluded = false;
                }
            }
            if (IngredientsIncluded && IncludedItems.Contains(r.Output) == false)
            {
                if(r.Output == null)
                {
                    Console.WriteLine("Recipe has null output:" + r.OutputItemName);
                }
                IncludedItems.Add(r.Output);
            }
        }
        //A second time for recipes that require recipe outputs
        foreach (Recipe r in ItemManager.Instance.SmithingRecipes)
        {
            bool IngredientsIncluded = true;
            foreach (Ingredient i in r.Ingredients)
            {
                GameItem item = ItemManager.Instance.GetItemByName(i.Item);
                if (IncludedItems.Contains(item) == false)
                {
                    IngredientsIncluded = false;
                }
            }
            if (IngredientsIncluded && IncludedItems.Contains(r.Output) == false)
            {
                IncludedItems.Add(r.Output);
            }
        }        
        //A third time
        foreach (Recipe r in ItemManager.Instance.SmithingRecipes)
        {
            bool IngredientsIncluded = true;
            foreach (Ingredient i in r.Ingredients)
            {
                GameItem item = ItemManager.Instance.GetItemByName(i.Item);
                if (IncludedItems.Contains(item) == false)
                {
                    IngredientsIncluded = false;
                }
            }
            if (IngredientsIncluded && IncludedItems.Contains(r.Output) == false)
            {
                IncludedItems.Add(r.Output);
            }
        }
        foreach (Monster m in BattleManager.Instance.Monsters)
        {
            foreach(Drop d in m.DropTable.Drops)
            {
                GameItem item = ItemManager.Instance.GetItemByName(d.ItemName);
                if (IncludedItems.Contains(item) == false)
                {
                    IncludedItems.Add(item);
                }
            }
        }
        MissingItems = ItemManager.Instance.Items;
        MissingItems.RemoveAll(x => IncludedItems.Contains(x));
        foreach(GameItem i in IncludedItems)
        {
            if(i == null)
            {
                Console.WriteLine("Null Item found.");
            }
        }
    }
}