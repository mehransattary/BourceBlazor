﻿using System.Buffers.Text;

namespace AppShared.Entities;

public class Formol : BourceBase
{
    public int TimeFormol { get; set; }

    public double HajmFormol { get; set; }

    public int MultiStage { get; set; }

    public bool CalculationPrice { get; set; }

}