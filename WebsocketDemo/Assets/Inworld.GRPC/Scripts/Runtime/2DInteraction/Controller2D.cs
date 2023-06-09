/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
namespace Inworld.Sample.Interaction2D
{
    public class Controller2D : MonoBehaviour
    {
        [SerializeField] protected TileBase m_Character;
        protected Tilemap m_Tilemap;
        public Vector3Int LastPos { get; set; }
        public Vector3Int CurrPos { get; set; }
        protected void Awake()
        {
            m_Tilemap = GetComponent<Tilemap>();
            for (int i = MapGrid.StartPosition.x; i < MapGrid.EndPosition.x; i++)
            {
                for (int j = MapGrid.StartPosition.y; j < MapGrid.EndPosition.y; j++)
                {
                    if (m_Tilemap.GetTile(new Vector3Int(i, j, 0)) != m_Character)
                        continue;
                    Debug.Log($"{gameObject.name} is at {i} {j}");
                    LastPos = CurrPos = new Vector3Int(i, j, 0);
                    break;
                }
            }
        }
        public virtual bool Move(Vector3Int newPos)
        {
            if (!CanMove(CurrPos + newPos))
                return false;
            LastPos = CurrPos;
            CurrPos += newPos;
            m_Tilemap.SetTile(LastPos, null);
            m_Tilemap.SetTile(CurrPos, m_Character);
            return true;
        }
        protected virtual bool CanMove(Vector3Int newPos)
        {
            return MapGrid.Boundary.Contains(newPos) && !MapGrid.Blocked.GetTile(newPos);
        }
    }
}
