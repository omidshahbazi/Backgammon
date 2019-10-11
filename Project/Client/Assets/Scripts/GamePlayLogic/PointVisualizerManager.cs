using Assets.Scripts.GamePlayLogic;
using ClientUtilities.Singleton;
using Simulation.Common;
using Simulation.Data.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void UpdatePointsData();
    public class PointVisualizerManager : MonoBehaviorSingleton<PointVisualizerManager>
    {
        public event UpdatePointsData OnUpdatePointsData;

        public PointVisualizer[] Points
        {
            get;
            private set;
        }

        public BarOff[] ExtraBar
        {
            get;
            private set;
        }

        private void Start()
        {
            FilBars();
            FillPointVisualizer();
            SimulationManager.Instance.OnActionsUndo += OnActionsUndo;
        }

        public void FilBars()
        {
            if (ExtraBar == null || ExtraBar.Length == 0)
            {
                ExtraBar = FindObjectsOfType<BarOff>();
                for (int i = 0; i < ExtraBar.Length - 1; i++)


                    for (int j = i + 1; j < ExtraBar.Length; j++)


                        if (ExtraBar[i].ID > ExtraBar[j].ID)
                        {

                            BarOff temp = ExtraBar[i];
                            ExtraBar[i] = ExtraBar[j];
                            ExtraBar[j] = temp;
                        }


            }

            UpdateExtraBars();
        }

        public PointVisualizer FindPoint(PointData PointData)
        {
            for (int i = 0; i < Points.Length; ++i)
            {
                if (Points[i].PointData.ID != PointData.ID)
                    continue;

                return Points[i];
            }

            return null;
        }

        public PointVisualizer FindPoint(Identifier ID)
        {
            for (int i = 0; i < Points.Length; ++i)
            {
                if (Points[i].PointData.ID != ID)
                    continue;

                return Points[i];
            }

            return null;
        }

        public void ShowPossibleMoves(MoveInfo[] PossibleMoves)
        {
            for (int i = 0; i < PossibleMoves.Length; ++i)
            {
                for (int j = 0; j < Points.Length; ++j)
                {
                    if (PossibleMoves[i].To.ID != Points[j].PointData.ID)
                        continue;

                    Points[j].HighlightHeleper.gameObject.SetActive(true);
                }
            }
        }

        public void HidePossibleMoves()
        {
            for (int j = 0; j < Points.Length; ++j)
                Points[j].HighlightHeleper.gameObject.SetActive(false);

        }

        private void OnActionsUndo()
        {
            UpdateAllPointVisualizer();
          
        }


        public void UpdateAllPointVisualizer()
        {
            for (int i = 0; i < SimulationManager.Instance.CurrentSimulator.Frame.Board.Points.Length; ++i)
            {
                Points[i].PointData = null;
                Points[i].PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[i];
                Points[i].Index = i;
            }
            UpdateExtraBars();

            OnUpdatePointsData?.Invoke();
        }

        public void UpdateExtraBars()
        {
            if (ExtraBar == null || ExtraBar.Length == 0)
                return;

            for (int i = 0; i < ExtraBar.Length / 2; ++i)
            {
                if (ExtraBar[i].Color == PlayerColors.White)
                    ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BearedOffCheckersCount;
                else
                    ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BearedOffCheckersCount;

            }

            for (int i = ExtraBar.Length / 2; i < ExtraBar.Length; ++i)
            {
                if (ExtraBar[i].Color == PlayerColors.White)
                    ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
                else
                    ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;

            }
        }

        public void UpdatePoints()
        {
            //To Do update  a specfic point
        }

        public int FindPointIndex(Identifier ID)
        {
            for(int i = 0; i<Points.Length;++i)
            {
                if (Points[i].PointData.ID == ID)
                    return Points[i].Index;
            }

            return -1;
        }

        private void FillPointVisualizer()
        {

            if (Points == null || Points.Length == 0)
                Points = FindObjectsOfType<PointVisualizer>();

            Debug.Assert(Points != null && Points.Length != 0, "Points are empty");

            for (int i = 0; i < Points.Length; ++i)
            {

                string name = Points[i].gameObject.name.Replace("PointVisualizer_", "");
                int index = -1;
                int.TryParse(name, out index);
                if (index == -1 || index == i)
                    continue;

                PointVisualizer pointHolder = Points[i];
                Points[i] = Points[index];
                Points[index] = pointHolder;
                --i;
            }

            UpdateAllPointVisualizer();

        }
    }
}