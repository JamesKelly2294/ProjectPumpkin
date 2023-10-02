using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemDefinition Definition;

    public int Value
    {
        get
        {
            return Definition.Value;
        }
    }

    public string Description
    {
        get
        {
            return Definition.Description;
        }
    }

    public string FlavorText
    {
        get
        {
            return Definition.FlavorText;
        }
    }

    public string Name
    {
        get
        {
            return Definition.Name;
        }
    }

    public Sprite Icon
    {
        get
        {
            return Definition.Icon;
        }
    }

    public Color Color
    {
        get
        {
            return Definition.Color;
        }
    }
}
