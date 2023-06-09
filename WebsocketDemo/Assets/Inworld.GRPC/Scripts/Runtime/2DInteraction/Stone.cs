/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Inworld.Sample.Interaction2D
{
    public class Stone : MovableObject
    {
        [SerializeField] TileBase m_Prince;

        protected override bool CanMove(Vector3Int newPos)
        {
            return MapGrid.Cart.CurrPos != newPos;
        }
        public override bool Move(Vector3Int newPos)
        {
            if (!base.Move(newPos))
                return false;
            if (!MapGrid.Items.GetTile(CurrPos))
                return true;
            m_Character = m_Prince;
            MapGrid.Items.SetTile(CurrPos, null);
            m_Tilemap.SetTile(CurrPos, m_Prince);
            return true;
        }
    }
}
