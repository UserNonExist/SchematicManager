﻿using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp330;
using MapEditorReborn.API.Features;
using MapEditorReborn.API.Features.Objects;
using Mirror;
using UnityEngine;

namespace SchematicManager.Controllers;

public class VendingMachineController : MonoBehaviour
{
    public enum VendingMachineType
    {
        EntranceZone,
        LightContainmentZone,
    }
    
    private Dictionary<int, bool[]> _digitsMap = new Dictionary<int, bool[]>
    {
        { 0, new[]{true, true, true, false, true, true, true} },
        { 1, new[]{false, true, false, false, true, false, false} },
        { 2, new[]{true, false, true, true, true, false, true} },
        { 3, new[]{true, true, false, true, true, false, true} },
        { 4, new[]{false, true, false, true, true, true, false} },
        { 5, new[]{true, true, false, true, false, true, true} },
        { 6, new[]{true, true, true, true, false, true, true} },
        { 7, new[]{false, true, false, false, true, false, true} },
        { 8, new[]{true, true, true, true, true, true, true} },
        { 9, new[]{true, true, false, true, true, true, true} }
    };

    private class DigitDisplay
    {
        public List<PrimitiveObject> Segments = new List<PrimitiveObject>();
    }
    
    private class VendingMachineItem
    {
        public int RequiredCoins { get; set; }
        public ItemType ItemType { get; set; }
        public string ItemName { get; set; }
    }
    
    private readonly Vector3 _basePosition = new Vector3(-1.058f * 0.73f, 1.4145f * 0.73f, 0.503f * 0.73f);
    private Vector3 _refundPosition = new Vector3(-1.14f * 0.73f, 1.8215f * 0.73f, 0.503f * 0.73f);
    private float step = 0.125f;
    private int _coins = 0;
    private string _display = "0000";
    private float _timer;
    private List<PrimitiveObject> _digits = new List<PrimitiveObject>();
    private List<DigitDisplay> _digitDisplays = new List<DigitDisplay>();
    private List<ItemPickupBase> DeletedItems = new List<ItemPickupBase>();
    private List<LightSourceObject> _lightSources = new List<LightSourceObject>();
    private static System.Random _random;
    public static Config Config = new Config();
    

    private List<VendingMachineItem> _vendingMachineItems = new List<VendingMachineItem>()
    {
        new VendingMachineItem { RequiredCoins = 1, ItemType = ItemType.Painkillers },
        new VendingMachineItem { RequiredCoins = 1, ItemType = ItemType.None, ItemName = "M&Ms" },
        new VendingMachineItem { RequiredCoins = 1, ItemType = ItemType.None, ItemName = "Layz"},
        new VendingMachineItem { RequiredCoins = 1, ItemType = ItemType.None, ItemName = "Conk"},
        new VendingMachineItem { RequiredCoins = 2, ItemType = ItemType.Medkit },
        new VendingMachineItem { RequiredCoins = 1, ItemType = ItemType.None, ItemName = "Bepis"},
        new VendingMachineItem { RequiredCoins = 2, ItemType = ItemType.Ammo9x19 },
        new VendingMachineItem { RequiredCoins = 4, ItemType = ItemType.None, ItemName = "MysteryBox"},
        new VendingMachineItem { RequiredCoins = 2, ItemType = ItemType.Ammo556x45 },
    };

    private List<ItemType> MysteryBoxLootEZ = new List<ItemType>()
    {
        ItemType.Adrenaline,
        ItemType.GrenadeFlash,
        ItemType.SCP2176,
        ItemType.SCP500,

        ItemType.KeycardMTFOperative,
        ItemType.ArmorHeavy,
    };
    
    private List<ItemType> MysteryBoxLootLCZ = new List<ItemType>()
    {
        ItemType.Adrenaline,
        ItemType.GrenadeFlash,
        ItemType.SCP2176,
        ItemType.SCP500,

        ItemType.GunRevolver,
        ItemType.KeycardZoneManager,
        ItemType.KeycardResearchCoordinator,
    };
    
    public VendingMachineType vendingMachineType { get; set; }
    public List<Pickup> Buttons { get; set; }
    public static List<VendingMachineController> VendingMachines = new List<VendingMachineController>();

