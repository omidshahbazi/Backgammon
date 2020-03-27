using Assets.Scripts.GamePlayLogic;
using ClientUtilities.AudioMangaer;
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
        private const float INTERPOLATION_TIME = 0.7F;
        private SimulationManager simInstance;
        private Audio click;

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


        public bool IsInterPolate
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
                simInstance.OnGameDataReady += SimInstance_OnGameDataReady;
                simInstance.OnGameFinished += SimInstance_OnGameFinished;
            }

        }



        private void OnDisable()
        {
            if (simInstance != null)
            {
                simInstance.OnActionsUndo -= OnActionsUndo;
                simInstance.OnTableReady -= Instance_OnTableReady;
                simInstance.OnGameDataReady -= SimInstance_OnGameDataReady;
                simInstance.OnGameFinished -= SimInstance_OnGameFinished;
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

        private void OnActionsUndo()
        {
            UpdateAllPointVisualizer();
            IsInterPolate = false;
        }

        private void SimInstance_OnGameFinished(PlayerColors WinnerColor, Networking.Common.GameFinishReasons Reason, int Score)
        {
            BoardPointsSendToPool();
            ExtraBarSendToPool();
        }

        public void ActiveBeardedOffHighlight()
        {
            for (int i = 0; i < ExtraBar.Length / 2; ++i)
            {
                BarOff extraBar = ExtraBar[i];
                if (extraBar.Color == simInstance.YourColor)
                    extraBar.SetHighlightHelper = true;
            }
        }

        public void DeactiveBeardedOffHighlight()
        {
            for (int i = 0; i < ExtraBar.Length / 2; ++i)
            {
                BarOff extraBar = ExtraBar[i];
                if (extraBar.Color == simInstance.YourColor)
                    extraBar.SetHighlightHelper = false;
            }
        }

        public void BoardToBoardMove(Identifier From, Identifier To)
        {

            int fromIndex = FindPointIndex(From);
            int toIndex = FindPointIndex(To);
            PointVisualizer pif = Points[fromIndex];
            PointVisualizer toi = Points[toIndex];

            pif.PointData = SimulationManager.Instance.GetPointData(From); /*SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[fromIndex];*/
            toi.PointData = SimulationManager.Instance.GetPointData(To); /*SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[toIndex];*/

            if (pif.pointBeeds == null || pif.pointBeeds.Count == 0)
            {
                Debug.LogWarning("pif.pointBeeds is null or zero ");
                return;
            }



            Beed bd = pif.pointBeeds[pif.pointBeeds.Count - 1];
            pif.pointBeeds.Remove(bd);
            toi.pointBeeds.Add(bd);
            if (toi.pointBeeds == null || toi.pointBeeds.Count == 0)
            {
                Debug.LogWarning("toi.pointBeeds is null or zero ");
                return;
            }

            bd.transform.SetParent(null);

            bd.Trail.enabled = true;
            IsInterPolate = true;
            LeanTween.move(bd.gameObject, toi.FindPosition(toi.PointData.CheckerCount - 1), INTERPOLATION_TIME).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                bd.Trail.enabled = false;

                bd.transform.SetParent(toi.transform);

                pif.Rearrange();
                toi.Rearrange();

                PlayAudioEffect();
                IsInterPolate = false;

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
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BearedOffCheckersCount;
                    else
                        extraBar.BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BearedOffCheckersCount;

                }
            }

            int fromIndex = FindPointIndex(From);


            if (fromIndex == -1)
            {
                Debug.LogWarning("From Index is Equal -1");
                return;
            }

            PointVisualizer pif = Points[fromIndex];

            pif.PointData = SimulationManager.Instance.GetPointData(From);

            if (pif.pointBeeds == null || pif.pointBeeds.Count == 0)
            {
                Debug.LogWarning("pif.pointBeeds is null or zero ");
                return;
            }

            Beed bd = pif.pointBeeds[pif.pointBeeds.Count - 1];
            bd.SetHighRenderOrder();

            pif.pointBeeds.Remove(bd);
            extraBar.pointBeeds.Add(bd);


            if (extraBar.pointBeeds == null || extraBar.pointBeeds.Count == 0)
            {
                Debug.LogWarning("extraBar.pointBeeds is null or zero ");
                return;
            }

            bd.transform.SetParent(null);

            bd.Trail.enabled = true;
            if (LeanTween.isTweening(bd.gameObject))
                LeanTween.cancel(bd.gameObject, true);


            IsInterPolate = true;
            LeanTween.move(bd.gameObject, extraBar.FindPosition(extraBar.pointBeeds.Count - 1), INTERPOLATION_TIME).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                PlayAudioEffect();

                bd.Trail.enabled = false;
                bd.transform.SetParent(extraBar.transform);
                pif.Rearrange();
                extraBar.Rearrange();
                IsInterPolate = false;
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

            if (fromIndex == -1)
            {
                Debug.LogWarning("From Index is Equal -1");
                return;
            }

            PointVisualizer pif = Points[fromIndex];

            pif.PointData = SimulationManager.Instance.GetPointData(From);

            if (pif.pointBeeds == null || pif.pointBeeds.Count == 0)
            {
                Debug.LogWarning("pif.pointBeeds is null or zero ");
                return;
            }


            Beed bd = pif.pointBeeds[pif.pointBeeds.Count - 1];
            pif.pointBeeds.Remove(bd);
            extraBar.pointBeeds.Add(bd);
            bd.Trail.enabled = true;

            bd.transform.SetParent(null);
            if (LeanTween.isTweening(bd.gameObject))
                LeanTween.cancel(bd.gameObject, true);
            IsInterPolate = true;
            LeanTween.move(bd.gameObject, extraBar.FindPosition(extraBar.pointBeeds.Count - 1), INTERPOLATION_TIME).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                PlayAudioEffect();
                bd.Trail.enabled = false;
                bd.transform.SetParent(extraBar.transform);
                extraBar.Rearrange();
                pif.Rearrange();
                IsInterPolate = false;
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
            if (toIndex == -1)
            {
                Debug.LogWarning("To index is equal -1");
                return;
            }
            PointVisualizer toi = Points[toIndex];



            toi.PointData = SimulationManager.Instance.GetPointData(To);
            if (extraBar.pointBeeds == null || extraBar.pointBeeds.Count == 0)
            {
                Debug.LogWarning("extraBar.pointBeeds is null or zero ");
                return;
            }


            Beed bd = extraBar.pointBeeds[extraBar.pointBeeds.Count - 1];
            extraBar.pointBeeds.Remove(bd);
            toi.pointBeeds.Add(bd);

            if (toi.pointBeeds == null || toi.pointBeeds.Count == 0)
            {
                Debug.LogWarning("toi.pointBeeds is null or zero ");
                return;
            }

            //toi.Rearrange();
            bd.Trail.enabled = true;
            bd.transform.SetParent(null);
            if (LeanTween.isTweening(bd.gameObject))
                LeanTween.cancel(bd.gameObject, true);
            IsInterPolate = true;
            LeanTween.move(bd.gameObject, toi.FindPosition(toi.pointBeeds.Count - 1), INTERPOLATION_TIME).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
            {
                PlayAudioEffect();
                bd.Trail.enabled = false;
                bd.transform.SetParent(toi.transform);
                toi.Rearrange();
                IsInterPolate = false;
            });

        }

        public void CancelAllMoves()
        {
            for (int i = 0; i < Points.Length; ++i)
            {
                for (int j = 0; j < Points[i].pointBeeds.Count; ++j)
                    LeanTween.cancel(Points[i].pointBeeds[j].gameObject, false);
            }

            for (int i = 0; i < ExtraBar.Length; ++i)
            {
                for (int j = 0; j < ExtraBar[i].pointBeeds.Count; ++j)
                    LeanTween.cancel(Points[i].pointBeeds[j].gameObject, false);
            }
        }


        private void PlayAudioEffect()
        {
            if (click == null)
            {
                click = AudioManager.Instance.Load("BeadEffect", AudioManager.SoundTypes.Effect);
                click.Volume = 100;
                //click.Stop();
                click.AutoUnload = false;
            }
            click.Play();
        }

        public void UpdateAllPointVisualizer()
        {
            BoardPointsSendToPool();

            if (simInstance.YourColor == PlayerColors.White)
            {
                for (int i = 0; i < SimulationManager.Instance.CurrentSimulator.Frame.Board.Points.Length; ++i)
                {
                    Points[i].SendToPool();
                    Points[i].PointData = null;
                    Points[i].PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[i];
                    Points[i].Index = i;

                }
            }
            else
            {


                for (int i = 0, j = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points.Length - 1; i <= SimulationManager.Instance.CurrentSimulator.Frame.Board.Points.Length - 1; ++i, --j)
                {
                    Points[i].SendToPool();
                    Points[i].PointData = null;
                    Points[i].PointData = SimulationManager.Instance.CurrentSimulator.Frame.Board.Points[j];
                    Points[i].Index = i;
                }
            }
            UpdateExtraBars();

            OnUpdatePointsData?.Invoke();
        }

        private void BoardPointsSendToPool()
        {
            for (int i = 0; i < Points.Length; ++i)
            {
                Points[i].SendToPool();
                Points[i].PointData = null;
            }
        }

        private void ExtraBarSendToPool()
        {
            for (int i = 0; i < ExtraBar.Length; ++i)
            {
                ExtraBar[i].SendToPool();
                ExtraBar[i].BarCheckerCount = 0;
            }
        }

        public void UpdateExtraBars()
        {

            ExtraBarSendToPool();
            for (int i = 0; i < ExtraBar.Length / 2; ++i)
            {
                ExtraBar[i].SendToPool();


                if (ExtraBar[i].ID == 2)
                {
                    ExtraBar[i].Color = simInstance.YourColor;
                    if (ExtraBar[i].Color == PlayerColors.White)
                        ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BearedOffCheckersCount;
                    else
                        ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BearedOffCheckersCount;
                }
                else if (ExtraBar[i].ID == 1)
                {
                    if (ExtraBar[i].Color == simInstance.YourColor)
                    {
                        if (ExtraBar[i].Color == PlayerColors.White)
                        {
                            ExtraBar[i].Color = PlayerColors.Black;
                            ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.BlackPlayer.BearedOffCheckersCount;
                        }
                        else
                        {
                            ExtraBar[i].Color = PlayerColors.White;
                            ExtraBar[i].BarCheckerCount = SimulationManager.Instance.CurrentSimulator.Frame.Board.WhitePlayer.BearedOffCheckersCount;
                        }
                    }
                }

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

        private void SimInstance_OnGameDataReady(PlayerColors Color)
        {
            Instance_OnTableReady();
        }

        private void Instance_OnTableReady()
        {
            FilBars();
            FillPointVisualizer();
            IsInterPolate = false;
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}