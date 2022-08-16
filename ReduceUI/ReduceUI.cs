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
    [BepInPlugin("SmilingLizard.plugins.ReduceUI", "ReduceUI", "2.0.0")]
    public class ReduceUI : BaseUnityPlugin
    {
        private static ReduceUI instance;

        private GameObject tutorial;
        private Dictionary<Elem, List<Behaviour>> elems;
        private Dictionary<Elem, ConfigEntry<bool>> show;
        private ConfigEntry<KeyboardShortcut> tapeKey;

        private enum Elem
        {
            InventoryFrame,
            InventoryNumbers,
            InventoryEmpty,
            Rank,
            Holster,
            TapeFrame,
            TapeIcon,
            TapeCounter,
            TapeQueue,
            SubtitleFrame,
            SubtitleWaveform,
            GunHelp,
            PopupMessages,
            PopupHelp,

            Count
        }

        public void Awake()
        {
            if (instance is null)
            {
                instance = this;

                const bool defaultValue = true;
                const string
                    inv = "Inventory",
                    group = "Bottom Right Group",
                    subs = "Subtitles",
                    tut = "Tutorials"
                ;

                this.show = new Dictionary<Elem, ConfigEntry<bool>>
                {
                    [Elem.InventoryFrame] = this.Config.Bind(
                        inv, 
                        "Frame",
                        defaultValue,
                        "Toggles visibilty of the frame around inventory items in the bottom left."
                    ),
                    [Elem.InventoryNumbers] = this.Config.Bind(
                        inv, 
                        "Numbers",
                        defaultValue,
                        "Toggles visibilty of the numbers in the inventory slots."
                    ),
                    [Elem.InventoryEmpty] = this.Config.Bind(
                        inv, 
                        "Empty Slots",
                        defaultValue,
                        "Toggles visibilty of the \"Empty\" indicator in empty inventory slots."
                    ),
                    [Elem.Rank] = this.Config.Bind(
                        group, 
                        "Rank Indicator",
                        defaultValue,
                        "Toggles visibilty of the rank indicator icon."
                    ),
                    [Elem.Holster] = this.Config.Bind(
                        group, 
                        "Holster Frame",
                        defaultValue,
                        "Toggles visibilty of the frame around the holster slot."
                    ),
                    [Elem.TapeFrame] = this.Config.Bind(
                        group, 
                        "Tape Frame",
                        defaultValue,
                        "Toggles visibilty of the frame around the tape UI group."
                    ),
                    [Elem.TapeIcon] = this.Config.Bind(
                        group, 
                        "Tape Icon",
                        defaultValue,
                        "Toggles visibilty of the tape icon in the tape UI group."
                    ),
                    [Elem.TapeCounter] = this.Config.Bind(
                        group, 
                        "Tape Counter",
                        defaultValue,
                        "Toggles visibilty of the tape counter."
                    ),
                    [Elem.TapeQueue] = this.Config.Bind(
                        group, 
                        "Tape Queue",
                        defaultValue,
                        "Toggles visibilty of the number of queued tapes."
                    ),
                    [Elem.SubtitleFrame] = this.Config.Bind(
                        subs, 
                        "Frame",
                        defaultValue,
                        "Toggles visibilty of the frame around subtitles."
                    ),
                    [Elem.SubtitleWaveform] = this.Config.Bind(
                        subs, 
                        "Waveform Icon",
                        defaultValue,
                        "Toggles the visibility of the Waveform icon when playing tapes."
                    ),
                    [Elem.GunHelp] = this.Config.Bind(
                        tut, 
                        "Gun Help",
                        defaultValue,
                        "Toggles visibility of the gun help menu in the top right."
                    ),
                    [Elem.PopupMessages] = this.Config.Bind(
                        tut, 
                        "Messages",
                        defaultValue,
                        "Toggles visibility of pop-up messages, like \"Collect 5 tapes\" or \"The voice of the Threat is taking control\"."
                    ),
                    [Elem.PopupHelp] = this.Config.Bind(
                        tut, 
                        "In World Help",
                        defaultValue,
                        "Toggles visibility of in-world help messages, like \'Holster your gun to hack\'."
                    )
                };

                this.tapeKey = this.Config.Bind(
                    "KeyBinding",
                    "Show Progress",
                    KeyboardShortcut.Empty,
                    "If the tape counter, tape queue, and/or rank indicator are hidden, hold this key to temporarily show them."
                );

                this.Config.SettingChanged += OnSettingsChanged;

                _ = Harmony.CreateAndPatchAll(typeof(ReduceUI));
            }
        }

        private void OnSettingsChanged(object sender, SettingChangedEventArgs args)
        {
            if (Init())
            {
                Apply();
            }
        }

        private bool OtherKeys()
        {
            foreach (KeyCode k in this.tapeKey.Value.Modifiers)
            {
                if (!Input.GetKey(k))
                {
                    return false;
                }
            }
            return true;
        }

        public void OverrideShowProgress(bool showOrHide)
        {
            // REVIEW: how sane are these first two ifs really?
            if (this.show[Elem.TapeCounter].Value == false && this.elems[Elem.TapeCounter][0] is Behaviour counter)
            {
                counter.enabled = showOrHide;
            }
            if (this.show[Elem.TapeQueue].Value == false && this.elems[Elem.TapeQueue][0] is Behaviour queue)
            {
                queue.enabled = showOrHide;
            }
            if (this.show[Elem.Rank].Value == false)
            {
                int? rank = ReceiverCoreScript.Instance()
                    ?.game_mode
                    ?.GetComponent<RankingProgressionGameMode>()
                    ?.progression_data
                    ?.receiver_rank
                ;
                if (rank.HasValue && this.elems[Elem.Rank].Count > rank)
                {
                    this.elems[Elem.Rank][rank].enabled = showOrHide;
                }
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(this.tapeKey.Value.MainKey) && OtherKeys())
            {
                this.OverrideShowProgress(true);
            }
            else if (Input.GetKeyUp(this.tapeKey.Value.MainKey))
            {
                this.OverrideShowProgress(false);
            }
        }

        private void Apply()
        {
            for (int e = 0; e < (int)Elem.Count; e++)
            {
                foreach (Behaviour b in this.elems[(Elem)e])
                {
                    b.enabled = this.show[(Elem)e].Value;
                }
            }

            this.tutorial.SetActive(this.show[Elem.GunHelp].Value);
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
            {
                this.elems = new Dictionary<Elem, List<Behaviour>>((int)Elem.Count);
            }

            if (!this.elems.ContainsKey(Elem.InventoryFrame))
            {
                this.elems[Elem.InventoryFrame] = new List<Behaviour>(8);
            }
            else
            {
                this.elems[Elem.InventoryFrame].Clear();
            }
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 1").GetComponent<Image>());
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 2").GetComponent<Image>());
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 3").GetComponent<Image>());
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 4").GetComponent<Image>());
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 5").GetComponent<Image>());
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/GUI Line Text/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/GUI Line Text/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[Elem.InventoryFrame].Add(gui.Find("Inventory/GUI Line Text/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(Elem.InventoryNumbers))
            {
                this.elems[Elem.InventoryNumbers] = new List<Behaviour>(5);
            }
            else
            {
                this.elems[Elem.InventoryNumbers].Clear();
            }
            TextMeshProUGUI num1 = gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 1/Number").GetComponent<TextMeshProUGUI>();

            this.elems[Elem.InventoryNumbers].Add(num1);
            this.elems[Elem.InventoryNumbers].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 2/Number").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryNumbers].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 3/Number").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryNumbers].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 4/Number").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryNumbers].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 5/Number").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(Elem.InventoryEmpty))
            {
                this.elems[Elem.InventoryEmpty] = new List<Behaviour>(5);
            }
            else
            {
                this.elems[Elem.InventoryEmpty].Clear();
            }
            this.elems[Elem.InventoryEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 1/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 2/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 3/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 4/Empty").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.InventoryEmpty].Add(gui.Find("Inventory/Inventory Slots/GUI Inventory Slot 5/Empty").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(Elem.Rank))
            {
                this.elems[Elem.Rank] = new List<Behaviour>(6);
            }
            else
            {
                this.elems[Elem.Rank].Clear();
            }
            this.elems[Elem.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Introduction").GetComponent<Image>());
            this.elems[Elem.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Beginner").GetComponent<Image>());
            this.elems[Elem.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Sleeper").GetComponent<Image>());
            this.elems[Elem.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Sleepwalker").GetComponent<Image>());
            this.elems[Elem.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Liminal").GetComponent<Image>());
            this.elems[Elem.Rank].Add(gui.Find("Bottom Right Layout Group/Rank/Awake").GetComponent<Image>());

            if (!this.elems.ContainsKey(Elem.Holster))
            {
                this.elems[Elem.Holster] = new List<Behaviour>(8);
            }
            else
            {
                this.elems[Elem.Holster].Clear();
            }
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Line Bottom Left Vertical").GetComponent<Image>());
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Layout Group/Line Bottom Line left").GetComponent<Image>());
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line Text/Line Bottom Right Cross").GetComponent<Image>());
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line/Line Bottom Left Cross").GetComponent<Image>());
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line/Line Bottom").GetComponent<Image>());
            this.elems[Elem.Holster].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Left layout Group/GUI Line/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(Elem.TapeFrame))
            {
                this.elems[Elem.TapeFrame] = new List<Behaviour>(9);
            }
            else
            {
                this.elems[Elem.TapeFrame].Clear();
            }
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Line Bottom Left Vertical").GetComponent<Image>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Layout Group/Line Bottom Line left").GetComponent<Image>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line Text/Line Bottom Right Cross").GetComponent<Image>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line (1)/Line Bottom").GetComponent<Image>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line/Line Bottom Left Cross").GetComponent<Image>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line/Line Bottom").GetComponent<Image>());
            this.elems[Elem.TapeFrame].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Line/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(Elem.TapeIcon))
            {
                this.elems[Elem.TapeIcon] = new List<Behaviour>(1);
            }
            else
            {
                this.elems[Elem.TapeIcon].Clear();
            }
            this.elems[Elem.TapeIcon].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Tapes/Tape Counter/Tape").GetComponent<Image>());

            if (!this.elems.ContainsKey(Elem.TapeCounter))
            {
                this.elems[Elem.TapeCounter] = new List<Behaviour>(1);
            }
            else
            {
                this.elems[Elem.TapeCounter].Clear();
            }
            this.elems[Elem.TapeCounter].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/GUI Tapes/Tape Counter/Global Counter").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(Elem.TapeQueue))
            {
                this.elems[Elem.TapeQueue] = new List<Behaviour>(1);
            }
            else
            {
                this.elems[Elem.TapeQueue].Clear();
            }
            this.elems[Elem.TapeQueue].Add(gui.Find("Bottom Right Layout Group/Bottom Bottom Right Layout Group/Bottom Bottom Right Right Layout Group/Queued Tape Counter").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(Elem.SubtitleFrame))
            {
                this.elems[Elem.SubtitleFrame] = new List<Behaviour>(8);
            }
            else
            {
                this.elems[Elem.SubtitleFrame].Clear();
            }
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Top line/Line Bottom Left Vertical").GetComponent<Image>());
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Top line/Layout Group/Line Bottom Line left").GetComponent<Image>());
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Top line/Layout Group/Text").GetComponent<TextMeshProUGUI>());
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Top line/Layout Group/Line Bottom Line Right").GetComponent<Image>());
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Top line/Line Bottom Right Cross").GetComponent<Image>());
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Bottom Line/Line Bottom Left Cross").GetComponent<Image>());
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Bottom Line/Line Bottom").GetComponent<Image>());
            this.elems[Elem.SubtitleFrame].Add(gui.Find("subtitle_region/Bottom Line/Line Bottom Right Cross").GetComponent<Image>());

            if (!this.elems.ContainsKey(Elem.SubtitleWaveform))
            {
                this.elems[Elem.SubtitleWaveform] = new List<Behaviour>(1);
            }
            else
            {
                this.elems[Elem.SubtitleWaveform].Clear();
            }
            this.elems[Elem.SubtitleWaveform].Add(gui.Find("subtitle_region/Waveform Visualization Container").GetComponent<Image>());

            if (!this.elems.ContainsKey(Elem.PopupMessages))
            {
                this.elems[Elem.PopupMessages] = new List<Behaviour>(1);
            }
            else
            {
                this.elems[Elem.PopupMessages].Clear();
            }
            this.elems[Elem.PopupMessages].Add(gui.Find("Event Message Text").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(Elem.PopupHelp))
            {
                this.elems[Elem.PopupHelp] = new List<Behaviour>(1);
            }
            else
            {
                this.elems[Elem.PopupHelp].Clear();
            }
            this.elems[Elem.PopupHelp].Add(gui.Find("in_world_tutorial_text").GetComponent<TextMeshProUGUI>());

            if (!this.elems.ContainsKey(Elem.GunHelp))
            {
                this.elems[Elem.GunHelp] = new List<Behaviour>(0);
            }
            this.tutorial = gui.Find("tutorial_text_container").gameObject;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ReceiverCoreScript), "OnLoadedLevel")]
        public static void Attach()
        {
            if (instance.Init())
            {
                instance.Apply();
            }
        }
    }
}