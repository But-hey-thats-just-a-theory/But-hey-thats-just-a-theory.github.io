﻿using System;
using System.Collections.Generic;

public class ArmorInfo
{
    public int Damage { get; set; }
    public int ArmorBonus { get; set; }
    public string StatusEffect { get; set; }
    public int EffectDuration { get; set; }
    public List<Requirement> WearRequirements { get; set; } = new List<Requirement>();

}
