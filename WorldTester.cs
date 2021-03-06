using System;
using System.Collections.Generic;

public class WorldTester
{
    public List<GameItem> IncludedItems = new List<GameItem>();
    public List<GameItem> MissingItems = new List<GameItem>();
    public void TestWorld()
    {
        foreach (Monster m in BattleManager.Instance.Monsters)
        {
            foreach (Drop d in m.DropTable.Drops)
            {
                GameItem item = ItemManager.Instance.GetItemByName(d.ItemName);
                if (IncludedItems.Contains(item) == false && item != null)
                {
                    IncludedItems.Add(item);
                }
                if(item == null)
                {
                    Console.WriteLine("Monster:" + m.Name + " drops unfound item:" + d.ItemName + " in database.");
                }
            }
        }
        foreach (Area area in AreaManager.Instance.Areas)
        {
            foreach (string a in area.Actions)
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
                        else if(it == null)
                        {
                            Console.WriteLine("Item not found:" + i);
                        }
                    }
                }
            }
            if (area.HuntingTripInfo != null)
            {
                foreach (Drop d in area.HuntingTripInfo.DropTable.Drops)
                {
                    GameItem it = ItemManager.Instance.GetItemByName(d.ItemName);
                    if (it != null && IncludedItems.Contains(it) == false)
                    {
                        IncludedItems.Add(it);
                    }
                }
            }
            foreach (Building b in area.Buildings)
            {
                foreach(Shop s in b.Shops)
                {
                    foreach(GameItem i in s.Items)
                    {
                        if(i != null && IncludedItems.Contains(i) == false)
                        {
                            IncludedItems.Add(i);
                        }
                    }
                }

            }

        }
        List<GameItem> newItems = new List<GameItem>();
        foreach (GameItem i in IncludedItems)
        {
            if (i.TanningInfo != null)
            {
                Console.WriteLine("1st Checking " + i);
                if (IncludedItems.Contains(i) == true && IncludedItems.Contains(i.TanningInfo.TansInto) == false)
                {
                    Console.WriteLine("1st Adding " + i.TanningInfo.TansInto);
                    newItems.Add(i.TanningInfo.TansInto);
                }

            }
        }
        IncludedItems.AddRange(newItems);
        foreach (Recipe r in ItemManager.Instance.Recipes)
        {
            bool IngredientsIncluded = true;
            foreach(Ingredient i in r.Ingredients)
            {
                GameItem item = ItemManager.Instance.GetItemByUniqueID(i.Item.UniqueID);
                if (IncludedItems.Contains(item) == false)
                {
                    if(item == null)
                    {
                        Console.WriteLine("Item was null (not in database):" + i.Item);
                    }
                    else
                    {
                        Console.WriteLine("Included items does not contain " + i.Item);
                    }
                    
                    IngredientsIncluded = false;
                }
            }
            if (IngredientsIncluded && IncludedItems.Contains(r.Output) == false)
            {
                IncludedItems.Add(r.Output);
                if(r.SecondaryOutput != null)
                {
                    IncludedItems.Add(r.SecondaryOutput);
                }
                if(r.TertiaryOutput != null)
                {
                    IncludedItems.Add(r.TertiaryOutput);
                }
            }
            else
            {
                Console.WriteLine("Didn't add " + r.Output.Name + ". Ingredients Included:" + IngredientsIncluded + ". Included items contains output:" + IncludedItems.Contains(r.Output));
            }
        }
        newItems = new List<GameItem>();
        foreach (GameItem i in IncludedItems)
        {
            if (i != null && i.TanningInfo != null)
            {
                if (IncludedItems.Contains(i) == true && IncludedItems.Contains(i.TanningInfo.TansInto) == false)
                {
                    newItems.Add(i.TanningInfo.TansInto);
                }

            }
        }
        IncludedItems.AddRange(newItems);
        foreach (Recipe r in ItemManager.Instance.Recipes)
        {
            bool IngredientsIncluded = true;
            foreach (Ingredient i in r.Ingredients)
            {
                GameItem item = ItemManager.Instance.GetItemByUniqueID(i.Item.UniqueID);
                if (IncludedItems.Contains(item) == false)
                {
                    IngredientsIncluded = false;
                }
            }
            if (IngredientsIncluded && IncludedItems.Contains(r.Output) == false)
            {
                IncludedItems.Add(r.Output);
                if (r.SecondaryOutput != null)
                {
                    IncludedItems.Add(r.SecondaryOutput);
                }
                if (r.TertiaryOutput != null)
                {
                    IncludedItems.Add(r.TertiaryOutput);
                }
            }
        }
        foreach (Recipe r in ItemManager.Instance.SmithingRecipes)
        {
            bool IngredientsIncluded = true;
            foreach (Ingredient i in r.Ingredients)
            {
                GameItem item = ItemManager.Instance.GetItemByUniqueID(i.Item.UniqueID);
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
                GameItem item = ItemManager.Instance.GetItemByUniqueID(i.Item.UniqueID);
                if (IncludedItems.Contains(item) == false)
                {
                    Console.WriteLine("Didn't add " + item + ". It lacked ingredient:" + i.Item);
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
                GameItem item = ItemManager.Instance.GetItemByUniqueID(i.Item.UniqueID);
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
