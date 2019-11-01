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
    public delegate void InterpolateFinished();
    public delegate void UpdatePointsData();
    public class PointVisualizerManager : MonoBehaviorSingleton<PointVisualizerManager>
    {
        private SimulationManager simInstance;

        public event UpdatePointsData OnUpdatePointsData;
        public event InterpolateFinished OnInterPolateFinished;

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

        private void Awake()
        {
            simInstance = SimulationManager.Instance;
        }

  

        private void OnEnable()
        {
            if (simInstance != null)
            {
                simInstance.OnActionsUndo += OnActionsUndo;
                simInstance.OnTableReady += Instance_OnTableReady;
            }

        }

        private void OnDisable()
        {
            if (simInstance != null)
            {
                simInstance.OnActionsUndo -= OnActionsUndo;
                simInstance.OnTableReady -= Instance_OnTableReady;
            }

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

        public void ShowPossibleMovesOut(MoveInfo[] PossibleMoves)
        {
            for (int i = 0; i < PossibleMoves.Length; ++i)
            {
                for (int j = 0; j < Points.Length; ++j)
                {
                    if (PossibleMoves[i].From.ID != Points[j].PointData.ID)
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

        public void BoardToBoardMove(Identifier From, Identifier To)
        {
            int fromIndex = FindPointIndex(From);
            int toIndex = FindPointIndex(To);
            PointVisualizer pif = Points[fromIndex];
            PointVisualizer toi = Points[toIndex];
            pif.PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[fromIndex];
            toi.PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[toIndex];
            Beed bd = null;

            toi.pointBeeds.Push(bd = pif.pointBeeds.Pop());
            bd.transform.SetParent(null);

            bd.Trail.enabled = true;
            LeanTween.move(bd.gameObject, toi.FindPosition(toi.pointBeeds.Count - 1), 0.5F).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                bd.Trail.enabled = false;

                bd.transform.SetParent(toi.transform);
                pif.Rearrange();
                toi.Rearrange();
            });
        }

        public void BeardOff(Identifier From)
        {
            BarOff extraBar = null;
            for (int i = 0; i < ExtraBar.Length / 2; ++i)
            {
                if (ExtraBar[i].Color == SimulationManager.Instance.CurrentSimulator.Frame.Board.TurnColor)
                {
                    extraBar = ExtraBar[i];
                    if (extraBar.Color == PlayerColors.White)
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
                    else
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;

                }
            }

            int fromIndex = FindPointIndex(From);

            PointVisualizer pif = Points[fromIndex];

            pif.PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[fromIndex];

            Beed bd = null;
            extraBar.pointBeeds.Push(bd = pif.pointBeeds.Pop());
            bd.transform.SetParent(null);
            //  pif.Rearrange();
            bd.Trail.enabled = true;
            LeanTween.move(bd.gameObject, extraBar.FindPosition(extraBar.pointBeeds.Count - 1), 0.8F).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                bd.Trail.enabled = false;
                bd.transform.SetParent(extraBar.transform);
                extraBar.Rearrange();
            });


        }

        public void BoardToBarMove(Identifier From)
        {
            BarOff extraBar = null;
            for (int i = ExtraBar.Length / 2; i < ExtraBar.Length; ++i)
            {
                if (ExtraBar[i].Color != SimulationManager.Instance.CurrentSimulator.Frame.Board.TurnColor)
                {
                    extraBar = ExtraBar[i];
                    if (extraBar.Color == PlayerColors.White)
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
                    else
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;

                }
            }

            int fromIndex = FindPointIndex(From);

            PointVisualizer pif = Points[fromIndex];

            pif.PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[fromIndex];

            Beed bd = null;

            extraBar.pointBeeds.Push(bd = pif.pointBeeds.Pop());
            bd.Trail.enabled = true;
            //  pif.Rearrange();
            bd.transform.SetParent(null);
            LeanTween.move(bd.gameObject, extraBar.FindPosition(extraBar.pointBeeds.Count - 1), 0.8F).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                bd.Trail.enabled = false;
                bd.transform.SetParent(extraBar.transform);
                extraBar.Rearrange();
            });

        }

        public void BarToBoardMove(Identifier To)
        {
            BarOff extraBar = null;
            for (int i = ExtraBar.Length / 2; i < ExtraBar.Length; ++i)
            {
                if (ExtraBar[i].Color == SimulationManager.Instance.CurrentSimulator.Frame.Board.TurnColor)
                {
                    extraBar = ExtraBar[i];
                    if (extraBar.Color == PlayerColors.White)
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BarCheckerCount;
                    else
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BarCheckerCount;

                }
            }


            int toIndex = FindPointIndex(To);
            PointVisualizer toi = Points[toIndex];

            toi.PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[toIndex];
            Beed bd = null;
            toi.pointBeeds.Push(bd = extraBar.pointBeeds.Pop());
            //toi.Rearrange();
            bd.Trail.enabled = true;
            bd.transform.SetParent(null);
            LeanTween.move(bd.gameObject, toi.FindPosition(toi.pointBeeds.Count - 1), 0.8F).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                bd.Trail.enabled = false;
                bd.transform.SetParent(toi.transform);
                toi.Rearrange();
            });

        }

        public void UpdateAllPointVisualizer()
        {
         
            for (int i = 0; i < SimulationManager.Instance.CurrentSimulator.Frame.Board.Points.Length; ++i)
            {
                Points[i].SendToPool();
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
                ExtraBar[i].SendToPool();
                if (ExtraBar[i].Color == PlayerColors.White)
                    ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BearedOffCheckersCount;
                else
                    ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BearedOffCheckersCount;

            }

            for (int i = ExtraBar.Length / 2; i < ExtraBar.Length; ++i)
            {
                ExtraBar[i].SendToPool();
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
            for (int i = 0; i < Points.Length; ++i)
            {
                if (Points[i].PointData.ID == ID)
                    return Points[i].Index;
            }

            return -1;
        }

        private void Instance_OnTableReady()
        {
            FilBars();
            FillPointVisualizer();
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