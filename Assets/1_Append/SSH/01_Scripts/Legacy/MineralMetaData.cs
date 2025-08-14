using System.Collections.Generic;
using UnityEngine;

public static class MineralMetaData
{
    static readonly Dictionary<MineralTypeEnum, MineralData> mineralDatas = new()
    {
        { MineralTypeEnum.Stone, new MineralData(0, 0.2f, 0.2f, 0.2f) },
        { MineralTypeEnum.Copper, new MineralData(100, 0.2f, 0.2f, 0.2f) },
        { MineralTypeEnum.Silver, new MineralData(250, 0.2f, 0.2f, 0.2f) },
        { MineralTypeEnum.Gold, new MineralData(500, 0.2f, 0.2f, 0.2f) },
    };

    static public int GetPrice(MineralTypeEnum type)
    {
        return mineralDatas[type].Price;
    }
    static public float GetFriction(MineralTypeEnum type)
    {
        return mineralDatas[type].Friction;
    }
    static public float GetBounciness(MineralTypeEnum type)
    {
        return mineralDatas[type].Bounciness;
    }
    static public float GetStickiness(MineralTypeEnum type)
    {
        return mineralDatas[type].Stickiness;
    }
}

public struct MineralData
{
    public int Price;
    public float Friction;
    public float Bounciness;
    public float Stickiness;

    public MineralData(int price, float friction, float bounciness, float stickiness)
    {
        Price = price;
        Friction = friction;
        Bounciness = bounciness;
        Stickiness = stickiness;
    }
}
public enum MineralTypeEnum
{
    None, Stone, Copper, Silver, Gold
}