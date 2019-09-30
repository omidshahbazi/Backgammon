using Assets.Scripts.GamePlayLogic;
using ClientUtilities.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GamePlayLogic
{
    public delegate void PointsIntialDataSet();
    public class PointVisualizerManager : MonoBehaviorSingleton<PointVisualizerManager>
    {
        public event PointsIntialDataSet OnInitialDataSet;

        public PointVisualizer[] Points
        {
            get;
            private set;
        }

        private void Start()
        {
            FillPointVisualizer();
        }

        private void ResetBoard()
        {

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

            for(int i = 0; i <SimulationManager.Instance.Simulator.Board.Points.Length;++i)
            {
                Points[i].PointData = SimulationManager.Instance.Simulator.Board.Points[i];
            }

            OnInitialDataSet?.Invoke();
        }
    }
}