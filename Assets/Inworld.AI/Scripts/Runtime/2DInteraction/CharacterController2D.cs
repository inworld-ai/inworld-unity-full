/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
namespace Inworld.Sample.Interaction2D
{
    public class CharacterController2D : Controller2D
    {
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            {
                Move(Vector3Int.left);
            }
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                Move(Vector3Int.up);
            }
            if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                Move(Vector3Int.down);
            }
            if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                Move(Vector3Int.right);
            }
        }
        protected override bool CanMove(Vector3Int newPos)
        {
            if (!base.CanMove(newPos))
                return false;
            if (MapGrid.Stone.CurrPos == newPos)
                return MapGrid.Stone.Move(newPos - CurrPos);
            if (MapGrid.Cart.CurrPos == newPos)
                return MapGrid.Cart.Move(newPos - CurrPos);
            return true;
        }
    }
}
