using System.Collections.Generic;

using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using Receiver2;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace ReduceUI
{
    [BepInProcess("Receiver2.exe")]
    [BepInPlugin("SmilingLizard.plugins.ReduceUI", "ReduceUI", "1.0.0")]
    public class ReduceUI : BaseUnityPlugin
    {
        private static ReduceUI instance;

        private GameObject tutorial;
        private Dictionary<E, List<Behaviour>> elems;
        private Dictionary<E, ConfigEntry<bool>> show;
        private ConfigEntry<KeyboardShortcut> tapeKey;

        private enum E
        {
            InvFrame,
            InvNums,
            InvEmpty,
            Rank,
            Holster,
            TapeFrame,
            TapeIcon,
            TapeCounter,
            TapeQueue,
            SubFrame,
            SubWaveform,
            Tut,
            Msg,
            H2H,

            Count
        }

        public void Awake()
        {
            if (instance is null)
            {
                instance = this;

                const bool def = true;
                const string
                    inv = "Inventory",
                    group = "Bottom Right Group",
                    subs = "Subtitles",
                    tut = "Tutorials";

                this.show = new Dictionary<E, ConfigEntry<bool>>
                {
                    [E.InvFrame] = this.Config.Bind(inv, "Frame", def, "Toggles visibilty of the frame around inventory items in the bottom left."),
                    [E.InvNums] = this.Config.Bind(inv, "Numbers", def, "Toggles visibilty of the numbers in the inventory slots."),
                    [E.InvEmpty] = this.Config.Bind(inv, "Empty Slots", def, "Toggles visibilty of the \"Empty\" indicator in empty inventory slots."),
                    [E.Rank] = this.Config.Bind(group, "Rank Indicator", def, "Toggles visibilty of the rank indicator icon."),
                    [E.Holster] = this.Config.Bind(group, "Holster Frame", def, "Toggles visibilty of the frame around the holster slot."),
                    [E.TapeFrame] = this.Config.Bind(group, "Tape Frame", def, "Toggles visibilty of the frame around the tape UI group."),
                    [E.TapeIcon] = this.Config.Bind(group, "Tape icon", def, "Toggles visibilty of the tape icon in the tape UI group."),
                    [E.TapeCounter] = this.Config.Bind(group, "Tape Counter", def, "Toggles visibilty of the tape counter."),
                    [E.TapeQueue] = this.Config.Bind(group, "Tape Queue", def, "Toggles visibilty of the number of queued tapes."),
                    [E.SubFrame] = this.Config.Bind(subs, "Frame", def, "Toggles visibilty of the frame around subtitles."),
                    [E.SubWaveform] = this.Config.Bind(subs, "Waveform Icon", def, "Toggles the visibility of the Waveform icon when playing tapes."),
                    [E.Tut] = this.Config.Bind(tut, "Gun Help", def, "Toggles visibility of the gun help menu in the top right."),
                    [E.Msg] = this.Config.Bind(tut, "Messages", def, "Toggles visibility of pop-up messages, like \"Collect 5 tapes\" or \"The voice of the Threat is taking control\"."),
                    [E.H2H] = this.Config.Bind(tut, "In World Help", def, "Toggles visibility of in-world help messages, like \'Holster your gun to hack\'.")
                };

                this.tapeKey = this.Config.Bind("KeyBinding", "Show Tape Counter", KeyboardShortcut.Empty, "If the tape counter and/or queue are hidden, hold this key to temporarily show them.");

                this.Config.SettingChanged += OnSettingsChanged;

                _ = Harmony.CreateAndPatchAll(typeof(ReduceUI));
            }
        }

        private void OnSettingsChanged(object sender, SettingChangedEventArgs args)
        {
            if (Init())
                Apply();
        }

        private bool OtherKeys()
        {
            foreach (KeyCode k in this.tapeKey.Value.Modifiers)
            {
                if (!Input.GetKey(k))
                    return false;
            }
            return true;
        }

        public void Update()
        {
            if (Input.GetKeyDown(this.tapeKey.Value.MainKey) && OtherKeys())
            {
                if (this.elems[E.TapeCounter][0] is Behaviour counter)
                {
                    counter.enabled = true;
                }
                if (this.elems[E.TapeQueue][0] is Behaviour queue)
                {
                    queue.enabled = true;
                }
            }
            else if (Input.GetKeyUp(this.tapeKey.Value.MainKey))
            {
                if (!this.show[E.TapeCounter].Value && this.elems[E.TapeCounter][0] is Behaviour counter)
                {
                    counter.enabled = false;
                }
                if (!this.show[E.TapeQueue].Value && this.elems[E.TapeQueue][0] is Behaviour queue)
                {
                    queue.enabled = false;
                }
            }
        }

        private void Apply()
        {
            for (int e = 0; e < (int)E.Count; e++)
            {
                foreach (Behaviour b in this.elems[(E)e])
                {
                    b.enabled = this.show[(E)e].Value;
                }
            }

            this.tutorial.SetActive(this.show[E.Tut].Value);
        }

        private bool Init()
        {
            Transform gui = GameObject.Find("/ReceiverCore/PlayerGUI/Canvas/gameplay").transform;

            if (gui is null)
            {
                this.Logger.LogError("Tried to init but couldn't find gui; aborting");
                return false;
            }

            if (this.elems is null)
                this.elems = new Dictionary<E, List<Behaviour>>((int)E.Count);

            if (!this.elems.ContainsKey(E.InvFrame))
                this.elems[E.InvFrame] = new List<Behaviour>(8);
            else
                this.elems[E.InvFrame].Clear();
            this.elems[E.InvFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 1").GetComponent<Image>());
            this.elems[E.InvFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 2").GetComponent<Image>());
            this.elems[E.InvFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 3").GetComponent<Image>());
            this.elems[E.InvFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 4").GetComponent<Image>());
            this.elems[E.InvFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 5").GetComponent<Image>());
            this.elems[E.InvFrame].Add(gui.Find("Inventory/GUI Line Text/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvFrame].Add(gui.Find("Inventory/GUI Line Text/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[E.InvFrame].Add(gui.Find("Inventory/GUI Line Text/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(E.InvNums))
                this.elems[E.InvNums] = new List<Behaviour>(5);
            else
                this.elems[E.InvNums].Clear();
            TextMeshProUGUI num1 = gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 1/Number").GetComponent<TextMeshProUGUI>();

            this.elems[E.InvNums].Add(num1);
            this.elems[E.InvNums].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 2/Number").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvNums].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 3/Number").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvNums].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 4/Number").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvNums].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 5/Number").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(E.InvEmpty))
                this.elems[E.InvEmpty] = new List<Behaviour>(5);
            else
                this.elems[E.InvEmpty].Clear();
            this.elems[E.InvEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 1/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 2/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 3/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 4/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[E.InvEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 5/Empty").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(E.Rank))
                this.elems[E.Rank] = new List<Behaviour>(5);
            else
                this.elems[E.Rank].Clear();
            this.elems[E.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Beginner").GetComponent<Image>());
            this.elems[E.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Sleeper").GetComponent<Image>());
            this.elems[E.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Sleepwalker").GetComponent<Image>());
            this.elems[E.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Liminal").GetComponent<Image>());
            this.elems[E.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Awake").GetComponent<Image>());

            if (!this.elems.ContainsKey(E.Holster))
                this.elems[E.Holster] = new List<Behaviour>(8);
            else
                this.elems[E.Holster].Clear();
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Line Bottom Left Vertical").GetComponent<Image>());
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Layout Group/Line Bottom Line left").GetComponent<Image>());
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Line Bottom Right Cross").GetComponent<Image>());
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line/Line Bottom Left Cross").GetComponent<Image>());
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line/Line Bottom").GetComponent<Image>());
            this.elems[E.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(E.TapeFrame))
                this.elems[E.TapeFrame] = new List<Behaviour>(9);
            else
                this.elems[E.TapeFrame].Clear();
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Line Bottom Left Vertical").GetComponent<Image>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Layout Group/Line Bottom Line left").GetComponent<Image>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Line Bottom Right Cross").GetComponent<Image>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line (1)/Line Bottom").GetComponent<Image>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line/Line Bottom Left Cross").GetComponent<Image>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line/Line Bottom").GetComponent<Image>());
            this.elems[E.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(E.TapeIcon))
                this.elems[E.TapeIcon] = new List<Behaviour>(1);
            else
                this.elems[E.TapeIcon].Clear();
            this.elems[E.TapeIcon].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Tapes/Tape Counter/Tape").GetComponent<Image>());

            if (!this.elems.ContainsKey(E.TapeCounter))
                this.elems[E.TapeCounter] = new List<Behaviour>(1);
            else
                this.elems[E.TapeCounter].Clear();
            this.elems[E.TapeCounter].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Tapes/Tape Counter/Global Counter").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(E.TapeQueue))
                this.elems[E.TapeQueue] = new List<Behaviour>(1);
            else
                this.elems[E.TapeQueue].Clear();
            this.elems[E.TapeQueue].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/Queued Tape Counter").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(E.SubFrame))
                this.elems[E.SubFrame] = new List<Behaviour>(8);
            else
                this.elems[E.SubFrame].Clear();
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Top line/Line Bottom Left Vertical").GetComponent<Image>());
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Top line/Layout Group/Line Bottom Line left").GetComponent<Image>());
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Top line/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Top line/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Top line/Line Bottom Right Cross").GetComponent<Image>());
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Bottom Line/Line Bottom Left Cross").GetComponent<Image>());
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Bottom Line/Line Bottom").GetComponent<Image>());
            this.elems[E.SubFrame].Add(gui.Find("subtitle_region/Bottom Line/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(E.SubWaveform))
                this.elems[E.SubWaveform] = new List<Behaviour>(1);
            else
                this.elems[E.SubWaveform].Clear();
            this.elems[E.SubWaveform].Add(gui.Find("subtitle_region/Waveform Visualization Container").GetComponent<Image>());

            if (!this.elems.ContainsKey(E.Msg))
                this.elems[E.Msg] = new List<Behaviour>(1);
            else
                this.elems[E.Msg].Clear();
            this.elems[E.Msg].Add(gui.Find("Event Message Text").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(E.H2H))
                this.elems[E.H2H] = new List<Behaviour>(1);
            else
                this.elems[E.H2H].Clear();
            this.elems[E.H2H].Add(gui.Find("in_world_tutorial_text").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(E.Tut))
                this.elems[E.Tut] = new List<Behaviour>(0);
            this.tutorial = gui.Find("tutorial_text_container").gameObject;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ReceiverCoreScript), "OnLoadedLevel")]
        public static void Attach()
        {
            if (instance.Init())
                instance.Apply();
        }
    }
}