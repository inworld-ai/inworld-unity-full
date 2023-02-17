/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Inworld.Sample.Interaction2D
{
    public class MapGrid : SingletonBehavior<MapGrid>
    {
        [SerializeField] Vector3Int m_AnchorPosition;
        [SerializeField] Vector3Int m_SizeDelta;
        [Header("Maps:")]
        [SerializeField] Tilemap m_Blocked;
        [SerializeField] Tilemap m_Railway;
        [SerializeField] Tilemap m_Item;
        [Header("Characters:")]
        [SerializeField] Stone m_Stone;
        [SerializeField] MovableObject m_Cart;
        [SerializeField] CharacterController2D m_Character;
        BoundsInt m_Boundary;
        List<Vector3Int> m_DirectionList = new List<Vector3Int>()
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };
        public static Vector3Int StartPosition => Instance.m_AnchorPosition;

        public static Vector3Int EndPosition => Instance.m_AnchorPosition + Instance.m_SizeDelta;
        public static Tilemap Blocked => Instance.m_Blocked;
        public static Stone Stone => Instance.m_Stone;
        public static MovableObject Cart => Instance.m_Cart;
        public static CharacterController2D Character => Instance.m_Character;
        public static List<Vector3Int> Directions => Instance.m_DirectionList;
        public static Tilemap Items => Instance.m_Item;
        public static Tilemap Railway => Instance.m_Railway;
        public static BoundsInt Boundary => new BoundsInt(Instance.m_AnchorPosition, Instance.m_SizeDelta);
    }
}
