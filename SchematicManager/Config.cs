using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using UnityEngine;


namespace SchematicManager;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;
    
    public Dictionary<RoomType, List<Vector3>> EZRecyclingBinLocations = new Dictionary<RoomType, List<Vector3>>()
    {
        { RoomType.EzUpstairsPcs, new List<Vector3>{new Vector3(-6.8f * 0.73f, 0, 9.4f * 0.73f) }},
        { RoomType.EzCurve, new List<Vector3>{new Vector3(-3.2f * 0.73f, 0, 5 * 0.73f), new Vector3(4.8f * 0.73f, 0, -3.0f * 0.73f)}},
        { RoomType.EzConference, new List<Vector3>{new Vector3(-3.6f * 0.73f, 0, 2.0f * 0.73f)} },
    };

    public Dictionary<RoomType, List<Vector3>> GateRecyclingBinLocations =
        new Dictionary<RoomType, List<Vector3>>()
        {
            {RoomType.EzGateB, new List<Vector3> {new Vector3(5.1f * 0.73f, 0, 1.2f * 0.73f), new Vector3(5.1f * 0.73f, 0, -9.9f * 0.73f)}},
            {RoomType.EzGateA, new List<Vector3> {new Vector3(5.7f * 0.73f, 0, -9.8f * 0.73f)}},
        };
		
    public Dictionary<RoomType, List<Vector3>> HCZRecyclingBinLocations = new Dictionary<RoomType, List<Vector3>>()
    {
        { RoomType.HczCurve, new List<Vector3>{new Vector3(-2.9f * 0.73f, 0, 3.0f * 0.73f) }},
        { RoomType.HczCrossing, new List<Vector3>{new Vector3(-2.7f * 0.73f, 0, 2.7f * 0.73f), new Vector3(2.7f * 0.73f, 0, 2.7f * 0.73f), new Vector3(2.7f * 0.73f, 0, -2.7f * 0.73f), new Vector3(-2.7f * 0.73f, 0, -2.7f * 0.73f)}},
    };
		
    public Dictionary<RoomType, List<Vector3>> LCZRecyclingBinLocations = new Dictionary<RoomType, List<Vector3>>()
    {
        { RoomType.Lcz914, new List<Vector3>{new Vector3(-2.2f * 0.73f, 0 * 0.73f, -9.5f * 0.73f), new Vector3(-9.3f * 0.73f, 0 * 0.73f, -3.6f * 0.73f) }},
        { RoomType.LczAirlock, new List<Vector3>{new Vector3(0f * 0.73f, 0, 1.6f * 0.73f)}},
        { RoomType.LczCafe, new List<Vector3>{new Vector3(-6.7f * 0.73f, 0.0f, 6.1f * 0.73f)}},
        { RoomType.Lcz330, new List<Vector3>{new Vector3(-2.9f * 0.73f, 0.0f, 0.0f * 0.73f)}},
    };
    
    public Dictionary<RoomType, List<Utils.Utils.PosRot>> LCZVendingLocations = new Dictionary<RoomType, List<Utils.Utils.PosRot>>
    {
        {RoomType.LczCafe, new List<Utils.Utils.PosRot>{new Utils.Utils.PosRot(new Vector3(-7.0f * 0.73f, 0.0f, -6.7f * 0.73f), new Vector3(0, 0, 0))}},
        {RoomType.LczToilets, new List<Utils.Utils.PosRot>{new Utils.Utils.PosRot(new Vector3(5.7f, 0.0f, -1.5f), new Vector3(0, 0, 0))}},
        {RoomType.Lcz330, new List<Utils.Utils.PosRot>{new Utils.Utils.PosRot(new Vector3(-7.8f, 0.0f, 4.3f), new Vector3(0, 180, 0))}},
    };
		
    public Dictionary<RoomType, List<Utils.Utils.PosRot>> EZVendingLocations = new Dictionary<RoomType, List<Utils.Utils.PosRot>>
    {
        {RoomType.EzPcs, new List<Utils.Utils.PosRot>{new Utils.Utils.PosRot(new Vector3(3.7f * 0.73f, 0.0f, 9.6f * 0.73f), new Vector3(0, 180, 0))}},
        {RoomType.EzUpstairsPcs, new List<Utils.Utils.PosRot>{new Utils.Utils.PosRot(new Vector3(10.1f * 0.73f, 3.9f, 1.3f) * 0.73f, new Vector3(0, 270, 0)),
            new Utils.Utils.PosRot(new Vector3(-3.5f * 0.73f, 0, 9.9f * 0.73f), new Vector3(0, 180, 0))}},
        {RoomType.EzStraight, new List<Utils.Utils.PosRot>{new Utils.Utils.PosRot(new Vector3(-1.8f * 0.73f, 0.0f, 0.8f * 0.73f), new Vector3(0, 90, 0))}},
    };
}