    public static void SpawnVendingMachines()
    {
        List<Room> LocationPicked = new List<Room>();
        _random = new System.Random();
        Dictionary<Room, List<Utils.Utils.PosRot>> LocationLight = new Dictionary<Room, List<Utils.Utils.PosRot>>();
        Dictionary<Room, List<Utils.Utils.PosRot>> LocationEntrance = new Dictionary<Room, List<Utils.Utils.PosRot>>();


        foreach (var room in Room.List)
        {
            if (Config.LCZVendingLocations.ContainsKey(room.Type))
            {
                if (!LocationLight.ContainsKey(room))
                    LocationLight.Add(room, Config.LCZVendingLocations[room.Type]);
            }
            else if (Config.EZVendingLocations.ContainsKey(room.Type))
            {
                if (!LocationEntrance.ContainsKey(room))
                    LocationEntrance.Add(room, Config.EZVendingLocations[room.Type]);
            }
        }

        while (VendingMachines.Count != 2)
        {
            var value = LocationEntrance.ElementAt(_random.Next(0, LocationEntrance.Count));
            if (LocationPicked.Contains(value.Key)) continue;
            
            LocationEntrance.Remove(value.Key);
            LocationPicked.Add(value.Key);

            var posRot = value.Value.ElementAt(_random.Next(value.Value.Count));

            var vendingMachine = MapUtils.GetSchematicDataByName("Vending_Machine");
            var vendingMachineObj = ObjectSpawner.SpawnSchematic("Vending_Machine",
                value.Key.Transform.TransformPoint(posRot.Pos),
                value.Key.Transform.rotation * Quaternion.Euler(posRot.Rot), Vector3.one, vendingMachine);

            vendingMachineObj.gameObject.AddComponent<VendingMachineController>()
                .Init(vendingMachineObj);
            VendingMachines.Add(vendingMachineObj.GetComponent<VendingMachineController>());
            Log.Info($"Spawned Vending Machine in {value.Key.Type}");
        }

        while (VendingMachines.Count != 4)
        {
            var value = LocationLight.ElementAt(_random.Next(0, LocationLight.Count - 1));
            if (LocationPicked.Contains(value.Key)) continue;
            
            LocationLight.Remove(value.Key);
            LocationPicked.Add(value.Key);
            
            var posRot = value.Value.ElementAt(_random.Next(value.Value.Count));
            
            var vendingMachine = MapUtils.GetSchematicDataByName("Vending_Machine");
            var vendingMachineObj = ObjectSpawner.SpawnSchematic("Vending_Machine",
                value.Key.Transform.TransformPoint(posRot.Pos),
                value.Key.Transform.rotation * Quaternion.Euler(posRot.Rot), Vector3.one, vendingMachine);
            
            vendingMachineObj.gameObject.AddComponent<VendingMachineController>()
                .Init(vendingMachineObj);
            VendingMachines.Add(vendingMachineObj.GetComponent<VendingMachineController>());
            Log.Info($"Spawned Vending Machine in {value.Key.Type}");
        }
    }
    
