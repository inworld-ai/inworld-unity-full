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
    public class MovableObject : Controller2D
    {
        [SerializeField] float m_MoveDuration = 0.3f;
        [SerializeField] bool m_AutoBrake = false;

        float m_CurrentCountDown;
        float m_InitDuration;
        int m_CurrWeight;
        readonly Dictionary<Vector3Int, int> m_RailwayWeights = new Dictionary<Vector3Int, int>();
        bool _IsOnRailway => MapGrid.Railway.GetTile(CurrPos);
        protected void Start()
        {
            m_RailwayWeights.Clear();
            m_CurrWeight = 0;
            m_CurrentCountDown = 0;
            m_InitDuration = m_MoveDuration;
        }
        protected void Update()
        {
            if (!_IsOnRailway)
            {
                m_MoveDuration = m_InitDuration;
                return;
            }
            MoveRailWay();
        }
        Vector3Int _FindNextRailway()
        {
            int nMinWeight = int.MaxValue;
            Vector3Int resDirection = Vector3Int.zero;
            foreach (Vector3Int direction in MapGrid.Directions)
            {
                int currWeight = _GetRailwayBlockWeight(CurrPos + direction);
                if (currWeight >= nMinWeight)
                    continue;
                nMinWeight = currWeight;
                resDirection = direction;
            }
            return resDirection;
        }
        int _GetRailwayBlockWeight(Vector3Int pos)
        {
            if (!MapGrid.Railway.GetTile(pos))
                return int.MaxValue;
            if (!m_RailwayWeights.ContainsKey(pos))
                m_RailwayWeights[pos] = 0;
            return m_RailwayWeights[pos];
        }
        protected void MoveRailWay()
        {
            m_CurrentCountDown += Time.deltaTime;
            if (!(m_CurrentCountDown > m_MoveDuration))
                return;
            m_MoveDuration = m_AutoBrake ? m_MoveDuration * 2 : m_MoveDuration;
            m_CurrentCountDown = 0;
            Move(_FindNextRailway());
        }
        public override bool Move(Vector3Int newPos)
        {
            if (!base.Move(newPos))
                return false;
            if (!MapGrid.Railway.GetTile(CurrPos))
                return true;
            m_RailwayWeights[CurrPos] = ++m_CurrWeight;
            // Get Character off Railway if blocked
            if (MapGrid.Character.CurrPos == CurrPos)
            {
                MapGrid.Character.Move(Vector3Int.left);
            }
            return true;
        }
        protected override bool CanMove(Vector3Int newPos)
        {
            return MapGrid.Stone.CurrPos != newPos;
        }
    }
}
