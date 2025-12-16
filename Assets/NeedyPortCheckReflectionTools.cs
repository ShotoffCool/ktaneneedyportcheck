using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class NeedyPortCheckReflectionTools
{

    public static Type FetchPortType()
    {
        return Type.GetType("Port, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    }

    public static IEnumerable<Component> GetAllPorts(Component module)
    {
        Transform transform = module.transform;
        Transform parentTransform;
        KMBomb bomb = transform.GetComponentInParent<KMBomb>();
        if (bomb != null)
            parentTransform = bomb.transform;
        else
        {
            parentTransform = transform;
            while (parentTransform.transform.parent != null && parentTransform.GetComponent("Bomb") == null)
                parentTransform = parentTransform.parent;
        }
        return parentTransform.GetComponentsInChildren(FetchPortType());
    }

    public static NeedyPortCheck.PortType GetPortType(Component port)
    {
        try
        {
            return (NeedyPortCheck.PortType)FetchPortType()
                .GetField("PortType", BindingFlags.Instance | BindingFlags.Public)
                .GetValue(port);
        }
        catch { return 0; }
    }

    public static Dictionary<Component, NeedyPortCheck.PortType> GetPortsMapped(Component module)
    {
        Dictionary<Component, NeedyPortCheck.PortType> portMap = new Dictionary<Component, NeedyPortCheck.PortType>();
        foreach (Component port in GetAllPorts(module))
            portMap.Add(port, GetPortType(port));

        return portMap;
    }
}