    public void Init(SchematicObject obj)
        {
            //Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            Exiled.Events.Handlers.Player.SearchingPickup += OnSearchingPickup;
            
            Buttons = new List<Pickup>();
            vendingMachineType = Room.FindParentRoom(gameObject).Zone == ZoneType.Entrance
                ? VendingMachineType.EntranceZone
                : VendingMachineType.LightContainmentZone;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var Button = Item.Create(ItemType.ArmorLight).CreatePickup(
                        gameObject.transform.TransformPoint(_basePosition + new Vector3(step * -j  * 0.73f, step * i  * 0.73f, 0)));
                    Button.Scale = Vector3.one * 0.1f;
                    Button.Rotation = gameObject.transform.rotation * Quaternion.Euler(0,-90,0);

                    var rigidBody1 = Button.Base.gameObject.GetComponent<Rigidbody>();
                    var collider1 = Button.Base.gameObject.GetComponents<Collider>();

                    foreach (var thing in collider1)
                    {
                        thing.enabled = false;
                    }

                    if (rigidBody1 != null)
                    {
                        rigidBody1.useGravity = false;
                        rigidBody1.angularDrag = 10000000000;
                        rigidBody1.drag = 10000000000;
                        rigidBody1.detectCollisions = false;
                        rigidBody1.freezeRotation = true;
                        rigidBody1.isKinematic = true;
                        rigidBody1.mass = 10000000000;
                    }

                    Buttons.Add(Button);
                }
            }

            var RefundButton = Item.Create(ItemType.ArmorLight).CreatePickup(gameObject.transform.TransformPoint(_refundPosition));
            RefundButton.Scale = new Vector3(0.1f, 0.1f, 0.2f);
            RefundButton.Rotation = gameObject.transform.rotation * Quaternion.Euler(0,-90,0);

            var rigidBody = RefundButton.Base.gameObject.GetComponent<Rigidbody>();
            var collider = RefundButton.Base.gameObject.GetComponents<Collider>();

            foreach (var thing in collider)
            {
                thing.enabled = false;
            }

            if (rigidBody != null)
            {
                rigidBody.useGravity = false;
                rigidBody.angularDrag = 10000000000;
                rigidBody.drag = 10000000000;
                rigidBody.detectCollisions = false;
                rigidBody.freezeRotation = true;
                rigidBody.isKinematic = true;
                rigidBody.mass = 10000000000;
            }

            Buttons.Add(RefundButton);

            foreach (var block in obj.AttachedBlocks)
            {
                if (block.name.Contains("DIGIT") || block.name.Contains("DECIMAL_POINT"))
                {
                    if(!block.name.Contains("DECIMAL_POINT"))
                        _digits.Add(block.GetComponent<PrimitiveObject>());
                    else
                    {
                        block.GetComponent<PrimitiveObject>().Primitive.Base.NetworkMaterialColor = Color.red;
                    }
                }

                if (block.name.Contains("LightSource"))
                {
                    block.gameObject.AddComponent<Components.LightBlink>();
                    
                    _lightSources.Add(block.GetComponent<LightSourceObject>());
                    
                }
            }

            for (int i = 0; i < 4; i++)
                _digitDisplays.Add(new DigitDisplay());

            _digits = _digits.OrderBy(x=>x.name).ToList();

            int counter = 0;
            int displayCounter = 0;
            
            for (int i = 0; i < 28; i++)
            {
                if(counter == 7)
                {
                    counter = 0;
                    displayCounter++;
                }
                
                _digitDisplays[displayCounter].Segments.Add(_digits[i]);
                counter++;
            }
            
            foreach(var display in _digitDisplays)
            {
                for (int i = 0; i < 7; i++)
                {
                    display.Segments[i].Primitive.Base.NetworkMaterialColor = _digitsMap[0][i] ? Color.red : Color.black;
                }
            }

            _digitDisplays.Reverse();

            for (int i = 0; i < _random.Next(3); i++)
            {
                Item.Create(ItemType.Coin).CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
            }
        }

    private void OnDestroy()
    {
        //Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
        Exiled.Events.Handlers.Player.SearchingPickup -= OnSearchingPickup;
        foreach (var button in Buttons)
        {
            button.Destroy();
        }
    }

    public static void DestroyVendingMachines()
    {
        foreach (var vending in VendingMachines)
        {
            NetworkServer.Destroy(vending.gameObject);
        }
        
        VendingMachines.Clear();
    }

    private void OnSearchingPickup(SearchingPickupEventArgs ev)
    {
        if (Buttons.Contains(ev.Pickup))
        {
            ev.IsAllowed = false;
            //ev.Pickup.IsLocked = false;
            //ev.Pickup.InUse = false;

            if (ev.Pickup == Buttons.Last())
            {
                if (ev.Player.CurrentItem.Type != ItemType.Coin)
                {
                    ev.Player.ShowHint("กรุณาถือเหรืยญเพื่อหยอดเหรียญ", 7f);
                    return;
                }

                _display = $"{int.Parse(_display) + 25:D4}";
                UpdateDisplay();
                Log.Info("Coin added, current display: " + _display);
                
                ev.Player.CurrentItem.Destroy();

                _coins++;

                return;
            }
            
            var index = Buttons.IndexOf(ev.Pickup);

            if (_coins >= _vendingMachineItems[index].RequiredCoins)
            {
                if (_coins > _vendingMachineItems[index].RequiredCoins)
                {
                    _coins -= _vendingMachineItems[index].RequiredCoins;

                    _display = $"{_coins * 25:D4}";
                    UpdateDisplay();
                }
                else
                {
                    _coins = 0;
                    _display = "0000";
                    UpdateDisplay();
                }

                if (_vendingMachineItems[index].ItemType == ItemType.None)
                {
                    switch (_vendingMachineItems[index].ItemName)
                    {
                        case "M&Ms":
                            Scp330Bag.AddSimpleRegeneration(ev.Player.ReferenceHub, 5, 10);
                            ev.Player.ShowHint("คุณได้รับ M&Ms จากตู้นี้ (ฟื้น HP ชั่วคราว)");
                            break;
                        case "Conk":
                            ev.Player.EnableEffect(EffectType.MovementBoost, 15f, true);
                            ev.Player.ShowHint("คุณได้รับ Conk จากตู้นี้ (เพิ่มความเร็วชั่วคราว)");
                            break;
                        case "Bepis":
                            ev.Player.EnableEffect(EffectType.MovementBoost, 15f, true);
                            ev.Player.ShowHint("คุณได้รับ Bepis จากตู้นี้ (เพิ่มความเร็วชั่วคราว)");
                            break;
                        case "Layz":
                            ev.Player.EnableEffect(EffectType.DamageReduction, 30f, true);
                            ev.Player.ShowHint("คุณได้รับ Layz จากตู้นี้ (ลดความเสียหายชั่วคราว)");
                            break;
                        case "MysteryBox":
                            if (vendingMachineType == VendingMachineType.EntranceZone)
                            {
                                Item.Create(MysteryBoxLootEZ.ElementAt(_random.Next(MysteryBoxLootEZ.Count))).CreatePickup(
                                    gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                            }
                            else
                            {
                                var item = MysteryBoxLootLCZ.ElementAt(_random.Next(MysteryBoxLootLCZ.Count));

                                if (item.IsWeapon())
                                {
                                    var f = (Firearm) Firearm.Create(item);
                                    f.Ammo = 0;
                                    f.CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                                }
                                else
                                {
                                    Item.Create(item).CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                                }
                            }

                            break;
                    }
                }
                else
                {
                    Item.Create(_vendingMachineItems[index].ItemType).CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                    if (_vendingMachineItems[index].ItemType.IsAmmo())
                    {
                        Item.Create(_vendingMachineItems[index].ItemType).CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                    }
                }
                
                ev.Player.ShowHint("ขอบคุณสำหรับการใช้บริการ!", 5f);
            }
            else
            {
                ev.Player.ShowHint("คุณขาดอีก " + (_vendingMachineItems[index].RequiredCoins - _coins) + " เหรียญเพื่อที่จะซื้อสิ่งนี้", 5f);
            }
        }
    }

    private void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (Buttons.Contains(ev.Pickup))
        {
            ev.IsAllowed = false;
            //ev.Pickup.IsLocked = false;
            //ev.Pickup.InUse = false;

            if (ev.Pickup == Buttons.Last())
            {
                if (ev.Player.CurrentItem.Type != ItemType.Coin)
                {
                    ev.Player.ShowHint("กรุณาถือเหรืยญเพื่อหยอดเหรียญ", 4f);
                    return;
                }

                _display = $"{int.Parse(_display) + 25:D4}";
                UpdateDisplay();
                Log.Info("Coin added, current display: " + _display);
                
                ev.Player.CurrentItem.Destroy();

                _coins++;

                return;
            }
            
            var index = Buttons.IndexOf(ev.Pickup);

            if (_coins >= _vendingMachineItems[index].RequiredCoins)
            {
                if (_coins > _vendingMachineItems[index].RequiredCoins)
                {
                    _coins -= _vendingMachineItems[index].RequiredCoins;

                    _display = $"{_coins * 25:D4}";
                    UpdateDisplay();
                }
                else
                {
                    _coins = 0;
                    _display = "0000";
                    UpdateDisplay();
                }

                if (_vendingMachineItems[index].ItemType == ItemType.None)
                {
                    switch (_vendingMachineItems[index].ItemName)
                    {
                        case "M&Ms":
                            Scp330Bag.AddSimpleRegeneration(ev.Player.ReferenceHub, 5, 10);
                            ev.Player.ShowHint("คุณได้รับ M&Ms จากตู้นี้ (ฟื้น HP ชั่วคราว)");
                            break;
                        case "Conk":
                            ev.Player.EnableEffect(EffectType.MovementBoost, 15f, true);
                            ev.Player.ShowHint("คุณได้รับ Conk จากตู้นี้ (เพิ่มความเร็วชั่วคราว)");
                            break;
                        case "Bepis":
                            ev.Player.EnableEffect(EffectType.MovementBoost, 15f, true);
                            ev.Player.ShowHint("คุณได้รับ Bepis จากตู้นี้ (เพิ่มความเร็วชั่วคราว)");
                            break;
                        case "Layz":
                            ev.Player.EnableEffect(EffectType.DamageReduction, 30f, true);
                            ev.Player.ShowHint("คุณได้รับ Layz จากตู้นี้ (ลดความเสียหายชั่วคราว)");
                            break;
                        case "MysteryBox":
                            if (vendingMachineType == VendingMachineType.EntranceZone)
                            {
                                Item.Create(MysteryBoxLootEZ.ElementAt(_random.Next(MysteryBoxLootEZ.Count))).CreatePickup(
                                    gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                            }
                            else
                            {
                                var item = MysteryBoxLootLCZ.ElementAt(_random.Next(MysteryBoxLootLCZ.Count));

                                if (item.IsWeapon())
                                {
                                    var f = (Firearm) Firearm.Create(item);
                                    f.Ammo = 0;
                                    f.CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                                }
                                else
                                {
                                    Item.Create(item).CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                                }
                            }

                            break;
                    }
                }
                else
                {
                    Item.Create(_vendingMachineItems[index].ItemType).CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                    if (_vendingMachineItems[index].ItemType.IsAmmo())
                    {
                        Item.Create(_vendingMachineItems[index].ItemType).CreatePickup(gameObject.transform.TransformPoint(0, 0.3f * 0.73f, 0.5f * 0.73f));
                    }
                }
                
                ev.Player.ShowHint( "ขอบคุณสำหรับการใช้บริการ!", 5f);
            }
            else
            {
                ev.Player.ShowHint( "คุณขาดอีก " + (_vendingMachineItems[index].RequiredCoins - _coins) + " เหรียญเพื่อที่จะซื้อสิ่งนี้", 5f);
            }
        }
    }

    /*private void FixedUpdate()
    {
        if (_timer < 0.5)
        {
            _timer += Time.deltaTime;
            return;
        }
        
        //UpdateVendingMachine();
    }

    private void UpdateVendingMachine()
    {
        var colliders = Physics.OverlapSphere(gameObject.transform.TransformPoint(_basePosition), 2.2f,
            LayerMask.GetMask("Pickup"));
        foreach (var col in colliders)
        {
            if (col.transform.root.gameObject.TryGetComponent(out ItemPickupBase pickup))
            {
                if (DeletedItems.Contains(pickup))
                    continue;

                if (pickup.NetworkInfo.ItemId == ItemType.Coin)
                {
                    _display = $"{int.Parse(_display) + 25:D4}";
                    UpdateDisplay();
                    Log.Info("Coin added, current display: " + _display);

                    _coins++;
                    pickup.DestroySelf();
                }
            }
        }
        
        DeletedItems.Clear();
        _timer = 0;
    }*/

    private void UpdateDisplay()
    {
        for (int i = 0; i < 4; i++)
        {
            for (var j = 0; j < _digitDisplays[i].Segments.Count; j++)
            {
                var display = _digitDisplays[i].Segments[j];
                display.Primitive.Base.NetworkMaterialColor =
                    _digitsMap[int.Parse(_display[i].ToString())][j] ? Color.red : Color.black;
            }
        }
    }
}