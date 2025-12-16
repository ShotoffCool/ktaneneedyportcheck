using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;

public class NeedyPortCheck : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMNeedyModule Needy;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved = true;

    public enum PortType
    {
        DVI = 0x01,
        Parallel = 0x02,
        PS2 = 0x04,
        RJ45 = 0x08,
        Serial = 0x10,
        StereoRCA = 0x20
    };

    public KMSelectable[] buttons;

    Dictionary<Component, PortType> ports;
    List<Renderer> disabledMeshes = new List<Renderer>();

    PortType currentPort;
    Component portComponent;

    static List<Component> occupiedComponents = new List<Component>();

    void Awake()
    { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
        ModuleId = ModuleIdCounter++;
        Needy.OnNeedyActivation += OnNeedyActivation;
        Needy.OnNeedyDeactivation += OnNeedyDeactivation;
        Needy.OnTimerExpired += OnTimerExpired;
        Needy.OnActivate += Activate;

        for (int i = 0; i < buttons.Length; i++)
        {
            int i2 = i;
            buttons[i].OnInteract += delegate ()
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[i2].transform);
                buttons[i2].AddInteractionPunch();
                if (ModuleSolved)
                    return false;
                if ((int)currentPort == 1 << i2)
                    OnNeedyDeactivation();
                else
                    Strike();
                return false;
            };
        }

    }

    void OnDestroy()
    { //Shit you need to do when the bomb ends

    }

    void Activate()
    { //Shit that should happen when the bomb arrives (factory)/Lights turn on

        ports = NeedyPortCheckReflectionTools.GetPortsMapped(this);

        foreach (KeyValuePair<Component, PortType> kvp in ports)
        {
            Debug.LogFormat("[needyPortCheck {0}] Key Value Pair" + kvp.Value, ModuleId);
        }
    }

    protected void OnNeedyActivation()
    { //Shit that happens when a needy turns on.
        ModuleSolved = false;
        List<Component> freeComponents = ports.Keys.Where(x => !occupiedComponents.Contains(x)).ToList();

        if (freeComponents.Count == 0)
        {
            OnNeedyDeactivation();
            return;
        }

        portComponent = freeComponents.PickRandom();
        currentPort = ports[portComponent];

        occupiedComponents.Add(portComponent);
        hidePort(portComponent);
        
        Debug.LogFormat("[needyPortCheck {0}] Hiding " + currentPort, ModuleId);
    }

    protected void OnNeedyDeactivation()
    { //Shit that happens when a needy turns off.
        ModuleSolved = true;

        Needy.OnPass();
        showPort();
        occupiedComponents.Remove(portComponent);
    }

    protected void OnTimerExpired()
    { //Shit that happens when a needy turns off due to running out of time.
        Strike();
        OnNeedyDeactivation();
    }

    void Start()
    { //Shit that you calculate, usually a majority if not all of the module

    }

    void Update()
    { //Shit that happens at any point after initialization

    }

    void Strike()
    {
        Needy.HandleStrike();
    }

    void hidePort(Component portToHide)
    {
        foreach(Renderer mesh in portToHide.GetComponentsInChildren<Renderer>())
        {
            if (!mesh.enabled)
            {
                continue;
            }

            mesh.enabled = false;
            disabledMeshes.Add(mesh);
        }
    }

    void showPort()
    {
        foreach (Renderer mesh in disabledMeshes)
        {
            mesh.enabled = true;
        }
        disabledMeshes.Clear();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    void TwitchHandleForcedSolve()
    { //Void so that autosolvers go to it first instead of potentially striking due to running out of time.
        StartCoroutine(HandleAutosolver());
    }

    IEnumerator HandleAutosolver()
    {
        yield return null;
    }
}