using Assets.Scripts.GamePlayLogic;
using ClientUtilities.Singleton;
using Simulation.Data.Game;
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


        private void Start()
        {
            FillPointVisualizer();
            SimulationManager.Instance.OnActionsUndo += OnActionsUndo;
        }

      

        public PointVisualizer FindPoint(PointData PointData)
        {
            for(int i = 0; i<Points.Length;++i)
            {
                if (Points[i].PointData.ID != PointData.ID)
                    continue;

                return Points[i];
            }

            return null;
        }

        public void ShowPossibleMoves(PointData [] PossibleMoves)
        {
          for(int i= 0; i<PossibleMoves.Length;++i)
            {
                for(int j = 0; j<Points.Length;++j)
                {
                    if (PossibleMoves[i].ID != Points[j].PointData.ID)
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
            UpdatePointVisualizer();

        }


        private void UpdatePointVisualizer()
        {
            for (int i = 0; i < SimulationManager.Instance.Shot.BoardData.Points.Length; ++i)
            {
                Points[i].PointData = SimulationManager.Instance.Shot.BoardData.Points[i];
                Points[i].Index = i;
            }
            OnUpdatePointsData?.Invoke();
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

            UpdatePointVisualizer();
            
        }
    }
}